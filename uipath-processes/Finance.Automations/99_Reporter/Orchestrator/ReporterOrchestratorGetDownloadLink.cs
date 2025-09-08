using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace Finance.Automations._99_Reporter.Orchestrator
{
    /// <summary>
    /// A workflow class that retrieves the download link for a specified export using the Orchestrator API.
    /// </summary>
    public class ReporterOrchestratorGetDownloadLink : CodedWorkflow
    {
        /// <summary>
        /// Retrieves the download link for a specified export by its ExportId from the Orchestrator API.
        /// </summary>
        /// <param name="ExportId">The ID of the export to retrieve the download link for. Default is "21565".</param>
        /// <returns>The download URI as a string.</returns>
        /// <exception cref="Exception">Thrown if the API request fails or the response status is not successful.</exception>
        [Workflow]
        public string Execute(string ExportId = "21565")
        {
            // Log the start of the download link retrieval
            Log($"Retrieving download link for export {ExportId}");

            // Make the HTTP request to get the download link for the export from the Orchestrator API
            var res_Code = system.OrchestratorHTTPRequest(
                OrchestratorAPIHttpMethods.GET,
                $"/odata/Exports({ExportId})/UiPath.Server.Configuration.OData.GetDownloadLink",
                out var res_Headers,
                out string res_Result);

            // Log the response from the Orchestrator API
            Log($"Response code: {res_Code} Content: {res_Result}");

            // If the response code is not successful, throw an exception
            if (!(res_Code >= 200 && res_Code < 300))
                throw new Exception($"Got {res_Code} response. {res_Result}");

            // Parse the response JSON to retrieve the download URI
            var json_Object = JsonConvert.DeserializeObject<JObject>(res_Result);
            var str_DownloadUri = json_Object["Uri"].ToString();

            // Log the download URI
            Log($"Download Uri: {str_DownloadUri}");

            // Return the download URI
            return str_DownloadUri;
        }
    }
}