/*
 * File: DispatcherWorkflow.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Abstract base class for Dispatcher workflows that provides
 *              common dispatcher functionality and standardized execution patterns.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UiPath.CodedWorkflows;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using LogLevel = UiPath.CodedWorkflows.LogLevel;
using Yash.Utility;
using UiPath.Core.Activities;

namespace Yash.StandardProject.CodedWorkflows.Dispatcher
{
    /// <summary>
    /// Abstract base class for Dispatcher workflows.
    /// Provides common dispatcher functionality including application lifecycle management,
    /// queue item creation, and error handling patterns.
    /// </summary>
    public abstract class DispatcherWorkflow : CodedWorkflowWithConfig
    {
        

        #region Main Workflow Entry Points
        
        /// <summary>
        /// Main workflow entry point that accepts parameters for flexible execution.
        /// </summary>
        /// <param name="ConfigPath">The path to the configuration file.</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        public virtual void Execute(
            string ConfigPath = "Data\\Config.xlsx",
            string testId = ""
        )
        {
            // Set properties for this execution
            this.ConfigPath = ConfigPath;
            this.TestId = testId;
            
            // Initialize configuration first
            InitializeConfiguration();
            
            // Execute the dispatcher logic
            ExecuteDispatcherLogic();
        }
        
        #endregion

        #region Core Dispatcher Logic
        
        /// <summary>
        /// Executes the main dispatcher workflow logic with standardized error handling.
        /// </summary>
        protected virtual void ExecuteDispatcherLogic()
        {
            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Dispatcher" });

            try
            {
                Log("Starting dispatching", LogLevel.Info);
                
                var itemsAdded = 0;
                
                // Lifecycle management
                CloseApplicationsGracefully();
                InitializeApplications();

                // Process input data and create queue items
                itemsAdded = ProcessInputData();
                
                Log($"Dispatched {itemsAdded} items", LogLevel.Info);
            }
            catch (Exception ex)
            {
                CloseApplicationsGracefully();
                Log($"Error occurred during dispatching: {ex.Message}", LogLevel.Error);
                HandleDispatcherError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// Processes input data and creates queue items.
        /// Override this method to implement specific data processing logic.
        /// </summary>
        /// <returns>Number of queue items created</returns>
        public abstract int ProcessInputData();
        
        #endregion

        #region Application Lifecycle Management
        
        /// <summary>
        /// Closes applications with fallback to process killing.
        /// </summary>
        public abstract void CloseApplications();
        
        /// <summary>
        /// Initializes required applications.
        /// </summary>
        public abstract void InitializeApplications();
        
        protected virtual void CloseApplicationsGracefully(){
            try 
            {
                CloseApplications();
            }
            catch (Exception ex)
            {
                Log($"Failed to close applications gracefully, killing processes: {ex.Message}", LogLevel.Warn);
                foreach (var process in DispatcherConfig.ProcessesToKill)
                    workflows.KillProcess(process);
            }
        }
        
        #endregion

        #region Error Handling
        
        /// <summary>
        /// Handles dispatcher errors by generating diagnostics and sending notifications.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        protected virtual void HandleDispatcherError(Exception exception)
        {
            // Generate diagnostic information and send notification email
            var diagnosticInfo = EmailHelperService.GenerateDiagnosticDictionary(exception, default);
            diagnosticInfo["ProcessName"] = DispatcherConfig.ProcessName;
            var (subject, body) = EmailHelperService.PrepareEmailTemplate(SharedConfig.Email_FrameEx, diagnosticInfo, Log);
            var screenshot = MiscHelperService.TakeScreenshot(SharedConfig.Folder_ExScreenshots, null, DispatcherConfig.ProcessName, Log);
            workflows.SendEmail(
                SharedConfig.SysEx_To, 
                SharedConfig.Email_FrameEx, 
                default, 
                new [] {screenshot}, 
                default
            );
        }
        
        #endregion

        #region Equality and Hash Code
        
        /// <summary>
        /// Determines equality based on dispatcher configuration.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DispatcherWorkflow workflow &&
                   TestId == workflow.TestId &&
                   ConfigPath == workflow.ConfigPath &&
                   System.Collections.Generic.EqualityComparer<string[]>.Default.Equals(ConfigScopes, workflow.ConfigScopes);
        }

        /// <summary>
        /// Gets hash code based on dispatcher configuration.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(TestId, ConfigPath, ConfigScopes);
        }
        
        #endregion
    }
}
