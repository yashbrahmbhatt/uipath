# Yash.Config

A comprehensive configuration management solution for UiPath workflows, supporting Excel-based configurations, UiPath Orchestrator assets, cloud storage files, and programmatic access through coded workflows.

---

## 🚀 Getting Started

1. Add the `Yash.Config` NuGet package as a dependency in your UiPath project.  
2. Create an Excel configuration file with sheets for **Settings**, **Assets**, and **Files**.
3. Use the **LoadConfig** activity to load your configuration into workflows as a `Dictionary<string, object>`.  
4. For coded workflows, use the **ConfigService** programmatic API for direct access to configuration loading.
5. Run the **ConfigureSettingsWizard** to set up code generation paths and namespaces.  
6. Use the **GenerateConfigClassesWizard** to create strongly-typed C# configuration classes.

---

## 🛠️ Activities

### LoadConfig Activity
Loads an Excel configuration file containing three types of configuration sources:

- **Settings**: Direct key-value pairs from the Excel file
- **Assets**: References to UiPath Orchestrator assets  
- **Files**: Local files or files stored in UiPath Orchestrator storage buckets

**Input Parameters:**
- `WorkbookPath` (string): Path to the Excel configuration file
- `Scope` (string, optional): Filter configuration items by scope
- `BaseUrl` (string): UiPath Orchestrator base URL for asset/file access
- `ClientId` (string): OAuth client ID for Orchestrator authentication
- `ClientSecret` (SecureString): OAuth client secret for Orchestrator authentication

**Output:**
- `Dictionary<string, object>`: Configuration dictionary with all resolved values

**Supported File Types:**
- **CSV**: Parsed into JSON-serialized DataTable format
- **XLSX**: Parsed into JSON-serialized DataSet format  
- **Text files**: Raw string content

---

## 💻 ConfigService - Programmatic API

For **coded workflows** and custom activities, the `ConfigService` provides direct programmatic access to configuration loading without requiring UiPath activities.

### Key Methods

#### `LoadConfigAsync()`
```csharp
public static async Task<Dictionary<string, object>> LoadConfigAsync(
    ConfigFile configFile, 
    string scope, 
    string baseUrl, 
    string clientId, 
    SecureString clientSecret, 
    Action<string, TraceEventType>? log = null
)
```

Asynchronously loads configuration from a parsed Excel file, resolving Orchestrator assets and downloading remote files.

#### `ReadConfigFile()`
```csharp
public static ConfigFile ReadConfigFile(
    string filePath, 
    Action<string, TraceEventType>? log = null
)
```

Parses an Excel configuration file into a structured `ConfigFile` object containing settings, assets, and file references.

#### `GenerateClassString()`
```csharp
public static string GenerateClassString(
    string excelPath,
    string outputClassName,
    string outputFolder,
    string namespaceName,
    string scope = "",
    string additionalUsings = "",
    Action<string, TraceEventType>? log = null
)
```

Generates C# class code for strongly-typed configuration access based on Excel configuration structure.

### Usage Example
```csharp
// In coded workflows or custom activities
using Yash.Config;
using System.Security;

// Load configuration programmatically
var configFile = ConfigService.ReadConfigFile(@"C:\path\to\config.xlsx");
var config = await ConfigService.LoadConfigAsync(
    configFile, 
    "Production", 
    "https://orchestrator.company.com", 
    "your-client-id",
    clientSecret
);

// Access configuration values
string connectionString = (string)config["DatabaseConnection"];
int timeout = (int)config["ApiTimeout"];
```

---

## 📋 Excel Configuration File Structure

This Excel file serves as a centralized configuration source for automation workflows. It supports reading and generating strongly-typed C# classes containing settings, assets, and file references defined in the sheets below.

Each row defines a configuration item with metadata about its source and type. The C# generator tool reads these entries and produces an auto-generated class where each configuration Name becomes a typed property or field.

### Settings Sheet
Defines simple key-value settings used in your automation.

| Column | Description |
|--------|-------------|
| **Name** | Property name in the generated class |
| **Value** | Default or initial value |
| **Type** | .NET data type (e.g., string, int, bool) |
| **Scope** | The scope of the setting when being loaded |
| **Description** | Optional description or comment |

### Assets Sheet  
Used to reference Orchestrator assets by name and folder.

| Column | Description |
|--------|-------------|
| **Name** | Property name in the generated class |
| **Value** | Orchestrator asset name |
| **Folder** | Orchestrator folder name (optional) |
| **Type** | .NET data type (e.g., string, int, bool) |
| **Scope** | The scope of the asset when being loaded |
| **Description** | Notes about usage or where it's consumed |

### Files Sheet
Defines file paths used by the automation, supporting both local and Orchestrator Storage Bucket locations.

| Column | Description |
|--------|-------------|
| **Name** | Property name in the generated class |
| **Path** | Path to the file (local path or bucket-relative path) |
| **Folder** | Storage Bucket folder (leave empty for local files) |
| **Bucket** | Storage Bucket name (leave empty for local files) |
| **FileType** | Must be one of: csv, xlsx, txt |
| **Type** | .NET data type (e.g., string, int, bool) |
| **Scope** | The scope of the asset when being loaded |
| **Description** | Purpose or format of the file |

### Guidelines
- **Avoid duplicate Name entries** across all sheets
- **Ensure Type values** are valid .NET types or predefined categories
- **Use descriptive Description fields** to aid in documentation and maintainability
- **For Files**, either:
  - Provide a local path, and leave Folder and Bucket empty, OR
  - Provide Bucket, Folder, and Path, and leave local path blank

### Supported Data Types & Examples

| Type | Example Input | Notes |
|------|---------------|-------|
| `string` | `hello world` | No transformation; returned as-is |
| `int` | `42` | Must be a valid integer |
| `double` | `3.14` | Accepts decimal format; culture-dependent parsing may apply |
| `bool` | `true or false` | Case-insensitive in bool.Parse |
| `DateTime` | `2024-12-31T23:59:59` | Uses DateTime.Parse – ISO 8601 recommended |
| `TimeSpan` | `1:30:00` | Format: hh:mm:ss or other TimeSpan.Parse-compatible |
| `List<string>` | `apple,banana,carrot` | No trimming; strings parsed as-is |
| `List<int>` | `1,2,3` | Each item must be an integer |
| `List<double>` | `1.1,2.2,3.3` | Each item must be a double |
| `List<bool>` | `true,false,true` | Case-insensitive |
| `List<DateTime>` | `2024-01-01,2024-12-31` | Acceptable DateTime.Parse formats |
| `List<TimeSpan>` | `00:30:00,01:00:00` | Must follow TimeSpan.Parse format |
| `string[]` | `foo,bar,baz` | Same behavior as List<string> |
| `int[]` | `10,20,30` | Same behavior as List<int> |
| `double[]` | `0.1,0.2,0.3` | Same behavior as List<double> |
| `bool[]` | `false,true,false` | Same behavior as List<bool> |
| `DateTime[]` | `2024-01-01T00:00:00,2025-01-01` | Same behavior as List<DateTime> |
| `TimeSpan[]` | `00:10:00,01:15:00` | Same behavior as List<TimeSpan> |

---

## 🧙‍♂️ Wizards

- **ConfigureSettingsWizard**  
  Helps you configure essential settings for the wizards and activities, including file paths and output directories. It prompts you if any required setting is missing or needs to be updated.

- **GenerateConfigClassesWizard**  
  Generates strongly-typed C# configuration classes from your Excel config file, based on your specified namespace, class name, and additional using directives. This makes working with configuration data type-safe and easier to maintain.

---

## ⚙️ Settings

The package includes a set of configurable settings to control the behavior of the wizards and code generation:

| Setting Key                                 | Label               | Description                                         | Validation                                         |
|---------------------------------------------|---------------------|-----------------------------------------------------|----------------------------------------------------|
| `Yash.Config.AutoGeneration.FilePath`       | Config File Path    | The Excel file path used to generate config classes.| Must be a valid existing file path.                 |
| `Yash.Config.AutoGeneration.OutputDirectory`| Output Directory    | The folder where generated config classes are saved.| Must be a valid existing directory.                 |
| `Yash.Config.AutoGeneration.ClassName`      | Class Name          | The name of the generated C# class.                 | Must start with a letter and not be empty.          |
| `Yash.Config.AutoGeneration.Namespace`      | Namespace           | Namespace for the generated C# class.               | Must not be empty.                                   |
| `Yash.Config.AutoGeneration.AdditionalUsings`| Additional Usings  | Comma-separated list of additional using directives.| Optional, default is none.                           |

---

## 🏗️ Architecture & Features

### Scope-Based Configuration
- Filter configuration items by scope (e.g., "Development", "Production", "Test")
- Generate separate configuration classes for each scope
- Runtime scope filtering for multi-environment deployments

### Orchestrator Integration
- **Assets**: Seamlessly access UiPath Orchestrator assets with folder-based organization
- **Storage Buckets**: Download and process files from Orchestrator cloud storage
- **OAuth Authentication**: Secure authentication using client credentials flow

### File Processing
- **CSV Files**: Automatically parsed into structured DataTable format
- **Excel Files**: Multi-sheet support with DataSet serialization
- **Text Files**: Raw content access for configuration templates, JSON, XML, etc.

### Code Generation
- **Strongly-Typed Classes**: Generate C# classes with property validation
- **Multi-Scope Support**: Separate classes per scope for environment-specific configurations  
- **Type Safety**: Compile-time checking with proper C# type annotations
- **XML Documentation**: Auto-generated documentation comments from descriptions

---

## 📚 Core Models and Type Parsing

### ConfigFile Model
The `ConfigFile` class represents the parsed Excel configuration structure:
- **Settings**: `List<ConfigSettingItem>` - Direct key-value configuration pairs
- **Assets**: `List<ConfigAssetItem>` - References to Orchestrator assets  
- **Files**: `List<ConfigFileItem>` - Local or remote file references

### Config Class
- A dynamic dictionary-like class that supports property-based access and serialization.  
- Supports JSON serialization and binary serialization (via `[Serializable]` and `ISerializable`).  
- Provides runtime-safe dynamic access to config keys as properties, throwing exceptions for missing keys to help catch errors early.

### Type Parsers
The package includes a comprehensive set of type parsers that convert string representations into various primitive and collection types for seamless deserialization.

**Supported Primitive Types:**
- `string`, `int`, `double`, `bool`, `DateTime`, `TimeSpan`

**Supported Collection Types:**
- `List<T>` and `T[]` for all supported primitives (e.g., `List<string>`, `int[]`, `List<DateTime>`)

### Data Type Parsing Rules
- **Primitive types:**  
  - **Integers and doubles**: Follow standard string representations (e.g., `"42"`, `"3.14"`)
  - **Booleans**: Case-insensitive parsing (`"true"`, `"false"`)
  - **DateTime**: ISO 8601 format recommended (e.g., `"2024-12-31T23:59:59"`)
  - **TimeSpan**: Standard format (e.g., `"01:30:00"` for 1 hour 30 minutes)
  - **Strings**: No transformation; returned as-is

- **Collections (Lists and Arrays):**  
  - Parsed from comma-separated strings **without spaces**
  - Examples:
    - `"apple,banana,carrot"` → `List<string>` or `string[]`
    - `"1,2,3,4"` → `List<int>` or `int[]`
    - `"true,false,true"` → `List<bool>` or `bool[]`
    - `"2024-01-01,2024-12-31"` → `List<DateTime>` or `DateTime[]`
  - Empty strings are parsed as empty collections
  - Each item in the collection must be valid for the specified type  

### ConfigFactory
- Provides a method to create strongly typed config class instances from a raw dictionary by mapping and parsing dictionary entries to properties, ensuring proper types and validation.

---

## 🚀 Quick Start Examples

### Basic Configuration Loading
```csharp
// Using the LoadConfig activity in a workflow
var config = await LoadConfigActivity.Execute(
    workbookPath: @"C:\Config\AppConfig.xlsx",
    scope: "Production",
    baseUrl: "https://orchestrator.company.com",
    clientId: "your-client-id",
    clientSecret: secureClientSecret
);

// Access configuration values
string apiEndpoint = (string)config["ApiEndpoint"];
int retryCount = (int)config["RetryCount"];
```

### Programmatic Access (Coded Workflows)
```csharp
using Yash.Config;

// Load and parse configuration file
var configFile = ConfigService.ReadConfigFile(@"C:\Config\AppConfig.xlsx");

// Load with scope filtering and Orchestrator integration
var config = await ConfigService.LoadConfigAsync(
    configFile, 
    scope: "Production",
    baseUrl: "https://orchestrator.company.com",
    clientId: "your-client-id", 
    clientSecret: clientSecret
);

// Access different types of configuration
string dbConnection = (string)config["DatabaseConnection"]; // From Settings
string apiKey = (string)config["ExternalApiKey"]; // From Orchestrator Assets
string csvData = (string)config["ReferenceData"]; // From processed CSV file
```

### Strongly-Typed Configuration Classes
```csharp
// Generated configuration class (auto-generated by wizard)
public class ProductionConfig : Yash.Config.Models.Config
{
    /// <summary>
    /// Database connection string for production environment
    /// </summary>
    public string DatabaseConnection { get; set; }
    
    /// <summary>
    /// API timeout in seconds
    /// </summary>
    public int ApiTimeout { get; set; }
    
    /// <summary>
    /// External API key from Orchestrator assets
    /// </summary>
    public string ExternalApiKey { get; set; }
}

// Usage
var typedConfig = ConfigFactory.Create<ProductionConfig>(configDictionary);
string connection = typedConfig.DatabaseConnection; // Strongly typed access
```

---

## � Best Practices

### Configuration File Organization
- **Separate environments by scope**: Use scope-based filtering for Development/Test/Production configurations
- **Descriptive naming**: Use clear, descriptive names for configuration keys
- **Type annotations**: Always specify the `Type` column for code generation and validation
- **Documentation**: Fill in the `Description` column for better maintainability

### Security Considerations  
- **Asset storage**: Store sensitive data (API keys, passwords) as Orchestrator assets rather than direct values
- **Secure authentication**: Use OAuth client credentials for Orchestrator access
- **File permissions**: Ensure proper access controls on configuration files

### Performance Optimization
- **Scope filtering**: Use scope filtering to load only necessary configuration items
- **Caching**: Consider caching configuration data in long-running processes
- **Async operations**: Use `ConfigService.LoadConfigAsync()` for non-blocking configuration loading

---

## �📖 License

This project is licensed under the MIT License.

---

## 🔗 Links

- **Repository**: [https://github.com/yashbrahmbhatt/uipath](https://github.com/yashbrahmbhatt/uipath)  
- **Issues & Support**: Submit issues through the repository's issue tracker

---

## 🏷️ Version History

See the repository's release notes for detailed version history and breaking changes.

---

*Built and maintained by Yash Brahmbhatt.*  
