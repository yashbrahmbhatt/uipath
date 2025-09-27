using Finance.Automations.ObjectRepository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UiPath.Activities.System.Jobs.Coded;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using UiPath.UIAutomationNext.API.Contracts;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;

namespace Finance.Automations
{
    public class Workflow : CodedWorkflow
    {
        [Workflow]
        public void Execute()
        {
            var ApplicantIdNumbers = new[] { "1234324324531", "" };
            var ApplicantIdTypes = new[] { "", "2", "", "0", "", "0" };

            var result = ApplicantIdTypes
                .Zip(ApplicantIdNumbers, (type, number) => new { type, number })
                .Where(x => !string.IsNullOrWhiteSpace(x.type)
                            && x.type != "0"
                            && !string.IsNullOrWhiteSpace(x.number)
                            && x.number != "0")
                .Select(x => $"{x.type} - {x.number}")
                .ToList();

            string output = string.Join(";", result);
        }
    }
}