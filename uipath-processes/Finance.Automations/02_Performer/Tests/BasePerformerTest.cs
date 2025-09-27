using System;
using Finance.Automations.CodedWorkflows;
using UiPath.CodedWorkflows;
using UiPath.Testing;

namespace Finance.Automations._02_Performer.Tests
{
    /// <summary>
    /// Base class for Performer test cases.
    /// Defines abstract properties that concrete test implementations must provide.
    /// </summary>
    public class BasePerformerTest : BaseCodedWorkflow
    {
        /// <summary>
        /// Unique identifier for the test case.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        public virtual string ConfigPath { get; set; }

        /// <summary>
        /// Expected exception message if the test should fail (empty string means success expected).
        /// </summary>
        public virtual string ExpectedExceptionMessage { get; set; }

        /// <summary>
        /// Executes the Performer workflow test with the defined properties.
        /// </summary>
        [TestCase]
        public void Execute()
        {
            Log($"🧪 Running Performer test: {this.GetType().Name}");

            Exception actualException = null;
            try
            {
                // Execute the performer workflow with test parameters
                workflows.Performer(ConfigPath, Id);
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

            Log("🎉 Performer test completed!");
        }
    }
}