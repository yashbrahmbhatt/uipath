using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Finance.Automations.CodedWorkflows;
using Finance.Automations.Configs;
using UiPath.Activities.Api.Base;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
using Yash.Config;
using Yash.Config.Models;
using Yash.Orchestrator;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Finance.Automations._02_Performer
{
    public class Performer : BaseCodedWorkflow
    {
        // State machine variables
        private Stack<Action> _stateStack;
        private SharedConfig _config_Shared;
        private PerformerConfig _config_Performer;
        private QueueItem _transactionItem;
        private Exception _businessException;
        private Exception _systemException;
        private Exception _frameworkException;
        private int _consecutiveSysEx;
        private bool _stopRequested;
        private bool _isMaintenanceTime;
        private string _testId;

        [Workflow]
        public void Execute(
            string ConfigPath = "Data\\Config.xlsx",
            string TestId = "")
        {
            // Initialize state machine
            _testId = TestId;
            _stateStack = new Stack<Action>();
            _consecutiveSysEx = 0;
            _stopRequested = false;
            _isMaintenanceTime = false;
            _frameworkException = null;

            // Load configuration
            (_config_Shared, _config_Performer, _, _) = LoadConfig(ConfigPath, new [] {"Shared","Performer"});
            

            Log("Starting Performer workflow", LogLevel.Info);

            // Push initial state
            _stateStack.Push(State_Initialization);

            // Execute state machine
            while (_stateStack.Count > 0)
            {
                var currentState = _stateStack.Pop();
                currentState?.Invoke();
            }

            Log("Performer workflow completed successfully", LogLevel.Info);


        }

        #region State Machine States

        private void State_Initialization()
        {
            Log("Entering Initialization state", LogLevel.Info);

            try
            {
                // Test variation handling
                if (_testId == "Performer.InitializationError")
                {
                    throw new Exception(_testId);
                }
                
                // Check if maintenance time or test for maintenance time scenario
                _isMaintenanceTime = workflows.IsMaintenanceTime(_config_Shared.Maintenance_Start, _config_Shared.Maintenance_End, default) || _testId == "Performer.MaintenanceTime";

                if (_isMaintenanceTime)
                {
                    Log("Maintenance time detected during initialization", LogLevel.Info);
                    _stateStack.Push(State_End);
                    return;
                }

                // Check consecutive system exceptions
                if (_consecutiveSysEx >= _config_Shared.MaxConsecutiveExceptions)
                {
                    Log($"Maximum consecutive exceptions reached: {_consecutiveSysEx}", LogLevel.Error);
                    _frameworkException = new Exception($"Maximum consecutive system exceptions ({_consecutiveSysEx}) reached");
                    _stateStack.Push(State_End);
                    return;
                }

                try
                {
                    // Close applications
                    workflows.PerformerCloseApplications();
                }
                catch (Exception ex)
                {
                    Log($"Failed to close applications gracefully, killing processes: {ex.Message}", LogLevel.Warn);

                    // Kill processes as fallback
                    foreach (var process in _config_Performer.ProcessesToKill)
                    {
                        try
                        {
                            workflows.Kill_Process(process);
                        }
                        catch (Exception killEx)
                        {
                            Log($"Failed to kill process {process}: {killEx.Message}", LogLevel.Warn);
                        }
                    }
                }

                // Initialize applications
                workflows.PerformerInitializeApplications(_config_Shared, _config_Performer);

                Log("Initialization completed successfully", LogLevel.Info);

                // Transition to GetTransactionData state
                _stateStack.Push(State_GetTransactionData);
            }
            catch (Exception ex)
            {
                Log($"Initialization failed: {ex.Message}", LogLevel.Error);
                _frameworkException = ex;

                _stateStack.Push(State_End);
            }
        }

        private void State_GetTransactionData()
        {
            Log("Entering GetTransactionData state", LogLevel.Info);

            try
            {
                _stopRequested = workflows.IsStopRequested();
                // Check for stop signal
                if (_stopRequested)
                {
                    Log("Stop requested, ending workflow", LogLevel.Info);
                    _stateStack.Push(State_End);
                    return;
                }

                // Check maintenance time or test for maintenance time scenario
                _isMaintenanceTime = workflows.IsMaintenanceTime(_config_Shared.Maintenance_Start, _config_Shared.Maintenance_End, default) || _testId == "Performer.MaintenanceTime";
                if (_isMaintenanceTime)
                {
                    Log("Maintenance time detected, ending workflow", LogLevel.Info);
                    _stateStack.Push(State_End);
                    return;
                }

                // Get next transaction from queue (or simulate no data for testing)
                if (_testId == "Performer.NoData")
                {
                    _transactionItem = null; // Simulate no data available
                }
                else
                {
                    _transactionItem = system.GetTransactionItem(_config_Shared.QueueName, _config_Shared.QueueFolder, ReferenceFilterStrategy.StartsWith, default, default);
                }

                if (_transactionItem != null)
                {
                    Log($"Retrieved transaction: {_transactionItem.Reference}", LogLevel.Info);
                    _stateStack.Push(State_ProcessTransaction);
                }
                else
                {
                    Log("No transactions available in queue", LogLevel.Info);
                    _stateStack.Push(State_End);
                }
            }
            catch (Exception ex)
            {
                Log($"Error getting transaction data: {ex.Message}", LogLevel.Error);
                _frameworkException = ex;

                _stateStack.Push(State_End);
            }
        }

        private void State_ProcessTransaction()
        {
            Log($"Entering ProcessTransaction state for: {_transactionItem?.Reference}", LogLevel.Info);

            try
            {

                // Test variation handling
                if (_testId == "Performer.BusinessException")
                {
                    throw new BusinessRuleException("Test business exception");
                }
                if (_testId == "Performer.SystemException")
                {
                    throw new Exception("Test system exception");
                }

                // TODO: Implement actual transaction processing logic here
                // This is where the main business logic would go
                Log($"Processing transaction: {_transactionItem.Reference}", LogLevel.Info);
                var output = workflows.PerformerProcess(_config_Shared, _config_Performer, _transactionItem);
                var outputDict = new Dictionary<string, object>(){
                    {"Response", output}
                };
                
                // Mark transaction as successful
                system.SetTransactionStatus(_transactionItem, ProcessingStatus.Successful, _config_Shared.QueueFolder, new(), outputDict, null, ErrorType.Application, null, default);

                // Reset consecutive system exceptions on success
                _consecutiveSysEx = 0;
                _systemException = null;
                _businessException = null;

                Log($"Transaction {_transactionItem.Reference} completed successfully", LogLevel.Info);

                // Go back to get next transaction
                _stateStack.Push(State_GetTransactionData);
            }
            catch (BusinessRuleException ex)
            {
                Log($"Business exception in transaction {_transactionItem?.Reference}: {ex.Message}", LogLevel.Warn);
                _businessException = ex;

                try
                {
                    // Set transaction as failed with business exception
                    system.SetTransactionStatus(_transactionItem, ProcessingStatus.Failed, _config_Shared.QueueFolder, new(), new(), ex.StackTrace, ErrorType.Business, ex.Message, default);

                    // Send business exception email
                    var dict_Diagnostic = workflows.GenerateDiagnosticDictionary(ex, _transactionItem);
                    dict_Diagnostic["ProcessName"] = _config_Performer.ProcessName;
                    dict_Diagnostic["Reference"] = _transactionItem.Reference;
                    workflows.SendEmail(_config_Shared.BusEx_To, _config_Shared.Email_BusEx, default, dict_Diagnostic,default);
                }
                catch (Exception emailEx)
                {
                    Log($"Failed to send business exception email: {emailEx.Message}", LogLevel.Error);
                }

                // Continue with next transaction
                _stateStack.Push(State_GetTransactionData);
            }
            catch (Exception ex)
            {
                Log($"System exception in transaction {_transactionItem?.Reference}: {ex.Message}", LogLevel.Error);
                _systemException = ex;
                _consecutiveSysEx++;

                try
                {
                    // Set transaction as failed with system exception
                    system.SetTransactionStatus(_transactionItem, ProcessingStatus.Failed, _config_Shared.QueueFolder, new(), new(), ex.StackTrace, ErrorType.Application, ex.Message, default);


                    // Send system exception email
                    var dict_Diagnostic = workflows.GenerateDiagnosticDictionary(ex, _transactionItem);
                    dict_Diagnostic["ProcessName"] = _config_Performer.ProcessName;
                     dict_Diagnostic["Reference"] = _transactionItem.Reference;
                    workflows.SendEmail(_config_Shared.SysEx_To, _config_Shared.Email_SysEx, default, dict_Diagnostic, default);
                }
                catch (Exception emailEx)
                {
                    Log($"Failed to send system exception email: {emailEx.Message}", LogLevel.Error);
                }

                // Go back to initialization to recover from system exception
                _stateStack.Push(State_Initialization);
            }
        }

        private void State_End()
        {
            Log("Entering End state", LogLevel.Info);

            try
            {
                // Close applications
                workflows.PerformerCloseApplications();
            }
            catch (Exception ex)
            {
                Log($"Error closing applications: {ex.Message}", LogLevel.Warn);

                // Kill processes as fallback
                foreach (var process in _config_Performer.ProcessesToKill)
                {
                    try
                    {
                        workflows.Kill_Process(process);
                    }
                    catch (Exception killEx)
                    {
                        Log($"Failed to kill process {process}: {killEx.Message}", LogLevel.Warn);
                    }
                }
            }

            // Generate final report if needed
            if (_frameworkException != null)
            {
                Log($"Workflow ended due to framework exception: {_frameworkException.Message}", LogLevel.Error);
                var dict_Diagnostic = workflows.GenerateDiagnosticDictionary(_frameworkException, _transactionItem);
                dict_Diagnostic["ProcessName"] = _config_Performer.ProcessName;
                workflows.SendEmail(_config_Shared.SysEx_To, _config_Shared.Email_FrameEx, null, dict_Diagnostic, null);
                throw _frameworkException;
            }
            else if (_isMaintenanceTime)
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
    }
}