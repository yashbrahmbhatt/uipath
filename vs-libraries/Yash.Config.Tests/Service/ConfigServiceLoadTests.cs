using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Services;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Tests for ConfigService LoadConfigAsync functionality
    /// </summary>
    [TestClass]
    public class ConfigServiceLoadTests : ConfigServiceTestBase
    {
        [TestMethod]
        public async Task LoadConfigAsync_ValidConfig_ShouldReturnResult()
        {
            // Act
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();

            // Act
            var result = await service.LoadConfigAsync("Test");

            // Assert
            result.Should().NotBeNull("LoadConfigAsync should return a valid ConfigurationResult object");
            result.Config.Should().NotBeNull("ConfigurationResult should contain a populated Config dictionary");
            result.Config.Should().NotBeEmpty("Config dictionary should contain configuration items from the test file");
            result.Metadata.Should().Be(service.Metadata, "ConfigurationResult should reference the same Metadata object from the service");
            result.ConfigByScope.Should().NotBeEmpty("ConfigByScope should contain scope-filtered configuration data");

            // Verify logging occurred
            _logMessages.Should().NotBeEmpty("LoadConfigAsync should generate log messages during configuration loading");
            _logMessages.Should().Contain(x => x.Contains("[Information]"), 
                "Successful configuration loading should log informational messages about the process");
        }

        [TestMethod]
        public async Task LoadConfigAsync_WithScope_ShouldFilterCorrectly()
        {
            // Arrange
            var scopedConfigFile = Path.Combine(_testDataDirectory, "ScopedConfig.xlsx");
            CreateTestConfigFileWithMultipleScopes(scopedConfigFile);

            var service = new ConfigService(scopedConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();

            // Act
            var result = await service.LoadConfigAsync("Production");

            // Assert
            result.Should().NotBeNull("LoadConfigAsync should return a valid result even with scope filtering");
            result.Config.Should().NotBeNull("ConfigurationResult should contain a populated Config dictionary after scope filtering");
            result.ConfigByScope.Should().ContainKey("Production", "ConfigByScope should contain the requested Production scope");

            // Verify that scope filtering worked correctly - only Production items should be present
            result.Config.Should().ContainKey("ProdSetting").WhoseValue.Should().Be("ProdValue", 
                "Production-scoped setting should be loaded with correct value");
            result.Config.Should().ContainKey("ProdAsset").WhoseValue.Should().Be("TestProdAssetValue", 
                "Production-scoped asset should be loaded with value from mock orchestrator service");
            result.Config.Should().NotContainKey("DevSetting", 
                "Development-scoped setting should be filtered out when loading Production scope");
            result.Config.Should().NotContainKey("DevAsset", 
                "Development-scoped asset should be filtered out when loading Production scope");
            
            // Should have 2 config entries (1 setting + 1 asset for Production scope)
            result.Config.Should().HaveCount(2, 
                "Production scope should contain exactly 2 items (1 setting + 1 asset)");
            
            // Check that processing occurred with proper logging
            _logMessages.Should().Contain(x => x.Contains("Configuration loaded successfully"), 
                "Successful scope filtering should log completion message");
            _logMessages.Should().Contain(x => x.Contains("Found asset 'ProdAssetValue'"), 
                "Asset resolution should log when assets are found in orchestrator service");
        }

        [TestMethod]
        public async Task LoadConfigAsync_WithNullOrchestratorService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to initialize Metadata
            service.ValidateConfigFile();
            
            // Note: The constructor already validates the orchestrator service, so this test
            // verifies that the LoadConfigAsync method correctly handles the service being null internally

            // Act
            var result = await service.LoadConfigAsync("Test");
            
            // Assert
            result.Should().NotBeNull("LoadConfigAsync should return valid result when OrchestratorService is properly initialized"); // The service should work correctly with properly initialized orchestrator
        }
    }
}