using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;
using Yash.Config.Activities;
using Yash.Config.Activities.ViewModels;
using Yash.Config.Settings;
using Yash.Config.Wizards;

namespace Yash.Config
{
    public class RegisterMetadata : IRegisterMetadata
    {

        public void Register()
        {
            var builder = new AttributeTableBuilder();
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        /// <summary>
        /// This method is discovered in Studio using reflection.
        /// If found, a reference to the studio api is passed
        public void Initialize(object argument)
        {
            try
            {
                var api = argument as IWorkflowDesignApi;
                if (api == null)
                {
                    return;
                }
                if (api.HasFeature(DesignFeatureKeys.Settings))
                {
                    ConfigSettings.CreateSettings(api);
                }
                if (api.HasFeature(DesignFeatureKeys.Wizards))
                {
                    ConfigWizards.CreateWizard(api);
                }
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
