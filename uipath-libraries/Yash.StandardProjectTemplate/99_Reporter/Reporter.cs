using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using Yash.Config.Models;
using Yash.StandardProject.CodedWorkflows;
using Yash.StandardProject.CodedWorkflows.Reporter;
using Yash.Utility;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject._99_Reporter
{
    /// <summary>
    /// A workflow class that handles reporting tasks, including filtering data, combining multiple data sources, and generating reports.
    /// Inherits from ReporterWorkflow to leverage standardized reporter functionality and patterns.
    /// </summary>
    public class Reporter : ReporterWorkflow
    {
        #region Abstract Property Implementations
        
        /// <summary>
        /// Path to the configuration file.
        /// </summary>
        public override string ConfigPath { get; set; } = "Data\\Config.xlsx";
        
        /// <summary>
        /// Configuration scopes required for the reporter workflow.
        /// </summary>
        public override string[] ConfigScopes { get; set; } = { "Shared", "Reporter" };
        
        #endregion

        #region Data Processing Methods (Override Base Class Methods)
        
        /// <summary>
        /// Filters columns in a DataTable based on a list of column names.
        /// </summary>
        /// <param name="originalTable">The original DataTable to filter.</param>
        /// <param name="columnNames">A list of column names to include in the filtered table.</param>
        /// <returns>A new DataTable containing only the specified columns.</returns>
        protected override DataTable FilterColumnsByNames(DataTable originalTable, List<string> columnNames)
        {
            
            // Create a new DataTable with the selected columns
            DataTable filteredTable = new();

            // Add only the columns specified in columnNames
            foreach (string columnName in columnNames)
            {
                if (originalTable.Columns.Contains(columnName))
                {
                    filteredTable.Columns.Add(originalTable.Columns[columnName].ColumnName);
                }
            }

            // Add rows to the filtered table
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = filteredTable.NewRow();
                foreach (string columnName in columnNames)
                {
                    if (originalTable.Columns.Contains(columnName))
                    {
                        newRow[columnName] = row[columnName];
                    }
                }
                filteredTable.Rows.Add(newRow);
            }

            return filteredTable;
        }

        /// <summary>
        /// Combines two DataTables based on a shared key column.
        /// </summary>
        /// <param name="table1">The first DataTable.</param>
        /// <param name="table2">The second DataTable to merge.</param>
        /// <param name="key">The key column to join the tables on.</param>
        /// <returns>A new DataTable with data from both tables combined based on the key column.</returns>
        protected override DataTable CombineDataTables(DataTable table1, DataTable table2, string key)
        {
            // Create a new DataTable to hold the combined result
            DataTable combinedTable = table1.Clone();  // Clone schema from table1

            // Merge the two DataTables into a dictionary by Reference
            var table2Dictionary = table2.AsEnumerable()
                                          .ToDictionary(row => row.Field<string>(key), row => row);

            // Add any columns from table2 that are not in table1 to the combined table
            foreach (DataColumn column in table2.Columns)
            {
                if (!combinedTable.Columns.Contains(column.ColumnName) && column.ColumnName != key)
                {
                    combinedTable.Columns.Add(column.ColumnName, column.DataType);
                }
            }

            // Loop through each row of table1
            foreach (DataRow row1 in table1.Rows)
            {
                string reference = row1.Field<string>(key);

                // Check if there's a corresponding row in table2
                if (table2Dictionary.ContainsKey(reference))
                {
                    DataRow row2 = table2Dictionary[reference];

                    // Create a new row for the combined table
                    DataRow newRow = combinedTable.NewRow();
                    newRow[key] = reference;

                    // Iterate through all columns in table1
                    foreach (DataColumn column in table1.Columns)
                    {
                        if (column.ColumnName != key)
                        {
                            newRow[column.ColumnName] = row1[column.ColumnName] ?? row2[column.ColumnName];
                        }
                    }

                    // Add columns from table2 that are not in table1
                    foreach (DataColumn column in table2.Columns)
                    {
                        if (!table1.Columns.Contains(column.ColumnName) && column.ColumnName != key)
                        {
                            newRow[column.ColumnName] = row2[column.ColumnName];
                        }
                    }

                    // Add the new row to the combined table
                    combinedTable.Rows.Add(newRow);
                }
                else
                {
                    // If no corresponding row in table2, just use the row from table1
                    DataRow newRow = combinedTable.NewRow();
                    newRow[key] = reference;

                    foreach (DataColumn column in table1.Columns)
                    {
                        if (column.ColumnName != key)
                        {
                            newRow[column.ColumnName] = row1[column.ColumnName];
                        }
                    }

                    // Add the new row to the combined table
                    combinedTable.Rows.Add(newRow);
                }
            }

            return combinedTable;
        }

        #endregion

        #region Implementation of Abstract Methods
        
        /// <summary>
        /// Implements the core reporter logic as required by the abstract base class.
        /// Generates reports from queue data and sends them via email.
        /// </summary>
        protected override void GenerateReport()
        {
            Log("Initializing reporter", LogLevel.Info);

            var str_queueFile = workflows.ReporterLogicGetQueueData(SharedConfig.QueueName, SharedConfig.QueueFolder, ReporterConfig.Folder_Temp);
            var dt_QueueData = workflows.ReporterLogicProcessQueueData(FromDate, ToDate, str_queueFile, ReporterConfig.StatusesToReport, ReporterConfig.TimeSaved_Success, ReporterConfig.TimeSaved_BusEx, ReporterConfig.TimeSaved_SysEx);

            Dictionary<string, string> list_FriendlyViewColumns = new Dictionary<string, string>()
            {
                {"Reference", "Reference"},
                {"Started (absolute)", "DateProcessed"},
                {"Output.DynamicProperties.Outcome", "ExecutionStatus"},
                {"Output.DynamicProperties.ExceptionReason", "BusinessExceptionReason"}
            };

            var dt_FilteredColumns = FilterColumnsByNames(dt_QueueData, list_FriendlyViewColumns.Keys.ToList());
            for (var i = 0; i < dt_FilteredColumns.Columns.Count; i++)
            {
                DataColumn col = dt_FilteredColumns.Columns[i];
                col.ColumnName = list_FriendlyViewColumns[col.ColumnName] ?? $"Column{i}";
            }

            Log($"Found {dt_QueueData.Rows.Count} rows", LogLevel.Info);

            var str_TempReportPath = Path.Combine(ReporterConfig.Folder_Temp, GenerateReportFileName(ReporterConfig.ProcessName, FromDate, ToDate));

            Log("Writing report", LogLevel.Info);
            CreateExcelReport(dt_QueueData, ReporterConfig.TemplatePath, str_TempReportPath);
            Log("Report generated", LogLevel.Info);

            Dictionary<string, object> dict_EmailTemplate = EmailHelperService.GenerateDiagnosticDictionary(default, default);
            dict_EmailTemplate["SummaryTable"] = dt_FilteredColumns;
            var (subject, body) = EmailHelperService.DefaultEmailTemplates.Report.PrepareEmailTemplate(dict_EmailTemplate, Log);
            workflows.SendEmail(SharedConfig.Report_To, body, subject, new List<string>(){str_TempReportPath}, default);
            
            File.Delete(str_TempReportPath);
            Log("Completed.", LogLevel.Info);
        }
        
        #endregion

        #region Workflow Execution
        
        /// <summary>
        /// Main workflow entry point that accepts parameters for flexible execution.
        /// </summary>
        /// <param name="configPath">The path to the configuration file.</param>
        /// <param name="configScopes">Configuration scopes to load.</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        [Workflow]
        public override void Execute(string configPath, string[] configScopes, string testId = "")
        {
            ConfigPath = configPath ?? ConfigPath;
            ConfigScopes = configScopes ?? ConfigScopes;
            TestId = testId;
            
            // For backward compatibility, use default dates if not set
            if (FromDate == default) FromDate = DateTime.Today.AddDays(-1);
            if (ToDate == default) ToDate = DateTime.Today;
            
            base.Execute(FromDate, ToDate, Cron, ConfigPath, TestId);
        }
        
        /// <summary>
        /// Alternative execution entry point that matches the signature of Dispatcher and Performer workflows.
        /// Provides compatibility with standard workflow execution patterns.
        /// </summary>
        /// <param name="From">The start date for the report data.</param>
        /// <param name="To">The end date for the report data.</param>
        /// <param name="CRON">The CRON schedule (optional).</param>
        /// <param name="ConfigPath">The path to the configuration file (default is "Data\\Config.xlsx").</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        [Workflow]
        public new void Execute(
            DateTime From,
            DateTime To,
            string CRON = "",
            string ConfigPath = "Data\\Config.xlsx",
            string testId = ""
        )
        {
            base.Execute(From, To, CRON, ConfigPath, testId);
        }
        
        #endregion
    }
}