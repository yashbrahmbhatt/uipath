using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;

namespace Yash.Config.Wizards
{
    public static class ConfigureSettingsWizard
    {
        public static Activity? Run(IWorkflowDesignApi api)
        {
            try
            {
                if (!Helpers.EnsureOrPromptSettings(api, forcePrompt: true, out _, out _, out _, out _, out _))
                    return null;

                System.Windows.MessageBox.Show("Settings have been successfully updated.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred during configuration:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
    }
}
