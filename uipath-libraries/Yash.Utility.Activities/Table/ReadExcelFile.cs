using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel.Activities.API;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Utility.Activities.Table
{
    /// <summary>
    /// A coded workflow activity that reads data from an Excel file and loads it into a <see cref="DataSet"/>.
    /// Each worksheet in the Excel file is imported as a separate <see cref="DataTable"/>.
    /// </summary>
    public class ReadExcelFile : CodedWorkflow
    {
        /// <summary>
        /// Reads an Excel file from the specified path and returns its contents as a <see cref="DataSet"/>.
        /// Each sheet in the Excel file is loaded into a separate <see cref="DataTable"/>, named after the sheet.
        /// </summary>
        /// <param name="in_str_FilePath">The full file path of the Excel workbook to read. Defaults to "Data\Config.xlsx".</param>
        /// <returns>
        /// A <see cref="DataSet"/> containing one <see cref="DataTable"/> per sheet in the Excel file.
        /// Tables will only be added for sheets with at least one row of data.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown if the specified Excel file does not exist.</exception>
        [Workflow]
        public DataSet Execute(string in_str_FilePath = "Data\\Config.xlsx")
        {
            Log($"Reading excel file {in_str_FilePath}", LogLevel.Trace);  // Log the start of the reading process.

            if (!File.Exists(in_str_FilePath))
                throw new FileNotFoundException($"File not found {in_str_FilePath}");

            var dataset = new DataSet();  // Create a new DataSet to hold the data from the Excel file.

            // Initialize the Excel process scope.
            var scope_Excel = excel.ExcelProcessScope();

            // Open the Excel file without making any changes (false for both parameters).
            var handle_Excel = excel.UseExcelFile(in_str_FilePath, false, false);

            // Iterate over each sheet in the Excel file.
            handle_Excel.ForEachSheet(sheet =>
            {
                // Read the range of data from the sheet and add it as a DataTable.
                var table = sheet.ReadRange(true, false);  // Read the range, including headers (true).

                Log($"Found {table.Rows.Count} rows in sheet {sheet.Name}");

                if (table.Rows.Count > 0)
                {
                    table.TableName = sheet.Name;  // Set the table name to the sheet's name.
                    dataset.Tables.Add(table);     // Add the DataTable to the DataSet.
                }

                return true;  // Continue processing the next sheet.
            });

            handle_Excel.Dispose();  // Release the Excel file handle to free resources.
            scope_Excel.Dispose();   // Release the Excel process scope to free resources.

            Log($"Excel file read with {dataset.Tables.Count} tables", LogLevel.Trace);  // Log the number of tables read.

            return dataset;  // Return the DataSet containing all the tables.
        }
    }
}