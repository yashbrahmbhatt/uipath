using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Services;
using Yash.Config.ConfigurationFile;
using Yash.Config.Models.Config;
using Yash.Config.Models;
using Yash.Config.ConfigurationService;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Tests for ConfigService edge cases and boundary conditions
    /// </summary>
    [TestClass]
    public class ConfigServiceEdgeCaseTests : ConfigServiceTestBase
    {
        [TestMethod]
        public void ValidateConfigFile_WithMultipleScopes_ShouldHandleCorrectly()
        {
                        // Arrange
            var scopedConfigFile = Path.Combine(_testDataDirectory, "ScopedConfig.xlsx");
            CreateTestConfigFileWithMultipleScopes(scopedConfigFile);

            var service = new ConfigService(scopedConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually
            service.ValidateConfigFile();

            // Assert
            service.Metadata.Should().NotBeNull("ValidateConfigFile should create Metadata for multi-scope configuration files");
            service.Metadata.ConfigFileError.Should().BeNull("Multi-scope configuration files should validate without errors");
            service.Metadata.Scopes.Should().NotBeEmpty("Metadata should extract scope information from configuration items");
            service.Metadata.Scopes.Should().Contain("Production", "Metadata should identify Production scope from configuration data");
            service.Metadata.Scopes.Should().Contain("Development", "Metadata should identify Development scope from configuration data");
            service.Metadata.ConfigByScope.Should().NotBeEmpty("Metadata should organize configuration items by their scope");
            service.Metadata.ConfigByScope.Should().ContainKey("Production", "ConfigByScope should have a section for Production scope items");
            service.Metadata.ConfigByScope.Should().ContainKey("Development", "ConfigByScope should have a section for Development scope items");
        }

        [TestMethod]
        public void SanitizeName_ComplexScenarios_ShouldHandleCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.SanitizeName("Special!@#$%Characters").Should().Be("SpecialCharacters", "Special characters should be removed to create valid C# identifier");
            service.SanitizeName("Multiple   Spaces").Should().Be("MultipleSpaces", "Multiple consecutive spaces should be collapsed and removed");
            service.SanitizeName("123_Start_With_Number").Should().Be("_123_Start_With_Number", "Names starting with numbers should be prefixed with underscore");
            service.SanitizeName("_ValidName123").Should().Be("_ValidName123", "Already valid C# identifiers should pass through unchanged");
            service.SanitizeName("àáâãäåæçèéêë").Should().Be("Unnamed", "Non-ASCII characters should be removed, and if result is empty, default to 'Unnamed'"); // Non-ASCII characters removed, becomes empty
        }

        [TestMethod]
        public void EscapeXml_ComplexInput_ShouldEscapeCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.EscapeXml("Test & \"complex\" input with <tags>")
                .Should().Be("Test &amp; &quot;complex&quot; input with &lt;tags&gt;", "Complex XML strings with multiple special characters should be fully escaped");
            
            service.EscapeXml("<>&\"").Should().Be("&lt;&gt;&amp;&quot;", "All XML reserved characters should be properly escaped");
            
            // Single quotes are escaped in the current implementation
            service.EscapeXml("'single quotes'").Should().Be("&apos;single quotes&apos;", "Single quotes should be escaped for XML attribute compatibility");
        }

        [TestMethod]
        public void DetermineConfigType_EdgeCases_ShouldHandleCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var emptyTable = new DataTable("Empty");
            var tableWithExtraColumns = CreateSettingDataTable();
            tableWithExtraColumns.Columns.Add("ExtraColumn", typeof(string));

            // Act & Assert
            service.DetermineConfigType(emptyTable).Should().Be(ConfigSheetType.Unknown, "Tables with no columns should be classified as Unknown type");
            service.DetermineConfigType(tableWithExtraColumns).Should().Be(ConfigSheetType.Setting, "Tables with extra columns beyond required ones should still be correctly identified by their core columns");
        }

        [TestMethod]
        public void IsValid_WithValidConfig_ShouldReturnTrue()
        {
            // Arrange & Act
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually
            service.ValidateConfigFile();

            // Assert
            service.IsValid.Should().BeTrue("ConfigService should report valid status when initialized with a valid configuration file");
        }

        [TestMethod]
        public void FilePath_ShouldReturnCorrectPath()
        {
            // Arrange & Act
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Assert
            service.FilePath.Should().Be(_testConfigFile, "ConfigService should correctly store and return the file path provided during initialization");
        }

        [TestMethod]
        public void IsValidConfigType_WithValidTypes_ShouldReturnTrue()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsValidConfigType(typeof(Dictionary<string, object>)).Should().BeTrue("Dictionary<string, object> should be recognized as a valid configuration type");
            service.IsValidConfigType(typeof(Yash.Config.Models.Config.Configuration)).Should().BeTrue("Configuration model class should be recognized as a valid configuration type");
        }

        [TestMethod]
        public void IsValidConfigType_WithInvalidTypes_ShouldReturnFalse()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsValidConfigType(typeof(string)).Should().BeFalse("Simple string type should not be recognized as a valid configuration type");
            service.IsValidConfigType(typeof(int)).Should().BeFalse("Primitive integer type should not be recognized as a valid configuration type");
            service.IsValidConfigType(typeof(object)).Should().BeFalse("Generic object type should not be recognized as a valid configuration type");
        }

        [TestMethod]
        public void ValidateConfigFileToType_WithValidType_ShouldReturnValidations()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            var configFile = service.Metadata.ConfigFile;

            // Act
            var validations = service.ValidateConfigFileToType(typeof(Dictionary<string, object>), configFile);

            // Assert
            validations.Should().NotBeNull();
            validations.Should().NotBeEmpty();
            validations.Should().Contain("Type is Dictionary<string, object>, no further validation performed.");
        }
    }
}