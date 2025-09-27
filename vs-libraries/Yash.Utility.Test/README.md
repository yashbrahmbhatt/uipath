# Yash.Utility.Tests

This project contains comprehensive unit tests for the Yash.Utility library services and components.

## Test Structure

The test project follows these conventions:

### Test Classes
- `EmailHelperServiceTests` - Tests for email-related functionality
- `ExcelHelperServiceTests` - Tests for Excel file operations (read, write, template generation)
- `MiscHelperServiceTests` - Tests for miscellaneous utility functions
- `SwaggerCodeGeneratorServiceTests` - Tests for Swagger code generation functionality with integrated file management
- `BaseCodeGenerationServiceTests` - Tests for the base code generation service

### Test Categories

#### Unit Tests
- Test individual methods and components in isolation
- Mock external dependencies where appropriate
- Focus on behavior verification and edge cases

#### Integration Tests
- Test service interactions with external resources (files, HTTP)
- Verify end-to-end functionality
- Test error handling with real scenarios

### Test Patterns

#### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
[TestMethod]
public void Method_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    
    // Act - Execute the method under test
    
    // Assert - Verify the results
}
```

#### Fluent Assertions
Tests use FluentAssertions for readable and expressive assertions:
```csharp
result.Should().NotBeNull();
result.Should().Contain("expected value");
result.Should().HaveCount(5);
```

#### Test Data Management
- Test files are created in temporary directories
- Cleanup is performed in `[TestCleanup]` methods
- Test data is isolated between test runs

### Coverage Areas

#### ExcelHelperService
- ✅ Reading Excel files with various formats
- ✅ Writing DataTables to Excel files
- ✅ Template generation
- ✅ Error handling for file access issues
- ✅ Special character and data type handling
- ✅ File locking scenarios

#### EmailHelperService
- ✅ HTML table generation from DataTables
- ✅ Custom formatting options
- ✅ Special character escaping
- ✅ Report email body generation
- ✅ Null value handling

#### MiscHelperService
- ✅ Maintenance time window checking
- ✅ Screenshot functionality
- ✅ Screen resolution detection
- ✅ Folder structure creation
- ✅ Various utility functions (email validation, string generation, etc.)

#### SwaggerCodeGeneratorService
- ✅ Swagger file parsing
- ✅ Code generation from Swagger definitions
- ✅ Integrated file management (directory creation, file writing, README generation)
- ✅ URL-based Swagger loading
- ✅ Configuration management
- ✅ Error handling for invalid Swagger content

#### BaseCodeGenerationService
- ✅ Code file generation
- ✅ Namespace and class generation
- ✅ Property and method generation
- ✅ Code formatting and indentation
- ✅ Identifier sanitization

### Test Configuration

#### Dependencies
- **MSTest**: Test framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework (if needed)
- **System.Drawing.Common**: For screenshot functionality
- **EPPlus**: Excel file operations (inherited from main project)

#### Test Settings
- Test run settings are configured in `test.runsettings`
- Code coverage is enabled
- Test results are logged in TRX format

### Running Tests

#### Visual Studio
1. Open Test Explorer
2. Build the solution
3. Run all tests or specific test classes

#### Command Line
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=ExcelHelperServiceTests"

# Run tests with detailed output
dotnet test --logger:console --verbosity normal
```

### Best Practices

#### Test Isolation
- Each test is independent and can run in any order
- Tests clean up after themselves
- No shared state between tests

#### Error Testing
- Tests verify both success and failure scenarios
- Exception types and messages are validated
- Logging behavior is verified where applicable

#### Performance Considerations
- Tests avoid unnecessary file I/O where possible
- Large file operations are tested with reasonable sizes
- Timeout scenarios are handled appropriately

#### Maintainability
- Tests are well-named and self-documenting
- Helper methods reduce code duplication
- Test data is easy to understand and modify

### Future Enhancements

- Add performance benchmarking tests
- Add more integration tests with external services
- Add property-based testing for complex scenarios
- Add mutation testing to verify test quality