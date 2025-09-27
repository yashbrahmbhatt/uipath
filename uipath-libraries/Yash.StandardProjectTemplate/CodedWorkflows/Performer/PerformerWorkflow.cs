/*
 * File: PerformerWorkflow.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Abstract base class for Performer workflows that provides
 *              common performer functionality including state machine pattern,
 *              transaction processing, and standardized execution patterns.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;
using System.Collections.Generic;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using Yash.Config.Models;
using Yash.Utility;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject.CodedWorkflows.Performer
{
    /// <summary>
    /// Abstract base class for Performer workflows.
    /// Provides common performer functionality including state machine pattern,
    /// transaction processing, error handling, and application lifecycle management.
    /// </summary>
    public abstract class PerformerWorkflow : CodedWorkflowWithConfig
    {
        #region State Machine Variables
        
        /// <summary>
        /// State machine stack for managing workflow states.
        /// </summary>
        protected Stack<Action> StateStack { get; private set; }
        
        /// <summary>
        /// Current transaction item being processed.
        /// </summary>
        protected QueueItem TransactionItem { get; set; }
        
        /// <summary>
        /// Business exception that occurred during processing.
        /// </summary>
        protected Exception BusinessException { get; set; }
        
        /// <summary>
        /// System exception that occurred during processing.
        /// </summary>
        protected Exception SystemException { get; set; }
        
        /// <summary>
        /// Framework exception that occurred during processing.
        /// </summary>
        protected Exception FrameworkException { get; set; }
        
        /// <summary>
        /// Counter for consecutive system exceptions.
        /// </summary>
        protected int ConsecutiveSysEx { get; set; }
        
        /// <summary>
        /// Flag indicating if stop was requested.
        /// </summary>
        protected bool StopRequested { get; set; }
        
        /// <summary>
        /// Flag indicating if it's maintenance time.
        /// </summary>
        protected bool IsMaintenanceTime { get; set; }
        
        #endregion

        #region Main Workflow Entry Points
        
        /// <summary>
        /// Main workflow entry point that accepts parameters for flexible execution.
        /// </summary>
        /// <param name="configPath">The path to the configuration file.</param>
        /// <param name="configScopes">Configuration scopes to load.</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        public virtual void Execute(
            string configPath = "Data\\Config.xlsx",
            string[] configScopes = null,
            string testId = ""
        )
        {
            // Set properties for this execution
            this.ConfigPath = configPath;
            this.ConfigScopes = configScopes ?? new[] { "Shared", "Performer" };
            this.TestId = testId;
            
            // Initialize configuration first
            InitializeConfiguration();
            
            // Execute the performer logic
            ExecutePerformerLogic();
        }
        
        #endregion

        #region Core Performer Logic
        
        /// <summary>
        /// Executes the main performer workflow logic with standardized error handling.
        /// </summary>
        protected virtual void ExecutePerformerLogic()
        {
            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Performer" });

            // Initialize state machine
            InitializeStateMachine();

            Log("Starting Performer workflow", LogLevel.Info);

            // Push initial state
            StateStack.Push(State_Initialization);

            // Execute state machine
            while (StateStack.Count > 0)
            {
                var currentState = StateStack.Pop();
                currentState?.Invoke();
            }

            Log("Performer workflow completed successfully", LogLevel.Info);
        }
        
        /// <summary>
        /// Initializes the state machine variables.
        /// </summary>
        protected virtual void InitializeStateMachine()
        {
            StateStack = new Stack<Action>();
            ConsecutiveSysEx = 0;
            StopRequested = false;
            IsMaintenanceTime = false;
            FrameworkException = null;
            BusinessException = null;
            SystemException = null;
            TransactionItem = null;
        }
        
        #endregion

        #region Abstract Methods for Customization
        
        /// <summary>
        /// Processes the specific business logic for a transaction.
        /// Override this method to implement specific transaction processing logic.
        /// </summary>
        /// <param name="transactionItem">The transaction item to process</param>
        protected abstract void ProcessTransactionData(QueueItem transactionItem);
        
        /// <summary>
        /// Closes applications with fallback to process killing.
        /// </summary>
        public abstract void CloseApplications();
        
        /// <summary>
        /// Initializes required applications.
        /// </summary>
        public abstract void InitializeApplications();
        
        #endregion

        #region State Machine States
        
        /// <summary>
        /// Initialization state - sets up applications and validates prerequisites.
        /// </summary>
        protected virtual void State_Initialization()
        {
            Log("Entering Initialization state", LogLevel.Info);

            try
            {
                // Check if maintenance time
                IsMaintenanceTime = MiscHelperService.IsMaintenanceTime(
                    SharedConfig.Maintenance_Start, 
                    SharedConfig.Maintenance_End, 
                    default);

                if (IsMaintenanceTime)
                {
                    Log("Maintenance time detected during initialization", LogLevel.Info);
                    StateStack.Push(State_End);
                    return;
                }

                // Check consecutive system exceptions
                if (ConsecutiveSysEx >= SharedConfig.MaxConsecutiveExceptions)
                {
                    Log($"Maximum consecutive exceptions reached: {ConsecutiveSysEx}", LogLevel.Error);
                    FrameworkException = new Exception($"Maximum consecutive system exceptions ({ConsecutiveSysEx}) reached");
                    StateStack.Push(State_End);
                    return;
                }

                // Close applications gracefully
                CloseApplicationsGracefully();

                // Initialize applications
                InitializeApplications();

                Log("Initialization completed successfully", LogLevel.Info);

                // Transition to GetTransactionData state
                StateStack.Push(State_GetTransactionData);
            }
            catch (Exception ex)
            {
                Log($"Initialization failed: {ex.Message}", LogLevel.Error);
                FrameworkException = ex;
                StateStack.Push(State_End);
            }
        }

        /// <summary>
        /// GetTransactionData state - retrieves the next transaction from the queue.
        /// </summary>
        protected virtual void State_GetTransactionData()
        {
            Log("Entering GetTransactionData state", LogLevel.Info);

            try
            {
                StopRequested = workflows.IsStopRequested();
                
                // Check for stop signal
                if (StopRequested)
                {
                    Log("Stop requested, ending workflow", LogLevel.Info);
                    StateStack.Push(State_End);
                    return;
                }

                // Check maintenance time
                IsMaintenanceTime = MiscHelperService.IsMaintenanceTime(
                    SharedConfig.Maintenance_Start, 
                    SharedConfig.Maintenance_End, 
                    default);
                    
                if (IsMaintenanceTime)
                {
                    Log("Maintenance time detected, ending workflow", LogLevel.Info);
                    StateStack.Push(State_End);
                    return;
                }

                // Get next transaction from queue
                TransactionItem = GetNextTransaction();

                if (TransactionItem != null)
                {
                    Log($"Retrieved transaction: {TransactionItem.Reference}", LogLevel.Info);
                    StateStack.Push(State_ProcessTransaction);
                }
                else
                {
                    Log("No transactions available in queue", LogLevel.Info);
                    StateStack.Push(State_End);
                }
            }
            catch (Exception ex)
            {
                Log($"Error getting transaction data: {ex.Message}", LogLevel.Error);
                FrameworkException = ex;
                StateStack.Push(State_End);
            }
        }

        /// <summary>
        /// ProcessTransaction state - processes the current transaction.
        /// </summary>
        protected virtual void State_ProcessTransaction()
        {
            Log($"Entering ProcessTransaction state for: {TransactionItem?.Reference}", LogLevel.Info);

            try
            {
                // Handle test variations first (may throw exceptions for testing)
                HandleTestVariations();
                
                // Execute main transaction processing logic
                Log($"Processing transaction: {TransactionItem.Reference}", LogLevel.Info);
                ProcessTransactionData(TransactionItem);

                // Mark transaction as successful
                system.SetTransactionStatus(TransactionItem, ProcessingStatus.Successful, "Transaction processed successfully");

                // Reset consecutive system exceptions on success
                ConsecutiveSysEx = 0;
                SystemException = null;
                BusinessException = null;

                Log($"Transaction {TransactionItem.Reference} completed successfully", LogLevel.Info);

                // Go back to get next transaction
                StateStack.Push(State_GetTransactionData);
            }
            catch (BusinessRuleException ex)
            {
                HandleBusinessException(ex);
                StateStack.Push(State_GetTransactionData);
            }
            catch (Exception ex)
            {
                HandleSystemException(ex);
                StateStack.Push(State_Initialization);
            }
        }

        /// <summary>
        /// End state - cleanup and final processing.
        /// </summary>
        protected virtual void State_End()
        {
            Log("Entering End state", LogLevel.Info);

            try
            {
                CloseApplicationsGracefully();
            }
            catch (Exception ex)
            {
                Log($"Error closing applications: {ex.Message}", LogLevel.Warn);
            }

            // Generate final report if needed
            if (FrameworkException != null)
            {
                HandleFrameworkException(FrameworkException);
                throw FrameworkException;
            }
            else if (IsMaintenanceTime)
            {
                Log("Workflow ended due to maintenance time", LogLevel.Info);
            }
            else
            {
                Log("Workflow ended normally", LogLevel.Info);
            }

            // Don't push any more states - this ends the state machine
        }
        
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Gets the next transaction from the queue.
        /// </summary>
        /// <returns>Next transaction item or null if no data available</returns>
        protected virtual QueueItem GetNextTransaction()
        {
            return system.GetTransactionItem(
                SharedConfig.QueueName, 
                SharedConfig.QueueFolder, 
                ReferenceFilterStrategy.StartsWith, 
                default, 
                default);
        }
        
        /// <summary>
        /// Handles test variations for exception scenarios.
        /// Override this method in test classes if needed.
        /// </summary>
        protected virtual void HandleTestVariations()
        {
            // Implement test-specific behavior based on TestId
            if (!string.IsNullOrEmpty(TestId))
            {
                Log($"Applying test behavior for TestId: {TestId}", LogLevel.Info);
                
                switch (TestId.ToLower())
                {
                    case "systemexception":
                    case "sysex":
                        throw new ApplicationException($"Test system exception triggered by TestId: {TestId}");
                        
                    case "businessexception":
                    case "busex":
                        throw new BusinessRuleException($"Test business exception triggered by TestId: {TestId}");
                        
                    case "frameworkexception":
                    case "frameex":
                        throw new InvalidOperationException($"Test framework exception triggered by TestId: {TestId}");
                        
                    default:
                        // Normal execution for other test IDs
                        Log($"Normal execution with TestId: {TestId}", LogLevel.Info);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Closes applications gracefully with fallback to process killing.
        /// </summary>
        protected virtual void CloseApplicationsGracefully()
        {
            try
            {
                CloseApplications();
            }
            catch (Exception ex)
            {
                Log($"Failed to close applications gracefully, killing processes: {ex.Message}", LogLevel.Warn);

                // Kill processes as fallback
                foreach (var process in PerformerConfig.ProcessesToKill)
                {
                    try
                    {
                        workflows.KillProcess(process);
                    }
                    catch (Exception killEx)
                    {
                        Log($"Failed to kill process {process}: {killEx.Message}", LogLevel.Warn);
                    }
                }
            }
        }
        
        #endregion

        #region Error Handling
        
        /// <summary>
        /// Handles business exceptions by logging and sending notifications.
        /// </summary>
        /// <param name="ex">The business exception that occurred</param>
        protected virtual void HandleBusinessException(BusinessRuleException ex)
        {
            Log($"Business exception in transaction {TransactionItem?.Reference}: {ex.Message}", LogLevel.Warn);
            BusinessException = ex;

            try
            {
                // Set transaction as failed with business exception
                system.SetTransactionStatus(
                    TransactionItem, 
                    ProcessingStatus.Failed, 
                    SharedConfig.QueueFolder, 
                    new(), 
                    new(), 
                    ex.StackTrace, 
                    ErrorType.Business, 
                    ex.Message, 
                    default);

                // Send business exception email
                var diagnosticInfo = EmailHelperService.GenerateDiagnosticDictionary(ex, TransactionItem);
                diagnosticInfo["ProcessName"] = PerformerConfig.ProcessName;
                diagnosticInfo["Reference"] = TransactionItem.Reference;
                var (subject, body) = EmailHelperService.DefaultEmailTemplates.BusEx.PrepareEmailTemplate(diagnosticInfo, Log);
                workflows.SendEmail(SharedConfig.BusEx_To, body, subject, default, default);
            }
            catch (Exception emailEx)
            {
                Log($"Failed to send business exception email: {emailEx.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Handles system exceptions by logging and sending notifications.
        /// </summary>
        /// <param name="ex">The system exception that occurred</param>
        protected virtual void HandleSystemException(Exception ex)
        {
            Log($"System exception in transaction {TransactionItem?.Reference}: {ex.Message}", LogLevel.Error);
            SystemException = ex;
            ConsecutiveSysEx++;

            try
            {
                // Set transaction as failed with system exception
                system.SetTransactionStatus(
                    TransactionItem, 
                    ProcessingStatus.Failed, 
                    SharedConfig.QueueFolder, 
                    new(), 
                    new(), 
                    ex.StackTrace, 
                    ErrorType.Application, 
                    ex.Message, 
                    default);

                // Send system exception email
                var diagnosticInfo = EmailHelperService.GenerateDiagnosticDictionary(ex, TransactionItem);
                diagnosticInfo["ProcessName"] = PerformerConfig.ProcessName;
                diagnosticInfo["Reference"] = TransactionItem.Reference;
                var (subject, body) = EmailHelperService.DefaultEmailTemplates.SysEx.PrepareEmailTemplate(diagnosticInfo, Log);
                var screenshot = MiscHelperService.TakeScreenshot(
                    SharedConfig.Folder_ExScreenshots, 
                    default, 
                    $"Performer[{TransactionItem.Reference}]", 
                    Log);
                workflows.SendEmail(SharedConfig.SysEx_To, SharedConfig.Email_SysEx, default, new[] { screenshot }, default);
            }
            catch (Exception emailEx)
            {
                Log($"Failed to send system exception email: {emailEx.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Handles framework exceptions by logging and sending notifications.
        /// </summary>
        /// <param name="ex">The framework exception that occurred</param>
        protected virtual void HandleFrameworkException(Exception ex)
        {
            Log($"Workflow ended due to framework exception: {ex.Message}", LogLevel.Error);
            
            var diagnosticInfo = EmailHelperService.GenerateDiagnosticDictionary(ex, TransactionItem);
            diagnosticInfo["ProcessName"] = PerformerConfig.ProcessName;
            var (subject, body) = EmailHelperService.DefaultEmailTemplates.FrameEx.PrepareEmailTemplate(diagnosticInfo, Log);
            var screenshot = MiscHelperService.TakeScreenshot(SharedConfig.Folder_ExScreenshots, default, "Framework", Log);
            workflows.SendEmail(SharedConfig.SysEx_To, body, subject, new[] { screenshot }, default);
        }
        
        #endregion

        #region Equality and Hash Code
        
        /// <summary>
        /// Determines equality based on performer configuration.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is PerformerWorkflow workflow &&
                   TestId == workflow.TestId &&
                   ConfigPath == workflow.ConfigPath &&
                   System.Collections.Generic.EqualityComparer<string[]>.Default.Equals(ConfigScopes, workflow.ConfigScopes);
        }

        /// <summary>
        /// Gets hash code based on performer configuration.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(TestId, ConfigPath, ConfigScopes);
        }
        
        #endregion
    }
}