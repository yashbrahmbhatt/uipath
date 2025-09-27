using System;
using System.Linq;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;
using UiPath.Testing;
using Yash.Config.Models;
using Yash.StandardProject._00_Shared;
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._02_Performer.Tests
{
    /// <summary>
    /// Test case for Performer business rule exception scenario.
    /// Tests business rule exception handling during transaction processing.
    /// Business exceptions should not cause the workflow to fail but should be handled gracefully.
    /// Inherits from BasePerformerTest which uses the IPerformerTestable interface.
    /// </summary>
    public class PerformerBusinessExceptionTest : BasePerformerTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "businessexception";
        
        /// <summary>
        /// Expected exception message - empty because business exceptions are handled, not thrown.
        /// </summary>
        public override string ExpectedExceptionMessage { get; set; } = "";

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
            Log("🔧 Initializing Performer business exception test");
            
            // Set up email monitoring
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            MailFilter filter = new MailFilter();
            
            // Clean up any existing emails
            var emails = workflows.GetEmails("eyashb", filter);
            workflows.DeleteEmails(emails.ToList());
            
            // Add test queue item
            system.AddQueueItem(SharedConfig.QueueName, SharedConfig.QueueFolder, default, default, default, default, TestId, default);
            
            Log("✅ Performer business exception test initialized");
        }

        /// <summary>
        /// Override ValidateTest to check for business exception email.
        /// </summary>
        public override void ValidateTest()
        {
            Log("🧪 Validating Performer business exception test results");
            
            // Wait for email processing
            System.Threading.Thread.Sleep(5000);
            
            // Validate that business exception email was sent
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            MailFilter filter = new MailFilter();
            
            var emails = workflows.GetEmails("eyashb", filter);
            testing.VerifyExpression(emails.Count > 0, "Business exception email should be sent", true, "Email Validation", false, false);
            
            Log("✅ Performer business exception test validation completed");
        }

        /// <summary>
        /// Override CleanupTest to clean up emails.
        /// </summary>
        public override void CleanupTest()
        {
            Log("🧹 Cleaning up Performer business exception test");
            
            // Clean up emails
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            MailFilter filter = new MailFilter();
            
            var emails = workflows.GetEmails("eyashb", filter);
            workflows.DeleteEmails(emails.ToList());
            
            Log("✅ Performer business exception test cleanup completed");
        }
    }
}
