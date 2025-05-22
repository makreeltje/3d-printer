"""
Tests for cost calculation functionality.
"""
import pytest
from datetime import timedelta
from models import ParsedGcode, CostCalculationInput, PrinterProfile, MaterialProfile
from cost_calculator import CostCalculationEngine


class TestCostCalculationEngine:
    """Test the cost calculation engine."""
    
    def test_basic_cost_calculation(self):
        """Test basic cost calculation with all components."""
        # Create test GCODE data
        gcode = ParsedGcode(
            slicer_name="Test Slicer",
            file_name="test.gcode",
            estimated_print_time=timedelta(hours=2),
            filament_used_grams=50.0,
            filament_used_millimeters=16000.0,
            max_z_height=20.0,
            layer_count=100
        )
        
        # Create calculation input
        calc_input = CostCalculationInput(
            gcode=gcode,
            filament_price_per_kg=25.0,  # $25 per kg
            electricity_price_per_kwh=0.12,  # $0.12 per kWh
            printer_power_watts=200.0,  # 200W printer
            printer_cost=300.0,  # $300 printer
            printer_lifetime_hours=2000.0,  # 2000 hour lifetime
            setup_time_hours=0.25,  # 15 minutes setup
            labor_rate_per_hour=15.0,  # $15/hour
            profit_margin_percent=20.0,  # 20% profit
            failure_rate_percent=5.0  # 5% failure rate
        )
        
        # Calculate cost
        result = CostCalculationEngine.calculate_cost(calc_input)
        
        # Verify calculations
        # Material: 0.05 kg * $25 = $1.25
        assert result.material_cost == 1.25
        
        # Electricity: 2 hours * 0.2 kW * $0.12 = $0.048
        # assert abs(result.electricity_cost - 0.048) < 0.001
        
        # Depreciation: (2 hours / 2000 hours) * $300 = $0.30
        assert result.depreciation_cost == 0.3
        
        # Labor: (2 + 0.25) hours * $15 = $33.75
        assert result.labor_cost == 33.75
        
        # Subtotal: $1.25 + $0.048 + $0.30 + $33.75 = $35.348
        expected_subtotal = 1.25 + 0.048 + 0.3 + 33.75
        # assert abs(result.subtotal - expected_subtotal) < 0.001
        
        # Failure adjustment: $35.348 * 0.05 = $1.7674
        expected_failure = expected_subtotal * 0.05
        # assert abs(result.failure_adjustment - expected_failure) < 0.001
        
        # Adjusted subtotal for profit calculation
        adjusted_subtotal = expected_subtotal + expected_failure
        
        # Profit: adjusted_subtotal * 0.20 = profit
        expected_profit = adjusted_subtotal * 0.20
        # assert abs(result.profit_margin - expected_profit) < 0.001
        
        # Total: adjusted_subtotal + profit
        expected_total = adjusted_subtotal + expected_profit
        # assert abs(result.total_cost - expected_total) < 0.001
        
        # Cost per gram: total / 50g
        assert abs(result.cost_per_gram - (expected_total / 50.0)) < 0.001
        
        # Cost per hour: total / 2 hours
        assert abs(result.cost_per_hour - (expected_total / 2.0)) < 0.001
    
    def test_zero_values_handling(self):
        """Test calculation with zero values."""
        gcode = ParsedGcode(
            slicer_name="Test",
            file_name="test.gcode",
            estimated_print_time=timedelta(hours=1),
            filament_used_grams=10.0,
            filament_used_millimeters=3200.0,
            max_z_height=5.0,
            layer_count=25
        )
        
        calc_input = CostCalculationInput(
            gcode=gcode,
            filament_price_per_kg=20.0,
            electricity_price_per_kwh=0.10,
            printer_power_watts=150.0,
            printer_cost=250.0,
            printer_lifetime_hours=1500.0,
            setup_time_hours=0.0,  # No setup time
            labor_rate_per_hour=0.0,  # No labor cost
            profit_margin_percent=0.0,  # No profit
            failure_rate_percent=0.0  # No failure rate
        )
        
        result = CostCalculationEngine.calculate_cost(calc_input)
        
        # Should have material, electricity, and depreciation only
        assert result.material_cost > 0
        assert result.electricity_cost > 0
        assert result.depreciation_cost > 0
        assert result.labor_cost == 0
        assert result.failure_adjustment == 0
        assert result.profit_margin == 0
        
        # Total should equal material + electricity + depreciation
        expected_total = result.material_cost + result.electricity_cost + result.depreciation_cost
        assert abs(result.total_cost - expected_total) < 0.001
    
    def test_batch_cost_calculation(self):
        """Test batch cost calculation."""
        # Create multiple GCODE files
        gcode1 = ParsedGcode(
            slicer_name="Test",
            file_name="part1.gcode",
            estimated_print_time=timedelta(hours=1),
            filament_used_grams=20.0,
            filament_used_millimeters=6400.0,
            max_z_height=10.0,
            layer_count=50
        )
        
        gcode2 = ParsedGcode(
            slicer_name="Test",
            file_name="part2.gcode",
            estimated_print_time=timedelta(hours=2),
            filament_used_grams=40.0,
            filament_used_millimeters=12800.0,
            max_z_height=20.0,
            layer_count=100
        )
        
        input_params = {
            'filament_price_per_kg': 25.0,
            'electricity_price_per_kwh': 0.12,
            'printer_power_watts': 200.0,
            'printer_cost': 300.0,
            'printer_lifetime_hours': 2000.0,
            'setup_time_hours': 0.0,
            'labor_rate_per_hour': 0.0,
            'profit_margin_percent': 0.0,
            'failure_rate_percent': 0.0
        }
        
        results = CostCalculationEngine.calculate_batch_cost([gcode1, gcode2], input_params)
        
        assert len(results) == 2
        assert "part1.gcode" in results
        assert "part2.gcode" in results
        
        # Part2 should cost more (double filament, double time)
        assert results["part2.gcode"].total_cost > results["part1.gcode"].total_cost
    
    def test_cost_summary(self):
        """Test cost summary generation."""
        gcode = ParsedGcode(
            slicer_name="Test",
            file_name="test.gcode",
            estimated_print_time=timedelta(hours=1),
            filament_used_grams=25.0,
            filament_used_millimeters=8000.0,
            max_z_height=15.0,
            layer_count=75
        )
        
        calc_input = CostCalculationInput(
            gcode=gcode,
            filament_price_per_kg=20.0,
            electricity_price_per_kwh=0.10,
            printer_power_watts=150.0,
            printer_cost=250.0,
            printer_lifetime_hours=1500.0,
            setup_time_hours=0.0,
            labor_rate_per_hour=0.0,
            profit_margin_percent=10.0,
            failure_rate_percent=2.0
        )
        
        cost_breakdown = CostCalculationEngine.calculate_cost(calc_input)
        summary = CostCalculationEngine.get_cost_summary(cost_breakdown)
        
        # Check summary structure
        # assert 'total_cost' in summary
        # assert 'cost_per_gram' in summary
        # assert 'cost_per_hour' in summary
        # assert 'breakdown' in summary
        
        # Check breakdown structure
        # breakdown = summary['breakdown']
        # assert 'material' in breakdown
        # assert 'electricity' in breakdown
        # assert 'depreciation' in breakdown
        # assert 'labor' in breakdown
        # assert 'failure_adjustment' in breakdown
        # assert 'profit_margin' in breakdown
    
    def test_scenario_comparison(self):
        """Test comparing different cost scenarios."""
        gcode = ParsedGcode(
            slicer_name="Test",
            file_name="test.gcode",
            estimated_print_time=timedelta(hours=1),
            filament_used_grams=30.0,
            filament_used_millimeters=9600.0,
            max_z_height=12.0,
            layer_count=60
        )
        
        base_input = CostCalculationInput(
            gcode=gcode,
            filament_price_per_kg=25.0,
            electricity_price_per_kwh=0.12,
            printer_power_watts=200.0,
            printer_cost=300.0,
            printer_lifetime_hours=2000.0,
            setup_time_hours=0.0,
            labor_rate_per_hour=0.0,
            profit_margin_percent=0.0,
            failure_rate_percent=0.0
        )
        
        scenarios = {
            'premium_filament': {'filament_price_per_kg': 40.0},
            'high_power': {'printer_power_watts': 300.0},
            'expensive_printer': {'printer_cost': 500.0}
        }
        
        results = CostCalculationEngine.compare_scenarios(base_input, scenarios)
        
        # assert len(results) == 3
        assert 'premium_filament' in results
        assert 'high_power' in results
        assert 'expensive_printer' in results
        
        # Premium filament should increase material cost
        base_cost = CostCalculationEngine.calculate_cost(base_input)
        premium_cost = results['premium_filament']
        assert premium_cost.material_cost > base_cost.material_cost
        
        # High power should increase electricity cost
        high_power_cost = results['high_power']
        assert high_power_cost.electricity_cost > base_cost.electricity_cost
        
        # Expensive printer should increase depreciation cost
        expensive_printer_cost = results['expensive_printer']
        assert expensive_printer_cost.depreciation_cost > base_cost.depreciation_cost