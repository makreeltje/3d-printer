# 3D Print Cost Calculator

## Project Overview

The 3D Print Cost Calculator is an application designed to help users accurately calculate the costs associated with 3D printing projects. The application processes 3MF (3D Manufacturing Format) files to extract model data and apply customizable cost calculations based on material usage, print time, and other parameters.

**Key Features:**
- Parse and analyze 3MF files to extract printable data
- Calculate printing costs based on customizable parameters
- Support for both local file system and cloud-hosted 3MF files
- Intuitive user interface for file selection and cost review
- Detailed breakdown of costs (material, time, electricity, etc.)

## Development Roadmap

### Phase 1: Core Foundation

1. **Project Setup**
   - Initialize frontend and backend repositories
   - Set up basic CI/CD pipeline
   - Configure development environment

2. **Core Backend Features**
   - 3MF file parsing service
   - Cost calculation algorithms
   - Basic API endpoints
   - Local file system integration

3. **Core Frontend Features**
   - Basic UI layout
   - File upload/selection component
   - Results display component

### Phase 2: Enhanced Functionality

1. **Cloud Integration**
   - Cloud storage connection
   - User authentication
   - Folder structure management

2. **Advanced Calculation Features**
   - Material customization
   - Printer-specific calculations
   - Batch processing

3. **UI Enhancements**
   - Responsive design improvements
   - Visualization of 3D models
   - User preferences

### Phase 3: Polishing & Optimization

1. **Performance Optimization**
   - Calculation speed improvements
   - Caching mechanisms
   - Frontend optimization

2. **Additional Features**
   - Reporting and exports
   - History tracking
   - Comparison tools

## Git Workflow

We use a modified GitHub Flow workflow with feature branches:

### Branch Structure

- **main**: Always stable and deployable
- **feature branches**: For all new development work
- **bugfix branches**: For bug fixes
- **hotfix branches**: For critical production fixes
- **release branches**: For release preparation

### Branch Naming Convention

```
<type>/<description>

Types:
- feature: New functionality
- bugfix: Bug fixes
- hotfix: Critical fixes for production
- design: UI/UX changes
- refactor: Code improvements without changing functionality
- docs: Documentation updates
```

Example: `feature/file-upload-component`

### Pull Request Process

1. Create a feature branch from `main`
2. Develop and test your changes
3. Create a Pull Request to `main`
4. Ensure CI tests pass
5. Get code review approval
6. Merge to `main`

## Release Flow

### Standard Release Process

```bash
# 1. Ensure you are on the main branch with latest changes
git checkout main
git pull origin main

# 2. Create a release branch
git checkout -b release/v1.0.0

# 3. Make any release-specific changes (version numbers, etc.)
# Edit version files as needed

# 4. Commit version changes
git add .
git commit -m "Bump version to 1.0.0"

# 5. Push release branch
git push origin release/v1.0.0

# 6. Create a pull request from release/v1.0.0 to main
# Complete code review and approval process

# 7. After merging the release PR to main:
git checkout main
git pull origin main

# 8. Tag the release
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# 9. Build and deploy production artifacts using CI/CD
# (This step typically happens automatically via CI/CD pipeline)
```

### Hotfix Process

```bash
# 1. Create hotfix branch from production tag
git checkout -b hotfix/critical-bug v1.0.0

# 2. Make the necessary fixes
# Edit files as needed

# 3. Commit changes
git add .
git commit -m "Fix critical bug in calculation algorithm"

# 4. Push hotfix branch
git push origin hotfix/critical-bug

# 5. Create pull request to main
# Complete code review and approval process

# 6. After merging, tag the new patch version
git checkout main
git pull origin main
git tag -a v1.0.1 -m "Hotfix release v1.0.1"
git push origin v1.0.1
```

## Development Workflow Guidance

### Frontend vs Backend Development

#### Backend Development Focus:

- Implementing core calculation logic
- Setting up file parsing services
- Creating new API endpoints
- Database schema updates
- Integration with external services

**Workflow:**
1. Define API contracts first
2. Implement backend services with tests
3. Create API endpoints
4. Document API for frontend team

#### Frontend Development Focus:

- User interface components
- File upload/selection interfaces
- Results visualization
- User experience improvements
- Responsive design implementation

**Workflow:**
1. Create wireframes/mockups
2. Implement UI components
3. Connect to backend APIs
4. Add validation and error handling

### Vertical Slice Approach

For optimal productivity, we implement features as "vertical slices" - complete features from backend to frontend:

1. Define feature requirements
2. Create backend API endpoints
3. Implement frontend components for the feature
4. Test the complete feature end-to-end
5. Refine and polish

## Project Setup

### Prerequisites

- Node.js (v16 or higher)
- npm or yarn
- Git
- [Additional backend requirements]

### Backend Setup

```bash
# Clone the repository
git clone https://github.com/your-org/3d-print-calculator.git
cd 3d-print-calculator/backend

# Install dependencies
npm install

# Configure environment
cp .env.example .env
# Edit .env with your configuration

# Run development server
npm run dev
```

### Frontend Setup

```bash
# Navigate to frontend directory
cd ../frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

### Configuration

- Backend configuration is managed through environment variables
- Frontend configuration is located in `src/config.js`

## Testing

### Backend Tests

```bash
cd backend
npm run test
```

### Frontend Tests

```bash
cd frontend
npm run test
```

### End-to-End Tests

```bash
npm run test:e2e
```

## Contributing

1. Ensure you've read our git workflow documentation
2. Create a feature branch from main
3. Make your changes following our coding standards
4. Write or update tests as needed
5. Create a pull request with a clear description of changes
6. Address review feedback

## License

[Specify license here]

# _3d_print_cost_calculator

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
