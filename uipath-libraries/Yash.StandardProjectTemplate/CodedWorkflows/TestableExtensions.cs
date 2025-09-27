/*
 * File: TestableExtensions.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Extension methods for ITestable interfaces that provide the core
 *              test execution framework without requiring inheritance.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;

namespace Yash.StandardProject.CodedWorkflows
{
    /// <summary>
    /// Extension methods for ITestable interfaces.
    /// Provides the core test execution framework logic without requiring inheritance from a base class.
    /// </summary>
    public static class TestableExtensions
    {
        /// <summary>
        /// Executes the complete test workflow with exception handling and validation.
        /// This method provides the standardized test execution framework for any ITestable implementation.
        /// </summary>
        /// <param name="testable">The testable workflow instance</param>
        public static void ExecuteTest(this ITestable testable)
        {
            // Get access to logging (assuming the testable object has access to Log method)
            var loggable = testable as CodedWorkflow;
            var configurable = testable as CodedWorkflowWithConfig;
            var testing = configurable?.test;
            
            void LogMessage(string message, LogLevel level = LogLevel.Info)
            {
                if (loggable != null)
                {
                    loggable.Log(message, level);
                }
                else
                {
                    // Fallback to console if Log method not available
                    Console.WriteLine($"[{level}] {message}");
                }
            }

            LogMessage($"🧪 Running testable coded workflow: {testable.GetType().Name}");
            LogMessage($"🔖 Test ID: {testable.TestId}");
            LogMessage("");
            LogMessage("Initializing...");
            
            testable.InitializeTest();
            LogMessage($"Initialized Test Data!");
            
            try
            {
                // Initialize configuration if the testable object supports it
                if (testing != null)
                {
                    configurable.InitializeConfiguration();
                }
                
                // Get configuration properties from the testable object
                string configPath = GetConfigPath(testable);
                string[] configScopes = GetConfigScopes(testable);
                
                // Execute the test-specific logic
                testable.Execute(configPath, configScopes, testable.TestId);
                LogMessage("Completed execution successfully");
            }
            catch (Exception exception)
            {
                LogMessage($"Exception encountered: {exception.Message}", LogLevel.Warn);
                testable.ActualException = exception;
            }
            
            // Validate exception expectations
            if (testing != null)
            {
                testing.VerifyExpression(
                    (testable.ActualException == null && string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage)) ||
                    (testable.ActualException != null && !string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage)),
                    $"Expected failure status ({!string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage)}) did not match output ({testable.ActualException != null})",
                    true, "Exception Validation", false, false
                );

                if (testable.ActualException != null && !string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage))
                {
                    testing.VerifyExpression(
                        testable.ActualException.Message.Contains(testable.ExpectedExceptionMessage),
                        $"Expected exception message to contain '{testable.ExpectedExceptionMessage}', but got '{testable.ActualException.Message}'",
                        true, "Exception Message Validation", false, false
                    );
                    LogMessage($"✅ Test failed as expected: {testable.ActualException.Message}");
                }
                else
                {
                    LogMessage($"✅ Test completed successfully");
                }
            }
            else
            {
                // Fallback validation without testing framework
                if (testable.ActualException != null && !string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage))
                {
                    if (testable.ActualException.Message.Contains(testable.ExpectedExceptionMessage))
                    {
                        LogMessage($"✅ Test failed as expected: {testable.ActualException.Message}");
                    }
                    else
                    {
                        LogMessage($"❌ Test failed but with unexpected message. Expected: '{testable.ExpectedExceptionMessage}', Got: '{testable.ActualException.Message}'", LogLevel.Error);
                    }
                }
                else if (testable.ActualException == null && string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage))
                {
                    LogMessage($"✅ Test completed successfully");
                }
                else
                {
                    LogMessage($"❌ Test expectation mismatch. Expected exception: {!string.IsNullOrWhiteSpace(testable.ExpectedExceptionMessage)}, Got exception: {testable.ActualException != null}", LogLevel.Error);
                }
            }
            
            testable.ValidateTest();
            LogMessage("🎉 Testable coded workflow completed!");
            testable.CleanupTest();
        }

        /// <summary>
        /// Gets the ConfigPath from the testable object using reflection or known interfaces.
        /// </summary>
        private static string GetConfigPath(ITestable testable)
        {
            // Try to get ConfigPath from CodedWorkflowWithConfig if the object inherits from it
            if (testable is CodedWorkflowWithConfig configurable)
            {
                return configurable.ConfigPath;
            }
            
            // Fallback to reflection to get ConfigPath property
            var configPathProperty = testable.GetType().GetProperty("ConfigPath");
            if (configPathProperty != null && configPathProperty.PropertyType == typeof(string))
            {
                return configPathProperty.GetValue(testable) as string ?? "Data\\Config.xlsx";
            }
            
            // Default fallback
            return "Data\\Config.xlsx";
        }

        /// <summary>
        /// Gets the ConfigScopes from the testable object using reflection or known interfaces.
        /// </summary>
        private static string[] GetConfigScopes(ITestable testable)
        {
            // Try to get ConfigScopes from CodedWorkflowWithConfig if the object inherits from it
            if (testable is CodedWorkflowWithConfig configurable)
            {
                return configurable.ConfigScopes;
            }
            
            // Fallback to reflection to get ConfigScopes property
            var configScopesProperty = testable.GetType().GetProperty("ConfigScopes");
            if (configScopesProperty != null && configScopesProperty.PropertyType == typeof(string[]))
            {
                return configScopesProperty.GetValue(testable) as string[] ?? new[] { "Shared" };
            }
            
            // Default fallback
            return new[] { "Shared" };
        }
    }
}