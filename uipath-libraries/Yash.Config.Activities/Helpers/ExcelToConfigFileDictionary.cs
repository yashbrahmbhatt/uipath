using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using Yash.Config.Activities.Models;

namespace Yash.Config.Activities.Helpers
{
    public class ExcelToConfigFileDictionary : CodedWorkflow
    {
        [Workflow]
        public Dictionary<string, ConfigFile> Execute(string path = "Data\\Config.xlsx")
        {
            var dataset = yashUtilityActivities.ReadExcelFile(path);
            List<DataTable> tables = dataset.Tables.Cast<DataTable>().ToList();
            List<string> names = tables.Select(dt => dt.TableName.Split("_")[0]).Distinct().ToList();
            Dictionary<string, ConfigFile> configs = new();
            foreach (var name in names)
            {
                var dt_Settings = tables.FirstOrDefault(dt => dt.TableName.Trim() == name + "_Settings", null);
                var dt_Assets = tables.FirstOrDefault(dt => dt.TableName.Trim() == name + "_Assets", null);
                var dt_Files = tables.FirstOrDefault(dt => dt.TableName.Trim() == name + "_Files", null);

                var file = new ConfigFile();
                if (dt_Settings != null) file.Settings = JsonConvert.DeserializeObject<List<ConfigSettingItem>>(JsonConvert.SerializeObject(dt_Settings)).Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                if (dt_Assets != null) file.Assets = JsonConvert.DeserializeObject<List<ConfigAssetItem>>(JsonConvert.SerializeObject(dt_Assets)).Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                if (dt_Files != null) file.Files = JsonConvert.DeserializeObject<List<ConfigFileItem>>(JsonConvert.SerializeObject(dt_Files)).Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                configs[name] = file;
            }
            return configs;
        }
    }
}