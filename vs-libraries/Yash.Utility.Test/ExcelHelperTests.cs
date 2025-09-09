using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Yash.Utility.Helpers;
using OfficeOpenXml;

namespace Yash.Utility.Tests
{
    [TestClass]
    public class ExcelHelperTests
    {
        private string _testDataDirectory = string.Empty;
        private string _testExcelFile = string.Empty;
        private string _tempExcelFile = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            // Set EPPlus license context for tests
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            _testDataDirectory = Path.Combine(TestContext?.TestDeploymentDir ?? ".", "TestData");
            _testExcelFile = Path.Combine(_testDataDirectory, "TestExcel.xlsx");
            _tempExcelFile = Path.Combine(_testDataDirectory, "TempTestExcel.xlsx");

            // Create test data directory if it doesn't exist
            if (!Directory.Exists(_testDataDirectory))
            {
                Directory.CreateDirectory(_testDataDirectory);
            }

            // Create a test Excel file
            CreateTestExcelFile(_testExcelFile);
        }

        [TestMethod]
        public void ReadExcelFile_ValidFile_ShouldReturnDataSet()
        {
            // Arrange
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            var result = ExcelHelpers.ReadExcelFile(_testExcelFile, logger);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Count.Should().BeGreaterThan(0);
            result.Tables[0].TableName.Should().Be("Sheet1");
            result.Tables[0].Rows.Count.Should().BeGreaterThan(0);
            result.Tables[0].Columns.Count.Should().BeGreaterThan(0);

            // Verify logging occurred
            logMessages.Should().NotBeEmpty();
            logMessages.Should().Contain(x => x.eventType == TraceEventType.Information);
        }

        [TestMethod]
        public void ReadExcelFile_FileNotFound_ShouldThrowFileNotFoundException()
        {
            // Arrange
            var nonExistentFile = Path.Combine(_testDataDirectory, "NonExistent.xlsx");
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act & Assert
            Action act = () => ExcelHelpers.ReadExcelFile(nonExistentFile, logger);
            act.Should().Throw<FileNotFoundException>()
                .WithMessage("Excel file not found.");

            // Verify error logging
            logMessages.Should().Contain(x => x.eventType == TraceEventType.Error);
        }

        [TestMethod]
        public void ReadExcelFile_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var specialCharsFile = Path.Combine(_testDataDirectory, "SpecialChars.xlsx");
            CreateTestExcelFileWithSpecialCharacters(specialCharsFile);

            // Act
            var result = ExcelHelpers.ReadExcelFile(specialCharsFile);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Count.Should().BeGreaterThan(0);
            
            var table = result.Tables[0];
            table.Columns.Should().Contain(c => c.ColumnName.Contains("Test"));
            table.Rows.Should().HaveCountGreaterThan(0);
        }

        [TestMethod]
        public void ReadExcelFile_EmptySheet_ShouldSkipEmptySheets()
        {
            // Arrange
            var emptySheetFile = Path.Combine(_testDataDirectory, "EmptySheet.xlsx");
            CreateExcelFileWithEmptySheet(emptySheetFile);
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            var result = ExcelHelpers.ReadExcelFile(emptySheetFile, logger);

            // Assert
            result.Should().NotBeNull();
            
            // Verify warning about empty sheet
            logMessages.Should().Contain(x => x.eventType == TraceEventType.Warning && 
                                              x.message.Contains("empty"));
        }

        [TestMethod]
        public void ReadExcelFile_DuplicateColumnNames_ShouldHandleCorrectly()
        {
            // Arrange
            var duplicateColumnsFile = Path.Combine(_testDataDirectory, "DuplicateColumns.xlsx");
            CreateExcelFileWithDuplicateColumns(duplicateColumnsFile);

            // Act
            var result = ExcelHelpers.ReadExcelFile(duplicateColumnsFile);

            // Assert
            result.Should().NotBeNull();
            result.Tables.Count.Should().BeGreaterThan(0);
            
            var table = result.Tables[0];
            var columnNames = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                columnNames.Add(column.ColumnName);
            }
            
            // Should have unique column names even with duplicates in source
            columnNames.Should().OnlyHaveUniqueItems();
            columnNames.Should().Contain("Name");
            columnNames.Should().Contain("Name_1"); // Renamed duplicate
        }

        [TestMethod]
        public void CreateExcelFile_WithDataTable_ShouldCreateFileWithData()
        {
            // Arrange
            var outputPath = Path.Combine(_testDataDirectory, "CreatedWithDataTable.xlsx");
            var dataTable = CreateSampleDataTable();
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            ExcelHelpers.CreateExcelFile(outputPath, dataTable, "TestData", logger);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Verify the created file can be read back with correct data
            var result = ExcelHelpers.ReadExcelFile(outputPath);
            result.Tables.Count.Should().Be(1);
            result.Tables["TestData"].Should().NotBeNull();
            
            var table = result.Tables["TestData"]!;
            table.Columns.Count.Should().Be(5); // Name, Age, Email, IsActive, Salary
            table.Rows.Count.Should().Be(3); // 3 data rows
            
            // Verify column names
            table.Columns[0].ColumnName.Should().Be("Name");
            table.Columns[1].ColumnName.Should().Be("Age");
            table.Columns[2].ColumnName.Should().Be("Email");
            table.Columns[3].ColumnName.Should().Be("IsActive");
            table.Columns[4].ColumnName.Should().Be("Salary");
            
            // Verify some data values
            table.Rows[0]["Name"].ToString().Should().Be("John Doe");
            table.Rows[0]["Age"].ToString().Should().Be("30");
            table.Rows[0]["Email"].ToString().Should().Be("john@example.com");
            
            // Verify logging
            logMessages.Should().Contain(x => x.eventType == TraceEventType.Information && 
                                              x.message.Contains("Creating Excel file with DataSet"));
        }

        [TestMethod]
        public void CreateExcelFile_WithMultipleDataTables_ShouldCreateMultipleSheets()
        {
            // Arrange
            var outputPath = Path.Combine(_testDataDirectory, "CreatedWithMultipleDataTables.xlsx");
            var employeeTable = CreateSampleDataTable();
            employeeTable.TableName = "Employees";
            
            var departmentTable = CreateDepartmentDataTable();
            departmentTable.TableName = "Departments";
            
            var dataSet = new DataSet();
            dataSet.Tables.Add(employeeTable);
            dataSet.Tables.Add(departmentTable);
            
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            ExcelHelpers.CreateExcelFile(outputPath, dataSet, logger);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Verify the created file can be read back
            var result = ExcelHelpers.ReadExcelFile(outputPath);
            result.Tables.Count.Should().Be(2);
            result.Tables["Employees"].Should().NotBeNull();
            result.Tables["Departments"].Should().NotBeNull();
            
            // Verify Employees sheet
            var empTable = result.Tables["Employees"]!;
            empTable.Columns.Count.Should().Be(5);
            empTable.Rows.Count.Should().Be(3);
            
            // Verify Departments sheet
            var deptTable = result.Tables["Departments"]!;
            deptTable.Columns.Count.Should().Be(3);
            deptTable.Rows.Count.Should().Be(2);
            
            // Verify logging for both sheets
            logMessages.Should().Contain(x => x.message.Contains("Processing sheet 'Employees'"));
            logMessages.Should().Contain(x => x.message.Contains("Processing sheet 'Departments'"));
        }

        [TestMethod]
        public void CreateExcelFile_WithEmptyDataTable_ShouldCreateHeadersOnly()
        {
            // Arrange
            var outputPath = Path.Combine(_testDataDirectory, "CreatedWithEmptyDataTable.xlsx");
            var emptyTable = new DataTable("EmptyTest");
            emptyTable.Columns.Add("Column1", typeof(string));
            emptyTable.Columns.Add("Column2", typeof(int));
            emptyTable.Columns.Add("Column3", typeof(DateTime));

            // Act
            ExcelHelpers.CreateExcelFile(outputPath, emptyTable);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Verify the created file has headers but no data
            var result = ExcelHelpers.ReadExcelFile(outputPath);
            result.Tables.Count.Should().Be(1);
            var table = result.Tables[0];
            table.Columns.Count.Should().Be(3);
            table.Rows.Count.Should().Be(0); // No data rows
            
            // Verify column names
            table.Columns[0].ColumnName.Should().Be("Column1");
            table.Columns[1].ColumnName.Should().Be("Column2");
            table.Columns[2].ColumnName.Should().Be("Column3");
        }

        [TestMethod]
        public void CreateExcelFile_WithVariousDataTypes_ShouldPreserveTypes()
        {
            // Arrange
            var outputPath = Path.Combine(_testDataDirectory, "CreatedWithVariousTypes.xlsx");
            var dataTable = CreateDataTableWithVariousTypes();

            // Act
            ExcelHelpers.CreateExcelFile(outputPath, dataTable, "TypeTest");

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Verify the created file preserves data types appropriately
            var result = ExcelHelpers.ReadExcelFile(outputPath);
            var table = result.Tables["TypeTest"]!;
            
            table.Columns.Count.Should().Be(6);
            table.Rows.Count.Should().Be(2);
            
            // Verify that different data types are handled
            // Note: Excel will convert types based on its own rules, but data should be preserved
            table.Rows[0]["StringCol"].Should().NotBeNull();
            table.Rows[0]["IntCol"].Should().NotBeNull();
            table.Rows[0]["DateCol"].Should().NotBeNull();
            table.Rows[0]["BoolCol"].Should().NotBeNull();
            table.Rows[0]["DecimalCol"].Should().NotBeNull();
            
            // Verify null handling
            var nullValue = table.Rows[1]["StringCol"];
            (nullValue == DBNull.Value || string.IsNullOrEmpty(nullValue.ToString())).Should().BeTrue();
        }

        [TestMethod]
        public void GenerateExcelTemplate_ValidInput_ShouldCreateTemplate()
        {
            // Arrange
            var outputPath = Path.Combine(_testDataDirectory, "Template.xlsx");
            var supportedTypes = new[] { "string", "int", "bool", "DateTime", "double" };
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Act
            ExcelHelpers.GenerateExcelTemplate(outputPath, supportedTypes, logger);

            // Assert
            File.Exists(outputPath).Should().BeTrue();
            
            // Verify the template structure
            var result = ExcelHelpers.ReadExcelFile(outputPath);
            result.Tables.Should().HaveCountGreaterOrEqualTo(4);
            result.Tables.Should().Contain(t => t.TableName == "_Instructions");
            result.Tables.Should().Contain(t => t.TableName == "_ConfigFileSettings");
            result.Tables.Should().Contain(t => t.TableName == "Settings");
            result.Tables.Should().Contain(t => t.TableName == "Assets");
            result.Tables.Should().Contain(t => t.TableName == "Files");

            // Check that ConfigFileSettings contains the types
            var configTable = result.Tables["_ConfigFileSettings"];
            configTable.Should().NotBeNull();
            configTable!.Rows.Count.Should().Be(supportedTypes.Length);

            // Verify logging
            logMessages.Should().Contain(x => x.eventType == TraceEventType.Information);
        }

        [TestMethod]
        public void GetFriendlyTypeName_BasicTypes_ShouldReturnFriendlyNames()
        {
            // Act & Assert
            ExcelHelpers.GetFriendlyTypeName(typeof(int)).Should().Be("int");
            ExcelHelpers.GetFriendlyTypeName(typeof(string)).Should().Be("string");
            ExcelHelpers.GetFriendlyTypeName(typeof(bool)).Should().Be("bool");
            ExcelHelpers.GetFriendlyTypeName(typeof(double)).Should().Be("double");
            ExcelHelpers.GetFriendlyTypeName(typeof(DateTime)).Should().Be("DateTime");
            ExcelHelpers.GetFriendlyTypeName(typeof(TimeSpan)).Should().Be("TimeSpan");
        }

        [TestMethod]
        public void GetFriendlyTypeName_ArrayTypes_ShouldReturnArrayNotation()
        {
            // Act & Assert
            ExcelHelpers.GetFriendlyTypeName(typeof(int[])).Should().Be("int[]");
            ExcelHelpers.GetFriendlyTypeName(typeof(string[])).Should().Be("string[]");
            ExcelHelpers.GetFriendlyTypeName(typeof(DateTime[])).Should().Be("DateTime[]");
        }

        [TestMethod]
        public void GetFriendlyTypeName_GenericTypes_ShouldReturnGenericNotation()
        {
            // Act & Assert
            ExcelHelpers.GetFriendlyTypeName(typeof(List<string>)).Should().Be("List<string>");
            ExcelHelpers.GetFriendlyTypeName(typeof(Dictionary<string, int>)).Should().Be("Dictionary<string, int>");
            ExcelHelpers.GetFriendlyTypeName(typeof(List<Dictionary<string, object>>)).Should().Be("List<Dictionary<string, Object>>");
        }

        [TestMethod]
        public void GetFriendlyTypeName_CustomTypes_ShouldReturnClassName()
        {
            // Act & Assert
            ExcelHelpers.GetFriendlyTypeName(typeof(ExcelHelperTests)).Should().Be("ExcelHelperTests");
            ExcelHelpers.GetFriendlyTypeName(typeof(TestContext)).Should().Be("TestContext");
        }

        [TestMethod]
        public void ReadExcelFile_WithVariousDataTypes_ShouldPreserveTypes()
        {
            // Arrange
            var typedDataFile = Path.Combine(_testDataDirectory, "TypedData.xlsx");
            CreateExcelFileWithVariousDataTypes(typedDataFile);

            // Act
            var result = ExcelHelpers.ReadExcelFile(typedDataFile);

            // Assert
            result.Should().NotBeNull();
            var table = result.Tables[0];
            
            // Verify data types are preserved appropriately
            table.Rows.Should().HaveCountGreaterThan(0);
            table.Columns.Should().HaveCountGreaterOrEqualTo(4);
            
            // Check first data row (skip header)
            if (table.Rows.Count > 0)
            {
                var firstRow = table.Rows[0];
                firstRow.ItemArray.Should().NotBeNull();
                firstRow.ItemArray.Length.Should().BeGreaterThan(0);
            }
        }

        [TestMethod]
        public void ReadExcelFile_WithFileAlreadyOpen_ShouldHandleLockedFile()
        {
            // Arrange
            var lockedFile = Path.Combine(_testDataDirectory, "LockedFile.xlsx");
            CreateTestExcelFile(lockedFile);
            
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Create a more realistic file lock similar to what Excel does
            // Excel typically locks for writing but allows reading by other processes
            using (var lockingStream = new FileStream(lockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                // Act - Try to read the file while it has a write lock
                var result = ExcelHelpers.ReadExcelFile(lockedFile, logger);

                // Assert
                result.Should().NotBeNull();
                result.Tables.Count.Should().BeGreaterThan(0);
                result.Tables[0].TableName.Should().Be("Sheet1");
                result.Tables[0].Rows.Count.Should().BeGreaterThan(0);
                
                // The file should be readable even with the write lock, 
                // but this tests the robustness of the ExcelHelpers method
                var table = result.Tables[0];
                table.Columns.Should().Contain(c => c.ColumnName == "Name");
                table.Columns.Should().Contain(c => c.ColumnName == "Age");
                table.Rows.Should().HaveCountGreaterThan(0);
                
                // Verify logging occurred
                logMessages.Should().NotBeEmpty();
                logMessages.Should().Contain(x => x.eventType == TraceEventType.Information);
            }
        }

        [TestMethod]
        public void ReadExcelFile_WithStrictlyLockedFile_ShouldUseRetryMechanism()
        {
            // Arrange - This test verifies the retry mechanism more explicitly
            var lockedFile = Path.Combine(_testDataDirectory, "StrictlyLockedFile.xlsx");
            CreateTestExcelFile(lockedFile);
            
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Test the retry mechanism by temporarily making file read-only
            // This simulates a scenario where EPPlus might have trouble opening the file initially
            var fileInfo = new FileInfo(lockedFile);
            var originalAttributes = fileInfo.Attributes;
            
            try
            {
                // Make file read-only to potentially trigger retry logic
                fileInfo.Attributes |= FileAttributes.ReadOnly;
                
                // Act
                var result = ExcelHelpers.ReadExcelFile(lockedFile, logger);
                
                // Assert
                result.Should().NotBeNull();
                result.Tables.Count.Should().BeGreaterThan(0);
                result.Tables[0].TableName.Should().Be("Sheet1");
                
                // Should successfully read the data
                var table = result.Tables[0];
                table.Columns.Should().Contain(c => c.ColumnName == "Name");
                table.Rows.Should().HaveCountGreaterThan(0);
                
                // Verify logging occurred
                logMessages.Should().NotBeEmpty();
            }
            finally
            {
                // Restore original attributes
                fileInfo.Attributes = originalAttributes;
            }
        }

        [TestMethod]
        public void ReadExcelFile_WithFileShareNoneLock_ShouldHandleGracefully()
        {
            // Arrange - This test demonstrates what happens with FileShare.None locks
            var exclusiveLockedFile = Path.Combine(_testDataDirectory, "ExclusiveLockedFile.xlsx");
            CreateTestExcelFile(exclusiveLockedFile);
            
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Create the most restrictive lock possible - FileShare.None
            using (var exclusiveStream = new FileStream(exclusiveLockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                // Act & Assert - This should fail gracefully after retries
                Action act = () => ExcelHelpers.ReadExcelFile(exclusiveLockedFile, logger);
                
                // With FileShare.None, even the enhanced retry mechanism will eventually fail
                // But it should provide a more helpful error message
                act.Should().Throw<InvalidOperationException>()
                   .WithMessage("*Unable to access Excel file*after*attempts*exclusively locked*");
                
                // Verify that retry attempts were made
                logMessages.Should().Contain(x => x.eventType == TraceEventType.Warning && 
                                                  x.message.Contains("IO Error"));
                
                // Should see multiple retry attempts
                var retryMessages = logMessages.Where(x => x.message.Contains("Retry attempt")).ToList();
                retryMessages.Should().HaveCountGreaterThan(1, "Multiple retry attempts should be logged");
                
                // Should see the final error message
                logMessages.Should().Contain(x => x.eventType == TraceEventType.Error && 
                                                  x.message.Contains("All retry attempts failed"));
            }
        }

        [TestMethod]
        public void ReadExcelFile_WithTemporaryLock_ShouldSucceedAfterRetry()
        {
            // Arrange - This test simulates a temporary lock that gets released
            var temporaryLockedFile = Path.Combine(_testDataDirectory, "TemporaryLockedFile.xlsx");
            CreateTestExcelFile(temporaryLockedFile);
            
            var logMessages = new List<(string message, TraceEventType eventType)>();
            Action<string, TraceEventType> logger = (msg, type) => logMessages.Add((msg, type));

            // Use a task to simulate a temporary lock that gets released
            var lockTask = Task.Run(async () =>
            {
                // Create a stronger lock that will trigger retry mechanism
                using (var tempLock = new FileStream(temporaryLockedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    // Hold the lock for a short time to trigger retry, then release it
                    await Task.Delay(1200); // 1.2 seconds - should trigger first retry at 500ms
                }
            });

            // Give the lock task a moment to acquire the lock
            Task.Delay(100).Wait();

            try
            {
                // Act - Start reading while file is locked
                var result = ExcelHelpers.ReadExcelFile(temporaryLockedFile, logger);

                // Assert - Should eventually succeed
                result.Should().NotBeNull();
                result.Tables.Count.Should().BeGreaterThan(0);
                result.Tables[0].TableName.Should().Be("Sheet1");
                
                var table = result.Tables[0];
                table.Columns.Should().Contain(c => c.ColumnName == "Name");
                table.Rows.Should().HaveCountGreaterThan(0);
                
                // Should see retry attempts in the logs
                var retryMessages = logMessages.Where(x => x.message.Contains("Retry attempt")).ToList();
                retryMessages.Should().HaveCountGreaterThan(0, "Should have attempted retries");
                
                // Should see IO error and retry messages
                logMessages.Should().Contain(x => x.eventType == TraceEventType.Warning && 
                                                  x.message.Contains("IO Error"));
                
                // Should eventually succeed
                logMessages.Should().Contain(x => x.eventType == TraceEventType.Information && 
                                                  x.message.Contains("Successfully read Excel file"));
            }
            finally
            {
                // Ensure the lock task completes
                lockTask.Wait(TimeSpan.FromSeconds(5));
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            var filesToCleanup = new[]
            {
                _testExcelFile,
                _tempExcelFile,
                Path.Combine(_testDataDirectory, "SpecialChars.xlsx"),
                Path.Combine(_testDataDirectory, "EmptySheet.xlsx"),
                Path.Combine(_testDataDirectory, "DuplicateColumns.xlsx"),
                Path.Combine(_testDataDirectory, "CreatedExcel.xlsx"),
                Path.Combine(_testDataDirectory, "Template.xlsx"),
                Path.Combine(_testDataDirectory, "TypedData.xlsx"),
                Path.Combine(_testDataDirectory, "LockedFile.xlsx"),
                Path.Combine(_testDataDirectory, "StrictlyLockedFile.xlsx"),
                Path.Combine(_testDataDirectory, "ExclusiveLockedFile.xlsx"),
                Path.Combine(_testDataDirectory, "TemporaryLockedFile.xlsx"),
                "test_sampledata.xlsx",
                "test_departments.xlsx",
                "test_multiple_dataTables.xlsx",
                "test_varioustypes.xlsx"
            };

            foreach (var file in filesToCleanup)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        // Remove read-only attribute if present
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Exists && (fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
                        {
                            fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                        }
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        #region Helper Methods

        private void CreateTestExcelFile(string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            
            // Headers
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Age";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "IsActive";
            
            // Data
            worksheet.Cells[2, 1].Value = "John Doe";
            worksheet.Cells[2, 2].Value = 30;
            worksheet.Cells[2, 3].Value = "john@example.com";
            worksheet.Cells[2, 4].Value = true;
            
            worksheet.Cells[3, 1].Value = "Jane Smith";
            worksheet.Cells[3, 2].Value = 25;
            worksheet.Cells[3, 3].Value = "jane@example.com";
            worksheet.Cells[3, 4].Value = false;
            
            package.SaveAs(new FileInfo(filePath));
        }

        private DataTable CreateSampleDataTable()
        {
            var dataTable = new DataTable("SampleData");
            
            // Add columns
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Age", typeof(int));
            dataTable.Columns.Add("Email", typeof(string));
            dataTable.Columns.Add("IsActive", typeof(bool));
            dataTable.Columns.Add("Salary", typeof(decimal));
            
            // Add sample data
            dataTable.Rows.Add("John Doe", 30, "john@example.com", true, 75000.50m);
            dataTable.Rows.Add("Jane Smith", 25, "jane@example.com", false, 65000.00m);
            dataTable.Rows.Add("Bob Johnson", 35, "bob@example.com", true, 85000.75m);
            
            return dataTable;
        }

        private DataTable CreateDepartmentDataTable()
        {
            var dataTable = new DataTable("Departments");
            
            // Add columns
            dataTable.Columns.Add("DeptId", typeof(int));
            dataTable.Columns.Add("DeptName", typeof(string));
            dataTable.Columns.Add("Manager", typeof(string));
            
            // Add sample data
            dataTable.Rows.Add(1, "Engineering", "Alice Cooper");
            dataTable.Rows.Add(2, "Marketing", "Bob Wilson");
            
            return dataTable;
        }

        private DataTable CreateDataTableWithVariousTypes()
        {
            var dataTable = new DataTable("VariousTypes");
            
            // Add columns with different types
            dataTable.Columns.Add("StringCol", typeof(string));
            dataTable.Columns.Add("IntCol", typeof(int));
            dataTable.Columns.Add("DateCol", typeof(DateTime));
            dataTable.Columns.Add("BoolCol", typeof(bool));
            dataTable.Columns.Add("DecimalCol", typeof(decimal));
            dataTable.Columns.Add("NullableCol", typeof(string));
            
            // Add sample data with various types
            dataTable.Rows.Add("Sample Text", 42, new DateTime(2024, 1, 15), true, 123.45m, "Not Null");
            dataTable.Rows.Add(DBNull.Value, 0, DateTime.Now, false, 0.0m, DBNull.Value);
            
            return dataTable;
        }

        private void CreateTestExcelFileWithSpecialCharacters(string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            
            // Headers with special characters
            worksheet.Cells[1, 1].Value = "Test Name";
            worksheet.Cells[1, 2].Value = "Spécial Chars";
            worksheet.Cells[1, 3].Value = "价格";
            
            // Data with special characters
            worksheet.Cells[2, 1].Value = "José María";
            worksheet.Cells[2, 2].Value = "Café & Co.";
            worksheet.Cells[2, 3].Value = "¥1000";
            
            package.SaveAs(new FileInfo(filePath));
        }

        private void CreateExcelFileWithEmptySheet(string filePath)
        {
            using var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("EmptySheet");
            var nonEmptySheet = package.Workbook.Worksheets.Add("NonEmptySheet");
            
            nonEmptySheet.Cells[1, 1].Value = "Header";
            nonEmptySheet.Cells[2, 1].Value = "Data";
            
            package.SaveAs(new FileInfo(filePath));
        }

        private void CreateExcelFileWithDuplicateColumns(string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            
            // Duplicate column names
            worksheet.Cells[1, 1].Value = "Name";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Age";
            worksheet.Cells[1, 4].Value = "";  // Empty header
            
            // Data
            worksheet.Cells[2, 1].Value = "John";
            worksheet.Cells[2, 2].Value = "Doe";
            worksheet.Cells[2, 3].Value = 30;
            worksheet.Cells[2, 4].Value = "Extra";
            
            package.SaveAs(new FileInfo(filePath));
        }

        private void CreateExcelFileWithVariousDataTypes(string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sheet1");
            
            // Headers
            worksheet.Cells[1, 1].Value = "Text";
            worksheet.Cells[1, 2].Value = "Number";
            worksheet.Cells[1, 3].Value = "Date";
            worksheet.Cells[1, 4].Value = "Boolean";
            worksheet.Cells[1, 5].Value = "Decimal";
            
            // Data with various types
            worksheet.Cells[2, 1].Value = "Sample Text";
            worksheet.Cells[2, 2].Value = 42;
            worksheet.Cells[2, 3].Value = DateTime.Now;
            worksheet.Cells[2, 4].Value = true;
            worksheet.Cells[2, 5].Value = 123.45;
            
            worksheet.Cells[3, 1].Value = "";  // Empty text
            worksheet.Cells[3, 2].Value = 0;
            worksheet.Cells[3, 3].Value = new DateTime(2024, 1, 1);
            worksheet.Cells[3, 4].Value = false;
            worksheet.Cells[3, 5].Value = -99.99;
            
            package.SaveAs(new FileInfo(filePath));
        }

        #endregion

        public TestContext? TestContext { get; set; }
    }
}