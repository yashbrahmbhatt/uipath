using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
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

namespace Yash.StandardProject._99_Reporter.Orchestrator
{
    /// <summary>
    /// A workflow class that extracts the contents of a ZIP file to a specified destination folder.
    /// </summary>
    public class ReporterOrchestratorUnzipFolder : CodedWorkflow
    {
        /// <summary>
        /// Extracts the contents of a ZIP file to the specified destination folder.
        /// </summary>
        /// <param name="ZipFilePath">The path to the ZIP file to be extracted. Default is "test.zip".</param>
        /// <param name="DestinationFolder">The folder where the contents of the ZIP file will be extracted. Default is "unzipped".</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if the destination folder cannot be created or found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the ZIP file cannot be extracted to the destination folder.</exception>
        [Workflow]
        public void Execute(string ZipFilePath = "test.zip", string DestinationFolder = "unzipped")
        {
            // Ensure the destination folder exists
            if (!Directory.Exists(DestinationFolder))
            {
                Directory.CreateDirectory(DestinationFolder);
            }

            // Extract the contents of the ZIP file to the specified directory
            ZipFile.ExtractToDirectory(ZipFilePath, DestinationFolder);
        }
    }

}