using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using UiPath.Studio.Activities.Api.Wizards;
using UiPath.Studio.Activities.Api;
using System.Activities;
using Yash.Config.Helpers;
using Yash.Config.Models;
using Newtonsoft.Json;
using UiPath.Studio.Activities.Api.BusyService;
using ClosedXML.Excel;

namespace Yash.Config.Wizards
{
    public static class ConfigWizards
    {
        public static IWorkflowDesignApi api;
        public static void CreateWizard(IWorkflowDesignApi workflowDesignApi)
        {
            api = workflowDesignApi;
            var wizardDefinition = new WizardDefinition()
            {
                Wizard = new WizardBase()
                {
                },
                DisplayName = "Config",
                IconUri = "Icons/RecordIcon",
                Tooltip = "A set of wizards to help with managing configurations in UiPath",
            };
            wizardDefinition.ChildrenDefinitions.Add(new WizardDefinition()
            {
                DisplayName = "Generate Config Classes",
                Shortcut = new KeyGesture(Key.F9, ModifierKeys.Control | ModifierKeys.Shift),
                Tooltip = "Generates hardly typed classes for your configuration given the input config file and output directory.",
                Wizard = new WizardBase()
                {
                    RunWizard = () => GenerateConfigClassesWizard.Run(api),
                }
            });
            wizardDefinition.ChildrenDefinitions.Add(new WizardDefinition()
            {
                DisplayName = "Configure Class Generation Settings",
                Shortcut = new KeyGesture(Key.F10, ModifierKeys.Control | ModifierKeys.Shift),
                Tooltip = "Re-initialize the config class generation wizard settings manually.",
                Wizard = new WizardBase()
                {
                    RunWizard = () => ConfigureSettingsWizard.Run(api),
                }
            });

            var collection = new WizardCollection(); //Use a collection to group all of your wizards.
            collection.WizardDefinitions.Add(wizardDefinition);
            workflowDesignApi.Wizards.Register(collection);
        }
        


        

    }
    
}