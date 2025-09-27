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
    public class CreateExcelFileWithDataSetTests : ExcelHelperServiceTestBase
    {
        [TestMethod]
        public void CreateExcelFile_WithDataSet_ShouldCreateFileWithData()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "dataset.xlsx");
            var dataSet = CreateTestDataSet();

            // Act
            _service.CreateExcelFile(outputPath, dataSet);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var readDataSet = _service.ReadExcelFile(outputPath);
            readDataSet.Tables.Should().HaveCount(2);
            
            // Verify Employees table
            var employeesTable = readDataSet.Tables["Employees"];
            employeesTable.Should().NotBeNull();
            employeesTable.Rows.Should().HaveCount(2);
            employeesTable.Rows[0]["Name"].ToString().Should().Be("John Doe");
            employeesTable.Rows[0]["Age"].ToString().Should().Be("30");
            
            // Verify Products table
            var productsTable = readDataSet.Tables["Products"];
            productsTable.Should().NotBeNull();
            productsTable.Rows.Should().HaveCount(1);
            productsTable.Rows[0]["Name"].ToString().Should().Be("Widget");
            productsTable.Rows[0]["Price"].ToString().Should().Be("19.99");
        }

        [TestMethod]
        public void CreateExcelFile_WithDataTypes_ShouldPreserveDataTypes()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "datatypes.xlsx");
            var dataTable = new DataTable("DataTypes");
            
            dataTable.Columns.Add("StringCol", typeof(string));
            dataTable.Columns.Add("IntCol", typeof(int));
            dataTable.Columns.Add("DateCol", typeof(DateTime));
            dataTable.Columns.Add("BoolCol", typeof(bool));
            dataTable.Columns.Add("DecimalCol", typeof(decimal));
            
            var testDate = new DateTime(2023, 6, 15, 14, 30, 0);
            dataTable.Rows.Add("Test String", 42, testDate, true, 123.45m);
            dataTable.Rows.Add("Another String", -10, testDate.AddDays(1), false, -67.89m);
            
            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            // Act
            _service.CreateExcelFile(outputPath, dataSet);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var readDataSet = _service.ReadExcelFile(outputPath);
            var readTable = readDataSet.Tables[0];
            
            readTable.Rows[0]["StringCol"].ToString().Should().Be("Test String");
            readTable.Rows[0]["IntCol"].ToString().Should().Be("42");
            readTable.Rows[0]["BoolCol"].ToString().Should().Be("TRUE");
            readTable.Rows[0]["DecimalCol"].ToString().Should().Be("123.45");
        }

        [TestMethod]
        public void CreateExcelFile_WithNullValues_ShouldHandleNulls()
        {
            // Arrange
            var outputPath = Path.Combine(_testDirectory, "nulls.xlsx");
            var dataTable = new DataTable("WithNulls");
            
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Value", typeof(int));
            
            dataTable.Rows.Add("John", 10);
            dataTable.Rows.Add(DBNull.Value, DBNull.Value);
            dataTable.Rows.Add("Jane", 20);
            
            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            // Act
            _service.CreateExcelFile(outputPath, dataSet);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            var readDataSet = _service.ReadExcelFile(outputPath);
            var readTable = readDataSet.Tables[0];
            
            readTable.Rows.Should().HaveCount(3);
            readTable.Rows[1]["Name"].Should().Be(DBNull.Value);
            readTable.Rows[1]["Value"].Should().Be(DBNull.Value);
        }

        private DataSet CreateTestDataSet()
        {
            var dataSet = new DataSet();

            // Create Employees table
            var employeesTable = new DataTable("Employees");
            employeesTable.Columns.Add("Name", typeof(string));
            employeesTable.Columns.Add("Age", typeof(int));
            employeesTable.Columns.Add("Department", typeof(string));
            employeesTable.Rows.Add("John Doe", 30, "IT");
            employeesTable.Rows.Add("Jane Smith", 25, "HR");
            dataSet.Tables.Add(employeesTable);

            // Create Products table
            var productsTable = new DataTable("Products");
            productsTable.Columns.Add("Name", typeof(string));
            productsTable.Columns.Add("Price", typeof(decimal));
            productsTable.Columns.Add("InStock", typeof(bool));
            productsTable.Rows.Add("Widget", 19.99m, true);
            dataSet.Tables.Add(productsTable);

            return dataSet;
        }
    }
}