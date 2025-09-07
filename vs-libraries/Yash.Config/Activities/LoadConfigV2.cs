using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities.Statements;
using TraceEventType = System.Diagnostics.TraceEventType;
using UiPath.Robot.Activities.Api;
using Yash.Config.Helpers;
using Yash.Config.Models;
using System.Data;
using Newtonsoft.Json;
using UiPath.Activities.Api.Base;
using UiPath.Studio.Activities.Api;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.ComponentModel;
using Yash.Orchestrator;

namespace Yash.Config.Activities
{
    public class LoadConfigException : Exception
    {
        public LoadConfigException(string message) : base("[LoadConfig] " + message) { }
        public LoadConfigException(string message, Exception innerException) : base("[LoadConfig] " + message, innerException) { }
        public LoadConfigException(string message, TraceEventType eventType) : base("[LoadConfig] " + message)
        {
            EventType = eventType;
        }
        public TraceEventType EventType { get; private set; } = TraceEventType.Error;
    }
    public class LoadConfig : AsyncCodeActivity<Dictionary<string, object>>
    {
        public InArgument<string> WorkbookPath { get; set; }


        private IExecutorRuntime _runtime;
        private IAccessProvider _accessProvider;
        private OrchestratorService _orchestratorService;

        private string _path;

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            _runtime = context.GetExtension<IExecutorRuntime>();
            _accessProvider = context.GetExtension<IAccessProvider>();

            _path = WorkbookPath.Get(context);

            if (string.IsNullOrWhiteSpace(_path))
                throw new LoadConfigException("Workbook path is required.");

            Log("Starting config load for: " + _path);

            var task = RunWorkflowAsync(context);
            return TaskHelpers.BeginTask(task, callback, state);
        }

        private async Task<Dictionary<string, object>> RunWorkflowAsync(AsyncCodeActivityContext context)
        {
            _orchestratorService = new(_accessProvider, Log);
            await _orchestratorService.InitializeAsync();
            var dataset = ExcelHelpers.ReadExcelFile(_path);
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

            Dictionary<string, object> outputDict = new();

            foreach (var setting in configFile.Settings)
            {
                if (string.IsNullOrWhiteSpace(setting.Name))
                {
                    Log("Setting with empty name found, skipping.", TraceEventType.Warning);
                    continue;
                }

                outputDict[setting.Name] = setting.Value;
                Log($"Setting '{setting.Name}' loaded with value '{setting.Value}'.", TraceEventType.Verbose);
            }

            foreach (var asset in configFile.Assets)
            {
                if (!string.IsNullOrWhiteSpace(asset.Name))
                {
                    var folderAssets = _orchestratorService.Assets.FirstOrDefault(kvp => kvp.Key.DisplayName == asset.Folder);
                    if (folderAssets.Key == null) throw new LoadConfigException($"Folder '{asset.Folder}' not found in orchestrator.");

                    if (folderAssets.Value.Any(a => a.Name == asset.Value))
                    {
                        outputDict[asset.Name] = folderAssets.Value.FirstOrDefault(a => a.Name == asset.Value).Value;
                        Log($"Found existing asset '{asset.Value}' in folder '{asset.Folder}' with value {outputDict[asset.Name]}.");
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
                    // Remote Storage Bucket file
                    Log($"[Storage Bucket] Resolving file '{file.Name}' in bucket '{file.Bucket}' and folder '{file.Folder}' with path '{file.Path}'.");

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
                        Log($"Resolved bucket file '{file.Name}' → FullPath: '{match.FullPath}'");
                        var tempPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(file.Path));
                        await _orchestratorService.DownloadBucketFile(bucket, match, tempPath);
                        Log($"Downloaded bucket file '{file.Name}' to local path '{tempPath}'.");
                        outputDict[file.Name] = await ParseFileAsync(tempPath, file.Type, file.Name);
                        Log($"Parsed bucket file '{file.Name}' with type '{file.Type}' and stored in output dictionary.");
                        File.Delete(tempPath); // Clean up temporary file
                    }
                }
                else if (!string.IsNullOrWhiteSpace(file.Path))
                {
                    // Optionally check for existence:
                    if (!System.IO.File.Exists(file.Path))
                        throw new FileNotFoundException($"Local file '{file.Path}' for '{file.Name}' does not exist.");

                    Log($"[Local] Found file '{file.Name}' at local path '{file.Path}'.");
                    outputDict[file.Name] = await ParseFileAsync(file.Path, file.Type, file.Name);

                }
                else
                {
                    throw new LoadConfigException($"File '{file.Name}' is missing a path", TraceEventType.Warning);
                }
            }
            Log($"Configuration loaded successfully with {outputDict.Count} entries.", TraceEventType.Information);
            return outputDict;
        }


        protected override Dictionary<string, object> EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            var output = TaskHelpers.EndTask<Dictionary<string, object>>(result);
            Result.Set(context, output);
            return output; // Safe, task is already awaited
        }

        private void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            _runtime?.LogMessage(new LogMessage
            {
                EventType = level,
                Message = msg
            });
        }

        private async Task<object> ParseFileAsync(string filePath, string type, string name)
        {
            switch (type.ToLowerInvariant())
            {
                case "csv":
                    var table = CSVHelpers.Parse(filePath, filePath, Log);
                    Log($"Hydrated CSV file '{name}' with {table.Columns.Count} columns and {table.Rows.Count} rows.");
                    return JsonConvert.SerializeObject(table);

                case "xlsx":
                    var ds = ExcelHelpers.ReadExcelFile(filePath);
                    Log($"Hydrated Excel file '{name}' with {ds.Tables.Count} tables.");
                    return JsonConvert.SerializeObject(ds);

                default:
                    var content = await File.ReadAllTextAsync(filePath);
                    Log($"Read raw contents of file '{name}'.");
                    return content;
            }
        }

    }

}
