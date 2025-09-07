using Newtonsoft.Json;
using System;
using System.Activities.Presentation.Metadata;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Widgets;
using Yash.Frameworks.Activities;
using Yash.Frameworks.Activities.ViewModels;
using Yash.Frameworks.Settings;

namespace Yash.Frameworks
{
    public class RegisterMetadata : IRegisterMetadata
    {

        public void Register()
        {
            var builder = new AttributeTableBuilder();
            //builder.AddCustomAttributes(typeof(LazyFrameworkV2), new DesignerAttribute(typeof(LazyFrameworkV2Designer)));
            //CategoryAttribute cat = new CategoryAttribute("Framework.Performers");
            //builder.AddCustomAttributes(typeof(LazyFrameworkV2), cat);

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
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
