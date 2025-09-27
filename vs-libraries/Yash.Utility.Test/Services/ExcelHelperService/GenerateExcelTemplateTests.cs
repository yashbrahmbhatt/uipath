using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class GenerateExcelTemplateTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void GenerateExcelTemplate_ShouldCreateTemplateWithAllSheets()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "template.xlsx");
            var supportedTypes = new[] { "string", "int", "bool", "DateTime", "decimal" };

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var dataSet = _service.ReadExcelFile(outputPath);
            var sheetNames = dataSet.Tables.Cast<DataTable>().Select(t => t.TableName).ToArray();
            
            sheetNames.Should().Contain("_Instructions");
            sheetNames.Should().Contain("_ConfigFileSettings");
            sheetNames.Should().Contain("Settings");
            sheetNames.Should().Contain("Assets");
            sheetNames.Should().Contain("Files");
        }

        [TestMethod]
        public void GenerateExcelTemplate_ShouldIncludeInstructionsContent()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "instructions_template.xlsx");
            var supportedTypes = new[] { "string", "int" };

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            var dataSet = _service.ReadExcelFile(outputPath);
            var instructionsSheet = dataSet.Tables["_Instructions"];
            
            // The first cell content becomes a column header when read back
            var firstColumnName = instructionsSheet.Columns[0].ColumnName;
            firstColumnName.Should().Contain("Welcome to the Config Template");
        }

        [TestMethod]
        public void GenerateExcelTemplate_ShouldIncludeSupportedTypes()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "types_template.xlsx");
            var supportedTypes = new[] { "string", "int", "bool", "DateTime" };

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            var dataSet = _service.ReadExcelFile(outputPath);
            var configSheet = dataSet.Tables["_ConfigFileSettings"];
            
            configSheet.Rows.Should().HaveCount(supportedTypes.Length);
            for (int i = 0; i < supportedTypes.Length; i++)
            {
                configSheet.Rows[i][0].ToString().Should().Be(supportedTypes[i]);
            }
        }

        [TestMethod]
        public void GenerateExcelTemplate_ConfigSheets_ShouldHaveCorrectHeaders()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "headers_template.xlsx");
            var supportedTypes = new[] { "string" };

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            var dataSet = _service.ReadExcelFile(outputPath);
            var expectedHeaders = new[] { "Name", "Value / Path", "Type", "Description" };
            
            foreach (var sheetName in new[] { "Settings", "Assets", "Files" })
            {
                var sheet = dataSet.Tables[sheetName];
                sheet.Columns.Should().HaveCount(4);
                
                for (int i = 0; i < expectedHeaders.Length; i++)
                {
                    sheet.Columns[i].ColumnName.Should().Be(expectedHeaders[i]);
                }
            }
        }

        [TestMethod]
        public void GenerateExcelTemplate_ShouldLogTemplateCreation()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "log_template.xlsx");
            var supportedTypes = new[] { "string" };

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Creating Excel template") && s.Contains(outputPath)),
                    TraceEventType.Information),
                Times.Once);

            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Excel template saved to") && s.Contains(outputPath)),
                    TraceEventType.Information),
                Times.Once);
        }

        [TestMethod]
        public void GenerateExcelTemplate_WithEmptyTypes_ShouldStillCreateTemplate()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "empty_types_template.xlsx");
            var supportedTypes = new string[0];

            // Act
            _service.GenerateExcelTemplate(outputPath, supportedTypes);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            var dataSet = _service.ReadExcelFile(outputPath);
            var configSheet = dataSet.Tables["_ConfigFileSettings"];
            configSheet.Rows.Should().HaveCount(0);
        }
    }
}