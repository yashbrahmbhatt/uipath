# AI Coding Agent Instructions for Yash's UiPath Monorepo

## 🏗️ Repository Architecture

This is a sophisticated **hybrid monorepo** combining three distinct but interconnected technology stacks:

- **`vs-libraries/`**: .NET 6+ class libraries (NuGet packages) for UiPath custom activities and utilities
- **`uipath-libraries/`**: UiPath library packages (.nupkg) with reusable workflow components  
- **`uipath-processes/`**: Complete UiPath automation processes following REFramework patterns

### Key Structural Pattern
The codebase follows a **dependency hierarchy** where UiPath components consume .NET libraries as custom activities. The `mono.json` file defines build order and dependencies—always check this file to understand project relationships.

## 🔧 Critical Development Workflows

### Building and Testing
```bash
# Build single project (use project ID from mono.json)
dotnet build vs-libraries/Yash.Utility

# Run specific project tests
dotnet test vs-libraries/Yash.Utility.Tests

# Build all .NET projects in dependency order
dotnet build vs-libraries/Yash.sln
```

**Important**: Never run UiPath CLI commands directly—the monorepo uses custom GitHub Actions that handle UiPath builds via `mono.json` configuration.

### Version Management
Projects use **automatic semantic versioning** via `Directory.Build.props`:
- Debug builds: `1.0.{dayOfYear}.{secondsInDay}` (allows multiple daily builds)
- Release builds: `1.0.{dayOfYear}` (one per day)
- Packages are auto-generated in `vs-libraries/Output/` on build

### Dependency Resolution
The codebase uses **Central Package Management** (`Directory.Packages.props`). When adding dependencies:
1. Add version to `<PackageVersion>` in `Directory.Packages.props`
2. Add reference without version in project file: `<PackageReference Include="PackageName" />`

## 🎯 Project-Specific Patterns

### .NET Libraries (`vs-libraries/`)
**Service-based architecture** with dependency injection-ready patterns:
```csharp
// Standard service constructor pattern used throughout
public ExcelHelperService(Action<string, TraceEventType>? logger = null)
{
    _logger = logger ?? ((_, _) => { });
}
```

**Key Conventions**:
- All services accept optional logger delegates
- Use `FluentAssertions` and `Moq` for testing
- `README.md` files include comprehensive usage examples
- Test classes inherit from `TestBase` classes for setup/teardown

### UiPath Libraries (`uipath-libraries/`)
**REFramework-based architecture** with coded workflows:
```csharp
// Standard inheritance pattern for all workflows
public class CustomDispatcher : DispatcherWorkflow
{
    public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
    public override string[] ConfigScopes { get; set; } = { "Shared", "Dispatcher" };
}
```

**Critical Pattern**: All UiPath workflows inherit from abstract base classes:
- `DispatcherWorkflow` → `TestableCodedWorkflow` → `CodedWorkflowWithConfig` → `CodedWorkflow`
- Configuration is **always external** via Excel files or Orchestrator assets
- Tests follow naming: `{Component}_{TestScenario}` (e.g., `Dispatcher_Success`)

### Testing Framework
**Unified testing approach** across all project types:
- .NET: MSTest with FluentAssertions
- UiPath: Custom testing framework built into base workflow classes
- All tests support expected exception validation
- Test methods include lifecycle: Initialize → Execute → Validate → Cleanup

## 🚀 CI/CD Integration

### Smart Build Detection
The repository uses **intelligent change detection** via `.github/actions/build-matrix/`:
- Only builds projects affected by code changes
- Respects dependency order defined in `mono.json`
- Handles cross-project dependencies automatically

### Deployment Pipeline
Projects deploy to **multiple targets** based on `mono.json` configuration:
- `.NET libraries` → NuGet.org + GitHub Packages
- `UiPath libraries/processes` → UiPath Orchestrator
- Self-hosted Windows runners handle UiPath-specific tooling

**Key Command**: Check `mono.json` for project deployment configuration before modifying CI/CD behavior.

## 🔍 Configuration Patterns

### .NET Configuration
- Global settings: `Directory.Build.props` (versioning, MSBuild)
- Dependencies: `Directory.Packages.props` (centralized package management)
- Per-project: Individual `.csproj` files (minimal, inherit globals)

### UiPath Configuration
- **Excel-based config**: `Data/Config.xlsx` with separate sheets per component
- **Code-generated config classes**: `Configs/` folder contains strongly-typed models
- **Environment-agnostic**: Configuration supports dev/test/prod via external files

### Orchestrator Integration
UiPath projects expect these **environment variables** for testing:
- `UIP_ACCOUNT_NAME`, `UIP_APPLICATION_ID`, `UIP_APPLICATION_SECRET`, `UIP_TENANT_NAME`

## ⚡ Quick Reference Commands

```bash
# Check which projects would build (without building)
node .github/actions/build-matrix/index.js

# Build specific dependency chain
dotnet build vs-libraries/Yash.Config  # Builds dependencies first

# Run all .NET tests
dotnet test vs-libraries/

# Generate packages only
dotnet pack vs-libraries/ --configuration Release
```

## 🎯 Common Tasks

### Adding New .NET Library
1. Create project in `vs-libraries/`
2. Add to `Yash.sln`
3. Update `mono.json` with build/test/deploy settings
4. Add `icon.png` and comprehensive `README.md`

### Adding New UiPath Component
1. Create in appropriate `uipath-*` folder
2. Inherit from correct base class (`DispatcherWorkflow`, `PerformerWorkflow`, etc.)
3. Add to `mono.json` with dependencies
4. Create test classes for all scenarios

### Debugging Build Issues
1. Check `mono.json` for correct project paths
2. Verify dependency order in build matrix
3. Ensure all PackageReferences have versions in `Directory.Packages.props`
4. For UiPath: Validate `project.json` entry points match coded workflow files

**Remember**: This monorepo prioritizes **intelligent automation** over manual processes. Always leverage the existing CI/CD infrastructure and configuration patterns rather than creating one-off solutions.