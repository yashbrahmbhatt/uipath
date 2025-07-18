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
    /// A coded workflow activity that reads a CSV file and converts its contents into a <see cref="DataTable"/>.
    /// </summary>
    public class ReadCSVTable : CodedWorkflow
    {
        /// <summary>
        /// Reads a CSV file and returns its contents as a <see cref="DataTable"/>.
        /// The first row is assumed to contain column headers.
        /// </summary>
        /// <param name="in_str_FilePath">The full path to the CSV file.</param>
        /// <param name="in_str_TableName">Optional name for the resulting DataTable. If null or empty, a GUID is used.</param>
        /// <returns>
        /// A <see cref="DT"/> representing the CSV content, where each line is a row and the first line defines column names.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown when the CSV file does not exist at the specified path.</exception>
        [Workflow]
        public DT Execute(string in_str_FilePath, string in_str_TableName)
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

            // Parse content into rows
            var rows = ParseCsvContent(csvContent);

            DT dt = new(in_str_TableName ?? Guid.NewGuid().ToString());

            if (rows.Length == 0)
            {
                Log("CSV file is empty", LogLevel.Warn);
                return dt;
            }

            // First row = headers
            var columnNames = rows[0];
            foreach (var columnName in columnNames)
            {
                dt.Columns.Add(columnName.Trim());
            }

            // Remaining rows = data
            for (int i = 1; i < rows.Length; i++)
            {
                var rowValues = rows[i];
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
        /// Parses raw CSV content into a jagged string array of rows and columns.
        /// Handles quoted fields, escaped quotes, and multiline rows.
        /// </summary>
        /// <param name="csvContent">The full CSV content as a string.</param>
        /// <returns>
        /// A two-dimensional string array where each sub-array represents a row, and each element in the sub-array is a cell value.
        /// </returns>
        public string[][] ParseCsvContent(string csvContent)
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
                        if (i + 1 < csvContent.Length && csvContent[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // skip escaped quote
                        }
                        else
                        {
                            insideQuotes = false;
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
                            rows.Add([.. currentValues]);
                        }

                        currentField.Clear();
                        currentValues.Clear();

                        if (currentChar == '\r' && i + 1 < csvContent.Length && csvContent[i + 1] == '\n')
                        {
                            i++; // Skip \n
                        }
                    }
                    else if (currentChar == '"')
                    {
                        insideQuotes = true;
                    }
                    else
                    {
                        currentField.Append(currentChar);
                    }
                }
            }

            // Add last line if needed
            if (currentField.Length > 0 || currentValues.Count > 0)
            {
                currentValues.Add(currentField.ToString());
                rows.Add([.. currentValues]);
            }

            return [.. rows];
        }
    }
}
