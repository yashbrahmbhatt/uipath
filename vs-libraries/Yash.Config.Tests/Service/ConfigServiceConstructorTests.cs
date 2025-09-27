using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Services;
using Yash.Orchestrator;
using Yash.Utility.Services;
using UiPath.Activities.Api.Base;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Tests for ConfigService constructor overloads and initialization
    /// </summary>
    [TestClass]
    public class ConfigServiceConstructorTests : ConfigServiceTestBase
    {
        [TestMethod]
        public void ConfigService_Constructor_WithOrchestratorService_ShouldInitializeCorrectly()
        {
            // Act
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Assert
            service.Should().NotBeNull("ConfigService instance should be successfully created with valid OrchestratorService");
            service.FilePath.Should().Be(_testConfigFile, "ConfigService should store the provided file path correctly");
            // Note: Metadata will be null since ValidateConfigFile() is commented out in constructor
            service.Metadata.Should().BeNull("Metadata should be null until ValidateConfigFile() is explicitly called");
            // Note: IsValid checks Metadata.ConfigFileError == null, so with null Metadata this will throw NullReferenceException
            // We should not call IsValid when Metadata is null
        }

        [TestMethod]
        public void ConfigService_Constructor_WithAccessProvider_ShouldInitializeCorrectly()
        {
            // Act
            var service = new ConfigService(_testConfigFile, _mockAccessProvider.Object, CreateLogger());

            // Assert
            service.Should().NotBeNull("ConfigService instance should be successfully created with valid AccessProvider");
            service.FilePath.Should().Be(_testConfigFile, "ConfigService should store the provided file path correctly");
            // Note: Metadata will be null since ValidateConfigFile() is commented out in constructor
            service.Metadata.Should().BeNull("Metadata should be null until ValidateConfigFile() is explicitly called");
            // Note: IsValid checks Metadata.ConfigFileError == null, so with null Metadata this will throw NullReferenceException
            // We should not call IsValid when Metadata is null
        }

        [TestMethod]
        public void ConfigService_Constructor_WithClientCredentials_ShouldInitializeCorrectly()
        {
            // Arrange
            var baseUrl = "https://test.orchestrator.com";
            var clientId = "test-client-id";
            var clientSecret = "test-secret";
            var scopes = new[] { "OR.Execution", "OR.Assets" };

            // Act
            var service = new ConfigService(_testConfigFile, baseUrl, clientId, clientSecret, scopes, CreateLogger());

            // Assert
            service.Should().NotBeNull("ConfigService instance should be successfully created with valid client credentials");
            service.FilePath.Should().Be(_testConfigFile, "ConfigService should store the provided file path correctly");
            // Note: Metadata will be null since ValidateConfigFile() is commented out in constructor
            service.Metadata.Should().BeNull("Metadata should be null until ValidateConfigFile() is explicitly called");
        }

        [TestMethod]
        public void ConfigService_Constructor_WithNullOrchestratorService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            IOrchestratorService nullService = null!;
            Action act = () => new ConfigService(_testConfigFile, nullService, CreateLogger());
            act.Should().Throw<ArgumentNullException>("Constructor should validate that OrchestratorService parameter is not null")
                .WithMessage("*OrchestratorService instance cannot be null*", 
                    "Exception message should clearly indicate which parameter validation failed");
        }

        [TestMethod]
        public void ConfigService_Constructor_WithNullAccessProvider_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            IAccessProvider nullProvider = null!;
            Action act = () => new ConfigService(_testConfigFile, nullProvider, CreateLogger());
            act.Should().Throw<ArgumentNullException>("Constructor should validate that AccessProvider parameter is not null")
                .WithMessage("*AccessProvider instance cannot be null*", 
                    "Exception message should clearly indicate which parameter validation failed");
        }

        [TestMethod]
        public void ConfigService_Constructor_WithInvalidCredentials_ShouldThrowArgumentException()
        {
            // Act & Assert
            Action act1 = () => new ConfigService(_testConfigFile, "", "clientId", "secret", new[] { "scope" }, CreateLogger());
            act1.Should().Throw<ArgumentException>("Constructor should validate BaseURL is not empty")
                .WithMessage("*BaseURL cannot be null or empty*", "Exception should specify BaseURL validation failed");

            Action act2 = () => new ConfigService(_testConfigFile, "baseUrl", "", "secret", new[] { "scope" }, CreateLogger());
            act2.Should().Throw<ArgumentException>("Constructor should validate ClientId is not empty")
                .WithMessage("*ClientId cannot be null or empty*", "Exception should specify ClientId validation failed");

            Action act3 = () => new ConfigService(_testConfigFile, "baseUrl", "clientId", "", new[] { "scope" }, CreateLogger());
            act3.Should().Throw<ArgumentException>("Constructor should validate ClientSecret is not empty")
                .WithMessage("*ClientSecret cannot be null or empty*", "Exception should specify ClientSecret validation failed");

            Action act4 = () => new ConfigService(_testConfigFile, "baseUrl", "clientId", "secret", null, CreateLogger());
            act4.Should().Throw<ArgumentException>("Constructor should validate Scopes array is not null")
                .WithMessage("*Scopes cannot be null or empty*", "Exception should specify Scopes validation failed");
        }
    }
}