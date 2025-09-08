using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
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

namespace Finance.Automations._99_Reporter.Logic
{
    /// <summary>
    /// A workflow class that flattens nested JSON structures within a <see cref="DataTable"/>.
    /// It iterates through the rows and columns of the provided table and flattens any JSON strings into individual columns.
    /// </summary>
    public class ReporterLogicFlattenTable : CodedWorkflow
    {
        /// <summary>
        /// Parses a JSON string, handling any surrounding quotes and potential nested JSON strings within properties.
        /// </summary>
        /// <param name="jsonString">The JSON string to be parsed.</param>
        /// <returns>A <see cref="JObject"/> representing the parsed JSON data.</returns>
        private static JObject ParseJson(string jsonString)
        {
            // Check if the string has surrounding quotes (i.e., it's an escaped JSON string)
            if (jsonString.StartsWith("\"") && jsonString.EndsWith("\""))
            {
                // Remove the surrounding quotes if present
                jsonString = jsonString.Substring(1, jsonString.Length - 2);
            }

            // Parse the outer JSON string into a JObject
            JObject outerJson;
            try
            {
                outerJson = JObject.Parse(jsonString);
            }
            catch (Exception)
            {
                outerJson = new JObject();
            }

            // Now check if any property in the outer JSON is itself a JSON string
            foreach (var property in outerJson.Properties())
            {
                // If the property value is a string that is a JSON string, parse it again
                if (property.Value is JValue jValue && jValue.Type == JTokenType.String)
                {
                    string innerJsonString = jValue.ToString();

                    // If the inner JSON string is wrapped in quotes, remove those quotes and unescape it
                    if (innerJsonString.StartsWith("\"") && innerJsonString.EndsWith("\""))
                    {
                        innerJsonString = innerJsonString.Substring(1, innerJsonString.Length - 2);
                    }

                    // Unescape the inner JSON string
                    innerJsonString = innerJsonString.Replace("\\\"", "\"");

                    // Parse the inner JSON string and assign it back to the property
                    try
                    {
                        property.Value = JObject.Parse(innerJsonString);
                    }
                    catch (Exception)
                    {
                        property.Value = innerJsonString;
                    }
                }
            }

            return outerJson;
        }

        /// <summary>
        /// Recursively flattens a nested JSON object and adds its properties to the provided row of the flattened table.
        /// The flattened column names are constructed by concatenating parent property names with child property names.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JObject"/> representing the JSON data to be flattened.</param>
        /// <param name="parentColumnName">The parent column name used to construct the new flattened column names.</param>
        /// <param name="newRow">The row in the flattened table where the values should be added.</param>
        /// <param name="flattenedTable">The <see cref="DataTable"/> that will hold the flattened data.</param>
        private static void FlattenJson(JObject jsonObject, string parentColumnName, DataRow newRow, DataTable flattenedTable)
        {
            // Iterate through the keys of the JSON object
            foreach (var property in jsonObject.Properties())
            {
                string newColumnName = $"{parentColumnName}.{property.Name}";

                // If the value is a nested JSON object, call FlattenJson recursively
                if (property.Value is JObject nestedObject)
                {
                    FlattenJson(nestedObject, newColumnName, newRow, flattenedTable);
                }
                else
                {
                    // If the value is not an object, just add the value to the row
                    if (!flattenedTable.Columns.Contains(newColumnName))
                    {
                        flattenedTable.Columns.Add(newColumnName); // Add the new column to the DataTable
                    }
                    var value = property.Value.ToString();
                    if (IsJson(value))
                    {
                        var jobject = ParseJson(value);
                        FlattenJson(jobject, newColumnName, newRow, flattenedTable);
                    }
                    else
                    {
                        newRow[newColumnName] = property.Value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether a given string is a valid JSON object.
        /// </summary>
        /// <param name="input">The string to be checked.</param>
        /// <returns><c>true</c> if the string is a valid JSON object, otherwise <c>false</c>.</returns>
        private static bool IsJson(string input)
        {
            // Check if the string can be parsed as a JSON object
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"));
        }

        /// <summary>
        /// Flattens a <see cref="DataTable"/> by iterating through its rows and columns and flattening any JSON string values into individual columns.
        /// The result is a new <see cref="DataTable"/> with the same structure but with flattened JSON data.
        /// </summary>
        /// <param name="OriginalTable">The <see cref="DataTable"/> to be flattened.</param>
        /// <returns>A new <see cref="DataTable"/> with the flattened data.</returns>
        [Workflow]
        public DataTable Execute(DataTable OriginalTable)
        {
            // Create a new DataTable for the flattened data
            DataTable flattenedTable = OriginalTable.Clone(); // Create a clone to preserve column structure (without data)

            // Iterate over the rows in the original table
            foreach (DataRow row in OriginalTable.Rows)
            {
                DataRow newRow = flattenedTable.NewRow(); // Create a new row for the flattened table

                // Iterate over the columns in the original table
                foreach (DataColumn column in OriginalTable.Columns)
                {
                    var cellValue = row[column];

                    // If the cell contains a JSON string, flatten it
                    if (cellValue is string jsonString && IsJson(jsonString))
                    {
                        var jsonObject = ParseJson(jsonString);
                        FlattenJson(jsonObject, column.ColumnName, newRow, flattenedTable);
                    }
                    else
                    {
                        // If the cell is not JSON, just copy the value
                        newRow[column.ColumnName] = cellValue;
                    }
                }

                // Add the new row to the flattened table
                flattenedTable.Rows.Add(newRow);
            }

            return flattenedTable;
        }
    }

}