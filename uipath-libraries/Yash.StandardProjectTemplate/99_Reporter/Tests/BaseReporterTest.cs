/*
 * File: BaseReporterTest.cs
 * Project: Yash Standard Project
 * Component: Reporter Tests (99_Reporter/Tests)
 * 
 * Description: Base class for Reporter test cases using the IReporterTestable interface.
 * 
 * Author: Yash Team
 * Created: 2025
 * Modified: September 2025
 */

using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Yash.StandardProject.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows.Reporter;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._99_Reporter.Tests
{
    /// <summary>
    /// Base class for Reporter test cases.
    /// Uses the IReporterTestable interface with extension methods for consistent testing patterns.
    /// </summary>
    public abstract class BaseReporterTest : CodedWorkflowWithConfig, IReporterTestable
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
        /// Configuration scopes required for reporter tests.
        /// </summary>
        public override string[] ConfigScopes { get; set; } = { "Shared", "Reporter" };
        
        #endregion

        #region IReporterTestable Implementation
        
        /// <summary>
        /// From date for Execute method testing.
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// To date for Execute method testing.
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// CRON schedule for Execute method testing.
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// Implements the test logic specific to Reporter workflow testing.
        /// This method is called by the TestableExtensions framework.
        /// </summary>
        public virtual void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;
            
            Log($"🧪 Executing Reporter test logic for: {TestId}", LogLevel.Info);
            
            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Reporter" });
            
            // Create and execute the reporter workflow with test parameters
            var reporter = new Yash.StandardProject._99_Reporter.Reporter();
            reporter.Execute(FromDate, ToDate, Cron, ConfigPath, TestId);
            
            Log($"✅ Reporter test logic completed for: {TestId}", LogLevel.Info);
        }
        
        /// <summary>
        /// Initializes test-specific setup before execution.
        /// Override this method in concrete test classes for specific initialization.
        /// </summary>
        public virtual void InitializeTest()
        {
            Log($"🔧 Initializing Reporter test: {TestId}", LogLevel.Info);
            
            // Set default date values if not already set
            if (FromDate == default) FromDate = DateTime.Today.AddDays(-7);
            if (ToDate == default) ToDate = DateTime.Today;
            if (string.IsNullOrEmpty(Cron)) Cron = "";
            
            Log($"✅ Reporter test initialized: {TestId} (From: {FromDate:yyyy-MM-dd}, To: {ToDate:yyyy-MM-dd})", LogLevel.Info);
        }
        
        /// <summary>
        /// Validates test results after execution.
        /// Override this method in concrete test classes for specific validation.
        /// </summary>
        public virtual void ValidateTest()
        {
            Log($"🧪 Validating Reporter test results: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Reporter test validation completed: {TestId}", LogLevel.Info);
        }
        
        /// <summary>
        /// Cleans up resources after test execution.
        /// Override this method in concrete test classes for specific cleanup.
        /// </summary>
        public virtual void CleanupTest()
        {
            Log($"🧹 Cleaning up Reporter test: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Reporter test cleanup completed: {TestId}", LogLevel.Info);
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