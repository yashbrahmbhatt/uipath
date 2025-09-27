/*
 * File: ReporterWorkflow.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Abstract base class for Reporter workflows that provides
 *              common reporter functionality including report generation,
 *              data processing, and standardized execution patterns.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using Yash.Utility;
using LogLevel = UiPath.CodedWorkflows.LogLevel;

namespace Yash.StandardProject.CodedWorkflows.Reporter
{
    /// <summary>
    /// Abstract base class for Reporter workflows.
    /// Provides common reporter functionality including report generation,
    /// data processing, error handling patterns, and standardized execution flow.
    /// </summary>
    public abstract class ReporterWorkflow : CodedWorkflowWithConfig
    {
        #region Reporter-Specific Properties
        
        /// <summary>
        /// From date for report data filtering.
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// To date for report data filtering.
        /// </summary>
        public DateTime ToDate { get; set; }

        /// <summary>
        /// CRON schedule for report execution.
        /// </summary>
        public string Cron { get; set; } = "";
        
        #endregion

        #region Main Workflow Entry Points
        
        /// <summary>
        /// Main workflow entry point that accepts parameters for flexible execution.
        /// </summary>
        /// <param name="From">The start date for the report data.</param>
        /// <param name="To">The end date for the report data.</param>
        /// <param name="CRON">The CRON schedule (optional).</param>
        /// <param name="ConfigPath">The path to the configuration file (default is "Data\\Config.xlsx").</param>
        /// <param name="testId">Test identifier for implementing test-specific behaviors (optional).</param>
        public virtual void Execute(
            DateTime From,
            DateTime To,
            string CRON = "",
            string ConfigPath = "Data\\Config.xlsx",
            string testId = ""
        )
        {
            // Set properties for this execution
            this.FromDate = From;
            this.ToDate = To;
            this.Cron = CRON;
            this.ConfigPath = ConfigPath;
            this.TestId = testId;
            
            // Initialize configuration first
            InitializeConfiguration();
            
            // Execute the reporter logic
            ExecuteReporterLogic();
        }
        
        #endregion

        #region Core Reporter Logic
        
        /// <summary>
        /// Executes the main reporter workflow logic with standardized error handling.
        /// </summary>
        protected virtual void ExecuteReporterLogic()
        {
            // Validate that required configurations are loaded
            ValidateRequiredConfigurations(new[] { "Shared", "Reporter" });

            try
            {
                Log("Starting report generation", LogLevel.Info);
                
                // Handle test variations first (may throw exceptions for testing)
                HandleTestVariations();
                
                // Reset temporary folder
                ResetTempFolder();
                
                // Generate the report
                GenerateReport();
                
                Log("Report generation completed successfully", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error occurred during report generation: {ex.Message}", LogLevel.Error);
                HandleReporterError(ex);
                throw;
            }
        }
        
        /// <summary>
        /// Handles test variations for exception scenarios.
        /// Override this method in test classes if needed.
        /// </summary>
        protected virtual void HandleTestVariations()
        {
            // Implement test-specific behavior based on TestId
            if (!string.IsNullOrEmpty(TestId))
            {
                Log($"Applying test behavior for TestId: {TestId}", LogLevel.Info);
                
                switch (TestId.ToLower())
                {
                    case "systemexception":
                    case "sysex":
                        throw new ApplicationException($"Test system exception triggered by TestId: {TestId}");
                        
                    case "businessexception":
                    case "busex":
                        throw new BusinessRuleException($"Test business exception triggered by TestId: {TestId}");
                        
                    case "frameworkexception":
                    case "frameex":
                        throw new InvalidOperationException($"Test framework exception triggered by TestId: {TestId}");
                        
                    default:
                        // Normal execution for other test IDs
                        Log($"Normal execution with TestId: {TestId}", LogLevel.Info);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Generates the report by processing data and creating output files.
        /// Override this method to implement specific report generation logic.
        /// </summary>
        protected abstract void GenerateReport();
        
        #endregion

        #region Data Processing Helper Methods
        
        /// <summary>
        /// Filters columns in a DataTable based on a list of column names.
        /// </summary>
        /// <param name="originalTable">The original DataTable to filter.</param>
        /// <param name="columnNames">A list of column names to include in the filtered table.</param>
        /// <returns>A new DataTable containing only the specified columns.</returns>
        protected virtual DataTable FilterColumnsByNames(DataTable originalTable, List<string> columnNames)
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
        protected virtual DataTable CombineDataTables(DataTable table1, DataTable table2, string key)
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

        #region Report Generation Helper Methods
        
        /// <summary>
        /// Creates an Excel report from a DataTable using a template.
        /// </summary>
        /// <param name="data">The data to write to the report</param>
        /// <param name="templatePath">Path to the Excel template</param>
        /// <param name="outputPath">Path where the report will be saved</param>
        /// <param name="sheetName">Name of the sheet to write data to (default: "Performer Data")</param>
        protected virtual void CreateExcelReport(DataTable data, string templatePath, string outputPath, string sheetName = "Performer Data")
        {
            Log($"Creating Excel report: {outputPath}", LogLevel.Info);
            
            File.Copy(templatePath, outputPath, true);

            var scope_Excel = excel.ExcelProcessScope();
            var handle_Excel = excel.UseExcelFile(outputPath, true, false);
            handle_Excel.Sheet[sheetName].WriteRange(data, false, false);
            handle_Excel.SaveExcelFile();
            handle_Excel.Dispose();
            
            Log("Excel report created successfully", LogLevel.Info);
        }
        
        /// <summary>
        /// Resets the temporary folder used for report generation.
        /// </summary>
        protected virtual void ResetTempFolder()
        {
            Log("Resetting temporary folder", LogLevel.Info);
            MiscHelperService.ResetFolder(ReporterConfig.Folder_Temp);
        }
        
        /// <summary>
        /// Generates a standardized report file name based on process name and date range.
        /// </summary>
        /// <param name="processName">Name of the process</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="extension">File extension (default: .xlsx)</param>
        /// <returns>Formatted file name</returns>
        protected virtual string GenerateReportFileName(string processName, DateTime fromDate, DateTime toDate, string extension = ".xlsx")
        {
            return $"{processName} Process {fromDate:yyyy-MM-dd} - {toDate:yyyy-MM-dd}{extension}";
        }
        
        #endregion

        #region Error Handling
        
        /// <summary>
        /// Handles reporter errors by generating diagnostics and sending notifications.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        protected virtual void HandleReporterError(Exception exception)
        {
            // Generate diagnostic information and send notification email
            var diagnosticInfo = EmailHelperService.GenerateDiagnosticDictionary(exception, default);
            diagnosticInfo["ProcessName"] = ReporterConfig.ProcessName;
            var (subject, body) = EmailHelperService.DefaultEmailTemplates.FrameEx.PrepareEmailTemplate(diagnosticInfo, Log);
            workflows.SendEmail(SharedConfig.SysEx_To, body, subject, default, default);
        }
        
        #endregion

        #region Equality and Hash Code
        
        /// <summary>
        /// Determines equality based on reporter configuration.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is ReporterWorkflow workflow &&
                   TestId == workflow.TestId &&
                   ConfigPath == workflow.ConfigPath &&
                   System.Collections.Generic.EqualityComparer<string[]>.Default.Equals(ConfigScopes, workflow.ConfigScopes) &&
                   FromDate == workflow.FromDate &&
                   ToDate == workflow.ToDate &&
                   Cron == workflow.Cron;
        }

        /// <summary>
        /// Gets hash code based on reporter configuration.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(TestId, ConfigPath, ConfigScopes, FromDate, ToDate, Cron);
        }
        
        #endregion
    }
}