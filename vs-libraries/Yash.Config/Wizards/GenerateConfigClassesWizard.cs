using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;
using Yash.Config.Helpers;
using Yash.Config.Services;
using Microsoft.Extensions.DependencyInjection;
using Yash.Orchestrator;

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

                // Create a simple service provider for the wizard
                var serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
                if (api.AccessProvider != null)
                {
                    serviceCollection.AddSingleton<IOrchestratorService>(provider => new OrchestratorService(api.AccessProvider));
                }
                var serviceProvider = serviceCollection.BuildServiceProvider();
                
                var service = new ConfigService(inputFile, serviceProvider);
                var finalSummary = service.GenerateClassFiles(outputDir, ns, usings);


                System.Windows.MessageBox.Show(finalSummary, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred while generating the config classes:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;

        }
    }
}
