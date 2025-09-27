/*
 * File: Dispatcher.cs
 * Project: Yash Standard Project
 * Component: Dispatcher (01_Dispatcher)
 * 
 * Description: Main dispatcher workflow responsible for creating queue items
 *              from input data sources for processing by Performer workflows.
 * 
 * Author: Yash Team
 * Created: 2025
 * Modified: September 2025
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities;
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
using Yash.StandardProject.CodedWorkflows.Dispatcher;
using Yash.Utility;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._01_Dispatcher
{
    /// <summary>
    /// Dispatcher workflow that processes queue observations based on configuration and dispatches them to the queue.
    /// Inherits from DispatcherWorkflow to leverage standardized dispatcher functionality and patterns.
    /// </summary>
    public class Dispatcher : DispatcherWorkflow
    {
        #region Property Implementations

        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        public override string[] ConfigScopes { get; set; } = new[] { "Shared", "Dispatcher" };

        public override void CloseApplications()
        {
        }

        [Workflow]
        public override void Execute(string configPath, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            TestId = testId;
            base.Execute(ConfigPath, TestId);
        }

        public override void InitializeApplications()
        {
        }

        public override int ProcessInputData()
        {
            return 0;
        }

        #endregion
    }
}