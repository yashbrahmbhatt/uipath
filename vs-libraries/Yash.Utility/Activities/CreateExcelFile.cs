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
    public class CreateExcelFileException : Exception
    {
        public CreateExcelFileException(string message) : base("[CreateExcelFile] " + message) { }
        public CreateExcelFileException(string message, Exception innerException) : base("[CreateExcelFile] " + message, innerException) { }
        public CreateExcelFileException(string message, TraceEventType eventType) : base("[CreateExcelFile] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }

    [Category("Yash.Utility")]
    [DisplayName("Create Excel File")]
    [Description("Creates an Excel file from a DataSet or DataTable. Each DataTable becomes a separate worksheet in the Excel file.")]
    public class CreateExcelFile : CodeActivity
    {
        [Category("Input")]
        [DisplayName("Output Path")]
        [Description("Path where the Excel file will be saved (.xlsx)")]
        [RequiredArgument]
        public InArgument<string> OutputPath { get; set; }

        [Category("Input")]
        [DisplayName("Data Set")]
        [Description("DataSet containing DataTables to export as sheets (use this OR Data Table, not both)")]
        public InArgument<DataSet> DataSet { get; set; }

        [Category("Input")]
        [DisplayName("Data Table")]
        [Description("Single DataTable to export (use this OR Data Set, not both)")]
        public InArgument<DataTable> DataTable { get; set; }

        [Category("Input")]
        [DisplayName("Sheet Name")]
        [Description("Optional sheet name when using Data Table input. Defaults to DataTable.TableName or 'Sheet1'")]
        public InArgument<string> SheetName { get; set; }

        [Category("Options")]
        [DisplayName("Enable Logging")]
        [Description("Enable detailed logging of the creation operation")]
        public InArgument<bool> EnableLogging { get; set; } = true;

        [Category("Options")]
        [DisplayName("Overwrite Existing")]
        [Description("Overwrite existing file if it exists")]
        public InArgument<bool> OverwriteExisting { get; set; } = true;

        private IExecutorRuntime _runtime;

        protected override void Execute(CodeActivityContext context)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();

            var outputPath = OutputPath.Get(context);
            var dataSet = DataSet.Get(context);
            var dataTable = DataTable.Get(context);
            var sheetName = SheetName.Get(context);
            var enableLogging = EnableLogging.Get(context);
            var overwriteExisting = OverwriteExisting.Get(context);

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new CreateExcelFileException("Output path is required.");

            if (dataSet == null && dataTable == null)
                throw new CreateExcelFileException("Either DataSet or DataTable must be provided.");

            if (dataSet != null && dataTable != null)
                throw new CreateExcelFileException("Provide either DataSet OR DataTable, not both.");

            // Check if file exists and handle overwrite
            if (File.Exists(outputPath) && !overwriteExisting)
                throw new CreateExcelFileException($"File already exists and overwrite is disabled: {outputPath}");

            // Ensure directory exists
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Log($"Created directory: {directory}");
            }

            Log($"Creating Excel file: {outputPath}");

            try
            {
                Action<string, TraceEventType> logger = enableLogging ? Log : null;

                if (dataSet != null)
                {
                    ExcelHelpers.CreateExcelFile(outputPath, dataSet, logger);
                    Log($"Successfully created Excel file with {dataSet.Tables.Count} sheets");
                }
                else if (dataTable != null)
                {
                    ExcelHelpers.CreateExcelFile(outputPath, dataTable, sheetName, logger);
                    Log($"Successfully created Excel file with single sheet: {sheetName ?? dataTable.TableName ?? "Sheet1"}");
                }
            }
            catch (Exception ex)
            {
                Log($"Error creating Excel file: {ex.Message}", TraceEventType.Error);
                throw new CreateExcelFileException($"Failed to create Excel file: {ex.Message}", ex);
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
