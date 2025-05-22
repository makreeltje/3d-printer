"""
Utility functions for the 3D printer cost calculator.
"""
import json
import streamlit as st
from typing import Dict, List, Any, Optional
from models import PrinterProfile, MaterialProfile, ParsedGcode, CostBreakdown
import pandas as pd
from datetime import timedelta
from database import db_manager

class ProfileManager:
    """Manager for printer and material profiles with database fallback."""
    
    @staticmethod
    def save_printer_profile(profile: PrinterProfile) -> bool:
        """Save a printer profile."""
        try:
            if db_manager.db_available:
                return db_manager.save_printer_profile(profile)
            else:
                # Fallback to session state
                if 'printer_profiles' not in st.session_state:
                    st.session_state.printer_profiles = {}
                st.session_state.printer_profiles[profile.name] = profile
                return True
        except Exception:
            # Fallback to session state
            if 'printer_profiles' not in st.session_state:
                st.session_state.printer_profiles = {}
            st.session_state.printer_profiles[profile.name] = profile
            return True
    
    @staticmethod
    def load_printer_profile(name: str) -> Optional[PrinterProfile]:
        """Load a printer profile."""
        try:
            if db_manager.db_available:
                return db_manager.load_printer_profile(name)
            else:
                # Fallback to session state
                if 'printer_profiles' in st.session_state:
                    return st.session_state.printer_profiles.get(name)
                return None
        except Exception:
            # Fallback to session state
            if 'printer_profiles' in st.session_state:
                return st.session_state.printer_profiles.get(name)
            return None
    
    @staticmethod
    def get_printer_profile_names() -> List[str]:
        """Get list of saved printer profile names."""
        try:
            if db_manager.db_available:
                return db_manager.get_printer_profile_names()
            else:
                # Fallback to session state
                if 'printer_profiles' in st.session_state:
                    return list(st.session_state.printer_profiles.keys())
                return []
        except Exception:
            # Fallback to session state
            if 'printer_profiles' in st.session_state:
                return list(st.session_state.printer_profiles.keys())
            return []
    
    @staticmethod
    def delete_printer_profile(name: str) -> bool:
        """Delete a printer profile."""
        try:
            if db_manager.db_available:
                return db_manager.delete_printer_profile(name)
            else:
                # Fallback to session state
                if 'printer_profiles' in st.session_state and name in st.session_state.printer_profiles:
                    del st.session_state.printer_profiles[name]
                    return True
                return False
        except Exception:
            # Fallback to session state
            if 'printer_profiles' in st.session_state and name in st.session_state.printer_profiles:
                del st.session_state.printer_profiles[name]
                return True
            return False
    
    @staticmethod
    def save_material_profile(profile: MaterialProfile) -> bool:
        """Save a material profile."""
        try:
            if db_manager.db_available:
                return db_manager.save_material_profile(profile)
            else:
                # Fallback to session state
                if 'material_profiles' not in st.session_state:
                    st.session_state.material_profiles = {}
                st.session_state.material_profiles[profile.name] = profile
                return True
        except Exception:
            # Fallback to session state
            if 'material_profiles' not in st.session_state:
                st.session_state.material_profiles = {}
            st.session_state.material_profiles[profile.name] = profile
            return True
    
    @staticmethod
    def load_material_profile(name: str) -> Optional[MaterialProfile]:
        """Load a material profile."""
        try:
            if db_manager.db_available:
                return db_manager.load_material_profile(name)
            else:
                # Fallback to session state
                if 'material_profiles' in st.session_state:
                    return st.session_state.material_profiles.get(name)
                return None
        except Exception:
            # Fallback to session state
            if 'material_profiles' in st.session_state:
                return st.session_state.material_profiles.get(name)
            return None
    
    @staticmethod
    def get_material_profile_names() -> List[str]:
        """Get list of saved material profile names."""
        try:
            if db_manager.db_available:
                return db_manager.get_material_profile_names()
            else:
                # Fallback to session state
                if 'material_profiles' in st.session_state:
                    return list(st.session_state.material_profiles.keys())
                return []
        except Exception:
            # Fallback to session state
            if 'material_profiles' in st.session_state:
                return list(st.session_state.material_profiles.keys())
            return []
    
    @staticmethod
    def delete_material_profile(name: str) -> bool:
        """Delete a material profile."""
        try:
            if db_manager.db_available:
                return db_manager.delete_material_profile(name)
            else:
                # Fallback to session state
                if 'material_profiles' in st.session_state and name in st.session_state.material_profiles:
                    del st.session_state.material_profiles[name]
                    return True
                return False
        except Exception:
            # Fallback to session state
            if 'material_profiles' in st.session_state and name in st.session_state.material_profiles:
                del st.session_state.material_profiles[name]
                return True
            return False
    
    @staticmethod
    def export_profiles() -> Dict[str, Any]:
        """Export all profiles as a dictionary."""
        return {
            'printer_profiles': {
                name: {
                    'name': profile.name,
                    'power_consumption_watts': profile.power_consumption_watts,
                    'purchase_cost': profile.purchase_cost,
                    'lifetime_hours': profile.lifetime_hours,
                    'maintenance_cost_per_hour': profile.maintenance_cost_per_hour,
                    'description': profile.description
                }
                for name, profile in st.session_state.get('printer_profiles', {}).items()
            },
            'material_profiles': {
                name: {
                    'name': profile.name,
                    'price_per_kg': profile.price_per_kg,
                    'density_g_cm3': profile.density_g_cm3,
                    'material_type': profile.material_type,
                    'description': profile.description
                }
                for name, profile in st.session_state.get('material_profiles', {}).items()
            }
        }
    
    @staticmethod
    def import_profiles(data: Dict[str, Any]) -> bool:
        """Import profiles from a dictionary."""
        try:
            # Import printer profiles
            if 'printer_profiles' in data:
                for name, profile_data in data['printer_profiles'].items():
                    profile = PrinterProfile(**profile_data)
                    ProfileManager.save_printer_profile(profile)
            
            # Import material profiles
            if 'material_profiles' in data:
                for name, profile_data in data['material_profiles'].items():
                    profile = MaterialProfile(**profile_data)
                    ProfileManager.save_material_profile(profile)
            
            return True
        except Exception:
            return False

class DataExporter:
    """Utilities for exporting data and reports."""
    
    @staticmethod
    def export_cost_breakdown_csv(gcode_data: ParsedGcode, cost_breakdown: CostBreakdown) -> str:
        """Export cost breakdown as CSV string."""
        data = {
            'Metric': [
                'File Name',
                'Slicer',
                'Print Time (hours)',
                'Filament Used (g)',
                'Max Z Height (mm)',
                'Layer Count',
                'Nozzle Temperature (°C)',
                'Bed Temperature (°C)',
                'Material Cost ($)',
                'Electricity Cost ($)',
                'Depreciation Cost ($)',
                'Labor Cost ($)',
                'Failure Adjustment ($)',
                'Profit Margin ($)',
                'Total Cost ($)',
                'Cost per Gram ($/g)',
                'Cost per Hour ($/h)'
            ],
            'Value': [
                gcode_data.file_name,
                gcode_data.slicer_name,
                f"{gcode_data.estimated_print_time.total_seconds() / 3600:.2f}",
                f"{gcode_data.filament_used_grams:.2f}",
                f"{gcode_data.max_z_height:.2f}",
                str(gcode_data.layer_count),
                f"{gcode_data.nozzle_temperature:.1f}" if gcode_data.nozzle_temperature else "N/A",
                f"{gcode_data.bed_temperature:.1f}" if gcode_data.bed_temperature else "N/A",
                f"{cost_breakdown.material_cost:.2f}",
                f"{cost_breakdown.electricity_cost:.2f}",
                f"{cost_breakdown.depreciation_cost:.2f}",
                f"{cost_breakdown.labor_cost:.2f}",
                f"{cost_breakdown.failure_adjustment:.2f}",
                f"{cost_breakdown.profit_margin:.2f}",
                f"{cost_breakdown.total_cost:.2f}",
                f"{cost_breakdown.cost_per_gram:.2f}",
                f"{cost_breakdown.cost_per_hour:.2f}"
            ]
        }
        
        df = pd.DataFrame(data)
        return df.to_csv(index=False)
    
    @staticmethod
    def export_batch_results_csv(results: Dict[str, tuple[ParsedGcode, CostBreakdown]]) -> str:
        """Export batch calculation results as CSV."""
        data = []
        
        for filename, (gcode_data, cost_breakdown) in results.items():
            data.append({
                'File Name': gcode_data.file_name,
                'Slicer': gcode_data.slicer_name,
                'Print Time (hours)': f"{gcode_data.estimated_print_time.total_seconds() / 3600:.2f}",
                'Filament Used (g)': f"{gcode_data.filament_used_grams:.2f}",
                'Max Z Height (mm)': f"{gcode_data.max_z_height:.2f}",
                'Layer Count': gcode_data.layer_count,
                'Material Cost ($)': f"{cost_breakdown.material_cost:.2f}",
                'Electricity Cost ($)': f"{cost_breakdown.electricity_cost:.2f}",
                'Depreciation Cost ($)': f"{cost_breakdown.depreciation_cost:.2f}",
                'Labor Cost ($)': f"{cost_breakdown.labor_cost:.2f}",
                'Total Cost ($)': f"{cost_breakdown.total_cost:.2f}",
                'Cost per Gram ($/g)': f"{cost_breakdown.cost_per_gram:.2f}"
            })
        
        df = pd.DataFrame(data)
        return df.to_csv(index=False)

class ValidationUtils:
    """Utilities for input validation."""
    
    @staticmethod
    def validate_gcode_file(file_content: str) -> tuple[bool, str]:
        """Validate if the uploaded file is a valid GCODE file."""
        if not file_content:
            return False, "File is empty"
        
        # Check for basic GCODE commands
        gcode_indicators = ['G0', 'G1', 'G28', 'M104', 'M109', 'M140', 'M190']
        has_gcode = any(indicator in file_content.upper() for indicator in gcode_indicators)
        
        if not has_gcode:
            return False, "File does not appear to contain valid GCODE commands"
        
        return True, "Valid GCODE file"
    
    @staticmethod
    def validate_positive_number(value: float, field_name: str) -> tuple[bool, str]:
        """Validate that a number is positive."""
        if value <= 0:
            return False, f"{field_name} must be greater than 0"
        return True, ""
    
    @staticmethod
    def validate_percentage(value: float, field_name: str) -> tuple[bool, str]:
        """Validate that a percentage is between 0 and 100."""
        if value < 0 or value > 100:
            return False, f"{field_name} must be between 0 and 100"
        return True, ""

class FormatUtils:
    """Utilities for formatting data for display."""
    
    @staticmethod
    def format_duration(duration: timedelta) -> str:
        """Format timedelta as human-readable string."""
        total_seconds = int(duration.total_seconds())
        hours, remainder = divmod(total_seconds, 3600)
        minutes, seconds = divmod(remainder, 60)
        
        if hours > 0:
            return f"{hours}h {minutes}m {seconds}s"
        elif minutes > 0:
            return f"{minutes}m {seconds}s"
        else:
            return f"{seconds}s"
    
    @staticmethod
    def format_currency(amount: float) -> str:
        """Format amount as currency."""
        return f"${amount:.2f}"
    
    @staticmethod
    def format_weight(grams: float) -> str:
        """Format weight in grams with appropriate units."""
        if grams >= 1000:
            return f"{grams/1000:.2f} kg"
        else:
            return f"{grams:.1f} g"
    
    @staticmethod
    def format_length(millimeters: float) -> str:
        """Format length in millimeters with appropriate units."""
        if millimeters >= 1000:
            return f"{millimeters/1000:.2f} m"
        else:
            return f"{millimeters:.1f} mm"
    
    @staticmethod
    def format_percentage(value: float) -> str:
        """Format percentage with one decimal place."""
        return f"{value:.1f}%"

def get_default_printer_profiles() -> List[PrinterProfile]:
    """Get a list of common printer profiles for quick setup."""
    return [
        PrinterProfile(
            name="Ender 3 V2",
            power_consumption_watts=200,
            purchase_cost=250,
            lifetime_hours=5000,
            maintenance_cost_per_hour=0.01,
            description="Popular entry-level FDM printer"
        ),
        PrinterProfile(
            name="Prusa i3 MK3S+",
            power_consumption_watts=150,
            purchase_cost=750,
            lifetime_hours=8000,
            maintenance_cost_per_hour=0.02,
            description="High-quality FDM printer with excellent reliability"
        ),
        PrinterProfile(
            name="Bambu Lab X1 Carbon",
            power_consumption_watts=350,
            purchase_cost=1200,
            lifetime_hours=10000,
            maintenance_cost_per_hour=0.03,
            description="Advanced FDM printer with automatic features"
        ),
        PrinterProfile(
            name="Ultimaker S3",
            power_consumption_watts=200,
            purchase_cost=3500,
            lifetime_hours=15000,
            maintenance_cost_per_hour=0.05,
            description="Professional dual-extrusion FDM printer"
        )
    ]

def get_default_material_profiles() -> List[MaterialProfile]:
    """Get a list of common material profiles for quick setup."""
    return [
        MaterialProfile(
            name="PLA - Generic",
            price_per_kg=25.0,
            density_g_cm3=1.24,
            material_type="PLA",
            description="Basic PLA filament for general use"
        ),
        MaterialProfile(
            name="PLA - Premium",
            price_per_kg=35.0,
            density_g_cm3=1.24,
            material_type="PLA",
            description="High-quality PLA with better finish"
        ),
        MaterialProfile(
            name="PETG - Generic",
            price_per_kg=30.0,
            density_g_cm3=1.27,
            material_type="PETG",
            description="Chemical resistant and durable"
        ),
        MaterialProfile(
            name="ABS - Generic",
            price_per_kg=28.0,
            density_g_cm3=1.04,
            material_type="ABS",
            description="Strong and heat-resistant thermoplastic"
        ),
        MaterialProfile(
            name="TPU - Flexible",
            price_per_kg=45.0,
            density_g_cm3=1.2,
            material_type="TPU",
            description="Flexible rubber-like material"
        )
    ]
