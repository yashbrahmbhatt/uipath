using CsvHelper;
using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Activities.Statements;
using System.Activities.ViewModels;
using System.Activities.ViewModels.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UiPath.Platform.ResourceHandling;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.BusyService;
using Yash.Config.Helpers;
using Yash.Config.Models;
using static Yash.Config.ConfigService;

namespace Yash.Config.Activities.ViewModels
{
    public class LoadConfigViewModel<T> : DesignPropertiesViewModel
    {
        public static class MenuActionIds
        {
            public const string AddDesignTimePath = "AddDesignTimePath";
            public const string RefreshScopes = "RefreshScopes";
            public const string OpenFile = "OpenFile";
            public const string GenerateClasses = "GenerateClasses";
            public const string RefreshFiles = "RefreshFiles";
        }
        [Category("Input")]
        public DesignInArgument<string> WorkbookPath { get; set; }
        [Category("Design")]
        [NotMappedProperty]
        public DesignProperty<string> DesignTimePath { get; set; }
        private BindingList<string> AvailableDesignTimePaths = [];

        public DesignProperty<bool> DebugMode { get; set; }

        [Category("Input")]
        public DesignProperty<Type> Scope { get; set; }
        private BindingList<Type> AvailableScopes = [];

        //[Category("Authentication")]
        //public DesignInArgument<string> BaseUrl { get; set; }
        //[Category("Authentication")]
        //public DesignInArgument<string> ClientId { get; set; }
        //[Category("Authentication")]
        //public DesignInArgument<SecureString> ClientSecret { get; set; }


        [Category("Output")]
        public DesignOutArgument<T> Result { get; set; }


        public DesignProperty<string> Debug { get; set; }
        private IWorkflowDesignApi? _workflowDesignApi;
        private readonly IBusyService _busyService;
        private IDesignerCustomTypesService _designerCustomTypesService;
        private IDesignerStaticTypesService _morphingService;
        private bool _typeWidgetAvailable;
        private bool _jitServiceAvailable;
        private ConfigFileMetadata? _meta;

        public LoadConfigViewModel(IDesignServices services) : base(services)
        {
            _workflowDesignApi = services.GetService<IWorkflowDesignApi>();

            _busyService = services.GetService<IBusyService>();
        }

        #region Main Activity Stuff
        protected override void InitializeModel()
        {
            base.InitializeModel();
            _morphingService = Services.GetService<IDesignerStaticTypesService>();
            RefreshScopes();
            try
            {

                RefreshFiles();
            }
            catch { }

            var orderIndex = 0;

            Debug.IsPrincipal = true;
            Debug.OrderIndex = orderIndex++;
            Debug.Value = "Add a design time path to enable design time features, otherwise the output will be a Dictionary<string, object>";
            Debug.Widget = new TextBlockWidget()
            {
                Center = false,
                Level = "Info",
                Multiline = true
            };

            WorkbookPath.DisplayName = Resources.LoadConfig_WorkflowPath_DisplayName;
            WorkbookPath.Tooltip = Resources.LoadConfig_WorkflowPath_Tooltip;
            WorkbookPath.IsRequired = true;
            WorkbookPath.IsPrincipal = true;
            WorkbookPath.OrderIndex = orderIndex++;
            WorkbookPath.Category = "Input";
            WorkbookPath.AddMenuAction(CreateMenuAction(MenuActionIds.AddDesignTimePath));


            DesignTimePath.DisplayName = "Design Time Path";
            DesignTimePath.Tooltip = "Path to the Excel file or folder containing config files used at design time for type generation and validation";
            DesignTimePath.IsPrincipal = false;
            DesignTimePath.IsVisible = false;
            DesignTimePath.OrderIndex = orderIndex++;
            DesignTimePath.IsRequired = false;
            DesignTimePath.Category = "Design";
 
            DesignTimePath.Widget = new DefaultWidget()
            {
                Type = "Dropdown",
            };
            DesignTimePath.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshFiles));


            Scope.DisplayName = Resources.LoadConfig_Scope_DisplayName;
            Scope.Tooltip = Resources.LoadConfig_Scope_Tooltip;
            Scope.IsPrincipal = DesignTimePath.HasValue;
            Scope.OrderIndex = orderIndex++;
            Scope.IsRequired = false;
            Scope.IsVisible = false;
            Scope.Category = "Input";
            Scope.Value = typeof(Dictionary<string, object>);
            Scope.Widget = new TypePickerWidget()
            {
                Type = "Dropdown",
                RecommendedTypes = AvailableScopes.ToList(),
                Filter = (t) => ConfigService.IsValidConfigType(t),
            };
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshScopes));

            DebugMode.DisplayName = "Debug Mode";
            DebugMode.Tooltip = "Enables verbose logging to the debug output";
            DebugMode.IsPrincipal = false;
            DebugMode.OrderIndex = orderIndex++;
            DebugMode.IsRequired = false;
            DebugMode.IsVisible = true;
            DebugMode.Category = "Options";
            DebugMode.Value = false;
            DebugMode.Widget = new DefaultWidget()
            {
                Type = "Toggle",
            };

            Result.DisplayName = Resources.LoadConfig_Result_DisplayName;
            Result.Tooltip = Resources.LoadConfig_Result_Tooltip;
            Result.IsPrincipal = true;
            Result.IsVisible = true;
            Result.OrderIndex = orderIndex++;
            Result.Category = "Output";


        }

        protected override void ManualRegisterDependencies()
        {
            base.ManualRegisterDependencies();
            RegisterDependency(DesignTimePath, "Value", "OnDesignTimePathUpdated");
            RegisterDependency(WorkbookPath, "Value", "OnWorkflowPathUpdated");
            RegisterDependency(Scope, "Value", "OnScopeUpdated");
        }

        protected override void InitializeRules()
        {
            base.InitializeRules();
            Rule("OnDesignTimePathUpdated", OnDesignTimePathUpdated, false);
            Rule("OnWorkflowPathUpdated", OnWorkflowPathUpdated, false);
            Rule("OnScopeUpdated", OnScopeUpdated, false);
        }

        private void RemoveMenuAction(DesignProperty prop, string actionId)
        {
            prop.RemoveMenuAction(actionId);
        }

        private async void MorphActivityAsync()
        {
            if (!(Scope.HasValue //&& ConfigSheetType.Value.IsAssignableFrom(typeof(Models.Config))
                )) return;
            await _morphingService.UseTypeAsync(Scope.Value);
        }

        #endregion

        #region Menu Actions
        private MenuAction CreateMenuAction(string actionId)
        {
            switch (actionId)
            {
                case MenuActionIds.AddDesignTimePath:
                    return new MenuAction()
                    {
                        Id = MenuActionIds.AddDesignTimePath,
                        DisplayName = "Add Design Time Path",
                        IsMain = true,
                        IsEnabled = DesignTimePath.HasValue ? DesignTimePath.Value == null : true,
                        IsVisible = true,
                        Handler = AddDesignTimePathMenuAction
                    };

                case MenuActionIds.GenerateClasses:
                    return new MenuAction()
                    {
                        Id = MenuActionIds.GenerateClasses,
                        DisplayName = "Generate Classes",
                        IsMain = false,
                        IsEnabled = false,
                        IsVisible = false,
                        Handler = GenerateClassesMenuAction
                    };

                case MenuActionIds.OpenFile:
                    return new MenuAction()
                    {
                        Id = MenuActionIds.OpenFile,
                        DisplayName = "Open File",
                        IsMain = true,
                        IsEnabled = true,
                        IsVisible = true,
                        Handler = OpenFileMenuAction
                    };

                case MenuActionIds.RefreshScopes:
                    return new MenuAction()
                    {
                        Id = MenuActionIds.RefreshScopes,
                        DisplayName = "Refresh Scopes",
                        IsMain = true,
                        IsEnabled = true,
                        IsVisible = true,
                        Handler = RefreshScopesMenuAction
                    };
                case MenuActionIds.RefreshFiles:
                    return new MenuAction()
                    {
                        Id = MenuActionIds.RefreshFiles,
                        DisplayName = "Refresh Files",
                        IsMain = true,
                        IsEnabled = true,
                        IsVisible = true,
                        Handler = RefreshFilesMenuAction
                    };
                default:
                    throw new ArgumentException($"Unknown menu action ID: {actionId}");
            }
        }
        public async Task RefreshFilesMenuAction(MenuAction action)
        {
            RefreshFiles();
        }

        public async Task RefreshScopesMenuAction(MenuAction action)
        {
            RefreshScopes();

        }
        public async Task AddDesignTimePathMenuAction(MenuAction action)
        {
            try
            {

                DesignTimePath.IsPrincipal = true;
                DesignTimePath.IsVisible = true;
                RefreshFiles();
                if (WorkbookPath.GetMenuActions().Any(ma => ma.Id == MenuActionIds.AddDesignTimePath))
                    WorkbookPath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
                Debug.Value = "Please select a design time path to enable enhanced features";
                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error adding design time path: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }


        public async Task OpenFileMenuAction(MenuAction action)
        {
            if (_meta == null) return;
            var pathToOpen = _meta.FilePath;


            try
            {
                if (Directory.Exists(pathToOpen))
                {
                    // Open folder in File Explorer
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{pathToOpen}\"",
                        UseShellExecute = true
                    });
                    Debug.Value = "Folder opened successfully in File Explorer";
                }
                else if (File.Exists(pathToOpen))
                {
                    // Open file with default application
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pathToOpen,
                        UseShellExecute = true
                    });
                    Debug.Value = "File opened successfully";
                }
                else
                {
                    Debug.Value = "Path does not exist";
                    ((TextBlockWidget)Debug.Widget).Level = "Error";
                    return;
                }

                ((TextBlockWidget)Debug.Widget).Level = "Success";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error opening path: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }
        public async Task GenerateClassesMenuAction(MenuAction action)
        {
            if (_meta == null) return;
            try
            {
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_OutputDir_Key, out var outputDir);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Namespace_Key, out var ns);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Usings_Key, out var usings);

                var result = ConfigService.GenerateClassFiles(_meta.FilePath, outputDir, ns, usings);

                Debug.Value = "Classes generated successfully";
                ((TextBlockWidget)Debug.Widget).Level = "Success";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error generating classes: " + ex.Message.ToString();
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }
        public void RefreshScopes()
        {
            if (_meta == null) return;

            try
            {
                AvailableScopes.Clear();
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetExportedTypes())
                    .Where(t => IsValidConfigType(t))
                    .ToList();
                foreach (var type in types)
                    AvailableScopes.Add(type);
                Scope.DataSource = DataSourceBuilder<Type>.WithId((s) => s.GUID.ToString()).WithLabel((s) => s.Name.ToString()).WithData(AvailableScopes).Build();
                Debug.Value = $"Found {AvailableScopes.Count} configuration types in loaded assemblies for scope.";
                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error refreshing scopes: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }

        public void RefreshFiles()
        {
            try
            {

                AvailableDesignTimePaths.Clear();
                var projectDir = _workflowDesignApi?.ProjectPropertiesService?.GetProjectDirectory() ?? "";
                if (Directory.Exists(projectDir))
                {
                    var files = Directory.GetFiles(projectDir, "*.*", SearchOption.AllDirectories)
                        .Where(f => new string[] { ".xlsx", ".xls" }
                            .Contains(Path.GetExtension(f).ToLowerInvariant()))
                        .Select(f => new FileInfo(f))
                        .Where(f => f != null)
                        .ToList();
                    foreach (var file in files)
                        if (file.Exists)
                            AvailableDesignTimePaths.Add(file.FullName);
                    DesignTimePath.DataSource = DataSourceBuilder<string>.WithId(f => f)
                        .WithLabel(f => Path.GetRelativePath(_workflowDesignApi.ProjectPropertiesService.GetProjectDirectory(), f))
                        .WithData(AvailableDesignTimePaths)
                        .Build();
                }
                Debug.Value = $"Found {AvailableDesignTimePaths.Count} Excel files in project directory for design time use.";
                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error refreshing files: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }
        #endregion

        #region Property Changed Handlers
        public void OnWorkflowPathUpdated()
        {
            try
            {
                _meta = ConfigService.ValidateConfigFile(WorkbookPath?.Value?.Expression?.ToString() ?? "");
                if (_meta.ConfigFileError != null)
                {
                    Debug.Value = "Click the menu action to add a design time path for enhanced features";
                    ((TextBlockWidget)Debug.Widget).Level = "Info";
                    return;
                }
                DesignTimePath.IsPrincipal = false;
                DesignTimePath.Value = _meta.FilePath;
                DesignTimePath.IsVisible = true;
                Scope.IsVisible = true;
                Scope.IsPrincipal = true;
                WorkbookPath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
                RefreshScopes();
                Debug.Value = "Workflow path is valid. You can now select a scope and generate classes.";
                ((TextBlockWidget)Debug.Widget).Level = "Success";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error validating workflow path: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }
        public void OnDesignTimePathUpdated()
        {
            try
            {

                _meta = ConfigService.ValidateConfigFile(DesignTimePath.Value);
                if (_meta.ConfigFileError != null)
                {

                    switch (_meta.ConfigFileError)
                    {
                        case ConfigService.ConfigFileError.FileNotFound
                            :
                            Debug.Value = "Design time path not found. Please provide a valid file or folder path.";
                            ((TextBlockWidget)Debug.Widget).Level = "Error";
                            DesignTimePath.IsPrincipal = true;
                            DesignTimePath.IsVisible = true;

                            break;
                        case ConfigService.ConfigFileError.NotExcelFile:
                            Debug.Value = "Design time path is not a valid Excel file. Please provide a valid file or folder path.";
                            ((TextBlockWidget)Debug.Widget).Level = "Error";
                            DesignTimePath.IsPrincipal = true;
                            DesignTimePath.IsVisible = true;
                            break;

                    }
                    Scope.Value = null;
                    Scope.IsVisible = false;
                    DesignTimePath = null;
                    RefreshScopes();
                    Debug.Value = "Please provide a valid design time Excel file path";
                    ((TextBlockWidget)Debug.Widget).Level = "Error";
                    DesignTimePath.IsPrincipal = false;
                    DesignTimePath.IsVisible = true;
                    _meta = null;
                    return;
                }
                DesignTimePath.IsPrincipal = false;
                Scope.IsVisible = true;
                Scope.IsPrincipal = true;
                DesignTimePath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
                DesignTimePath.AddMenuAction(CreateMenuAction(MenuActionIds.OpenFile));

                RefreshScopes();
                var finalSummary = new StringBuilder();
                finalSummary.AppendLine($"Design time path set to: {Path.GetRelativePath(_workflowDesignApi.ProjectPropertiesService.GetProjectDirectory(), _meta.FilePath)}");
                finalSummary.AppendLine($"Scopes:");
                finalSummary.AppendLine($"{string.Join("\n\t- ", _meta.Scopes)}");
                finalSummary.AppendLine($"Configs:");
                finalSummary.AppendLine(string.Join("\n\t- ", _meta.ConfigByScope.Keys.Select(scope =>
                {
                    var config = _meta.ConfigByScope[scope];
                    return $"[{scope}] Settings: {config.Settings.Count} | Assets: {config.Assets.Count} | Files: {config.Files.Count}";
                })));
                Debug.Value = finalSummary.ToString();

                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            catch (Exception ex)
            {
                Debug.Value = "Error validating design time path: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                DesignTimePath.IsPrincipal = true;
                DesignTimePath.IsVisible = true;
                return;
            }
        }
        public void OnScopeUpdated()
        {
            MorphActivityAsync();
            Scope.RemoveMenuAction(MenuActionIds.RefreshScopes);
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.GenerateClasses));
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshScopes));
            var finalSummary = new StringBuilder();
            finalSummary.AppendLine($"Design time path set to: {Path.GetRelativePath(_workflowDesignApi.ProjectPropertiesService.GetProjectDirectory(), _meta.FilePath)}");
            finalSummary.AppendLine($"Scopes:");
            finalSummary.AppendLine($"{string.Join("\n\t- ", _meta.Scopes)}");
            finalSummary.AppendLine($"Configs:");
            finalSummary.AppendLine(string.Join("\n\t- ", _meta.ConfigByScope.Keys.Select(scope =>
            {
                var config = _meta.ConfigByScope[scope];
                return $"[{scope}] Settings: {config.Settings.Count} | Assets: {config.Assets.Count} | Files: {config.Files.Count}";
            })));
            Debug.Value = finalSummary.ToString();
        }

        #endregion



        public bool TryGetValidPath(out string path)
        {
            path = "";
            if (WorkbookPath.Value != null && WorkbookPath.Value.Expression.IsLiteral())
            {
                path = WorkbookPath.Value.Expression.ToString() ?? "";
                path = path.Contains(":") ? path : Path.Combine(_workflowDesignApi?.ProjectPropertiesService.GetProjectDirectory() ?? "", path);
                var valid = (path.EndsWith(".xls") || path.EndsWith(".xlsx")) && File.Exists(path);
                if (!valid)
                {
                    Debug.Value = "Please use a valid Excel file path";
                    ((TextBlockWidget)Debug.Widget).Level = "Error";
                }
                return valid;
            }
            else
            {
                Debug.Value = "Use a string literal path to get design time features for scope. Leave empty for it to be a Dictionary<string, object> instead.";
                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            return false;
        }


    }
}
