using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Finance.Automations._00_Shared
{
    public class GenerateDiagnosticDictionary : CodedWorkflow
    {
        [Workflow]
        public Dictionary<string, object> Execute(Exception ex = null, QueueItem transaction = null)
        {
            Dictionary<string, object> dict = new();
            var now = DateTime.Now;
            var jobInfo = GetRunningJobInformation();
            dict["Env_MachineName"] = Environment.MachineName;
            dict["Env_User"] = Environment.UserName;
            dict["Env_Date"] = now.ToLongDateString();
            dict["Env_Time"] = now.ToLongTimeString();
            dict["Today"] = now.ToString("dd-MM-yyyy");

            if (ex != null)
            {
                dict["Ex_Message"] = ex.Message;
                dict["Ex_Source"] = ex.Source;
                dict["Ex_Stack"] = ex.StackTrace;
                dict["Ex_Type"] = 
                ex is BusinessRuleException ? 
                    nameof(BusinessRuleException) : 
                    ex is Exception ? 
                        "System" : 
                        "Unknow";
                dict["Ex_Data"] = JsonConvert.SerializeObject(ex.Data);
            }

            if (transaction != null)
            {
                dict["Transaction_Reference"] = transaction.Reference;
                dict["Transaction_SpecificData"] = JsonConvert.SerializeObject(transaction.SpecificContent);
            }
            return dict;
        }
    }
}