using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Utility.Activities.Reporting
{
    public class GenerateDiagnosticDictionary : CodedWorkflow
    {
        [Workflow]
        public Dictionary<string, object> Execute(string in_str_ProcessName, Exception ex)
        {
            Dictionary<string, object> dict = new();
            var now = DateTime.Now;
            var currentJob = GetRunningJobInformation();
            dict["Logs_URL"] = $"https://cloud.uipath.com/{currentJob.OrganizationId}/{currentJob.TenantName}/orchestrator_/jobs(sidepanel:sidepanel/jobs/{currentJob.JobId}/logs)?tid={currentJob.TenantId}&fid={currentJob.FolderId}";
            dict["ProcessName"] = in_str_ProcessName;
            dict["Env_MachineName"] = Environment.MachineName;
            dict["Env_User"] = Environment.UserName;
            dict["Env_Date"] = now.ToString("MMM dd, yyyy");
            dict["Env_Time"] = now.ToString("HH:mm:ss");
            dict["Ex_Message"] = ex.Message;
            dict["Ex_Source"] = ex.Source;
            dict["Ex_Stack"] = ex.StackTrace;
            dict["Ex_Data"] = JsonConvert.SerializeObject(ex.Data);
            return dict;
        }
    }
}