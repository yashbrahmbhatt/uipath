using System.Collections.Generic;

namespace Yash.Utility.Services.Excel.Models
{
    /// <summary>
    /// Information about Excel sheet structure
    /// </summary>
    public class ExcelSheetInfo
    {
        /// <summary>
        /// Name of the sheet
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of rows with data
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Number of columns with data
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Column names/headers
        /// </summary>
        public List<string> ColumnNames { get; set; } = new();

        /// <summary>
        /// Whether the sheet has a header row
        /// </summary>
        public bool HasHeaders { get; set; } = true;

        /// <summary>
        /// Range of data in Excel notation (e.g., "A1:D100")
        /// </summary>
        public string? DataRange { get; set; }
    }
}