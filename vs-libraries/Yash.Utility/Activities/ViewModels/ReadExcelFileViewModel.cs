using System;
using System.Activities.DesignViewModels;
using System.ComponentModel;
using System.Data;
using System.IO;
using UiPath.Studio.Activities.Api;

namespace Yash.Utility.Activities.ViewModels
{
    public class ReadExcelFileViewModel : DesignPropertiesViewModel
    {
        [Category("Input")]
        public DesignInArgument<string> FilePath { get; set; } = null!;

        [Category("Options")]
        public DesignInArgument<bool> EnableLogging { get; set; } = null!;

        [Category("Output")]
        public DesignOutArgument<DataSet> Result { get; set; } = null!;

        public DesignProperty<string> Info { get; set; } = null!;

        private IWorkflowDesignApi _api;

        public ReadExcelFileViewModel(IDesignServices services) : base(services)
        {
            _api = services.GetService<IWorkflowDesignApi>();
        }

        protected override void InitializeModel()
        {
            base.InitializeModel();

            var orderIndex = 0;

            // Info display
            Info.IsPrincipal = true;
            Info.OrderIndex = orderIndex++;
            Info.Value = "Reads Excel files (.xls/.xlsx) into DataSet with sheet data";
            Info.Widget = new TextBlockWidget()
            {
                Center = false,
                Level = "Info",
                Multiline = true
            };

            // File Path
            FilePath.DisplayName = "Excel File Path";
            FilePath.Tooltip = "Path to the Excel workbook file (.xls or .xlsx)";
            FilePath.IsRequired = true;
            FilePath.IsPrincipal = true;
            FilePath.OrderIndex = orderIndex++;
            FilePath.Widget = new DefaultWidget() { Type = "FilePicker" };

            // Enable Logging
            EnableLogging.DisplayName = "Enable Logging";
            EnableLogging.Tooltip = "Enable detailed logging of the read operation";
            EnableLogging.IsPrincipal = true;
            EnableLogging.OrderIndex = orderIndex++;

            // Result
            Result.DisplayName = "DataSet Result";
            Result.Tooltip = "DataSet containing all sheets as DataTables";
            Result.IsPrincipal = true;
            Result.OrderIndex = orderIndex++;
        }

        protected override void ManualRegisterDependencies()
        {
            base.ManualRegisterDependencies();
            RegisterDependency(FilePath, "Value", "OnFilePathUpdated");
        }

        protected override void InitializeRules()
        {
            base.InitializeRules();
            Rule("OnFilePathUpdated", OnFilePathUpdated);
        }

        public void OnFilePathUpdated()
        {
            if (TryGetValidPath(out var path))
            {
                try
                {
                    var fileInfo = new FileInfo(path);
                    Info.Value = $"Ready to read: {fileInfo.Name} ({fileInfo.Length:N0} bytes)";
                    ((TextBlockWidget)Info.Widget).Level = "Info";
                }
                catch (Exception ex)
                {
                    Info.Value = $"Error accessing file: {ex.Message}";
                    ((TextBlockWidget)Info.Widget).Level = "Error";
                }
            }
            else
            {
                Info.Value = "Please provide a valid Excel file path (.xls or .xlsx)";
                ((TextBlockWidget)Info.Widget).Level = "Warning";
            }
        }

        private bool TryGetValidPath(out string path)
        {
            path = "";
            if (FilePath.Value != null && FilePath.Value.Expression.IsLiteral())
            {
                path = FilePath.Value.Expression.ToString()?.Trim('"') ?? "";
                path = path.Contains(":") ? path : Path.Combine(_api?.ProjectPropertiesService.GetProjectDirectory() ?? "", path);
                return (path.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) || 
                       path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)) && 
                       File.Exists(path);
            }
            return false;
        }
    }
}
