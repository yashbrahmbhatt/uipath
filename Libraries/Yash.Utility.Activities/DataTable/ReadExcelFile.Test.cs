using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using DT = System.Data.DataTable;

namespace Yash.Utility.Activities.DataTable
{
    public class ReadExcelFile_Test : CodedWorkflow
    {
        private readonly string _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "ReadExcelFile");

        /// <summary>
        /// Executes all test cases for the ReadExcelFile workflow.
        /// </summary>
        [TestCase]
        public void Execute()
        {
            testing.VerifyExpression(true, $"Running all tests for {nameof(ReadExcelFile)}");
            Dictionary<string, bool> results = new();
            var testCases = new ReadExcelFile_TestCases().All;

            // Run each test case and track results
            foreach (ReadExcelFile_TestCase testCase in testCases)
            {
                try
                {
                    RunTestCase(testCase);
                    results[testCase.Id] = true;
                }
                catch (Exception ex)
                {
                    testing.VerifyExpression(true, $"Test {testCase.Id} failed: {ex.Message}");
                    results[testCase.Id] = false;
                }
            }

            // Log the final result of all tests
            testing.VerifyExpression(true, "=== Results ===");

            testing.VerifyExpression(true, $"Finished all tests for {nameof(ReadExcelFile)}: PASS: {results.Values.Count(v => v)} FAIL: {results.Values.Count(v => !v)}");
        }

        /// <summary>
        /// Runs a single test case for ReadExcelFile workflow.
        /// </summary>
        /// <param name="testCase">The test case to execute.</param>
        public void RunTestCase(ReadExcelFile_TestCase testCase)
        {
            testing.VerifyExpression(true, $"=== {testCase.Id} ===");

            DataSet result = null;
            string testFilePath = Path.Combine(_testDataPath, testCase.InputData.FileName);

            try
            {
                result = workflows.ReadExcelFile(testFilePath);  // Execute the workflow with the test file path.
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(testCase.ExpectedExceptionMessage) && ex.Message.Contains(testCase.ExpectedExceptionMessage))
                {
                    testing.VerifyExpression(true, $"-\tTest {testCase.Id} failed as expected with exception: {ex.Message}");
                    return;
                }
                else
                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed unexpectedly: {ex.Message}");
            }

            // Verify the output data against expected values.
            if (testCase.ExpectedOutput != null)
            {
                if (result.Tables.Count != testCase.ExpectedOutput.Tables.Count)
                {
                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Expected {testCase.ExpectedOutput.Tables.Count} tables, but got {result.Tables.Count}");
                }
                else
                {
                    for (int i = 0; i < result.Tables.Count; i++)
                    {
                        var actualTable = result.Tables[i];
                        var expectedTable = testCase.ExpectedOutput.Tables[i];

                        // Check if the number of columns matches.
                        if (actualTable.Columns.Count != expectedTable.Columns.Count)
                        {
                            testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Table {i} column count mismatch. Expected {expectedTable.Columns.Count}, but got {actualTable.Columns.Count}");
                            break;
                        }

                        // Check if the rows match.
                        for (int j = 0; j < actualTable.Rows.Count; j++)
                        {
                            var actualRow = actualTable.Rows[j].ItemArray;
                            var expectedRow = expectedTable.Rows[j].ItemArray;

                            // Compare the actual and expected rows
                            for (int col = 0; col < actualRow.Length; col++)
                            {
                                if (!actualRow[col].Equals(expectedRow[col]))
                                {
                                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Table {i} Row {j}, Column {col} mismatch.\n-\t" +
                                        $"Expected: {JsonConvert.SerializeObject(expectedRow[col])}\n-\tActual: {JsonConvert.SerializeObject(actualRow[col])}");
                                    break;
                                }
                            }
                        }

                    }
                }

                testing.VerifyExpression(true, $"-\t{testCase.Id} successful");
            }
        }
    }

    [Serializable]
    public class ReadExcelFile_TestCases
    {
        public List<ReadExcelFile_TestCase> All { get; set; }

        public ReadExcelFile_TestCases()
        {
            All = new()
            {
                BasicExcelFile,
                MissingFile,
                EmptyExcelFile
            };
        }

        public static ReadExcelFile_TestCase BasicExcelFile { get; } = new()
        {
            Id = "ReadExcelFile.BasicExcelFile",
            InputData = new() { FileName = "BasicExcelFile.xlsx" },
            ExpectedOutput = new()
            {
                Tables = new List<DT>
                {
                    new DT() {
                        Columns = { "Column1", "Column2", "Column3" },
                        Rows = {
                            new object[] { "A1", "B1", "C1" },
                            new object[] { "A2", "B2", "C2" }
                        }
                    },
                    new DT() {
                        Columns = { "Column1", "Column2", "Column3" },
                        Rows = {
                            new object[] { "D1", "E1", "F1" },
                            new object[] { "D2", "E2", "F2" }
                        }
                    }
                }
            }
        };

        public static ReadExcelFile_TestCase MissingFile { get; } = new()
        {
            Id = "ReadExcelFile.MissingFile",
            InputData = new() { FileName = "MissingFile.xlsx" },
            ExpectedExceptionMessage = "File not found"
        };

        public static ReadExcelFile_TestCase EmptyExcelFile { get; } = new()
        {
            Id = "ReadExcelFile.EmptyExcelFile",
            InputData = new() { FileName = "EmptyExcelFile.xlsx" },
            ExpectedOutput = new() { Tables = new List<DT>() }
        };
    }

    [Serializable]
    public class ReadExcelFile_TestCase
    {
        public string Id { get; set; } = "Default";
        public ReadExcelFile_TestCaseInput InputData { get; set; } = new();
        public ReadExcelFile_TestCaseOutput ExpectedOutput { get; set; } = new();
        public string ExpectedExceptionMessage { get; set; } = "";
    }

    [Serializable]
    public class ReadExcelFile_TestCaseInput
    {
        public string FileName { get; set; } = "";
    }

    [Serializable]
    public class ReadExcelFile_TestCaseOutput
    {
        public List<DT> Tables { get; set; } = new();
    }
}