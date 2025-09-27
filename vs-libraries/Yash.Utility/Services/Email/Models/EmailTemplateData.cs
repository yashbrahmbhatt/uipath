using System;
using System.Collections.Generic;
using System.Data;

namespace Yash.Utility.Services.Email.Models
{
    /// <summary>
    /// Data container for email template processing
    /// </summary>
    public class EmailTemplateData
    {
        /// <summary>
        /// Process name for email headers
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// Transaction reference or identifier
        /// </summary>
        public string Reference { get; set; } = string.Empty;

        /// <summary>
        /// Environment machine name
        /// </summary>
        public string MachineName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Environment user name
        /// </summary>
        public string UserName { get; set; } = Environment.UserName;

        /// <summary>
        /// Current date formatted as long date string
        /// </summary>
        public string Date { get; set; } = DateTime.Now.ToLongDateString();

        /// <summary>
        /// Current time formatted as long time string
        /// </summary>
        public string Time { get; set; } = DateTime.Now.ToLongTimeString();

        /// <summary>
        /// Today's date in dd-MM-yyyy format
        /// </summary>
        public string Today { get; set; } = DateTime.Now.ToString("dd-MM-yyyy");

        /// <summary>
        /// Exception message if applicable
        /// </summary>
        public string? ExceptionMessage { get; set; }

        /// <summary>
        /// Exception source if applicable
        /// </summary>
        public string? ExceptionSource { get; set; }

        /// <summary>
        /// Exception stack trace if applicable
        /// </summary>
        public string? ExceptionStackTrace { get; set; }

        /// <summary>
        /// Exception type (BusinessRuleException, System, Unknown)
        /// </summary>
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Exception data serialized as JSON
        /// </summary>
        public string? ExceptionData { get; set; }

        /// <summary>
        /// Transaction specific data serialized as JSON
        /// </summary>
        public string? TransactionSpecificData { get; set; }

        /// <summary>
        /// Summary statistics for reports
        /// </summary>
        public ReportStatistics? Statistics { get; set; }

        /// <summary>
        /// Summary table data for reports
        /// </summary>
        public DataTable? SummaryTable { get; set; }

        /// <summary>
        /// Additional custom data for template replacement
        /// </summary>
        public Dictionary<string, object> CustomData { get; set; } = new();

        /// <summary>
        /// Converts this instance to a dictionary for template replacement
        /// </summary>
        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>
            {
                ["ProcessName"] = ProcessName,
                ["Reference"] = Reference,
                ["Env_MachineName"] = MachineName,
                ["Env_User"] = UserName,
                ["Env_Date"] = Date,
                ["Env_Time"] = Time,
                ["Today"] = Today
            };

            if (!string.IsNullOrEmpty(ExceptionMessage))
                dict["Ex_Message"] = ExceptionMessage;
            if (!string.IsNullOrEmpty(ExceptionSource))
                dict["Ex_Source"] = ExceptionSource;
            if (!string.IsNullOrEmpty(ExceptionStackTrace))
                dict["Ex_Stack"] = ExceptionStackTrace;
            if (!string.IsNullOrEmpty(ExceptionType))
                dict["Ex_Type"] = ExceptionType;
            if (!string.IsNullOrEmpty(ExceptionData))
                dict["Ex_Data"] = ExceptionData;
            if (!string.IsNullOrEmpty(TransactionSpecificData))
                dict["Transaction_SpecificData"] = TransactionSpecificData;

            if (Statistics != null)
            {
                dict["TotalCount"] = Statistics.TotalCount;
                dict["SuccessCount"] = Statistics.SuccessCount;
                dict["FailedCount"] = Statistics.FailedCount;
            }

            if (SummaryTable != null)
                dict["SummaryTable"] = SummaryTable;

            // Add custom data
            foreach (var kvp in CustomData)
            {
                dict[kvp.Key] = kvp.Value;
            }

            return dict;
        }
    }
}