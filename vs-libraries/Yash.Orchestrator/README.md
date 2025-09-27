# Yash.Orchestrator

A utility library for interacting with UiPath Cloud Orchestrator APIs. Provides a reusable `OrchestratorService` class that simplifies access to folders, assets, storage buckets, and files — with support for both direct credential and Studio-integrated token authentication.

## ✨ Key Features

- 🔗 **Interface-Based Design**: Implements `IOrchestratorService` for dependency injection and testability
- 🔐 **Dual Authentication**: Client credentials or Studio integration via `IAccessProvider`
- 📊 **Observable Collections**: Real-time data binding support for UI applications
- 🔄 **Smart Token Management**: Automatic refresh with configurable force refresh
- 🪝 **Comprehensive Logging**: Integrated logging with customizable log actions
- 📁 **Folder-Aware Access**: Organize resources by UiPath Orchestrator folders
- 📦 **File Downloads**: Direct download from storage buckets with signed URIs
- ✅ **Fully Tested**: 18 comprehensive tests covering all functionality

---

## 🚀 Getting Started

### Quick Start Example
```csharp
// Using interface for dependency injection
IOrchestratorService service = new OrchestratorService(
    "https://cloud.uipath.com/org/account", 
    "clientId", 
    "secret", 
    log
);

await service.InitializeAsync();

foreach (var (folder, assets) in service.Assets)
{
    Console.WriteLine($"{folder.DisplayName} has {assets.Count} assets");
}
```

### Installation
1. Add the `Yash.Orchestrator` NuGet package to your project
2. Create an instance using client credentials or an `IAccessProvider`
3. Call `InitializeAsync()` to populate all collections
4. Access folders, assets, buckets, and files through organized collections

---

## 🏗️ Architecture

### Interface Design
The library is built around the `IOrchestratorService` interface, enabling:
- **Dependency Injection**: Easy integration with DI containers
- **Unit Testing**: Mock the interface for isolated testing
- **Extensibility**: Implement custom orchestrator services
- **SOLID Principles**: Interface segregation and dependency inversion

### Class Hierarchy
```
IOrchestratorService (Interface)
└── OrchestratorService (Implementation)
    ├── Token Management
    ├── API Communication
    ├── Collection Management
    └── File Operations
```

---

## 🛠️ Core Interface: IOrchestratorService

The `IOrchestratorService` interface defines the contract for UiPath Orchestrator interactions:

```csharp
public interface IOrchestratorService
{
    // Authentication & Configuration
    string? BaseURL { get; }
    string Token { get; }
    Action<string, TraceEventType> LogAction { get; }

    // Collections
    ObservableCollection<Folder> Folders { get; }
    ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>> Assets { get; }
    ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>> Buckets { get; }
    ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>> BucketFiles { get; }

    // Operations
    Task InitializeAsync();
    Task UpdateTokenAsync(bool force = false);
    Task RefreshFoldersAsync();
    Task RefreshAssetsAsync();
    Task RefreshBucketsAsync();
    Task RefreshBucketFilesAsync();
    Task DownloadBucketFile(Bucket bucket, BucketFile bucketFile, string filePath);
}
```

### Implementation: OrchestratorService

The `OrchestratorService` class provides the concrete implementation with enhanced features:
- ✅ Smart token caching and refresh logic
- ✅ Comprehensive error handling and logging
- ✅ Observable collections for data binding
- ✅ Folder-aware resource organization

```csharp
// Client Credentials Authentication
var service = new OrchestratorService("https://cloud.uipath.com/org/account", "clientId", "secret", log);

// IAccessProvider Authentication (Studio Integration)
var service = new OrchestratorService(accessProvider, log);

await service.InitializeAsync();
```

---

## 🧙‍♂️ Features

- 🔐 **Authentication Methods**:
  - Client credentials (client ID + secret)
  - `IAccessProvider` (from Studio integration context)
  - Smart token caching with configurable force refresh

- 🗂️ **Folder-Aware Resource Access**:
  - Assets organized by folder
  - Storage buckets per folder
  - Files within each bucket
  - Observable collections for real-time updates

- � **Dynamic Operations**:
  - Refresh methods to update all collections
  - Token management with automatic refresh
  - File downloads via signed URIs
  - Comprehensive error handling

- 🪝 **Logging & Monitoring**:
  - Integrated logging with `TraceEventType`
  - Customizable log actions
  - Detailed operation tracking
  - Authentication event logging

- 🏗️ **Developer Experience**:
  - Interface-based design for DI and testing
  - Observable collections for UI data binding
  - Comprehensive XML documentation
  - Full test coverage (18 tests)

---

## ⚙️ Public Properties

| Property      | Type                                                | Description                            |
|---------------|-----------------------------------------------------|----------------------------------------|
| `BaseURL`     | `string?`                                           | Base URL for Orchestrator API          |
| `Token`       | `string`                                            | OAuth2 access token                    |
| `Folders`     | `ObservableCollection<Folder>`                      | List of accessible folders             |
| `Assets`      | `ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>>` | Map of folder → assets |
| `Buckets`     | `ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>>` | Map of folder → buckets |
| `BucketFiles` | `ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>>` | Map of bucket → files |

---

## 🔁 Methods

| Method                      | Description                                              | Returns |
|-----------------------------|----------------------------------------------------------|---------|
| `InitializeAsync()`         | Initializes and refreshes all data collections          | `Task` |
| `UpdateTokenAsync(force)`   | Updates the token using credentials or provider         | `Task` |
| `RefreshFoldersAsync()`     | Fetches all available folders from Orchestrator         | `Task` |
| `RefreshAssetsAsync()`      | Fetches assets scoped to each folder                    | `Task` |
| `RefreshBucketsAsync()`     | Fetches storage buckets per folder                      | `Task` |
| `RefreshBucketFilesAsync()` | Fetches files in each bucket                            | `Task` |
| `DownloadBucketFile()`      | Downloads a file using signed URI and saves to disk     | `Task` |

### Advanced Usage Examples

```csharp
// Force token refresh
await service.UpdateTokenAsync(force: true);

// Refresh specific collections
await service.RefreshAssetsAsync();
await service.RefreshBucketsAsync();

// Download with error handling
try 
{
    await service.DownloadBucketFile(bucket, file, @"C:\Downloads\file.csv");
    Console.WriteLine("Download completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Download failed: {ex.Message}");
}
```

---

## 📦 Downloading Files

```csharp
await service.DownloadBucketFile(bucket, bucketFile, "C:\\Downloads\\MyFile.csv");
```

This method:
- Fetches a signed download URI from the Orchestrator API
- Downloads the file using a `GET` request
- Saves the file to the given path, creating directories if necessary

---

## 🧠 Authentication

### Client Credentials Mode
```csharp
var service = new OrchestratorService(
    baseUrl: "https://cloud.uipath.com/org/account",
    clientId: "your-client-id",
    clientSecret: "your-client-secret",
    logger: (message, level) => Console.WriteLine($"[{level}] {message}")
);
```

### IAccessProvider Mode (Studio Integration)
```csharp
// In a UiPath custom activity
var service = new OrchestratorService(
    accessProvider: context.GetExtension<IAccessProvider>(),
    logger: (message, level) => Console.WriteLine($"[{level}] {message}")
);
```

### Authentication Features
- ✅ **Token Caching**: Tokens are cached and reused until expiration
- ✅ **Smart Refresh**: Only refreshes when necessary or when forced
- ✅ **Error Handling**: Comprehensive error handling for auth failures
- ✅ **Logging**: All authentication events are logged with appropriate levels

| Mode                  | Use Case                                                 | Requirements |
|-----------------------|----------------------------------------------------------|--------------|
| Client Credentials    | Standalone applications, custom services                 | Client ID, Secret, Base URL |
| IAccessProvider       | UiPath Studio custom activities, plugins                 | Studio context with IAccessProvider |

---

## 🧪 Testing

The library includes a comprehensive test suite with 18 tests covering:

### Test Coverage
- ✅ **Interface Implementation**: Validates IOrchestratorService contract
- ✅ **Authentication Flows**: Tests both credential and provider modes  
- ✅ **Token Management**: Validates caching and refresh logic
- ✅ **Collection Operations**: Tests observable collection behavior
- ✅ **Error Handling**: Validates exception scenarios
- ✅ **Logging Integration**: Tests logging functionality

### Running Tests
```bash
# Run all tests
dotnet test Yash.Orchestrator.Tests

# Run with detailed output
dotnet test Yash.Orchestrator.Tests --verbosity normal

# Current status: 18/18 tests passing
```

### Test Results
```
Test summary: total: 18, failed: 0, succeeded: 18, skipped: 0
```

For detailed testing information, see [Yash.Orchestrator.Tests README](../Yash.Orchestrator.Tests/README.md).

---

## 📚 Dependencies

### Runtime Dependencies
- [`RestSharp`](https://www.nuget.org/packages/RestSharp): HTTP communication with UiPath APIs
- [`Newtonsoft.Json`](https://www.nuget.org/packages/Newtonsoft.Json): JSON serialization/deserialization
- [`UiPath.Activities.Api`](https://www.nuget.org/packages/UiPath.Activities.Api): Studio integration support

### Development Dependencies  
- [`Microsoft.NET.Test.Sdk`](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk): Test framework support
- [`MSTest.TestFramework`](https://www.nuget.org/packages/MSTest.TestFramework): Unit testing framework
- [`FluentAssertions`](https://www.nuget.org/packages/FluentAssertions): Readable test assertions
- [`Moq`](https://www.nuget.org/packages/Moq): Mock object framework

### Framework Support
- **.NET 6.0+**: Target framework with Windows 7.0 compatibility
- **Observable Collections**: Full data binding support for WPF/WinUI applications
- **Async/Await**: Modern asynchronous programming patterns

---

## 📖 License

This project is licensed under the MIT License.

---

## 🔗 Links

- GitHub Repository: [https://github.com/yashbrahmbhatt/Yash.Orchestrator](https://github.com/yashbrahmbhatt/Yash.Orchestrator)  
- NuGet Package: [https://www.nuget.org/packages/Yash.Orchestrator](https://www.nuget.org/packages/Yash.Orchestrator)

---

*Built and maintained by Yash Brahmbhatt.*