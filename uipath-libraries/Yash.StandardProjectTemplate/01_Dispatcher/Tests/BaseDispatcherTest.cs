/*
 * File: BaseDispatcherTest.cs
 * Project: Yash Standard Project
 * Component: Dispatcher Tests (01_Dispatcher/Tests)
 * 
 * Description: Base class for Dispatcher test cases using the IDispatcherTestable interface.
 * 
 * Author: Yash Team
 * Created: 2025
 * Modified: September 2025
 */

using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Yash.StandardProject.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows.Dispatcher;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._01_Dispatcher.Tests
{
    /// <summary>
    /// Base class for Dispatcher test cases.
    /// Uses the IDispatcherTestable interface with extension methods for consistent testing patterns.
    /// </summary>
    public abstract class BaseDispatcherTest : CodedWorkflowWithConfig, IDispatcherTestable
    {
        #region ITestable Implementation

        /// <summary>
        /// Test identifier - must be set by concrete implementations.
        /// </summary>
        public abstract string TestId { get; set; }

        /// <summary>
        /// Expected exception message if the test should fail (empty string means success expected).
        /// </summary>
        public abstract string ExpectedExceptionMessage { get; set; }

        /// <summary>
        /// The actual exception that occurred during test execution.
        /// </summary>
        public Exception ActualException { get; set; } = null;

        #endregion

        #region Configuration Properties

        /// <summary>
        /// Path to the configuration file - defaults to standard location.
        /// </summary>
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";

        /// <summary>
        /// Configuration scopes required for dispatcher tests.
        /// </summary>
        public override string[] ConfigScopes { get; set; } = { "Shared", "Dispatcher" };

        #endregion

        #region IDispatcherTestable Implementation

        /// <summary>
        /// Implements the test logic specific to Dispatcher workflow testing.
        /// This method is called by the TestableExtensions framework.
        /// </summary>
        public virtual void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;

            Log($"🧪 Executing Dispatcher test logic for: {TestId}", LogLevel.Info);

            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Dispatcher" });

            // Execute the dispatcher workflow with test parameters
            workflows.Dispatcher(ConfigPath, TestId);

            Log($"✅ Dispatcher test logic completed for: {TestId}", LogLevel.Info);
        }

        /// <summary>
        /// Initializes test-specific setup before execution.
        /// Override this method in concrete test classes for specific initialization.
        /// </summary>
        public virtual void InitializeTest()
        {
            Log($"🔧 Initializing Dispatcher test: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Dispatcher test initialized: {TestId}", LogLevel.Info);
        }

        /// <summary>
        /// Validates test results after execution.
        /// Override this method in concrete test classes for specific validation.
        /// </summary>
        public virtual void ValidateTest()
        {
            Log($"🧪 Validating Dispatcher test results: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Dispatcher test validation completed: {TestId}", LogLevel.Info);
        }

        /// <summary>
        /// Cleans up resources after test execution.
        /// Override this method in concrete test classes for specific cleanup.
        /// </summary>
        public virtual void CleanupTest()
        {
            Log($"🧹 Cleaning up Dispatcher test: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Dispatcher test cleanup completed: {TestId}", LogLevel.Info);
        }

        /// <summary>
        /// Executes the complete test workflow with exception handling and validation.
        /// This method provides the standardized test execution framework.
        /// </summary>
        public virtual void ExecuteTest()
        {
            // Use the extension method to execute the standardized test workflow
            TestableExtensions.ExecuteTest(this);
        }

        #endregion
    }
}
