using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;
using UiPath.GSuite.Activities.Api;


namespace Finance.Automations._99_Reporter.Tests
{
    public class Reporter_Failure : BaseReporterTest
    {
        public Reporter_Failure() { }
        public new string Id { get; set; } = "Reporter.Failure";
        public new DateTime FromDate { get; set; } = DateTime.Now.AddDays(-7);
        public new DateTime ToDate { get; set; } = DateTime.Now.AddDays(0);
        public new string Cron { get; set; } = "";
        public new string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public new string ExpectedExceptionMessage { get; set; } = "Reporter.Failure";
        [TestCase]
        public new void Execute()
        {
            Log($"🧪 Running Reporter test: {Id}");
            var connector = connections.Gmail.Shared_yash_brahmbhatt_ybdev_one__2;
            var service = new GmailConnection(connector.ConnectionId, connector.Resolver);
            var filter = new MailFilter();
            filter = filter.BySubject(FilterStringOperator.Contains, "[Reporter] Critical Framework Exception");

            var emails = workflows.GetEmails("eyashb", filter);
            if (emails.Count > 0)
            {
                foreach (var email in emails)
                {
                    google.Gmail(service).DeleteEmail(email);
                }
            }
            Exception actualException = null;
            try
            {
                workflows.Reporter(FromDate, ToDate, Cron, ConfigPath, Id);

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
                    actualException.Message.Contains(ExpectedExceptionMessage) || ExpectedExceptionMessage == "any",
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

            if (emails.Count > 0)
            {
                foreach (var email in emails)
                {
                    google.Gmail(service).MarkEmailAsRead(email);
                }
            }
            Log("🎉 Reporter test completed!");
        }
    }
}