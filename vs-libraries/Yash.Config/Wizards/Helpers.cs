using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiPath.Studio.Activities.Api;
using Yash.Config.Helpers;

namespace Yash.Config.Wizards
{
    public static class Helpers
    {
        public static bool EnsureOrPromptSettings(IWorkflowDesignApi api, bool forcePrompt, out string inputFile, out string outputDirectory, out string namespaceName, out string additionalUsings)
        {
            inputFile = outputDirectory = namespaceName = additionalUsings = "";
            var projectDir = api.ProjectPropertiesService.GetProjectDirectory();
            var projectName = api.ProjectPropertiesService.GetProjectName();

            // INPUT FILE
            if (!forcePrompt)
            {
                if (!TryGetOrCreateInputFilePath(api, out inputFile)) return false;
            }
            else
            {
                System.Windows.MessageBox.Show("Please select the configuration file for generating config classes.", "Select Configuration File", MessageBoxButton.OK, MessageBoxImage.Information);
                inputFile = FileDialogHelpers.PromptForFile("Select your configuration file", "Excel Files (*.xlsx)|*.xlsx", projectDir);
                if (string.IsNullOrWhiteSpace(inputFile)) return false;
                api.Settings.TrySetValue(Settings.Keys.Setting_Generation_FilePath_Key, inputFile);
            }

            // OUTPUT DIRECTORY
            if (!forcePrompt && api.Settings.TryGetValue(Settings.Keys.Setting_Generation_OutputDir_Key, out outputDirectory) &&
                !string.IsNullOrWhiteSpace(outputDirectory) && Directory.Exists(outputDirectory))
            {
                // Valid existing setting
            }
            else
            {
                System.Windows.MessageBox.Show("Please select the output directory for the generated classes.", "Select Output Directory", MessageBoxButton.OK, MessageBoxImage.Information);
                outputDirectory = FileDialogHelpers.PromptForDirectory("Select the output directory for generated classes", projectDir);
                if (string.IsNullOrWhiteSpace(outputDirectory)) return false;
                api.Settings.TrySetValue(Settings.Keys.Setting_Generation_OutputDir_Key, outputDirectory);
            }

            var relativePath = Path.GetRelativePath(projectDir, outputDirectory);
            var defaultNamespace = $"{projectName}.{relativePath.Replace("\\", ".")}";

            // NAMESPACE
            if (!forcePrompt && api.Settings.TryGetValue(Settings.Keys.Setting_Generation_Namespace_Key, out namespaceName) &&
                !string.IsNullOrWhiteSpace(namespaceName))
            {
                // Valid existing setting
            }
            else
            {
                namespaceName = InputBoxHelpers.PromptForText("Namespace Name", "Enter the namespace for the generated class", defaultNamespace);
                if (string.IsNullOrWhiteSpace(namespaceName)) return false;
                api.Settings.TrySetValue(Settings.Keys.Setting_Generation_Namespace_Key, namespaceName);
            }

            // USINGS
            if (!forcePrompt && api.Settings.TryGetValue(Settings.Keys.Setting_Generation_Usings_Key, out additionalUsings) &&
                !string.IsNullOrWhiteSpace(additionalUsings))
            {
                // Valid existing setting
            }
            else
            {
                additionalUsings = InputBoxHelpers.PromptForText("Additional Usings", "Enter any additional usings, separated by commas", "System,System.Collections.Generic");
                api.Settings.TrySetValue(Settings.Keys.Setting_Generation_Usings_Key, additionalUsings);
            }

            return true;
        }



        public static bool TryGetOrCreateInputFilePath(IWorkflowDesignApi api, out string inputFilePath)
        {
            inputFilePath = "";
            var projectDir = api.ProjectPropertiesService.GetProjectDirectory();

            if (api.Settings.TryGetValue<string>(Settings.Keys.Setting_Generation_FilePath_Key, out var configuredPath) &&
                !string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
            {
                inputFilePath = configuredPath;
                return true;
            }

            var choice = FileDialogHelpers.PromptYesNoCancel(
                "Select Configuration Source",
                "No config file is currently set.\n\nWould you like to select an existing file (Yes) or create a new one from a template (No)?"
            );

            if (choice == null)
                return false;

            if (choice == true)
            {
                inputFilePath = FileDialogHelpers.PromptForFile("Select your configuration file", "Excel Files (*.xlsx)|*.xlsx", projectDir);
            }
            else
            {
                var savePath = FileDialogHelpers.PromptForSaveFile("Create new configuration file", "Excel Files (*.xlsx)|*.xlsx", "Configuration.xlsx");
                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    var resourceName = "Yash.Configuration.ConfigTemplate.xlsx"; // Use the correct namespace + file path
                    using var resourceStream = typeof(ConfigWizards).Assembly
                        .GetManifestResourceStream(resourceName);

                    if (resourceStream == null)
                    {
                        System.Windows.MessageBox.Show("Template resource not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    using var fileStream = File.Create(savePath);
                    resourceStream.CopyTo(fileStream);
                    inputFilePath = savePath;
                }
            }

            if (string.IsNullOrWhiteSpace(inputFilePath))
                return false;

            api.Settings.TrySetValue(Settings.Keys.Setting_Generation_FilePath_Key, inputFilePath);
            return true;
        }
    }
}
