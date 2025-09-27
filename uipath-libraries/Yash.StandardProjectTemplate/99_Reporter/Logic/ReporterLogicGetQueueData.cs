using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
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
namespace Yash.StandardProject._99_Reporter.Logic
{
    /// <summary>
    /// A workflow class that retrieves and processes data from a specified queue in a given folder.
    /// The class requests an export, waits for the export to be ready, downloads the export file,
    /// and unzips it into a temporary directory.
    /// </summary>
    public class ReporterLogicGetQueueData : CodedWorkflow
    {
        /// <summary>
        /// Retrieves data from a queue in a specified folder by requesting an export, waiting for it to be ready,
        /// and downloading and unzipping the export file into a temporary directory.
        /// </summary>
        /// <param name="QueueName">The name of the queue from which data will be retrieved. Default is "001_DocumentUnderstanding".</param>
        /// <param name="QueueFolder">The folder where the queue is located. Default is "Property Insurance Cancellation".</param>
        /// <param name="TempFolder">The temporary folder where files will be stored during processing. Default is "Data\\Temp".</param>
        /// <returns>The path to the first file in the unzipped export.</returns>
        [Workflow]
        public string Execute(string QueueName = "001_DocumentUnderstanding", string QueueFolder = "Property Insurance Cancellation", string TempFolder = "Data\\Temp")
        {
            // Log the request for queue data
            Log($"Getting queue data for queue {QueueName} in folder {QueueFolder}");

            // Get the queue definition ID
            var int_QueueDefinitionId = workflows.ReporterOrchestratorGetQueueDefinitionId(QueueName, QueueFolder);

            // Request the export for the queue data
            var str_ExportId = workflows.ReporterOrchestratorRequestExport(int_QueueDefinitionId.ToString());
            Log($"Export {str_ExportId} requested");

            // Check the export status and wait until it is ready
            var status = workflows.ReporterOrchestratorCheckExportStatus(str_ExportId);
            while (!status)
            {
                Log($"Status: {status}, waiting 5s.");
                Thread.Sleep(5000);
                status = workflows.ReporterOrchestratorCheckExportStatus(str_ExportId);
            }

            // Log that the export is ready for download
            Log($"Export ready for download");

            // Get the download link for the export file
            var str_DownloadUri = workflows.ReporterOrchestratorGetDownloadLink(str_ExportId);

            // Define the temporary path where the file will be downloaded
            var str_TempPath = Path.Combine(TempFolder, Path.GetFileNameWithoutExtension(Path.GetTempFileName()));

            // Download and unzip the file
            workflows.ReporterOrchestratorDownloadFile(str_DownloadUri, str_TempPath + ".zip");
            workflows.ReporterOrchestratorUnzipFolder(str_TempPath + ".zip", str_TempPath);
            Log($"Export downloaded");

            // Get the files from the unzipped folder
            var files = Directory.GetFiles(str_TempPath);

            // Return the path of the first file in the unzipped folder
            return files[0];
        }
    }

}