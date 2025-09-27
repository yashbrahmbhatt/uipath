using System;
using System.Collections.Generic;

namespace Yash.Utility.Services.Email.Models
{
    /// <summary>
    /// Defines options for converting a DataTable to an HTML table.
    /// </summary>
    public class DataTableToHTMLOptions
    {
        /// <summary>Prefix for the HTML table.</summary>
        public string TablePrefix { get; set; } = "<table>";

        /// <summary>Suffix for the HTML table.</summary>
        public string TableSuffix { get; set; } = "</table>";

        /// <summary>Prefix for the header row.</summary>
        public string HeaderRowPrefix { get; set; } = "<tr>";

        /// <summary>Suffix for the header row.</summary>
        public string HeaderRowSuffix { get; set; } = "</tr>";

        /// <summary>Prefix for a header cell.</summary>
        public string HeaderCellPrefix { get; set; } = "<th>";

        /// <summary>Suffix for a header cell.</summary>
        public string HeaderCellSuffix { get; set; } = "</th>";

        /// <summary>Prefix for a body row.</summary>
        public string BodyRowPrefix { get; set; } = "<tr>";

        /// <summary>Suffix for a body row.</summary>
        public string BodyRowSuffix { get; set; } = "</tr>";

        /// <summary>Prefix for a body cell.</summary>
        public string BodyCellPrefix { get; set; } = "<td>";

        /// <summary>Suffix for a body cell.</summary>
        public string BodyCellSuffix { get; set; } = "</td>";

        /// <summary>CSS classes to be applied to the table.</summary>
        public string CSSClasses { get; set; } = "";

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