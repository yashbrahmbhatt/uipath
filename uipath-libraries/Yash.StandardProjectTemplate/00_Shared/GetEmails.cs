using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.GSuite.Activities.Api;
using UiPath.Mail.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using System.Linq;
using Newtonsoft.Json;
namespace Yash.StandardProject._00_Shared
{
    public class GetEmails : CodedWorkflow
    {
        [Workflow]
        public IReadOnlyCollection<IMail> Execute(string folderName, MailFilter filter)
        {
            var connection = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connection.ConnectionId, connection.Resolver);
            var folders = google.Gmail(service).GetMailLabels();
           // Log($"{JsonConvert.SerializeObject(folders.Select(f=>f.Name).ToList())}");
            var folder = folders.First(f => f.Name == folderName);
            return google.Gmail(service).GetEmails(folder, filter);
        }
    }
}