using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace Finance.Automations._00_Shared
{
    /// <summary>
    /// Reads data from an Excel file and loads it into a DataSet.
    /// </summary>
    public class ReadExcelFile : CodedWorkflow
    {
        /// <summary>
        /// Reads an Excel file from the specified path and returns its content as a DataSet.
        /// Each sheet in the Excel file is loaded into a separate DataTable.
        /// </summary>
        /// <param name="path">The path of the Excel file to read.</param>
        /// <returns>A DataSet containing the data from all sheets in the Excel file.</returns>
        [Workflow]
        public DataSet Execute(string path)
        {
            Log($"Reading excel file {path}", LogLevel.Trace);  // Log the start of the reading process.
            if (!File.Exists(path)) throw new FileNotFoundException($"File not found {path}");
            var dataset = new DataSet();  // Create a new DataSet to hold the data from the Excel file.
            
            // Initialize the Excel process scope.
            var scope_Excel = excel.ExcelProcessScope();

            // Open the Excel file without making any changes (false for both parameters).
            var handle_Excel = excel.UseExcelFile(path, false, false);

            // Iterate over each sheet in the Excel file.
            handle_Excel.ForEachSheet((sheet) =>
            {
                // Read the range of data from the sheet and add it as a DataTable.
                var table = sheet.ReadRange(true, false);  // Read the range, including headers (true).

                if (table.Rows.Count > 0)
                {
                    table.TableName = sheet.Name;  // Set the table name to the sheet's name.

                    dataset.Tables.Add(table);  // Add the DataTable to the DataSet.
                }

                return true;  // Continue processing the next sheet.
            });

            handle_Excel.Dispose();  // Release the Excel file handle to free resources.
            scope_Excel.Dispose();  // Release the Excel process scope to free resources.

            Log($"Excel file read with {dataset.Tables.Count} tables", LogLevel.Trace);  // Log the number of tables read.

            return dataset;  // Return the DataSet containing all the tables.
        }
    }
}
