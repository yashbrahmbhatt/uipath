using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using UiPath.Core;
using UiPath.Core.Activities;
using UiPath.Robot.Activities.Api;
using UiPath.Studio.Activities.Api;
using Yash.Frameworks.Activities.ViewModels.Helpers;
using Activity = System.Activities.Activity;
using Formatting = Newtonsoft.Json.Formatting;
using LogMessage = UiPath.Robot.Activities.Api.LogMessage;

namespace Yash.Frameworks.Activities
{
    public class LazyFramework : NativeActivity// This base class exposes an OutArgument named Result
    {
        public Activity FrameworkInitializeConfig { get; set; }
        public Activity FrameworkInitializeApplications { get; set; }
        public Activity FrameworkCloseApplications { get; set; }
        public Activity FrameworkProcessTransaction { get; set; }
        public Activity FrameworkBusinessException { get; set; }
        public Activity FrameworkSystemException { get; set; }
        public Activity FrameworkSuccessful { get; set; }
        public Activity FrameworkEnd { get; set; }
        public Activity FrameworkGetTransaction { get; set; }
        public bool EnableQueue { get; set; } = false;
        public InArgument<string> QueueName { get; set; }
        public InArgument<string> QueueFolder { get; set; }
        public bool EnableInitializeConfig { get; set; } = false;
        public bool EnableInitializeApplications { get; set; } = false;
        public bool EnableBusinessException { get; set; } = false;
        public bool EnableSystemException { get; set; } = false;
        public bool EnableSuccessful { get; set; } = false;
        public bool EnableEnd { get; set; } = false;
        public InArgument<QueueItem> QueueItem { get; set;}

        [Browsable(false)]
        private IExecutorRuntime _executorRuntime;
        [Browsable(false)]
        public IDesignServices _designServices;
        [Browsable(false)]
        private NativeActivityContext? _context;

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
            FrameworkEnd = new Sequence()
            {
                DisplayName = "End",
            };
            FrameworkGetTransaction = new Sequence()
            {
                DisplayName = "Get Transaction",
            };
        }

        /*
         * The returned value will be used to set the value of the Result argument
         */
        protected override void Execute(NativeActivityContext context)
        {
            // This is how you can log messages from your activity. logs are sent to the Robot which will forward them to Orchestrator
            _executorRuntime = context.GetExecutorRuntime();
            _context = context;

            var qName = QueueName.Get(context);
            var qFolder = QueueFolder.Get(context);

            Log($"[INIT] Starting execution of LazyFramework with Queue: {qName}, Folder: {qFolder}", TraceEventType.Information);
            Log($"[CONFIG] EnableQueue: {EnableQueue}, EnableInitializeConfig: {EnableInitializeConfig}, EnableInitializeApplications: {EnableInitializeApplications}", TraceEventType.Information);
            Log($"[CONFIG] EnableBusinessException: {EnableBusinessException}, EnableSystemException: {EnableSystemException}, EnableSuccessful: {EnableSuccessful}, EnableEnd: {EnableEnd}", TraceEventType.Information);
            Log($"[SYSTEM] The primary screen resolution is: {SystemParameters.PrimaryScreenWidth.ToString()} x {SystemParameters.PrimaryScreenHeight.ToString()}", TraceEventType.Information);

            if (EnableInitializeConfig)
            {
                Log("[WORKFLOW] Executing Framework Initialize Config", TraceEventType.Information);
                CallActivity(FrameworkInitializeConfig);
            }
            if (EnableInitializeApplications)
            {
                Log("[WORKFLOW] Executing Framework Close Applications (pre-initialization)", TraceEventType.Information);
                CallActivity(FrameworkCloseApplications);
            }
            if (EnableInitializeApplications)
            {
                Log("[WORKFLOW] Executing Framework Initialize Applications", TraceEventType.Information);
                CallActivity(FrameworkInitializeApplications);
            }
            if (EnableQueue)
            {
                Log("[QUEUE] Starting queue processing mode", TraceEventType.Information);
                while (true)
                {
                    Log("[QUEUE] Executing Framework Get Transaction", TraceEventType.Information);
                    CallActivity(FrameworkGetTransaction);
                    var queueItem = QueueItem.Get(context);
                    if (queueItem == null)
                    {
                        Log("[QUEUE] No queue item retrieved, exiting queue processing", TraceEventType.Information);
                        break;
                    }
                    try
                    {
                        Log("[TRANSACTION] Executing Framework Process Transaction", TraceEventType.Information);
                        CallActivity(FrameworkProcessTransaction);
                        if (EnableSuccessful)
                        {
                            Log("[SUCCESS] Executing Framework Successful", TraceEventType.Information);
                            CallActivity(FrameworkSuccessful);
                        }
                        Log("[TRANSACTION] Transaction processed successfully", TraceEventType.Information);

                    }
                    catch (BusinessRuleException ex)
                    {
                        Log($"[BUSINESS_EXCEPTION] Business exception occurred: {ex.Message}", TraceEventType.Warning);
                        if (EnableBusinessException)
                        {
                            Log("[EXCEPTION_HANDLER] Executing Framework Business Exception", TraceEventType.Information);
                            CallActivity(FrameworkBusinessException);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"[SYSTEM_EXCEPTION] System exception occurred: {ex.Message}", TraceEventType.Error);
                        if (EnableSystemException)
                        {
                            Log("[EXCEPTION_HANDLER] Executing Framework System Exception", TraceEventType.Information);
                            CallActivity(FrameworkSystemException);
                        }
                        Log("[RECOVERY] Reinitializing applications due to system exception", TraceEventType.Information);
                        CallActivity(FrameworkCloseApplications);
                        CallActivity(FrameworkInitializeApplications);
                    }
                    break;
                }
            }
            else
            {
                Log("[TRANSACTION] Starting single transaction processing mode", TraceEventType.Information);
                CallActivity(FrameworkProcessTransaction);
            }
                if (EnableInitializeApplications)
                {
                    Log("[CLEANUP] Executing Framework Close Applications (cleanup)", TraceEventType.Information);
                    CallActivity(FrameworkCloseApplications);
                }
                if(EnableEnd)
                {
                    Log("[FINALIZE] Executing Framework End", TraceEventType.Information);
                    CallActivity(FrameworkEnd);
                }
                
                Log("[COMPLETE] LazyFramework execution completed", TraceEventType.Information);
        }


        public void CallActivity(Activity activity, Action? callback = null)
        {
            if (activity == null)
            {
                Log("[ACTIVITY_WARNING] Attempted to call a null activity", TraceEventType.Warning);
                return;
            }
            
            Log($"[ACTIVITY_SCHEDULE] Scheduling activity: {activity.DisplayName ?? activity.GetType().Name}", TraceEventType.Verbose);
            _context?.ScheduleActivity(activity, OnActivityCompletedCallback, OnActivityFaultedCallback);
        }


        private void OnActivityFaultedCallback(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            Log($"[ACTIVITY_FAULT] Activity faulted: {propagatedFrom.Activity.DisplayName ?? propagatedFrom.Activity.GetType().Name} - {propagatedException.Message}", TraceEventType.Error);
            throw propagatedException;
        }

        private void OnActivityCompletedCallback(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Log($"[ACTIVITY_COMPLETE] Activity completed: {completedInstance.Activity.DisplayName ?? completedInstance.Activity.GetType().Name}", TraceEventType.Verbose);
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
