using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
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

namespace Finance.Automations._99_Reporter.Orchestrator
{
    /// <summary>
    /// A workflow class that downloads a file from a specified URL and saves it to a local file.
    /// </summary>
    public class ReporterOrchestratorDownloadFile : CodedWorkflow
    {
        /// <summary>
        /// Downloads a file from the specified download URI and saves it to the specified file path.
        /// </summary>
        /// <param name="DownloadUri">The URI from which to download the file. Default is a sample URL.</param>
        /// <param name="FilePath">The local file path where the downloaded file will be saved. Default is "test.zip".</param>
        /// <exception cref="HttpRequestException">Thrown if the HTTP request fails or the response code indicates an error.</exception>
        [Workflow]
        public void Execute(string DownloadUri = "https://crpapaidorch0cacestg.blob.core.windows.net/orchestrator-7eecb541-ae1e-401c-8503-63abd1af3899/SystemBuckets/user-fd4e4a89-a02e-4a0f-8c18-fcda563cbd6f/exports/001_DocumentUnderstanding-items-2025-02-05-04-29-16-000-21565.zip?sv=2024-11-04&st=2025-02-05T04%3A38%3A10Z&se=2025-02-05T05%3A38%3A40Z&sr=b&sp=r&sig=lsxafiV2DuUuPXPcT57mJxnWZhL0fKhHgmpK5P6FHBE%3D", string FilePath = "test.zip")
        {
            // Create an instance of HttpClient for downloading the file
            HttpClient http_Client = new();

            // Log the download attempt
            Log($"Downloading file from {DownloadUri}");

            // Send a GET request to the specified URL to download the file
            HttpResponseMessage response = http_Client.GetAsync(DownloadUri).Result;

            // Ensure the HTTP request was successful
            response.EnsureSuccessStatusCode();

            // Read the content as a stream
            using (var contentStream = response.Content.ReadAsStreamAsync().Result)
            {
                // Open a FileStream to save the content to the specified file path
                using (var fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                {
                    // Copy the content stream to the file stream
                    contentStream.CopyTo(fileStream);

                    // Log the successful download and save
                    Log($"Export downloaded and saved to {FilePath}");
                }
            }
        }
    }

}