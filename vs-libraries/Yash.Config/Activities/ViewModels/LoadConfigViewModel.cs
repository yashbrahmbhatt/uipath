using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using Yash.Config.Models;

namespace Yash.Config.Activities.ViewModels
{
    public class LoadConfigViewModel : DesignPropertiesViewModel
    {
        [Category("Input")]
        public DesignInArgument<string> WorkbookPath { get; set; }

        [Category("Output")]
        public DesignOutArgument<Dictionary<string, object>> Result { get; set; }


        public LoadConfigViewModel(IDesignServices services) : base(services) { }

        protected override void InitializeModel()
        {
            //Debugger.Break(); 
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            base.InitializeModel();

            PersistValuesChangedDuringInit(); // just for heads-up here; it's a mandatory call only when you change the values of properties during initialization

            var orderIndex = 0;

            WorkbookPath.DisplayName = Resources.LoadConfig_WorkflowPath_DisplayName;
            WorkbookPath.Tooltip = Resources.LoadConfig_WorkflowPath_Tooltip;
            /*
             * Required fields will automatically raise validation errors when empty.
             * Unless you do custom validation, required activity properties should be marked as such both in the view model and in the activity:
             *   -> in the view model use the IsRequired property
             *   -> in the activity use the [RequiredArgument] attribute.
             */
            WorkbookPath.IsRequired = true;

            WorkbookPath.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            WorkbookPath.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);

            //ConfigType.DisplayName = Resources.LoadConfig_ConfigType_DisplayName;
            //ConfigType.Tooltip = Resources.LoadConfig_ConfigType_Tooltip;
            //ConfigType.IsRequired = true; // this is a required field, so it will raise validation errors when empty
            //ConfigType.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            //ConfigType.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
            //ConfigType.PropertyChanged += ConfigType_PropertyChanged;

            /*
             * Output properties are never mandatory.
             * By convention, they are not principal and they are placed at the end of the activity.
             */
            Result.DisplayName = Resources.LoadConfig_Result_DisplayName;
            Result.Tooltip = Resources.LoadConfig_Result_Tooltip;
            Result.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            Result.OrderIndex = orderIndex++;
        }

        //private void ConfigType_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ConfigType.Value) && ConfigType.Value is Type t)
        //    {
        //        Result.DisplayName = $"{Resources.LoadConfig_Config_DisplayName} ({t.Name})";
        //        Result.Tooltip = $"The loaded configuration object of type {t.FullName}";
        //    }
        //}

    }
}
