using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;
using UiPath.Platform.ResourceHandling;

namespace Finance.Automations._00_Shared
{
    public class SendEmail : CodedWorkflow
    {
        [Workflow]
        public void Execute(
            string to,
            string body,
            IEnumerable<string> attachments,
            Dictionary<string, object> data,
            string cc = null
            )
        {
            Log($"Sending email using {data.Keys.Count.ToString()} keys in the template data dictionary");
            
            // Replace template variables in body
            foreach(var key in data.Keys){
                if(data[key] is DataTable){
                    body = body.Replace("{" + key + "}", workflows.DataTableToHTML((DataTable)data[key], new()));
                    body = body.Replace("{" + key + "_Name}", ((DataTable)data[key]).TableName);
                }
                else {
                    body = body.Replace("{" + key + "}", data[key]?.ToString());
                }
            }
            
            // Extract subject from HTML title tag
            string subject = ExtractSubjectFromHtml(body);
            
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
        
        private string ExtractSubjectFromHtml(string htmlBody)
        {
            try
            {
                // Use regex to extract the content of the <title> tag
                var titleMatch = Regex.Match(htmlBody, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                if (titleMatch.Success)
                {
                    // Get the title content and decode any HTML entities
                    string subject = titleMatch.Groups[1].Value.Trim();
                    
                    // Decode common HTML entities
                    subject = subject.Replace("&lt;", "<")
                                   .Replace("&gt;", ">")
                                   .Replace("&amp;", "&")
                                   .Replace("&quot;", "\"")
                                   .Replace("&#39;", "'");
                    
                    Log($"Extracted email subject from HTML title: {subject}");
                    return subject;
                }
                else
                {
                    // Fallback subject if no title tag found
                    string fallbackSubject = "Automated Process Notification";
                    Log($"No title tag found in HTML body. Using fallback subject: {fallbackSubject}");
                    return fallbackSubject;
                }
            }
            catch (Exception ex)
            {
                Log($"Error extracting subject from HTML: {ex.Message}. Using fallback subject.");
                return "Automated Process Notification";
            }
        }
    }
}