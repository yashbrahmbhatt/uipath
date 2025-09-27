# Yash.Utility

[![NuGet Version](https://img.shields.io/nuget/v/Yash.Utility.svg)](https://www.nuget.org/packages/Yash.Utility/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/yashbrahmbhatt/uipath/blob/main/LICENSE)

A comprehensive C# utility library providing reusable services for Excel operations, email templating, file management, code generation, and more. Designed for UiPath automation projects but suitable for any .NET application.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Services Overview](#services-overview)
- [Detailed Usage](#detailed-usage)
- [Models and Configuration](#models-and-configuration)
- [Dependencies](#dependencies)
- [Contributing](#contributing)
- [License](#license)

## Features

🎯 **Core Services**
- **Excel Operations**: Read/write Excel files with DataTables/DataSets
- **Email Templating**: Generate HTML tables and formatted email bodies
- **File Management**: Advanced file operations with backup and retry logic
- **Code Generation**: Generate C# code from models programmatically
- **Swagger Integration**: Generate code from Swagger/OpenAPI specifications
- **Miscellaneous Utilities**: Screenshots, maintenance windows, validation

🔧 **Key Capabilities**
- ✅ Robust error handling and logging
- ✅ Excel file operations with lock handling
- ✅ HTML email template generation
- ✅ Swagger/OpenAPI code generation
- ✅ File backup and management
- ✅ Screenshot capture functionality
- ✅ Maintenance time window validation
- ✅ Type-safe configuration models

## Installation

### NuGet Package Manager
```powershell
Install-Package Yash.Utility
```

### .NET CLI
```bash
dotnet add package Yash.Utility
```

### PackageReference
```xml
<PackageReference Include="Yash.Utility" Version="1.0.0" />
```

## Quick Start

```csharp
using Yash.Utility.Services;
using System.Data;
using System.Diagnostics;

// Initialize services with optional logging
Action<string, TraceEventType> logger = (msg, level) => Console.WriteLine($"[{level}] {msg}");

// Excel operations
var excelService = new ExcelHelperService(logger);
var dataSet = excelService.ReadExcelFile("data.xlsx");
excelService.CreateExcelFile("output.xlsx", dataSet);

// Email templating
var emailService = new EmailHelperService(logger);
var htmlTable = emailService.ToHtmlTable(dataSet.Tables[0]);

// Screenshot capture
var miscService = new MiscHelperService(logger);
var screenshotResult = miscService.TakeScreenshot(new ScreenshotSettings 
{ 
    FilePath = "screenshot.png",
    Format = ScreenshotFormat.PNG 
});
```

## Services Overview

### ExcelHelperService
Advanced Excel file operations with robust error handling and data type preservation.

**Key Features:**
- Read Excel files to DataSet/DataTable
- Write DataSet/DataTable to Excel files
- Handle locked files with retry mechanism
- Generate Excel templates with validation
- Support for various data types (DateTime, numeric, boolean)

### EmailHelperService
Generate formatted HTML content for email templates and reports.

**Key Features:**
- Convert DataTables to HTML tables
- Customizable formatting options
- HTML escaping and special character handling
- Report email body generation
- Duration and statistics formatting

### SwaggerCodeGeneratorService
Generate C# code from Swagger/OpenAPI specifications (includes integrated file management).

**Key Features:**
- Parse Swagger JSON/YAML files
- Generate models and service classes
- Support for Swagger 2.0 and OpenAPI 3.0
- HTTP client integration
- Customizable code generation templates
- Integrated directory creation and file management
- README and usage example generation

### BaseCodeGenerationService
Abstract base class for generating C# code programmatically.

**Key Features:**
- Generate complete C# files from models
- Support for namespaces, classes, properties, methods
- Code formatting and indentation control
- Identifier sanitization
- Support for inheritance, interfaces, and modifiers

### MiscHelperService
Collection of miscellaneous utility functions.

**Key Features:**
- Maintenance time window validation
- Screenshot capture functionality
- Screen resolution detection
- Email validation
- Random string generation
- File size formatting

## Detailed Usage

### Excel Operations

#### Reading Excel Files
```csharp
var excelService = new ExcelHelperService();

// Read entire workbook
DataSet dataSet = excelService.ReadExcelFile("workbook.xlsx");

// Access individual sheets
DataTable sheet1 = dataSet.Tables["Sheet1"];
DataTable sheet2 = dataSet.Tables[0]; // By index
```

#### Writing Excel Files
```csharp
// From DataSet (multiple sheets)
excelService.CreateExcelFile("output.xlsx", dataSet);

// From single DataTable
DataTable table = CreateSampleData();
excelService.CreateExcelFile("single-sheet.xlsx", table, "MyData");

// Generate template with validation
string[] types = { "string", "int", "bool", "DateTime" };
excelService.GenerateExcelTemplate("template.xlsx", types);
```

### Email Templates

```csharp
var emailService = new EmailHelperService();
var dataTable = GetReportData();

// Basic HTML table
string htmlTable = emailService.ToHtmlTable(dataTable);

// Custom formatting
var options = new DataTableToHTMLOptions
{
    TablePrefix = "<table class='report-table'>",
    HeaderCellPrefix = "<th class='header'>",
    CellPrefix = "<td class='data'>"
};
string customTable = emailService.ToHtmlTable(dataTable, options);

// Report email body
var templateData = new EmailTemplateData
{
    ProcessName = "Daily Report",
    Environment = "Production",
    Status = "Completed",
    StartTime = DateTime.Now.AddHours(-2),
    EndTime = DateTime.Now
};

var statistics = new ReportStatistics
{
    TotalItems = 100,
    ProcessedItems = 95,
    FailedItems = 5,
    SuccessRate = 95.0
};

string emailBody = emailService.GenerateReportEmailBody(templateData, statistics, dataTable);
```

### Swagger Code Generation

```csharp
using var httpClient = new HttpClient();
var swaggerService = new SwaggerCodeGeneratorService(httpClient, logger);

// Generate from file (includes automatic directory creation, README, and usage examples)
string summary = await swaggerService.GenerateFromSwaggerFileAsync(
    "./swagger.json",
    "./generated",
    "MyApp.ApiClient",
    "ApiService"
);

// From URL
string summary = await swaggerService.GenerateFromSwaggerUrlAsync(
    "https://api.example.com/swagger.json",
    "./generated",
    "MyApp.ApiClient",
    "ApiService"
);

// From file
string summary2 = await swaggerService.GenerateFromSwaggerFileAsync(
    "swagger.json",
    "./generated",
    "MyApp.ApiClient",
    "ApiService"
);

// Analyze Swagger content
var analysis = await swaggerService.AnalyzeSwaggerAsync(swaggerJson);
Console.WriteLine($"Found {analysis.ModelsFound} models and {analysis.EndpointsFound} endpoints");
```

### Miscellaneous Utilities

```csharp
var miscService = new MiscHelperService();

// Maintenance window check
bool inMaintenance = miscService.IsMaintenanceTime(
    new TimeSpan(2, 0, 0),  // 2:00 AM
    new TimeSpan(6, 0, 0)   // 6:00 AM
);

// Screenshot
var screenshotSettings = new ScreenshotSettings
{
    FilePath = "screenshot.png",
    Format = ScreenshotFormat.PNG,
    Quality = 90
};
var result = miscService.TakeScreenshot(screenshotSettings);

// Utilities
bool isValidEmail = miscService.IsValidEmail("user@example.com");
string randomString = miscService.GenerateRandomString(20);
string fileSize = miscService.ConvertBytesToHumanReadable(1048576); // "1.00 MB"
```

## Models and Configuration

The library includes comprehensive model classes organized by functionality:

### CodeGeneration Models
- `CodeGenerationFile` - Represents a complete C# file
- `CodeGenerationNamespace` - Namespace definition
- `CodeGenerationClass` - Class definition with properties and methods
- `CodeGenerationProperty` - Property definition
- `CodeGenerationMethod` - Method definition

### Configuration Models
- `SwaggerConfiguration` - Settings for Swagger code generation
- `ExcelConfiguration` - Excel operation settings
- `ExcelTemplateSettings` - Template generation configuration

### Email Models
- `EmailTemplateData` - Email template variables
- `DataTableToHTMLOptions` - HTML table formatting options
- `ReportStatistics` - Report statistics for email templates

### Miscellaneous Models
- `ScreenshotSettings` - Screenshot capture configuration
- `MaintenanceConfiguration` - Maintenance window settings
- `FolderOperationSettings` - Folder creation settings

## Dependencies

- **.NET Standard 2.0+** - Compatible with .NET Framework 4.6.1+ and .NET Core 2.0+
- **EPPlus** - Excel file operations
- **RestSharp** - HTTP client operations
- **Newtonsoft.Json** - JSON serialization
- **System.Drawing.Common** - Screenshot functionality

## Error Handling

All services include comprehensive error handling and logging:

```csharp
// Services accept an optional logging action
Action<string, TraceEventType> logger = (message, level) =>
{
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}");
};

var service = new ExcelHelperService(logger);

try
{
    var data = service.ReadExcelFile("data.xlsx");
}
catch (FileNotFoundException ex)
{
    // Handle missing file
}
catch (InvalidOperationException ex)
{
    // Handle corrupted or locked file
}
```

## Testing

The library includes comprehensive unit tests covering:
- All service methods and edge cases
- Error handling scenarios
- File I/O operations
- Integration tests with external dependencies

Run tests using:
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow existing code conventions
- Add unit tests for new functionality
- Update documentation for public APIs
- Ensure all tests pass before submitting

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/yashbrahmbhatt/uipath/blob/main/LICENSE) file for details.

## Changelog

### v1.0.0 (Initial Release)
- ✅ ExcelHelperService with comprehensive Excel operations
- ✅ EmailHelperService for HTML email templating
- ✅ SwaggerCodeGeneratorService for API client generation with integrated file management
- ✅ BaseCodeGenerationService for programmatic code generation
- ✅ MiscHelperService with utility functions
- ✅ Comprehensive model classes and configuration options
- ✅ Full unit test coverage
- ✅ Robust error handling and logging

---

**Author**: Yash Brahmbhatt  
**Repository**: [https://github.com/yashbrahmbhatt/uipath](https://github.com/yashbrahmbhatt/uipath)  
**Issues**: [Report bugs or request features](https://github.com/yashbrahmbhatt/uipath/issues)