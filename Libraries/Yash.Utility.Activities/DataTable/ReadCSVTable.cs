using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Testing;
using DT = System.Data.DataTable;

namespace Yash.Utility.Activities.DataTable
{
    /// <summary>
    /// Reads a CSV file and converts it into a DataTable.
    /// </summary>
    public class ReadCSVTable : CodedWorkflow
    {
        /// <summary>
        /// Default table name if none is provided.
        /// </summary>
        public string DefaultTableName { get; }

        public ReadCSVTable()
        {
            DefaultTableName = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Reads a CSV file and returns its contents as a DataTable.
        /// </summary>
        /// <param name="in_str_FilePath">The CSV file path.</param>
        /// <param name="in_str_TableName">Optional table name. If null, a GUID is used.</param>
        /// <returns>A DataTable representation of the CSV file.</returns>
        [Workflow]
        public DT Execute(string in_str_FilePath = "Data\\Mappings\\MemoTextKeyword.csv", string in_str_TableName = null)
        {
            if (!File.Exists(in_str_FilePath))
            {
                Log($"File not found: {in_str_FilePath}", LogLevel.Error);
                throw new FileNotFoundException($"CSV file not found at {in_str_FilePath}");
            }

            Log($"Reading CSV from {in_str_FilePath}", LogLevel.Trace);
            string csvContent = File.ReadAllText(in_str_FilePath);

            // Normalize all line endings to \n (Unix style)
            csvContent = csvContent.Replace("\r\n", "\n");

            // Split the content into rows and parse each row
            var rows = ParseCsvContent(csvContent);

            DT dt = new(in_str_TableName ?? DefaultTableName);

            if (rows.Length == 0)
            {
                Log("CSV file is empty", LogLevel.Warn);
                return dt;
            }

            // Read headers and create columns
            var columnNames = rows[0]; // First row contains column names
            foreach (var columnName in columnNames)
            {
                dt.Columns.Add(columnName.Trim());
            }

            // Read data rows
            for (int i = 1; i < rows.Length; i++)
            {
                var rowValues = rows[i];

                // Ensure row values align with columns
                DataRow dataRow = dt.NewRow();
                for (int j = 0; j < columnNames.Length; j++)
                {
                    dataRow[j] = j < rowValues.Length ? rowValues[j].Trim() : DBNull.Value;
                }

                dt.Rows.Add(dataRow);
            }

            Log($"CSV successfully read: {dt.Columns.Count} columns, {dt.Rows.Count} rows", LogLevel.Trace);
            return dt;
        }

        /// <summary>
        /// Parses CSV content into rows and columns.
        /// </summary>
        /// <param name="csvContent">The CSV content as a string.</param>
        /// <returns>An array of rows, each containing an array of column values.</returns>
        private string[][] ParseCsvContent(string csvContent)
        {
            var rows = new List<string[]>();
            var currentValues = new List<string>();
            var currentField = new StringBuilder();
            bool insideQuotes = false;

            for (int i = 0; i < csvContent.Length; i++)
            {
                char currentChar = csvContent[i];

                if (insideQuotes)
                {
                    if (currentChar == '"')
                    {
                        // Check if next char is also a quote (escaped quote)
                        if (i + 1 < csvContent.Length && csvContent[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip the escaped quote
                        }
                        else
                        {
                            insideQuotes = false; // Close quoted field
                        }
                    }
                    else
                    {
                        currentField.Append(currentChar);
                    }
                }
                else
                {
                    if (currentChar == ',')
                    {
                        currentValues.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else if (currentChar == '\r' || currentChar == '\n')
                    {
                        if (currentField.Length > 0 || currentValues.Count > 0)
                        {
                            currentValues.Add(currentField.ToString());
                            rows.Add(currentValues.ToArray());
                        }
                        currentField.Clear();
                        currentValues.Clear();

                        // Handle cases where \r\n might be together
                        if (currentChar == '\r' && i + 1 < csvContent.Length && csvContent[i + 1] == '\n')
                        {
                            i++; // Skip \n if it's part of \r\n
                        }
                    }
                    else if (currentChar == '"')
                    {
                        insideQuotes = true; // Start quoted value
                    }
                    else
                    {
                        currentField.Append(currentChar);
                    }
                }
            }

            // Add last row if present
            if (currentField.Length > 0 || currentValues.Count > 0)
            {
                currentValues.Add(currentField.ToString());
                rows.Add(currentValues.ToArray());
            }

            return rows.ToArray();
        }


    }
}
