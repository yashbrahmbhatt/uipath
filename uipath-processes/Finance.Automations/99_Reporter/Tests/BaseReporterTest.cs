using System;
using UiPath.CodedWorkflows;

namespace Finance.Automations._99_Reporter.Tests
{
    public class BaseReporterTest : CodedWorkflow
    {
        public  string Id {get; set;}
        /// <summary>
        /// From date for Execute method testing.
        /// </summary>
        public  DateTime FromDate { get; set; }

        /// <summary>
        /// To date for Execute method testing.
        /// </summary>
        public  DateTime ToDate { get; set; }

        /// <summary>
        /// CRON schedule for Execute method testing.
        /// </summary>
        public  string Cron { get; set; }

        /// <summary>
        /// Configuration path for Execute method testing.
        /// </summary>
        public  string ConfigPath { get; set; }
        public  string ExpectedExceptionMessage { get; set; }


        [Workflow]
        public void Execute()
        {
            Log("🚀 Starting Reporter module test execution");

            Exception ex = null;
            try
            {
                workflows.Reporter(FromDate,ToDate,Cron,ConfigPath, "");
            }
            catch (Exception e)
            {
                ex = e;
            }

            testing.VerifyExpression(ex == null && string.IsNullOrWhiteSpace(ExpectedExceptionMessage), $"Expected failure status ({!string.IsNullOrWhiteSpace(ExpectedExceptionMessage)}) did not match output ({ex == null})");
            if (ex != null) testing.VerifyExpression(ex.Message.Contains(ExpectedExceptionMessage));
            Log("🎉 Reporter test completed successfully!");
        }



    }
}