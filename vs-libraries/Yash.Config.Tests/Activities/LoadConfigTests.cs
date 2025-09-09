using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Activities;
using Yash.Config.Models;
using Yash.Config.Helpers;

namespace Yash.Config.Tests.Activities
{
    [TestClass]
    public class LoadConfigTests
    {
        private LoadConfig? _loadConfigActivity;
        private string? _testConfigPath;

        [TestInitialize]
        public void Setup()
        {
            _loadConfigActivity = new LoadConfig();
            _testConfigPath = Path.Combine(TestContext?.TestDeploymentDir ?? ".", "TestData", "TestConfig.xlsx");
            
            // Create test data directory if it doesn't exist
            var testDataDir = Path.GetDirectoryName(_testConfigPath);
            if (!Directory.Exists(testDataDir))
            {
                Directory.CreateDirectory(testDataDir);
            }
        }

        [TestMethod]
        public void LoadConfig_Activity_ShouldInitialize()
        {
            // Assert
            _loadConfigActivity.Should().NotBeNull();
        }

        [TestMethod]
        public void LoadConfig_WorkbookPath_ShouldAcceptValue()
        {
            // Arrange & Act
            var workbookPath = "C:\\TestPath\\Config.xlsx";
            
            // Note: InArgument properties in UiPath activities are complex objects
            // For unit testing, we're checking that the activity can be instantiated
            // and the property exists (not null after instantiation)
            _loadConfigActivity.Should().NotBeNull();
            
            // InArgument properties are auto-initialized by the UiPath framework
            // In unit tests, we can verify the property exists without null reference
            var propertyInfo = typeof(LoadConfig).GetProperty("WorkbookPath");
            propertyInfo.Should().NotBeNull();
            propertyInfo!.PropertyType.Name.Should().Contain("InArgument");
        }

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

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
        }

        public TestContext? TestContext { get; set; }
    }
}
