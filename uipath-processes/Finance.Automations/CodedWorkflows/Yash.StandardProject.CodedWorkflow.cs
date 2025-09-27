using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Finance.Automations.ObjectRepository;
using UiPath.Activities.System.Jobs.Coded;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using UiPath.UIAutomationNext.API.Contracts;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;
using UiPath.Platform.ResourceHandling;
using UiPath.Activities.Api.Base;
using Finance.Automations.Configs;
using UiPath.CodedWorkflows;
using Yash.Config;
using Yash.Orchestrator;

namespace Finance.Automations.CodedWorkflows
{
    public class BaseCodedWorkflow : CodedWorkflow
    {
        private void LogConfigApi(string message, TraceEventType type){
            Log(message, ConvertTraceEventToLogLevel(type));
        }
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
            
        
        
        public (
            SharedConfig? shared,
            PerformerConfig? performer,
            DispatcherConfig? dispatcher,
            ReporterConfig? reporter
        )
        LoadConfig(string path, string[] scopes) {
            var access = services.Container.Resolve<IAccessProvider>();
            var orch = new OrchestratorService(access, LogConfigApi, TraceEventType.Verbose);
            var config_Shared = ConfigService.TryLoadStrictConfigAsync<SharedConfig>(path, orch, "Shared", LogConfigApi, TraceEventType.Verbose).Result;
            SharedConfig shared = null;
            PerformerConfig performer = null;
            DispatcherConfig dispatcher = null;
            ReporterConfig reporter = null;
            
            foreach(var scope in scopes){
                switch(scope)   {
                    case "Dispatcher":
                        dispatcher = ConfigService.TryLoadStrictConfigAsync<DispatcherConfig>(path, orch, "Dispatcher", LogConfigApi, TraceEventType.Verbose).Result;
                        break;
                    case "Shared":
                        shared = ConfigService.TryLoadStrictConfigAsync<SharedConfig>(path, orch, "Shared", LogConfigApi, TraceEventType.Verbose).Result;
                        break;
                    case "Performer":
                        performer = ConfigService.TryLoadStrictConfigAsync<PerformerConfig>(path, orch, "Performer", LogConfigApi, TraceEventType.Verbose).Result;
                        break;
                    case "Reporter":
                        reporter = ConfigService.TryLoadStrictConfigAsync<ReporterConfig>(path, orch, "Reporter", LogConfigApi, TraceEventType.Verbose).Result;
                        break;
                    default:
                        break;
                        
                }
            }
            return (shared, performer, dispatcher, reporter);
        }
    }
}