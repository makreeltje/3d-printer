# 3D Printer Cost Calculator

## Overview

This is a Streamlit-based web application that analyzes GCODE files to calculate comprehensive 3D printing costs. The application parses GCODE files from various slicers, extracts printing metadata, and provides detailed cost breakdowns including material, electricity, machine depreciation, labor, and profit margins.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Frontend Architecture
- **Framework**: Streamlit web application
- **Visualization**: Plotly for interactive charts and graphs
- **UI Components**: Streamlit widgets for file uploads, forms, and data display
- **Layout**: Wide layout with expandable sidebar for configuration

### Backend Architecture
- **Language**: Python 3.11
- **Design Pattern**: Modular architecture with separated concerns
- **Data Models**: Dataclass-based models for type safety and structure
- **Parser Factory**: Factory pattern for handling different slicer formats
- **Cost Engine**: Standalone calculation engine with comprehensive formula

### Data Storage Solutions
- **Session State**: Streamlit session state for temporary data persistence
- **Profile Management**: In-memory storage for printer and material profiles
- **No Database**: Currently uses session-based storage (extensible to database later)

## Key Components

### Core Modules

1. **Models (models.py)**
   - `ParsedGcode`: Structured GCODE data with metadata
   - `PrinterProfile`: Printer specifications and costs
   - `MaterialProfile`: Filament properties and pricing
   - `CostCalculationInput`: Input parameters for calculations
   - `CostBreakdown`: Detailed cost analysis results

2. **GCODE Parsers (gcode_parsers.py)**
   - Abstract parser interface (`IGcodeParser`)
   - Factory pattern for slicer-specific parsing
   - Utility functions for extracting metadata from GCODE comments
   - Support for multiple slicer formats

3. **Cost Calculator (cost_calculator.py)**
   - `CostCalculationEngine`: Core calculation logic
   - Comprehensive cost formula including material, electricity, depreciation, labor, failure adjustment, and profit
   - Additional metrics like cost per gram and cost per hour

4. **Utilities (utils.py)**
   - `ProfileManager`: CRUD operations for printer/material profiles
   - `DataExporter`: Export functionality for results
   - `ValidationUtils`: Input validation and error handling
   - `FormatUtils`: Data formatting and display utilities

### Cost Calculation Formula
```
Total = Material + Electricity + Depreciation + Labor + Failure_Adjustment + Profit
```

Where:
- Material: (filament_grams / 1000) × price_per_kg
- Electricity: (power_watts / 1000) × print_hours × electricity_rate
- Depreciation: (printer_cost / lifetime_hours) × print_hours
- Labor: (print_hours + setup_hours) × labor_rate
- Failure_Adjustment: subtotal × failure_rate_percent
- Profit: adjusted_subtotal × profit_margin_percent

## Data Flow

1. **File Upload**: User uploads GCODE file through Streamlit interface
2. **Parsing**: Factory pattern determines appropriate parser and extracts metadata
3. **Configuration**: User selects/configures printer and material profiles
4. **Calculation**: Cost engine processes inputs using comprehensive formula
5. **Visualization**: Plotly generates interactive charts and breakdowns
6. **Export**: Results can be exported for reporting and analysis

## External Dependencies

### Python Packages
- **streamlit**: Web application framework
- **plotly**: Interactive data visualization
- **pandas**: Data manipulation and analysis
- **typing**: Type hints for better code quality

### Development Environment
- **Python 3.11**: Runtime environment
- **UV**: Package management
- **Nix**: Development environment configuration

## Deployment Strategy

### Replit Configuration
- **Target**: Autoscale deployment on Replit
- **Port**: 5000 (configured for Streamlit)
- **Run Command**: `streamlit run app.py --server.port 5000`
- **Workflow**: Parallel execution with automatic port detection

### Scalability Considerations
- Session-based storage allows for easy migration to database
- Modular architecture supports adding new slicer parsers
- Cost calculation engine is stateless and thread-safe
- Ready for containerization with minimal configuration changes

### Future Database Integration
The current architecture is designed to easily accommodate database integration:
- Models are structured for ORM compatibility
- ProfileManager can be extended with database operations
- Session state can be replaced with persistent storage
- Batch processing capabilities are built into the design

The application follows clean architecture principles with clear separation of concerns, making it maintainable and extensible for future enhancements like analytics, machine learning integration, and multi-user support.