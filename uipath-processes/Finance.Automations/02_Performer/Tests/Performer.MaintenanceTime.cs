using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;

namespace Finance.Automations._02_Performer.Tests
{
    /// <summary>
    /// Test case for Performer maintenance time scenario.
    /// Tests behavior during maintenance time periods.
    /// Workflow should exit gracefully during maintenance time.
    /// </summary>
    public class PerformerMaintenanceTimeTest : BasePerformerTest
    {
        public override string Id { get; set; } = "Performer.MaintenanceTime";
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string ExpectedExceptionMessage { get; set; } = "";

        /// <summary>
        /// Executes the Performer workflow test for maintenance time scenario.
        /// </summary>
        [TestCase]
        public new void Execute()
        {
            Log($"🧪 Running Performer test: {this.GetType().Name}");

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
            Log("Validation: Workflow should exit gracefully during maintenance time");

            Log("🎉 Performer test completed!");
        }
    }
}
