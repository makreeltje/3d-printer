"""
Database layer for the 3D printer cost calculator.
Handles persistent storage of profiles, print history, and analytics data.
"""
import os
from datetime import datetime, timedelta
from typing import List, Optional, Dict, Any

import dateutil
from sqlalchemy import create_engine, Column, Integer, Float, String, DateTime, Text, JSON
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, Session
from models import PrinterProfile, MaterialProfile, ParsedGcode, CostBreakdown

# Database setup
DATABASE_URL = os.getenv('DATABASE_URL')
if DATABASE_URL:
    try:
        engine = create_engine(DATABASE_URL, pool_pre_ping=True, pool_recycle=300)
        SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
        Base = declarative_base()
    except Exception as e:
        print(f"Database connection failed: {e}")
        engine = None
        SessionLocal = None
        Base = None
else:
    engine = None
    SessionLocal = None
    Base = None

# Database Models
class DBPrinterProfile(Base):
    __tablename__ = "printer_profiles"
    
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, unique=True, index=True)
    power_consumption_watts = Column(Float)
    purchase_cost = Column(Float)
    lifetime_hours = Column(Float)
    maintenance_cost_per_hour = Column(Float, default=0.0)
    description = Column(Text, default="")
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

class DBMaterialProfile(Base):
    __tablename__ = "material_profiles"
    
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, unique=True, index=True)
    price_per_kg = Column(Float)
    density_g_cm3 = Column(Float)
    material_type = Column(String)
    description = Column(Text, default="")
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

class DBPrintHistory(Base):
    __tablename__ = "print_history"
    
    id = Column(Integer, primary_key=True, index=True)
    file_name = Column(String)
    slicer_name = Column(String)
    estimated_print_time_seconds = Column(Float)
    filament_used_grams = Column(Float)
    filament_used_millimeters = Column(Float)
    layer_count = Column(Integer)

    # Cost breakdown
    material_cost = Column(Float)
    electricity_cost = Column(Float)
    depreciation_cost = Column(Float)
    labor_cost = Column(Float)
    failure_adjustment = Column(Float)
    subtotal = Column(Float)
    profit_margin = Column(Float)
    total_cost = Column(Float)
    cost_per_gram = Column(Float)
    cost_per_hour = Column(Float)
    
    # Cost parameters used
    filament_price_per_kg = Column(Float)
    electricity_price_per_kwh = Column(Float)
    printer_power_watts = Column(Float)
    printer_cost = Column(Float)
    printer_lifetime_hours = Column(Float)
    setup_time_hours = Column(Float, default=0.0)
    labor_rate_per_hour = Column(Float, default=0.0)
    profit_margin_percent = Column(Float, default=0.0)
    failure_rate_percent = Column(Float, default=0.0)
    
    # Metadata
    printer_profile_name = Column(String, nullable=True)
    material_profile_name = Column(String, nullable=True)
    printed_at = Column(DateTime, default=datetime.utcnow)
    created_at = Column(DateTime, default=datetime.utcnow)

# Database operations
class DatabaseManager:
    """Database manager for CRUD operations."""
    
    def __init__(self):
        self.engine = engine
        self.SessionLocal = SessionLocal
        self.db_available = engine is not None
        
    def init_db(self):
        """Initialize the database tables."""
        if not self.db_available:
            return False
        try:
            Base.metadata.create_all(bind=self.engine)
            return True
        except Exception as e:
            print(f"Failed to initialize database: {e}")
            self.db_available = False
            return False
    
    def get_session(self) -> Session:
        """Get a database session."""
        return self.SessionLocal()
    
    # Printer Profile operations
    def save_printer_profile(self, profile: PrinterProfile) -> bool:
        """Save a printer profile to the database."""
        try:
            session = self.get_session()
            
            # Check if profile exists
            existing = session.query(DBPrinterProfile).filter(
                DBPrinterProfile.name == profile.name
            ).first()
            
            if existing:
                # Update existing profile
                existing.power_consumption_watts = profile.power_consumption_watts
                existing.purchase_cost = profile.purchase_cost
                existing.lifetime_hours = profile.lifetime_hours
                existing.maintenance_cost_per_hour = profile.maintenance_cost_per_hour
                existing.description = profile.description
                existing.updated_at = datetime.utcnow()
            else:
                # Create new profile
                db_profile = DBPrinterProfile(
                    name=profile.name,
                    power_consumption_watts=profile.power_consumption_watts,
                    purchase_cost=profile.purchase_cost,
                    lifetime_hours=profile.lifetime_hours,
                    maintenance_cost_per_hour=profile.maintenance_cost_per_hour,
                    description=profile.description
                )
                session.add(db_profile)
            
            session.commit()
            session.close()
            return True
        except Exception as e:
            session.rollback()
            session.close()
            print(f"Error saving printer profile: {e}")
            return False
    
    def load_printer_profile(self, name: str) -> Optional[PrinterProfile]:
        """Load a printer profile from the database."""
        try:
            session = self.get_session()
            db_profile = session.query(DBPrinterProfile).filter(
                DBPrinterProfile.name == name
            ).first()
            session.close()
            
            if db_profile:
                return PrinterProfile(
                    name=db_profile.name,
                    power_consumption_watts=db_profile.power_consumption_watts,
                    purchase_cost=db_profile.purchase_cost,
                    lifetime_hours=db_profile.lifetime_hours,
                    maintenance_cost_per_hour=db_profile.maintenance_cost_per_hour,
                    description=db_profile.description
                )
            return None
        except Exception as e:
            session.close()
            print(f"Error loading printer profile: {e}")
            return None
    
    def get_printer_profile_names(self) -> List[str]:
        """Get list of printer profile names."""
        try:
            session = self.get_session()
            profiles = session.query(DBPrinterProfile.name).all()
            session.close()
            return [profile.name for profile in profiles]
        except Exception as e:
            session.close()
            print(f"Error getting printer profile names: {e}")
            return []
    
    def delete_printer_profile(self, name: str) -> bool:
        """Delete a printer profile."""
        try:
            session = self.get_session()
            profile = session.query(DBPrinterProfile).filter(
                DBPrinterProfile.name == name
            ).first()
            
            if profile:
                session.delete(profile)
                session.commit()
                session.close()
                return True
            
            session.close()
            return False
        except Exception as e:
            session.rollback()
            session.close()
            print(f"Error deleting printer profile: {e}")
            return False
    
    # Material Profile operations
    def save_material_profile(self, profile: MaterialProfile) -> bool:
        """Save a material profile to the database."""
        try:
            session = self.get_session()
            
            # Check if profile exists
            existing = session.query(DBMaterialProfile).filter(
                DBMaterialProfile.name == profile.name
            ).first()
            
            if existing:
                # Update existing profile
                existing.price_per_kg = profile.price_per_kg
                existing.density_g_cm3 = profile.density_g_cm3
                existing.material_type = profile.material_type
                existing.description = profile.description
                existing.updated_at = datetime.utcnow()
            else:
                # Create new profile
                db_profile = DBMaterialProfile(
                    name=profile.name,
                    price_per_kg=profile.price_per_kg,
                    density_g_cm3=profile.density_g_cm3,
                    material_type=profile.material_type,
                    description=profile.description
                )
                session.add(db_profile)
            
            session.commit()
            session.close()
            return True
        except Exception as e:
            session.rollback()
            session.close()
            print(f"Error saving material profile: {e}")
            return False
    
    def load_material_profile(self, name: str) -> Optional[MaterialProfile]:
        """Load a material profile from the database."""
        try:
            session = self.get_session()
            db_profile = session.query(DBMaterialProfile).filter(
                DBMaterialProfile.name == name
            ).first()
            session.close()
            
            if db_profile:
                return MaterialProfile(
                    name=db_profile.name,
                    price_per_kg=db_profile.price_per_kg,
                    density_g_cm3=db_profile.density_g_cm3,
                    material_type=db_profile.material_type,
                    description=db_profile.description
                )
            return None
        except Exception as e:
            session.close()
            print(f"Error loading material profile: {e}")
            return None
    
    def get_material_profile_names(self) -> List[str]:
        """Get list of material profile names."""
        try:
            session = self.get_session()
            profiles = session.query(DBMaterialProfile.name).all()
            session.close()
            return [profile.name for profile in profiles]
        except Exception as e:
            session.close()
            print(f"Error getting material profile names: {e}")
            return []
    
    def delete_material_profile(self, name: str) -> bool:
        """Delete a material profile."""
        try:
            session = self.get_session()
            profile = session.query(DBMaterialProfile).filter(
                DBMaterialProfile.name == name
            ).first()
            
            if profile:
                session.delete(profile)
                session.commit()
                session.close()
                return True
            
            session.close()
            return False
        except Exception as e:
            session.rollback()
            session.close()
            print(f"Error deleting material profile: {e}")
            return False
    
    # Print History operations
    def save_print_record(self, gcode_data: ParsedGcode, cost_breakdown: CostBreakdown, 
                         cost_params: Dict[str, Any], printer_profile_name: Optional[str] = None,
                         material_profile_name: Optional[str] = None) -> bool:
        """Save a print record to the database."""
        try:
            session = self.get_session()
            
            print_record = DBPrintHistory(
                file_name=gcode_data.file_name,
                slicer_name=gcode_data.slicer_name,
                estimated_print_time_seconds=gcode_data.estimated_print_time.total_seconds(),
                filament_used_grams=gcode_data.filament_used_grams,
                filament_used_millimeters=gcode_data.filament_used_millimeters,
                layer_count=gcode_data.layer_count,
                nozzle_temperature=gcode_data.nozzle_temperature,
                bed_temperature=gcode_data.bed_temperature,

                # Cost breakdown
                material_cost=cost_breakdown.material_cost,
                electricity_cost=cost_breakdown.electricity_cost,
                depreciation_cost=cost_breakdown.depreciation_cost,
                labor_cost=cost_breakdown.labor_cost,
                failure_adjustment=cost_breakdown.failure_adjustment,
                subtotal=cost_breakdown.subtotal,
                profit_margin=cost_breakdown.profit_margin,
                total_cost=cost_breakdown.total_cost,
                cost_per_gram=cost_breakdown.cost_per_gram,
                cost_per_hour=cost_breakdown.cost_per_hour,
                
                # Cost parameters
                filament_price_per_kg=cost_params.get('filament_price_per_kg', 0),
                electricity_price_per_kwh=cost_params.get('electricity_price_per_kwh', 0),
                printer_power_watts=cost_params.get('printer_power_watts', 0),
                printer_cost=cost_params.get('printer_cost', 0),
                printer_lifetime_hours=cost_params.get('printer_lifetime_hours', 0),
                setup_time_hours=cost_params.get('setup_time_hours', 0),
                labor_rate_per_hour=cost_params.get('labor_rate_per_hour', 0),
                profit_margin_percent=cost_params.get('profit_margin_percent', 0),
                failure_rate_percent=cost_params.get('failure_rate_percent', 0),
                
                # Profile references
                printer_profile_name=printer_profile_name,
                material_profile_name=material_profile_name
            )
            
            session.add(print_record)
            session.commit()
            session.close()
            return True
        except Exception as e:
            session.rollback()
            session.close()
            print(f"Error saving print record: {e}")
            return False
    
    def get_print_history(self, limit: int = 100) -> List[Dict[str, Any]]:
        """Get print history records."""
        try:
            session = self.get_session()
            records = session.query(DBPrintHistory).order_by(
                DBPrintHistory.printed_at.desc()
            ).limit(limit).all()
            session.close()
            
            history = []
            for record in records:
                history.append({
                    'id': record.id,
                    'file_name': record.file_name,
                    'slicer_name': record.slicer_name,
                    'print_time_hours': record.estimated_print_time_seconds / 3600,
                    'filament_grams': record.filament_used_grams,
                    'total_cost': record.total_cost,
                    'material_cost': record.material_cost,
                    'electricity_cost': record.electricity_cost,
                    'printed_at': record.printed_at,
                    'printer_profile': record.printer_profile_name,
                    'material_profile': record.material_profile_name
                })
            
            return history
        except Exception as e:
            session.close()
            print(f"Error getting print history: {e}")
            return []
    
    def get_analytics_data(self, days: int = 30) -> Dict[str, Any]:
        """Get analytics data for the specified number of days."""
        try:
            session = self.get_session()
            cutoff_date = datetime.utcnow() - timedelta(days=days)
            
            records = session.query(DBPrintHistory).filter(
                DBPrintHistory.printed_at >= cutoff_date
            ).all()
            session.close()
            
            if not records:
                return {
                    'total_prints': 0,
                    'total_cost': 0,
                    'total_time_hours': 0,
                    'total_filament_grams': 0,
                    'avg_cost_per_print': 0,
                    'cost_by_day': [],
                    'cost_by_slicer': {},
                    'cost_breakdown': {
                        'material': 0,
                        'electricity': 0,
                        'depreciation': 0,
                        'labor': 0
                    }
                }
            
            total_cost = sum(r.total_cost for r in records)
            total_time = sum(r.estimated_print_time_seconds for r in records) / 3600
            total_filament = sum(r.filament_used_grams for r in records)
            
            # Cost by slicer
            cost_by_slicer = {}
            for record in records:
                if record.slicer_name not in cost_by_slicer:
                    cost_by_slicer[record.slicer_name] = 0
                cost_by_slicer[record.slicer_name] += record.total_cost
            
            # Cost breakdown
            material_cost = sum(r.material_cost for r in records)
            electricity_cost = sum(r.electricity_cost for r in records)
            depreciation_cost = sum(r.depreciation_cost for r in records)
            labor_cost = sum(r.labor_cost for r in records)
            
            return {
                'total_prints': len(records),
                'total_cost': total_cost,
                'total_time_hours': total_time,
                'total_filament_grams': total_filament,
                'avg_cost_per_print': total_cost / len(records) if records else 0,
                'cost_by_slicer': cost_by_slicer,
                'cost_breakdown': {
                    'material': material_cost,
                    'electricity': electricity_cost,
                    'depreciation': depreciation_cost,
                    'labor': labor_cost
                }
            }
        except Exception as e:
            session.close()
            print(f"Error getting analytics data: {e}")
            return {}

# Global database manager instance
db_manager = DatabaseManager()