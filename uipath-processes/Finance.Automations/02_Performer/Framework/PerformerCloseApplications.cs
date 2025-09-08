using System;
using System.Collections.Generic;
using System.Data;
using Finance.Automations.Configs;
using UiPath.CodedWorkflows;

namespace Finance.Automations._02_Performer.Framework
{
    public class PerformerCloseApplications : CodedWorkflow
    {
        [Workflow]
        public void Execute(
            )
        {
            // To start using services, use IntelliSense (CTRL + Space) to discover the available services:
            // e.g. system.GetAsset(...)

            // For accessing UI Elements from Object Repository, you can use the Descriptors class e.g:
            // var screen = uiAutomation.Open(Descriptors.MyApp.FirstScreen);
            // screen.Click(Descriptors.MyApp.FirstScreen.SettingsButton);
        }
    }
}