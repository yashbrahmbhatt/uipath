using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UiPath.Activities.Api.Base;
using Yash.Config.Services;
using Yash.Config.ConfigurationFile;
using Yash.Orchestrator;
using Yash.Orchestrator.GetFolders;
using Yash.Orchestrator.GetAssets;
using Yash.Orchestrator.GetBuckets;
using Yash.Orchestrator.GetBucketFiles;
using Yash.Utility.Services;
using OfficeOpenXml;
using Yash.Config.Models.Config;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Base class for ConfigService tests containing shared setup, helper methods, and common functionality
    /// </summary>
    [TestClass]
    public abstract class ConfigServiceTestBase
    {
        protected string _testDataDirectory = string.Empty;
        protected string _testConfigFile = string.Empty;
        protected string _tempConfigFile = string.Empty;
        protected Mock<IOrchestratorService> _mockOrchestratorService = new();
        protected Mock<IAccessProvider> _mockAccessProvider = new();

        // Environment variables for integration tests
        protected readonly string? _testBaseUrl;
        protected readonly string? _testClientId;
        protected readonly string? _testClientSecret;
        protected readonly string[] _testScopes;
        protected readonly List<string> _logMessages = new();

        protected ConfigServiceTestBase()
        {
            // Read environment variables for integration tests
            _testClientId = Environment.GetEnvironmentVariable("UIP_APPLICATION_ID");
            _testClientSecret = Environment.GetEnvironmentVariable("UIP_APPLICATION_SECRET");
            var accountName = Environment.GetEnvironmentVariable("UIP_ACCOUNT_NAME");
            var tenantName = Environment.GetEnvironmentVariable("UIP_TENANT_NAME");
            _testBaseUrl = $"https://{accountName}.uipath.com/{tenantName}/{tenantName}/orchestrator_/";
            var scopesEnv = Environment.GetEnvironmentVariable("UIP_TEST_SCOPES");
            _testScopes = !string.IsNullOrEmpty(scopesEnv) 
                ? scopesEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : new[] { "OR.Assets.Read", "OR.Folders.Read" };
        }

        [TestInitialize]
        public virtual void Setup()
        {
            // Set EPPlus license context for tests
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            _testDataDirectory = Path.Combine(TestContext?.TestDeploymentDir ?? ".", "TestData");
            _testConfigFile = Path.Combine(_testDataDirectory, "TestConfig.xlsx");
            _tempConfigFile = Path.Combine(_testDataDirectory, "TempTestConfig.xlsx");

            // Create test data directory if it doesn't exist
            if (!Directory.Exists(_testDataDirectory))
            {
                Directory.CreateDirectory(_testDataDirectory);
            }

            // Create a test config file
            CreateTestConfigFile(_testConfigFile);

            // Setup mock services
            SetupMockOrchestratorService();
            SetupMockAccessProvider();
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            try
            {
                if (File.Exists(_testConfigFile))
                    File.Delete(_testConfigFile);
                
                if (File.Exists(_tempConfigFile))
                    File.Delete(_tempConfigFile);

                if (Directory.Exists(_testDataDirectory))
                {
                    var files = Directory.GetFiles(_testDataDirectory, "*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Helper Methods

        protected void CreateTestConfigFile(string filePath)
        {
            using var package = new ExcelPackage();
            
            // Create Settings sheet
            var settingsSheet = package.Workbook.Worksheets.Add("Settings");
            settingsSheet.Cells[1, 1].Value = "Name";
            settingsSheet.Cells[1, 2].Value = "Value";
            settingsSheet.Cells[1, 3].Value = "Type";
            settingsSheet.Cells[1, 4].Value = "Description";
            settingsSheet.Cells[1, 5].Value = "Scope";
            
            settingsSheet.Cells[2, 1].Value = "TestSetting1";
            settingsSheet.Cells[2, 2].Value = "Value1";
            settingsSheet.Cells[2, 3].Value = "string";
            settingsSheet.Cells[2, 4].Value = "Test setting 1";
            settingsSheet.Cells[2, 5].Value = "Test";

            // Create Assets sheet
            var assetsSheet = package.Workbook.Worksheets.Add("Assets");
            assetsSheet.Cells[1, 1].Value = "Name";
            assetsSheet.Cells[1, 2].Value = "Value";
            assetsSheet.Cells[1, 3].Value = "Folder";
            assetsSheet.Cells[1, 4].Value = "Type";
            assetsSheet.Cells[1, 5].Value = "Description";
            assetsSheet.Cells[1, 6].Value = "Scope";
            
            assetsSheet.Cells[2, 1].Value = "TestAsset1";
            assetsSheet.Cells[2, 2].Value = "AssetValue1";
            assetsSheet.Cells[2, 3].Value = "TestFolder";
            assetsSheet.Cells[2, 4].Value = "string";
            assetsSheet.Cells[2, 5].Value = "Test asset 1";
            assetsSheet.Cells[2, 6].Value = "Test";

            // Create Files sheet
            var filesSheet = package.Workbook.Worksheets.Add("Files");
            filesSheet.Cells[1, 1].Value = "Name";
            filesSheet.Cells[1, 2].Value = "Path";
            filesSheet.Cells[1, 3].Value = "Bucket";
            filesSheet.Cells[1, 4].Value = "Folder";
            filesSheet.Cells[1, 5].Value = "Type";
            filesSheet.Cells[1, 6].Value = "Description";
            filesSheet.Cells[1, 7].Value = "Scope";
            
            filesSheet.Cells[2, 1].Value = "TestFile1";
            filesSheet.Cells[2, 2].Value = Path.Combine(_testDataDirectory, "test.txt");
            filesSheet.Cells[2, 3].Value = "TestBucket";
            filesSheet.Cells[2, 4].Value = "TestFolder";
            filesSheet.Cells[2, 5].Value = "string";
            filesSheet.Cells[2, 6].Value = "Test file 1";
            filesSheet.Cells[2, 7].Value = "Test";

            package.SaveAs(new FileInfo(filePath));
            
            // Create the test file that is referenced
            var testFilePath = Path.Combine(_testDataDirectory, "test.txt");
            File.WriteAllText(testFilePath, "This is a test file content");
        }

        protected void SetupMockOrchestratorService()
        {
            var testFolder = new Folder { DisplayName = "TestFolder" };
            var prodFolder = new Folder { DisplayName = "ProdFolder" };
            var devFolder = new Folder { DisplayName = "DevFolder" };
            
            var testAsset = new Asset { Name = "AssetValue1", Value = "TestAssetValue" };
            var prodAsset = new Asset { Name = "ProdAssetValue", Value = "TestProdAssetValue" };
            var devAsset = new Asset { Name = "DevAssetValue", Value = "TestDevAssetValue" };
            
            var testBucket = new Bucket { Name = "TestBucket" };
            var testBucketFile = new BucketFile { FullPath = "test.txt" };

            var mockAssets = new ObservableCollection<KeyValuePair<Folder, ObservableCollection<Asset>>>
            {
                new KeyValuePair<Folder, ObservableCollection<Asset>>(
                    testFolder,
                    new ObservableCollection<Asset> { testAsset }
                ),
                new KeyValuePair<Folder, ObservableCollection<Asset>>(
                    prodFolder,
                    new ObservableCollection<Asset> { prodAsset }
                ),
                new KeyValuePair<Folder, ObservableCollection<Asset>>(
                    devFolder,
                    new ObservableCollection<Asset> { devAsset }
                )
            };

            var mockBuckets = new ObservableCollection<KeyValuePair<Folder, ObservableCollection<Bucket>>>
            {
                new KeyValuePair<Folder, ObservableCollection<Bucket>>(
                    testFolder,
                    new ObservableCollection<Bucket> { testBucket }
                )
            };

            var mockBucketFiles = new ObservableCollection<KeyValuePair<Bucket, ObservableCollection<BucketFile>>>
            {
                new KeyValuePair<Bucket, ObservableCollection<BucketFile>>(
                    testBucket,
                    new ObservableCollection<BucketFile> { testBucketFile }
                )
            };

            _mockOrchestratorService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            _mockOrchestratorService.Setup(x => x.Assets).Returns(mockAssets);
            _mockOrchestratorService.Setup(x => x.Buckets).Returns(mockBuckets);
            _mockOrchestratorService.Setup(x => x.BucketFiles).Returns(mockBucketFiles);
        }

        protected void SetupMockAccessProvider()
        {
            _mockAccessProvider.Setup(x => x.GetResourceUrl("Orchestrator"))
                .Returns(Task.FromResult("https://test.orchestrator.com"));
        }

        protected ConfigFile CreateTestConfigFileWithoutFiles()
        {
            return new ConfigFile
            {
                Settings = new List<ConfigSettingItem>
                {
                    new ConfigSettingItem { Name = "TestSetting", Value = "TestValue", Type = "string" }
                },
                Assets = new List<ConfigAssetItem>
                {
                    new ConfigAssetItem { Name = "TestAsset", Value = "AssetValue1", Folder = "TestFolder", Type = "string" }
                },
                Files = new List<ConfigFileItem>() // Empty files list to avoid path issues
            };
        }

        protected ConfigFile CreateTestConfigFileModel()
        {
            return new ConfigFile
            {
                Settings = new List<ConfigSettingItem>
                {
                    new ConfigSettingItem { Name = "TestSetting", Value = "TestValue", Type = "string" }
                },
                Assets = new List<ConfigAssetItem>
                {
                    new ConfigAssetItem { Name = "TestAsset", Value = "AssetValue1", Folder = "TestFolder", Type = "string" }
                },
                Files = new List<ConfigFileItem>
                {
                    new ConfigFileItem { Name = "TestFile", Path = Path.Combine(_testDataDirectory, "test.txt"), Type = "string" }
                }
            };
        }

        protected DataTable CreateSettingDataTable()
        {
            var table = new DataTable("Settings");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Scope", typeof(string));
            return table;
        }

        protected DataTable CreateAssetDataTable()
        {
            var table = new DataTable("Assets");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.Columns.Add("Folder", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Scope", typeof(string));
            return table;
        }

        protected DataTable CreateFileDataTable()
        {
            var table = new DataTable("Files");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Path", typeof(string));
            table.Columns.Add("Bucket", typeof(string));
            table.Columns.Add("Folder", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Scope", typeof(string));
            table.Columns.Add("FileType", typeof(string));
            return table;
        }

        protected DataTable CreateNameValueDataTable()
        {
            var table = new DataTable("NameValue");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            return table;
        }

        protected DataTable CreateNameValueFolderDataTable()
        {
            var table = new DataTable("NameValueFolder");
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Value", typeof(string));
            table.Columns.Add("Folder", typeof(string));
            return table;
        }

        protected void CreateTestConfigFileWithMultipleScopes(string filePath)
        {
            using var package = new ExcelPackage();
            
            // Create Settings sheet with multiple scopes
            var settingsSheet = package.Workbook.Worksheets.Add("Settings");
            settingsSheet.Cells[1, 1].Value = "Name";
            settingsSheet.Cells[1, 2].Value = "Value";
            settingsSheet.Cells[1, 3].Value = "Type";
            settingsSheet.Cells[1, 4].Value = "Description";
            settingsSheet.Cells[1, 5].Value = "Scope";
            
            settingsSheet.Cells[2, 1].Value = "ProdSetting";
            settingsSheet.Cells[2, 2].Value = "ProdValue";
            settingsSheet.Cells[2, 3].Value = "string";
            settingsSheet.Cells[2, 4].Value = "Production setting";
            settingsSheet.Cells[2, 5].Value = "Production";

            settingsSheet.Cells[3, 1].Value = "DevSetting";
            settingsSheet.Cells[3, 2].Value = "DevValue";
            settingsSheet.Cells[3, 3].Value = "string";
            settingsSheet.Cells[3, 4].Value = "Development setting";
            settingsSheet.Cells[3, 5].Value = "Development";

            // Create Assets sheet with multiple scopes
            var assetsSheet = package.Workbook.Worksheets.Add("Assets");
            assetsSheet.Cells[1, 1].Value = "Name";
            assetsSheet.Cells[1, 2].Value = "Value";
            assetsSheet.Cells[1, 3].Value = "Folder";
            assetsSheet.Cells[1, 4].Value = "Type";
            assetsSheet.Cells[1, 5].Value = "Description";
            assetsSheet.Cells[1, 6].Value = "Scope";
            
            assetsSheet.Cells[2, 1].Value = "ProdAsset";
            assetsSheet.Cells[2, 2].Value = "ProdAssetValue";
            assetsSheet.Cells[2, 3].Value = "ProdFolder";
            assetsSheet.Cells[2, 4].Value = "string";
            assetsSheet.Cells[2, 5].Value = "Production asset";
            assetsSheet.Cells[2, 6].Value = "Production";

            assetsSheet.Cells[3, 1].Value = "DevAsset";
            assetsSheet.Cells[3, 2].Value = "DevAssetValue";
            assetsSheet.Cells[3, 3].Value = "DevFolder";
            assetsSheet.Cells[3, 4].Value = "string";
            assetsSheet.Cells[3, 5].Value = "Development asset";
            assetsSheet.Cells[3, 6].Value = "Development";

            package.SaveAs(new FileInfo(filePath));
        }

        protected void CreateTestConfigFileWithOrchestratorAssets(string filePath)
        {
            using var package = new ExcelPackage();
            
            // Create Settings sheet with a simple setting
            var settingsSheet = package.Workbook.Worksheets.Add("Settings");
            settingsSheet.Cells[1, 1].Value = "Name";
            settingsSheet.Cells[1, 2].Value = "Value";
            settingsSheet.Cells[1, 3].Value = "Type";
            settingsSheet.Cells[1, 4].Value = "Description";
            settingsSheet.Cells[1, 5].Value = "Scope";
            
            settingsSheet.Cells[2, 1].Value = "TestIntegrationSetting";
            settingsSheet.Cells[2, 2].Value = "IntegrationValue";
            settingsSheet.Cells[2, 3].Value = "string";
            settingsSheet.Cells[2, 4].Value = "Integration test setting";
            settingsSheet.Cells[2, 5].Value = "Test";

            // Create Assets sheet that will attempt to load from real Orchestrator
            // Note: These assets may or may not exist in the target Orchestrator
            var assetsSheet = package.Workbook.Worksheets.Add("Assets");
            assetsSheet.Cells[1, 1].Value = "Name";
            assetsSheet.Cells[1, 2].Value = "Value";
            assetsSheet.Cells[1, 3].Value = "Folder";
            assetsSheet.Cells[1, 4].Value = "Type";
            assetsSheet.Cells[1, 5].Value = "Description";
            assetsSheet.Cells[1, 6].Value = "Scope";
            
            // Add some common asset names that might exist in a test Orchestrator
            assetsSheet.Cells[2, 1].Value = "TestAsset";
            assetsSheet.Cells[2, 2].Value = "TestAssetValue";  // This will be looked up from Orchestrator
            assetsSheet.Cells[2, 3].Value = "Shared";         // Common folder name
            assetsSheet.Cells[2, 4].Value = "string";
            assetsSheet.Cells[2, 5].Value = "Test asset from Orchestrator";
            assetsSheet.Cells[2, 6].Value = "Test";

            assetsSheet.Cells[3, 1].Value = "CommonAsset";
            assetsSheet.Cells[3, 2].Value = "CommonAssetValue";
            assetsSheet.Cells[3, 3].Value = "Shared";
            assetsSheet.Cells[3, 4].Value = "string";
            assetsSheet.Cells[3, 5].Value = "Common asset from Orchestrator";
            assetsSheet.Cells[3, 6].Value = "Test";

            package.SaveAs(new FileInfo(filePath));
        }

        #endregion

        #region Integration Test Helper Methods

        protected Action<string, TraceEventType> CreateLogger()
        {
            return (message, level) =>
            {
                Console.WriteLine(message);
                _logMessages.Add($"[{level}] {message}");
            };
        }

        protected bool AreEnvironmentVariablesAvailable()
        {
            return !string.IsNullOrEmpty(_testBaseUrl) &&
                   !string.IsNullOrEmpty(_testClientId) &&
                   !string.IsNullOrEmpty(_testClientSecret);
        }

        protected ConfigService CreateRealConfigService()
        {
            if (!AreEnvironmentVariablesAvailable())
            {
                throw new InvalidOperationException("Environment variables not configured for integration tests");
            }

            return new ConfigService(
                _testConfigFile,
                _testBaseUrl!,
                _testClientId!,
                _testClientSecret!,
                _testScopes,
                CreateLogger()
            );
        }

        #endregion

        public TestContext? TestContext { get; set; }
    }
}