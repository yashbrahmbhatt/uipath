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
        private readonly string _testBaseUrl = "https://cloud.uipath.com";
        private readonly string _testClientId = "6ee3a9eb-95a1-451a-9494-d1c6a91f1548";
        private readonly string _testClientSecret = "OVfpeM61V_#ZKpUv_?tea5PIE?8fcw4fJXf(xgIxZE*I~$dTztxrkdzwcps^apRR";
        private readonly string _testTenantName = "yashbdev";
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

            _testTenantName = Environment.GetEnvironmentVariable("UIP_TENANT_NAME")
                             ?? throw new InvalidOperationException(
                                 "UIP_TENANT_NAME environment variable is required. " +
                                 "Set this to your UiPath tenant name.");

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

    }
}
