/*
 * File: ITestable.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Interface that defines the contract for testable workflows.
 *              Separates testing concerns from business logic workflows.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;

namespace Yash.StandardProject.CodedWorkflows
{
    /// <summary>
    /// Interface that defines the contract for testable workflows.
    /// Classes implementing this interface can participate in the testing framework.
    /// </summary>
    public interface ITestable
    {
        #region Test Properties
        
        /// <summary>
        /// Unique identifier for the test case.
        /// </summary>
        string TestId { get; set; }
        
        /// <summary>
        /// Expected exception message if the test should fail (empty string means success expected).
        /// </summary>
        string ExpectedExceptionMessage { get; set; }
        
        /// <summary>
        /// The actual exception that occurred during test execution.
        /// </summary>
        Exception ActualException { get; set; }
        
        #endregion

        #region Test Lifecycle Methods
        
        /// <summary>
        /// Executes the workflow with specified configuration for testing.
        /// </summary>
        /// <param name="configPath">Path to the configuration file</param>
        /// <param name="configScopes">Configuration scopes to load</param>
        /// <param name="testId">Test identifier</param>
        void Execute(string configPath, string[] configScopes, string testId = "");
        
        /// <summary>
        /// Initializes test-specific setup before execution.
        /// </summary>
        void InitializeTest();
        
        /// <summary>
        /// Validates test results after execution.
        /// </summary>
        void ValidateTest();
        
        /// <summary>
        /// Cleans up resources after test execution.
        /// </summary>
        void CleanupTest();
        
        /// <summary>
        /// Executes the complete test workflow with exception handling and validation.
        /// </summary>
        void ExecuteTest();
        
        #endregion
    }
}