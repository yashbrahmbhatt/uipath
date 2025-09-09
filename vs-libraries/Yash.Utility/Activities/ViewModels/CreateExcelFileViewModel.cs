using System;
using System.Activities.DesignViewModels;
using System.Activities.ViewModels.Interfaces;
using System.ComponentModel;
using System.Data;
using System.IO;
using UiPath.Studio.Activities.Api;

namespace Yash.Utility.Activities.ViewModels
{
    public class CreateExcelFileViewModel : DesignPropertiesViewModel 
    {
        [Category("Input")]
        public DesignInArgument<string> OutputPath { get; set; } = null!;

        [Category("Input")]
        public DesignInArgument<DataSet> DataSet { get; set; } = null!;

        [Category("Input")]
        public DesignInArgument<DataTable> DataTable { get; set; } = null!;

        [Category("Input")]
        public DesignInArgument<string> SheetName { get; set; } = null!;

        [Category("Options")]
        public DesignInArgument<bool> EnableLogging { get; set; } = null!;

        [Category("Options")]
        public DesignInArgument<bool> OverwriteExisting { get; set; } = null!;

        public DesignProperty<string> Info { get; set; } = null!;

        private IWorkflowDesignApi _workflowDesignApi;
        private bool _typeWidgetAvailable;
        private bool _jitServiceAvailable;
        private readonly IBusyService _busyService;
        private IDesignerCustomTypesService _designerCustomTypesService;
        private IDesignerStaticTypesService _morphingService;

        public CreateExcelFileViewModel(IDesignServices services) : base(services)
        {
            _workflowDesignApi = services.GetService<IWorkflowDesignApi>();
            _busyService = services.GetService<IBusyService>();
        }

        protected override void InitializeModel()
        {
            base.InitializeModel();

            var orderIndex = 0;

            // Info display
            Info.IsPrincipal = true;
            Info.OrderIndex = orderIndex++;
            Info.Value = "Creates Excel files from DataSet or DataTable";
            Info.Widget = new TextBlockWidget()
            {
                Center = false,
                Level = "Info",
                Multiline = true
            };

            // Output Path
            OutputPath.DisplayName = "Output File Path";
            OutputPath.Tooltip = "Path where the Excel file will be saved (.xlsx)";
            OutputPath.IsRequired = true;
            OutputPath.IsPrincipal = true;
            OutputPath.OrderIndex = orderIndex++;
            OutputPath.Widget = new DefaultWidget() { Type = "FilePicker" };

            // DataSet
            DataSet.DisplayName = "Data Set";
            DataSet.Tooltip = "DataSet containing DataTables to export as sheets (use this OR Data Table)";
            DataSet.IsPrincipal = true;
            DataSet.OrderIndex = orderIndex++;

            // DataTable
            DataTable.DisplayName = "Data Table";
            DataTable.Tooltip = "Single DataTable to export (use this OR Data Set)";
            DataTable.IsPrincipal = true;
            DataTable.OrderIndex = orderIndex++;

            // Sheet Name
            SheetName.DisplayName = "Sheet Name";
            SheetName.Tooltip = "Optional sheet name when using Data Table input";
            SheetName.IsPrincipal = true;
            SheetName.OrderIndex = orderIndex++;

            // Enable Logging
            EnableLogging.DisplayName = "Enable Logging";
            EnableLogging.Tooltip = "Enable detailed logging of the creation operation";
            EnableLogging.IsPrincipal = true;
            EnableLogging.OrderIndex = orderIndex++;

            // Overwrite Existing
            OverwriteExisting.DisplayName = "Overwrite Existing";
            OverwriteExisting.Tooltip = "Overwrite existing file if it exists";
            OverwriteExisting.IsPrincipal = true;
            OverwriteExisting.OrderIndex = orderIndex++;
        }

        private void InitCustomTypeService()
        {
            if (_busyService == null || _morphingService == null)
                return;
            _designerCustomTypesService = Services.GetService<IDesignerCustomTypesService>();
            if (_designerCustomTypesService == null)
                return;
            _jitServiceAvailable = true;
        }

        protected override void ManualRegisterDependencies()
        {
            base.ManualRegisterDependencies();
            RegisterDependency(OutputPath, "Value", "OnOutputPathUpdated");
            RegisterDependency(DataSet, "Value", "OnDataInputUpdated");
            RegisterDependency(DataTable, "Value", "OnDataInputUpdated");
        }

        protected override void InitializeRules()
        {
            base.InitializeRules();
            Rule("OnOutputPathUpdated", OnOutputPathUpdated);
            Rule("OnDataInputUpdated", OnDataInputUpdated);
        }

        public void OnOutputPathUpdated()
        {
            if (TryGetValidOutputPath(out var path))
            {
                var directory = Path.GetDirectoryName(path);
                var fileName = Path.GetFileName(path);
                
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Info.Value = $"Will create: {fileName} in {directory}";
                    ((TextBlockWidget)Info.Widget).Level = "Info";
                }
                else
                {
                    Info.Value = $"Directory will be created: {directory}";
                    ((TextBlockWidget)Info.Widget).Level = "Warning";
                }
            }
            else
            {
                Info.Value = "Please provide a valid output path ending with .xlsx";
                ((TextBlockWidget)Info.Widget).Level = "Warning";
            }
        }

        public void OnDataInputUpdated()
        {
            var hasDataSet = DataSet.Value != null;
            var hasDataTable = DataTable.Value != null;

            if (hasDataSet && hasDataTable)
            {
                Info.Value = "Warning: Provide either DataSet OR DataTable, not both";
                ((TextBlockWidget)Info.Widget).Level = "Error";
            }
            else if (!hasDataSet && !hasDataTable)
            {
                Info.Value = "Please provide either a DataSet or DataTable as input";
                ((TextBlockWidget)Info.Widget).Level = "Warning";
            }
            else if (hasDataSet)
            {
                Info.Value = "Using DataSet input - each table will become a sheet";
                ((TextBlockWidget)Info.Widget).Level = "Info";
            }
            else if (hasDataTable)
            {
                Info.Value = "Using DataTable input - will create single sheet";
                ((TextBlockWidget)Info.Widget).Level = "Info";
            }
        }

        private bool TryGetValidOutputPath(out string path)
        {
            path = "";
            if (OutputPath.Value != null && OutputPath.Value.Expression.IsLiteral())
            {
                path = OutputPath.Value.Expression.ToString()?.Trim('"') ?? "";
                path = path.Contains(":") ? path : Path.Combine(_workflowDesignApi?.ProjectPropertiesService.GetProjectDirectory() ?? "", path);
                return path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
