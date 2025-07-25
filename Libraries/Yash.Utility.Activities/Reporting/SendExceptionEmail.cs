using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using Yash.Utility.Activities.Reporting;

namespace Yash.Utility.Activities.Reporting
{
    public class SendExceptionEmail : CodedWorkflow
    {
        [Workflow]
        public void Execute(
            Exception in_ex_ExceptionToReport,
            string in_str_ScreenshotFolder,
            string in_str_ProcessName = null,
            string in_str_TemplatePath = null,
            string in_str_To = null)
        {
            in_str_ProcessName = in_str_ProcessName ?? "Default Process Name";
            in_str_TemplatePath = in_str_TemplatePath ?? "Reporting\\Templates\\ExceptionEmail.html";
            in_str_To = in_str_To ?? "eyashb@gmail.com";
            Log($"=== Inputs ===");
            Log(JsonConvert.SerializeObject(new
            {
                in_str_ProcessName,
                in_str_ScreenshotFolder,
                in_str_TemplatePath,
                in_str_To,
                in_ex_ExceptionToReport
            }, GlobalStaticHelpers.JsonSafeSettings));
            var template = string.IsNullOrEmpty(in_str_TemplatePath) ? ResolveLibraryRelativePath(in_str_TemplatePath) : File.ReadAllText(in_str_TemplatePath);
            Log($"Template read from {in_str_TemplatePath}");
            var dict_diagnostic = workflows.GenerateBaseDictionary(in_str_ProcessName, in_ex_ExceptionToReport);
            var (body, subject) = workflows.PrepareHTMLTemplate(template, dict_diagnostic, default);
            Log($"Template prepared");
            var screenshot = workflows.TakeScreenshot(in_str_ScreenshotFolder, null, in_str_ProcessName);
            workflows.SendEmail(new List<string>() { screenshot }, body, subject, in_str_To);
            Log($"Email Sent");
            File.Delete(screenshot);
        }

        public static string ResolveLibraryRelativePath(string relativePath)
        {
            var assembly = typeof(SendExceptionEmail).Assembly;
            var assemblyPath = assembly.Location;
            var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
            return Path.Combine(assemblyDir, relativePath);
        }
    }
}