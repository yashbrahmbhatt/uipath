# Yash Standard Project

## Overview
This is a comprehensive UiPath RPA project following the REFramework pattern with Dispatcher-Performer architecture. The project features a sophisticated coded workflow framework with abstract base classes, comprehensive testing capabilities, and standardized error handling patterns.

## Project Structure

```
├── 00_Shared/              # Shared components and utilities
├── 01_Dispatcher/          # Queue item creation and validation
├── 02_Performer/           # Transaction processing
├── 99_Reporter/            # Data reporting and analytics
├── CodedWorkflows/         # Abstract base workflow classes
├── Configs/               # Configuration management classes
└── Data/                 # Static data and temporary files
```

## Architecture Overview

### Base Class Hierarchy

```
CodedWorkflowBase (UiPath)
    ↓
CodedWorkflow (Logging utilities)
    ↓
CodedWorkflowWithConfig (Configuration loading)
    ↓
TestableCodedWorkflow (Testing framework)
    ↓
┌─────────────────────┬─────────────────────┐
│   DispatcherWorkflow │   PerformerWorkflow │
│   (Abstract)         │   (Abstract)        │
└─────────────────────┴─────────────────────┘
           ↓                       ↓
      Dispatcher              Performer
    (Concrete)              (Concrete)
           ↓                       ↓
  BaseDispatcherTest       BasePerformerTest
       (Test Base)           (Test Base)
           ↓                       ↓
   Test Implementations    Test Implementations
```

## Key Components

### CodedWorkflows Base Classes

#### **CodedWorkflow** (`CodedWorkflows/CodedWorkflow.cs`)
- **Purpose**: Foundation class with logging utilities
- **Features**: 
  - Enhanced logging with TraceEventType conversion
  - Common UiPath service access patterns

#### **CodedWorkflowWithConfig** (`CodedWorkflows/CodedWorkflowWithConfig.cs`)
- **Purpose**: Adds configuration management capabilities
- **Features**:
  - Automatic configuration loading for multiple scopes
  - Support for Shared, Dispatcher, Performer, and Reporter configurations
  - Async configuration loading with orchestrator integration

#### **TestableCodedWorkflow** (`CodedWorkflows/TestableCodedWorkflow.cs`)
- **Purpose**: Adds comprehensive testing framework
- **Features**:
  - Test execution with expected exception validation
  - Test lifecycle management (Initialize, Execute, Validate, Cleanup)
  - Standardized test patterns and assertions

#### **DispatcherWorkflow** (`CodedWorkflows/DispatcherWorkflow.cs`)
- **Purpose**: Abstract base for all dispatcher workflows
- **Features**:
  - Standardized dispatcher execution patterns
  - Application lifecycle management
  - Error handling with email notifications
  - Queue item creation framework

#### **PerformerWorkflow** (`CodedWorkflows/PerformerWorkflow.cs`)
- **Purpose**: Abstract base for all performer workflows
- **Features**:
  - Complete state machine implementation
  - Transaction processing with retry logic
  - Business and system exception handling
  - Maintenance time and stop signal management
  - Comprehensive error notifications

### Main Components

#### **Dispatcher** (`01_Dispatcher/`)
- **Purpose**: Creates queue items for processing
- **Main Entry**: `Dispatcher.cs` (inherits from `DispatcherWorkflow`)
- **Framework Components**: 
  - `Framework/DispatcherInitializeApplications.cs`
  - `Framework/DispatcherCloseApplications.cs`
- **Tests**: Located in `Tests/` subfolder, inherit from `BaseDispatcherTest`

#### **Performer** (`02_Performer/`)
- **Purpose**: Processes individual transactions from queue
- **Main Entry**: `Performer.cs` (inherits from `PerformerWorkflow`)
- **Framework Components**:
  - `Framework/PerformerInitializeApplications.cs`
  - `Framework/PerformerProcess.cs`
  - `Framework/PerformerCloseApplications.cs`
- **Tests**: Located in `Tests/` subfolder, inherit from `BasePerformerTest`

#### **Reporter** (`99_Reporter/`)
- **Purpose**: Generates reports and analytics
- **Main Entry**: `Reporter.cs`
- **Components**: Logic and Orchestrator subfolders

### Shared Components (`00_Shared/`)
- **Email Operations**: `GetEmails.cs`, `SendEmail.cs`, `DeleteEmails.cs`, `MarkEmailsAsRead.cs`
- **Utility Workflows**: `IsStopRequested.xaml`, `KillProcess.xaml`
- **Test Data Management**: Various data loading utilities

## Testing Framework

The project uses a sophisticated testing framework built on `TestableCodedWorkflow`:

- **BaseDispatcherTest**: Base class for Dispatcher tests
- **BasePerformerTest**: Base class for Performer tests  
- **BaseReporterTest**: Base class for Reporter tests
- **Test Scenarios**: Success, failure, business exceptions, system exceptions, maintenance time, no data

### Test Categories
- **Success Tests**: Normal execution scenarios
- **Failure Tests**: Framework and initialization errors
- **Business Exception Tests**: Data validation and business rule violations
- **System Exception Tests**: Technical failures and recovery
- **Edge Case Tests**: Maintenance time, no data, stop signals

## Configuration Management

Configuration is managed through a multi-layered approach:
- **Excel Configuration**: `Data/Config.xlsx` - Main configuration file
- **Generated Config Classes**: Separate classes for each component (Shared, Dispatcher, Performer, Reporter)
- **Orchestrator Integration**: Asset and queue management
- **Environment-Specific Settings**: Support for different deployment environments

## Getting Started

1. Open the project in UiPath Studio
2. Configure `Data/Config.xlsx` with your environment settings
3. Run tests to validate setup
4. Execute main workflows as needed

## Dependencies

- **UiPath Packages**:
  - UiPath.CodedWorkflows [24.10.1]
  - UiPath.Excel.Activities [3.2.1]
  - UiPath.GSuite.Activities [3.3.10]
  - UiPath.Mail.Activities [2.3.10]
  - UiPath.System.Activities [25.8.0]
  - UiPath.Testing.Activities [25.4.1]

- **Custom Yash Packages**:
  - Yash.Config [1.0.254.4155]
  - Yash.Orchestrator [1.0.255.23850]
  - Yash.Utility [1.0.254.4233]

---

# 📚 Development Tutorials

## Tutorial 1: Creating a New Dispatcher Workflow

### Step 1: Create the Dispatcher Class

Create a new file in the appropriate folder (e.g., `03_NewDispatcher/NewDispatcher.cs`):

```csharp
using System;
using UiPath.CodedWorkflows;
using UiPath.Orchestrator.Client.Models;
using Yash.StandardProject.CodedWorkflows;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._03_NewDispatcher
{
    /// <summary>
    /// Custom dispatcher workflow for [describe your purpose].
    /// Inherits from DispatcherWorkflow to leverage standardized dispatcher functionality.
    /// </summary>
    public class NewDispatcher : DispatcherWorkflow
    {
        #region Property Implementations

        public override string TestId { get; set; } = "";
        public override string ExpectedExceptionMessage { get; set; } = "";
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string[] ConfigScopes { get; set; } = new[] { "Shared", "Dispatcher" };

        #endregion

        #region Required Method Implementations

        /// <summary>
        /// Implements the main data processing logic.
        /// This is where you read input data and create queue items.
        /// </summary>
        /// <returns>Number of queue items created</returns>
        public override int ProcessInputData()
        {
            Log("Starting data processing", LogLevel.Info);
            
            var itemsCreated = 0;
            
            try
            {
                // TODO: Implement your data source reading logic here
                // Examples:
                // - Read from Excel file
                // - Query database
                // - Call web service
                // - Process emails
                
                // Example: Reading from Excel
                // var dataTable = workflows.ReadExcelFile("Data/InputData.xlsx", "Sheet1");
                // foreach (DataRow row in dataTable.Rows)
                // {
                //     var queueItem = new QueueItem
                //     {
                //         Name = "ProcessTransaction",
                //         SpecificContent = new Dictionary<string, object>
                //         {
                //             {"CustomerID", row["CustomerID"].ToString()},
                //             {"Amount", row["Amount"].ToString()},
                //             // Add other data fields as needed
                //         }
                //     };
                //     
                //     system.AddQueueItem(SharedConfig.QueueName, queueItem, default, default, default, default);
                //     itemsCreated++;
                // }
                
                Log($"Created {itemsCreated} queue items", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error processing input data: {ex.Message}", LogLevel.Error);
                throw;
            }
            
            return itemsCreated;
        }

        /// <summary>
        /// Closes applications gracefully.
        /// Implement cleanup logic for any applications opened during processing.
        /// </summary>
        public override void CloseApplications()
        {
            try
            {
                Log("Closing applications", LogLevel.Info);
                
                // TODO: Implement application cleanup
                // Examples:
                // - Close Excel applications
                // - Close browser instances
                // - Close database connections
                // - Save and close files
                
                Log("Applications closed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error closing applications: {ex.Message}", LogLevel.Warn);
                // Don't throw - this is cleanup code
            }
        }

        /// <summary>
        /// Initializes required applications.
        /// Set up any applications or connections needed for processing.
        /// </summary>
        public override void InitializeApplications()
        {
            try
            {
                Log("Initializing applications", LogLevel.Info);
                
                // TODO: Implement application initialization
                // Examples:
                // - Open Excel application
                // - Launch browser
                // - Connect to database
                // - Authenticate with web services
                
                Log("Applications initialized successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error initializing applications: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        #endregion

        #region Test Support Methods

        [Workflow]
        public override void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;
            base.Execute();
        }

        public override void InitializeTest()
        {
            Log("Initializing dispatcher test", LogLevel.Info);
            // TODO: Add test-specific initialization
        }

        public override void ValidateTest()
        {
            Log("Validating dispatcher test", LogLevel.Info);
            // TODO: Add test-specific validation
        }

        public override void CleanupTest()
        {
            Log("Cleaning up dispatcher test", LogLevel.Info);
            // TODO: Add test-specific cleanup
        }

        #endregion
    }
}
```

### Step 2: Create Test Classes

Create test files in `03_NewDispatcher/Tests/`:

#### Base Test Class (`BaseNewDispatcherTest.cs`):
```csharp
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._03_NewDispatcher.Tests
{
    public abstract class BaseNewDispatcherTest : NewDispatcher
    {
        public override string TestId { get; set; }
        public override string ExpectedExceptionMessage { get; set; } = "";
        
        public new void Execute()
        {
            base.Execute();
        }
    }
}
```

#### Success Test (`NewDispatcher.Success.cs`):
```csharp
using UiPath.Testing;

namespace Yash.StandardProject._03_NewDispatcher.Tests
{
    public class NewDispatcher_Success : BaseNewDispatcherTest
    {
        public override string TestId { get; set; } = "NewDispatcher.Success";
        
        [TestCase]
        protected new void Execute()
        {
            base.Execute();
        }
    }
}
```

### Step 3: Update Configuration

Add any new configuration settings to your config classes in the `Configs/` folder.

### Step 4: Register in project.json

Add the new entry points to `project.json`:
```json
{
  "filePath": "03_NewDispatcher\\NewDispatcher.cs",
  "uniqueId": "your-new-guid-here",
  "input": [],
  "output": []
}
```

---

## Tutorial 2: Creating a New Performer Workflow

### Step 1: Create the Performer Class

Create a new file (e.g., `04_NewPerformer/NewPerformer.cs`):

```csharp
using System;
using UiPath.CodedWorkflows;
using UiPath.Orchestrator.Client.Models;
using Yash.StandardProject.CodedWorkflows;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._04_NewPerformer
{
    /// <summary>
    /// Custom performer workflow for [describe your purpose].
    /// Inherits from PerformerWorkflow to leverage standardized performer functionality.
    /// </summary>
    public class NewPerformer : PerformerWorkflow
    {
        #region Property Implementations

        public override string TestId { get; set; } = "";
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string[] ConfigScopes { get; set; } = { "Shared", "Performer" };
        public override string ExpectedExceptionMessage { get; set; } = "";

        #endregion

        #region Required Method Implementations

        /// <summary>
        /// Processes the specific business logic for a transaction.
        /// This is where you implement your main business process.
        /// </summary>
        /// <param name="transactionItem">The transaction item to process</param>
        protected override void ProcessTransactionData(QueueItem transactionItem)
        {
            Log($"Processing transaction: {transactionItem.Reference}", LogLevel.Info);
            
            try
            {
                // Extract data from queue item
                var customerID = transactionItem.SpecificContent["CustomerID"]?.ToString();
                var amount = transactionItem.SpecificContent["Amount"]?.ToString();
                
                // TODO: Implement your business logic here
                // Examples:
                // - Data validation
                // - Web automation
                // - Database updates
                // - File processing
                // - API calls
                
                // Example validation
                if (string.IsNullOrEmpty(customerID))
                {
                    throw new BusinessRuleException("Customer ID is required");
                }
                
                if (!decimal.TryParse(amount, out var amountValue) || amountValue <= 0)
                {
                    throw new BusinessRuleException("Valid amount is required");
                }
                
                // Example processing
                Log($"Processing customer {customerID} with amount {amountValue}", LogLevel.Info);
                
                // TODO: Add your specific processing steps here
                // - Open applications
                // - Navigate to websites
                // - Fill forms
                // - Submit data
                // - Validate results
                
                Log($"Transaction {transactionItem.Reference} processed successfully", LogLevel.Info);
            }
            catch (BusinessRuleException)
            {
                // Re-throw business exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                // Wrap unexpected errors as system exceptions
                throw new Exception($"System error processing transaction: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Closes applications gracefully.
        /// </summary>
        public override void CloseApplications()
        {
            try
            {
                Log("Closing performer applications", LogLevel.Info);
                
                // TODO: Implement application cleanup
                // Examples:
                // - Close browser tabs
                // - Close Excel files
                // - Logout from applications
                // - Close database connections
                
                Log("Performer applications closed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error closing applications: {ex.Message}", LogLevel.Warn);
                // Don't throw - this is cleanup code
            }
        }

        /// <summary>
        /// Initializes required applications.
        /// </summary>
        public override void InitializeApplications()
        {
            try
            {
                Log("Initializing performer applications", LogLevel.Info);
                
                // TODO: Implement application initialization
                // Examples:
                // - Open browser
                // - Navigate to application
                // - Login
                // - Open Excel files
                // - Connect to databases
                
                Log("Performer applications initialized successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error initializing applications: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        #endregion

        #region Test Support Methods

        [Workflow]
        public override void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;
            base.Execute(ConfigPath, TestId);
        }

        public override void InitializeTest()
        {
            Log("Initializing performer test", LogLevel.Info);
            // TODO: Add test-specific initialization
            // - Set up test data
            // - Prepare test environment
        }

        public override void ValidateTest()
        {
            Log("Validating performer test", LogLevel.Info);
            // TODO: Add test-specific validation
            // - Check processing results
            // - Validate data changes
        }

        public override void CleanupTest()
        {
            Log("Cleaning up performer test", LogLevel.Info);
            // TODO: Add test-specific cleanup
            // - Remove test data
            // - Reset test environment
        }

        #endregion
    }
}
```

### Step 2: Create Test Classes

#### Base Test Class (`BaseNewPerformerTest.cs`):
```csharp
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._04_NewPerformer.Tests
{
    public abstract class BaseNewPerformerTest : NewPerformer
    {
        public override string TestId { get; set; }
        public override string ExpectedExceptionMessage { get; set; } = "";
        
        public new void Execute()
        {
            base.Execute();
        }
    }
}
```

#### Test Implementations:
```csharp
// Success Test
public class NewPerformer_Success : BaseNewPerformerTest
{
    public override string TestId { get; set; } = "NewPerformer.Success";
}

// Business Exception Test
public class NewPerformer_BusinessException : BaseNewPerformerTest
{
    public override string TestId { get; set; } = "NewPerformer.BusinessException";
    public override string ExpectedExceptionMessage { get; set; } = "Test business exception";
}

// System Exception Test
public class NewPerformer_SystemException : BaseNewPerformerTest
{
    public override string TestId { get; set; } = "NewPerformer.SystemException";
    public override string ExpectedExceptionMessage { get; set; } = "Test system exception";
}
```

---

## Tutorial 3: Creating a New Reporter Workflow

### Step 1: Create the Reporter Class

Create a new file (e.g., `98_NewReporter/NewReporter.cs`):

```csharp
using System;
using System.Data;
using UiPath.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._98_NewReporter
{
    /// <summary>
    /// Custom reporter workflow for [describe your purpose].
    /// Inherits from TestableCodedWorkflow for configuration and testing support.
    /// </summary>
    public class NewReporter : TestableCodedWorkflow
    {
        #region Property Implementations

        public override string TestId { get; set; } = "";
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string[] ConfigScopes { get; set; } = { "Shared", "Reporter" };
        public override string ExpectedExceptionMessage { get; set; } = "";

        #endregion

        #region Main Workflow Logic

        /// <summary>
        /// Main reporter execution logic.
        /// </summary>
        [Workflow]
        public override void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;
            
            // Initialize configuration
            InitializeConfiguration();
            
            // Execute reporting logic
            ExecuteReportingLogic();
        }

        /// <summary>
        /// Implements the main reporting workflow logic.
        /// </summary>
        protected virtual void ExecuteReportingLogic()
        {
            ValidateRequiredConfigurations(new[] { "Shared", "Reporter" });

            try
            {
                Log("Starting reporting workflow", LogLevel.Info);
                
                // Handle test failure scenario
                if (TestId == "NewReporter.Failure")
                    throw new Exception(TestId);
                
                // Step 1: Extract data
                var reportData = ExtractReportData();
                
                // Step 2: Transform data
                var transformedData = TransformReportData(reportData);
                
                // Step 3: Generate report
                var reportPath = GenerateReport(transformedData);
                
                // Step 4: Distribute report
                DistributeReport(reportPath);
                
                Log("Reporting workflow completed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error in reporting workflow: {ex.Message}", LogLevel.Error);
                HandleReportingError(ex);
                throw;
            }
        }

        #endregion

        #region Reporting Steps

        /// <summary>
        /// Extracts data for the report from various sources.
        /// </summary>
        /// <returns>Raw data for reporting</returns>
        protected virtual DataTable ExtractReportData()
        {
            Log("Extracting report data", LogLevel.Info);
            
            try
            {
                // TODO: Implement data extraction logic
                // Examples:
                // - Query Orchestrator for queue statistics
                // - Read from databases
                // - Aggregate log files
                // - Call APIs for data
                
                // Example: Get queue statistics
                // var queueData = orchestrator.GetQueueItems(SharedConfig.QueueName, filter);
                // var processedCount = queueData.Count(x => x.Status == ProcessingStatus.Successful);
                // var failedCount = queueData.Count(x => x.Status == ProcessingStatus.Failed);
                
                // Create sample data table
                var dataTable = new DataTable();
                dataTable.Columns.Add("Date", typeof(DateTime));
                dataTable.Columns.Add("ProcessedItems", typeof(int));
                dataTable.Columns.Add("FailedItems", typeof(int));
                dataTable.Columns.Add("SuccessRate", typeof(decimal));
                
                // TODO: Populate with actual data
                
                Log($"Extracted {dataTable.Rows.Count} data records", LogLevel.Info);
                return dataTable;
            }
            catch (Exception ex)
            {
                Log($"Error extracting report data: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Transforms raw data into report format.
        /// </summary>
        /// <param name="rawData">Raw data to transform</param>
        /// <returns>Transformed data ready for reporting</returns>
        protected virtual DataTable TransformReportData(DataTable rawData)
        {
            Log("Transforming report data", LogLevel.Info);
            
            try
            {
                // TODO: Implement data transformation logic
                // Examples:
                // - Calculate aggregations
                // - Apply business rules
                // - Format data
                // - Add calculated columns
                
                var transformedData = rawData.Copy();
                
                // Example transformations
                foreach (DataRow row in transformedData.Rows)
                {
                    var processed = Convert.ToInt32(row["ProcessedItems"]);
                    var failed = Convert.ToInt32(row["FailedItems"]);
                    var total = processed + failed;
                    
                    if (total > 0)
                    {
                        row["SuccessRate"] = Math.Round((decimal)processed / total * 100, 2);
                    }
                }
                
                Log("Data transformation completed", LogLevel.Info);
                return transformedData;
            }
            catch (Exception ex)
            {
                Log($"Error transforming report data: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Generates the actual report file.
        /// </summary>
        /// <param name="data">Data to include in the report</param>
        /// <returns>Path to the generated report file</returns>
        protected virtual string GenerateReport(DataTable data)
        {
            Log("Generating report", LogLevel.Info);
            
            try
            {
                var reportPath = $"Data/Reports/Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                // TODO: Implement report generation
                // Examples:
                // - Create Excel workbook
                // - Generate PDF report
                // - Create HTML dashboard
                // - Build PowerBI report
                
                // Example: Excel report generation
                // workflows.WriteExcelFile(reportPath, data, "ReportData");
                
                Log($"Report generated: {reportPath}", LogLevel.Info);
                return reportPath;
            }
            catch (Exception ex)
            {
                Log($"Error generating report: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Distributes the generated report.
        /// </summary>
        /// <param name="reportPath">Path to the report file</param>
        protected virtual void DistributeReport(string reportPath)
        {
            Log("Distributing report", LogLevel.Info);
            
            try
            {
                // TODO: Implement report distribution
                // Examples:
                // - Send email with attachment
                // - Upload to SharePoint
                // - Save to network location
                // - Publish to dashboard
                
                // Example: Email distribution
                // workflows.SendEmail(
                //     ReporterConfig.ReportRecipients,
                //     "Daily Process Report",
                //     "Please find attached the daily process report.",
                //     new[] { reportPath },
                //     default
                // );
                
                Log("Report distributed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error distributing report: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Handles reporting errors by sending notifications.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        protected virtual void HandleReportingError(Exception exception)
        {
            try
            {
                // Send error notification
                var errorMessage = $"Reporting workflow failed: {exception.Message}";
                
                // TODO: Implement error notification
                // workflows.SendEmail(
                //     SharedConfig.SysEx_To,
                //     "Reporting Error",
                //     errorMessage,
                //     default,
                //     default
                // );
                
                Log("Error notification sent", LogLevel.Info);
            }
            catch (Exception emailEx)
            {
                Log($"Failed to send error notification: {emailEx.Message}", LogLevel.Error);
            }
        }

        #endregion

        #region Test Support Methods

        public override void InitializeTest()
        {
            Log("Initializing reporter test", LogLevel.Info);
            // TODO: Add test-specific initialization
        }

        public override void ValidateTest()
        {
            Log("Validating reporter test", LogLevel.Info);
            // TODO: Add test-specific validation
        }

        public override void CleanupTest()
        {
            Log("Cleaning up reporter test", LogLevel.Info);
            // TODO: Add test-specific cleanup
        }

        #endregion
    }
}
```

### Step 2: Create Test Classes

Similar pattern as above, create base test class and specific test implementations.

---

## Tutorial 4: Best Practices and Tips

### Configuration Management
1. **Always use external configuration**: Keep settings in `Config.xlsx` or Orchestrator assets
2. **Validate configuration**: Check required settings are present and valid
3. **Environment-specific configs**: Support different settings for dev/test/prod

### Error Handling
1. **Use appropriate exception types**: 
   - `BusinessRuleException` for business logic violations
   - `Exception` for system/technical errors
2. **Log appropriately**: Different log levels for different scenarios
3. **Send meaningful notifications**: Include context and diagnostic information

### Testing Strategy
1. **Test all scenarios**: Success, business exceptions, system exceptions, edge cases
2. **Use meaningful test IDs**: Make it clear what each test validates
3. **Clean up after tests**: Ensure tests don't affect each other

### Performance Optimization
1. **Minimize application restarts**: Reuse connections where possible
2. **Batch operations**: Process multiple items together when feasible
3. **Implement retry logic**: Handle transient failures gracefully

### Code Organization
1. **Follow the established patterns**: Use the abstract base classes consistently
2. **Keep methods focused**: Each method should have a single responsibility
3. **Document your code**: Include meaningful comments and XML documentation

---

---

## 🛠️ Development Guidelines

### Code Standards
- Follow REFramework patterns and established architecture
- Use the testing framework for all new components
- Utilize shared utilities to avoid code duplication
- Maintain proper logging throughout workflows
- Keep configuration externalized in Excel files or Orchestrator assets

### Naming Conventions
- **Workflows**: Use PascalCase (e.g., `CustomerDataProcessor`)
- **Variables**: Use camelCase (e.g., `customerData`)
- **Constants**: Use UPPER_CASE (e.g., `MAX_RETRY_COUNT`)
- **Test Classes**: Use `ComponentName_TestScenario` pattern (e.g., `Dispatcher_Success`)

### Folder Structure Guidelines
- **Main Components**: Use numbered prefixes (01_, 02_, etc.)
- **Tests**: Always in `Tests/` subfolder within component folder
- **Framework**: Common framework files in `Framework/` subfolder
- **Shared**: Reusable components in `00_Shared/`

### Version Control
- Commit frequently with meaningful messages
- Use feature branches for new development
- Test thoroughly before merging to main branch
- Tag releases with semantic versioning

### Deployment
- Test in development environment first
- Use configuration files for environment-specific settings
- Validate all dependencies are available in target environment
- Document any manual setup steps required

---

## 🚀 Quick Start Checklist

### For New Developers
- [ ] Clone the repository
- [ ] Open project in UiPath Studio
- [ ] Configure `Data/Config.xlsx` with your settings
- [ ] Run test suite to validate setup
- [ ] Read through the base class documentation
- [ ] Start with the tutorials above

### For New Features
- [ ] Identify which component type (Dispatcher/Performer/Reporter)
- [ ] Follow the appropriate tutorial
- [ ] Create comprehensive tests
- [ ] Update configuration as needed
- [ ] Document any new patterns or utilities
- [ ] Submit for code review

### For Maintenance
- [ ] Review error logs and notifications
- [ ] Check queue item status in Orchestrator
- [ ] Validate configuration is up to date
- [ ] Test in isolated environment
- [ ] Update documentation if needed

---

## 📞 Support and Resources

### Documentation
- UiPath Studio Documentation
- REFramework Best Practices
- Custom Yash Package Documentation

### Testing
- All test cases are documented with expected behaviors
- Use test scenarios to validate configuration changes
- Run full test suite before deployment

### Troubleshooting
- Check Orchestrator logs for runtime issues
- Review exception screenshots in `Data/Exception Screenshots/`
- Validate configuration settings
- Ensure all dependencies are properly installed

### Contact
- For framework questions: Refer to base class documentation
- For business logic: Review component-specific documentation
- For configuration: Check `Configs/` folder and Excel files
