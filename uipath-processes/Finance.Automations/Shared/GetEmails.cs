using System.Collections.Generic;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;
using System.Linq;

namespace Finance.Automations._00_Shared
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