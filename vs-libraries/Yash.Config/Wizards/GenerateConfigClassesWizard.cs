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
                if (!Helpers.EnsureOrPromptSettings(api, forcePrompt: false, out var inputFile, out var outputDir, out var ns, out var className, out var usings))
                    return null;

                var configFile = ConfigHelpers.ReadConfigFile(inputFile);
                var classString = ConfigHelpers.GenerateClassString(
                    excelPath: inputFile,
                    outputClassName: className,
                    outputFolder: outputDir,
                    namespaceName: ns,
                    additionalUsings: usings
                );

                var outputPath = Path.Combine(outputDir, $"{className}.cs");
                File.Delete(outputPath);// Ensure we overwrite any existing file
                File.WriteAllText(outputPath, classString);

                System.Windows.MessageBox.Show(
                    $"Config class generated at:\n" +
                    $"\"{outputPath}\"\n" +
                    $"{configFile.Settings.Count} Settings\n" +
                    $"{configFile.Assets.Count} Assets\n" +
                    $"{configFile.Files.Count} Files\n", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred while generating the config classes:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;

        }
    }
}
