using System;
using System.Collections.Generic;
using System.Data;
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
    /// Success test case for Dispatcher workflow.
    /// Inherits from BaseDispatcherTest which uses the IDispatcherTestable interface.
    /// </summary>
    public class Dispatcher_Success : BaseDispatcherTest
    {
        /// <summary>
        /// Test identifier for this specific test case.
        /// </summary>
        public override string TestId { get; set; } = "Dispatcher.Success";
        
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