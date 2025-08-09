# Test Projects for VS Libraries

This directory contains comprehensive test projects for the Visual Studio libraries in the UiPath monorepo.

## Test Projects

### Yash.Orchestrator.Tests
Tests for the UiPath Orchestrator service library (`Yash.Orchestrator`).

**Test Coverage:**
- `OrchestratorServiceTests.cs`: Tests for REST API functionality, token management, folder operations, asset management, and bucket operations
- Mocking with Moq framework for REST client dependencies
- Comprehensive error handling and validation tests

**Key Test Scenarios:**
- ✅ Token authentication with valid/invalid credentials
- ✅ Folder retrieval and creation
- ✅ Asset management operations
- ✅ Bucket operations and file management
- ✅ Error handling for invalid URLs and parameters
- ✅ REST API response parsing and serialization

### Yash.Config.Tests
Tests for the configuration management library (`Yash.Config`).

**Test Coverage:**
- `Activities/LoadConfigTests.cs`: Tests for the LoadConfig UiPath activity
- `Helpers/HelpersTests.cs`: Tests for configuration, Excel, and JSON helper classes
- `Models/ModelsTests.cs`: Tests for configuration model classes and factory patterns

**Key Test Scenarios:**
- ✅ Excel file loading and parsing
- ✅ Environment and component filtering
- ✅ Configuration validation and structure checks
- ✅ Asset integration with UiPath Orchestrator
- ✅ JSON serialization/deserialization
- ✅ Configuration model validation
- ✅ Helper method functionality

## Test Framework Features

### Testing Stack
- **MSTest**: Primary testing framework
- **FluentAssertions**: Readable assertion syntax
- **Moq**: Mocking framework for dependencies
- **ClosedXML**: Excel file manipulation for test data
- **Newtonsoft.Json**: JSON handling in tests

### Test Data Management
- `TestData/` folders contain sample Excel files and configuration data
- Automatic test data cleanup after test execution
- Dynamic test data generation for different scenarios

### Code Coverage
- **coverlet.collector** integration for code coverage reporting
- Coverage reports generated during test execution
- Integrated with GitHub Actions for CI/CD pipeline

## Running Tests

### Local Development
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test vs-libraries/Yash.Config.Tests
dotnet test vs-libraries/Yash.Orchestrator.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### GitHub Actions Integration
Tests are automatically executed as part of the CI/CD pipeline:
- Matrix builds ensure tests run for all project dependencies
- Test results are published as TRX reports
- Code coverage metrics are collected and reported
- Test failures block deployments to prevent regression

### Test Project Structure
```
vs-libraries/
├── Yash.Config.Tests/
│   ├── Activities/
│   │   └── LoadConfigTests.cs
│   ├── Helpers/
│   │   └── HelpersTests.cs
│   ├── Models/
│   │   └── ModelsTests.cs
│   ├── TestData/
│   │   └── (Excel files, JSON configs)
│   └── Yash.Config.Tests.csproj
├── Yash.Orchestrator.Tests/
│   ├── OrchestratorServiceTests.cs
│   └── Yash.Orchestrator.Tests.csproj
```

## Development Guidelines

### Adding New Tests
1. Follow the existing naming conventions (`ClassNameTests.cs`)
2. Use FluentAssertions for readable test assertions
3. Mock external dependencies using Moq framework
4. Include both positive and negative test scenarios
5. Add test data files to `TestData/` directories

### Test Maintenance
- Keep test data minimal and focused
- Clean up resources in `[TestCleanup]` methods
- Use descriptive test method names that explain the scenario
- Group related tests in the same test class

### Dependencies
Both test projects reference their corresponding main projects and include all necessary UiPath activity dependencies for comprehensive testing of UiPath-specific functionality.
