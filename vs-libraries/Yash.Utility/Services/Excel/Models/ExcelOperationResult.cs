using System;
using System.Collections.Generic;

namespace Yash.Utility.Services.Excel.Models
{
    /// <summary>
    /// Result of an Excel operation
    /// </summary>
    public class ExcelOperationResult
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Number of sheets processed
        /// </summary>
        public int SheetsProcessed { get; set; }

        /// <summary>
        /// Total number of rows processed across all sheets
        /// </summary>
        public int TotalRowsProcessed { get; set; }

        /// <summary>
        /// Path to the processed file
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Processing duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Additional metadata about the operation
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}