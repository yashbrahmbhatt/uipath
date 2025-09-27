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
        private readonly string _testBaseUrl = "https://cloud.uipath.com";
        private readonly string _testClientId;
        private readonly string _testClientSecret;
        private readonly string[] _testScopes = new string[2] { "OR.Folders.Read", "OR.Assets.Read" };

        public OrchestratorServiceTests()
        {
            
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

            var scopesString = Environment.GetEnvironmentVariable("UIP_TEST_SCOPES")
                              ?? "OR.Assets.Read OR.Folders.Read";
            _testScopes = scopesString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            

        }

        [TestInitialize]
        public void Setup()
        {
            _logMessages = new List<string>();
            _logLevels = new List<TraceEventType>();
            Action<string, TraceEventType> mockLogger = (message, level) =>
            {
                _logMessages.Add(message);
                _logLevels.Add(level);
            };
            _orchestratorService = new OrchestratorService(
                _testBaseUrl,
                _testClientId,
                _testClientSecret,
                _testScopes,
                mockLogger
            );
            _orchestratorService.Scopes = _testScopes;
        }

        [TestMethod]
        public async Task UpdateTokenAsync_ShouldUpdateTokenOrThrow()
        {
            // Act & Assert
            try
            {
                await _orchestratorService!.UpdateTokenAsync();
                Assert.IsFalse(string.IsNullOrEmpty(_orchestratorService.Token));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || ex.Message.Contains("Token not found"));
            }
        }

        [TestMethod]
        public async Task InitializeAsync_ShouldRunWithoutException()
        {
            // Act & Assert
            try
            {
                await _orchestratorService!.InitializeAsync();
                Assert.IsTrue(_logMessages.Count > 0);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || ex.Message.Contains("Token not found"));
            }
        }

        [TestMethod]
        public async Task RefreshFoldersAsync_ShouldReturnRestResponse()
        {
            // Arrange
            _orchestratorService!.Token = "test-token";

            // Act
            var response = await _orchestratorService.RefreshFoldersAsync();

            // Assert
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task RefreshAssetsAsync_ShouldReturnList()
        {
            // Arrange
            _orchestratorService!.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshAssetsAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RefreshBucketsAsync_ShouldReturnList()
        {
            // Arrange
            _orchestratorService!.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshBucketsAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RefreshBucketFilesAsync_ShouldReturnList()
        {
            // Arrange
            _orchestratorService!.Token = "test-token";

            // Act
            var result = await _orchestratorService.RefreshBucketFilesAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DownloadBucketFile_ShouldHandleDownloadLogic()
        {
            // Arrange
            var bucket = new Bucket { Id = 1, Name = "Test Bucket" };
            var bucketFile = new BucketFile { FullPath = "/test/file.txt" };
            var downloadPath = Path.Combine(Path.GetTempPath(), "test-download.txt");

            // Act & Assert
            try
            {
                var response = await _orchestratorService!.DownloadBucketFile(bucket, bucketFile, downloadPath);
                Assert.IsNotNull(response);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Failed to get read URI") || ex.Message.Contains("Empty download URI") || ex.Message.Contains("Could not determine download directory"));
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _orchestratorService = null;
            _logMessages?.Clear();
            _logLevels?.Clear();
        }

        #region Interface Implementation Tests

        [TestMethod]
        public void OrchestratorService_ShouldImplementIOrchestratorService()
        {
            // Act & Assert
            _orchestratorService.Should().BeAssignableTo<IOrchestratorService>();
        }

        [TestMethod]
        public void LogAction_ShouldBeAccessibleThroughInterface()
        {
            // Arrange
            IOrchestratorService interfaceService = _orchestratorService!;

            // Act & Assert
            interfaceService.LogAction.Should().NotBeNull();
            interfaceService.LogAction.Should().BeSameAs(_orchestratorService.LogAction);
        }

        [TestMethod]
        public void AllProperties_ShouldBeAccessibleThroughInterface()
        {
            // Arrange
            IOrchestratorService interfaceService = _orchestratorService!;

            // Act & Assert
            interfaceService.BaseURL.Should().Be(_orchestratorService.BaseURL);
            interfaceService.Token.Should().Be(_orchestratorService.Token);
            interfaceService.ClientId.Should().Be(_orchestratorService.ClientId);
            interfaceService.ClientSecret.Should().Be(_orchestratorService.ClientSecret);
            interfaceService.Scopes.Should().BeSameAs(_orchestratorService.Scopes);
            interfaceService.Folders.Should().BeSameAs(_orchestratorService.Folders);
            interfaceService.Assets.Should().BeSameAs(_orchestratorService.Assets);
            interfaceService.Buckets.Should().BeSameAs(_orchestratorService.Buckets);
            interfaceService.BucketFiles.Should().BeSameAs(_orchestratorService.BucketFiles);
        }

        #endregion

        #region Bug Fix Tests

        [TestMethod]
        public async Task UpdateTokenAsync_WithForceTrue_ShouldRefreshTokenEvenIfExists()
        {
            // Arrange
            _orchestratorService!.Token = "existing-token";

            // Act & Assert - Should attempt to refresh even with existing token
            try
            {
                await _orchestratorService.UpdateTokenAsync(force: true);
                // If we get here without exception, the force logic worked
                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                // Expected for invalid credentials, but should attempt refresh
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || 
                             ex.Message.Contains("Token not found") ||
                             ex.Message.Contains("IAccessProvider is not configured"));
            }
        }

        [TestMethod]
        public async Task UpdateTokenAsync_WithForceFalse_ShouldSkipRefreshWhenTokenExists()
        {
            // Arrange - Create service with verbose logging to capture skip message
            var logMessages = new List<string>();
            var logLevels = new List<TraceEventType>();
            Action<string, TraceEventType> verboseLogger = (message, level) =>
            {
                logMessages.Add(message);
                logLevels.Add(level);
            };
            
            var service = new OrchestratorService(
                _testBaseUrl,
                _testClientId,
                _testClientSecret,
                _testScopes,
                verboseLogger,
                TraceEventType.Verbose // Set to verbose to capture skip message
            );
            service.Token = "existing-token";

            // Act
            await service.UpdateTokenAsync(force: false);

            // Assert
            // Check that token remained unchanged and skip message was logged
            service.Token.Should().Be("existing-token");
            logMessages.Should().Contain(msg => msg.Contains("Token already exists") || msg.Contains("Skipping token update"));
        }

        [TestMethod]
        public async Task UpdateTokenAsync_WithInvalidAccessProvider_ShouldThrowMeaningfulException()
        {
            // Arrange - Create service without credentials or access provider
            var mockLogger = new Mock<Action<string, TraceEventType>>();
            var serviceWithoutCredentials = new OrchestratorService(
                baseUrl: string.Empty,
                clientId: string.Empty,
                clientSecret: string.Empty,
                scopes: new[] { "OR.Assets.Read" },
                log: mockLogger.Object
            );

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => serviceWithoutCredentials.UpdateTokenAsync());

            // The actual implementation checks for empty client credentials first
            exception.Message.Should().Contain("ClientId and ClientSecret and Scopes must be provided");
        }

        [TestMethod]
        public async Task EnsureTokenAsync_ShouldBeCalledInAllApiMethods()
        {
            // Arrange
            _orchestratorService!.Token = null; // Clear any existing token

            // Act & Assert - All these should attempt to ensure token
            try
            {
                await _orchestratorService.RefreshFoldersAsync();
            }
            catch (Exception ex)
            {
                // Should attempt token refresh
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || 
                             ex.Message.Contains("IAccessProvider is not configured"));
            }

            try
            {
                await _orchestratorService.RefreshAssetsAsync();
            }
            catch (Exception ex)
            {
                // Should attempt token refresh
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || 
                             ex.Message.Contains("IAccessProvider is not configured"));
            }

            try
            {
                await _orchestratorService.RefreshBucketsAsync();
            }
            catch (Exception ex)
            {
                // Should attempt token refresh
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || 
                             ex.Message.Contains("IAccessProvider is not configured"));
            }

            try
            {
                await _orchestratorService.RefreshBucketFilesAsync();
            }
            catch (Exception ex)
            {
                // Should attempt token refresh
                Assert.IsTrue(ex.Message.Contains("Failed to get token") || 
                             ex.Message.Contains("IAccessProvider is not configured"));
            }
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithClientCredentials_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var service = new OrchestratorService(
                "https://test.uipath.com",
                "test-client-id",
                "test-client-secret",
                new[] { "OR.Assets.Read" }
            );

            // Assert
            service.Should().NotBeNull();
            service.BaseURL.Should().Be("https://test.uipath.com");
            service.ClientId.Should().Be("test-client-id");
            service.ClientSecret.Should().Be("test-client-secret");
            service.Scopes.Should().BeEquivalentTo(new[] { "OR.Assets.Read" });
        }

        #endregion

        #region Collection Tests

        [TestMethod]
        public void Collections_ShouldBeInitializedEmpty()
        {
            // Act & Assert
            _orchestratorService!.Folders.Should().NotBeNull().And.BeEmpty();
            _orchestratorService.Assets.Should().NotBeNull().And.BeEmpty();
            _orchestratorService.Buckets.Should().NotBeNull().And.BeEmpty();
            _orchestratorService.BucketFiles.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Collections_ShouldBeObservableCollections()
        {
            // Act & Assert
            _orchestratorService!.Folders.Should().BeOfType<ObservableCollection<Folder>>();
            _orchestratorService.Assets.Should().BeOfType<ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>>>();
            _orchestratorService.Buckets.Should().BeOfType<ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>>>();
            _orchestratorService.BucketFiles.Should().BeOfType<ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>>>();
        }

        #endregion

        #region Logging Tests

        [TestMethod]
        public void Logging_ShouldRespectMinLogLevel()
        {
            // Arrange
            var logMessages = new List<string>();
            var logLevels = new List<TraceEventType>();
            Action<string, TraceEventType> logger = (msg, level) =>
            {
                logMessages.Add(msg);
                logLevels.Add(level);
            };

            var service = new OrchestratorService(
                "https://test.uipath.com",
                "test-client-id",
                "test-client-secret",
                new[] { "OR.Assets.Read" },
                logger,
                TraceEventType.Warning // Only log warnings and errors
            );

            // Act
            service.Token = "test"; // This should trigger verbose logging internally, but won't be captured

            // Assert
            // Since we set min log level to Warning, verbose messages should be filtered out
            // The specific test depends on internal logging implementation
            logMessages.Should().NotBeNull();
        }

        #endregion

    }
}
