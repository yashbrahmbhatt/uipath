using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Statements;
using TraceEventType = System.Diagnostics.TraceEventType;
using UiPath.Robot.Activities.Api;
using System.Data;
using Newtonsoft.Json;
using UiPath.Activities.Api.Base;
using UiPath.Studio.Activities.Api;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.ComponentModel;
using Yash.Frameworks.Helpers;
using System.Activities.DesignViewModels;

namespace Yash.Frameworks.Activities
{
    public class LazyFrameworkException : Exception
    {
        public LazyFrameworkException(string message) : base("[LazyFramework] " + message) { }
        public LazyFrameworkException(string message, Exception innerException) : base("[LazyFramework] " + message, innerException) { }
        public LazyFrameworkException(string message, TraceEventType eventType) : base("[LazyFramework] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }
    public class LazyFramework : AsyncCodeActivity
    {
        public Activity Framework_Initialize { get; set; } = new Sequence();
        public Activity Framework_Config { get; set; } = new Sequence();
        public bool EnableConfig { get; set; } = true;


        private IExecutorRuntime _runtime;

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();


            var task = RunWorkflowAsync(context);
            return TaskHelpers.BeginTask(task, callback, state);
        }

        private async Task<Dictionary<string, object>> RunWorkflowAsync(AsyncCodeActivityContext context)
        {
            return new();
        }


        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            TaskHelpers.EndTask(result);
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
