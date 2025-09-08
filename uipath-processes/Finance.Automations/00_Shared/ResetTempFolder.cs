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
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Finance.Automations._00_Shared
{
    public class ResetTempFolder : CodedWorkflow
    {
        [Workflow]
        public void Execute(string folderPath)
        {
            if(Directory.Exists(folderPath)) {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
        }
    }
}