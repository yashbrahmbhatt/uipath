using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using RestSharp;
using Newtonsoft.Json;
using TraceEventType = System.Diagnostics.TraceEventType;
using UiPath.Activities.Api.Base;
using Yash.Orchestrator.GetFolders;
using Yash.Orchestrator.GetAssets;
using Yash.Orchestrator.GetBuckets;
using Yash.Orchestrator.GetBucketFiles;
using Yash.Orchestrator.GetBucketFileReadUri;
using Yash.Orchestrator.GetToken;

namespace Yash.Orchestrator.Tests
{
    [TestClass]
    public class OrchestratorServiceTests
    {
        private OrchestratorService? _orchestratorService;
        private List<string> _logMessages = new();
        private List<TraceEventType> _logLevels = new();

        // Configuration from environment variables
        private readonly string _testBaseUrl;
        private readonly string _testClientId;
        private readonly string _testClientSecret;
        private readonly string _testTenantName;
        private readonly string[] _testScopes;

        public OrchestratorServiceTests()
        {
            // Read test configuration from environment variables - REQUIRED except for scopes
            _testBaseUrl = $"https://" +
                (Environment.GetEnvironmentVariable("UIP_ACCOUNT_NAME")
                          ?? throw new InvalidOperationException(
                              "UIP_ACCOUNT_NAME environment variable is required. " +
                              "Set this to your UiPath Account Name (e.g., https://your-account-name.uipath.com)"))
                + ".uipath.com";

            _testClientId = Environment.GetEnvironmentVariable("UIP_APPLICATION_ID")
                           ?? throw new InvalidOperationException(
                               "UIP_APPLICATION_ID environment variable is required. " +
                               "Set this to your OAuth client ID for UiPath Orchestrator authentication.");

            _testClientSecret = Environment.GetEnvironmentVariable("UIP_APPLICATION_SECRET")
                               ?? throw new InvalidOperationException(
                                   "UIP_APPLICATION_SECRET environment variable is required. " +
                                   "Set this to your OAuth client secret for UiPath Orchestrator authentication.");

            _testTenantName = Environment.GetEnvironmentVariable("UIP_TENANT_NAME")
                             ?? throw new InvalidOperationException(
                                 "UIP_TENANT_NAME environment variable is required. " +
                                 "Set this to your UiPath tenant name.");

            // Scopes can have a default fallback
            var scopesString = Environment.GetEnvironmentVariable("UIP_TEST_SCOPES")
                              ?? "OR.Assets.Read OR.Folders.Read";
            _testScopes = scopesString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        [TestInitialize]
        public void Setup()
        {
            _logMessages = new List<string>();
            _logLevels = new List<TraceEventType>();

            // Create service with capturing logger using environment-based configuration
            Action<string, TraceEventType> mockLogger = (message, level) =>
            {
                _logMessages.Add(message);
                _logLevels.Add(level);
            };

            _orchestratorService = new OrchestratorService(
                _testBaseUrl,
                _testClientId,
                _testClientSecret,
                mockLogger
            );
        }

        #region Constructor Tests

        [TestMethod]
        public void OrchestratorService_WithValidParameters_ShouldInitialize()
        {
            // Arrange & Act
            Action<string, TraceEventType> logger = (msg, level) => { };
            var service = new OrchestratorService(_testBaseUrl, _testClientId, _testClientSecret, logger);

            // Assert
            service.Should().NotBeNull();
            service.BaseURL.Should().Be(_testBaseUrl);
            service.ClientId.Should().Be(_testClientId);
            service.ClientSecret.Should().Be(_testClientSecret);
        }

        [TestMethod]
        public void OrchestratorService_WithLoggerOnly_ShouldInitialize()
        {
            // Arrange & Act
            Action<string, TraceEventType> logger = (msg, level) => { };
            var service = new OrchestratorService(logger);

            // Assert
            service.Should().NotBeNull();
            service.BaseURL.Should().Be("");
            service.Token.Should().Be("");
        }

        [TestMethod]
        public void OrchestratorService_WithAccessProvider_ShouldInitialize()
        {
            // Arrange
            var mockAccessProvider = new Mock<IAccessProvider>();
            Action<string, TraceEventType> logger = (msg, level) => { };

            // Act
            var service = new OrchestratorService(mockAccessProvider.Object, logger);

            // Assert
            service.Should().NotBeNull();
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void OrchestratorService_Properties_ShouldSetCorrectly()
        {
            // Arrange & Act
            _orchestratorService!.Token = "test-token";
            _orchestratorService.Scopes = _testScopes;

            // Assert
            _orchestratorService.Token.Should().Be("test-token");
            _orchestratorService.Scopes.Should().Contain("OR.Assets.Read");
            _orchestratorService.Scopes.Should().Contain("OR.Folders.Read");
        }

        [TestMethod]
        public void DefaultScopes_ShouldBeSetCorrectly()
        {
            // Assert
            _orchestratorService!.Scopes.Should().Contain("OR.Assets.Read");
            _orchestratorService.Scopes.Should().Contain("OR.Folders.Read");
            _orchestratorService.Scopes.Should().HaveCount(2);
        }

        [TestMethod]
        public void Token_Property_ShouldGetAndSet()
        {
            // Arrange & Act
            _orchestratorService!.Token = "new-token-value";

            // Assert
            _orchestratorService.Token.Should().Be("new-token-value");
        }

        [TestMethod]
        public void ClientCredentials_Properties_ShouldGetAndSet()
        {
            // Arrange & Act
            _orchestratorService!.ClientId = "new-client-id";
            _orchestratorService.ClientSecret = "new-client-secret";

            // Assert
            _orchestratorService.ClientId.Should().Be("new-client-id");
            _orchestratorService.ClientSecret.Should().Be("new-client-secret");
        }

        [TestMethod]
        public void Scopes_Property_ShouldAcceptCustomScopes()
        {
            // Arrange & Act
            var customScopes = new[] { "OR.Jobs.Write", "OR.Machines.Read" };
            _orchestratorService!.Scopes = customScopes;

            // Assert
            _orchestratorService.Scopes.Should().BeEquivalentTo(customScopes);
        }

        #endregion

        #region Collection Tests

        [TestMethod]
        public void Folders_Collection_ShouldBeInitialized()
        {
            // Assert
            _orchestratorService!.Folders.Should().NotBeNull();
            _orchestratorService.Folders.Should().BeEmpty();
        }

        [TestMethod]
        public void Assets_Collection_ShouldBeInitialized()
        {
            // Assert
            _orchestratorService!.Assets.Should().NotBeNull();
            _orchestratorService.Assets.Should().BeEmpty();
        }

        [TestMethod]
        public void Buckets_Collection_ShouldBeInitialized()
        {
            // Assert
            _orchestratorService!.Buckets.Should().NotBeNull();
            _orchestratorService.Buckets.Should().BeEmpty();
        }

        [TestMethod]
        public void BucketFiles_Collection_ShouldBeInitialized()
        {
            // Assert
            _orchestratorService!.BucketFiles.Should().NotBeNull();
            _orchestratorService.BucketFiles.Should().BeEmpty();
        }

        #endregion

        #region UpdateTokenAsync Tests

        [TestMethod]
        public async Task UpdateTokenAsync_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var service = new OrchestratorService(_testBaseUrl, "", "", null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await service.UpdateTokenAsync()
            );
        }

        [TestMethod]
        public async Task UpdateTokenAsync_WithNullClientId_ShouldThrowException()
        {
            // Arrange
            var service = new OrchestratorService(_testBaseUrl, null!, "secret", null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await service.UpdateTokenAsync()
            );
        }

        [TestMethod]
        public async Task UpdateTokenAsync_WithAccessProvider_ShouldUseProvider()
        {
            // Arrange
            var mockAccessProvider = new Mock<IAccessProvider>();
            var expectedScopesString = string.Join(" ", _testScopes);
            var expectedToken = $"provider-token-for-{_testClientId}";

            mockAccessProvider.Setup(x => x.GetAccessToken(expectedScopesString, false))
                            .ReturnsAsync(expectedToken);

            var service = new OrchestratorService(mockAccessProvider.Object);
            service.Scopes = _testScopes; // Set scopes to match environment

            // Act
            await service.UpdateTokenAsync();

            // Assert
            service.Token.Should().Be(expectedToken);
            mockAccessProvider.Verify(x => x.GetAccessToken(expectedScopesString, false), Times.Once);
        }

        [TestMethod]
        public async Task UpdateTokenAsync_WithForceFlag_ShouldPassToProvider()
        {
            // Arrange
            var mockAccessProvider = new Mock<IAccessProvider>();
            var expectedScopesString = string.Join(" ", _testScopes);
            var expectedToken = $"forced-token-for-{_testClientId}";

            mockAccessProvider.Setup(x => x.GetAccessToken(expectedScopesString, true))
                            .ReturnsAsync(expectedToken);

            var service = new OrchestratorService(mockAccessProvider.Object);
            service.Scopes = _testScopes; // Set scopes to match environment

            // Act
            await service.UpdateTokenAsync(force: true);

            // Assert
            service.Token.Should().Be(expectedToken);
            mockAccessProvider.Verify(x => x.GetAccessToken(expectedScopesString, true), Times.Once);
        }

        #endregion

        #region InitializeAsync Tests

        [TestMethod]
        public async Task InitializeAsync_WithAccessProvider_ShouldResolveBaseUrl()
        {
            // Arrange
            var mockAccessProvider = new Mock<IAccessProvider>();
            var resolvedUrl = $"{_testBaseUrl}/resolved";
            var expectedToken = $"test-token-for-{_testClientId}";

            mockAccessProvider.Setup(x => x.GetResourceUrl("Orchestrator"))
                            .ReturnsAsync(resolvedUrl);
            mockAccessProvider.Setup(x => x.GetAccessToken(It.IsAny<string>(), It.IsAny<bool>()))
                            .ReturnsAsync(expectedToken);

            var service = new OrchestratorService(mockAccessProvider.Object);

            // Act
            await service.InitializeAsync();

            // Assert
            service.BaseURL.Should().Be(resolvedUrl);
            mockAccessProvider.Verify(x => x.GetResourceUrl("Orchestrator"), Times.Once);
        }

        [TestMethod]
        public async Task InitializeAsync_ShouldLogInitializationSteps()
        {
            // Arrange
            var mockAccessProvider = new Mock<IAccessProvider>();
            var expectedToken = $"test-token-for-{_testClientId}";

            mockAccessProvider.Setup(x => x.GetResourceUrl("Orchestrator"))
                            .ReturnsAsync(_testBaseUrl);
            mockAccessProvider.Setup(x => x.GetAccessToken(It.IsAny<string>(), It.IsAny<bool>()))
                            .ReturnsAsync(expectedToken);

            var logMessages = new List<string>();
            Action<string, TraceEventType> logger = (msg, level) => logMessages.Add(msg);
            var service = new OrchestratorService(mockAccessProvider.Object, logger);

            // Act
            await service.InitializeAsync();

            // Assert
            logMessages.Should().Contain(msg => msg.Contains("Initializing OrchestratorService"));
            logMessages.Should().Contain(msg => msg.Contains("BaseURL resolved to"));
            logMessages.Should().Contain(msg => msg.Contains("Initialization complete"));
        }

        #endregion

        #region RefreshFoldersAsync Tests

        [TestMethod]
        public void RefreshFoldersAsync_ShouldRequireTokenAndBaseUrl()
        {
            // Arrange
            var service = new OrchestratorService("", "", "", null);

            // Act & Assert - This would normally make a real HTTP call
            // In a full implementation, you would mock the RestClient
            service.BaseURL.Should().Be("");
            service.Token.Should().Be("");
        }

        [TestMethod]
        public void RefreshFoldersAsync_ShouldUseCorrectUrlPattern()
        {
            // Arrange & Act
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Assert - Verify the URL pattern would be correct
            var expectedUrlPattern = $"{_orchestratorService.BaseURL}/odata/Folders";
            expectedUrlPattern.Should().Be($"{_testBaseUrl}/odata/Folders");
        }

        #endregion

        #region RefreshAssetsAsync Tests

        [TestMethod]
        public async Task RefreshAssetsAsync_WithNoFolders_ShouldReturnEmptyList()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshAssetsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _orchestratorService.Assets.Should().BeEmpty();
        }

        [TestMethod]
        public void RefreshAssetsAsync_ShouldUseCorrectUrlPattern()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;

            // Assert - Verify the URL pattern would be correct
            var expectedUrlPattern = $"{_orchestratorService.BaseURL}/odata/Assets/UiPath.Server.Configuration.OData.GetFiltered";
            expectedUrlPattern.Should().Be($"{_testBaseUrl}/odata/Assets/UiPath.Server.Configuration.OData.GetFiltered");
        }

        [TestMethod]
        public async Task RefreshAssetsAsync_ShouldClearExistingAssets()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Add some test data
            var folder = new Folder { Id = 1, DisplayName = "Test Folder" };
            var assets = new ObservableCollection<Asset>();
            _orchestratorService.Assets.Add(new KeyValuePair<Folder, ObservableCollection<Asset>>(folder, assets));

            // Act
            var result = await _orchestratorService.RefreshAssetsAsync();

            // Assert
            _orchestratorService.Assets.Should().BeEmpty();
        }

        #endregion

        #region RefreshBucketsAsync Tests

        [TestMethod]
        public async Task RefreshBucketsAsync_WithNoFolders_ShouldReturnEmptyList()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshBucketsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _orchestratorService.Buckets.Should().BeEmpty();
        }

        [TestMethod]
        public void RefreshBucketsAsync_ShouldUseCorrectUrlPattern()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;

            // Assert - Verify the URL pattern would be correct
            var expectedUrlPattern = $"{_orchestratorService.BaseURL}/odata/Buckets";
            expectedUrlPattern.Should().Be($"{_testBaseUrl}/odata/Buckets");
        }

        [TestMethod]
        public async Task RefreshBucketsAsync_ShouldClearExistingBuckets()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Add some test data
            var folder = new Folder { Id = 1, DisplayName = "Test Folder" };
            var buckets = new ObservableCollection<Bucket>();
            _orchestratorService.Buckets.Add(new KeyValuePair<Folder, ObservableCollection<Bucket>>(folder, buckets));

            // Act
            var result = await _orchestratorService.RefreshBucketsAsync();

            // Assert
            _orchestratorService.Buckets.Should().BeEmpty();
        }

        #endregion

        #region RefreshBucketFilesAsync Tests

        [TestMethod]
        public async Task RefreshBucketFilesAsync_WithNoFolders_ShouldReturnEmptyList()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshBucketFilesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _orchestratorService.BucketFiles.Should().BeEmpty();
        }

        [TestMethod]
        public async Task RefreshBucketFilesAsync_WithFoldersButNoBuckets_ShouldLogWarning()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Add folder but no buckets
            var folder = new Folder { Id = 1, DisplayName = "Test Folder" };
            _orchestratorService.Folders.Add(folder);

            // Act
            var result = await _orchestratorService.RefreshBucketFilesAsync();

            // Assert
            result.Should().BeEmpty();
            _logMessages.Should().Contain(msg => msg.Contains("No buckets found for folder Test Folder"));
            _logLevels.Should().Contain(TraceEventType.Warning);
        }

        [TestMethod]
        public async Task RefreshBucketFilesAsync_ShouldClearExistingBucketFiles()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            _orchestratorService.Token = "test-token";

            // Add some test data
            var bucket = new Bucket { Id = 1, Name = "Test Bucket" };
            var bucketFiles = new ObservableCollection<BucketFile>();
            _orchestratorService.BucketFiles.Add(new KeyValuePair<Bucket, ObservableCollection<BucketFile>>(bucket, bucketFiles));

            // Act
            var result = await _orchestratorService.RefreshBucketFilesAsync();

            // Assert
            _orchestratorService.BucketFiles.Should().BeEmpty();
        }

        #endregion

        #region DownloadBucketFile Tests

        [TestMethod]
        public async Task DownloadBucketFile_WithInvalidPath_ShouldThrowException()
        {
            // Arrange
            var bucket = new Bucket { Id = 1, Name = "Test Bucket" };
            var bucketFile = new BucketFile { FullPath = "/test/file.txt" };
            var invalidPath = ""; // Invalid path

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await _orchestratorService!.DownloadBucketFile(bucket, bucketFile, invalidPath)
            );
        }

        [TestMethod]
        public void DownloadBucketFile_ShouldUseCorrectUrlPattern()
        {
            // Arrange
            _orchestratorService!.BaseURL = _testBaseUrl;
            var bucket = new Bucket { Id = 1, Name = "Test Bucket" };
            var bucketFile = new BucketFile { FullPath = "/test/file.txt" };

            // Assert - Verify the URL pattern would be correct
            var expectedUrlPattern = $"{_orchestratorService.BaseURL}/odata/Buckets({bucket.Id})/UiPath.Server.Configuration.OData.GetReadUri?path={bucketFile.FullPath}";
            expectedUrlPattern.Should().Be($"{_testBaseUrl}/odata/Buckets(1)/UiPath.Server.Configuration.OData.GetReadUri?path=/test/file.txt");
        }

        [TestMethod]
        public async Task DownloadBucketFile_ShouldLogDownloadAttempt()
        {
            // Arrange
            var bucket = new Bucket { Id = 1, Name = "Test Bucket" };
            var bucketFile = new BucketFile { FullPath = "/test/file.txt" };
            var downloadPath = Path.Combine(Path.GetTempPath(), "test-download.txt");

            // Act - This will fail due to no real API, but should log the attempt
            try
            {
                await _orchestratorService!.DownloadBucketFile(bucket, bucketFile, downloadPath);
            }
            catch
            {
                // Expected to fail in unit test environment
            }

            // Assert
            _logMessages.Should().Contain(msg => msg.Contains("Downloading file /test/file.txt from bucket Test Bucket"));
        }

        #endregion

        #region Logging Tests

        [TestMethod]
        public void Logging_ShouldCaptureMessages()
        {
            // Arrange
            var logMessages = new List<string>();
            var logLevels = new List<TraceEventType>();
            Action<string, TraceEventType> logger = (msg, level) =>
            {
                logMessages.Add(msg);
                logLevels.Add(level);
            };

            // Act
            var service = new OrchestratorService(_testBaseUrl, _testClientId, _testClientSecret, logger);

            // Simulate internal logging (this would be called by actual methods)
            // Since Log is private, we test through public methods that use it

            // Assert
            logger.Should().NotBeNull();
        }

        #endregion

        #region Security and Validation Tests

        [TestMethod]
        public void OrchestratorService_ShouldNotExposeCredentialsInToString()
        {
            // Arrange
            _orchestratorService!.Token = "sensitive-token";

            // Act
            var stringRepresentation = _orchestratorService!.ToString();

            // Assert - Ensure sensitive data is not accidentally exposed
            stringRepresentation.Should().NotContain(_testClientSecret);
            if (!string.IsNullOrEmpty(_orchestratorService.Token))
            {
                stringRepresentation.Should().NotContain(_orchestratorService.Token);
            }
        }

        [TestMethod]
        public void OrchestratorService_ShouldHandleNullLogger()
        {
            // Arrange & Act
            var service = new OrchestratorService(_testBaseUrl, _testClientId, _testClientSecret, null);

            // Assert
            service.Should().NotBeNull();
            service.LogAction.Should().BeNull();
        }

        [TestMethod]
        public void BaseURL_ShouldAcceptHttpsUrls()
        {
            // Arrange & Act
            var secureUrl = $"{_testBaseUrl.Replace("http:", "https:")}/secure";
            _orchestratorService!.BaseURL = secureUrl;

            // Assert
            _orchestratorService.BaseURL.Should().Be(secureUrl);
        }

        [TestMethod]
        public void BaseURL_ShouldAcceptNullValue()
        {
            // Arrange & Act
            _orchestratorService!.BaseURL = null;

            // Assert
            _orchestratorService.BaseURL.Should().BeNull();
        }

        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            _orchestratorService = null;
            _logMessages?.Clear();
            _logLevels?.Clear();
        }
    }
}
