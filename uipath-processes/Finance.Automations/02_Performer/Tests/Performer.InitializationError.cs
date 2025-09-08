using System;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;
using UiPath.Testing;

namespace Finance.Automations._02_Performer.Tests
{
    /// <summary>
    /// Test case for Performer initialization failure scenario.
    /// Tests framework exception during initialization phase.
    /// </summary>
    public class PerformerInitializationErrorTest : BasePerformerTest
    {
        public override string Id { get; set; } = "Performer.InitializationError";
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string ExpectedExceptionMessage { get; set; } = "Performer.InitializationError";

        /// <summary>
        /// Executes the Performer workflow test for initialization error scenario.
        /// </summary>
        [TestCase]
        public new void Execute()
        {
            Log($"🧪 Running Performer test: {this.GetType().Name}");
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Performer] Critical Framework Exception");
            
            var emails = workflows.GetEmails("eyashb", filter);
            if(emails.Count >0){
                foreach(var email in emails){
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            
            
            Exception actualException = null;
            try
            {
                // Execute the performer workflow with test parameters
                workflows.Performer(ConfigPath, Id);
                
                // Add small delay to allow for email processing
                System.Threading.Thread.Sleep(2000);
            }
            catch (Exception exception)
            {
                actualException = exception;
            }

            // Validate exception expectations using UiPath testing framework
            testing.VerifyExpression(
                (actualException == null && string.IsNullOrWhiteSpace(ExpectedExceptionMessage)) || 
                (actualException != null && !string.IsNullOrWhiteSpace(ExpectedExceptionMessage)), 
                $"Expected failure status ({!string.IsNullOrWhiteSpace(ExpectedExceptionMessage)}) did not match output ({actualException != null})"
            );
            
            if (actualException != null && !string.IsNullOrWhiteSpace(ExpectedExceptionMessage))
            {
                testing.VerifyExpression(
                    actualException.Message.Contains(ExpectedExceptionMessage),
                    $"Expected exception message to contain '{ExpectedExceptionMessage}', but got '{actualException.Message}'"
                );
                Log($"✅ Test failed as expected: {actualException.Message}");
            }
            else
            {
                Log($"✅ Test completed successfully");
            }

            // Additional validations for this test case
                System.Threading.Thread.Sleep(5000);
            
            emails = workflows.GetEmails("eyashb", filter);
            testing.VerifyExpression(emails.Count > 0, "Found email");
            
            if(emails.Count >0){
                foreach(var email in emails){
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            Log("🎉 Performer test completed!");
        }
    }
}
