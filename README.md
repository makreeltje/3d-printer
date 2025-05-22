# 3D Printer Cost Calculator

A comprehensive web application for calculating accurate 3D printing costs with GCODE parsing, profile management, and detailed analytics. Built with Streamlit and designed for professional use with Docker deployment.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Python](https://img.shields.io/badge/python-3.11-blue.svg)
![Docker](https://img.shields.io/badge/docker-ready-green.svg)

## Features

### Core Functionality
- **GCODE Analysis**: Intelligent parsing of GCODE files from multiple slicers (Bambu Studio, Orca Slicer, Creality Print)
- **Comprehensive Cost Calculation**: Material, electricity, machine depreciation, labor, failure rates, and profit margins
- **Profile Management**: Save and reuse printer and material configurations
- **Batch Processing**: Calculate costs for multiple prints simultaneously
- **Data Persistence**: PostgreSQL database with fallback to session storage

### Analytics & Visualization
- **Cost Breakdown Charts**: Interactive pie charts and bar graphs showing cost components
- **Print History**: Track all your prints with searchable history
- **Analytics Dashboard**: Trends, statistics, and insights over time
- **Export Capabilities**: CSV export for reports and analysis

### Supported Slicers
- **Bambu Studio**: Full metadata extraction including time estimates and material usage
- **Orca Slicer**: Advanced parsing with temperature and speed data
- **Creality Print**: Complete support for Creality ecosystem
- **Generic Support**: Fallback parser for any GCODE format

## Quick Start with Docker

### Prerequisites
- Docker and Docker Compose installed
- Git for cloning the repository

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/3d-printer-cost-calculator.git
cd 3d-printer-cost-calculator
```

### 2. Start with Docker Compose
```bash
docker-compose up -d
```

### 3. Access the Application
Open your browser and navigate to `http://localhost:5000`

The application will be ready with a PostgreSQL database automatically configured.

## Self-Hosted Deployment

### Environment Configuration

Create a `.env` file in the project root:

```env
# Database Configuration
DATABASE_URL=postgresql://username:password@host:port/database
POSTGRES_DB=cost_calculator_db
POSTGRES_USER=cost_calculator
POSTGRES_PASSWORD=your_secure_password

# Application Settings
STREAMLIT_SERVER_PORT=5000
STREAMLIT_SERVER_ADDRESS=0.0.0.0

# Optional: Analytics and Monitoring
SENTRY_DSN=your_sentry_dsn_if_using_error_tracking
```

### Custom Database Setup

#### Using External PostgreSQL
```bash
# Set your database URL
export DATABASE_URL="postgresql://user:pass@your-db-host:5432/your-db"
docker run -p 5000:5000 -e DATABASE_URL="$DATABASE_URL" yourusername/3d-printer-cost-calculator
```

#### Using SQLite (for single-user deployments)
```bash
# For local file-based storage
docker run -p 5000:5000 -v $(pwd)/data:/app/data yourusername/3d-printer-cost-calculator
```

### Advanced Docker Configuration

#### Custom docker-compose.yml for Production
```yaml
version: '3.8'

services:
  app:
    image: yourusername/3d-printer-cost-calculator:latest
    ports:
      - "80:5000"
    environment:
      - DATABASE_URL=postgresql://cost_calc:${DB_PASSWORD}@db:5432/cost_calculator
    depends_on:
      - db
    restart: unless-stopped
    volumes:
      - ./uploads:/app/uploads
    networks:
      - app-network

  db:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=cost_calculator
      - POSTGRES_USER=cost_calc
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./backups:/backups
    restart: unless-stopped
    networks:
      - app-network

  nginx:
    image: nginx:alpine
    ports:
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - app
    restart: unless-stopped
    networks:
      - app-network

volumes:
  postgres_data:

networks:
  app-network:
```

## Development Setup

### Local Development
```bash
# Clone repository
git clone https://github.com/yourusername/3d-printer-cost-calculator.git
cd 3d-printer-cost-calculator

# Install UV package manager
curl -LsSf https://astral.sh/uv/install.sh | sh

# Install dependencies
uv sync

# Set up local database (optional)
export DATABASE_URL="postgresql://localhost/cost_calculator_dev"

# Run the application
uv run streamlit run app.py --server.port 5000
```

### Running Tests
```bash
# Run all tests
uv run pytest

# Run with coverage
uv run pytest --cov=. --cov-report=html

# Run specific test modules
uv run pytest tests/test_gcode_parsers.py -v
```

## Configuration Options

### Database Settings
| Variable | Description | Default |
|----------|-------------|---------|
| `DATABASE_URL` | Full PostgreSQL connection string | Required for persistence |
| `POSTGRES_DB` | Database name | `cost_calculator_db` |
| `POSTGRES_USER` | Database username | `cost_calculator` |
| `POSTGRES_PASSWORD` | Database password | `password` |

### Application Settings
| Variable | Description | Default |
|----------|-------------|---------|
| `STREAMLIT_SERVER_PORT` | Application port | `5000` |
| `STREAMLIT_SERVER_ADDRESS` | Bind address | `0.0.0.0` |
| `STREAMLIT_SERVER_HEADLESS` | Run without browser | `true` |

### Performance Tuning
| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_MAX_CONNECTIONS` | Database connection pool | `20` |
| `STREAMLIT_SERVER_MAX_UPLOAD_SIZE` | Max file upload size | `200` |

## Usage Guide

### 1. Upload GCODE Files
- Drag and drop GCODE files or use the file picker
- Supports .gcode, .g, and .nc file extensions
- Automatic slicer detection and metadata extraction

### 2. Configure Cost Parameters
- **Material Costs**: Price per kilogram of filament
- **Electricity**: Local energy rates per kWh
- **Machine Costs**: Printer purchase price and expected lifetime
- **Labor**: Setup time and hourly rates
- **Business Factors**: Failure rates and profit margins

### 3. Save Profiles
- Create reusable printer profiles for different machines
- Save material profiles for various filament types
- Quick selection for consistent calculations

### 4. Batch Processing
- Upload multiple GCODE files for bulk cost analysis
- Compare costs across different print jobs
- Export results for reporting

### 5. Analytics
- View print history and cost trends
- Analyze material usage patterns
- Track profitability over time

## Cost Calculation Formula

The application uses a comprehensive formula to ensure accurate cost estimation:

```
Total Cost = Material + Electricity + Depreciation + Labor + Failure_Adjustment + Profit

Where:
- Material = (filament_grams ÷ 1000) × price_per_kg
- Electricity = (power_watts ÷ 1000) × print_hours × electricity_rate
- Depreciation = (printer_cost ÷ lifetime_hours) × print_hours
- Labor = (print_hours + setup_hours) × labor_rate
- Failure_Adjustment = subtotal × failure_rate_percent
- Profit = adjusted_subtotal × profit_margin_percent
```

## API Reference

### GCODE Parser Factory
```python
from gcode_parsers import GcodeParserFactory

factory = GcodeParserFactory()
parsed_data = factory.parse_gcode(gcode_content, filename)
```

### Cost Calculation Engine
```python
from cost_calculator import CostCalculationEngine
from models import CostCalculationInput

cost_breakdown = CostCalculationEngine.calculate_cost(calc_input)
```

## Contributing

### Development Workflow
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes and add tests
4. Run the test suite: `uv run pytest`
5. Commit your changes: `git commit -m 'Add amazing feature'`
6. Push to the branch: `git push origin feature/amazing-feature`
7. Open a Pull Request

### Code Quality
- Follow PEP 8 style guidelines
- Add type hints for all functions
- Write comprehensive tests for new features
- Update documentation for any API changes

## CI/CD Pipeline

The project includes a complete GitHub Actions workflow:

- **Testing**: Automated test execution with coverage reporting
- **Security**: Vulnerability scanning with Trivy
- **Building**: Multi-platform Docker image creation
- **Deployment**: Automatic deployment to Docker Hub

### Setting Up CI/CD
1. Add Docker Hub credentials to GitHub Secrets:
   - `DOCKER_USERNAME`: Your Docker Hub username
   - `DOCKER_PASSWORD`: Your Docker Hub access token

2. The pipeline automatically:
   - Runs tests on every push and PR
   - Builds and pushes Docker images on main branch
   - Provides security scanning and coverage reports

## Troubleshooting

### Common Issues

#### Database Connection Errors
```bash
# Check database connectivity
docker-compose logs db

# Reset database
docker-compose down -v
docker-compose up -d
```

#### Memory Issues with Large GCODE Files
```bash
# Increase Docker memory limit
docker run -m 2g yourusername/3d-printer-cost-calculator
```

#### Port Already in Use
```bash
# Use different port
docker run -p 8080:5000 yourusername/3d-printer-cost-calculator
```

### Performance Optimization
- Use PostgreSQL for better performance with large datasets
- Configure connection pooling for high-traffic deployments
- Enable caching for frequently accessed profiles

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Documentation**: Check this README and inline code documentation
- **Issues**: Report bugs and request features on GitHub Issues
- **Discussions**: Join community discussions on GitHub Discussions

## Acknowledgments

- Built with [Streamlit](https://streamlit.io/) for the web interface
- [Plotly](https://plotly.com/) for interactive visualizations
- [PostgreSQL](https://postgresql.org/) for reliable data storage
- [Docker](https://docker.com/) for containerization
- Thanks to the 3D printing community for feedback and feature requests