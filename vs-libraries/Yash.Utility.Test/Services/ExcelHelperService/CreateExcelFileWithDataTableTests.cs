using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;

namespace Yash.Utility.Test.Services.ExcelHelperService
{
    [TestClass]
    public class CreateExcelFileWithDataTableTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void CreateExcelFile_WithDataTable_ShouldCreateSingleSheetFile()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "datatable.xlsx");
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Rows.Add(1, "Test Item");
            dataTable.Rows.Add(2, "Another Item");

            // Act
            _service.CreateExcelFile(outputPath, dataTable);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var readDataSet = _service.ReadExcelFile(outputPath);
            readDataSet.Tables.Should().HaveCount(1);
            readDataSet.Tables[0].Rows.Should().HaveCount(2);
            readDataSet.Tables[0].Rows[0]["Name"].ToString().Should().Be("Test Item");
        }

        [TestMethod]
        public void CreateExcelFile_WithDataTableAndCustomSheetName_ShouldUseCustomName()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "custom_sheet.xlsx");
            var dataTable = new DataTable();
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");

            // Act
            _service.CreateExcelFile(outputPath, dataTable, "CustomSheetName");

            // Assert
            var readDataSet = _service.ReadExcelFile(outputPath);
            readDataSet.Tables[0].TableName.Should().Be("CustomSheetName");
        }

        [TestMethod]
        public void CreateExcelFile_WithDataTableWithTableName_ShouldUseTableName()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "table_name.xlsx");
            var dataTable = new DataTable("MyTableName");
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");

            // Act
            _service.CreateExcelFile(outputPath, dataTable);

            // Assert
            var readDataSet = _service.ReadExcelFile(outputPath);
            readDataSet.Tables[0].TableName.Should().Be("MyTableName");
        }

        [TestMethod]
        public void CreateExcelFile_WithDataTableNoName_ShouldUseDefaultSheetName()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "default_name.xlsx");
            var dataTable = new DataTable(); // No table name
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Rows.Add("Value1");

            // Act
            _service.CreateExcelFile(outputPath, dataTable);

            // Assert
            var readDataSet = _service.ReadExcelFile(outputPath);
            readDataSet.Tables[0].TableName.Should().Be("Sheet1");
        }
    }
}