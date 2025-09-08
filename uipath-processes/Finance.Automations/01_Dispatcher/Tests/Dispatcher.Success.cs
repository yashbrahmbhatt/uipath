using System;
using UiPath.CodedWorkflows;


namespace Finance.Automations._01_Dispatcher.Tests
{
    public class Dispatcher_Success : BaseDispatcherTest
    {
        public new string Id { get; set; } = "Dispatcher.Success";
        public new string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public new string ExpectedExceptionMessage { get; set; } = "";

        [TestCase]
        public new  void Execute()
        {
            Log($"🧪 Running Dispatcher test: {Id}");

            Exception actualException = null;
            try
            {
                // Execute the dispatcher workflow with test parameters
                workflows.Dispatcher(ConfigPath, Id);
                
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

            Log("🎉 Dispatcher test completed!");
        }
    }
}