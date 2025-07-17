using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Utility.Activities.Misc
{
    public class ResetTempFolder : CodedWorkflow
    {
        [Workflow]
        public void Execute(string in_str_FolderPath)
        {
            if (Directory.Exists(in_str_FolderPath))
            {
                Directory.Delete(in_str_FolderPath, true);
            }
            Directory.CreateDirectory(in_str_FolderPath);
        }
    }
}