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

                var configFile = ConfigHelpers.ReadConfigFile(inputFile);
                var distinctScopes = configFile.Settings.Select(s => s.Scope)
                    .Concat(configFile.Assets.Select(a => a.Scope))
                    .Concat(configFile.Files.Select(f => f.Scope))
                    .Distinct();

                var finalSummary = new StringBuilder();
                foreach (var scope in distinctScopes)
                {
                    var classString = ConfigHelpers.GenerateClassString(
                        excelPath: inputFile,       
                        outputFolder: outputDir,
                        outputClassName: $"{(string.IsNullOrWhiteSpace(scope) ? "" : scope)}Config",
                        scope: scope,
                        namespaceName: ns,
                        additionalUsings: usings
                    );
                    var outputPath = Path.Combine(outputDir, $"{(string.IsNullOrWhiteSpace(scope) ? "Config" : scope)}.cs");
                    File.Delete(outputPath);// Ensure we overwrite any existing file
                    File.WriteAllText(outputPath, classString);
                    finalSummary.AppendLine($"Generated class for scope '{scope}' at: {outputPath}");
                    finalSummary.AppendLine();
                    finalSummary.AppendLine($" - {configFile.Settings.Count(s => s.Scope == scope)} Settings");
                    finalSummary.AppendLine($" - {configFile.Assets.Count(a => a.Scope == scope)} Assets");
                    finalSummary.AppendLine($" - {configFile.Files.Count(f => f.Scope == scope)} Files");
                    finalSummary.AppendLine(new string('-', 40));
                }


                System.Windows.MessageBox.Show(finalSummary.ToString(), "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred while generating the config classes:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;

        }
    }
}
