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
using Yash.Config.Activities.Models;

namespace Yash.Config.Activities
{
    /// <summary>
    /// Test class for <see cref="LoadConfig"/>. Executes test cases to validate functionality.
    /// </summary>
    public class LoadConfig_Test : CodedWorkflow
    {
        /// <summary>
        /// Runs all test cases for <see cref="LoadConfig"/>.
        /// </summary>
        [TestCase]
        public void Execute()
        {
            testing.VerifyExpression(true, $"Running all tests for {nameof(LoadConfig)}");
            Dictionary<string, bool> results = new();
            var testCases = new LoadConfig_TestCases().All;

            foreach (LoadConfig_TestCase testCase in testCases)
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

            testing.VerifyExpression(true, $"Finished all tests for {nameof(LoadConfig)}: PASS: {results.Count(r => r.Value)} FAIL: {results.Count(r => !r.Value)}");
        }

        /// <summary>
        /// Runs a single test case for <see cref="LoadConfig"/>.
        /// </summary>
        /// <param name="testCase">The test case to execute.</param>
        public void RunTestCase(LoadConfig_TestCase testCase)
        {
            testing.VerifyExpression(true, $"Running test {testCase.Id}");

            Exception ex = null;
            Dictionary<string, object> config = null;
            var temp = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".json");

            // Write test configuration to a temporary file
            File.WriteAllText(temp, JsonConvert.SerializeObject(testCase.InputData.Config));

            try
            {
                config = workflows.LoadConfig(testCase.InputData.Names, temp);
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            finally
            {
                File.Delete(temp);
            }

            // Verify the test case outcome
            if (!string.IsNullOrWhiteSpace(testCase.ExceptionMessage))
            {
                if (ex == null)
                    testing.VerifyExpression(false, $"Should fail and didn't fail");
                else
                    testing.VerifyExpression(true, $"Failed as expected");
            }
            else
            {
                if (ex != null)
                    testing.VerifyExpression(false, $"Shouldn't fail but did");

                else if (testCase.OutputData.Count != config.Count ||
                    JsonConvert.SerializeObject(testCase.OutputData) != JsonConvert.SerializeObject(config))
                {
                    testing.VerifyExpression(false, $"Output does not match\nExpected: {JsonConvert.SerializeObject(testCase.OutputData)}\nOutcome: {JsonConvert.SerializeObject(config)}");
                }
                else
                    testing.VerifyExpression(true, $"{testCase.Id} Successful");
            }
        }
    }

    /// <summary>
    /// Contains a collection of test cases for <see cref="LoadConfig"/>.
    /// </summary>
    [Serializable]
    public class LoadConfig_TestCases
    {
        /// <summary>
        /// List of all test cases.
        /// </summary>
        public List<LoadConfig_TestCase> All { get; set; }

        /// <summary>
        /// Test case for loading a setting from configuration.
        /// </summary>
        public static LoadConfig_TestCase Setting { get; set; } = new()
        {
            Id = "LoadConfig.Setting",
            InputData = new()
            {
                Names = new() { "Config" },
                Config = new()
                {
                    {
                        "Config",
                        new()
                        {
                            Settings = new() { new ConfigSettingItem(){Name="Key", Value= "Value", Description=""}},
                            Assets = new(),
                            Files = new()
                        }
                    }
                }
            },
            OutputData = new() { { "Key", "Value" } }
        };

        /// <summary>
        /// Test case for loading an asset from configuration.
        /// </summary>
        public static LoadConfig_TestCase Asset { get; set; } = new()
        {
            Id = "LoadConfig.Asset",
            InputData = new()
            {
                Names = new() { "Config" },
                Config = new()
                {
                    {
                        "Config",
                        new()
                        {
                            Settings = new(),
                            Assets = new()
                            {
                                new()
                                {
                                    Name = "Key",
                                    Value = "LoadConfig_TestAsset",
                                    Folder = "Shared",
                                    Description = "Unit Test"
                                }
                            },
                            Files = new()
                        }
                    }
                }
            },
            OutputData = new() { { "Key", "Value" } }
        };

        /// <summary>
        /// Test case for loading a local text file from configuration.
        /// </summary>
        public static LoadConfig_TestCase LocalTextFile { get; set; } = new()
        {
            Id = "LoadConfig.LocalTextFile",
            InputData = new()
            {
                Names = new() { "Config" },
                Config = new()
                {
                    {
                        "Config",
                        new()
                        {
                            Settings = new(),
                            Assets = new(),
                            Files = new()
                            {
                                new()
                                {
                                    Name = "Key",
                                    Path = "TestData\\LoadConfig\\LocalTextFile.txt",
                                    Folder = "",
                                    Bucket = "",
                                    Description = "Unit Test",
                                    Type = "txt"
                                }
                            }
                        }
                    }
                }
            },
            OutputData = new() { { "Key", "Value" } }
        };

        /// <summary>
        /// Test case for loading a local CSV file from configuration.
        /// </summary>
        public static LoadConfig_TestCase LocalCsvFile { get; set; } = new()
        {
            Id = "LoadConfig.LocalCsvFile",
            InputData = new()
            {
                Names = new() { "Config" },
                Config = new()
                {
                    {
                        "Config",
                        new()
                        {
                            Settings = new(),
                            Assets = new(),
                            Files = new()
                            {
                                new()
                                {
                                    Name = "Key",
                                    Path = "TestData\\LoadConfig\\LocalCSVFile.csv",
                                    Folder = "",
                                    Bucket = "",
                                    Description = "Unit Test",
                                    Type = "csv"
                                }
                            }
                        }
                    }
                }
            },
            OutputData = new() { { "Key", JsonConvert.DeserializeObject<DataTable>("[{\"A\":\"1\",\"B\":\"2\",\"C\":\"3\"}]") } }
        };

        /// <summary>
        /// Initializes the test case collection.
        /// </summary>
        public LoadConfig_TestCases()
        {
            All = new()
            {
                Setting,
                Asset,
                LocalTextFile,
                LocalCsvFile
            };
        }
    }

    /// <summary>
    /// Represents a single test case for <see cref="LoadConfig"/>.
    /// </summary>
    [Serializable]
    public class LoadConfig_TestCase
    {
        /// <summary>
        /// Unique identifier for the test case.
        /// </summary>
        public string Id { get; set; } = "Default";

        /// <summary>
        /// Input data for the test case.
        /// </summary>
        public LoadConfig_TestCaseInput InputData { get; set; } = new();

        /// <summary>
        /// Expected output data for the test case.
        /// </summary>
        public Dictionary<string, object> OutputData { get; set; } = new();

        /// <summary>
        /// Expected exception message if the test should fail.
        /// </summary>
        public string ExceptionMessage { get; set; } = "";

        public LoadConfig_TestCase() { }
    }

    /// <summary>
    /// Represents the input data structure for a <see cref="LoadConfig_TestCase"/>.
    /// </summary>
    [Serializable]
    public class LoadConfig_TestCaseInput
    {
        /// <summary>
        /// The configuration data for the test case.
        /// </summary>
        public Dictionary<string, ConfigFile> Config { get; set; } = new();

        /// <summary>
        /// The names of the configurations to load.
        /// </summary>
        public List<string> Names { get; set; } = new();

        public LoadConfig_TestCaseInput() { }
    }
}