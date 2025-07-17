using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Config.Activities.Models
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