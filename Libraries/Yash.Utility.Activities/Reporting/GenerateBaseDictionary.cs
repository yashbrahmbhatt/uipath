using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using Yash.Utility.Activities;
using Yash.Utility.Activities.Misc;
namespace Yash.Utility.Activities.Reporting
{
    public class GenerateBaseDictionary : CodedWorkflow
    {
        [Workflow]
        public Dictionary<string, object> Execute(string in_str_ProcessName, Exception in_ex_ExceptionToReport = null)
        {
            Dictionary<string, object> dict = new();
            var now = DateTime.Now;
            var currentJob = GetRunningJobInformation();
            dict["Logs_URL"] = $"https://cloud.uipath.com/{currentJob.OrganizationId}/{currentJob.TenantName}/orchestrator_/jobs(sidepanel:sidepanel/jobs/{currentJob.JobId}/logs)?tid={currentJob.TenantId}&fid={currentJob.FolderId}";
            dict["ProcessName"] = in_str_ProcessName;
            dict["Env_MachineName"] = Environment.MachineName;
            dict["Env_User"] = Environment.UserDomainName + "\\" + Environment.UserName;
            dict["Env_Date"] = now.ToString("MMM dd, yyyy");
            dict["Env_Time"] = now.ToString("HH:mm:ss");
            if (in_ex_ExceptionToReport != null)
            {

                dict["Ex_Message"] = in_ex_ExceptionToReport.Message;
                if (in_ex_ExceptionToReport.Source != null)
                    dict["Ex_Source"] = in_ex_ExceptionToReport.Source;
                if (in_ex_ExceptionToReport.StackTrace != null)
                    dict["Ex_Stack"] = in_ex_ExceptionToReport.StackTrace.Length < 300 ?
                                        in_ex_ExceptionToReport.StackTrace :
                                        in_ex_ExceptionToReport.StackTrace.Substring(0, 300) + "...";
                dict["Ex_Data"] = JsonConvert.SerializeObject(in_ex_ExceptionToReport.Data, GlobalStaticHelpers.JsonSafeSettings);
                dict["Theme"] = in_ex_ExceptionToReport is BusinessRuleException ? "warning" : "error";
            }
            dict["Env_TenantName"] = currentJob.TenantName;
            dict["Env_OrchestratorFolderPath"] = currentJob.FolderName;
            dict["debug_JobInfo"] = JsonConvert.SerializeObject(currentJob, GlobalStaticHelpers.JsonSafeSettings);
            return dict;
        }
    }
}