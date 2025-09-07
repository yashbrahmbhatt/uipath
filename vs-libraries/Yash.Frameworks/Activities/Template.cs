using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yash.Frameworks.Activities.ViewModels.Helpers;
using Activity = System.Activities.Activity;

namespace Yash.Frameworks.Activities
{
    public class ActivityTemplate : CodeActivity<int> // This base class exposes an OutArgument named Result
    {




        public Activity FrameworkInitializeSettings { get; set; }
        public Activity FrameworkInitializeApplications { get; set; }
        public bool EnableInitializeSettings { get; set; }
        public bool EnableInitializeApplications { get; set; }


        /*
         * The returned value will be used to set the value of the Result argument
         */
        protected override int Execute(CodeActivityContext context)
        {
            // This is how you can log messages from your activity. logs are sent to the Robot which will forward them to Orchestrator
            context.GetExecutorRuntime().LogMessage(new UiPath.Robot.Activities.Api.LogMessage()
            {
                EventType = TraceEventType.Information,
                Message = "Executing Calculator activity"
            });



            return ExecuteInternal();
        }

        public int ExecuteInternal()
        {
            return 0;
        }
    }
    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}
