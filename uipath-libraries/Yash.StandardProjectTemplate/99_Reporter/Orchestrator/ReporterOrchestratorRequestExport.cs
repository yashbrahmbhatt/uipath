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
    /// A workflow class that requests an export for a specified queue from the Orchestrator API.
    /// </summary>
    public class ReporterOrchestratorRequestExport : CodedWorkflow
    {
        /// <summary>
        /// Requests an export for a specified queue definition ID from the Orchestrator API.
        /// </summary>
        /// <param name="QueueDefinitionId">The ID of the queue definition to request the export for. Default is "30428".</param>
        /// <returns>The report ID returned from the Orchestrator API for the export request.</returns>
        /// <exception cref="Exception">Thrown if the export request fails or the API response is unsuccessful.</exception>
        [Workflow]
        public string Execute(string QueueDefinitionId = "30428")
        {
            // Log the start of the export request
            Log($"Requesting export for queue id {QueueDefinitionId}");

            // Make the HTTP request to request the export from the Orchestrator API
            var res_Code = system.OrchestratorHTTPRequest(
                OrchestratorAPIHttpMethods.POST,
                $"/odata/QueueDefinitions({QueueDefinitionId})/UiPathODataSvc.Export",
                "{\"params\":{},\"headers\":{\"X-UIPATH-OrganizationUnitId-SKIP\":\"0\"}}",
                out var res_Headers,
                out string res_Result,
                default);

            // Log the response from the Orchestrator API
            Log($"Response code: {res_Code} Content: {res_Result}");

            // If the response code is not successful, throw an exception
            if (!(res_Code >= 200 && res_Code < 300))
                throw new Exception($"Got {res_Code} response. {res_Result}");

            // Parse the response JSON to retrieve the report ID
            var json_Object = JsonConvert.DeserializeObject<JObject>(res_Result);
            var str_ReportId = json_Object["Id"].ToString();

            // Log the report ID
            Log($"Report id: {str_ReportId}");

            // Return the report ID
            return str_ReportId;
        }
    }

}