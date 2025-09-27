using System;
using UiPath.CodedWorkflows;
using UiPath.Testing;
using Yash.StandardProject.CodedWorkflows;

namespace Yash.StandardProject._02_Performer.Tests
{
    /// <summary>
    /// Test case for successful Performer execution.
    /// Tests normal performer execution without errors.
    /// Inherits from BasePerformerTest which uses the IPerformerTestable interface.
    /// </summary>
    public class PerformerSuccessTest : BasePerformerTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "Performer.Success";
        
        /// <summary>
        /// Expected exception message - empty string means success is expected.
        /// </summary>
        public override string ExpectedExceptionMessage { get; set; } = "";

        [TestCase]
        public void ExecuteTestCase()
        {
            // Use the TestableExtensions framework to execute the test
            this.ExecuteTest();
        }
    }
}
