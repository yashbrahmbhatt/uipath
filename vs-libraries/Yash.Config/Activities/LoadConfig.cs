using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Statements;
using UiPath.Robot.Activities.Api;
using System.Data;
using Newtonsoft.Json;
using UiPath.Activities.Api.Base;
using UiPath.Studio.Activities.Api;
using Newtonsoft.Json.Linq;
using Yash.Orchestrator;
using System.Net.Http.Headers;
using System.ComponentModel;
using System.Security;
using CsvHelper;
using Yash.Config.Models.Config;
using Yash.Config.ConfigurationService;
using UiPath.Core.Activities;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Yash.Config.Services;
using System.Activities.Presentation;
using Yash.Config.Helpers;

namespace Yash.Config.Activities
{
    public class LoadConfigException : Exception
    {
        public LoadConfigException(string message) : base("[LoadConfigAsync] " + message) { }
        public LoadConfigException(string message, Exception innerException) : base("[LoadConfigAsync] " + message, innerException) { }
        public LoadConfigException(string message, TraceEventType eventType) : base("[LoadConfigAsync] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }
    public class LoadConfig<T> : AsyncCodeActivity<T>
    {
        public InArgument<string?> WorkbookPath { get; set; } = null;
        public Type? Scope { get; set; } = typeof(T);
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string? DesignTimePath { get; set; } = null;
        public bool DebugMode { get; set; } = false;
        private string _scopeName => Scope?.Name.Replace("Config", "") ?? "";
        private bool _scopeExists => Scope != null;

        //public InArgument<string> BaseUrl { get; set; }

        //public InArgument<string> ClientId { get; set; }
        //public InArgument<SecureString> ClientSecret { get; set; }

        private IExecutorRuntime _runtime;
        private IAccessProvider _access;

        private string? _path;
        //private string _baseUrl;
        //private SecureString _clientSecret;
        //private string _clientId;
        public LoadConfig()
        {
        }
        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();
            _access = context.GetExtension<IAccessProvider>();

            _path = WorkbookPath.Get(context);
            //_baseUrl = BaseUrl.Get(context);
            //_clientId = ClientId.Get(context);
            //_clientSecret = ClientSecret.Get(context);
            Log(
               $"_path='{_path}'\n" +
               $"_scope='{_scopeName}'\n" +
                //$"_baseUrl='{_baseUrl}'\n" +
                //$"_clientId='{_clientId}'\n" +
                //$"_clientSecret is null={_clientSecret == null}\n" +
                $"_scopeExists={_scopeExists}\n" +
               $"Level={Level}\n" +
                $"DebugMode={DebugMode}\n" +
                $"DesignTimePath='{DesignTimePath}'\n" +
               $"Acces is null {_access == null}",
               LogLevel.Trace
            );

            if (string.IsNullOrWhiteSpace(_path))
                throw new LoadConfigException("Workbook path is required.");

            Log("Starting config load for: " + _path, LogLevel.Info);

            return TaskHelpers.BeginTask(RunWorkflowAsync(context), callback, state, Log);
        }

        private async Task<T> RunWorkflowAsync(AsyncCodeActivityContext context)
        {
            string orchestratorUrl = "";

            Log($"Creating ConfigService with path '{_path}' and access exists: {_access != null} and orchestrator url '{orchestratorUrl}'", LogLevel.Trace);
            
            // Create a simple service provider that provides the OrchestratorService
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            if (_access != null)
            {
                serviceCollection.AddSingleton<IOrchestratorService>(provider => new OrchestratorService(_access, Log, ConvertLogLevelToTraceEvent(Level)));
            }
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var config = new ConfigService(_path ?? "", serviceProvider, Log, ConvertLogLevelToTraceEvent(Level));
            Log($"ConfigService created with dependency injection support", LogLevel.Trace);
            Log("Validating config file...", LogLevel.Trace);
            config.ValidateConfigFile();
            var meta = config.Metadata;
            if (meta.ConfigFileError != null)
            {
                Log("Config file error detected: " + meta.ConfigFileError, LogLevel.Error);
                switch (meta.ConfigFileError)
                {
                    case ConfigFileError.NullValue:
                        throw new LoadConfigException("The specified configuration file path is null or empty", TraceEventType.Error);
                    case ConfigFileError.FileNotFound:
                        throw new LoadConfigException("The specified configuration file was not found", TraceEventType.Error);
                    case ConfigFileError.NotExcelFile:
                        throw new LoadConfigException("The specified file is not a valid Excel file.", TraceEventType.Error);
                }
            }
            Log("Loading config...", LogLevel.Trace);
            var loadConfig = await config.LoadConfigAsync(_scopeName);
            Log("Loaded config successfully.", LogLevel.Info);
            if (loadConfig == null)
                throw new LoadConfigException("Failed to load configuration from the specified file.", TraceEventType.Error);
            if (loadConfig.ConfigByScope.Count == 0)
                throw new LoadConfigException("No scopes found in the configuration file.", TraceEventType.Error);
            if (_scopeExists && !loadConfig.ConfigByScope.ContainsKey(_scopeName))
                throw new LoadConfigException($"The specified scope '{_scopeName}' was not found in the configuration file.", TraceEventType.Error);

            var type = typeof(T);
            if (type.IsAssignableFrom(typeof(Configuration)) && type.HasParameterlessConstructor())
            {
                Log($"Mapping config to '{type.Name}'...", LogLevel.Trace);
                return (T)ConfigFactory.FromDictionary(type, loadConfig.Config ?? new(), Log);
            }
            else if (type == typeof(Dictionary<string, object>))
            {
                Log($"Mapping config to 'Dictionary<string, object>'...", LogLevel.Trace);
                return (T)(object)loadConfig.Config ?? (T)(object)new Dictionary<string, object>();
            }
            else
            {
                Log($"Mapping config to '{type.Name}' via JSON serialization...", LogLevel.Trace);
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(loadConfig.Config));
            }
        }


        protected override T EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var output = TaskHelpers.EndTask<T>(result, Log);
            return output; // Safe, task is already awaited
        }

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            if (!DebugMode && level == TraceEventType.Verbose) return;
            _runtime?.LogMessage(new UiPath.Robot.Activities.Api.LogMessage
            {
                EventType = level,
                Message = msg
            });
        }
        private void  Log(string msg, LogLevel level = LogLevel.Info) => Log(msg, ConvertLogLevelToTraceEvent(level));
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
                    throw new Exception("Could not convert TraceEvent to LogLevel: " + Enum.GetName(type));
            }
        }
        public TraceEventType ConvertLogLevelToTraceEvent(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return TraceEventType.Verbose;
                case LogLevel.Info:
                    return TraceEventType.Information;
                case LogLevel.Warn:
                    return TraceEventType.Warning;
                case LogLevel.Error:
                    return TraceEventType.Error;
                default:
                    throw new Exception("Could not convert LogLevel to TraceEvent: " + level.ToString());
            }
        }
    }
}
