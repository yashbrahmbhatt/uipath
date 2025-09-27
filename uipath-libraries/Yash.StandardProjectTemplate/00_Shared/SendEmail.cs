using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Mail.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Platform.ResourceHandling;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Yash.StandardProject._00_Shared
{
    public class SendEmail : CodedWorkflow
    {
        [Workflow]
        public void Execute(
            string to,
            string body,
            string subject,
            IEnumerable<string> attachments,
            string cc = null
            )
        {
            
           
            
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var connection = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var tos = Regex.Split(to, "[,;]").ToList();
            var req = new SendEmailRequest();
            req.IsBodyHtml = true;
            req.To = tos;
            req.Body = body;
            req.Attachments = attachments != null ?  attachments.Select(a => (IResource)LocalResource.FromPath(a)).ToList() : new();
            req.Subject = subject;
            
            google.Gmail(connection).SendEmail(req);
        }
        
        
    }
}