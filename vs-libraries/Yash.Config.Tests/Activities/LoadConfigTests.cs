using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using System.Data;
using System.IO;
using Yash.Config.Activities;
using Yash.Config.Models.Config;
using OfficeOpenXml;
using UiPath.Core.Activities;
using System.Diagnostics;

namespace Yash.Config.Tests.Activities
{
    [TestClass]
    public class LoadConfigTests
    {
        private string _testDataDirectory = string.Empty;
        private string _testConfigPath = string.Empty;
        private List<string> _logMessages = new();
        private List<TraceEventType> _logLevels = new();
        
        public TestContext? TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            // Set EPPlus license context for tests
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Initialize logging collections
            _logMessages = new List<string>();
            _logLevels = new List<TraceEventType>();

            _testDataDirectory = Path.Combine(TestContext?.TestDeploymentDir ?? ".", "TestData");
            _testConfigPath = Path.Combine(_testDataDirectory, "TestConfig.xlsx");
            
            // Create test data directory if it doesn't exist
            if (!Directory.Exists(_testDataDirectory))
            {
                Directory.CreateDirectory(_testDataDirectory);
            }

            CreateTestConfigFile(_testConfigPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
        }

        private void CreateTestConfigFile(string filePath)
        {
            using var package = new ExcelPackage();
            
            // Create Settings sheet
            var settingsSheet = package.Workbook.Worksheets.Add("Settings");
            settingsSheet.Cells[1, 1].Value = "Name";
            settingsSheet.Cells[1, 2].Value = "Value";
            settingsSheet.Cells[2, 1].Value = "TestSetting";
            settingsSheet.Cells[2, 2].Value = "TestValue";

            // Create Constants sheet
            var constantsSheet = package.Workbook.Worksheets.Add("Constants");
            constantsSheet.Cells[1, 1].Value = "Name";
            constantsSheet.Cells[1, 2].Value = "Value";
            constantsSheet.Cells[2, 1].Value = "TestConstant";
            constantsSheet.Cells[2, 2].Value = "ConstantValue";

            package.SaveAs(new FileInfo(filePath));
        }

        #region Constructor Tests

        [TestMethod]
        public void LoadConfig_Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var activity = new LoadConfig<string>();

            // Assert
            activity.Should().NotBeNull();
            activity.DisplayName.Should().NotBeNull();
        }

        [TestMethod]
        public void LoadConfig_Constructor_WithGenericType_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var stringActivity = new LoadConfig<string>();
            var configActivity = new LoadConfig<Configuration>();
            var intActivity = new LoadConfig<int>();

            // Assert
            stringActivity.Should().NotBeNull();
            configActivity.Should().NotBeNull();
            intActivity.Should().NotBeNull();
        }

        #endregion

        #region Activity Property Tests

        [TestMethod]
        public void LoadConfig_WorkbookPath_ShouldAcceptStringArgument()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();
            var testPath = "C:\\TestPath\\Configuration.xlsx";

            // Act
            activity.WorkbookPath = new InArgument<string>(testPath);

            // Assert
            activity.WorkbookPath.Should().NotBeNull();
            var propertyInfo = typeof(LoadConfig<Configuration>).GetProperty("WorkbookPath");
            propertyInfo.Should().NotBeNull();
            propertyInfo!.PropertyType.Name.Should().Contain("InArgument");
        }

        [TestMethod]
        public void LoadConfig_Scope_ShouldAcceptTypeValue()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.Scope = typeof(object);

            // Assert
            activity.Scope.Should().Be(typeof(object));
        }

        [TestMethod]
        public void LoadConfig_Level_ShouldAcceptLogLevelValue()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.Level = LogLevel.Error;

            // Assert
            activity.Level.Should().Be(LogLevel.Error);
        }

        [TestMethod]
        public void LoadConfig_DesignTimePath_ShouldAcceptStringValue()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.DesignTimePath = "TestPath";

            // Assert
            activity.DesignTimePath.Should().Be("TestPath");
        }

        [TestMethod]
        public void LoadConfig_DebugMode_ShouldAcceptBooleanValue()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.DebugMode = true;

            // Assert
            activity.DebugMode.Should().BeTrue();
        }

        #endregion

        #region LogLevel Tests

        [TestMethod]
        public void LoadConfig_WithDifferentLogLevels_ShouldAcceptValues()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act & Assert
            activity.Level = LogLevel.Trace;
            activity.Level.Should().Be(LogLevel.Trace);

            activity.Level = LogLevel.Info;
            activity.Level.Should().Be(LogLevel.Info);

            activity.Level = LogLevel.Warn;
            activity.Level.Should().Be(LogLevel.Warn);

            activity.Level = LogLevel.Error;
            activity.Level.Should().Be(LogLevel.Error);
        }

        [TestMethod]
        public void LoadConfig_DefaultLogLevel_ShouldBeInfo()
        {
            // Arrange & Act
            var activity = new LoadConfig<Configuration>();

            // Assert
            activity.Level.Should().Be(LogLevel.Info);
        }

        #endregion

        #region DebugMode Tests

        [TestMethod]
        public void LoadConfig_DefaultDebugMode_ShouldBeFalse()
        {
            // Arrange & Act
            var activity = new LoadConfig<Configuration>();

            // Assert
            activity.DebugMode.Should().BeFalse();
        }

        [TestMethod]
        public void LoadConfig_WithDebugModeEnabled_ShouldSetToTrue()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.DebugMode = true;

            // Assert
            activity.DebugMode.Should().BeTrue();
        }

        #endregion

        #region Default Scope Tests

        [TestMethod]
        public void LoadConfig_DefaultScope_ShouldBeGenericType()
        {
            // Arrange & Act
            var stringActivity = new LoadConfig<string>();
            var configActivity = new LoadConfig<Configuration>();

            // Assert
            stringActivity.Scope.Should().Be(typeof(string));
            configActivity.Scope.Should().Be(typeof(Configuration));
        }

        [TestMethod]
        public void LoadConfig_WithCustomScope_ShouldSetScope()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.Scope = typeof(object);

            // Assert
            activity.Scope.Should().Be(typeof(object));
        }

        [TestMethod]
        public void LoadConfig_WithNullScope_ShouldAcceptNull()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            activity.Scope = null;

            // Assert
            activity.Scope.Should().BeNull();
        }

        #endregion

        #region Property Validation Tests

        [TestMethod]
        public void LoadConfig_Properties_ShouldHaveCorrectTypes()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act & Assert
            var workbookPathProperty = typeof(LoadConfig<Configuration>).GetProperty("WorkbookPath");
            workbookPathProperty.Should().NotBeNull();
            workbookPathProperty!.PropertyType.Should().Be(typeof(InArgument<string>));

            var scopeProperty = typeof(LoadConfig<Configuration>).GetProperty("Scope");
            scopeProperty.Should().NotBeNull();
            scopeProperty!.PropertyType.Should().Be(typeof(Type));

            var levelProperty = typeof(LoadConfig<Configuration>).GetProperty("Level");
            levelProperty.Should().NotBeNull();
            levelProperty!.PropertyType.Should().Be(typeof(LogLevel));

            var designTimePathProperty = typeof(LoadConfig<Configuration>).GetProperty("DesignTimePath");
            designTimePathProperty.Should().NotBeNull();
            designTimePathProperty!.PropertyType.Should().Be(typeof(string));

            var debugModeProperty = typeof(LoadConfig<Configuration>).GetProperty("DebugMode");
            debugModeProperty.Should().NotBeNull();
            debugModeProperty!.PropertyType.Should().Be(typeof(bool));
        }

        #endregion

        #region LoadConfigException Tests

        [TestMethod]
        public void LoadConfigException_WithMessage_ShouldCreateCorrectly()
        {
            // Arrange & Act
            var exception = new LoadConfigException("Test error message");

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("[LoadConfigAsync]");
            exception.Message.Should().Contain("Test error message");
        }

        [TestMethod]
        public void LoadConfigException_WithInnerException_ShouldCreateCorrectly()
        {
            // Arrange
            var innerException = new ArgumentException("Inner error");
            
            // Act
            var exception = new LoadConfigException("Test error message", innerException);

            // Assert
            exception.Should().NotBeNull();
            exception.Message.Should().Contain("[LoadConfigAsync]");
            exception.InnerException.Should().Be(innerException);
        }

        [TestMethod]
        public void LoadConfigException_WithEventType_ShouldSetEventType()
        {
            // Arrange & Act
            var exception = new LoadConfigException("Test error", System.Diagnostics.TraceEventType.Warning);

            // Assert
            exception.Should().NotBeNull();
            exception.EventType.Should().Be(System.Diagnostics.TraceEventType.Warning);
        }

        #endregion

        #region Activity Inheritance Tests

        [TestMethod]
        public void LoadConfig_ShouldInheritFromAsyncCodeActivity()
        {
            // Arrange & Act
            var activity = new LoadConfig<Configuration>();

            // Assert
            activity.Should().BeAssignableTo<AsyncCodeActivity<Configuration>>();
        }

        [TestMethod]
        public void LoadConfig_ShouldSupportGenericTypes()
        {
            // Arrange & Act
            var stringActivity = new LoadConfig<string>();
            var intActivity = new LoadConfig<int>();
            var objectActivity = new LoadConfig<object>();

            // Assert
            stringActivity.Should().BeAssignableTo<AsyncCodeActivity<string>>();
            intActivity.Should().BeAssignableTo<AsyncCodeActivity<int>>();
            objectActivity.Should().BeAssignableTo<AsyncCodeActivity<object>>();
        }

        #endregion

        #region WorkbookPath Validation Tests

        [TestMethod]
        public void LoadConfig_WorkbookPath_ShouldBeInArgumentType()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act
            var workbookPathProperty = activity.GetType().GetProperty("WorkbookPath");

            // Assert
            workbookPathProperty.Should().NotBeNull();
            workbookPathProperty!.PropertyType.Should().Be(typeof(InArgument<string>));
        }

        [TestMethod]
        public void LoadConfig_CanSetWorkbookPath_WithDifferentValues()
        {
            // Arrange
            var activity = new LoadConfig<Configuration>();

            // Act & Assert
            activity.WorkbookPath = new InArgument<string>("Test1.xlsx");
            activity.WorkbookPath.Should().NotBeNull();

            activity.WorkbookPath = new InArgument<string>("C:\\Temp\\Test2.xlsx");
            activity.WorkbookPath.Should().NotBeNull();

            activity.WorkbookPath = new InArgument<string>("");
            activity.WorkbookPath.Should().NotBeNull();
        }

        #endregion
    }
}
