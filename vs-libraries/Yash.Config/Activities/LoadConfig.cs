using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Statements;
using TraceEventType = System.Diagnostics.TraceEventType;
using UiPath.Robot.Activities.Api;
using Yash.Config.Helpers;
using Yash.Config.Models;
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
        public InArgument<string> WorkbookPath { get; set; }
        public Type? Scope { get; set; }
        public bool DebugMode { get; set; } = false;
        private string _scopeName => Scope?.Name.Replace("Config","") ?? "";
        private bool _scopeExists => Scope != null;


        //public InArgument<string> BaseUrl { get; set; }

        //public InArgument<string> ClientId { get; set; }
        //public InArgument<SecureString> ClientSecret { get; set; }

        private IExecutorRuntime _runtime;
        private IAccessProvider _access;

        private string _path;
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
                $"_scopeExists={_scopeExists}\n"
            );

            if (string.IsNullOrWhiteSpace(_path))
                throw new LoadConfigException("Workbook path is required.");

            Log("Starting config load for: " + _path);

            var task = RunWorkflowAsync(context);
            return TaskHelpers.BeginTask(task, callback, state);
        }

        private async Task<T> RunWorkflowAsync(AsyncCodeActivityContext context)
        {
            
            var meta = ConfigService.ValidateConfigFile(_path, Log);
            if(meta.ConfigFileError != null) {
                switch (meta.ConfigFileError)
                {
                    case ConfigService.ConfigFileError.NullValue:
                        throw new LoadConfigException("The specified configuration file path is null or empty", TraceEventType.Error);
                    case ConfigService.ConfigFileError.FileNotFound:
                        throw new LoadConfigException("The specified configuration file was not found", TraceEventType.Error);
                    case ConfigService.ConfigFileError.NotExcelFile:
                        throw new LoadConfigException("The specified file is not a valid Excel file.", TraceEventType.Error);
                }
            }
            var loadConfig = await ConfigService.TryLoadConfigWithAccessProviderAsync(meta, _scopeName, _access, Log, DebugMode ? TraceEventType.Information : TraceEventType.Verbose);
            if(loadConfig == null)
                throw new LoadConfigException("Failed to load configuration from the specified file.", TraceEventType.Error);
            if(loadConfig.ConfigByScope.Count == 0)
                throw new LoadConfigException("No scopes found in the configuration file.", TraceEventType.Error);
            if (_scopeExists && !loadConfig.ConfigByScope.ContainsKey(_scopeName))
                throw new LoadConfigException($"The specified scope '{_scopeName}' was not found in the configuration file.", TraceEventType.Error);

            var type = typeof(T);
            if (type.IsAssignableFrom(typeof(Models.Config)) && type.HasParameterlessConstructor())
                return (T)ConfigFactory.FromDictionary(type,  loadConfig.Config ?? new (), Log);
            else if (type == typeof(Dictionary<string, object>))
                return (T)(object)loadConfig.Config ?? (T)(object)new Dictionary<string, object>();
            else
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(loadConfig.Config));
        }


        protected override T EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var output = TaskHelpers.EndTask<T>(result);
            return output; // Safe, task is already awaited
        }

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            if (!DebugMode && level == TraceEventType.Verbose) return;
            _runtime?.LogMessage(new LogMessage
            {
                EventType = level,
                Message = msg
            });
        }
    }
}
