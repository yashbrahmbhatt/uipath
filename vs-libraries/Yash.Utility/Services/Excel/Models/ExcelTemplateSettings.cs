namespace Yash.Utility.Services.Excel.Models
{
    /// <summary>
    /// Settings for Excel template generation
    /// </summary>
    public class ExcelTemplateSettings
    {
        /// <summary>
        /// Supported data types for dropdowns
        /// </summary>
        public string[] SupportedTypes { get; set; } = {
            "string", "int", "bool", "double", "DateTime", "TimeSpan",
            "List<string>", "Dictionary<string,object>", "object"
        };

        /// <summary>
        /// Maximum number of rows to apply validation to
        /// </summary>
        public int MaxValidationRows { get; set; } = 100;

        /// <summary>
        /// Include instructions sheet in templates
        /// </summary>
        public bool IncludeInstructions { get; set; } = true;

        /// <summary>
        /// Sheet names to create in templates
        /// </summary>
        public string[] DefaultSheetNames { get; set; } = { "Settings", "Assets", "Files" };
    }
}