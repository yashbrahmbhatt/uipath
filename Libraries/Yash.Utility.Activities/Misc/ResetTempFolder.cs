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
    /// <summary>
    /// A coded workflow activity that resets a temporary folder by deleting its contents and recreating it.
    /// </summary>
    public class ResetTempFolder : CodedWorkflow
    {
        /// <summary>
        /// Deletes the specified folder and all its contents if it exists, then recreates the folder.
        /// </summary>
        /// <param name="in_str_FolderPath">The full path of the folder to reset.</param>
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