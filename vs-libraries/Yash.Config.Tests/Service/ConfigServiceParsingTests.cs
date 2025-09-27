using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Config.Services;
using Newtonsoft.Json.Linq;

namespace Yash.Config.Tests.Service
{
    /// <summary>
    /// Tests for ConfigService parsing functionality for different file types
    /// </summary>
    [TestClass]
    public class ConfigServiceParsingTests : ConfigServiceTestBase
    {
        [TestMethod]
        public async Task ParseConfigFileItemAsync_CsvType_ShouldParseCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var csvFile = Path.Combine(_testDataDirectory, "test.csv");
            File.WriteAllText(csvFile, "Name,Value\nTest1,Value1\nTest2,Value2");

            // Act
            var result = await service.ParseConfigFileItemAsync(csvFile, "csv", "TestData");

            // Assert
            result.Should().NotBeNull("ParseConfigFileItemAsync should return a valid result when parsing CSV files");
            result.Should().BeOfType<DataTable>("CSV files should be parsed into DataTable objects for tabular data representation");
            
            var dataTable = (DataTable)result;
            dataTable.Rows.Count.Should().Be(2, "CSV parser should correctly identify and parse 2 data rows from the test CSV file");
            dataTable.Columns.Count.Should().Be(2, "CSV parser should correctly identify and parse 2 columns (Name, Value) from the test CSV file");
        }

        [TestMethod]
        public async Task ParseConfigFileItemAsync_XlsxType_ShouldParseCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());

            // Act
            var result = await service.ParseConfigFileItemAsync(_testConfigFile, "xlsx", "TestData");

            // Assert
            result.Should().NotBeNull("ParseConfigFileItemAsync should return a valid result when parsing Excel files");
            result.Should().BeOfType<DataSet>("Excel files should be parsed into DataSet objects to accommodate multiple worksheets");
            
            var dataSet = (DataSet)result;
            dataSet.Tables.Count.Should().BeGreaterThan(0, "Excel file parser should extract at least one worksheet from the test Excel file");
        }

        [TestMethod]
        public async Task ParseConfigFileItemAsync_JsonType_ShouldParseCorrectly()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var jsonFile = Path.Combine(_testDataDirectory, "test.json");
            File.WriteAllText(jsonFile, "{\"key\": \"value\", \"number\": 123}");

            // Act
            var result = await service.ParseConfigFileItemAsync(jsonFile, "json", "TestData");

            // Assert
            result.Should().NotBeNull("ParseConfigFileItemAsync should return a valid result when parsing JSON files");
            result.Should().BeOfType<JObject>("JSON files should be parsed into JObject for structured data access");
        }

        [TestMethod]
        public async Task ParseConfigFileItemAsync_PlainTextType_ShouldReturnTextContent()
        {
            // Arrange
            var service = new ConfigService(_testConfigFile, _mockOrchestratorService.Object, CreateLogger());
            var testFile = Path.Combine(_testDataDirectory, "test.txt");
            File.WriteAllText(testFile, "test content");

            // Act
            var result = await service.ParseConfigFileItemAsync(testFile, "txt", "TestData");

            // Assert
            result.Should().NotBeNull("ParseConfigFileItemAsync should return a valid result when parsing text files");
            result.Should().BeOfType<string>("Plain text files should be parsed and returned as string content");
            result.Should().Be("test content", "Text file parser should return the exact content of the file");
        }
    }
}