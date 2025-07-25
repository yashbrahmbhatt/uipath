using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Testing
{
    public abstract class TestSet<TInput, TOutput> : CodedWorkflow
    {
        /// <summary>
        /// Path to the workflow being tested (e.g., "Workflows/ReadCSVTable")
        /// </summary>
        protected abstract string WorkflowPath { get; }

        /// <summary>
        /// Name of the workflow being tested (eg. ReadCSVTable)
        /// </summary>
        protected string WorkflowName => Path.GetFileNameWithoutExtension(WorkflowPath);

        /// <summary>
        /// All test cases to run.
        /// </summary>
        protected abstract IEnumerable<TestCase<TInput, TOutput>> GetTestCases();

        /// <summary>
        /// Map input to dictionary suitable for RunWorkflow
        /// </summary>
        protected abstract Dictionary<string, object> MapInputs(TInput input);

        /// <summary>
        /// Compares the actual output from the workflow to the expected output.
        /// </summary>
        protected abstract void AssertOutputs(TOutput expected, object actual);

        [Workflow]
        public void Execute()
        {
            testing.VerifyExpression(true, $"Running all tests for {Path.GetFileNameWithoutExtension(WorkflowPath)}");
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
    }
}