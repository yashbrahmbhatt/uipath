using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
using UiPath.Core.Activities.Orchestrator;
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
    /// A workflow class that retrieves the definition ID of a queue from the Orchestrator API.
    /// </summary>
    public class ReporterOrchestratorGetQueueDefinitionId : CodedWorkflow
    {
        /// <summary>
        /// Retrieves the queue definition ID for a specified queue from the Orchestrator API.
        /// </summary>
        /// <param name="QueueName">The name of the queue to retrieve the definition ID for. Default is "001_DocumentUnderstanding".</param>
        /// <param name="QueueFolder">The folder where the queue is located. Default is "Property Insurance Cancellation".</param>
        /// <returns>The definition ID of the specified queue.</returns>
        /// <exception cref="Exception">Thrown if the queue definition cannot be found or the API response is unsuccessful.</exception>
        [Workflow]
        public int Execute(string QueueName = "001_DocumentUnderstanding", string QueueFolder = "Property Insurance Cancellation")
        {
            // Log the start of the process
            Log($"Looking for queue {QueueName} in folder {QueueFolder}");

            // Make the HTTP request to retrieve queue definitions
            var res_Code = system.OrchestratorHTTPRequest(OrchestratorAPIHttpMethods.GET, "/odata/QueueDefinitions", out var res_Headers, out string res_Result);

            // Log the response from the Orchestrator API
            Log($"Response code: {res_Code} Content: {res_Result}");

            // If the response code is not successful, throw an exception
            if (!(res_Code >= 200 && res_Code < 300))
                throw new Exception($"Got {res_Code} response. {res_Result}");

            // Parse the response JSON to find the queue definition ID
            var json_Object = JsonConvert.DeserializeObject<JObject>(res_Result);
            var list_Ids = json_Object["value"]
                .Where(q => q["Name"].ToString() == QueueName && q["OrganizationUnitFullyQualifiedName"].ToString() == QueueFolder)
                .Select(q => q["Id"]);

            // If no matching queue is found, throw an exception
            if (list_Ids.Count() == 0)
                throw new Exception("Couldn't find matching queue");

            // Log the found queue definition ID
            Log($"Found matching queue with id {list_Ids.First().ToString()}");

            // Return the queue definition ID
            return Convert.ToInt32(list_Ids.First().ToString());
        }
    }

}