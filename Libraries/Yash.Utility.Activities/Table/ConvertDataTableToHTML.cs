using System;
using System.Collections.Generic;
using System.Data;
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

namespace Yash.Utility.Activities.Table
{
    
    /// <summary>
    /// Workflow class to convert a <see cref="DataTable"/> into an HTML table string representation.
    /// </summary>
    public class ConvertDataTableToHTML : CodedWorkflow
    {
        /// <summary>
        /// Executes the workflow to convert a DataTable into an HTML table string, using the provided formatting options.
        /// </summary>
        /// <param name="in_DataTable_Table">The <see cref="DataTable"/> to be converted into an HTML table.</param>
        /// <param name="in_options_DataTableToHTML">Formatting and serialization options for customizing the HTML output. If null, default formatting is used.</param>
        /// <returns>An HTML string representing the contents of the DataTable.</returns>
        [Workflow]
        public string Execute(DataTable in_DataTable_Table, DataTableToHTMLOptions in_options_DataTableToHTML)
        {
            if (in_options_DataTableToHTML == null) in_options_DataTableToHTML = new();

            string html = in_options_DataTableToHTML.TablePrefix.Invoke(in_DataTable_Table) + in_options_DataTableToHTML.HeaderRowPrefix.Invoke(in_DataTable_Table);

            foreach (DataColumn col in in_DataTable_Table.Columns)
            {
                html += in_options_DataTableToHTML.HeaderCellPrefix.Invoke(in_DataTable_Table, col) + col.ColumnName + in_options_DataTableToHTML.HeaderCellSuffix.Invoke(in_DataTable_Table, col);

                if (!in_options_DataTableToHTML.Serializers.ContainsKey(col.ColumnName))
                {
                    in_options_DataTableToHTML.Serializers.Add(col.ColumnName, (o) => o.ToString());
                }
            }

            html += in_options_DataTableToHTML.HeaderRowSuffix.Invoke(in_DataTable_Table);

            foreach (DataRow row in in_DataTable_Table.Rows)
            {
                html += in_options_DataTableToHTML.BodyRowPrefix.Invoke(in_DataTable_Table, row);

                foreach (DataColumn col in in_DataTable_Table.Columns)
                {
                    html += in_options_DataTableToHTML.BodyCellPrefix.Invoke(in_DataTable_Table, row, col)
                         + in_options_DataTableToHTML.Serializers[col.ColumnName].Invoke(row[col])
                         + in_options_DataTableToHTML.BodyCellSuffix.Invoke(in_DataTable_Table, row, col);
                }

                html += in_options_DataTableToHTML.BodyRowSuffix.Invoke(in_DataTable_Table, row);
            }

            html += in_options_DataTableToHTML.TableSuffix.Invoke(in_DataTable_Table);
            return html;
        }
    }

    /// <summary>
    /// Configuration options for customizing how a <see cref="DataTable"/> is converted to an HTML table.
    /// </summary>
    public class DataTableToHTMLOptions
    {
        /// <summary>
        /// Function to return the prefix for the HTML table element.
        /// Default: <c>"&lt;table&gt;"</c>
        /// </summary>
        public Func<DataTable, string> TablePrefix { get; set; } = (table) => "<table>";

        /// <summary>
        /// Function to return the suffix for the HTML table element.
        /// Default: <c>"&lt;/table&gt;"</c>
        /// </summary>
        public Func<DataTable, string> TableSuffix { get; set; } = (table) => "</table>";

        /// <summary>
        /// Function to return the prefix for the table's header row.
        /// Default: <c>"&lt;tr&gt;"</c>
        /// </summary>
        public Func<DataTable, string> HeaderRowPrefix { get; set; } = (table) => "<tr>";

        /// <summary>
        /// Function to return the suffix for the table's header row.
        /// Default: <c>"&lt;/tr&gt;"</c>
        /// </summary>
        public Func<DataTable, string> HeaderRowSuffix { get; set; } = (table) => "</tr>";

        /// <summary>
        /// Function to return the prefix for each header cell.
        /// Default: <c>"&lt;th&gt;"</c>
        /// </summary>
        public Func<DataTable, DataColumn, string> HeaderCellPrefix { get; set; } = (table, column) => "<th>";

        /// <summary>
        /// Function to return the suffix for each header cell.
        /// Default: <c>"&lt;/th&gt;"</c>
        /// </summary>
        public Func<DataTable, DataColumn, string> HeaderCellSuffix { get; set; } = (table, column) => "</th>";

        /// <summary>
        /// Function to return the prefix for each body row.
        /// Default: <c>"&lt;tr&gt;"</c>
        /// </summary>
        public Func<DataTable, DataRow, string> BodyRowPrefix { get; set; } = (table, row) => "<tr>";

        /// <summary>
        /// Function to return the suffix for each body row.
        /// Default: <c>"&lt;/tr&gt;"</c>
        /// </summary>
        public Func<DataTable, DataRow, string> BodyRowSuffix { get; set; } = (table, row) => "</tr>";

        /// <summary>
        /// Function to return the prefix for each body cell.
        /// Default: <c>"&lt;td&gt;"</c>
        /// </summary>
        public Func<DataTable, DataRow, DataColumn, string> BodyCellPrefix { get; set; } = (table, row, column) => "<td>";

        /// <summary>
        /// Function to return the suffix for each body cell.
        /// Default: <c>"&lt;/td&gt;"</c>
        /// </summary>
        public Func<DataTable, DataRow, DataColumn, string> BodyCellSuffix { get; set; } = (table, row, column) => "</td>";

        /// <summary>
        /// Dictionary containing custom serializers for specific columns.
        /// The key is the column name, and the value is a delegate used to convert the cell value to a string.
        /// If not provided, values are converted using <see cref="object.ToString()"/>.
        /// </summary>
        public Dictionary<string, Func<object, string>> Serializers { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableToHTMLOptions"/> class with default HTML tag formatting.
        /// </summary>
        public DataTableToHTMLOptions() { }
    }
    
}