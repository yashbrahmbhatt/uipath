using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Yash.Config.Activities;
using Yash.Config.Helpers;
using Yash.Config.Models;
using Yash.Orchestrator;
using Yash.Utility.Helpers;

namespace Yash.Config
{
    public static class ConfigService
    {
        public static async Task<Dictionary<string, object>> LoadConfigAsync(ConfigFile configFile, string scope, string baseUrl, string clientId, SecureString clientSecret, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(baseUrl, clientId, new System.Net.NetworkCredential("", clientSecret).Password, new string[] { "OR.Folders.Read", "OR.Assets.Read" }, (msg, level) => log?.Invoke(msg, level));
            await _orchestratorService.InitializeAsync();


            Dictionary<string, object> outputDict = new();

            foreach (var setting in configFile.Settings)
            {
                if (string.IsNullOrWhiteSpace(setting.Name))
                {
                    Log("Setting with empty name found, skipping.", TraceEventType.Warning);
                    continue;
                }
                if (setting.Scope != null && scope != null && setting.Scope != scope)
                {
                    Log($"Setting '{setting.Name}' skipped due to scope mismatch (Setting Scope: '{setting.Scope}', Provided Scope: '{scope}').", TraceEventType.Verbose);
                    continue;
                }

                outputDict[setting.Name] = setting.Value;
                Log($"Setting '{setting.Name}' loaded with value '{setting.Value}'.", TraceEventType.Verbose);
            }

            foreach (var asset in configFile.Assets)
            {
                if (!string.IsNullOrWhiteSpace(asset.Name))
                {
                    if (asset.Scope != null && scope != null && asset.Scope != scope)
                    {
                        Log($"Asset '{asset.Name}' skipped due to scope mismatch (Asset Scope: '{asset.Scope}', Provided Scope: '{scope}').", TraceEventType.Verbose);
                        continue;
                    }
                    var folderAssets = _orchestratorService.Assets.FirstOrDefault(kvp => kvp.Key.DisplayName == asset.Folder);
                    if (folderAssets.Key == null) throw new LoadConfigException($"Folder '{asset.Folder}' not found in orchestrator.");

                    if (folderAssets.Value.Any(a => a.Name == asset.Value))
                    {
                        var foundAsset = folderAssets.Value.FirstOrDefault(a => a.Name == asset.Value);
                        if (foundAsset?.Value != null)
                        {
                            outputDict[asset.Name] = foundAsset.Value;
                            Log($"Found existing asset '{asset.Value}' in folder '{asset.Folder}' with value {outputDict[asset.Name]}.");
                        }
                        else
                        {
                            Log($"Asset '{asset.Value}' found in folder '{asset.Folder}' but has null value.", TraceEventType.Warning);
                        }
                    }
                    else
                    {
                        throw new LoadConfigException($"Asset '{asset.Value}' not found in folder '{asset.Folder}'.", TraceEventType.Warning);
                    }
                }
                else
                {
                    Log($"Asset with empty name found in folder '{asset.Folder}'.", TraceEventType.Warning);
                }
            }

            foreach (var file in configFile.Files)
            {
                if (string.IsNullOrWhiteSpace(file.Name))
                {
                    Log("File entry skipped: missing Name.", TraceEventType.Warning);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(file.Bucket) && !string.IsNullOrWhiteSpace(file.Folder))
                {
                    if (file.Scope != null && scope != null && file.Scope != scope)
                    {
                        Log($"File '{file.Name}' skipped due to scope mismatch (File Scope: '{file.Scope}', Provided Scope: '{scope}').", TraceEventType.Verbose);
                        continue;
                    }
                    // Remote Storage Bucket file
                    Log($"[Storage Bucket] Resolving file '{file.Name}' in bucket '{file.Bucket}' and folder '{file.Folder}' with path '{file.Path}'.", TraceEventType.Information);

                    var folderEntry = _orchestratorService.Buckets.FirstOrDefault(kvp => kvp.Key.DisplayName == file.Folder);
                    if (folderEntry.Key == null)
                    {
                        throw new LoadConfigException($"Folder '{file.Folder}' not found for file '{file.Name}'.", TraceEventType.Error);
                    }

                    var bucket = folderEntry.Value.FirstOrDefault(b => b.Name == file.Bucket);
                    if (bucket == null)
                    {
                        throw new LoadConfigException($"Bucket '{file.Bucket}' not found in folder '{file.Folder}' for file '{file.Name}'.", TraceEventType.Error);
                    }

                    var bucketFiles = _orchestratorService.BucketFiles.FirstOrDefault(kvp => kvp.Key == bucket);
                    if (bucketFiles.Value == null)
                    {
                        throw new LoadConfigException($"No files found in bucket '{bucket.Name}' for file '{file.Name}'.", TraceEventType.Warning);
                    }

                    var match = bucketFiles.Value.FirstOrDefault(f => f.FullPath == file.Path);
                    if (match == null)
                    {
                        throw new LoadConfigException($"File '{file.Path}' not found in bucket '{bucket.Name}'.", TraceEventType.Warning);
                    }
                    else
                    {
                        Log($"Resolved bucket file '{file.Name}' → FullPath: '{match.FullPath}'", TraceEventType.Verbose);
                        var tempPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(file.Path));
                        await _orchestratorService.DownloadBucketFile(bucket, match, tempPath);
                        Log($"Downloaded bucket file '{file.Name}' to local path '{tempPath}'.", TraceEventType.Verbose);
                        outputDict[file.Name] = await ParseConfigFileItemAsync(tempPath, file.Type, file.Name, log);
                        Log($"Parsed bucket file '{file.Name}' with type '{file.Type}' and stored in output dictionary.", TraceEventType.Verbose);
                        File.Delete(tempPath); // Clean up temporary file
                    }
                }
                else if (!string.IsNullOrWhiteSpace(file.Path))
                {
                    // Optionally check for existence:
                    if (!System.IO.File.Exists(file.Path))
                        throw new FileNotFoundException($"Local file '{file.Path}' for '{file.Name}' does not exist.");

                    Log($"[Local] Found file '{file.Name}' at local path '{file.Path}'.", TraceEventType.Verbose);
                    outputDict[file.Name] = await ParseConfigFileItemAsync(file.Path, file.Type, file.Name, log);
                }
                else
                {
                    throw new LoadConfigException($"File '{file.Name}' is missing a path", TraceEventType.Warning);
                }
            }
            Log($"Configuration loaded successfully with {outputDict.Count} entries.", TraceEventType.Information);
            return outputDict;
        }
        public static async Task<object> ParseConfigFileItemAsync(string filePath, string type, string name, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            switch (type.ToLowerInvariant())
            {
                case "csv":
                    var table = CSVHelpers.Parse(filePath, filePath, (msg, level) => log?.Invoke(msg, level));
                    Log($"Hydrated CSV file '{name}' with {table.Columns.Count} columns and {table.Rows.Count} rows.");
                    return JsonConvert.SerializeObject(table);

                case "xlsx":
                    var ds = Yash.Utility.Helpers.ExcelHelpers.ReadExcelFile(filePath);
                    Log($"Hydrated Excel file '{name}' with {ds.Tables.Count} tables.");
                    return JsonConvert.SerializeObject(ds);

                default:
                    var content = await File.ReadAllTextAsync(filePath);
                    Log($"Read raw contents of file '{name}'.");
                    return content;
            }
        }
        public static ConfigFile ReadConfigFile(string filePath, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            var dataset = Yash.Utility.Helpers.ExcelHelpers.ReadExcelFile(filePath);
            Log($"Found {dataset.Tables.Count} sheets in workbook.");
            var settingsSheet = dataset.Tables.Contains("Settings") ? dataset.Tables["Settings"] : null;
            var assetsSheet = dataset.Tables.Contains("Assets") ? dataset.Tables["Assets"] : null;
            var filesSheet = dataset.Tables.Contains("Files") ? dataset.Tables["Files"] : null;
            Log($"Sheet existences: Settings={settingsSheet != null}, Assets={assetsSheet != null}, Files={filesSheet != null}");

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
            string scope = "",
            string additionalUsings = "",
            Action<string, TraceEventType>? log = null
        )
        {
            // Load the config (you'll need a method like ExcelToConfigFile)
            ConfigFile config = ReadConfigFile(excelPath, log);
            if (scope != "")
            {
                config = new ConfigFile()
                {
                    Settings = config.Settings.Where(s => s.Scope == scope).ToList(),
                    Assets = config.Assets.Where(a => a.Scope == scope).ToList(),
                    Files = config.Files.Where(f => f.Scope == scope).ToList()
                };
            }
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

        public static string GenerateClassFiles(string inputFile, string outputDir, string ns, string usings, Action<string, TraceEventType>? log = null)
        {
            var configFile = ReadConfigFile(inputFile);
            var distinctScopes = configFile.Settings.Select(s => s.Scope)
                .Concat(configFile.Assets.Select(a => a.Scope))
                .Concat(configFile.Files.Select(f => f.Scope))
                .Distinct();

            var finalSummary = new StringBuilder();
            foreach (var scope in distinctScopes)
            {
                var classString = GenerateClassString(inputFile, scope + "Config", outputDir, ns, scope, usings);
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
            return finalSummary.ToString();
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
