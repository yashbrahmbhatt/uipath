# Yash.Orchestrator.Tests

Comprehensive test suite for the `Yash.Orchestrator` library, providing validation for UiPath Cloud Orchestrator API interactions, authentication flows, and service functionality.

---

## 🧪 Test Overview

This test project validates all aspects of the `OrchestratorService` class using MSTest framework with FluentAssertions for readable assertions and Moq for test doubles.

### Test Coverage

- ✅ **Interface Implementation**: Validates `IOrchestratorService` contract compliance
- ✅ **Constructor Behavior**: Tests initialization with different authentication methods
- ✅ **Collection Management**: Verifies proper initialization of observable collections
- ✅ **Token Authentication**: Tests OAuth2 token management and refresh logic
- ✅ **Logging Integration**: Validates logging functionality and event types
- ✅ **Error Handling**: Tests exception scenarios and edge cases
- ✅ **Authentication Providers**: Tests both client credentials and IAccessProvider modes

---

## 🏗️ Test Architecture

### Test Framework Stack
- **MSTest**: Primary testing framework
- **FluentAssertions**: Readable and expressive assertions
- **Moq**: Mock object framework for dependencies
- **Microsoft.Extensions.Logging**: Logging abstractions for test validation

### Test Structure
```
OrchestratorServiceTests.cs
├── Interface Implementation Tests
├── Constructor Tests
├── Collection Initialization Tests
├── Token Management Tests
├── Authentication Provider Tests
├── Logging Tests
└── Integration Tests
```

---

## 🚀 Running Tests

### Prerequisites
1. .NET 6.0 or higher
2. UiPath Orchestrator credentials (for integration tests)
3. Environment variables configured (see Environment Setup)

### Command Line
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter "TestCategory=Unit"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
1. Open Test Explorer (`Test` → `Test Explorer`)
2. Click "Run All Tests" or run individual test methods
3. View detailed results and output in the Test Explorer window

---

## 🔧 Environment Setup

### Required Environment Variables
For integration tests that connect to UiPath Orchestrator:

```bash
UIP_ACCOUNT_NAME=your-account-name
UIP_APPLICATION_ID=your-client-id
UIP_APPLICATION_SECRET=your-client-secret
UIP_TENANT_NAME=your-tenant-name
```

### Configuration Files
- `.env.example`: Template for environment variables
- `set-uipath-env.sh`: Script to set environment variables for testing

### Setting Up Test Environment
1. Copy `.env.example` to `.env`
2. Fill in your UiPath Orchestrator credentials
3. Run `set-uipath-env.sh` to load environment variables
4. Execute tests

---

## 📊 Test Details

### 1. Interface Implementation Tests
```csharp
[TestMethod]
public void OrchestratorService_ShouldImplementIOrchestratorService()
```
Validates that `OrchestratorService` properly implements the `IOrchestratorService` interface.

### 2. Constructor Tests
- Tests initialization with client credentials
- Tests initialization with IAccessProvider
- Validates logger integration
- Tests parameter validation

### 3. Collection Tests
```csharp
[TestMethod]
public void Constructor_ShouldInitializeCollections()
```
Ensures all observable collections (`Folders`, `Assets`, `Buckets`, `BucketFiles`) are properly initialized.

### 4. Token Management Tests
- Tests token refresh with force parameter
- Validates token caching behavior
- Tests authentication failure scenarios
- Verifies logging of authentication events

### 5. Authentication Provider Tests
- Tests IAccessProvider integration
- Validates token retrieval from providers
- Tests provider failure scenarios

### 6. Logging Tests
- Validates log message generation
- Tests different log levels
- Verifies log action delegation

---

## 🎯 Test Categories

### Unit Tests
Fast, isolated tests that don't require external dependencies:
- Constructor validation
- Collection initialization
- Interface implementation
- Mock-based authentication tests

### Integration Tests
Tests that interact with real UiPath Orchestrator APIs:
- Live authentication flows
- API endpoint validation
- Real token management

---

## 🐛 Debugging Tests

### Common Issues
1. **Authentication Failures**: Verify environment variables are set correctly
2. **Network Issues**: Check connectivity to UiPath Cloud
3. **Token Expiration**: Tests handle token refresh automatically
4. **Rate Limiting**: Some tests may be throttled by Orchestrator API limits

### Debugging Tips
- Use `--verbosity diagnostic` for detailed test output
- Check test logs for authentication and API call details
- Verify environment variables with `set-uipath-env.sh`
- Use breakpoints in test methods for step-by-step debugging

---

## 📈 Test Metrics

### Current Test Coverage
- **Total Tests**: 18
- **Success Rate**: 100% (18/18 passing)
- **Execution Time**: ~3 seconds
- **Code Coverage**: Comprehensive coverage of public API surface

### Test Categories Breakdown
- Interface Tests: 2 tests
- Constructor Tests: 4 tests
- Collection Tests: 3 tests
- Token Management: 5 tests
- Authentication: 2 tests
- Logging: 2 tests

---

## 🔄 Continuous Integration

### GitHub Actions Integration
Tests are automatically executed on:
- Pull requests to main branch
- Commits to main branch
- Manual workflow triggers

### Build Pipeline
1. Restore NuGet packages
2. Build solution
3. Run all tests
4. Generate test reports
5. Publish test results

---

## 📚 Dependencies

### Test Framework Dependencies
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="MSTest.TestAdapter" />
<PackageReference Include="MSTest.TestFramework" />
<PackageReference Include="FluentAssertions" />
<PackageReference Include="Moq" />
<PackageReference Include="Microsoft.Extensions.Logging" />
```

### Project Dependencies
- `Yash.Orchestrator`: Main library under test
- `UiPath.Activities.Api`: For IAccessProvider integration

---

## 🤝 Contributing

### Adding New Tests
1. Follow the existing naming convention: `MethodName_Scenario_ExpectedResult`
2. Use FluentAssertions for readable assertions
3. Include both positive and negative test cases
4. Add appropriate test categories and documentation

### Test Guidelines
- Keep tests focused and atomic
- Use descriptive test names
- Mock external dependencies
- Validate both success and failure scenarios
- Include edge cases and boundary conditions

---

## 📖 License

This test project is licensed under the MIT License, same as the main `Yash.Orchestrator` library.

---

*Built and maintained by Yash Brahmbhatt as part of the UiPath automation toolkit.*