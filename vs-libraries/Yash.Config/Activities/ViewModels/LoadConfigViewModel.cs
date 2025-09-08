using System;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using Yash.Config.Helpers;
using Yash.Config.Models;

namespace Yash.Config.Activities.ViewModels
{
    public class LoadConfigViewModel : DesignPropertiesViewModel
    {
        [Category("Input")]
        public DesignInArgument<string> WorkbookPath { get; set; }
        public DesignInArgument<string> BaseUrl { get; set; }
        public DesignInArgument<string> ClientId { get; set; }
        public DesignInArgument<SecureString> ClientSecret { get; set; }

        public DesignInArgument<string> Scope { get; set; }

        [Category("Output")]
        public DesignOutArgument<Dictionary<string, object>> Result { get; set; }

        
        public LoadConfigViewModel(IDesignServices services) : base(services) { 
        }

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

            Scope.DisplayName = Resources.LoadConfig_Scope_DisplayName;
            Scope.Tooltip = Resources.LoadConfig_Scope_Tooltip;
            Scope.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            Scope.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
            Scope.IsRequired = true; // this is an optional field

            //ConfigType.DisplayName = Resources.LoadConfig_ConfigType_DisplayName;
            //ConfigType.Tooltip = Resources.LoadConfig_ConfigType_Tooltip;
            //ConfigType.IsRequired = true; // this is a required field, so it will raise validation errors when empty
            //ConfigType.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            //ConfigType.OrderIndex = orderIndex++; // indicates the order in which the fields appear in the designer (i.e. the line number);
            //ConfigType.PropertyChanged += ConfigType_PropertyChanged;
            BaseUrl.DisplayName = Resources.LoadConfig_BaseUrl_DisplayName;
            BaseUrl.Tooltip = Resources.LoadConfig_BaseUrl_Tooltip;
            BaseUrl.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            BaseUrl.OrderIndex = orderIndex++;
            BaseUrl.IsRequired = true; // this is an optional field
            ClientId.DisplayName = Resources.LoadConfig_ClientId_DisplayName;
            ClientId.Tooltip = Resources.LoadConfig_ClientId_Tooltip;
            ClientId.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
            ClientId.OrderIndex = orderIndex++;
            ClientId.IsRequired = true; // this is an optional field
            ClientSecret.DisplayName = Resources.LoadConfig_ClientSecret_DisplayName;
            ClientSecret.Tooltip = Resources.LoadConfig_ClientSecret_Tooltip;
            ClientSecret.IsPrincipal = true; // specifies if it belongs to the main category (which cannot be collapsed)
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
        }

    }
}
