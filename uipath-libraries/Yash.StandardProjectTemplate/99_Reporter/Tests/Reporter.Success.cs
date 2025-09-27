using System;
using System.Collections.Generic;
using System.Data;
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
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._99_Reporter.Tests
{
    /// <summary>
    /// Success test case for Reporter workflow.
    /// Inherits from BaseReporterTest which uses the IReporterTestable interface.
    /// </summary>
    public class Reporter_Success : BaseReporterTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "Success";
        
        /// <summary>
        /// Expected exception message - empty string means success is expected.
        /// </summary>
        public override string ExpectedExceptionMessage { get; set; } = "";

        /// <summary>
        /// Initialize test-specific properties.
        /// </summary>
        public Reporter_Success()
        {
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now;
            Cron = "";
        }

        [TestCase]
        public void ExecuteTestCase()
        {
            // Use the TestableExtensions framework to execute the test
            this.ExecuteTest();
        }

        /// <summary>
        /// Override InitializeTest to set up email monitoring.
        /// </summary>
        public override void InitializeTest()
        {
            Log("🔧 Initializing Reporter success test");
            
            // Set up email monitoring
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "Process Report").ByUnread(true);
            
            // Clean up any existing emails
            var emails = workflows.GetEmails("INBOX", filter);
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).MarkEmailAsRead(email);
                }
            }
            
            Log("✅ Reporter success test initialized");
        }

        /// <summary>
        /// Override ValidateTest to check for email sending.
        /// </summary>
        public override void ValidateTest()
        {
            Log("🧪 Validating Reporter success test results");
            
            // Validate that report email was sent
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "Process Report").ByUnread(true);
            
            var emails = workflows.GetEmails("INBOX", filter);
            testing.VerifyExpression(emails.Count > 0, "Report email should be sent", true, "Email Validation", false, false);
            
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).MarkEmailAsRead(email);
                }
            }
            
            Log("✅ Reporter success test validation completed");
        }

        /// <summary>
        /// Override CleanupTest to perform any necessary cleanup.
        /// </summary>
        public override void CleanupTest()
        {
            Log("🧹 Cleaning up Reporter success test");
            // Cleanup is handled in ValidateTest by marking emails as read
            Log("✅ Reporter success test cleanup completed");
        }
    }
}