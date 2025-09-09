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
using System.Net.Http.Headers;
using System.ComponentModel;
using Yash.Orchestrator;
using System.Security;

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
    public class LoadConfig : AsyncCodeActivity<Dictionary<string, object>>
    {
        public InArgument<string> WorkbookPath { get; set; }
        public InArgument<string> Scope { get; set; }
        public InArgument<string> BaseUrl { get; set; }

        public InArgument<string> ClientId { get; set; }
        public InArgument<SecureString> ClientSecret { get; set; }

        private IExecutorRuntime _runtime;
        private OrchestratorService _orchestratorService;

        private string _path;
        private string _scope;
        private string _baseUrl;
        private SecureString _clientSecret;
        private string _clientId;

        public LoadConfig()
        {
        }
        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();

            _path = WorkbookPath.Get(context);
            _scope = Scope.Get(context);
            _baseUrl = BaseUrl.Get(context);
            _clientId = ClientId.Get(context);
            _clientSecret = ClientSecret.Get(context);
            Log($"_baseUrl='{_baseUrl}', _clientId='{_clientId}', _clientSecret is null={_clientSecret == null}");
            _orchestratorService = new(_baseUrl, _clientId, new System.Net.NetworkCredential("", _clientSecret).Password, new string[] { "OR.Folders.Read", "OR.Assets.Read" }, Log);

            if (string.IsNullOrWhiteSpace(_path))
                throw new LoadConfigException("Workbook path is required.");

            Log("Starting config load for: " + _path);

            var task = RunWorkflowAsync(context);
            return TaskHelpers.BeginTask(task, callback, state);
        }

        private async Task<Dictionary<string, object>> RunWorkflowAsync(AsyncCodeActivityContext context)
        {
            var file = ConfigService.ReadConfigFile(_path, Log);
            return await ConfigService.LoadConfigAsync(file, _scope, _baseUrl, _clientId, _clientSecret, Log);
        }


        protected override Dictionary<string, object> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var output = TaskHelpers.EndTask<Dictionary<string, object>>(result);
            return output; // Safe, task is already awaited
        }

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            _runtime?.LogMessage(new LogMessage
            {
                EventType = level,
                Message = msg
            });
        }
    }
}
