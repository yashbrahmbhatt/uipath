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
    public class LazyFrameworkV2Exception : Exception
    {
        public LazyFrameworkV2Exception(string message) : base("[LazyFramework] " + message) { }
        public LazyFrameworkV2Exception(string message, Exception innerException) : base("[LazyFramework] " + message, innerException) { }
        public LazyFrameworkV2Exception(string message, TraceEventType eventType) : base("[LazyFramework] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }


    public class LazyFrameworkV2 : NativeActivity<LazyFrameworkResult>
    {
        [DefaultValue(null)]
        [DependsOn("EnableInitialize")]
        public Activity Framework_Initialize { get; set; }

        [DefaultValue(null)]
        [DependsOn("EnableSettings")]
        public Activity Framework_Settings { get; set; }

        public bool EnableSettings { get; set; } = false;
        public bool EnableInitialize { get; set; } = false;
        private IExecutorRuntime _runtime;
        internal static string ParentContainerPropertyTag => "LazyFramework";

        private readonly IObjectContainer _objectContainer;
        public LazyFrameworkV2()
        {
            _objectContainer = new ObjectContainer();
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
        

        #region Activity Implementation


        protected override void Execute(NativeActivityContext context)
        {
            _runtime = ActivityContextExtensions.GetExecutorRuntime(context);
            
            var result = new LazyFrameworkResult();
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
    public class LazyFrameworkV2Result
    {
    }
}
