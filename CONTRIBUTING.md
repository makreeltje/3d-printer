# Contributing to 3D Printer Cost Calculator

Thank you for your interest in contributing to the 3D Printer Cost Calculator! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites
- Python 3.11 or higher
- UV package manager
- Docker and Docker Compose
- Git

### Development Setup
1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/3d-printer-cost-calculator.git
   cd 3d-printer-cost-calculator
   ```
3. Install dependencies:
   ```bash
   curl -LsSf https://astral.sh/uv/install.sh | sh
   uv sync
   ```
4. Set up pre-commit hooks (optional but recommended):
   ```bash
   uv run pre-commit install
   ```

## Development Workflow

### Making Changes
1. Create a new branch for your feature:
   ```bash
   git checkout -b feature/your-feature-name
   ```
2. Make your changes
3. Add tests for new functionality
4. Run the test suite:
   ```bash
   uv run pytest
   ```
5. Run code formatting:
   ```bash
   uv run black .
   uv run isort .
   ```

### Commit Guidelines
- Use clear, descriptive commit messages
- Follow the format: `type(scope): description`
- Examples:
  - `feat(parser): add support for PrusaSlicer GCODE`
  - `fix(cost): correct electricity calculation formula`
  - `docs(readme): update installation instructions`

## Code Guidelines

### Python Style
- Follow PEP 8 style guidelines
- Use type hints for all functions
- Maximum line length: 88 characters (Black default)
- Use meaningful variable and function names

### Testing
- Write tests for all new features
- Maintain test coverage above 80%
- Use descriptive test names
- Test both success and error cases

### Documentation
- Add docstrings for all public functions and classes
- Update README for new features
- Include inline comments for complex logic

## Types of Contributions

### Bug Reports
- Use the GitHub issue template
- Include steps to reproduce
- Provide system information
- Include relevant log output

### Feature Requests
- Describe the use case
- Explain the expected behavior
- Consider backwards compatibility

### Code Contributions
- GCODE parser improvements
- Cost calculation enhancements
- UI/UX improvements
- Database optimizations
- Documentation updates

## Testing

### Running Tests
```bash
# Run all tests
uv run pytest

# Run with coverage
uv run pytest --cov=. --cov-report=html

# Run specific test file
uv run pytest tests/test_gcode_parsers.py -v
```

### Writing Tests
- Place tests in the `tests/` directory
- Use descriptive test class and method names
- Test edge cases and error conditions
- Mock external dependencies appropriately

## Documentation

### Code Documentation
- Use Google-style docstrings
- Document parameters, return values, and exceptions
- Include usage examples for complex functions

### User Documentation
- Update README for new features
- Add configuration examples
- Include troubleshooting information

## Release Process

### Version Numbering
We follow Semantic Versioning (SemVer):
- MAJOR: Breaking changes
- MINOR: New features (backwards compatible)
- PATCH: Bug fixes (backwards compatible)

### Release Checklist
1. Update version in `pyproject.toml`
2. Update CHANGELOG.md
3. Run full test suite
4. Create release PR
5. Tag release after merge

## Community Guidelines

### Code of Conduct
- Be respectful and inclusive
- Focus on constructive feedback
- Help newcomers learn and contribute
- Acknowledge different perspectives

### Getting Help
- Check existing issues and documentation
- Ask questions in GitHub Discussions
- Join our community chat (if available)
- Reach out to maintainers for guidance

## License

By contributing, you agree that your contributions will be licensed under the MIT License.