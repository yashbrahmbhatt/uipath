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
using System.Activities.ViewModels;
using Yash.Frameworks.Activities.ViewModels.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Markup;

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


    public class LazyFramework : CodeActivity<LazyFrameworkResult>
    {
        
        public Activity Framework_Initialize { get; set; }

        public Activity Framework_Settings { get; set; }

        public bool EnableSettings { get; set; } = false;
        public bool EnableInitialize { get; set; } = false;

        [Browsable(false)]

        private IExecutorRuntime? _runtime;

        public LazyFramework()
        {
            Framework_Settings =
                new Sequence
                {
                    DisplayName = "Initialize Settings Workflow"
                };
            Framework_Initialize = new Sequence
            {
                DisplayName = "Initialize Framework Workflow"
            };
        }

        public LazyFramework(IDesignServices serivices) : base()
        {
        }
        

        #region Activity Implementation


        protected override LazyFrameworkResult Execute(CodeActivityContext context)
        {
            _runtime = ActivityContextExtensions.GetExecutorRuntime(context);
            var doConfig = EnableSettings && Framework_Settings != null;
            var doInit = EnableInitialize && Framework_Initialize != null;

            var result = ExecuteInternal(Framework_Settings,Framework_Initialize,doConfig,doInit); 
            Result.Set(context, new LazyFrameworkResult());
            return new LazyFrameworkResult();
        }

        public LazyFrameworkResult ExecuteInternal(Activity? initConfig, Activity? initApps, bool doConfig, bool doInit)
        {
            try
            {
                if (doConfig && initConfig != null)
                {
                    Log("Starting Settings Initialization");
                    WorkflowInvoker.Invoke(initConfig, new Dictionary<string, object?>());
                    Log("Settings Initialization Completed");
                }
                if (doInit && initApps != null)
                {
                    Log("Starting Framework Initialization");
                    WorkflowInvoker.Invoke(initApps, new Dictionary<string, object?>());
                    Log("Framework Initialization Completed");
                }
            }
            catch (Exception ex)
            {
                throw new LazyFrameworkException("Error during framework initialization", ex);
            }
            return new LazyFrameworkResult();
        }

        #endregion

        #region Utility

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            _runtime?.LogMessage(new UiPath.Robot.Activities.Api.LogMessage
            {
                EventType = level,
                Message = msg
            });
        }

       

        #endregion

        #region Generative Methods


        #endregion
    }
    public class LazyFrameworkResult
    {
    }
}
