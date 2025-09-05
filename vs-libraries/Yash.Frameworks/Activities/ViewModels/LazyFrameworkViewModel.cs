using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Activities.Statements;
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

namespace Yash.Frameworks.Activities.ViewModels
{
    public class LazyFrameworkViewModel : DesignPropertiesViewModel
    {
        private readonly IWorkflowDesignApi _workflowDesignApi;
        string AddButtonLabelKey = "AddButtonLabelKey";
        private const string AddButtonTooltipKey = "AddButtonTooltipKey";
        private const string DefaultDisplayNameKey = "DefaultDisplayNameKey";
        private bool _removeContainerGroupProperties;

        [Category("Framework")]
        public DesignProperty<Activity> Framework_Initialize { get; set; }

        [Category("Framework")]
        public DesignProperty<Activity> Framework_Config { get; set; }

        [Category("Options")]
        public DesignProperty<bool> EnableConfig { get; set; }

        [Category("Options")]

        public DesignProperty<bool> EnableInitialize { get; set; }


        public LazyFrameworkViewModel(IDesignServices services) : base(services)
        {
            this._workflowDesignApi = services.GetService<IWorkflowDesignApi>();
        }

        protected override async ValueTask InitializeModelAsync()
        {
            //Debugger.Break(); 
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            await base.InitializeModelAsync();

            PersistValuesChangedDuringInit(); // just for heads-up here; it's a mandatory call only when you change the values of properties during initialization

            var orderIndex = 0;

            EnableConfig.IsPrincipal = true;
            EnableConfig.IsVisible = true;
            EnableConfig.Tooltip = "Enable or disable loading configuration.";
            EnableConfig.OrderIndex = orderIndex++;

            Framework_Config.IsRequired = false;
            Framework_Config.IsPrincipal = true;
            Framework_Config.IsVisible = true;
            Framework_Config.ActivityPropertyName = "Framework_Config";
            Framework_Config.Widget = (IWidget)new DefaultWidget()
            {
                Type = "Container",
            };
            Framework_Config.OrderIndex = orderIndex++;

            EnableInitialize.IsPrincipal = true;
            EnableInitialize.IsVisible = true;
            EnableInitialize.Tooltip = "Enable or disable loading configuration.";
            EnableInitialize.OrderIndex = orderIndex++;

            Framework_Initialize.IsRequired = false;
            Framework_Initialize.IsPrincipal = false;
            Framework_Initialize.IsVisible = false;
            Framework_Initialize.ActivityPropertyName = "Framework_Initialize";
            Framework_Initialize.Widget = (IWidget)new DefaultWidget()
            {
                Type = "Container",
            };
            Framework_Initialize.OrderIndex = orderIndex++;

            //WorkbookPath.DisplayName = Resources.LoadConfig_WorkflowPath_DisplayName;
            //WorkbookPath.Tooltip = Resources.LoadConfig_WorkflowPath_Tooltip;
            ///*
            // * Required fields will automatically raise validation errors when empty.
            // * Unless you do custom validation, required activity properties should be marked as such both in the view model and in the activity:
            // *   -> in the view model use the IsRequired property
            // *   -> in the activity use the [RequiredArgument] attribute.
            // */
            //WorkbookPath.IsRequired = true;

            //WorkbookPath.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            //WorkbookPath.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);

            //ConfigType.DisplayName = Resources.LoadConfig_ConfigType_DisplayName;
            //ConfigType.Tooltip = Resources.LoadConfig_ConfigType_Tooltip;
            //ConfigType.IsRequired = true; // this is a required field, so it will raise validation errors when empty
            //ConfigType.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            //ConfigType.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
        }

        //private void ConfigType_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ConfigType.Value) && ConfigType.Value is Type t)
        //    {
        //        Result.DisplayName = $"{Resources.LoadConfig_Config_DisplayName} ({t.Name})";
        //        Result.Tooltip = $"The loaded configuration object of type {t.FullName}";
        //    }
        //}

        public virtual async ValueTask<IReadOnlyList<ReadOnlyDesignProperty>> GetPropertiesAsync()
        {
            IReadOnlyList<ReadOnlyDesignProperty> propertiesAsync = await base.GetPropertiesAsync();
            return !this._removeContainerGroupProperties ? propertiesAsync : (IReadOnlyList<ReadOnlyDesignProperty>)propertiesAsync.AsEnumerable<ReadOnlyDesignProperty>().Where<ReadOnlyDesignProperty>((Func<ReadOnlyDesignProperty, bool>)(p => p.ActivityPropertyName != "ElseIfs")).ToArray<ReadOnlyDesignProperty>();
        }

        public virtual async ValueTask<ReadOnlyObservableCollection<ReadOnlyDesignProperty>> GetBrowsablePropertiesAsync()
        {
            ReadOnlyObservableCollection<ReadOnlyDesignProperty> browsablePropertiesAsync = await base.GetBrowsablePropertiesAsync();
            return !this._removeContainerGroupProperties ? browsablePropertiesAsync : new ReadOnlyObservableCollection<ReadOnlyDesignProperty>(new ObservableCollection<ReadOnlyDesignProperty>(browsablePropertiesAsync.AsEnumerable<ReadOnlyDesignProperty>().Where<ReadOnlyDesignProperty>((Func<ReadOnlyDesignProperty, bool>)(p => p.ActivityPropertyName != "ElseIfs"))));
        }
        }
    internal record RestrictedActivity(string AssemblyQualifiedName, string DisplayName);
}
