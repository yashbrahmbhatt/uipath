# Yash.Orchestrator

A utility library for interacting with UiPath Cloud Orchestrator APIs. Provides a reusable `OrchestratorService` class that simplifies access to folders, assets, storage buckets, and files — with support for both direct credential and Studio-integrated token authentication.

---

## 🚀 Getting Started

1. Add the `Yash.Orchestrator` NuGet package as a dependency in your UiPath plugin or library project.  
2. Create an instance of `OrchestratorService` using either client credentials or an `IAccessProvider`.  
3. Call `InitializeAsync()` to populate folders, assets, buckets, and bucket files.  
4. Use the service to read, inspect, or download resources in a structured and token-aware manner.

---

## 🛠️ Core Class: OrchestratorService

The `OrchestratorService` class is the main entry point. It provides high-level, folder-aware access to UiPath Cloud resources with built-in refresh and download methods.

```csharp
var service = new OrchestratorService("https://cloud.uipath.com/org/account", "clientId", "secret", log);
await service.InitializeAsync();

foreach (var (folder, assets) in service.Assets)
{
    Console.WriteLine($"{folder.DisplayName} has {assets.Count} assets");
}
```

---

## 🧙‍♂️ Features

- 🔐 Token authentication via:
  - Client credentials (client ID + secret)
  - `IAccessProvider` (from Studio integration context)

- 🗂️ Folder-aware access to:
  - Assets
  - Buckets
  - Bucket Files

- 🔁 Refresh methods to update all collections dynamically
- 🪝 Logs events using `TraceEventType` and customizable log actions
- 📦 File downloads via signed URI

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

| Method                      | Description                                              |
|-----------------------------|----------------------------------------------------------|
| `InitializeAsync()`         | Initializes and refreshes all data                       |
| `UpdateTokenAsync(force)`   | Updates the token using either credentials or provider   |
| `RefreshFoldersAsync()`     | Fetches all available folders                            |
| `RefreshAssetsAsync()`      | Fetches assets scoped to each folder                    |
| `RefreshBucketsAsync()`     | Fetches storage buckets per folder                      |
| `RefreshBucketFilesAsync()` | Fetches files in each bucket                            |
| `DownloadBucketFile()`      | Downloads a file from storage buckets using a signed URI|

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

Supports two authentication modes:

| Mode                  | Details                                                       |
|-----------------------|---------------------------------------------------------------|
| Client Credentials    | Provide `BaseURL`, `ClientId`, and `ClientSecret`             |
| IAccessProvider       | Provide an implementation via UiPath Studio plugin context    |

---

## 📚 Dependencies

- [`RestSharp`](https://www.nuget.org/packages/RestSharp): HTTP communication  
- [`Newtonsoft.Json`](https://www.nuget.org/packages/Newtonsoft.Json): JSON deserialization  
- [`UiPath.Activities.Api`](https://www.nuget.org/packages/UiPath.Activities.Api): Optional for Studio plugin integration  

---

## 📖 License

This project is licensed under the MIT License.

---

## 🔗 Links

- GitHub Repository: [https://github.com/yashbrahmbhatt/Yash.Orchestrator](https://github.com/yashbrahmbhatt/Yash.Orchestrator)  
- NuGet Package: [https://www.nuget.org/packages/Yash.Orchestrator](https://www.nuget.org/packages/Yash.Orchestrator)

---

*Built and maintained by Yash Brahmbhatt*