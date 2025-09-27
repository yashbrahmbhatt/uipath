using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Finance.Automations.Configs;
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
using Finance.Automations.CodedWorkflows;

namespace Finance.Automations._99_Reporter
{
    /// <summary>
    /// A workflow class that handles reporting tasks, including filtering data, combining multiple data sources, and generating reports.
    /// </summary>
    public class Reporter : BaseCodedWorkflow
    {
        
        /// <summary>
        /// Filters columns in a DataTable based on a list of column names.
        /// </summary>
        /// <param name="originalTable">The original DataTable to filter.</param>
        /// <param name="columnNames">A list of column names to include in the filtered table.</param>
        /// <returns>A new DataTable containing only the specified columns.</returns>
        public DataTable FilterColumnsByNames(DataTable originalTable, List<string> columnNames)
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
        public DataTable CombineDataTables(DataTable table1, DataTable table2, string key)
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

        /// <summary>
        /// Executes the reporting workflow, which loads configuration, processes queue data, filters and combines data, and generates a report.
        /// </summary>
        /// <param name="ConfigNames">Comma-separated names of the configuration files to load.</param>
        /// <param name="From">The start date for the report data.</param>
        /// <param name="To">The end date for the report data.</param>
        /// <param name="CRON">The CRON schedule (optional).</param>
        /// <param name="OutputFolder">The folder where the report will be saved (optional).</param>
        /// <param name="ConfigPath">The path to the configuration file (default is "Data\\Config.json").</param>
        /// <returns>A tuple containing the configuration, report file path, and email template data.</returns>
        [Workflow]
        public void Execute(
            DateTime From,
            DateTime To,
            string CRON = "",
            string ConfigPath = "Data\\Config.xlsx",
            string TestId = ""
        )
        {
            (var confic_Shared, _, _, var c_Config) = LoadConfig(ConfigPath, new [] {"Shared", "Reporter"});
            

            try
            {
                Log("Initializing reporter");
                if(TestId == "Reporter.Failure") throw new Exception(TestId);
                workflows.ResetTempFolder(c_Config.Folder_Temp);

                var str_queueFile = workflows.ReporterLogicGetQueueData(confic_Shared.QueueName, confic_Shared.QueueFolder, c_Config.Folder_Temp);
                var dt_QueueData = workflows.ReporterLogicProcessQueueData(From, To, str_queueFile, c_Config.StatusesToReport, c_Config.TimeSaved_Success, c_Config.TimeSaved_BusEx, c_Config.TimeSaved_SysEx);

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

                Log($"Found {dt_QueueData.Rows.Count} rows");

                var str_TempReportPath = Path.Combine(c_Config.Folder_Temp, $"{c_Config.ProcessName} Process {From.ToString("yyyy-MM-dd")} - {To.ToString("yyyy-MM-dd")}.xlsx");
                File.Copy(c_Config.TemplatePath, str_TempReportPath);

                Log("Writing report");
                var scope_Excel = excel.ExcelProcessScope();
                var handle_Excel = excel.UseExcelFile(str_TempReportPath, true, false);
                handle_Excel.Sheet["Performer Data"].WriteRange(dt_QueueData, false, false);
                handle_Excel.RefreshDataConnections();
                handle_Excel.SaveExcelFile();
                handle_Excel.Dispose();
                Log("Report generated");

                Dictionary<string, object> dict_EmailTemplate = workflows.GenerateDiagnosticDictionary(default, default);
                dict_EmailTemplate["SummaryTable"] = dt_FilteredColumns;
                
                workflows.SendEmail(confic_Shared.Report_To, confic_Shared.Email_Report, new List<string>(){str_TempReportPath}, dict_EmailTemplate, default);
                
                File.Delete(str_TempReportPath);
                Log("Completed.");
            }
            catch (Exception ex)
            {
                var dict_Diagnostic = workflows.GenerateDiagnosticDictionary(ex, default);
                dict_Diagnostic["ProcessName"] = c_Config.ProcessName;
                workflows.SendEmail(confic_Shared.SysEx_To, confic_Shared.Email_FrameEx, default, dict_Diagnostic, default);
                throw;
            }
        }
    }
}