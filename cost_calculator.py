"""
Cost calculation engine for 3D printing projects.
"""
from models import CostCalculationInput, CostBreakdown, ParsedGcode
from typing import Dict, Any

class CostCalculationEngine:
    """Engine for calculating comprehensive 3D printing costs."""
    
    @staticmethod
    def calculate_cost(input_data: CostCalculationInput) -> CostBreakdown:
        """
        Calculate comprehensive cost breakdown for a 3D print.
        
        Formula: Total = Material + Electricity + Depreciation + Labor + Failure_Adjustment + Profit
        """
        # Material cost calculation
        material_cost = (input_data.gcode.filament_used_grams / 1000.0) * input_data.filament_price_per_kg
        
        # Electricity cost calculation
        print_time_hours = input_data.gcode.estimated_print_time.total_seconds() / 3600.0
        electricity_cost = (input_data.printer_power_watts / 1000.0) * print_time_hours * input_data.electricity_price_per_kwh
        
        # Machine depreciation cost calculation
        depreciation_per_hour = input_data.printer_cost / input_data.printer_lifetime_hours
        depreciation_cost = depreciation_per_hour * print_time_hours
        
        # Labor cost calculation
        total_labor_time = print_time_hours + input_data.setup_time_hours
        labor_cost = total_labor_time * input_data.labor_rate_per_hour
        
        # Subtotal before failure adjustment and profit
        subtotal = material_cost + electricity_cost + depreciation_cost + labor_cost
        
        # Failure rate adjustment
        failure_adjustment = subtotal * (input_data.failure_rate_percent / 100.0)
        
        # Adjusted subtotal
        adjusted_subtotal = subtotal + failure_adjustment
        
        # Profit margin calculation
        profit_margin = adjusted_subtotal * (input_data.profit_margin_percent / 100.0)
        
        # Total cost
        total_cost = adjusted_subtotal + profit_margin
        
        # Calculate additional metrics
        cost_per_gram = total_cost / max(input_data.gcode.filament_used_grams, 0.1)  # Avoid division by zero
        cost_per_hour = total_cost / max(print_time_hours, 0.01)  # Avoid division by zero
        
        # Calculate cost percentages
        if total_cost > 0:
            material_percentage = (material_cost / total_cost) * 100
            electricity_percentage = (electricity_cost / total_cost) * 100
            depreciation_percentage = (depreciation_cost / total_cost) * 100
            labor_percentage = (labor_cost / total_cost) * 100
        else:
            material_percentage = electricity_percentage = depreciation_percentage = labor_percentage = 0
        
        return CostBreakdown(
            material_cost=round(material_cost, 2),
            electricity_cost=round(electricity_cost, 2),
            depreciation_cost=round(depreciation_cost, 2),
            labor_cost=round(labor_cost, 2),
            failure_adjustment=round(failure_adjustment, 2),
            subtotal=round(adjusted_subtotal, 2),
            profit_margin=round(profit_margin, 2),
            total_cost=round(total_cost, 2),
            cost_per_gram=round(cost_per_gram, 2),
            cost_per_hour=round(cost_per_hour, 2),
            material_percentage=round(material_percentage, 1),
            electricity_percentage=round(electricity_percentage, 1),
            depreciation_percentage=round(depreciation_percentage, 1),
            labor_percentage=round(labor_percentage, 1)
        )
    
    @staticmethod
    def calculate_batch_cost(gcode_files: list[ParsedGcode], input_params: Dict[str, Any]) -> Dict[str, CostBreakdown]:
        """Calculate costs for multiple GCODE files."""
        results = {}
        
        for gcode in gcode_files:
            # Create input for this specific file
            calc_input = CostCalculationInput(
                gcode=gcode,
                filament_price_per_kg=input_params['filament_price_per_kg'],
                electricity_price_per_kwh=input_params['electricity_price_per_kwh'],
                printer_power_watts=input_params['printer_power_watts'],
                printer_cost=input_params['printer_cost'],
                printer_lifetime_hours=input_params['printer_lifetime_hours'],
                setup_time_hours=input_params.get('setup_time_hours', 0.0),
                labor_rate_per_hour=input_params.get('labor_rate_per_hour', 0.0),
                profit_margin_percent=input_params.get('profit_margin_percent', 0.0),
                failure_rate_percent=input_params.get('failure_rate_percent', 0.0)
            )
            
            results[gcode.file_name] = CostCalculationEngine.calculate_cost(calc_input)
        
        return results
    
    @staticmethod
    def get_cost_summary(cost_breakdown: CostBreakdown) -> Dict[str, Any]:
        """Get a summary of cost breakdown for display."""
        return {
            "Total Cost": f"${cost_breakdown.total_cost:.2f}",
            "Material": f"${cost_breakdown.material_cost:.2f} ({cost_breakdown.material_percentage:.1f}%)",
            "Electricity": f"${cost_breakdown.electricity_cost:.2f} ({cost_breakdown.electricity_percentage:.1f}%)",
            "Depreciation": f"${cost_breakdown.depreciation_cost:.2f} ({cost_breakdown.depreciation_percentage:.1f}%)",
            "Labor": f"${cost_breakdown.labor_cost:.2f} ({cost_breakdown.labor_percentage:.1f}%)",
            "Failure Adjustment": f"${cost_breakdown.failure_adjustment:.2f}",
            "Profit Margin": f"${cost_breakdown.profit_margin:.2f}",
            "Cost per Gram": f"${cost_breakdown.cost_per_gram:.2f}",
            "Cost per Hour": f"${cost_breakdown.cost_per_hour:.2f}"
        }
    
    @staticmethod
    def compare_scenarios(base_input: CostCalculationInput, scenarios: Dict[str, Dict[str, Any]]) -> Dict[str, CostBreakdown]:
        """Compare different cost scenarios."""
        results = {}
        
        # Calculate base scenario
        results["Base"] = CostCalculationEngine.calculate_cost(base_input)
        
        # Calculate alternative scenarios
        for scenario_name, changes in scenarios.items():
            # Create modified input
            modified_input = CostCalculationInput(
                gcode=base_input.gcode,
                filament_price_per_kg=changes.get('filament_price_per_kg', base_input.filament_price_per_kg),
                electricity_price_per_kwh=changes.get('electricity_price_per_kwh', base_input.electricity_price_per_kwh),
                printer_power_watts=changes.get('printer_power_watts', base_input.printer_power_watts),
                printer_cost=changes.get('printer_cost', base_input.printer_cost),
                printer_lifetime_hours=changes.get('printer_lifetime_hours', base_input.printer_lifetime_hours),
                setup_time_hours=changes.get('setup_time_hours', base_input.setup_time_hours),
                labor_rate_per_hour=changes.get('labor_rate_per_hour', base_input.labor_rate_per_hour),
                profit_margin_percent=changes.get('profit_margin_percent', base_input.profit_margin_percent),
                failure_rate_percent=changes.get('failure_rate_percent', base_input.failure_rate_percent)
            )
            
            results[scenario_name] = CostCalculationEngine.calculate_cost(modified_input)
        
        return results
