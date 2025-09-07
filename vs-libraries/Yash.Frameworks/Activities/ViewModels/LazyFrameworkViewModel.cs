using DocumentFormat.OpenXml.Bibliography;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Activities.Statements;
using System.Activities.ViewModels;
using System.Activities.ViewModels.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Api;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Widgets;
using Yash.Frameworks.Activities.ViewModels.Helpers;

namespace Yash.Frameworks.Activities.ViewModels
{
    public class LazyFrameworkViewModel : DesignPropertiesViewModel
    {
        public DesignProperty<Activity> Framework_Initialize { get; set; }

        public DesignProperty<Activity> Framework_Settings { get; set; }

        public DesignProperty<bool> EnableSettings { get; set; }


        public DesignProperty<bool> EnableInitialize { get; set; }

        public DesignProperty<string> Debug { get; set; }
        public DesignOutArgument<LazyFrameworkResult> Result { get; set; }


        public LazyFrameworkViewModel(IDesignServices services) : base(services)
        {
        }

        protected override void InitializeModel()
        {

            //Debugger.Break(); 
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            base.InitializeModel();

            //PersistValuesChangedDuringInit(); // just for heads-up here; it's a mandatory call only when you change the values of properties during initialization

            var _orderIndex = 0;

            Debug.IsPrincipal = true;
                Debug.IsVisible = true;
            Debug.Category = "Debug";
            Debug.Tooltip = "Debug Information";
            Debug.Widget =  new TextBlockWidget()
                            {
                                Center = false,
                                Multiline = true,
                                Level = "Info",
                            };
            Debug.OrderIndex = _orderIndex++;
            Debug.Value = "Debug";

            EnableSettings.Name = "Do you have settings?";
            EnableSettings.OrderIndex = _orderIndex++;
            EnableSettings.IsVisible = true;
            EnableSettings.IsPrincipal = true;
            EnableSettings.Category = "Settings";
            EnableSettings.Tooltip = "Enable or disable loading configuration.";
            EnableSettings.Widget = new DefaultWidget() { Type = "Toggle" };

            Framework_Settings.Name = "Initialize Settings Workflow";
            Framework_Settings.OrderIndex = _orderIndex++;
            Framework_Settings.IsVisible = true;
            Framework_Settings.IsPrincipal = true;
            Framework_Settings.Category = "Framework";
            Framework_Settings.Tooltip = "The configuration loading sequence.";
            Framework_Settings.Widget = new DefaultWidget() { Type = "Container", Metadata = { { "ShowExpanded", "true" } } };

            EnableInitialize.Name = "Do you need to initialize applications?";
            EnableInitialize.OrderIndex = _orderIndex++;
            EnableInitialize.IsVisible = true;
            EnableInitialize.IsPrincipal = true;
            EnableInitialize.Category = "Settings";
            EnableInitialize.Tooltip = "Enable or disable framework initialization.";
            EnableInitialize.Widget = new DefaultWidget() { Type = "Toggle" };

            Framework_Initialize.Name = "Initialize Applications Workflow";
            Framework_Initialize.OrderIndex = _orderIndex++;
            Framework_Initialize.IsVisible = true;
            Framework_Initialize.IsPrincipal = true;
            Framework_Initialize.Category = "Framework";
            Framework_Initialize.Tooltip = "The framework initialization sequence.";
            Framework_Initialize.Widget = new DefaultWidget() { Type = "Container", Metadata = { { "ShowExpanded", "true" } } };

            Result.Name = "Result";
            Result.OrderIndex = _orderIndex++;
            Result.IsVisible = true;
            Result.IsPrincipal = false;
            Result.Category = "Output";
            Result.Tooltip = "The result of the framework execution.";




            Debug.Value =  $"Framework Settings Enabled: {EnableSettings.Value}\n" +
                           $"Framework Initialize Enabled: {EnableInitialize.Value}\n" +
                           $"Framework_Initialize Metadata: {JsonConvert.SerializeObject(EnableSettings.ActivityPropertyName)}";


            //UpdateDebug(_workflowDesignApi, "test");
        }

        //protected override void ManualRegisterDependencies()
        //{
        //    base.ManualRegisterDependencies();
        //    //RegisterDependency(EnableSettings, "Value", "ToggleSettings");
        //    //RegisterDependency(EnableInitialize, "Value", "ToggleInitialize");
        //    //RegisterDependency(Framework_Initialize, "Value", "Framework_Initialize");
        //    //RegisterDependency(Framework_Settings, "Value", "Framework_Settings");
        //}
        
        //private void UpdateDebug(object obj, string prop)
        //{
        //    Debug.Value = JsonConvert.SerializeObject(obj, Formatting.Indented);
        //}

        //protected override void InitializeRules()
        //{
        //    base.InitializeRules();
        //   // Rule("ToggleSettings", new Action(ToggleSettings));
        //    //Rule("ToggleInitialize", new Action(ToggleInitialize));
        //   // Rule("Framework_Initialize", new Action(Framework_InitializedChanged));
        //   // Rule("Framework_Settings", new Action(Framework_SettingsChanged));
        //}

        //private void Framework_InitializedChanged()
        //{
        //    UpdateDebug(Framework_Initialize, nameof(Framework_Initialize));
        //}
        //protected void Framework_SettingsChanged()
        //{
        //    UpdateDebug(Framework_Settings, nameof(Framework_Settings));
        //}
        //private void ToggleSettings()
        //{
        //    //if (EnableSettings.Value)
        //    //{
        //    //    Framework_Settings.IsVisible = true;
        //    //    EnableSettings.IsPrincipal = false;
        //    //}
        //    //else
        //    //{
        //    //    Framework_Settings.IsVisible = false;
        //    //    EnableSettings.IsPrincipal = true;
        //    //}
        //}
        //private void ToggleInitialize()
        //{
        //    if (EnableInitialize.Value)
        //    {
        //        Framework_Initialize.IsVisible = true;
        //        EnableInitialize.IsPrincipal = false;
        //    }
        //    else
        //    {
        //        Framework_Initialize.IsVisible = false;
        //        EnableInitialize.IsPrincipal = true;
        //    }
        //}
        //public override async ValueTask<IReadOnlyList<ReadOnlyDesignProperty>> GetPropertiesAsync()
        //{
        //    IReadOnlyList<ReadOnlyDesignProperty> propertiesAsync = await base.GetPropertiesAsync();
        //    return !this._removeContainerGroupProperties ? propertiesAsync : (IReadOnlyList<ReadOnlyDesignProperty>)propertiesAsync.AsEnumerable<ReadOnlyDesignProperty>().Where<ReadOnlyDesignProperty>((Func<ReadOnlyDesignProperty, bool>)(p => p.ActivityPropertyName != "ElseIfs")).ToArray<ReadOnlyDesignProperty>();
        //}

        //public override async ValueTask<ReadOnlyObservableCollection<ReadOnlyDesignProperty>> GetBrowsablePropertiesAsync()
        //{
        //    ReadOnlyObservableCollection<ReadOnlyDesignProperty> browsablePropertiesAsync = await base.GetBrowsablePropertiesAsync();
        //    return !this._removeContainerGroupProperties ? browsablePropertiesAsync : new ReadOnlyObservableCollection<ReadOnlyDesignProperty>(new ObservableCollection<ReadOnlyDesignProperty>(browsablePropertiesAsync.AsEnumerable<ReadOnlyDesignProperty>().Where<ReadOnlyDesignProperty>((Func<ReadOnlyDesignProperty, bool>)(p => p.ActivityPropertyName != "ElseIfs"))));
        //}

    }
    //record RestrictedActivity(string AssemblyQualifiedName, string DisplayName);
}
