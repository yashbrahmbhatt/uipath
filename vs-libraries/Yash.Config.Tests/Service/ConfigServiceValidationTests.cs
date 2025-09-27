using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
    /// Tests for ConfigService validation functionality including file validation, type determination, and format checking
    /// </summary>
    [TestClass]
    public class ConfigServiceValidationTests : ConfigServiceTestBase
    {
        #region ValidateConfigFile Tests

        [TestMethod]
        public void ValidateConfigFile_ValidFile_ShouldSetValidMetadata()
        {
            // Act
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            // Since ValidateConfigFile() is commented out, we need to call it manually to test validation
            service.ValidateConfigFile();

            // Assert
            service.Metadata.Should().NotBeNull("ValidateConfigFile should create Metadata object when processing valid file");
            service.Metadata.ConfigFileError.Should().BeNull("Valid Excel config file should not produce any validation errors");
            service.Metadata.FilePath.Should().Be(_testConfigFile, "Metadata should store the correct file path that was validated");
            service.Metadata.Sheets.Should().NotBeEmpty("Valid config file should contain at least one worksheet with configuration data");
            service.Metadata.ConfigFile.Should().NotBeNull("ValidateConfigFile should parse and populate ConfigFile object from Excel data");
            service.IsValid.Should().BeTrue("ConfigService should report valid status when config file has no errors");

            // Verify logging occurred
            _logMessages.Should().NotBeEmpty("ValidateConfigFile should generate log messages during processing");
            _logMessages.Should().Contain(x => x.Contains("[Information]"), 
                "Successful validation should log informational messages about the processing");
        }

        [TestMethod]
        public void ValidateConfigFile_FileNotFound_ShouldSetFileNotFoundError()
        {
            // Arrange
            var nonExistentFile = Path.Combine(_testDataDirectory, "NonExistent.xlsx");

            // Act
            var service = new ConfigService(nonExistentFile, _mockOrchestratorService.Object, CreateLogger());
            
            // Since ValidateConfigFile() is commented out, we need to call it manually to test validation
            service.ValidateConfigFile();

            // Assert
            service.Metadata.Should().NotBeNull("ValidateConfigFile should create Metadata even when file is not found");
            service.Metadata.ConfigFileError.Should().Be(ConfigFileError.FileNotFound, 
                "Validation should detect and report when the specified config file does not exist");
            service.IsValid.Should().BeFalse("ConfigService should report invalid status when config file is missing");

            // Verify error logging occurred
            _logMessages.Should().Contain(x => x.Contains("[Error]"), 
                "File not found scenario should generate error-level log messages");
        }

        [TestMethod]
        public void ValidateConfigFile_NotExcelFile_ShouldSetNotExcelFileError()
        {
            // Arrange
            var textFile = Path.Combine(_testDataDirectory, "NotExcel.txt");
            File.WriteAllText(textFile, "This is not an Excel file");

            // Act
            var service = new ConfigService(textFile, _mockOrchestratorService.Object, CreateLogger());
            
            // Since ValidateConfigFile() is commented out, we need to call it manually to test validation
            service.ValidateConfigFile();

            // Assert
            service.Metadata.Should().NotBeNull("ValidateConfigFile should create Metadata even when file format is invalid");
            service.Metadata.ConfigFileError.Should().Be(ConfigFileError.NotExcelFile, 
                "Validation should detect and report when file is not in Excel format (.xls/.xlsx)");
            service.IsValid.Should().BeFalse("ConfigService should report invalid status when file is not Excel format");

            // Verify error logging occurred
            _logMessages.Should().Contain(x => x.Contains("[Error]"), 
                "Non-Excel file scenario should generate error-level log messages about file format");
        }

        #endregion

        #region IsExcelFile Tests

        [TestMethod]
        public void IsExcelFile_XlsxFile_ShouldReturnTrue()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsExcelFile("test.xlsx").Should().BeTrue("Files with .xlsx extension should be recognized as Excel files");
            service.IsExcelFile("TEST.XLSX").Should().BeTrue("Excel file detection should be case-insensitive for .xlsx extension");
            service.IsExcelFile("path/to/file.xlsx").Should().BeTrue("Excel file detection should work with full file paths containing .xlsx extension");
        }

        [TestMethod]
        public void IsExcelFile_XlsFile_ShouldReturnTrue()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsExcelFile("test.xls").Should().BeTrue("Files with .xls extension should be recognized as Excel files");
            service.IsExcelFile("TEST.XLS").Should().BeTrue("Excel file detection should be case-insensitive for .xls extension");
            service.IsExcelFile("path/to/file.xls").Should().BeTrue("Excel file detection should work with full file paths containing .xls extension");
        }

        [TestMethod]
        public void IsExcelFile_NonExcelFile_ShouldReturnFalse()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsExcelFile("test.txt").Should().BeFalse("Text files should not be recognized as Excel files");
            service.IsExcelFile("test.docx").Should().BeFalse("Word documents should not be recognized as Excel files");
            service.IsExcelFile("test.pdf").Should().BeFalse("PDF files should not be recognized as Excel files");
            service.IsExcelFile("test").Should().BeFalse("Files without extensions should not be recognized as Excel files");
        }

        [TestMethod]
        public void IsExcelFile_EmptyOrNullPath_ShouldReturnFalse()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act & Assert
            service.IsExcelFile("").Should().BeFalse("Empty string should not be recognized as Excel file");
            service.IsExcelFile(null).Should().BeFalse("Null value should not be recognized as Excel file");
            service.IsExcelFile("   ").Should().BeFalse("Whitespace-only string should not be recognized as Excel file");
        }

        #endregion

        #region DetermineConfigType Tests

        [TestMethod]
        public void DetermineConfigType_SettingTable_ShouldReturnSetting()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateSettingDataTable();

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.Setting, 
                "DataTable with Setting columns (Name, Value, Type, Description, Scope) should be identified as Setting type");
        }

        [TestMethod]
        public void DetermineConfigType_AssetTable_ShouldReturnAsset()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateAssetDataTable();

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.Asset, 
                "DataTable with Asset columns (Name, Value, Folder, Type, Description, Scope) should be identified as Asset type");
        }

        [TestMethod]
        public void DetermineConfigType_FileTable_ShouldReturnFile()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateFileDataTable();

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.File, 
                "DataTable with File columns (Name, Path, Bucket, Folder, Type, Description, Scope, FileType) should be identified as File type");
        }

        [TestMethod]
        public void DetermineConfigType_NameValueTable_ShouldReturnNameValue()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateNameValueDataTable();

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.NameValue, 
                "DataTable with only Name and Value columns should be identified as NameValue type");
        }

        [TestMethod]
        public void DetermineConfigType_NameValueFolderTable_ShouldReturnNameValueFolder()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateNameValueFolderDataTable();

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.NameValueFolder, 
                "DataTable with Name, Value, and Folder columns should be identified as NameValueFolder type");
        }

        [TestMethod]
        public void DetermineConfigType_UnknownTable_ShouldReturnUnknown()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = new DataTable("Unknown");
            table.Columns.Add("SomeColumn", typeof(string));
            table.Columns.Add("AnotherColumn", typeof(int));

            // Act
            var result = service.DetermineConfigType(table);

            // Assert
            result.Should().Be(ConfigSheetType.Unknown, 
                "DataTable with unrecognized column structure should be identified as Unknown type");
        }

        #endregion

        #region HasPropertyHeaders Tests

        [TestMethod]
        public void HasPropertyHeaders_AllPropertiesPresent_ShouldReturnTrue()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = CreateSettingDataTable();

            // Act
            var result = service.HasPropertyHeaders(table, typeof(ConfigSettingItem));

            // Assert
            result.Should().BeTrue("DataTable with all required ConfigSettingItem properties should return true for header validation");
        }

        [TestMethod]
        public void HasPropertyHeaders_MissingProperties_ShouldReturnFalse()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var table = new DataTable("Test");
            table.Columns.Add("Name", typeof(string));
            // Missing other required properties

            // Act
            var result = service.HasPropertyHeaders(table, typeof(ConfigSettingItem));

            // Assert
            result.Should().BeFalse("DataTable missing required ConfigSettingItem properties should return false for header validation");
        }

        #endregion
    }
}