using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UiPath.CodedWorkflows;
using Yash.Config.Helpers;

namespace Finance.Automations._00_Shared
{
    /// <summary>
    /// Test suite for DataTableToHTML conversion functionality.
    /// </summary>
    public class DataTableToHTML_Test : CodedWorkflow
    {
        /// <summary>
        /// Executes all test cases for DataTableToHTML.
        /// </summary>
        [TestCase]
        public void Execute()
        {
            Log($"Running all tests for {nameof(DataTableToHTML)}");
            Dictionary<string, bool> results = new();
            var testCases = new DataTableToHTML_TestCases().All;
            foreach (DataTableToHTML_TestCase testCase in testCases)
            {
                try
                {
                    RunTestCase(testCase);
                    results[testCase.Id] = true;
                }
                catch
                {
                    results[testCase.Id] = false;
                }
            }
            Log($"Finished all tests for {nameof(DataTableToHTML)}: PASS: {results.Where(r => r.Value).Count()} FAIL: {results.Where(r => !r.Value).Count()}");
        }

        /// <summary>
        /// Runs a single test case for DataTableToHTML.
        /// </summary>
        /// <param name="testCase">The test case to execute.</param>
        public void RunTestCase(DataTableToHTML_TestCase testCase)
        {
            Log($"Running test {testCase.Id}");

            Exception ex = null;
            string html = "";
            DataTableToHTMLOptions options = new();
            if (testCase.Id == "DataTableToHTML.WithSerializer") options.Serializers.Add("A", (val) => (Convert.ToInt32(val) + 2).ToString());
            var temp = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".csv");
            File.WriteAllText(temp, testCase.InputData.TableCSV);
            var table = CSVHelpers.Parse(temp, testCase.Id);
            
            File.Delete(temp);
            try
            {
                html = workflows.DataTableToHTML(table, options);
            }
            catch (Exception exception) { ex = exception; }

            if (!string.IsNullOrWhiteSpace(testCase.ExceptionMessage))
            {
                if (ex == null) throw new Exception("Should fail and didn't fail");
                Log("Failed as expected");
            }
            else
            {
                if (ex != null) throw new Exception("Shouldn't fail but did");
                if (testCase.OutputData != html) throw new Exception($"Output does not match\nExpected: {testCase.OutputData}\nOutcome: {html}");
                Log($"{testCase.Id} Successful");
            }
        }
    }

    /// <summary>
    /// Collection of test cases for DataTableToHTML.
    /// </summary>
    [Serializable]
    public class DataTableToHTML_TestCases
    {
        /// <summary>
        /// List of all test cases.
        /// </summary>
        public List<DataTableToHTML_TestCase> All { get; set; }

        /// <summary>
        /// Default test case with standard options.
        /// </summary>
        public static DataTableToHTML_TestCase DefaultOptions { get; set; } = new()
        {
            Id = "DataTableToHTML.DefaultOptions",
            InputData = new()
            {
                TableCSV = "A,B,C\n1,2,3\n4,5,6"
            },
            OutputData = "<table><tr><th>A</th><th>B</th><th>C</th></tr><tr><td>1</td><td>2</td><td>3</td></tr><tr><td>4</td><td>5</td><td>6</td></tr></table>"
        };

        /// <summary>
        /// Test case with a custom serializer applied.
        /// </summary>
        public static DataTableToHTML_TestCase WithSerializer { get; set; } = new()
        {
            Id = "DataTableToHTML.WithSerializer",
            InputData = new()
            {
                TableCSV = "A,B,C\n1,2,3\n4,5,6"
            },
            OutputData = "<table><tr><th>A</th><th>B</th><th>C</th></tr><tr><td>3</td><td>2</td><td>3</td></tr><tr><td>6</td><td>5</td><td>6</td></tr></table>"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableToHTML_TestCases"/> class with predefined test cases.
        /// </summary>
        public DataTableToHTML_TestCases()
        {
            All = new(){
                DefaultOptions,
                WithSerializer
            };
        }
    }

    /// <summary>
    /// Represents a single test case for DataTableToHTML.
    /// </summary>
    [Serializable]
    public class DataTableToHTML_TestCase
    {
        /// <summary>
        /// Unique identifier for the test case.
        /// </summary>
        public string Id { get; set; } = "Default";

        /// <summary>
        /// Input data for the test case.
        /// </summary>
        public DataTableToHTML_TestCaseInput InputData { get; set; } = new();

        /// <summary>
        /// Expected output data in HTML format.
        /// </summary>
        public string OutputData { get; set; } = "";

        /// <summary>
        /// Expected exception message if the test should fail.
        /// </summary>
        public string ExceptionMessage { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableToHTML_TestCase"/> class.
        /// </summary>
        public DataTableToHTML_TestCase() { }
    }

    /// <summary>
    /// Represents the input data for a test case.
    /// </summary>
    [Serializable]
    public class DataTableToHTML_TestCaseInput
    {
        /// <summary>
        /// CSV representation of the table data.
        /// </summary>
        public string TableCSV { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableToHTML_TestCaseInput"/> class.
        /// </summary>
        public DataTableToHTML_TestCaseInput() { }
    }
}
