using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using Yash.Config.Activities.Models;

namespace Yash.Config.Activities
{
    /// <summary>
    /// Loads configuration data from a JSON file and retrieves related assets and files.
    /// </summary>
    public class LoadConfig : CodedWorkflow
    {
        /// <summary>
        /// Reads configuration settings, assets, and files from a JSON file and returns them as a dictionary.
        /// </summary>
        /// <param name="in_list_ConfigNames">Comma-separated list of configuration names to load.</param>
        /// <param name="in_str_Path">Path to the configuration JSON file (default: "Data\\Config.json").</param>
        /// <returns>A dictionary containing loaded configuration values.</returns>
        [Workflow]
        public Dictionary<string, object> Execute(
            List<string> in_list_ConfigNames,
            string in_str_Path
        )
        {
            Log($"Reading config file from {in_str_Path}", LogLevel.Info);

            // Read and deserialize the config file
            var dict = new Dictionary<string, object>();
            Dictionary<string, ConfigFile> configs;
            configs = in_str_Path.EndsWith(".json") ? ParseFromJson(in_str_Path) : in_str_Path.EndsWith(".xlsx") ? workflows.ExcelToConfigFileDictionary(in_str_Path) : throw new Exception($"Unsupported config file format");
            

            foreach (var configName in in_list_ConfigNames)
            {
                if (configs.ContainsKey(configName))
                {
                    Log($"Processing config {configName}", LogLevel.Trace);

                    // Load settings into the dictionary
                    foreach (var setting in configs[configName].Settings)
                        dict[setting.Name] = setting.Value;

                    // Load assets from Orchestrator or leave as blank if not being used
                    foreach (var asset in configs[configName].Assets)
                        dict[asset.Name] = !string.IsNullOrEmpty(asset.Value) ? system.GetAsset(asset.Value, asset.Folder) : "";

                    // Load files based on their type
                    foreach (var file in configs[configName].Files)
                    {
                        var tempPath = !string.IsNullOrWhiteSpace(file.Bucket)
                            ? Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() + "." + file.Type.ToLower())
                            : file.Path;

                        if (!string.IsNullOrWhiteSpace(file.Bucket))
                            system.DownloadStorageFile(file.Path, file.Bucket, file.Folder, tempPath, 10000);

                        if (file.Type.Equals("csv", StringComparison.OrdinalIgnoreCase))
                            dict[file.Name] = yashUtilityActivities.ReadCSVTable(tempPath, file.Name);
                        else if (file.Type.Equals("xlsx", StringComparison.OrdinalIgnoreCase) || file.Type.Equals("xls", StringComparison.OrdinalIgnoreCase))
                            dict[file.Name] = yashUtilityActivities.ReadExcelFile(tempPath);
                        else if (file.Type.Equals("txt", StringComparison.OrdinalIgnoreCase))
                            dict[file.Name] = File.ReadAllText(file.Path);

                        // Delete temporary files if downloaded
                        if (!string.IsNullOrWhiteSpace(file.Bucket))
                            File.Delete(tempPath);
                    }
                }
                else
                {
                    Log($"Could not find config with name {configName}", LogLevel.Warn);
                }
            }

            Log($"{dict.Keys.Count} keys loaded", LogLevel.Info);
            return dict;
        }

        public Dictionary<string, ConfigFile> ParseFromJson(string path)
        {
            var raw = File.ReadAllText(path);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, ConfigFile>>(raw);
            return configs;
        }
    }
}