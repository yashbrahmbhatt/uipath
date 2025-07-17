using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Yash.Config.Activities
{
    public class GenerateConfigClasses : CodedWorkflow
    {
        [Workflow]
        public void Execute(
        string in_str_FilePath,
        Dictionary<string, List<string>> in_dict_Variations,
        List<string> in_list_AdditionalUsings,
        string in_str_OutputFolder,
        string in_str_NamespaceName
    )
        {
            in_str_NamespaceName = in_str_NamespaceName ?? Path.GetDirectoryName(Directory.GetCurrentDirectory()) + ".Models.Configs";
            in_str_OutputFolder = in_str_OutputFolder ?? Path.Combine(Path.GetDirectoryName(Directory.GetCurrentDirectory()), "Models", "Configs");
            in_list_AdditionalUsings = in_list_AdditionalUsings ?? new List<string>();
            var configs = workflows.ExcelToConfigFileDictionary(in_str_FilePath);

            foreach (var variation in in_dict_Variations)
            {
                var outputClassName = variation.Key + "Config";

                var relevantConfigs = variation.Value
                    .Where(configs.ContainsKey)
                    .Select(key => new { Key = key, Config = configs[key] })
                    .ToList();

                var usedTypes = new HashSet<string>();
                var sb = new StringBuilder();
                var body = new StringBuilder();

                foreach (var entry in relevantConfigs)
                {
                    if (entry.Config.Settings.Any())
                    {
                        body.AppendLine($"\n\t\t// *** Settings from {entry.Key} ***");
                        foreach (var setting in entry.Config.Settings)
                        {
                            string typeName = setting.Type;
                            usedTypes.Add(typeName);

                            if (!string.IsNullOrWhiteSpace(setting.Description))
                                body.AppendLine($"\n\t\t/// <summary>\n\t\t/// {EscapeXml(setting.Description)}\n\t\t/// </summary>");

                            body.AppendLine($"\t\tpublic {typeName} {SanitizeName(setting.Name)} {{ get; set; }}");
                        }
                    }

                    if (entry.Config.Assets.Any())
                    {
                        body.AppendLine($"\n\t\t// *** Assets from {entry.Key} ***");
                        foreach (var asset in entry.Config.Assets)
                        {
                            string typeName = asset.Type;
                            usedTypes.Add(typeName);

                            if (!string.IsNullOrWhiteSpace(asset.Description))
                                body.AppendLine($"\n\t\t/// <summary>\n\t\t/// {EscapeXml(asset.Description)}\n\t\t/// </summary>");

                            body.AppendLine($"\t\tpublic {typeName} {SanitizeName(asset.Name)} {{ get; set; }}");
                        }
                    }

                    if (entry.Config.Files.Any())
                    {
                        body.AppendLine($"\n\t\t// *** Files from {entry.Key} *** ");
                        foreach (var file in entry.Config.Files)
                        {
                            string typeName = file.Type;
                            usedTypes.Add(typeName);

                            if (!string.IsNullOrWhiteSpace(file.Description))
                                body.AppendLine($"\n\t\t/// <summary>\n\t\t/// {EscapeXml(file.Description)}\n\t\t/// </summary>");

                            body.AppendLine($"\t\tpublic {typeName} {SanitizeName(file.Name)} {{ get; set; }}");
                        }
                    }
                }

                // Add necessary using directives
                sb.AppendLine("using System;");
                sb.AppendLine("using Yash.Config.Activities.Models");
                foreach (var additional in in_list_AdditionalUsings)
                {
                    sb.AppendLine($"using {additional};");
                }
                if (usedTypes.Any(t => t.StartsWith("List<")))
                    sb.AppendLine("using System.Collections.Generic;");
                if (usedTypes.Any(t => t.Contains("DataTable")))
                    sb.AppendLine("using System.Data;");

                sb.AppendLine();
                sb.AppendLine($"namespace {in_str_NamespaceName}");
                sb.AppendLine("{");
                sb.AppendLine($"\tpublic class {outputClassName} : Config");
                sb.AppendLine("\t{");
                sb.Append(body.ToString());
                sb.AppendLine("\t}");
                sb.AppendLine("}");

                var filePath = Path.Combine(in_str_OutputFolder, outputClassName + ".cs");
                File.WriteAllText(filePath, sb.ToString());
            }
        }

        private string SanitizeName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "Unnamed";

            var validChars = new string(rawName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (char.IsDigit(validChars.FirstOrDefault()))
                validChars = "_" + validChars;

            return validChars;
        }
        private string EscapeXml(string input)
        {
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

    }
}