using System;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Orchestrator.Client.Models;
using Yash.StandardProject.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows.Performer;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._02_Performer
{
    /// <summary>
    /// Performer workflow that processes queue items using a state machine pattern.
    /// Inherits from PerformerWorkflow to leverage standardized performer functionality and patterns.
    /// </summary>
    public class Performer : PerformerWorkflow
    {
        #region Abstract Property Implementations
        
        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        
        /// <summary>
        /// Configuration scopes required for the performer workflow.
        /// </summary>
        public override string[] ConfigScopes { get; set; } = { "Shared", "Performer" };
        
        #endregion

        #region Implementation of Abstract Methods
        
        /// <summary>
        /// Processes the specific business logic for a transaction.
        /// Override this method to implement specific transaction processing logic.
        /// </summary>
        /// <param name="transactionItem">The transaction item to process</param>
        protected override void ProcessTransactionData(QueueItem transactionItem)
        {
            // Execute main transaction processing logic
            workflows.PerformerProcess(SharedConfig, PerformerConfig, transactionItem);
        }
        
        /// <summary>
        /// Closes applications with fallback to process killing.
        /// </summary>
        public override void CloseApplications()
        {
            workflows.PerformerCloseApplications();
        }
        
        /// <summary>
        /// Initializes required applications.
        /// </summary>
        public override void InitializeApplications()
        {
            workflows.PerformerInitializeApplications(SharedConfig, PerformerConfig);
        }
        
        #endregion

        #region Workflow Execution
        
        /// <summary>
        /// Main workflow entry point that accepts parameters for flexible execution.
        /// </summary>
        /// <param name="configPath">The path to the configuration file.</param>
        /// <param name="configScopes">Configuration scopes to load.</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        [Workflow]
        public override void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId;
            base.Execute(ConfigPath, ConfigScopes, TestId);
        }
        
        #endregion
    }
}