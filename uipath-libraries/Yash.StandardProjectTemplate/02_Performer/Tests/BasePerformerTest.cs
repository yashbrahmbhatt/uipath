/*
 * File: BasePerformerTest.cs
 * Project: Yash Standard Project
 * Component: Performer Tests (02_Performer/Tests)
 * 
 * Description: Base class for Performer test cases using the IPerformerTestable interface.
 * 
 * Author: Yash Team
 * Created: 2025
 * Modified: September 2025
 */

using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Yash.StandardProject.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows.Performer;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._02_Performer.Tests
{
    /// <summary>
    /// Base class for Performer test cases.
    /// Uses the IPerformerTestable interface with extension methods for consistent testing patterns.
    /// </summary>
    public abstract class BasePerformerTest : CodedWorkflowWithConfig, IPerformerTestable
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
        /// Configuration scopes required for performer tests.
        /// </summary>
        public override string[] ConfigScopes { get; set; } = { "Shared", "Performer" };
        
        #endregion

        #region IPerformerTestable Implementation
        
        /// <summary>
        /// Queue item reference for performer testing.
        /// </summary>
        public string QueueItemReference { get; set; } = "TEST_REFERENCE";
        
        /// <summary>
        /// Transaction data for performer testing.
        /// </summary>
        public object TransactionData { get; set; } = null;

        /// <summary>
        /// Implements the test logic specific to Performer workflow testing.
        /// This method is called by the TestableExtensions framework.
        /// </summary>
        public virtual void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId ?? TestId;
            
            Log($"🧪 Executing Performer test logic for: {TestId}", LogLevel.Info);
            
            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Performer" });
            
            // Execute the performer workflow with test parameters
            workflows.Performer(ConfigPath, ConfigScopes, TestId);
            
            Log($"✅ Performer test logic completed for: {TestId}", LogLevel.Info);
        }
        
        /// <summary>
        /// Initializes test-specific setup before execution.
        /// Override this method in concrete test classes for specific initialization.
        /// </summary>
        public virtual void InitializeTest()
        {
            Log($"🔧 Initializing Performer test: {TestId}", LogLevel.Info);
            
            // Set up default test data if not already configured
            if (string.IsNullOrEmpty(QueueItemReference))
            {
                QueueItemReference = $"TEST_REF_{TestId}_{DateTime.Now:yyyyMMddHHmmss}";
            }
            
            Log($"✅ Performer test initialized: {TestId} (Reference: {QueueItemReference})", LogLevel.Info);
        }
        
        /// <summary>
        /// Validates test results after execution.
        /// Override this method in concrete test classes for specific validation.
        /// </summary>
        public virtual void ValidateTest()
        {
            Log($"🧪 Validating Performer test results: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Performer test validation completed: {TestId}", LogLevel.Info);
        }
        
        /// <summary>
        /// Cleans up resources after test execution.
        /// Override this method in concrete test classes for specific cleanup.
        /// </summary>
        public virtual void CleanupTest()
        {
            Log($"🧹 Cleaning up Performer test: {TestId}", LogLevel.Info);
            // Base implementation - can be overridden by concrete test classes
            Log($"✅ Performer test cleanup completed: {TestId}", LogLevel.Info);
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