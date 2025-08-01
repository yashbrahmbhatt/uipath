﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yash.Config.Models;

namespace Yash.Config.Helpers
{
    public static class ConfigHelpers
    {
        public static ConfigFile ReadConfigFile(string path)
        {
            var dataset = ExcelHelpers.ReadExcelFile(path);
            var settingsSheet = dataset.Tables.Contains("Settings") ? dataset.Tables["Settings"] : null;
            var assetsSheet = dataset.Tables.Contains("Assets") ? dataset.Tables["Assets"] : null;
            var filesSheet = dataset.Tables.Contains("Files") ? dataset.Tables["Files"] : null;

            ConfigFile configFile = new()
            {
                Settings = settingsSheet == null ? new() : JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(settingsSheet)) ?? new(),
                Assets = assetsSheet == null ? new() : JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(assetsSheet)) ?? new(),
                Files = filesSheet == null ? new() : JsonConvert.DeserializeObject<List<ConfigFileItem>>(JsonConvert.SerializeObject(filesSheet)) ?? new(),
            };
            return configFile;
        }

        public static string GenerateClassString(
            string excelPath,
            string outputClassName,
            string outputFolder,
            string namespaceName,
            string additionalUsings = ""
        )
        {
            // Load the config (you'll need a method like ExcelToConfigFile)
            ConfigFile config = ReadConfigFile(excelPath);

            var usedTypes = new HashSet<string>();
            var sb = new StringBuilder();
            var body = new StringBuilder();
            void AppendProperty(string typeName, string name, string description)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    usedTypes.Add(typeName);

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        body.AppendLine($"\n\t\t/// <summary>");
                        body.AppendLine($"\t\t/// {EscapeXml(description)}");
                        body.AppendLine($"\t\t/// </summary>");
                    }

                    body.AppendLine($"\t\tpublic {(string.IsNullOrWhiteSpace(typeName) ? "string" : typeName)} {SanitizeName(name)} {{ get; set; }}");
                }
            }

            // Settings
            if (config.Settings.Any())
            {
                body.AppendLine($"\n\t\t#region Settings");
                foreach (var setting in config.Settings)
                    AppendProperty(setting.Type, setting.Name, setting.Description);
                body.AppendLine($"\n\t\t#endregion");
            }

            // Assets
            if (config.Assets.Any())
            {
                body.AppendLine($"\n\t\t#region Assets");
                foreach (var asset in config.Assets)
                    AppendProperty(asset.Type, asset.Name, asset.Description);
                body.AppendLine($"\n\t\t#endregion");

            }

            // Files
            if (config.Files.Any())
            {
                body.AppendLine($"\n\t\t#region Files");
                foreach (var file in config.Files)
                    AppendProperty(file.Type, file.Name, file.Description);
                body.AppendLine($"\n\t\t#endregion");

            }


            // Usings
            sb.AppendLine("using System;");
            sb.AppendLine("using Yash.Config.Models;");
            if (usedTypes.Any(t => t.StartsWith("List<")))
                sb.AppendLine("using System.Collections.Generic;");
            if (usedTypes.Any(t => t.Contains("DataTable")))
                sb.AppendLine("using System.Data;");
            foreach (var extra in additionalUsings.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                sb.AppendLine($"using {extra};");
            sb.AppendLine();

            // Disclaimer
            sb.AppendLine("// ------------------------------------------------------------------------------");
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine($"//     This code was generated by the Yash.Config GenerateConfigClass wizard on {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}.");
            sb.AppendLine("//");
            sb.AppendLine("//     Changes to this file may be overwritten if the tool is rerun.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("// ------------------------------------------------------------------------------");
            sb.AppendLine();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic class {outputClassName} : Yash.Config.Models.Config");
            sb.AppendLine("\t{");
            sb.Append(body.ToString());
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }



        public static string SanitizeName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return "Unnamed";

            var validChars = new string(rawName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (char.IsDigit(validChars.FirstOrDefault()))
                validChars = "_" + validChars;

            return validChars;
        }

        public static string EscapeXml(string input)
        {
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}
