using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using UiPath.Core.Activities;
using UiPath.Robot.Activities.Api;
using Yash.Config.Activities;
using Yash.Frameworks.Activities.ViewModels.Helpers;
using Activity = System.Activities.Activity;
using Formatting = Newtonsoft.Json.Formatting;
using LogMessage = UiPath.Robot.Activities.Api.LogMessage;

namespace Yash.Frameworks.Activities
{
    public class LazyFramework : CodeActivity// This base class exposes an OutArgument named Result
    {
        public Activity FrameworkInitializeConfig { get; set; }
        public Activity FrameworkInitializeApplications { get; set; }
        public Activity FrameworkCloseApplications { get; set; }
        public Activity FrameworkProcessTransaction { get; set; }
        public Activity FrameworkBusinessException { get; set; }
        public Activity FrameworkSystemException { get; set; }
        public Activity FrameworkSuccessful { get; set; }
        public Activity FrameworkEnd { get; set; }
        public InArgument<string> QueueName { get; set; }
        public InArgument<string> QueueFolder { get; set; }
        public bool EnableQueue { get; set; } = false;
        public bool EnableInitializeConfig { get; set; } = false;
        public bool EnableInitializeApplications { get; set; } = false;
        public bool EnableBusinessException { get; set; } = false;
        public bool EnableSystemException { get; set; } = false;
        public bool EnableSuccessful { get; set; } = false;
        public bool EnableEnd { get; set; } = false;
        public bool EnableEmailNotifications { get; set; } = false;

        [Browsable(false)]
        private IExecutorRuntime _executorRuntime;

        public LazyFramework()
        {
            FrameworkInitializeConfig = new Sequence()
            {
                DisplayName = "Initialize Settings",
            };
            FrameworkInitializeApplications = new Sequence()
            {
                DisplayName = "Initialize Applications",
            };
            FrameworkCloseApplications = new Sequence()
            {
                DisplayName = "Close Applications",
            };
            FrameworkProcessTransaction = new Sequence()
            {
                DisplayName = "Process Transaction",
            };
            FrameworkBusinessException = new Sequence()
            {
                DisplayName = "Business Exception",
            };
            FrameworkSystemException = new Sequence()
            {
                DisplayName = "System Exception",
            };
            FrameworkSuccessful = new Sequence()
            {
                DisplayName = "Successful",
            };
        }

        /*
         * The returned value will be used to set the value of the Result argument
         */
        protected override void Execute(CodeActivityContext context)
        {
            // This is how you can log messages from your activity. logs are sent to the Robot which will forward them to Orchestrator
            _executorRuntime = context.GetExecutorRuntime();

            var qName = QueueName.Get<string>(context);
            var qFolder = QueueFolder.Get<string>(context);

            Log($"Starting execution of LazyFramework with Queue: {qName}, Folder: {qFolder}", TraceEventType.Information);
            Log($"The primary screen resolution is: {SystemParameters.PrimaryScreenWidth.ToString()} x {SystemParameters.PrimaryScreenHeight.ToString()}", TraceEventType.Information);
            ExecuteInternal(qName, qFolder);

        }

        public void ExecuteInternal(string QueueName, string QueueFolder)
        {
            if (EnableInitializeConfig)
            {
                Log($"Executing Initialize Config");
                WorkflowInvoker.Invoke(FrameworkInitializeConfig);
                Log($"Completed Initialize Config");
            }
            if (EnableInitializeApplications)
            {
                Log($"Executing Initialize Applications");
                WorkflowInvoker.Invoke(FrameworkInitializeApplications);
                Log($"Completed Initialize Applications");
            }
            var asset = new GetRobotAsset()
            {
                FolderPath = "LazyFramework",
                AssetName = "ShouldLoadAsset",

            };
            Log($"Starting Transaction Processing for Queue: {QueueName}, Folder: {QueueFolder}");

        }

        public void Log(string message, TraceEventType level = TraceEventType.Verbose)
        {
            _executorRuntime.LogMessage(new LogMessage()
            {
                EventType = level,
                Message = message
            });
        }

        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
