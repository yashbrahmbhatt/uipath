using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;
using Yash.Config.Helpers;

namespace Yash.Config.Wizards
{
    public static class GenerateConfigClassesWizard
    {
        public static Activity? Run(IWorkflowDesignApi api)
        {
            try
            {
                if (!Helpers.EnsureOrPromptSettings(api, forcePrompt: false, out var inputFile, out var outputDir, out var ns, out var usings))
                    return null;

                var finalSummary = ConfigService.GenerateClassFiles(inputFile, outputDir, ns, usings, null);


                System.Windows.MessageBox.Show(finalSummary, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred while generating the config classes:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;

        }
    }
}
