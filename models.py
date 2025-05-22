"""
Data models for the 3D printer cost calculator application.
"""
from dataclasses import dataclass, field
from typing import Dict, List, Optional
from datetime import timedelta

@dataclass
class ParsedGcode:
    """Data model for parsed GCODE file information."""
    slicer_name: str
    file_name: str
    estimated_print_time: timedelta
    filament_used_grams: float
    filament_used_millimeters: float
    max_z_height: float
    layer_count: int
    nozzle_temperature: Optional[float] = None
    bed_temperature: Optional[float] = None
    average_print_speed: Optional[float] = None
    extrusion_volume: float = 0.0
    extra_metadata: Dict[str, str] = field(default_factory=dict)

@dataclass
class PrinterProfile:
    """Data model for printer configuration."""
    name: str
    power_consumption_watts: float
    purchase_cost: float
    lifetime_hours: float
    maintenance_cost_per_hour: float = 0.0
    description: str = ""

@dataclass
class MaterialProfile:
    """Data model for material configuration."""
    name: str
    price_per_kg: float
    density_g_cm3: float
    material_type: str
    description: str = ""

@dataclass
class CostCalculationInput:
    """Input parameters for cost calculation."""
    gcode: ParsedGcode
    filament_price_per_kg: float
    electricity_price_per_kwh: float
    printer_power_watts: float
    printer_cost: float
    printer_lifetime_hours: float
    setup_time_hours: float = 0.0
    labor_rate_per_hour: float = 0.0
    profit_margin_percent: float = 0.0
    failure_rate_percent: float = 0.0

@dataclass
class CostBreakdown:
    """Detailed cost breakdown result."""
    material_cost: float
    electricity_cost: float
    depreciation_cost: float
    labor_cost: float
    failure_adjustment: float
    subtotal: float
    profit_margin: float
    total_cost: float
    
    # Additional metrics
    cost_per_gram: float
    cost_per_hour: float
    material_percentage: float
    electricity_percentage: float
    depreciation_percentage: float
    labor_percentage: float

@dataclass
class PrintProject:
    """Data model for a print project with associated metadata."""
    group_name: str
    project_name: str
    gcode_data: ParsedGcode
    stl_files: List[str] = field(default_factory=list)
    mp4_timelapses: List[str] = field(default_factory=list)
    source_url: str = ""
    cost_breakdown: Optional[CostBreakdown] = None
