using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Activities.ViewModels;
using System.Activities.ViewModels.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.BusyService;
using Yash.Config.Helpers;
using Yash.Config.Models;

namespace Yash.Config.Activities.ViewModels
{
    public class LoadConfigViewModel<T> : DesignPropertiesViewModel where T: Models.Config, new()
    {
        [Category("Input")]
        public DesignInArgument<string> WorkbookPath { get; set; }
        [Category("Input")]

        public DesignInArgument<string> Scope { get; set; }
        [Category("Authentication")]
        public DesignInArgument<string> BaseUrl { get; set; }
        [Category("Authentication")]
        public DesignInArgument<string> ClientId { get; set; }
        [Category("Authentication")]
        public DesignInArgument<SecureString> ClientSecret { get; set; }


        [Category("Output")]
        public DesignOutArgument<T> Result { get; set; }

        [NotMappedProperty]
        public DesignProperty<Type> ConfigType { get; set; }


        private readonly BindingList<string> AvailableScopes = new();

        public DesignProperty<string> Debug { get; set; }
        private IWorkflowDesignApi? _workflowDesignApi;
        private readonly IBusyService _busyService;
        private IDesignerCustomTypesService _designerCustomTypesService;
        private IDesignerStaticTypesService _morphingService;
        private bool _typeWidgetAvailable;
        private bool _jitServiceAvailable;

        public LoadConfigViewModel(IDesignServices services) : base(services)
        {
            _workflowDesignApi = services.GetService<IWorkflowDesignApi>();
        }
        protected override void InitializeModel()
        {
            //Debugger.Break(); 
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            base.InitializeModel();
            _morphingService = Services.GetService<IDesignerStaticTypesService>();
            //PersistValuesChangedDuringInit(); // just for heads-up here; it's a mandatory call only when you change the values of properties during initialization

            var orderIndex = 0;

            Debug.IsPrincipal = true;
            Debug.OrderIndex = orderIndex++;
            Debug.Value = "Debug";
            Debug.Widget = new TextBlockWidget()
            {
                Center = false,
                Level = "Info",
                Multiline = true
            };

            WorkbookPath.DisplayName = Resources.LoadConfig_WorkflowPath_DisplayName;
            WorkbookPath.Tooltip = Resources.LoadConfig_WorkflowPath_Tooltip;
            WorkbookPath.IsRequired = true;

            WorkbookPath.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            WorkbookPath.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
            WorkbookPath.AddMenuAction(new MenuAction()
            {
                DisplayName = "Generate Classes",
                IsMain = true,
                IsEnabled = false,
                IsVisible = true,
                Handler = OnGenerateClassesMenuAction
            });
            WorkbookPath.AddMenuAction(new MenuAction()
            {
                DisplayName = "Open File",
                IsMain = true,
                IsEnabled = false,
                IsVisible = true,
                Handler = OpenFileMenuAction,
            });

            Scope.DisplayName = Resources.LoadConfig_Scope_DisplayName;
            Scope.Tooltip = Resources.LoadConfig_Scope_Tooltip;
            Scope.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            Scope.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
            Scope.IsRequired = true; // this is an optional field
            Scope.DataSource = DataSourceBuilder<string>.WithId((s) => s).WithLabel((s) => s).WithData(AvailableScopes).Build();
            Scope.Widget = new DefaultWidget() { Type = "Dropdown" };
            Scope.AddMenuAction(new MenuAction()
            {
                DisplayName = "Refresh Scopes",
                IsMain = true,
                IsEnabled = false,
                IsVisible = true,
                Handler = RefreshScopesMenuAction,
            });



            BaseUrl.DisplayName = Resources.LoadConfig_BaseUrl_DisplayName;
            BaseUrl.Tooltip = Resources.LoadConfig_BaseUrl_Tooltip;
            BaseUrl.IsPrincipal = false; // specifies if it belongs to the main category (which cannot be collapsed)
            BaseUrl.OrderIndex = orderIndex++;
            BaseUrl.IsRequired = true; // this is an optional field
            ClientId.DisplayName = Resources.LoadConfig_ClientId_DisplayName;
            ClientId.Tooltip = Resources.LoadConfig_ClientId_Tooltip;
            ClientId.IsPrincipal = false; // specifies if it belongs to the main category (which cannot be collapsed)
            ClientId.OrderIndex = orderIndex++;
            ClientId.IsRequired = true; // this is an optional field
            ClientSecret.DisplayName = Resources.LoadConfig_ClientSecret_DisplayName;
            ClientSecret.Tooltip = Resources.LoadConfig_ClientSecret_Tooltip;
            ClientSecret.IsPrincipal = false; // specifies if it belongs to the main category (which cannot be collapsed)
            ClientSecret.OrderIndex = orderIndex++;
            ClientSecret.IsRequired = true; // this is an optional field

            /*
             * Output properties are never mandatory.
             * By convention, they are not principal and they are placed at the end of the activity.
             */
            Result.DisplayName = Resources.LoadConfig_Result_DisplayName;
            Result.Tooltip = Resources.LoadConfig_Result_Tooltip;
            Result.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            Result.OrderIndex = orderIndex++;

            InitCustomTypeService();
        }


        protected override void ManualRegisterDependencies()
        {
            base.ManualRegisterDependencies();
            RegisterDependency(WorkbookPath, "Value", "OnWorkflowPathUpdated");
            RegisterDependency(ConfigType, "Value", "OnConfigTypeUpdated");
        }

        protected override void InitializeRules()
        {
            base.InitializeRules();
            Rule("OnWorkflowPathUpdated", OnWorkflowPathUpdated);
            Rule("OnConfigTypeUpdated", new Action(MorphActivityAsync), false);
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

        private async void MorphActivityAsync()
        {
            if (!(ConfigType.HasValue && ConfigType.Value.IsAssignableFrom(typeof(Models.Config)))) return;
            await _morphingService.UseTypeAsync(ConfigType.Value);
        }
        public void RefreshScopes()
        {
            var valid = TryGetValidPath(out var path);
            if (!valid) return;
            try
            {
                AvailableScopes.Clear();
                var file = ConfigService.ReadConfigFile(path, null);
                var fileScopes = file.Files.Select(f => f.Scope).Concat(file.Assets.Select(a => a.Scope)).Concat(file.Settings.Select(s => s.Scope)).Distinct().OrderBy(s => s);
                foreach (var scope in fileScopes)
                {
                    AvailableScopes.Add(scope);
                }

                var fullSummary = new StringBuilder();
                fullSummary.AppendLine($"Found {file.Settings.Count} settings, {file.Assets.Count} assets and {file.Files.Count} files in config.");
                fullSummary.AppendLine($"Found scopes: " + string.Join(", ", AvailableScopes));
                Debug.Value = fullSummary.ToString();
                ((TextBlockWidget)Debug.Widget).Level = "Info";
                Scope.Value = null;
                Scope.DataSource = DataSourceBuilder<string>.WithId((s) => s).WithLabel((s) => s).WithData(AvailableScopes).Build();
            }
            catch (Exception ex)
            {
                Debug.Value = "Error reading config file: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
            }
        }
        public void OnWorkflowPathUpdated()
        {
            var valid = TryGetValidPath(out var path);
            var actions = WorkbookPath.GetMenuActions().Concat(Scope.GetMenuActions());
            foreach (var action in actions)
            {
                action.IsEnabled = valid;
            }
            if (!valid) return;

            RefreshScopes();
        }

        public async Task RefreshScopesMenuAction(MenuAction action)
        {
            var valid = TryGetValidPath(out var path);
            if (!valid) return;
            try
            {
                RefreshScopes();
            }
            catch (Exception ex)
            {
                Debug.Value = "Error refreshing scopes: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }


        public async Task OpenFileMenuAction(MenuAction action)
        {
            var valid = TryGetValidPath(out var path);
            if (!valid) return;
            try
            {
                File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                Debug.Value = "Error opening file: " + ex.Message;
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }
        }
        public async Task OnGenerateClassesMenuAction(MenuAction action)
        {
            var valid = TryGetValidPath(out var path);
            if (valid) return;
            try
            {
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_OutputDir_Key, out var outputDir);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Namespace_Key, out var ns);
                _workflowDesignApi.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_Usings_Key, out var usings);
                MessageBox.Show(ConfigService.GenerateClassFiles(path, outputDir, ns, usings), "Summary", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                Debug.Value = "Error generating classes: " + ex.Message.ToString();
                ((TextBlockWidget)Debug.Widget).Level = "Error";
                return;
            }

        }

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
                Debug.Value = "Use a string literal path to get design time features for scope";
                ((TextBlockWidget)Debug.Widget).Level = "Info";
            }
            return false;
        }
    }
}
