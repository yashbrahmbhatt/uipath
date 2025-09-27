using System;
using Yash.StandardProject.Configs;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;
using UiPath.Testing;
using Yash.Config.Models;
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._02_Performer.Tests
{
    /// <summary>
    /// Test case for Performer system exception scenario.
    /// Tests system exception handling and recovery logic.
    /// System exceptions should be handled and not cause workflow failure.
    /// </summary>
    public class PerformerSystemExceptionTest : BasePerformerTest
    {
        public override string TestId { get; set; } = "systemexception";
        public override string ExpectedExceptionMessage { get; set; } = ""; // System exceptions are handled, not thrown

        [TestCase]
        public void ExecuteTestCase()
        {
            // Use the TestableExtensions framework to execute the test
            this.ExecuteTest();
        }

        /// <summary>
        /// Override InitializeTest to set up queue items and email monitoring.
        /// </summary>
        public override void InitializeTest()
        {
            Log("🔧 Initializing Performer system exception test");
            
            // Set up email monitoring
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Performer] System Exception - Transaction");
            
            // Clean up any existing emails
            var emails = workflows.GetEmails("eyashb", filter);
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            
            // Add test queue item
            system.AddQueueItem(SharedConfig.QueueName, SharedConfig.QueueFolder, default, default, default, default, TestId, default);
            
            Log("✅ Performer system exception test initialized");
        }

        /// <summary>
        /// Override ValidateTest to check for system exception email.
        /// </summary>
        public override void ValidateTest()
        {
            Log("🧪 Validating Performer system exception test results");
            
            // Wait for email processing
            System.Threading.Thread.Sleep(5000);
            
            // Validate that system exception email was sent
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Performer] System Exception - Transaction");
            
            var emails = workflows.GetEmails("eyashb", filter);
            testing.VerifyExpression(emails.Count > 0, "System exception email should be sent", true, "Email Validation", false, false);
            
            Log("✅ Performer system exception test validation completed");
        }

        /// <summary>
        /// Override CleanupTest to clean up emails.
        /// </summary>
        public override void CleanupTest()
        {
            Log("🧹 Cleaning up Performer system exception test");
            
            // Clean up emails
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Performer] System Exception - Transaction");
            
            var emails = workflows.GetEmails("eyashb", filter);
            if(emails.Count > 0){
                foreach(var email in emails){
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            
            Log("✅ Performer system exception test cleanup completed");
        }
    }
}

