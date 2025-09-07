using System.Activities.DesignViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Frameworks.Activities.ViewModels
{
    public class ActivityTemplateViewModel : DesignPropertiesViewModel
    {
        /*
         * The result property comes from the activity's base class
         */
        /*
         * Properties names must match the names and generic type arguments of the properties in the activity
         * Use DesignInArgument for properties that accept a variable
         */
        /*
         * Use DesignProperty for properties that accept a constant value                
         */
        /*
        * The result property comes from the activity's base class
        */
        public DesignOutArgument<int> Result { get; set; }

        public DesignProperty<bool> EnableInitializeSettings { get; set; }
        public DesignProperty<System.Activities.Activity> FrameworkInitializeSettings { get; set; }
        public DesignProperty<System.Activities.Activity> FrameworkInitializeApplications { get; set; }
        public DesignProperty<bool> EnableInitializeApplications { get; set; }

        public ActivityTemplateViewModel(IDesignServices services) : base(services)
        {
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



            /*
             * Output properties are never mandatory.
             * By convention, they are not principal and they are placed at the end of the activity.
             */
            Result.DisplayName = "Result";
            Result.Tooltip = "";
            Result.OrderIndex = orderIndex;

            EnableInitializeSettings.IsPrincipal = true;
            EnableInitializeSettings.IsVisible = true;
            EnableInitializeSettings.DisplayName = "Enable Initialize Settings";
            EnableInitializeSettings.Tooltip = "Enable or disable loading configuration.";
            EnableInitializeSettings.OrderIndex = orderIndex++;

            FrameworkInitializeSettings.IsPrincipal = true;
            FrameworkInitializeSettings.IsVisible = true; // hidden by default, visibility is controlled by the EnableInitializeSettings property
            FrameworkInitializeSettings.DisplayName = "Initialize Settings Workflow";
            FrameworkInitializeSettings.Tooltip = "The workflow that initializes the settings.";
            FrameworkInitializeSettings.OrderIndex = orderIndex++;
            FrameworkInitializeSettings.Widget =new DefaultWidget() { Type = "Container" };

            EnableInitializeApplications.IsPrincipal = true;
            EnableInitializeApplications.IsVisible = true;
            EnableInitializeApplications.DisplayName = "Enable Initialize Applications";
            EnableInitializeApplications.Tooltip = "Enable or disable framework initialization.";
            EnableInitializeApplications.OrderIndex = orderIndex++;

            FrameworkInitializeApplications.IsPrincipal = true;
            FrameworkInitializeApplications.IsVisible = true;
            FrameworkInitializeApplications.DisplayName = "Initialize Applications Workflow";
            FrameworkInitializeApplications.Tooltip = "The workflow that initializes the applications.";
            FrameworkInitializeApplications.OrderIndex = orderIndex++;
            FrameworkInitializeApplications.Widget = new DefaultWidget() { Type = "Container" };


        }
    }
}
