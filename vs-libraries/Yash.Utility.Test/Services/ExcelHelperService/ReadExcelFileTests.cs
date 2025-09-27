using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class ReadExcelFileTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void ReadExcelFile_WithNonExistentFile_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.xlsx");

            // Act & Assert
            var action = () => _service.ReadExcelFile(nonExistentPath);
            action.Should().Throw<FileNotFoundException>()
                .WithMessage("Excel file not found.*");
        }

        [TestMethod]
        public void ReadExcelFile_WithValidFile_ShouldReadCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "test_read.xlsx");
            CreateTestExcelFile(filePath);

            // Act
            var result = _service.ReadExcelFile(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Should().HaveCount(1);
            result.Tables[0].Rows.Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public void ReadExcelFile_ShouldLogReadingProcess()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "log_test.xlsx");
            CreateTestExcelFile(filePath);

            // Act
            _service.ReadExcelFile(filePath);

            // Assert
            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Opening Excel file") && s.Contains(filePath)),
                    TraceEventType.Information),
                Times.Once);

            _mockLogger.Verify(
                x => x.Invoke(
                    It.Is<string>(s => s.Contains("Completed reading Excel file")),
                    TraceEventType.Information),
                Times.Once);
        }

        [TestMethod]
        public void ReadExcelFile_WithEmptySheet_ShouldLogWarning()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "empty_sheet.xlsx");
            var sheets = new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "EmptySheet", new[] { "Header1" } } // Headers but no data
            };
            _service.CreateExcelFile(filePath, sheets);

            // Act
            var result = _service.ReadExcelFile(filePath);

            // Assert
            result.Tables.Should().HaveCount(1);
            result.Tables[0].Rows.Should().HaveCount(0); // No data rows
        }

        [TestMethod]
        public void ReadExcelFile_WithDuplicateColumnNames_ShouldHandleDuplicates()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "duplicate_columns.xlsx");
            var sheets = new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "TestSheet", new[] { "Name", "Name", "Name" } }
            };
            _service.CreateExcelFile(filePath, sheets);

            // Act
            var result = _service.ReadExcelFile(filePath);

            // Assert
            result.Tables[0].Columns.Should().HaveCount(3);
            result.Tables[0].Columns[0].ColumnName.Should().Be("Name");
            result.Tables[0].Columns[1].ColumnName.Should().Be("Name_1");
            result.Tables[0].Columns[2].ColumnName.Should().Be("Name_2");
        }

        private void CreateTestExcelFile(string filePath)
        {
            var dataTable = new DataTable("TestSheet");
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Value", typeof(int));
            dataTable.Rows.Add("Item1", 100);
            dataTable.Rows.Add("Item2", 200);

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            _service.CreateExcelFile(filePath, dataSet);
        }
    }
}