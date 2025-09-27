using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Services;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Tests for ConfigService code generation functionality including class generation, name sanitization, and XML escaping
    /// </summary>
    [TestClass]
    public class ConfigServiceCodeGenerationTests : ConfigServiceTestBase
    {
        #region GenerateClassString Tests

        [TestMethod]
        public void GenerateClassString_ValidConfig_ShouldGenerateClass()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            
            var className = "TestConfig";
            var outputFolder = _testDataDirectory;
            var namespaceName = "Test.Namespace";

            // Act
            var result = service.GenerateClassString(className, outputFolder, namespaceName);

            // Assert
            result.Should().NotBeNull("GenerateClassString should return a valid class string");
            result.Should().NotBeEmpty("Generated class string should contain actual code content");
            result.Should().Contain(className, "Generated class should include the specified class name");
            result.Should().Contain(namespaceName, "Generated class should be placed in the specified namespace");
            result.Should().Contain("public class", "Generated code should define a public class structure");
            result.Should().Contain("using System;", "Generated code should include necessary using statements");
        }

        [TestMethod]
        public void GenerateClassString_WithSettings_ShouldIncludeSettingsRegion()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            
            var className = "TestConfig";
            var outputFolder = _testDataDirectory;
            var namespaceName = "Test.Namespace";

            // Act
            var result = service.GenerateClassString(className, outputFolder, namespaceName);

            // Assert
            result.Should().Contain("#region Settings", "Generated class should organize settings in a dedicated region");
            result.Should().Contain("TestSetting1", "Generated class should include properties for settings from config file");
        }

        [TestMethod]
        public void GenerateClassString_WithAssets_ShouldIncludeAssetsRegion()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            
            var className = "TestConfig";
            var outputFolder = _testDataDirectory;
            var namespaceName = "Test.Namespace";

            // Act
            var result = service.GenerateClassString(className, outputFolder, namespaceName);

            // Assert
            result.Should().Contain("#region Assets", "Generated class should organize assets in a dedicated region");
            result.Should().Contain("TestAsset1", "Generated class should include properties for assets from config file");
        }

        #endregion

        #region SanitizeName Tests

        [TestMethod]
        public void SanitizeName_ValidName_ShouldReturnSameName()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.SanitizeName("ValidName").Should().Be("ValidName", "Valid C# identifiers should pass through unchanged");
            service.SanitizeName("Valid_Name_123").Should().Be("Valid_Name_123", "Valid names with underscores and numbers should pass through unchanged");
        }

        [TestMethod]
        public void SanitizeName_InvalidCharacters_ShouldRemoveInvalidChars()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.SanitizeName("Invalid-Name!@#").Should().Be("InvalidName", "Names with special characters should have invalid characters removed");
            service.SanitizeName("Name With Spaces").Should().Be("NameWithSpaces", "Names with spaces should have spaces removed to create valid C# identifier");
        }

        [TestMethod]
        public void SanitizeName_StartsWithDigit_ShouldPrefixWithUnderscore()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.SanitizeName("123Name").Should().Be("_123Name", "Names starting with digits should be prefixed with underscore to create valid C# identifier");
        }

        [TestMethod]
        public void SanitizeName_EmptyOrNull_ShouldReturnUnnamed()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.SanitizeName("").Should().Be("Unnamed", "Empty string should be replaced with default 'Unnamed' identifier");
            service.SanitizeName(null).Should().Be("Unnamed", "Null value should be replaced with default 'Unnamed' identifier");
            service.SanitizeName("   ").Should().Be("Unnamed", "Whitespace-only string should be replaced with default 'Unnamed' identifier");
        }

        #endregion

        #region EscapeXml Tests

        [TestMethod]
        public void EscapeXml_SpecialCharacters_ShouldEscapeCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.EscapeXml("<test>").Should().Be("&lt;test&gt;", "Angle brackets should be escaped for XML compatibility");
            service.EscapeXml("A&B").Should().Be("A&amp;B", "Ampersand characters should be escaped for XML compatibility");
            service.EscapeXml("\"quoted\"").Should().Be("&quot;quoted&quot;", "Double quotes should be escaped for XML compatibility");
            service.EscapeXml("'single'").Should().Be("&apos;single&apos;", "Single quotes should be escaped for XML compatibility");
        }

        [TestMethod]
        public void EscapeXml_NoSpecialCharacters_ShouldReturnSame()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.EscapeXml("Normal text").Should().Be("Normal text", "Text without special characters should pass through unchanged");
            service.EscapeXml("123").Should().Be("123", "Numeric text should pass through unchanged");
        }

        [TestMethod]
        public void EscapeXml_EmptyOrNull_ShouldHandleGracefully()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.EscapeXml("").Should().Be("", "Empty string should be handled gracefully by XML escaping");
            service.EscapeXml(null).Should().Be("", "Null value should be handled gracefully by XML escaping and converted to empty string");
        }

        #endregion

        #region GenerateClassFiles Tests

        [TestMethod]
        public void GenerateClassFiles_ValidInputFile_ShouldGenerateFiles()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            
            var outputDir = Path.Combine(_testDataDirectory, "Generated");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var ns = "Test.Generated";
            var usings = "System.Linq";

            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            var result = service.GenerateClassFiles(outputDir, ns, usings, logger);

            // Assert
            result.Should().NotBeNull("GenerateClassFiles should return a valid result string");
            result.Should().NotBeEmpty("GenerateClassFiles should provide feedback about the generation process");
            result.Should().Contain("Generated class", "Result should indicate successful class generation");
            
            // Verify at least one file was created
            var generatedFiles = Directory.GetFiles(outputDir, "*.cs");
            generatedFiles.Should().NotBeEmpty("GenerateClassFiles should create at least one .cs file in the output directory");
        }

        [TestMethod]
        public void GenerateClassFiles_InvalidService_ShouldThrowException()
        {
            // Arrange
            var invalidFile = Path.Combine(_testDataDirectory, "InvalidFile.txt");
            File.WriteAllText(invalidFile, "Not an Excel file");
            
            // Create a service that will have validation errors
            var service = new ConfigService(invalidFile, _mockOrchestratorService.Object, CreateLogger());
            service.ValidateConfigFile(); // This will set the ConfigFileError
            
            var outputDir = _testDataDirectory;
            var ns = "Test.Generated";
            var usings = "";

            // Act & Assert
            Action act = () => service.GenerateClassFiles(outputDir, ns, usings);
            act.Should().Throw<InvalidOperationException>("GenerateClassFiles should throw when ConfigService has validation errors");
        }

        #endregion
    }
}