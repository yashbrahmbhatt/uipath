using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
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
using Yash.Config.Helpers;

namespace Yash.StandardProject._99_Reporter.Logic
{
    /// <summary>
    /// A workflow class that processes queue data by parsing a CSV file, filtering rows based on provided parameters,
    /// adding calculated columns, and flattening the data into a new table.
    /// </summary>
    public class ReporterLogicProcessQueueData : CodedWorkflow
    {
        /// <summary>
        /// Processes the queue data by reading a CSV file, filtering rows based on the provided date range and statuses,
        /// adding calculated columns, and flattening the data.
        /// </summary>
        /// <param name="From">The start date and time for filtering the queue items.</param>
        /// <param name="To">The end date and time for filtering the queue items.</param>
        /// <param name="FilePath">The path to the CSV file containing the queue data. Default is "Data\\Temp\\tmp550lbn\\001_DocumentUnderstanding-items-2025-02-05-06-45-22-880-21580.csv".</param>
        /// <param name="Statuses">Comma-separated list of statuses to filter by (e.g., "Successful,Failed"). Default is "Successful,Failed".</param>
        /// <param name="TimeSaved_Success">The time saved for successful items. Default is 10.</param>
        /// <param name="TimeSaved_BusEx">The time saved for items with business exceptions. Default is 2.</param>
        /// <param name="TimeSaved_SysEx">The time saved for items with system exceptions. Default is 0.</param>
        /// <returns>A DataTable containing the processed and flattened queue data.</returns>
        [Workflow]
        public DataTable Execute(DateTime From, DateTime To, string FilePath = @"Data\Temp\tmp550lbn\001_DocumentUnderstanding-items-2025-02-05-06-45-22-880-21580.csv", string Statuses = "Successful,Failed", double TimeSaved_Success = 10, double TimeSaved_BusEx = 2, double TimeSaved_SysEx = 0)
        {
            // Log the start of the parsing process
            Log($"Parsing data");

            // Read the CSV file into a DataTable
            var csvContent = File.ReadAllText(FilePath);
            csvContent = csvContent.Replace("\r\n", "\n");

            // Parse content into rows
            var rows = CSVHelpers.ParseCsvContent(csvContent);

            DataTable dt_Item = new("Report" ?? Guid.NewGuid().ToString());

            // First row = headers
            var columnNames = rows[0];
            foreach (var columnName in columnNames)
            {
                dt_Item.Columns.Add(columnName.Trim());
            }

            // Remaining rows = data
            for (int i = 1; i < rows.Length; i++)
            {
                var rowValues = rows[i];
                DataRow dataRow = dt_Item.NewRow();

                for (int j = 0; j < columnNames.Length; j++)
                {
                    dataRow[j] = j < rowValues.Length ? rowValues[j].Trim() : DBNull.Value;
                }

                dt_Item.Rows.Add(dataRow);
            }

            Log($"CSV successfully read: {dt_Item.Columns.Count} columns, {dt_Item.Rows.Count} rows", LogLevel.Trace);

            // Log the addition of calculated columns
            Log($"Adding calculated columns");

            // Add calculated columns to the filtered DataTable
            var dt_Calculated = workflows.ReporterLogicAddCalculatedColumns(dt_Item, TimeSaved_Success, TimeSaved_BusEx, TimeSaved_SysEx);

            // Log the flattening of the table
            Log($"Flattening table");

            // Flatten the calculated DataTable
            var dt_Flattened = workflows.ReporterLogicFlattenTable(dt_Calculated);

            // Return the flattened DataTable
            return dt_Flattened;
        }
    }

}