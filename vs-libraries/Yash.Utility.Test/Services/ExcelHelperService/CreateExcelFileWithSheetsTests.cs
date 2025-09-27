using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class CreateExcelFileWithSheetsTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void CreateExcelFile_WithSingleSheet_ShouldCreateValidExcelFile()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "single_sheet.xlsx");
            var sheets = new Dictionary<string, string[]>
            {
                { "TestSheet", new[] { "Name", "Age", "Email" } }
            };

            // Act
            _service.CreateExcelFile(outputPath, sheets);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Read back the file to verify
            var dataSet = _service.ReadExcelFile(outputPath);
            dataSet.Tables.Should().HaveCount(1);
            dataSet.Tables[0].TableName.Should().Be("TestSheet");
            dataSet.Tables[0].Columns.Should().HaveCount(3);
            dataSet.Tables[0].Columns[0].ColumnName.Should().Be("Name");
            dataSet.Tables[0].Columns[1].ColumnName.Should().Be("Age");
            dataSet.Tables[0].Columns[2].ColumnName.Should().Be("Email");
        }

        [TestMethod]
        public void CreateExcelFile_WithMultipleSheets_ShouldCreateAllSheets()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "multiple_sheets.xlsx");
            var sheets = new Dictionary<string, string[]>
            {
                { "Employees", new[] { "Name", "Department" } },
                { "Products", new[] { "SKU", "Price", "Category" } },
                { "Orders", new[] { "OrderID", "Date", "CustomerID" } }
            };

            // Act
            _service.CreateExcelFile(outputPath, sheets);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var dataSet = _service.ReadExcelFile(outputPath);
            dataSet.Tables.Should().HaveCount(3);
            
            var sheetNames = dataSet.Tables.Cast<DataTable>().Select(t => t.TableName).ToArray();
            sheetNames.Should().Contain("Employees");
            sheetNames.Should().Contain("Products");
            sheetNames.Should().Contain("Orders");
        }

        [TestMethod]
        public void CreateExcelFile_WithEmptyHeaders_ShouldCreateFileWithEmptyColumns()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "empty_headers.xlsx");
            var sheets = new Dictionary<string, string[]>
            {
                { "EmptySheet", new string[0] }
            };

            // Act
            _service.CreateExcelFile(outputPath, sheets);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var dataSet = _service.ReadExcelFile(outputPath);
            dataSet.Tables.Should().HaveCount(1);
            dataSet.Tables[0].Columns.Should().HaveCount(0);
        }

        [TestMethod]
        public void CreateExcelFile_ShouldLogCreationProcess()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "logged.xlsx");
            var sheets = new Dictionary<string, string[]>
            {
                { "TestSheet", new[] { "Column1" } }
            };

            // Act
            _service.CreateExcelFile(outputPath, sheets);

            // Assert
            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Creating Excel file") && s.Contains(outputPath)),
                    TraceEventType.Information),
                Times.Once);

            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Excel file saved to") && s.Contains(outputPath)),
                    TraceEventType.Information),
                Times.Once);
        }
    }
}