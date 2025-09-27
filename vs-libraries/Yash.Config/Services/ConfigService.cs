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
using Yash.Config.Activities;
using Yash.Config.Helpers;
using Yash.Config.Models.Config;
using Yash.Config.ConfigurationFile;
using Yash.Config.ConfigurationService;
using Yash.Orchestrator;
using Yash.Utility.Services.Parsing;
using Newtonsoft.Json.Linq;
using UiPath.Shared.Orchestrator;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;
using Yash.Utility.Services;
using Yash.Utility.Services.Excel;

namespace Yash.Config.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly IExcelHelperService _excelService;
        private readonly ITypeParsingService _typeParserService;
        private readonly ConfigCodeGenerationService _codeGenerationService;
        private IOrchestratorService? _orchestratorService = null;
        private readonly Action<string, TraceEventType>? _logAction = null;
        private readonly TraceEventType _minLogLevel = TraceEventType.Information;
        
        private void Log(string msg, TraceEventType level = TraceEventType.Information) 
        { 
            if (level <= _minLogLevel) 
                _logAction?.Invoke($"[ConfigService] {msg}", level); 
        }
        
        public string FilePath { get; set; }
        public ConfigFileMetadata Metadata { get; private set; }
        public bool IsValid => FilePath != null && File.Exists(FilePath) && IsExcelFile(FilePath) && Metadata.ConfigFileError == null;

        /// <summary>
        /// Initializes a new instance of ConfigService with dependency injection support.
        /// </summary>
        /// <param name="filePath">Path to the configuration Excel file</param>
        /// <param name="serviceProvider">Service provider for dependency resolution (optional)</param>
        /// <param name="log">Optional logging action</param>
        /// <param name="minLogLevel">Minimum log level for filtering</param>
        public ConfigService(string filePath, IServiceProvider? serviceProvider = null, Action<string, TraceEventType>? log = null, TraceEventType? minLogLevel = TraceEventType.Information)
        {
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            
            _serviceProvider = serviceProvider;
            _logAction = log;
            _minLogLevel = minLogLevel ?? TraceEventType.Information;
            FilePath = filePath;
            
            // Initialize services - check service provider first, then create new instances
            _excelService = _serviceProvider?.GetService<IExcelHelperService>() ?? new ExcelHelperService(_logAction);
            _codeGenerationService = _serviceProvider?.GetService<ConfigCodeGenerationService>() ?? new ConfigCodeGenerationService(_logAction);
            _typeParserService = _serviceProvider?.GetService<ITypeParsingService>() ?? new TypeParsingService(_logAction);

            // Try to get OrchestratorService from service provider if available
            _orchestratorService = _serviceProvider?.GetService<IOrchestratorService>();
            
            Log($"ConfigService initialized with file path: {filePath}", TraceEventType.Verbose);
        }

        /// <summary>
        /// Ensures the OrchestratorService is available for operations that require it.
        /// If not available from the service provider, throws an exception with guidance.
        /// </summary>
        private void EnsureOrchestratorService()
        {
            if (_orchestratorService == null)
            {
                throw new InvalidOperationException(
                    "OrchestratorService is not available. Please register IOrchestratorService in your service provider " +
                    "or use ConfigService in an environment where Orchestrator integration is not required.");
            }
        }

        public async Task<LoadConfigResult> LoadConfigAsync(string? scope = null)
        {
            EnsureOrchestratorService();
            ValidateConfigFile();
            var result = new LoadConfigResult
            {
                Metadata = Metadata,
                ConfigByScope = new(),
            };
            try
            {
                await _orchestratorService.InitializeAsync().ConfigureAwait(false);
                Log($"Scopes available in metadata: {string.Join(", ", Metadata.Scopes)}", TraceEventType.Verbose);

                foreach (var _scope in Metadata.Scopes)
                {
                    ConfigFile configFile = null;
                    if (!string.IsNullOrWhiteSpace(scope))
                    {
                        if (_scope != scope)
                        {
                            Log($"Skipping config for scope '{_scope}' due to provided scope '{scope}'.", TraceEventType.Verbose);
                            continue;
                        }
                    }
                    configFile = Metadata.ConfigByScope.ContainsKey(_scope) ? Metadata.ConfigByScope[_scope] : Metadata.ConfigFile;

                    Dictionary<string, object> outputDict = new();

                    foreach (var setting in configFile.Settings)
                    {
                        if (string.IsNullOrWhiteSpace(setting.Name))
                        {
                            Log("Setting with empty name found, skipping.", TraceEventType.Warning);
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(setting.Scope))
                        {
                            Log($"Warning: Setting '{setting.Name}' does not have a scope defined. It will be considered for the Shared scope.", TraceEventType.Warning);
                            setting.Scope = "Shared";
                        }
                        if (setting.Scope != null && setting.Scope != scope)
                        {
                            Log($"Setting '{setting.Name}' skipped due to scope mismatch (Setting Scope: '{setting.Scope}', Provided Scope: '{scope}').", TraceEventType.Information);
                            continue;
                        }

                        outputDict[setting.Name] = setting.Value;
                        Log($"Setting '{setting.Name}' loaded with value '{setting.Value}'.", TraceEventType.Verbose);
                    }

                    foreach (var asset in configFile.Assets)
                    {
                        if (!string.IsNullOrWhiteSpace(asset.Name))
                        {
                            if (string.IsNullOrWhiteSpace(asset.Scope))
                            {
                                Log($"Warning: Asset '{asset.Name}' does not have a scope defined. It will be considered for the Shared scope.", TraceEventType.Warning);
                                asset.Scope = "Shared";
                            }
                            if (asset.Scope != null && asset.Scope != scope)
                            {
                                Log($"Asset '{asset.Name}' skipped due to scope mismatch (Asset Scope: '{asset.Scope}', Provided Scope: '{scope}').", TraceEventType.Information);
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
                                    Log($"Found asset '{asset.Value}' in folder '{asset.Folder}' with value {outputDict[asset.Name]}.");
                                }
                                else
                                {
                                    Log($"Asset '{asset.Value}' found in folder '{asset.Folder}' but has null value.", TraceEventType.Warning);
                                }
                            }
                            else
                            {
                                Log($"Asset '{asset.Value}' not found in folder '{asset.Folder}'.", TraceEventType.Warning);
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
                        if (string.IsNullOrWhiteSpace(file.Scope))
                        {
                            Log($"Warning: Asset '{file.Name}' does not have a scope defined. It will be considered for the Shared scope.", TraceEventType.Warning);
                            file.Scope = "Shared";
                        }

                        if (!string.IsNullOrWhiteSpace(file.Bucket) && !string.IsNullOrWhiteSpace(file.Folder))
                        {
                            if (file.Scope != null && scope != null && file.Scope != scope)
                            {
                                Log($"File '{file.Name}' skipped due to scope mismatch (File Scope: '{file.Scope}', Provided Scope: '{scope}').", TraceEventType.Information);
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
                                await _orchestratorService.DownloadBucketFile(bucket, match, tempPath).ConfigureAwait(false);
                                Log($"Downloaded bucket file '{file.Name}' to local path '{tempPath}'.", TraceEventType.Verbose);
                                outputDict[file.Name] = await ParseConfigFileItemAsync(tempPath, file.Type, file.Name, Log).ConfigureAwait(false);
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
                            outputDict[file.Name] = await ParseConfigFileItemAsync(file.Path, file.Type, file.Name, Log).ConfigureAwait(false);
                        }
                        else
                        {
                            throw new LoadConfigException($"File '{file.Name}' is missing a path", TraceEventType.Warning);
                        }
                    }
                    Log($"Configuration loaded successfully with {outputDict.Count} entries.", TraceEventType.Information);

                    result.Config = outputDict;
                    result.ConfigByScope[_scope] = outputDict;
                }
            }
            catch (Exception ex)
            {
                Log($"[LoadConfigWithOrchestratorServiceAsync] Exception: {ex.Message}", TraceEventType.Error);
                throw;
            }

            return result;
        }
        // Try versions with file path validation
        public async Task<object> ParseConfigFileItemAsync(string _FilePath, string type, string name, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) { if (level <= minLogLevel) log?.Invoke(msg, level); }

            switch (type.ToUpperInvariant())
            {
                case ConfigFileItemType.CSV:
                    var table = CSVHelpers.Parse(_FilePath, _FilePath, (msg, level) => log?.Invoke(msg, level));
                    Log($"Hydrated CSV file '{name}' with {table.Columns.Count} columns and {table.Rows.Count} rows.");
                    return table;
                case ConfigFileItemType.XLSX:
                case ConfigFileItemType.XLS:
                    try
                    {
                        var ds = _excelService.ReadExcelFile(_FilePath);
                        return ds;
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to parse Excel file '{name}': {ex.Message}", TraceEventType.Error);
                    }
                    return null;
                case ConfigFileItemType.JSON:
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(_FilePath).ConfigureAwait(false);
                        return JsonConvert.DeserializeObject<JObject>(jsonContent);
                    }
                    catch (JsonException ex)
                    {
                        Log($"Failed to parse JSON file '{name}': {ex.Message}. Returning raw content.", TraceEventType.Error);
                        return null;
                    }
                case ConfigFileItemType.Other:
                    return null;
                default:
                    Log($"Parsing file '{name}' as plain text.");
                    return await File.ReadAllTextAsync(_FilePath).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Validates if an Excel file is a valid config file with required sheets and headers.
        /// </summary>
        /// <param name="_FilePath">Path to the Excel file to validate.</param>
        /// <param name="Log">Optional logging delegate for diagnostics.</param>
        /// <returns>ConfigFileValidationResult containing validation details.</returns>
        public void ValidateConfigFile()
        {
            Metadata = new ConfigFileMetadata
            {
                FilePath = FilePath
            };
            if (string.IsNullOrEmpty(FilePath))
            {
                Log($"[ExcelHelpers] Configuration file path is null or empty.", TraceEventType.Error);
                Metadata.ConfigFileError = ConfigFileError.NullValue;
                return;
            }

            if (!File.Exists(FilePath))
            {
                Log($"[ExcelHelpers] Configuration file not found: {FilePath}", TraceEventType.Error);
                Metadata.ConfigFileError = ConfigFileError.FileNotFound;
                return;
            }

            if (!IsExcelFile(FilePath))
            {
                Log($"[ExcelHelpers] File is not an Excel file: {FilePath}", TraceEventType.Error);
                Metadata.ConfigFileError = ConfigFileError.NotExcelFile;
                return;
            }

            Log($"[ExcelHelpers] Validating config file structure: {FilePath}", TraceEventType.Information);

            try
            {
                var dataSet = _excelService.ReadExcelFile(FilePath);

                foreach (var table in dataSet.Tables.Cast<DataTable>())
                {
                    Log($"[ExcelHelpers] Found sheet: {table.TableName} with {table.Columns.Count} columns and {table.Rows.Count} rows.", TraceEventType.Verbose);
                    var configType = DetermineConfigType(table, Log);
                    Metadata.Sheets.Add(new SheetMetadata()
                    {
                        SheetName = table.TableName,
                        ConfigType = configType,
                        Sheet = table
                    });
                    switch (configType)
                    {
                        case ConfigSheetType.Setting:
                            Metadata.ConfigFile.Settings = Metadata.ConfigFile.Settings.Concat(JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigSettingItem>()).ToList();
                            break;
                        case ConfigSheetType.Asset:
                            Metadata.ConfigFile.Assets = Metadata.ConfigFile.Assets.Concat(JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigAssetItem>()).ToList();
                            break;
                        case ConfigSheetType.File:
                            Metadata.ConfigFile.Files = Metadata.ConfigFile.Files.Concat(JsonConvert.DeserializeObject<List<ConfigFileItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigFileItem>()).ToList();
                            break;
                        case ConfigSheetType.NameValue:
                            Metadata.ConfigFile.Settings = Metadata.ConfigFile.Settings.Concat((JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigSettingItem>()).Select(nv => new ConfigSettingItem { Name = nv.Name, Value = nv.Value, Type = "string" })).ToList();
                            break;
                        case ConfigSheetType.NameValueFolder:
                            Metadata.ConfigFile.Assets = Metadata.ConfigFile.Assets.Concat((JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(table)) ?? new List<ConfigAssetItem>()).Select(nv => new ConfigAssetItem { Name = nv.Name, Value = nv.Value, Folder = nv.Folder, Type = "string" })).ToList();
                            break;
                    }
                }
                Metadata.ConfigFile.Settings = Metadata.ConfigFile.Settings.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();
                Metadata.ConfigFile.Assets = Metadata.ConfigFile.Assets.Where(a => !string.IsNullOrWhiteSpace(a.Name) && !string.IsNullOrWhiteSpace(a.Folder) && !string.IsNullOrWhiteSpace(a.Value)).ToList();
                Metadata.ConfigFile.Files = Metadata.ConfigFile.Files.Where(f => !string.IsNullOrWhiteSpace(f.Name) && !string.IsNullOrWhiteSpace(f.Path)).ToList();
                foreach (var setting in Metadata.ConfigFile.Settings)
                    setting.Scope = string.IsNullOrWhiteSpace(setting.Scope) ? "Shared" : setting.Scope.Trim();
                foreach (var asset in Metadata.ConfigFile.Assets)
                    asset.Scope = string.IsNullOrWhiteSpace(asset.Scope) ? "Shared" : asset.Scope.Trim();
                foreach (var file in Metadata.ConfigFile.Files)
                    file.Scope = string.IsNullOrWhiteSpace(file.Scope) ? "Shared" : file.Scope.Trim();

                var usesScopes = Metadata.ConfigFile.Settings.Any(s => !string.IsNullOrEmpty(s.Scope)) ||
                    Metadata.ConfigFile.Assets.Any(a => !string.IsNullOrEmpty(a.Scope)) ||
                    Metadata.ConfigFile.Files.Any(f => !string.IsNullOrEmpty(f.Scope));
                if (usesScopes)
                {
                    Metadata.Scopes = Metadata.ConfigFile.Settings.Select(s => s.Scope)
                        .Concat(Metadata.ConfigFile.Assets.Select(a => a.Scope))
                        .Concat(Metadata.ConfigFile.Files.Select(f => f.Scope))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct()
                        .ToList();
                    foreach (var scope in Metadata.Scopes)
                    {
                        Metadata.ConfigByScope[scope] = new ConfigFile()
                        {
                            Settings = Metadata.ConfigFile.Settings.Where(s => s.Scope == scope).ToList(),
                            Assets = Metadata.ConfigFile.Assets.Where(a => a.Scope == scope).ToList(),
                            Files = Metadata.ConfigFile.Files.Where(f => f.Scope == scope).ToList()
                        };

                    }
                }
                Log($"[ExcelHelpers] Configuration file validation completed successfully. Found {Metadata.Sheets.Count} sheets.", TraceEventType.Information);
                Log($"[ExcelHelpers] Total Settings: {Metadata.ConfigFile.Settings.Count}, Assets: {Metadata.ConfigFile.Assets.Count}, Files: {Metadata.ConfigFile.Files.Count}.", TraceEventType.Information);
                Log($"[ExcelHelpers] Sheets detected: {Metadata.Sheets.Count} ({string.Join(", ", Metadata.Sheets.Select(s => $"{s.ConfigType}('{s.SheetName}')"))})", TraceEventType.Information);
                foreach (var sheet in Metadata.Sheets)
                {
                    if (sheet.ConfigType == ConfigSheetType.Unknown)
                        Log($"[ExcelHelpers] - Warning: Sheet '{sheet.SheetName}' has unknown format and was not processed.", TraceEventType.Warning);
                    else
                        Log($"[ExcelHelpers] - '{sheet.SheetName}': Detected as {sheet.ConfigType} with {sheet.Sheet?.Rows.Count ?? 0} rows.", TraceEventType.Information);
                }
                Log($"[ExcelHelpers] Scopes detected: {(Metadata.Scopes.Any() ? string.Join(", ", Metadata.Scopes) : "None")}.", TraceEventType.Information);
                foreach (var scope in Metadata.Scopes)
                {
                    Log($"[ExcelHelpers] - '{scope}': {Metadata.ConfigByScope[scope].Settings.Count} Settings, {Metadata.ConfigByScope[scope].Assets.Count} Assets, {Metadata.ConfigByScope[scope].Files.Count} Files.", TraceEventType.Information);
                }
                Log($"[ExcelHelpers] Completed Validations", TraceEventType.Information);

            }
            catch (Exception ex)
            {
                Log($"[ExcelHelpers] Error validating config file: {ex.Message}", TraceEventType.Error);
                throw;
            }

        }

        /// <summary>
        /// Checks if a file is an Excel file based on its extension.
        /// </summary>
        public bool IsExcelFile(string _FilePath)
        {
            if (string.IsNullOrEmpty(_FilePath))
                return false;

            var extension = Path.GetExtension(_FilePath).ToLowerInvariant();
            return extension == ".xlsx" || extension == ".xls";
        }
        public bool IsValidConfigType(Type type) => type == typeof(Dictionary<string, object>) || type == typeof(Configuration) || (type != null && type.IsClass && !type.IsAbstract && typeof(Configuration).IsAssignableFrom(type) && type.HasParameterlessConstructor() && type != typeof(object));

        public List<string> ValidateConfigFileToType(Type type, ConfigFile configFile)
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

        public string DetermineConfigType(DataTable table, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) { if (level <= minLogLevel) log?.Invoke(msg, level); }
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

        public bool HasPropertyHeaders(DataTable table, Type type, Action<string, TraceEventType>? log = null, TraceEventType minLogLevel = TraceEventType.Information)
        {
            void Log(string msg, TraceEventType level = TraceEventType.Information) { if (level <= minLogLevel) log?.Invoke(msg, level); }
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
        /// Generates an Excel configuration template with proper data validation using current template structure.
        /// </summary>
        /// <param name="outputPath">Path where the template file will be saved.</param>
        /// <param name="supportedTypeNames">Optional array of supported type names for dropdown validation. If null, uses default types.</param>
        public void GenerateExcelTemplate(string outputPath, string[]? supportedTypeNames = null)
        {
            Log($"Creating Excel configuration template: {outputPath}", TraceEventType.Information);

            // Use default supported types if none provided
            var defaultTypes = new[] { 
                "string", "int", "bool", "double", "DateTime", "TimeSpan",
                "List<string>", "Dictionary<string,object>", "DataTable", "object"
            };
            var typesToUse = supportedTypeNames ?? defaultTypes;

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                // 1. _Instructions Sheet
                var instructions = package.Workbook.Worksheets.Add("_Instructions");
                instructions.Cells["A1"].Value = "Welcome to the Config Template!";
                instructions.Cells["A1"].Style.Font.Bold = true;
                instructions.Cells["A1"].Style.Font.Size = 14;
                
                instructions.Cells["A3"].Value = "This file is used to define your automation settings, assets, and file references.";
                instructions.Cells["A5"].Value = "Use the Yash.Config wizards in UiPath to generate strongly-typed C# classes based on this file.";
                instructions.Cells["A7"].Value = "Sheet Descriptions:";
                instructions.Cells["A7"].Style.Font.Bold = true;
                instructions.Cells["A8"].Value = "• Settings: Configuration values used by your automation";
                instructions.Cells["A9"].Value = "• Assets: References to UiPath Orchestrator assets by folder";
                instructions.Cells["A10"].Value = "• Files: File paths and references (local or bucket storage)";
                instructions.Cells["A12"].Value = "Required Columns:";
                instructions.Cells["A12"].Style.Font.Bold = true;
                instructions.Cells["A13"].Value = "• Name: Unique identifier for the config item";
                instructions.Cells["A14"].Value = "• Value/Path: The actual value or file path";
                instructions.Cells["A15"].Value = "• Type: Data type (use dropdown in Type column)";
                instructions.Cells["A16"].Value = "• Scope: Environment scope (e.g., Shared, Dev, Prod)";
                instructions.Cells["A17"].Value = "• Description: Optional documentation";
                
                instructions.Cells.AutoFitColumns();

                // 2. _ConfigFileSettings Sheet (defines types)
                var typeSheet = package.Workbook.Worksheets.Add("_ConfigFileSettings");
                typeSheet.Cells["A1"].Value = "SupportedTypes";
                typeSheet.Cells["A1"].Style.Font.Bold = true;
                int row = 2;
                foreach (var typeName in typesToUse)
                {
                    typeSheet.Cells[row, 1].Value = typeName;
                    row++;
                }
                typeSheet.Cells.AutoFitColumns();

                // 3. Settings / Assets / Files with enhanced structure and validation
                void AddConfigSheet(string name, string[] extraColumns = null)
                {
                    var ws = package.Workbook.Worksheets.Add(name);
                    
                    // Standard columns
                    ws.Cells["A1"].Value = "Name";
                    ws.Cells["B1"].Value = name == "Assets" ? "Value" : (name == "Files" ? "Path" : "Value");
                    ws.Cells["C1"].Value = "Type";
                    ws.Cells["D1"].Value = "Scope";
                    ws.Cells["E1"].Value = "Description";
                    
                    // Asset-specific columns
                    if (name == "Assets")
                    {
                        ws.Cells["F1"].Value = "Folder";
                    }
                    // File-specific columns  
                    else if (name == "Files")
                    {
                        ws.Cells["F1"].Value = "Bucket";
                        ws.Cells["G1"].Value = "Folder";
                    }

                    // Apply header formatting
                    var headerRange = name == "Settings" ? ws.Cells["A1:E1"] : 
                                     name == "Assets" ? ws.Cells["A1:F1"] : ws.Cells["A1:G1"];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    // Apply validation to column C (Type) for first 100 rows
                    var maxRows = 100;
                    for (int i = 2; i <= maxRows + 1; i++)
                    {
                        var validation = ws.DataValidations.AddListValidation($"C{i}");
                        validation.Formula.ExcelFormula = $"_ConfigFileSettings.$A$2:$A${typesToUse.Length + 1}";
                        validation.AllowBlank = false;
                        validation.ShowInputMessage = true;
                        validation.PromptTitle = "Data Type";
                        validation.Prompt = "Select a valid data type from the list.";
                        validation.ShowErrorMessage = true;
                        validation.ErrorTitle = "Invalid Type";
                        validation.Error = "Please select a type from the dropdown list.";
                    }

                    // Add example data for first row
                    if (name == "Settings")
                    {
                        ws.Cells["A2"].Value = "ExampleSetting";
                        ws.Cells["B2"].Value = "Example Value";
                        ws.Cells["C2"].Value = "string";
                        ws.Cells["D2"].Value = "Shared";
                        ws.Cells["E2"].Value = "This is an example setting";
                    }
                    else if (name == "Assets")
                    {
                        ws.Cells["A2"].Value = "ExampleAsset";
                        ws.Cells["B2"].Value = "AssetName";
                        ws.Cells["C2"].Value = "string";
                        ws.Cells["D2"].Value = "Shared";
                        ws.Cells["E2"].Value = "This is an example asset reference";
                        ws.Cells["F2"].Value = "FolderName";
                    }
                    else if (name == "Files")
                    {
                        ws.Cells["A2"].Value = "ExampleFile";
                        ws.Cells["B2"].Value = "Data\\example.xlsx";
                        ws.Cells["C2"].Value = "DataTable";
                        ws.Cells["D2"].Value = "Shared";
                        ws.Cells["E2"].Value = "This is an example file reference";
                        ws.Cells["F2"].Value = "";
                        ws.Cells["G2"].Value = "";
                    }

                    ws.Cells.AutoFitColumns();
                }

                AddConfigSheet("Settings");
                AddConfigSheet("Assets");
                AddConfigSheet("Files");

                // Save file
                var fileInfo = new FileInfo(outputPath);
                package.SaveAs(fileInfo);
                Log($"Excel configuration template saved to: {outputPath}", TraceEventType.Information);
            }
        }


    }
}
