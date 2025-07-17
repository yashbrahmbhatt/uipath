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
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using DT = System.Data.DataTable;

namespace Yash.Utility.Activities.DataTable
{
    public class ReadCSVTable_Test : CodedWorkflow
    {
        private readonly string _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData", "ReadCSVTable");

        [TestCase]
        public void Execute()
        {
            testing.VerifyExpression(true, $"Running all tests for {nameof(ReadCSVTable)}");
            Dictionary<string, bool> results = new();
            var testCases = new ReadCSVTable_TestCases().All;

            foreach (ReadCSVTable_TestCase testCase in testCases)
            {
                try
                {
                    RunTestCase(testCase);
                    results[testCase.Id] = true;
                }
                catch (Exception ex)
                {
                    testing.VerifyExpression(false, $"Test {testCase.Id} failed: {ex.Message}");
                    results[testCase.Id] = false;
                }
                
            }
            testing.VerifyExpression(true, "=== Results ===");
            testing.VerifyExpression(true, $"Finished all tests for {nameof(ReadCSVTable)}: PASS: {results.Values.Count(v => v)} FAIL: {results.Values.Count(v => !v)}");
        }

        public void RunTestCase(ReadCSVTable_TestCase testCase)
        {
            testing.VerifyExpression(true, $"=== {testCase.Id} ===");

            Exception ex = null;
            DT result = null;
            string filePath = Path.Combine(_testDataPath, testCase.InputData.FileName);

            try
            {
                // Run the ReadCSVTable execution
                result = workflows.ReadCSVTable(filePath, default);
            }
            catch (Exception exception)
            {
                ex = exception;
            }

            // Verify the test case outcome
            if (!string.IsNullOrWhiteSpace(testCase.ExpectedExceptionMessage))
            {
                // If an exception is expected, check if it occurred
                if (ex == null)
                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} should have failed but didn't.");

                // Check if the exception message matches the expected message
                else if (!ex.Message.Contains(testCase.ExpectedExceptionMessage))
                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed with unexpected exception message: {ex.Message}");

                else
                    testing.VerifyExpression(true, $"-\tTest {testCase.Id} failed as expected with exception: {ex.Message}");
            }
            else
            {
                // If no exception is expected, verify the result
                if (ex != null)
                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed unexpectedly: {ex.Message}");

                else
                {

                    // Compare columns
                    var resultColumns = result.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                    if (!resultColumns.SequenceEqual(testCase.ExpectedOutput.Columns))
                    {
                        testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Expected columns: {string.Join(", ", testCase.ExpectedOutput.Columns)}. Actual columns: {string.Join(", ", resultColumns)}");
                    }
                    else
                    {


                        // Compare rows
                        var resultRows = result.AsEnumerable().Select(row => row.ItemArray.Select(val => val.ToString()).ToList()).ToList();
                        // Iterate through each row and validate independently
                        for (int i = 0; i < testCase.ExpectedOutput.Rows.Count; i++)
                        {
                            var expectedRow = testCase.ExpectedOutput.Rows[i];
                            var actualRow = resultRows.ElementAtOrDefault(i);

                            // If the actual row is null (or doesn't exist), it's a mismatch
                            if (actualRow == null)
                            {
                                testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Expected row {i}: {JsonConvert.SerializeObject(expectedRow)} but actual row is missing.");
                                break;
                            }

                            // Compare each value in the row independently
                            for (int j = 0; j < expectedRow.Count; j++)
                            {
                                var expectedValue = expectedRow[j];
                                var actualValue = actualRow.ElementAtOrDefault(j);

                                if (expectedValue != actualValue)
                                {
                                    testing.VerifyExpression(false, $"-\tTest {testCase.Id} failed. Row {i}, Column {j}: Expected '{expectedValue}' but found '{actualValue}'.");
                                }
                            }
                        }
                    }

                    testing.VerifyExpression(true, $"-\t{testCase.Id} Successful");
                }
            }
        }

    }


    [Serializable]
    public class ReadCSVTable_TestCases
    {
        public List<ReadCSVTable_TestCase> All { get; set; }

        public ReadCSVTable_TestCases()
        {
            All = new()
            {
                BasicCSV,
                QuotedValues,
                EmptyCSV,
                MissingFile
            };
        }

        public static ReadCSVTable_TestCase BasicCSV { get; } = new()
        {
            Id = "ReadCSVTable.BasicCSV",
            InputData = new() { FileName = "BasicCSV.csv" },
            ExpectedOutput = new()
            {
                Columns = new() { "Column1", "Column2", "Column3" },
                Rows = new List<List<string>>()
                {
                    new() { "A1", "B1", "C1" },
                    new() { "A2", "B2", "C2" }
                }
            }
        };

        public static ReadCSVTable_TestCase QuotedValues { get; } = new()
        {
            Id = "ReadCSVTable.QuotedValues",
            InputData = new() { FileName = "QuotedValues.csv" },
            ExpectedOutput = new()
            {
                Columns = new() { "ID", "Name", "Comment" },
                Rows = new List<List<string>>()
                {
                    new() { "1", "Alice", "Hello, world" },
                    new() { "2", "Bob", "Multi-line\ncomment" },
                    new() { "3", "Charlie", "Quoted \"text\"" }
                }
            }
        };

        public static ReadCSVTable_TestCase EmptyCSV { get; } = new()
        {
            Id = "ReadCSVTable.EmptyCSV",
            InputData = new() { FileName = "EmptyCSV.csv" },
            ExpectedOutput = new() { Columns = new(), Rows = new List<List<string>>() }
        };

        public static ReadCSVTable_TestCase MissingFile { get; } = new()
        {
            Id = "ReadCSVTable.MissingFile",
            InputData = new() { FileName = "DoesNotExist.csv" },
            ExpectedOutput = null,
            ExpectedExceptionMessage = "CSV file not found at"
        };
    }

    [Serializable]
    public class ReadCSVTable_TestCase
    {
        public string Id { get; set; } = "Default";
        public ReadCSVTable_TestCaseInput InputData { get; set; } = new();
        public ReadCSVTable_TestCaseOutput ExpectedOutput { get; set; } = new();
        public string ExpectedExceptionMessage { get; set; } = "";
    }

    [Serializable]
    public class ReadCSVTable_TestCaseInput
    {
        public string FileName { get; set; } = "";
    }

    [Serializable]
    public class ReadCSVTable_TestCaseOutput
    {
        public List<string> Columns { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}