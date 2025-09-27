using System.Collections.Generic;

namespace Yash.Utility.Services.Excel.Models
{
    /// <summary>
    /// Configuration options for Excel operations
    /// </summary>
    public class ExcelConfiguration
    {
        /// <summary>
        /// Whether to auto-fit columns after writing data
        /// </summary>
        public bool AutoFitColumns { get; set; } = true;

        /// <summary>
        /// Whether to format header row with bold text
        /// </summary>
        public bool FormatHeaders { get; set; } = true;

        /// <summary>
        /// Maximum number of retry attempts for locked files
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 5;

        /// <summary>
        /// Base delay in milliseconds between retry attempts
        /// </summary>
        public int RetryDelayMs { get; set; } = 500;

        /// <summary>
        /// Default date format for Excel cells
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-mm-dd hh:mm:ss";

        /// <summary>
        /// Default time format for Excel cells
        /// </summary>
        public string TimeFormat { get; set; } = "[h]:mm:ss";

        /// <summary>
        /// Whether to include empty rows when reading
        /// </summary>
        public bool IncludeEmptyRows { get; set; } = false;

        /// <summary>
        /// Custom column mappings for reading Excel files
        /// Key: Excel column name, Value: Property name
        /// </summary>
        public Dictionary<string, string> ColumnMappings { get; set; } = new();

        /// <summary>
        /// Template settings for creating configuration files
        /// </summary>
        public ExcelTemplateSettings TemplateSettings { get; set; } = new();
    }
}