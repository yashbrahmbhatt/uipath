using System;
using System.Activities.DesignViewModels;
using System.Activities.Presentation;
using System.Activities.ViewModels;
using System.Activities.ViewModels.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UiPath.Core.Activities;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.BusyService;
using Yash.Config.Activities.Resources;
using Yash.Config.ConfigurationService;
using Yash.Config.Models;
using Yash.Config.Services;
using Microsoft.Extensions.DependencyInjection;
using Yash.Orchestrator;

namespace Yash.Config.Activities.ViewModels
{
    /// <summary>
    /// ViewModel for the LoadConfig activity that handles configuration loading with design-time support
    /// </summary>
    public class LoadConfigViewModel<T> : DesignPropertiesViewModel
    {
        #region Constants and Menu Action IDs
        public static class MenuActionIds
        {
            public const string AddDesignTimePath = "AddDesignTimePath";
            public const string RefreshScopes = "RefreshScopes";
            public const string OpenFile = "OpenFile";
            public const string GenerateClasses = "GenerateClasses";
            public const string RefreshFiles = "RefreshFiles";
        }
        #endregion

        #region Properties
        public DesignInArgument<string> WorkbookPath { get; set; }

        public DesignProperty<string> DesignTimePath { get; set; }

        public DesignProperty<bool> DebugMode { get; set; }

        public DesignProperty<Type?> Scope { get; set; }

        public DesignProperty<LogLevel> Level { get; set; }
        public DesignOutArgument<T> Result { get; set; }
        [NotMappedProperty]
        public DesignProperty<string> Debug { get; set; }
        #endregion

        #region Private Fields
        private readonly BindingList<string> AvailableDesignTimePaths = [];
        private readonly BindingList<Type> AvailableScopes = [];

        private readonly IWorkflowDesignApi? _workflowDesignApi;
        private readonly IBusyService _busyService;
        private IDesignerStaticTypesService? _morphingService { get; set; }
        private IConfigService? _configService;

        private ConfigFileMetadata? _meta => _configService?.Metadata;
        private ViewModelState _currentState = ViewModelState.Initial;
        #endregion

        #region State Management
        /// <summary>
        /// Represents the current state of the view model
        /// </summary>
        private enum ViewModelState
        {
            /// <summary>Initial state - no design-time path configured</summary>
            Initial,
            /// <summary>Design-time setup - user adding design-time path</summary>
            DesignTimeSetup,
            /// <summary>Fully configured - valid design-time path with scopes available</summary>
            FullyConfigured,
            /// <summary>Error state - validation failed</summary>
            Error
        }

        /// <summary>
        /// Transitions to a new state and updates UI accordingly
        /// </summary>
        private void TransitionToState(ViewModelState newState, string debugMessage = "", string debugLevel = "Info")
        {
            var previousState = _currentState;
            _currentState = newState;

            // Update UI based on state
            UpdateUIForState(newState);

            // Set debug message if provided
            if (!string.IsNullOrEmpty(debugMessage))
                SetDebugMessage(debugMessage, debugLevel);

            // Log state transition for debugging
            System.Diagnostics.Debug.WriteLine($"State transition: {previousState} → {newState}");
        }

        /// <summary>
        /// Updates UI visibility and principal properties based on current state
        /// </summary>
        private void UpdateUIForState(ViewModelState state)
        {
            switch (state)
            {
                case ViewModelState.Initial:
                    WorkbookPath.IsPrincipal = true;
                    DesignTimePath.IsVisible = false;
                    DesignTimePath.IsPrincipal = false;
                    Scope.IsVisible = false;
                    Scope.IsPrincipal = false;
                    break;

                case ViewModelState.DesignTimeSetup:
                    WorkbookPath.IsPrincipal = false;
                    DesignTimePath.IsVisible = true;
                    DesignTimePath.IsPrincipal = true;
                    Scope.IsVisible = false;
                    Scope.IsPrincipal = false;
                    break;

                case ViewModelState.FullyConfigured:
                    WorkbookPath.IsPrincipal = false;
                    DesignTimePath.IsVisible = true;
                    DesignTimePath.IsPrincipal = false;
                    Scope.IsVisible = true;
                    Scope.IsPrincipal = true;
                    break;

                case ViewModelState.Error:
                    DesignTimePath.IsPrincipal = true;
                    DesignTimePath.IsVisible = true;
                    Scope.IsVisible = false;
                    Scope.Value = null;
                    break;
            }
        }
        #endregion

        #region Constructor
        public LoadConfigViewModel(IDesignServices services) : base(services)
        {
            _workflowDesignApi = services.GetService<IWorkflowDesignApi>();
            _busyService = services.GetService<IBusyService>();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the design model with all properties and their configurations
        /// </summary>
        protected override void InitializeModel()
        {
            base.InitializeModel();
            _morphingService = Services.GetService<IDesignerStaticTypesService>();

            InitializeDebugProperty();
            InitializeWorkbookPathProperty();
            InitializeDesignTimePathProperty();
            InitializeScopeProperty();
            InitializeDebugModeProperty();
            InitializeResultProperty();

            // Create a simple service provider that provides the OrchestratorService if AccessProvider is available
            var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            if (_workflowDesignApi?.AccessProvider != null)
            {
                serviceCollection.AddSingleton<IOrchestratorService>(provider => new OrchestratorService(_workflowDesignApi.AccessProvider, (msg, level) => Debug.Value = msg));
            }
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            _configService = new ConfigService(DesignTimePath?.Value, serviceProvider, (msg, level) => Debug.Value = msg);
            //RegisterActivityService(_configService);
            // If DesignTimePath has a saved value, validate and set up the appropriate state
            if (DesignTimePath.HasValue && !string.IsNullOrEmpty(DesignTimePath.Value))
            {
                OnDesignTimePathUpdated();
            }
            else
            {
                TransitionToState(ViewModelState.Initial,
                    "Add a design time path to enable design time features, otherwise the output will be a Dictionary<string, object>", "Info");
            }
        }

        /// <summary>
        /// Initializes the Debug property for displaying status messages
        /// </summary>
        private void InitializeDebugProperty()
        {
            Debug.IsPrincipal = true;
            Debug.OrderIndex = 0;
            Debug.Value = "Add a design time path to enable design time features, otherwise the output will be a Dictionary<string, object>";
            Debug.Widget = new TextBlockWidget()
            {
                Center = false,
                Level = "Info",
                Multiline = true
            };
        }

        /// <summary>
        /// Initializes the WorkbookPath property for specifying the Excel configuration file
        /// </summary>
        private void InitializeWorkbookPathProperty()
        {
            WorkbookPath.DisplayName = Resources.Resources.LoadConfig_WorkflowPath_DisplayName;
            WorkbookPath.Tooltip = Resources.Resources.LoadConfig_WorkflowPath_Tooltip;
            WorkbookPath.IsRequired = true;
            WorkbookPath.IsPrincipal = true;
            WorkbookPath.OrderIndex = 1;
            WorkbookPath.Category = "Input";
            WorkbookPath.AddMenuAction(CreateMenuAction(MenuActionIds.AddDesignTimePath));
        }

        /// <summary>
        /// Initializes the DesignTimePath property for design-time configuration
        /// </summary>
        private void InitializeDesignTimePathProperty()
        {
            DesignTimePath.DisplayName = "Design Time Path";
            DesignTimePath.Tooltip = "Path to the Excel file or folder containing config files used at design time for type generation and validation";
            DesignTimePath.IsPrincipal = false;
            DesignTimePath.IsVisible = false;
            DesignTimePath.OrderIndex = 2;
            DesignTimePath.IsRequired = false;
            DesignTimePath.Category = "Design";
            DesignTimePath.DataSource = DataSourceBuilder<string>.WithId(f => f)
                .WithLabel(f => f)
                .WithData(AvailableDesignTimePaths)
                .Build();
            DesignTimePath.Widget = new DefaultWidget() { Type = "Dropdown" };
            DesignTimePath.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshFiles));
        }

        /// <summary>
        /// Initializes the Scope property for selecting configuration types
        /// </summary>
        private void InitializeScopeProperty()
        {
            Scope.DisplayName = Resources.Resources.LoadConfig_Scope_DisplayName;
            Scope.Tooltip = Resources.Resources.LoadConfig_Scope_Tooltip;
            Scope.IsPrincipal = DesignTimePath.HasValue;
            Scope.OrderIndex = 3;
            Scope.IsRequired = false;
            Scope.IsVisible = false;
            Scope.Category = "Input";
            Scope.Value = typeof(T);
            Scope.Widget = new TypePickerWidget()
            {
                Type = "Dropdown",
                RecommendedTypes = AvailableScopes.ToList(),
                Filter = (t) => _configService?.IsValidConfigType(t) ?? true,
            };
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshScopes));
        }

        /// <summary>
        /// Initializes the DebugMode property for enabling verbose logging
        /// </summary>
        private void InitializeDebugModeProperty()
        {
            DebugMode.DisplayName = "Debug Mode";
            DebugMode.Tooltip = "Enables verbose logging to the debug output";
            DebugMode.IsPrincipal = false;
            DebugMode.OrderIndex = 4;
            DebugMode.IsRequired = false;
            DebugMode.IsVisible = true;
            DebugMode.Category = "Options";
            DebugMode.Value = false;
            DebugMode.Widget = new DefaultWidget() { Type = "Toggle" };
        }

        /// <summary>
        /// Initializes the Result property for the loaded configuration output
        /// </summary>
        private void InitializeResultProperty()
        {
            Result.DisplayName = Resources.Resources.LoadConfig_Result_DisplayName;
            Result.Tooltip = Resources.Resources.LoadConfig_Result_Tooltip;
            Result.IsPrincipal = true;
            Result.IsVisible = true;
            Result.OrderIndex = 5;
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
        #endregion

        #region Menu Actions
        /// <summary>
        /// Creates a menu action based on the provided action ID
        /// </summary>
        private MenuAction CreateMenuAction(string actionId)
        {
            return actionId switch
            {
                MenuActionIds.AddDesignTimePath => new MenuAction()
                {
                    Id = MenuActionIds.AddDesignTimePath,
                    DisplayName = "Add Design Time Path",
                    IsMain = true,
                    IsEnabled = DesignTimePath.HasValue ? DesignTimePath.Value == null : true,
                    IsVisible = true,
                    Handler = AddDesignTimePathMenuAction
                },
                MenuActionIds.GenerateClasses => new MenuAction()
                {
                    Id = MenuActionIds.GenerateClasses,
                    DisplayName = "Generate Classes",
                    IsMain = true,
                    IsEnabled = DesignTimePath.HasValue ? DesignTimePath.Value != null : false,
                    IsVisible = true,
                    Handler = GenerateClassesMenuAction
                },
                MenuActionIds.OpenFile => new MenuAction()
                {
                    Id = MenuActionIds.OpenFile,
                    DisplayName = "Open File",
                    IsMain = true,
                    IsEnabled = DesignTimePath.HasValue ? DesignTimePath.Value != null : false,
                    IsVisible = true,
                    Handler = OpenFileMenuAction
                },
                MenuActionIds.RefreshScopes => new MenuAction()
                {
                    Id = MenuActionIds.RefreshScopes,
                    DisplayName = "Refresh Scopes",
                    IsMain = true,
                    IsEnabled = true,
                    IsVisible = true,
                    Handler = RefreshScopesMenuAction
                },
                MenuActionIds.RefreshFiles => new MenuAction()
                {
                    Id = MenuActionIds.RefreshFiles,
                    DisplayName = "Refresh Files",
                    IsMain = true,
                    IsEnabled = true,
                    IsVisible = true,
                    Handler = RefreshFilesMenuAction
                },
                _ => throw new ArgumentException($"Unknown menu action ID: {actionId}")
            };
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
                TransitionToState(ViewModelState.DesignTimeSetup,
                    "Please select a design time path to enable enhanced features", "Info");
                RefreshFiles();

                // Remove the menu action since we're now in setup mode
                if (WorkbookPath.GetMenuActions().Any(ma => ma.Id == MenuActionIds.AddDesignTimePath))
                    WorkbookPath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
            }
            catch (Exception ex)
            {
                HandleException(ex, "adding design time path", true);
            }
        }

        public async Task OpenFileMenuAction(MenuAction action)
        {
            if (!IsInValidState()) return;

            var pathToOpen = _meta.FilePath;

            try
            {
                if (Directory.Exists(pathToOpen))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{pathToOpen}\"",
                        UseShellExecute = true
                    });
                    SetDebugMessage("Folder opened successfully in File Explorer", "Info");
                }
                else if (File.Exists(pathToOpen))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pathToOpen,
                        UseShellExecute = true
                    });
                    SetDebugMessage("File opened successfully", "Info");
                }
                else
                {
                    SetDebugMessage("Path does not exist", "Error");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "opening path");
            }
        }

        public async Task GenerateClassesMenuAction(MenuAction action)
        {
            if (!IsInValidState()) return;
            if (_workflowDesignApi == null) return;
            if (_configService == null) return;

            try
            {
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_OutputDir_Key, out var outputDir);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Namespace_Key, out var ns);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Usings_Key, out var usings);

                var result = _configService.GenerateClassFiles(outputDir, ns, usings);
                SetDebugMessage("Classes generated successfully", "Info");
            }
            catch (Exception ex)
            {
                HandleException(ex, "generating classes");
            }
        }

        /// <summary>
        /// Refreshes the list of available configuration scopes/types
        /// </summary>
        public void RefreshScopes()
        {
            if (!IsInValidState()) return;

            try
            {
                AvailableScopes.Clear();
                var types = new List<Type>();

                // Get assemblies with error handling for each step
                var assemblies = GetLoadedAssembliesSafely();

                // Safely enumerate assemblies and handle type loading failures
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Use GetTypes() with exception handling for each assembly
                        var assemblyTypes = GetTypesFromAssemblySafely(assembly)
                            .Where(t => IsValidConfigTypeSafe(t))
                            .ToList();

                        types.AddRange(assemblyTypes);
                    }
                    catch (Exception ex)
                    {
                        // Log but continue with other assemblies
                        System.Diagnostics.Debug.WriteLine($"Warning: Could not process assembly {assembly.FullName}: {ex.Message}");
                    }
                }

                foreach (var type in types)
                    AvailableScopes.Add(type);

                Scope.DataSource = DataSourceBuilder<Type>
                    .WithId((s) => s.GUID.ToString())
                    .WithLabel((s) => s.Name.ToString())
                    .WithData(AvailableScopes)
                    .Build();

                var message = AvailableScopes.Count > 0
                    ? $"Found {AvailableScopes.Count} configuration types in loaded assemblies for scope."
                    : "No configuration types found. This may be normal if no config classes are loaded yet.";

                SetDebugMessage(message, "Info");
            }
            catch (Exception ex)
            {
                HandleException(ex, "refreshing scopes");
            }
        }

        /// <summary>
        /// Safely gets loaded assemblies, filtering out problematic ones
        /// </summary>
        private Assembly[] GetLoadedAssembliesSafely()
        {
            try
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not get all assemblies: {ex.Message}");
                return Array.Empty<Assembly>();
            }
        }

        /// <summary>
        /// Safely gets types from an assembly, handling type loading exceptions
        /// </summary>
        private Type[] GetTypesFromAssemblySafely(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle type loading exceptions - some types in assembly failed to load
                System.Diagnostics.Debug.WriteLine($"Type loading warning in assembly {assembly.FullName}: {ex.Message}");

                // Log specific loader exceptions for debugging
                if (ex.LoaderExceptions?.Any() == true)
                {
                    foreach (var loaderEx in ex.LoaderExceptions.Where(e => e != null))
                    {
                        System.Diagnostics.Debug.WriteLine($"  Loader exception: {loaderEx.Message}");
                    }
                }

                // Return the successfully loaded types
                return ex.Types?.Where(t => t != null).ToArray() ?? Array.Empty<Type>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not load types from assembly {assembly.FullName}: {ex.Message}");
                return Array.Empty<Type>();
            }
        }

        /// <summary>
        /// Safely checks if a type is a valid configuration type, handling any exceptions
        /// </summary>
        private bool IsValidConfigTypeSafe(Type type)
        {
            try
            {
                return _configService?.IsValidConfigType(type) ?? false;
            }
            catch (Exception ex)
            {
                // Log the specific type that caused issues for debugging
                System.Diagnostics.Debug.WriteLine($"Warning: Could not validate config type {type?.FullName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refreshes the list of available Excel files in the project directory
        /// </summary>
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
                        .Where(f => f?.Exists == true)
                        .ToList();

                    foreach (var file in files)
                        AvailableDesignTimePaths.Add(file.FullName);

                    DesignTimePath.DataSource = DataSourceBuilder<string>
                        .WithId(f => f)
                        .WithLabel(f => Path.GetRelativePath(_workflowDesignApi?.ProjectPropertiesService?.GetProjectDirectory() ?? "", f))
                        .WithData(AvailableDesignTimePaths)
                        .Build();
                }

                SetDebugMessage($"Found {AvailableDesignTimePaths.Count} Excel files in project directory for design time use.", "Info");
            }
            catch (Exception ex)
            {
                HandleException(ex, "refreshing files");
            }
        }
        #endregion

        #region Property Changed Handlers
        /// <summary>
        /// Handles changes to the WorkbookPath property
        /// </summary>
        public void OnWorkflowPathUpdated()
        {
            try
            {
                if (_meta?.ConfigFileError != null)
                {
                    TransitionToState(ViewModelState.Initial,
                        "Click the menu action to add a design time path for enhanced features", "Info");
                    return;
                }

                ConfigureDesignTimePathAfterValidation();
                TransitionToState(ViewModelState.FullyConfigured,
                    "Workflow path is valid. You can now select a scope and generate classes.", "Info");
            }
            catch (Exception ex)
            {
                HandleException(ex, "validating workflow path", true);
            }
        }

        /// <summary>
        /// Handles changes to the DesignTimePath property
        /// </summary>
        public void OnDesignTimePathUpdated()
        {
            if (_configService == null) return;

            try
            {
                _configService.FilePath = DesignTimePath.Value;
                _configService.ValidateConfigFile();

                if (_meta?.ConfigFileError != null)
                {
                    HandleConfigFileError();
                    return;
                }

                ConfigureDesignTimePathAfterValidation();
                TransitionToState(ViewModelState.FullyConfigured);
                DisplayConfigurationSummary();
            }
            catch (Exception ex)
            {
                HandleException(ex, "validating design time path", true);
                ResetDesignTimePathOnError();
            }
        }

        /// <summary>
        /// Handles changes to the Scope property
        /// </summary>
        public void OnScopeUpdated()
        {
            try
            {
                MorphActivityAsync();
                UpdateScopeMenuActions();
            }
            catch (Exception ex)
            {
                HandleException(ex, "updating scope", true);
            }
        }

        protected override void OnChanged()
        {
            base.OnChanged();
            if (_configService != null && _morphingService != null)
            {
                if (!_configService.IsValidConfigType(typeof(T)))
                {
                    Scope.Value = typeof(Models.Config.Configuration);
                    MorphActivityAsync();
                }
            }
        }

        /// <summary>
        /// Updates menu actions for the Scope property
        /// </summary>
        private void UpdateScopeMenuActions()
        {
            // Clean up existing menu actions first to avoid duplicates
            Scope.RemoveMenuAction(MenuActionIds.RefreshScopes);
            Scope.RemoveMenuAction(MenuActionIds.GenerateClasses);

            // Add the menu actions for this state
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.GenerateClasses));
            Scope.AddMenuAction(CreateMenuAction(MenuActionIds.RefreshScopes));
        }

        /// <summary>
        /// Configures design-time path properties after successful validation
        /// </summary>
        private void ConfigureDesignTimePathAfterValidation()
        {
            DesignTimePath.IsPrincipal = false;
            DesignTimePath.Value = _meta.FilePath;
            DesignTimePath.IsVisible = true;
            Scope.IsVisible = true;
            Scope.IsPrincipal = true;

            // Clean up menu actions first to avoid duplicates
            WorkbookPath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
            DesignTimePath.RemoveMenuAction(MenuActionIds.AddDesignTimePath);
            DesignTimePath.RemoveMenuAction(MenuActionIds.GenerateClasses);
            DesignTimePath.RemoveMenuAction(MenuActionIds.OpenFile);

            // Add the appropriate menu actions for this state
            DesignTimePath.AddMenuAction(CreateMenuAction(MenuActionIds.GenerateClasses));
            DesignTimePath.AddMenuAction(CreateMenuAction(MenuActionIds.OpenFile));

            RefreshScopes();
        }

        /// <summary>
        /// Handles configuration file errors by setting appropriate error messages
        /// </summary>
        private void HandleConfigFileError()
        {
            var errorMessage = _meta?.ConfigFileError switch
            {
                ConfigFileError.FileNotFound => "Design time path not found. Please provide a valid file or folder path.",
                ConfigFileError.NotExcelFile => "Design time path is not a valid Excel file. Please provide a valid file or folder path.",
                ConfigFileError.NullValue => "Design time path is null or empty. Please provide a valid file or folder path.",
                _ => "Unknown error with design time path. Please provide a valid file or folder path."
            };

            SetDebugMessage(errorMessage, "Error");
            ResetDesignTimePathOnError();
        }

        /// <summary>
        /// Resets design-time path configuration when an error occurs
        /// </summary>
        private void ResetDesignTimePathOnError()
        {
            Scope.Value = null;
            Scope.IsVisible = false;
            DesignTimePath.IsPrincipal = true;
            DesignTimePath.IsVisible = true;
            RefreshScopes();
            TransitionToState(ViewModelState.Error);
        }

        /// <summary>
        /// Displays a summary of the loaded configuration
        /// </summary>
        private void DisplayConfigurationSummary()
        {
            if (_workflowDesignApi?.ProjectPropertiesService == null || _meta == null)
            {
                SetDebugMessage("Unable to display configuration summary: missing dependencies", "Warning");
                return;
            }

            try
            {
                var finalSummary = new StringBuilder();
                var projectDir = _workflowDesignApi.ProjectPropertiesService.GetProjectDirectory();
                var relativePath = Path.GetRelativePath(projectDir, _meta.FilePath);

                finalSummary.AppendLine($"Design time path set to: {relativePath}");
                finalSummary.AppendLine($"Configs:");

                if (_meta.ConfigByScope?.Any() == true)
                {
                    var logConfigs = _meta.ConfigByScope.Keys.SelectMany(scope =>
                    {
                        var config = _meta.ConfigByScope[scope];
                        var logs = new List<string>
                        {
                            $"\n\t- [{scope}] Settings: {config.Settings?.Count ?? 0} | Assets: {config.Assets?.Count ?? 0} | Files: {config.Files?.Count ?? 0}\n"
                        };

                        if (config.Settings?.Any() == true)
                            logs.AddRange(config.Settings.Select(setting => $"\t\t- Setting: {setting.Name} = {setting.Value}"));
                        if (config.Assets?.Any() == true)
                            logs.AddRange(config.Assets.Select(asset => $"\t\t- Asset: {asset.Name} = {asset.Value}"));
                        if (config.Files?.Any() == true)
                            logs.AddRange(config.Files.Select(file => $"\t\t- File: {file.Name} = {file.Path}"));

                        return logs;
                    });

                    foreach (var lc in logConfigs)
                        finalSummary.AppendLine(lc);
                }
                else
                {
                    finalSummary.AppendLine("\t(No configurations found)");
                }

                SetDebugMessage(finalSummary.ToString(), "Info");
            }
            catch (Exception ex)
            {
                HandleException(ex, "displaying configuration summary");
            }
        }
        #endregion



        #region Helper Methods
        /// <summary>
        /// Checks if the view model is in a valid state for operations
        /// </summary>
        private bool IsInValidState()
        {
            return _configService != null && _meta?.ConfigFileError == null && _currentState != ViewModelState.Error;
        }

        /// <summary>
        /// Sets a debug message with the specified level
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="level">The debug level (Info, Success, Error, etc.)</param>
        private void SetDebugMessage(string message, string level = "Info")
        {
            Debug.Value = message;
            ((TextBlockWidget)Debug.Widget).Level = level;
        }

        /// <summary>
        /// Handles exceptions by setting an error debug message and optionally transitioning to error state
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="operation">The operation that was being performed</param>
        /// <param name="transitionToError">Whether to transition to error state</param>
        private void HandleException(Exception ex, string operation, bool transitionToError = false)
        {
            var errorMessage = $"Error {operation}: [{ex.Source}] {ex.Message}";
            SetDebugMessage(errorMessage, "Error");

            if (transitionToError)
                TransitionToState(ViewModelState.Error, errorMessage, "Error");
        }

        /// <summary>
        /// Asynchronously morphs the activity to use the selected scope type
        /// </summary>
        private async void MorphActivityAsync()
        {
            if (!Scope.HasValue)
            {
                SetDebugMessage("Scope is not set. Cannot morph activity.", "Warning");
                return;
            }
            if (_morphingService == null)
            {
                SetDebugMessage("Type morphing service is unavailable.", "Warning");
                return;
            }
            try
            {
                await _morphingService.UseTypeAsync(Scope.Value);

            }
            catch (Exception ex)
            {
                HandleException(ex, "morphing activity to selected scope type");
            }
        }

        /// <summary>
        /// Validates if the provided workbook path is valid
        /// </summary>
        /// <param name="path">The validated path if successful</param>
        /// <returns>True if the path is valid, false otherwise</returns>
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
                    SetDebugMessage("Please use a valid Excel file path", "Error");
                }
                return valid;
            }
            else
            {
                SetDebugMessage("Use a string literal path to get design time features for scope. Leave empty for it to be a Dictionary<string, object> instead.", "Info");
            }
            return false;
        }

        /// <summary>
        /// Converts TraceEventType to LogLevel
        /// </summary>
        public LogLevel ConvertTraceEventToLogLevel(TraceEventType type)
        {
            return type switch
            {
                TraceEventType.Verbose => LogLevel.Trace,
                TraceEventType.Start or TraceEventType.Stop or TraceEventType.Suspend or TraceEventType.Resume or TraceEventType.Information => LogLevel.Info,
                TraceEventType.Warning => LogLevel.Warn,
                TraceEventType.Critical or TraceEventType.Error => LogLevel.Error,
                _ => throw new Exception("Could not convert TraceEvent to LogLevel: " + Enum.GetName(type))
            };
        }

        /// <summary>
        /// Converts LogLevel to TraceEventType
        /// </summary>
        public TraceEventType ConvertLogLevelToTraceEvent(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => TraceEventType.Verbose,
                LogLevel.Info => TraceEventType.Information,
                LogLevel.Warn => TraceEventType.Warning,
                LogLevel.Error => TraceEventType.Error,
                _ => throw new Exception("Could not convert LogLevel to TraceEvent: " + level.ToString())
            };
        }
        #endregion
    }
}
