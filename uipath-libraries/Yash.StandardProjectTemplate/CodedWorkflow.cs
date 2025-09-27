using UiPath.Testing.API;
using UiPath.CodedWorkflows.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using UiPath.Activities.Api.Base;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Mail.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using Yash.Config;
using Yash.Orchestrator;
namespace Yash.StandardProject
{
    public partial class CodedWorkflow : CodedWorkflowBase
    {  
        /*
        protected OrchestratorService orchService => services.Container.Resolve<OrchestratorService>();
        public new void RegisterServices(ICodedWorkflowsServiceLocator locator){
            base.RegisterServices(locator);
            Log("[CodedWorkflow] Registering Orchestrator Service");
            var access = services.Container.Resolve<IAccessProvider>();
            Log("[CodedWorkflow] Access is null?: " + (access == null).ToString());
            var orch = new OrchestratorService(access, Log);
            locator.RegisterInstance(orch, new[]{typeof(IOrchestratorService), typeof(OrchestratorService)});
        }
        */
        public ITestingService test => testing;

        
        
        #region Logging Utilities

        /// <summary>
        /// Logs API messages with appropriate trace level conversion.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="type">The trace event type</param>
        public void Log(string message, TraceEventType type) => Log(message, ConvertTraceEventToLogLevel(type));
        public void Log(string message, LogLevel level = LogLevel.Info, IDictionary<string, object> additionalFields = null) => base.Log(message, level, additionalFields);
        /// <summary>
        /// Converts TraceEventType to UiPath LogLevel.
        /// </summary>
        /// <param name="type">The trace event type to convert</param>
        /// <returns>Corresponding UiPath LogLevel</returns>
        public LogLevel ConvertTraceEventToLogLevel(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Verbose:
                    return LogLevel.Trace;
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Resume:
                case TraceEventType.Information:
                    return LogLevel.Info;
                case TraceEventType.Warning:
                    return LogLevel.Warn;
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    return LogLevel.Error;
                default:
                    throw new Exception("Could not convert TraceEvent to LogLevel: " + TraceEventType.GetName(type));
            }
        }

        #endregion
    }
}