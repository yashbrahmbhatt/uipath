using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Finance.Automations._00_Shared
{
    public class IsMaintenanceTime_Test : CodedWorkflow
    {
        private readonly string _testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "00_Shared", "TestData", "IsMaintenanceTime");

        [TestCase]
        public void Execute()
        {
            Log($"Running all tests for {nameof(IsMaintenanceTime)}");
            Dictionary<string, bool> results = new();
            var testCases = new IsMaintenanceTime_TestCases().All;

            foreach (var testCase in testCases)
            {
                try
                {
                    RunTestCase(testCase);
                    results[testCase.Id] = true;
                }
                catch (Exception ex)
                {
                    Log($"Test {testCase.Id} failed: {ex.Message}", LogLevel.Error);
                    results[testCase.Id] = false;
                }
            }

            Log($"Finished all tests for {nameof(IsMaintenanceTime)}: PASS: {results.Values.Count(v => v)} FAIL: {results.Values.Count(v => !v)}");
        }

        public void RunTestCase(IsMaintenanceTime_TestCase testCase)
        {
            Log($"Running test {testCase.Id}");

            // Use the current system time if 'now' is not provided
            TimeSpan now = testCase.InputData.Now ?? DateTime.Now.TimeOfDay;

            // Call the IsMaintenanceTime method with 'now' (this will use the overload)
            bool result = workflows.IsMaintenanceTime(testCase.InputData.StartTime, testCase.InputData.EndTime, now);

            // Validate the test case outcome
            if (testCase.ExpectedOutput != result)
            {
                throw new Exception($"Test {testCase.Id} failed. Expected: {testCase.ExpectedOutput}, Actual: {result}");
            }

            Log($"{testCase.Id} Successful");
        }
    }

    [Serializable]
    public class IsMaintenanceTime_TestCases
    {
        public List<IsMaintenanceTime_TestCase> All { get; set; }

        public IsMaintenanceTime_TestCases()
        {
            All = new()
            {
                MaintenanceTimeInsideWindow,
                MaintenanceTimeOutsideWindow,
                MaintenanceTimeSpanMidnight,
                MaintenanceTimeWithinDay,
                MaintenanceTimeWithCustomNow
            };
        }

        public static IsMaintenanceTime_TestCase MaintenanceTimeInsideWindow { get; } = new()
        {
            Id = "IsMaintenanceTime.MaintenanceTimeInsideWindow",
            InputData = new() { StartTime = TimeSpan.Parse("02:00"), EndTime = TimeSpan.Parse("04:00"), Now = TimeSpan.Parse("03:00") },
            ExpectedOutput = true // Assume current time is within 2 AM - 4 AM range
        };

        public static IsMaintenanceTime_TestCase MaintenanceTimeOutsideWindow { get; } = new()
        {
            Id = "IsMaintenanceTime.MaintenanceTimeOutsideWindow",
            InputData = new() { StartTime = TimeSpan.Parse("02:00"), EndTime = TimeSpan.Parse("04:00"), Now = TimeSpan.Parse("05:00") },
            ExpectedOutput = false // Assume current time is outside 2 AM - 4 AM range
        };

        public static IsMaintenanceTime_TestCase MaintenanceTimeSpanMidnight { get; } = new()
        {
            Id = "IsMaintenanceTime.MaintenanceTimeSpanMidnight",
            InputData = new() { StartTime = TimeSpan.Parse("22:00"), EndTime = TimeSpan.Parse("04:00"), Now = TimeSpan.Parse("23:00") },
            ExpectedOutput = true // Assume current time is between 10 PM - 4 AM
        };

        public static IsMaintenanceTime_TestCase MaintenanceTimeWithinDay { get; } = new()
        {
            Id = "IsMaintenanceTime.MaintenanceTimeWithinDay",
            InputData = new() { StartTime = TimeSpan.Parse("09:00"), EndTime = TimeSpan.Parse("17:00"), Now = TimeSpan.Parse("10:00") },
            ExpectedOutput = true // Assume current time is within 9 AM - 5 PM
        };

        public static IsMaintenanceTime_TestCase MaintenanceTimeWithCustomNow { get; } = new()
        {
            Id = "IsMaintenanceTime.MaintenanceTimeWithCustomNow",
            InputData = new() { StartTime = TimeSpan.Parse("02:00"), EndTime = TimeSpan.Parse("04:00"), Now = TimeSpan.Parse("03:00") },
            ExpectedOutput = true // Assume current time is provided as 3 AM (within the 2 AM - 4 AM range)
        };
    }

    [Serializable]
    public class IsMaintenanceTime_TestCase
    {
        public string Id { get; set; } = "Default";
        public IsMaintenanceTime_TestCaseInput InputData { get; set; } = new();
        public bool ExpectedOutput { get; set; }
    }

    [Serializable]
    public class IsMaintenanceTime_TestCaseInput
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan? Now { get; set; } // Now is optional; can be used for testing custom time
    }
}
