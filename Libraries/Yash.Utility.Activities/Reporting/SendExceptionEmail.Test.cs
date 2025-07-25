using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Yash.Utility.Activities.Misc;

namespace Yash.Utility.Activities.Reporting
{
    public class SendExceptionEmail_Test : CodedWorkflow
    {
        [TestCase]
        public void Execute()
        {
            var template = File.ReadAllText("Reporting\\Templates\\ExceptionEmail.html");
            Exception sysEx= null;
            try
            {
                File.ReadAllText("1");
            }
            catch (Exception ex1)
            {
                sysEx = ex1;
            }
            sysEx.Data.Add("ExceptionDataField", "ExceptionDataFieldValue");
            var exceptions = new List<Exception>() { sysEx,new BusinessRuleException("SendExceptionEmailTest") };
            foreach (var ex in exceptions)
            {
                var dict_diagnostic = workflows.GenerateBaseDictionary("CreateSamples", ex);
                var (body, subject) = workflows.PrepareHTMLTemplate(template, dict_diagnostic, default);
                workflows.SendExceptionEmail(ex, Directory.GetCurrentDirectory(), "SendExceptionEmailTest", default, default);
            }
        }
    }
}