# 3D Printer Cost Calculator

## Project Overview

The 3D Printer Cost Calculator is an application designed to analyze GCODE files from 3D printing slicers and calculate the total cost of printing a specific model. By parsing GCODE files, the application extracts crucial information such as filament usage, print time, and other parameters to provide accurate cost estimates for 3D printing projects.

This application helps 3D printing enthusiasts, small businesses, and service providers to:
- Calculate exact costs for 3D printing projects
- Make informed decisions about pricing for clients
- Analyze efficiency and cost-effectiveness of different print settings
- Manage and track printing expenses

## Features to be Implemented

### Phase 1: Core Functionality

- **GCODE File Parser**
  - [x] Extract filament usage (length and weight)
  - [x] Extract estimated print time
  - [x] Extract layer information (count, heights)
  - [x] Extract print settings (temperature, speed, etc.)

- **Cost Calculation Engine**
  - [ ] Material cost calculation (based on filament weight and price per kg)
  - [ ] Electricity cost calculation (based on printer power consumption and print time)
  - [ ] Machine depreciation calculation (based on printer cost and usage)
  - [ ] Failure rate adjustment (risk factor for print failures)

- **Basic User Interface**
  - [ ] GCODE file upload/selection
  - [ ] Material cost input (price per kg)
  - [ ] Electricity cost input (price per kWh)
  - [ ] Printer settings (power consumption, cost, etc.)
  - [ ] Cost breakdown display

### Phase 2: Advanced Features

- **Printer Profiles**
  - [ ] Save multiple printer configurations
  - [ ] Different power consumption rates
  - [ ] Specific maintenance costs

- **Material Profiles**
  - [ ] Manage different filament types and their costs
  - [ ] Account for different densities and properties

- **Project Management**
  - [ ] Save projects and their cost calculations
  - [ ] Compare different print settings for cost optimization
  - [ ] Batch processing of multiple GCODE files

- **Enhanced Analytics**
  - [ ] Visualize cost breakdowns
  - [ ] Track printing costs over time
  - [ ] Compare different slicing settings for cost efficiency

### Phase 3: Professional Features

- **Business Tools**
  - [ ] Generate quotes for clients
  - [ ] Export cost reports (PDF, CSV)
  - [ ] Profit margin calculation

- **Integration with Slicers**
  - [ ] Direct connection to popular slicers
  - [ ] Real-time cost calculation as settings change

- **Multi-material Support**
  - Calculate costs for multi-material/multi-extruder prints
  - Account for support material usage separately

## Development Workflow

### Git Branching Strategy

We follow a modified Git Flow approach:

- **main**: Production-ready code, always stable
- **develop**: Integration branch for features, pre-release testing
- **feature/xxx**: Feature branches for new development
- **bugfix/xxx**: Bug fix branches
- **release/x.x.x**: Release preparation branches
- **hotfix/xxx**: Emergency fixes for production

#### Branch Naming Convention

```
<type>/<short-description>

Types:
- feature: New functionality
- bugfix: Bug fixes 
- hotfix: Critical production fixes
- release: Release preparation
- docs: Documentation updates
- refactor: Code improvements without changing functionality
```

Examples:
- `feature/gcode-parser`
- `feature/material-cost-calculator`
- `bugfix/filament-weight-calculation`
- `docs/api-documentation`

### Feature Development Workflow

1. **Create Feature Branch**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/gcode-parser
   ```

2. **Develop and Test**
   - Implement the feature with appropriate tests
   - Commit regularly with meaningful commit messages

3. **Keep Branch Updated**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout feature/gcode-parser
   git merge develop
   # Resolve any conflicts
   ```

4. **Create Pull Request**
   - Push branch: `git push origin feature/gcode-parser`
   - Create PR against `develop` branch
   - Include description of changes, testing performed
   - Link to relevant issue/ticket

5. **Code Review**
   - Address review comments
   - Update PR as needed

6. **Merge**
   - PR is merged into `develop` after approval
   - Delete feature branch after merge

### Release Process

1. **Create Release Branch**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b release/1.0.0
   ```

2. **Finalize Release**
   - Update version numbers
   - Perform final testing
   - Fix any release-specific issues

3. **Merge to Main and Develop**
   - Create PR to merge `release/1.0.0` into `main`
   - After merge, tag the release: `git tag -a v1.0.0 -m "Version 1.0.0"`
   - Merge `release/1.0.0` back into `develop`

## Technical Implementation

### GCODE Parsing

GCODE files contain commands that control the 3D printer. Key information to extract includes:

- **Filament Usage**: Found in comments like `;filament used [g] = 45.32`
- **Print Time**: Often indicated with comments like `;estimated printing time = 5h 23m`
- **Layer Info**: Extracted from layer change commands and comments
- **Temperatures**: Extracted from temperature commands (M104, M109, etc.)
- **Print Settings**: From various comments added by the slicer

The GCODE parser will:
1. Read the file line by line
2. Extract metadata from header comments
3. Calculate filament usage from extrusion commands (E values)
4. Interpret timestamps and layer changes
5. Calculate total print time and material usage

### Cost Calculation Algorithm

The core cost calculation will be based on:

```
Total Cost = Material Cost + Electricity Cost + Depreciation Cost + Labor Cost + Profit Margin
```

Where:
- **Material Cost** = Filament Weight (kg) × Filament Price ($/kg)
- **Electricity Cost** = Power Consumption (kW) × Print Time (h) × Electricity Price ($/kWh)
- **Depreciation Cost** = (Printer Cost × Print Time) ÷ Expected Lifetime
- **Labor Cost** = Setup Time (h) × Labor Rate ($/h)
- **Profit Margin** = (Sum of above costs) × Profit Percentage

## Project Setup

### Prerequisites

- .NET 6.0 or later
- Node.js and npm
- Angular CLI
- Git

### Backend Setup (ASP.NET Core)

```bash
# Clone the repository
git clone https://github.com/yourusername/3d-printer-cost-calculator.git
cd 3d-printer-cost-calculator

# Build and run the application
dotnet build
dotnet run
```

### Frontend Setup (Angular)

```bash
# Navigate to ClientApp directory
cd ClientApp

# Install dependencies
npm install

# Start Angular development server
npm start
```

### Development Server

The application can be run in development mode with:

```bash
dotnet run
```

This will start both the ASP.NET Core backend and Angular frontend through the proxy configuration.

## Contributing

1. Fork the repository
2. Create your feature branch following our branching strategy
3. Implement your changes with appropriate tests
4. Ensure existing tests pass: `dotnet test`
5. Submit a pull request

Please adhere to our coding standards and commit message guidelines.

## License

[Specify license here]

---

# Angular Specific Information

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 12.0.2.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).


xunit
fluentassertions
moq
coverlet collector
microsoft net test sdk
microsoft aspnetcore testhost

Theory
InlineData