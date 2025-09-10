using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UiPath.Activities.Api.Base;
using UiPath.Core.Activities;
using UiPath.Studio.Activities.Api;
using Yash.Config.Activities;
using Yash.Config.Helpers;
using Yash.Config.Models;
using Yash.Orchestrator;
using Yash.Utility.Helpers;

namespace Yash.Config
{
    public static class ConfigService
    {
        public class LoadConfigWithMetadata
        {
            public Dictionary<string, object> Config { get; set; } = new();
            public Dictionary<string, Dictionary<string, object>> ConfigByScope { get; set; } = new();
            public Dictionary<string, ConfigFile> ConfigFileByScope => Metadata.ConfigByScope;
            public ConfigFileMetadata Metadata { get; set; } = new();
        }
        public static async Task<LoadConfigWithMetadata> LoadConfigWithOrchestratorServiceAsync(ConfigFileMetadata meta, string scope, OrchestratorService _orchestratorService, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) { if (level <= minLogLevel) log?.Invoke(msg, level); }
            if (_orchestratorService == null) throw new ArgumentNullException(nameof(_orchestratorService), "OrchestratorService instance cannot be null.");
            var result = new LoadConfigWithMetadata
            {
                Metadata = meta,
                ConfigByScope = new(),
            };
            try
            {

                await _orchestratorService.InitializeAsync();
                Log($"Scopes available in metadata: {string.Join(", ", meta.Scopes)}", TraceEventType.Verbose);

                var configFile = meta.ConfigByScope.ContainsKey(scope) ? meta.ConfigByScope[scope] : meta.ConfigFile;

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

                result.Config = outputDict;
                result.ConfigByScope[scope] = outputDict;
            }
            catch (Exception ex)
            {
                Log($"[LoadConfigWithOrchestratorServiceAsync] Exception: {ex.Message}", TraceEventType.Error);
                throw;
            }

            return result;
        }
        public static async Task<LoadConfigWithMetadata> LoadConfigWithClientCredentialsAsync(ConfigFileMetadata meta, string scope, string baseUrl, string clientId, SecureString clientSecret, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(baseUrl, clientId, new System.Net.NetworkCredential("", clientSecret).Password, new string[] { "OR.Folders.Read", "OR.Assets.Read" }, (msg, level) => log?.Invoke(msg, level));

            return await LoadConfigWithOrchestratorServiceAsync(meta, scope, _orchestratorService, Log);
        }

        public static async Task<LoadConfigWithMetadata> LoadConfigWithClientCredentialsAsync(ConfigFileMetadata meta, string scope, string baseUrl, string clientId, SecureString clientSecret, string[] scopes, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(baseUrl, clientId, new System.Net.NetworkCredential("", clientSecret).Password, scopes, (msg, level) => log?.Invoke(msg, level));

            return await LoadConfigWithOrchestratorServiceAsync(meta, scope, _orchestratorService, Log);
        }

        public static async Task<LoadConfigWithMetadata> LoadConfigWithAccessProviderAsync(ConfigFileMetadata meta, string scope, IAccessProvider accessProvider, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            try
            {

                OrchestratorService _orchestratorService = new(accessProvider, log, minLogLevel);

                return await LoadConfigWithOrchestratorServiceAsync(meta, scope, _orchestratorService, Log);

            }
            catch (Exception ex)
            {
                Log($"[LoadConfigWithAccessProviderAsync] Exception: {ex.Message}", TraceEventType.Error);
                throw;
            }
        }
        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithOrchestratorServiceAsync(ConfigFileMetadata meta, string scope, OrchestratorService orchestratorService, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithOrchestratorServiceAsync(meta, scope, orchestratorService, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithOrchestratorServiceAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithClientCredentialsAsync(ConfigFileMetadata meta, string scope, string baseUrl, string clientId, SecureString clientSecret, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithClientCredentialsAsync(meta, scope, baseUrl, clientId, clientSecret, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithClientCredentialsAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithClientCredentialsAsync(ConfigFileMetadata meta, string scope, string baseUrl, string clientId, SecureString clientSecret, string[] scopes, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithClientCredentialsAsync(meta, scope, baseUrl, clientId, clientSecret, scopes, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithClientCredentialsAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithAccessProviderAsync(ConfigFileMetadata meta, string scope, IAccessProvider accessProvider, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithAccessProviderAsync(meta, scope, accessProvider, Log, minLogLevel);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithAccessProviderAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        // New overload methods that take file path and include validation
        public static async Task<LoadConfigWithMetadata> LoadConfigWithOrchestratorServiceAsync(string configFilePath, string scope, OrchestratorService orchestratorService, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            var result = new LoadConfigWithMetadata();

            // Validate the config file first
            result.Metadata = ValidateConfigFile(configFilePath, log);

            if (result.Metadata.ConfigFileError != null)
            {
                Log($"Config file validation failed: {result.Metadata.ConfigFileError}", TraceEventType.Error);
                throw new LoadConfigException($"Config file validation failed: {result.Metadata.ConfigFileError}");
            }

            // Read the config file

            // Load the configuration
            var loadResult = await LoadConfigWithOrchestratorServiceAsync(result.Metadata, scope, orchestratorService, log);
            result.Config = loadResult.Config;

            return result;
        }

        public static async Task<LoadConfigWithMetadata> LoadConfigWithClientCredentialsAsync(string configFilePath, string scope, string baseUrl, string clientId, SecureString clientSecret, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(baseUrl, clientId, new System.Net.NetworkCredential("", clientSecret).Password, new string[] { "OR.Folders.Read", "OR.Assets.Read" }, (msg, level) => log?.Invoke(msg, level));

            return await LoadConfigWithOrchestratorServiceAsync(configFilePath, scope, _orchestratorService, Log);
        }

        public static async Task<LoadConfigWithMetadata> LoadConfigWithClientCredentialsAsync(string configFilePath, string scope, string baseUrl, string clientId, SecureString clientSecret, string[] scopes, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(baseUrl, clientId, new System.Net.NetworkCredential("", clientSecret).Password, scopes, (msg, level) => log?.Invoke(msg, level));

            return await LoadConfigWithOrchestratorServiceAsync(configFilePath, scope, _orchestratorService, Log);
        }

        public static async Task<LoadConfigWithMetadata> LoadConfigWithAccessProviderAsync(string configFilePath, string scope, IAccessProvider accessProvider, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);

            OrchestratorService _orchestratorService = new(accessProvider, (msg, level) => log?.Invoke(msg, level));

            return await LoadConfigWithOrchestratorServiceAsync(configFilePath, scope, _orchestratorService, Log);
        }

        // Try versions with file path validation
        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithOrchestratorServiceAsync(string configFilePath, string scope, OrchestratorService orchestratorService, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithOrchestratorServiceAsync(configFilePath, scope, orchestratorService, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithOrchestratorServiceAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithClientCredentialsAsync(string configFilePath, string scope, string baseUrl, string clientId, SecureString clientSecret, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithClientCredentialsAsync(configFilePath, scope, baseUrl, clientId, clientSecret, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithClientCredentialsAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithClientCredentialsAsync(string configFilePath, string scope, string baseUrl, string clientId, SecureString clientSecret, string[] scopes, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithClientCredentialsAsync(configFilePath, scope, baseUrl, clientId, clientSecret, scopes, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithClientCredentialsAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
        }

        public static async Task<LoadConfigWithMetadata?> TryLoadConfigWithAccessProviderAsync(string configFilePath, string scope, IAccessProvider accessProvider, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            try
            {
                return await LoadConfigWithAccessProviderAsync(configFilePath, scope, accessProvider, Log);
            }
            catch (Exception ex)
            {
                Log($"[TryLoadConfigWithAccessProviderAsync] Failed to load config: {ex.Message}", TraceEventType.Error);
                return null;
            }
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

        public static string GenerateClassString(
            ConfigFile config,
            string outputClassName,
            string outputFolder,
            string namespaceName,
            string additionalUsings = "",
            Action<string, TraceEventType>? log = null
        )
        {
            // Load the config (you'll need a method like ExcelToConfigFile)

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
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            var meta = ValidateConfigFile(inputFile, Log);


            var finalSummary = new StringBuilder();
            foreach (var scope in meta.ConfigByScope.Keys)
            {
                var configFile = meta.ConfigByScope[scope];
                var classString = GenerateClassString(configFile, scope + "Config", outputDir, ns, usings);
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


        public class ConfigFileMetadata
        {
            public string FilePath { get; set; } = "";
            public int SheetCount { get; set; } = 0;
            public ConfigFileError? ConfigFileError { get; set; } = null;
            public List<SheetMetadata> Sheets { get; set; } = new();
            public ConfigFile ConfigFile { get; set; } = new();
            public List<string> Scopes { get; set; } = new();
            public Dictionary<string, ConfigFile> ConfigByScope { get; set; } = new();
        }
        public enum ConfigFileError
        {
            FileNotFound,
            NotExcelFile,
            NullValue
        }
        /// <summary>
        /// Validates if an Excel file is a valid config file with required sheets and headers.
        /// </summary>
        /// <param name="filePath">Path to the Excel file to validate.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        /// <returns>ConfigFileValidationResult containing validation details.</returns>
        public static ConfigFileMetadata ValidateConfigFile(string filePath, Action<string, TraceEventType>? Log = null)
        {
            var result = new ConfigFileMetadata
            {
                FilePath = filePath
            };
            if (string.IsNullOrEmpty(filePath))
            {
                Log?.Invoke($"[ExcelHelpers] Config file path is null or empty.", TraceEventType.Error);
                result.ConfigFileError = ConfigFileError.NullValue;
                return result;
            }

            if (!File.Exists(filePath))
            {
                Log?.Invoke($"[ExcelHelpers] Config file not found: {filePath}", TraceEventType.Error);
                result.ConfigFileError = ConfigFileError.FileNotFound;
                return result;
            }

            if (!IsExcelFile(filePath))
            {
                Log?.Invoke($"[ExcelHelpers] File is not an Excel file: {filePath}", TraceEventType.Error);
                result.ConfigFileError = ConfigFileError.NotExcelFile;
                return result;
            }

            Log?.Invoke($"[ExcelHelpers] Validating config file structure: {filePath}", TraceEventType.Information);

            try
            {
                var dataSet = ExcelHelpers.ReadExcelFile(filePath, Log);

                foreach (var table in dataSet.Tables.Cast<DataTable>())
                {
                    Log?.Invoke($"[ExcelHelpers] Found sheet: {table.TableName} with {table.Columns.Count} columns and {table.Rows.Count} rows.", TraceEventType.Verbose);
                    var configType = DetermineConfigType(table, Log);
                    result.Sheets.Add(new SheetMetadata()
                    {
                        SheetName = table.TableName,
                        ConfigType = configType,
                        Sheet = table
                    });
                    switch (configType)
                    {
                        case ConfigSheetType.Setting:
                            result.ConfigFile.Settings = result.ConfigFile.Settings.Concat(JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigSettingItem>()).ToList();
                            break;
                        case ConfigSheetType.Asset:
                            result.ConfigFile.Assets = result.ConfigFile.Assets.Concat(JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigAssetItem>()).ToList();
                            break;
                        case ConfigSheetType.File:
                            result.ConfigFile.Files = result.ConfigFile.Files.Concat(JsonConvert.DeserializeObject<List<ConfigFileItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigFileItem>()).ToList();
                            break;
                        case ConfigSheetType.NameValue:
                            result.ConfigFile.Settings = result.ConfigFile.Settings.Concat((JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigSettingItem>()).Select(nv => new ConfigSettingItem { Name = nv.Name, Value = nv.Value, Type = "string" })).ToList();
                            break;
                        case ConfigSheetType.NameValueFolder:
                            result.ConfigFile.Assets = result.ConfigFile.Assets.Concat((JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigAssetItem>()).Select(nv => new ConfigAssetItem { Name = nv.Name, Value = nv.Value, Folder = nv.Folder, Type = "string" })).ToList();
                            break;
                    }
                }
                result.ConfigFile.Settings = result.ConfigFile.Settings.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();
                result.ConfigFile.Assets = result.ConfigFile.Assets.Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Folder) && !string.IsNullOrWhiteSpace(a.Value)).ToList();
                result.ConfigFile.Files = result.ConfigFile.Files.Where(f => !string.IsNullOrWhiteSpace(f.Name) && !string.IsNullOrWhiteSpace(f.Path)).ToList();
                foreach (var setting in result.ConfigFile.Settings)
                    setting.Scope = string.IsNullOrWhiteSpace(setting.Scope) ? "Shared" : setting.Scope.Trim();
                foreach (var asset in result.ConfigFile.Assets)
                    asset.Scope = string.IsNullOrWhiteSpace(asset.Scope) ? "Shared" : asset.Scope.Trim();
                foreach (var file in result.ConfigFile.Files)
                    file.Scope = string.IsNullOrWhiteSpace(file.Scope) ? "Shared" : file.Scope.Trim();

                var usesScopes = result.ConfigFile.Settings.Any(s => !string.IsNullOrEmpty(s.Scope)) ||
                    result.ConfigFile.Assets.Any(a => !string.IsNullOrEmpty(a.Scope)) ||
                    result.ConfigFile.Files.Any(f => !string.IsNullOrEmpty(f.Scope));
                if (usesScopes)
                {
                    result.Scopes = result.ConfigFile.Settings.Select(s => s.Scope)
                        .Concat(result.ConfigFile.Assets.Select(a => a.Scope))
                        .Concat(result.ConfigFile.Files.Select(f => f.Scope))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct()
                        .ToList();
                    foreach (var scope in result.Scopes)
                    {
                        result.ConfigByScope[scope] = new ConfigFile()
                        {
                            Settings = result.ConfigFile.Settings.Where(s => s.Scope == scope).ToList(),
                            Assets = result.ConfigFile.Assets.Where(a => a.Scope == scope).ToList(),
                            Files = result.ConfigFile.Files.Where(f => f.Scope == scope).ToList()
                        };

                    }
                }
                Log?.Invoke($"[ExcelHelpers] Config file validation completed successfully. Found {result.Sheets.Count} sheets.", TraceEventType.Information);
                Log?.Invoke($"[ExcelHelpers] Total Settings: {result.ConfigFile.Settings.Count}, Assets: {result.ConfigFile.Assets.Count}, Files: {result.ConfigFile.Files.Count}.", TraceEventType.Information);
                Log?.Invoke($"[ExcelHelpers] Sheets detected: {result.Sheets.Count} ({string.Join(", ", result.Sheets.Select(s => $"{s.ConfigType}('{s.SheetName}')"))})", TraceEventType.Information);
                foreach (var sheet in result.Sheets)
                {
                    if (sheet.ConfigType == ConfigSheetType.Unknown)
                        Log?.Invoke($"[ExcelHelpers] - Warning: Sheet '{sheet.SheetName}' has unknown format and was not processed.", TraceEventType.Warning);
                    else
                        Log?.Invoke($"[ExcelHelpers] - '{sheet.SheetName}': Detected as {sheet.ConfigType} with {sheet.Sheet?.Rows.Count ?? 0} rows.", TraceEventType.Information);
                }
                Log?.Invoke($"[ExcelHelpers] Scopes detected: {(result.Scopes.Any() ? string.Join(", ", result.Scopes) : "None")}.", TraceEventType.Information);
                foreach (var scope in result.Scopes)
                {
                    Log?.Invoke($"[ExcelHelpers] - '{scope}': {result.ConfigByScope[scope].Settings.Count} Settings, {result.ConfigByScope[scope].Assets.Count} Assets, {result.ConfigByScope[scope].Files.Count} Files.", TraceEventType.Information);
                }
                Log?.Invoke($"[ExcelHelpers] Completed Validations", TraceEventType.Information);

            }
            catch (Exception ex)
            {
                Log?.Invoke($"[ExcelHelpers] Error validating config file: {ex.Message}", TraceEventType.Error);
                throw;
            }

            return result;
        }

        /// <summary>
        /// Checks if a file is an Excel file based on its extension.
        /// </summary>
        public static bool IsExcelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".xlsx" || extension == ".xls";
        }
        public static bool IsValidConfigType(Type type) => type == typeof(Dictionary<string, object>) || (type != null && type.IsClass && !type.IsAbstract && typeof(Models.Config).IsAssignableFrom(type) && type.HasParameterlessConstructor() && type != typeof(object) && type != typeof(Models.Config));

        public static List<string> ValidateConfigFileToType(Type type, ConfigFile configFile)
        {
            var validations = new List<string>();
            if (!IsValidConfigType(type))
            {
                validations.Add("Provided type is not a valid configuration class type.");
                return validations;
            }

            if (type == typeof(Dictionary<string, object>))
            {
                validations.Add("Type is Dictionary<string, object>, no further validation performed.");
                return validations;
            }

            var properties = type.GetProperties();
            foreach (var setting in configFile.Settings)
            {
                var prop = properties.FirstOrDefault(p => p.Name == setting.Name);
                if (prop == null)
                {
                    validations.Add($"No matching property found for setting '{setting.Name}'.");
                    continue;
                }
                if (prop.PropertyType != typeof(string) && string.IsNullOrWhiteSpace(setting.Type))
                {
                    validations.Add($"Setting '{setting.Name}' is mapped to property of type '{prop.PropertyType.Name}' but has no specified type.");
                    continue;
                }
                var expectedType = string.IsNullOrWhiteSpace(setting.Type) ? "string" : setting.Type.ToLowerInvariant();
                var actualType = prop.PropertyType;
                bool typeMismatch = expectedType switch
                {
                    "string" => actualType != typeof(string),
                    "int" or "integer" => actualType != typeof(int) && actualType != typeof(int?),
                    "bool" or "boolean" => actualType != typeof(bool) && actualType != typeof(bool?),
                    "double" => actualType != typeof(double) && actualType != typeof(double?),
                    "float" => actualType != typeof(float) && actualType != typeof(float?),
                    "datetime" => actualType != typeof(DateTime) && actualType != typeof(DateTime?),
                    "list<string>" or "list" => !(actualType == typeof(List<string>) || (actualType.IsGenericType && actualType.GetGenericTypeDefinition() == typeof(List<>) && actualType.GetGenericArguments()[0] == typeof(string))),
                    "datatable" => actualType != typeof(DataTable),
                    _ => false
                };
                if (typeMismatch)
                {
                    validations.Add($"Type mismatch for setting '{setting.Name}': expected '{expectedType}', found '{actualType.Name}'.");
                }
            }
            return validations;
        }

        public static string DetermineConfigType(DataTable table, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            var type = ConfigSheetType.Unknown;
            if (HasPropertyHeaders(table, typeof(ConfigFileItem), Log))
                type = ConfigSheetType.File;
            else if (HasPropertyHeaders(table, typeof(ConfigAssetItem), Log))
                type = ConfigSheetType.Asset;
            else if (HasPropertyHeaders(table, typeof(ConfigSettingItem), Log))
                type = ConfigSheetType.Setting;
            else if (HasPropertyHeaders(table, typeof(NameValueFolderItem), Log))
                type = ConfigSheetType.NameValueFolder;
            else if (HasPropertyHeaders(table, typeof(NameValueItem), Log))
                type = ConfigSheetType.NameValue;

            Log($"Determined config type for sheet '{table.TableName}': {type}", TraceEventType.Information);
            return type;
        }

        public static bool HasPropertyHeaders(DataTable table, Type type, Action<string, TraceEventType>? log = null)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) => log?.Invoke(msg, level);
            if (table == null)
            {
                Log("DataTable is null.", TraceEventType.Error);
                return false;
            }
            if (type == null)
            {
                Log("Type is null.", TraceEventType.Error);
                return false;
            }
            Log($"Checking if table has headers for type {type.Name}", TraceEventType.Verbose);
            var props = type.GetFields().Select(p => p.Name).ToList();
            Log($"Expected properties: {string.Join(", ", props)}", TraceEventType.Verbose);
            Log($"Table columns: {string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}", TraceEventType.Verbose);
            Log($"Result: {props.All(table.Columns.Contains)}", TraceEventType.Verbose);
            return props.All(table.Columns.Contains);
        }

        /// <summary>
        /// Result of validating a single sheet's headers.
        /// </summary>
        public class SheetMetadata
        {
            public string SheetName { get; set; } = "";
            public string ConfigType { get; set; } = ConfigSheetType.Unknown;
            public DataTable? Sheet { get; set; } = null;
        }

        public static class ConfigSheetType
        {
            public const string Setting = "Setting";
            public const string Asset = "Asset";
            public const string File = "File";
            public const string NameValue = "NameValue";
            public const string NameValueFolder = "NameValueFolder";
            public const string Unknown = "Unknown";
        }
    }
}
