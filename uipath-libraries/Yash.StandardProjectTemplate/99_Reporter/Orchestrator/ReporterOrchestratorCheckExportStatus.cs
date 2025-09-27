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

namespace Yash.StandardProject._99_Reporter.Orchestrator
{
    /// <summary>
    /// A workflow class that checks the status of an export using the Orchestrator API.
    /// </summary>
    public class ReporterOrchestratorCheckExportStatus : CodedWorkflow
    {
        /// <summary>
        /// Checks the status of a specified export by its ExportId in the Orchestrator API.
        /// </summary>
        /// <param name="ExportId">The ID of the export to check. Default is "21565".</param>
        /// <returns>A boolean indicating whether the export status is "Completed".</returns>
        /// <exception cref="Exception">Thrown if the API request fails or the response status is not successful.</exception>
        [Workflow]
        public bool Execute(string ExportId = "21565")
        {
            // Log the start of the export status check
            Log($"Checking export {ExportId}'s status");

            // Make the HTTP request to check the export status from the Orchestrator API
            var res_Code = system.OrchestratorHTTPRequest(
                OrchestratorAPIHttpMethods.GET,
                $"/odata/Exports({ExportId})",
                out var res_Headers,
                out string res_Result);

            // Log the response from the Orchestrator API
            Log($"Response code: {res_Code} Content: {res_Result}");

            // If the response code is not successful, throw an exception
            if (!(res_Code >= 200 && res_Code < 300))
                throw new Exception($"Got {res_Code} response. {res_Result}");

            // Parse the response JSON to retrieve the export status
            var json_Object = JsonConvert.DeserializeObject<JObject>(res_Result);
            var str_ExportStatus = json_Object["Status"].ToString();

            // Log the export status
            Log($"Report status: {str_ExportStatus}");

            // Return true if the export status is "Completed", otherwise return false
            return str_ExportStatus == "Completed";
        }
    }

}