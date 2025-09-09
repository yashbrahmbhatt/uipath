using System;
using System.Activities;
using System.Data;
using System.IO;
using System.ComponentModel;
using TraceEventType = System.Diagnostics.TraceEventType;
using UiPath.Robot.Activities.Api;
using Yash.Utility.Helpers;

namespace Yash.Utility.Activities
{
    public class ReadExcelFileException : Exception
    {
        public ReadExcelFileException(string message) : base("[ReadExcelFile] " + message) { }
        public ReadExcelFileException(string message, Exception innerException) : base("[ReadExcelFile] " + message, innerException) { }
        public ReadExcelFileException(string message, TraceEventType eventType) : base("[ReadExcelFile] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }

    [Category("Yash.Utility")]
    [DisplayName("Read Excel File")]
    [Description("Reads an Excel file and converts its sheets into a DataSet. Uses EPPlus which can read files even when they are open in Excel.")]
    public class ReadExcelFile : CodeActivity<DataSet>
    {
        [Category("Input")]
        [DisplayName("File Path")]
        [Description("Path to the Excel workbook file (.xls or .xlsx)")]
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        [Category("Options")]
        [DisplayName("Enable Logging")]
        [Description("Enable detailed logging of the read operation")]
        public InArgument<bool> EnableLogging { get; set; } = true;

        [Category("Output")]
        [DisplayName("Result")]
        [Description("DataSet containing all sheets as DataTables")]
        public OutArgument<DataSet> Result { get; set; }

        private IExecutorRuntime _runtime;

        protected override DataSet Execute(CodeActivityContext context)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();

            var filePath = FilePath.Get(context);
            var enableLogging = EnableLogging.Get(context);

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ReadExcelFileException("File path is required.");

            if (!File.Exists(filePath))
                throw new ReadExcelFileException($"File not found: {filePath}");

            Log($"Reading Excel file: {filePath}");

            try
            {
                Action<string, TraceEventType> logger = enableLogging ? Log : null;
                var result = ExcelHelpers.ReadExcelFile(filePath, logger);
                
                Log($"Successfully read {result.Tables.Count} sheets from Excel file");
                
                return result;
            }
            catch (Exception ex)
            {
                Log($"Error reading Excel file: {ex.Message}", TraceEventType.Error);
                throw new ReadExcelFileException($"Failed to read Excel file: {ex.Message}", ex);
            }
        }

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            _runtime?.LogMessage(new LogMessage
            {
                EventType = level,
                Message = msg
            });
        }
    }
}
