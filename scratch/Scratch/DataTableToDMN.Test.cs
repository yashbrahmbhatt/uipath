using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Scratch
{
    public class DataTableToDMN_Test : CodedWorkflow
    {
        [TestCase]
        public void Execute()
        {
            // Arrange
            Log("Test run started for DataTableToDMN_Test.");

            // Act
            var dt = new DataTable();
            dt.Columns.Add("Country");
            dt.Columns.Add("Age", typeof(int));
            dt.Columns.Add("Risk Category");

            // Add rows...
            dt.Rows.Add("US", 25, "Low");
            dt.Rows.Add("US", 60, "High");
            dt.Rows.Add("CA", 30, "Medium");

            // Assert
            var dmnXml = workflows.DataTableToDMN(dt, "TestDecision", "UNIQUE");
            File.WriteAllText("DecisionTable.dmn", dmnXml);
        }
    }
}