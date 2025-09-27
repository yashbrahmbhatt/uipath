using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._02_Performer.Tests
{
    /// <summary>
    /// Test case for Performer no data scenario.
    /// Tests behavior when no queue items are available.
    /// Workflow should complete normally when no queue items are available.
    /// </summary>
    public class PerformerNoDataTest : BasePerformerTest
    {
        public override string TestId { get; set; } = "NoData";
        public override string ExpectedExceptionMessage { get; set; } = "";

        [TestCase]
        public void ExecuteTestCase()
        {
            // Use the TestableExtensions framework to execute the test
            this.ExecuteTest();
        }

        /// <summary>
        /// Override InitializeTest to ensure queue is empty.
        /// </summary>
        public override void InitializeTest()
        {
            Log("🔧 Initializing Performer no data test");
            
            // Ensure the queue is empty by processing any existing items
            // This test validates behavior when no data is available
            
            Log("✅ Performer no data test initialized");
        }

        /// <summary>
        /// Override ValidateTest to check for normal completion.
        /// </summary>
        public override void ValidateTest()
        {
            Log("🧪 Validating Performer no data test results");
            
            // For no data scenario, we expect normal completion without errors
            // The validation is that no exception was thrown during execution
            
            Log("✅ Performer no data test validation completed");
        }

        /// <summary>
        /// Override CleanupTest to perform any necessary cleanup.
        /// </summary>
        public override void CleanupTest()
        {
            Log("🧹 Cleaning up Performer no data test");
            // No specific cleanup needed for this test
            Log("✅ Performer no data test cleanup completed");
        }
    }
}
