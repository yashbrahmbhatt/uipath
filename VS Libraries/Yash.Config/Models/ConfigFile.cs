using System;
using System.Collections.Generic;
using System.Data;
using Yash.Config.Services.Orchestrator.GetAssets;
using Yash.Config.Services.Orchestrator.GetFolders;

namespace Yash.Config.Models
{
    /// <summary>
    /// Represents a configuration file containing settings, assets, and file references.
    /// </summary>
    [Serializable]
    public class ConfigFile
    {
        /// <summary>
        /// A dictionary containing key-value pairs of configuration settings.
        /// </summary>
        public List<ConfigSettingItem> Settings = new();

        /// <summary>
        /// A list of assets referenced in the configuration.
        /// </summary>
        public List<ConfigAssetItem> Assets = new();

        /// <summary>
        /// A list of file entries referenced in the configuration.
        /// </summary>
        public List<ConfigFileItem> Files = new();

        public ConfigFile() { }
    }
}