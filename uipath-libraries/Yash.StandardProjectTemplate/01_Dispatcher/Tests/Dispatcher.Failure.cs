using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

namespace Yash.StandardProject._01_Dispatcher.Tests
{
    /// <summary>
    /// Failure test case for Dispatcher workflow.
    /// Inherits from BaseDispatcherTest which uses the IDispatcherTestable interface.
    /// Tests that the dispatcher properly handles and reports failures.
    /// </summary>
    public class Dispatcher_Failure : BaseDispatcherTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "Dispatcher.Failure";

        /// <summary>
        /// Expected exception message - "any" means any exception is expected.
        /// </summary>
        public override string ExpectedExceptionMessage { get; set; } = "any";

        /// <summary>
        /// Execute the failure test case.
        /// </summary>
        [TestCase]
        public void ExecuteTestCase()
        {
            // Use the TestableExtensions framework to execute the test
            this.ExecuteTest();
        }
    }
}