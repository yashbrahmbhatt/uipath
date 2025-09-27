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
    /// Failure test case for Reporter workflow.
    /// Inherits from BaseReporterTest which uses the IReporterTestable interface.
    /// Tests that the reporter properly handles and reports failures.
    /// </summary>
    public class Reporter_Failure : BaseReporterTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "systemexception";
        
        /// <summary>
        /// Expected exception message - specific failure message expected.
        /// </summary>
        public override string ExpectedExceptionMessage { get; set; } = "Test system exception triggered by TestId: systemexception";

        /// <summary>
        /// Initialize test-specific properties.
        /// </summary>
        public Reporter_Failure() 
        {
            FromDate = DateTime.Now.AddDays(-7);
            ToDate = DateTime.Now.AddDays(0);
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
            Log("🔧 Initializing Reporter failure test");
            
            // Set up email monitoring for framework exception emails
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Reporter] Critical Framework Exception");
            
            // Clean up any existing emails
            var emails = workflows.GetEmails("eyashb", filter);
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            
            Log("✅ Reporter failure test initialized");
        }

        /// <summary>
        /// Override ValidateTest to check for framework exception email.
        /// </summary>
        public override void ValidateTest()
        {
            Log("🧪 Validating Reporter failure test results");
            
            // Wait for email processing
            System.Threading.Thread.Sleep(5000);
            
            // Validate that framework exception email was sent
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Reporter] Critical Framework Exception");
            
            var emails = workflows.GetEmails("eyashb", filter);
            testing.VerifyExpression(emails.Count > 0, "Framework exception email should be sent", true, "Email Validation", false, false);
            
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).MarkEmailAsRead(email);
                }
            }
            
            Log("✅ Reporter failure test validation completed");
        }

        /// <summary>
        /// Override CleanupTest to perform any necessary cleanup.
        /// </summary>
        public override void CleanupTest()
        {
            Log("🧹 Cleaning up Reporter failure test");
            // Cleanup is handled in ValidateTest by marking emails as read
            Log("✅ Reporter failure test cleanup completed");
        }
    }
}