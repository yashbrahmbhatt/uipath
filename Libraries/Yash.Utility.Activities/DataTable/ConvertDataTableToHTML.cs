using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using DT = System.Data.DataTable;

namespace Yash.Utility.Activities.DataTable
{
    /// <summary>
    /// Converts a DataTable to an HTML table representation.
    /// </summary>
    public class DataTableToHTML : CodedWorkflow
    {
        /// <summary>
        /// Converts the given DataTable into an HTML string based on the provided formatting options.
        /// </summary>
        /// <param name="in_dt_Table">The DataTable to be converted.</param>
        /// <param name="in_options_DataTableToHTML">Formatting options for generating the HTML table.</param>
        /// <returns>A string representing the HTML table.</returns>
        [Workflow]
        public string Execute(DT in_dt_Table, DataTableToHTMLOptions in_options_DataTableToHTML)
        {
            if (in_options_DataTableToHTML == null) in_options_DataTableToHTML = new();

            string html = in_options_DataTableToHTML.TablePrefix.Invoke(in_dt_Table) + in_options_DataTableToHTML.HeaderRowPrefix.Invoke(in_dt_Table);

            foreach (DataColumn col in in_dt_Table.Columns)
            {
                html += in_options_DataTableToHTML.HeaderCellPrefix.Invoke(in_dt_Table, col) + col.ColumnName + in_options_DataTableToHTML.HeaderCellSuffix.Invoke(in_dt_Table, col);

                if (!in_options_DataTableToHTML.Serializers.ContainsKey(col.ColumnName))
                {
                    in_options_DataTableToHTML.Serializers.Add(col.ColumnName, (o) => o.ToString());
                }
            }

            html += in_options_DataTableToHTML.HeaderRowSuffix.Invoke(in_dt_Table);

            foreach (DataRow row in in_dt_Table.Rows)
            {
                html += in_options_DataTableToHTML.BodyRowPrefix.Invoke(in_dt_Table, row);

                foreach (DataColumn col in in_dt_Table.Columns)
                {
                    html += in_options_DataTableToHTML.BodyCellPrefix.Invoke(in_dt_Table, row, col) + in_options_DataTableToHTML.Serializers[col.ColumnName].Invoke(row[col]) + in_options_DataTableToHTML.BodyCellSuffix.Invoke(in_dt_Table, row, col);
                }

                html += in_options_DataTableToHTML.BodyRowSuffix.Invoke(in_dt_Table, row);
            }

            html += in_options_DataTableToHTML.TableSuffix.Invoke(in_dt_Table);
            return html;
        }
    }

    /// <summary>
    /// Defines options for converting a DataTable to an HTML table.
    /// </summary>
    public class DataTableToHTMLOptions
    {
        /// <summary>Prefix for the HTML table.</summary>
        public Func<DT, string> TablePrefix { get; set; } = (table) => "<table>";

        /// <summary>Suffix for the HTML table.</summary>
        public Func<DT, string> TableSuffix { get; set; } = (table) => "</table>";

        /// <summary>Prefix for the header row.</summary>
        public Func<DT, string> HeaderRowPrefix { get; set; } = (table) => "<tr>";

        /// <summary>Suffix for the header row.</summary>
        public Func<DT, string> HeaderRowSuffix { get; set; } = (table) => "</tr>";

        /// <summary>Prefix for a header cell.</summary>
        public Func<DT, DataColumn, string> HeaderCellPrefix { get; set; } = (table, column) => "<th>";

        /// <summary>Suffix for a header cell.</summary>
        public Func<DT, DataColumn, string> HeaderCellSuffix { get; set; } = (table, column) => "</th>";

        /// <summary>Prefix for a body row.</summary>
        public Func<DT, DataRow, string> BodyRowPrefix { get; set; } = (table, row) => "<tr>";

        /// <summary>Suffix for a body row.</summary>
        public Func<DT, DataRow, string> BodyRowSuffix { get; set; } = (table, row) => "</tr>";

        /// <summary>Prefix for a body cell.</summary>
        public Func<DT, DataRow, DataColumn, string> BodyCellPrefix { get; set; } = (table, row, column) => "<td>";

        /// <summary>Suffix for a body cell.</summary>
        public Func<DT, DataRow, DataColumn, string> BodyCellSuffix { get; set; } = (table, row, column) => "</td>";

        /// <summary>
        /// Dictionary of serializers for custom object-to-string conversion.
        /// The key is the column name, and the value is a function that converts objects to strings.
        /// </summary>
        public Dictionary<string, Func<object, string>> Serializers { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTableToHTMLOptions"/> class with default values.
        /// </summary>
        public DataTableToHTMLOptions() { }
    }
}