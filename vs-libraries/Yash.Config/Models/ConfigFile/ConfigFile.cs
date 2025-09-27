using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Yash.Config.ConfigurationFile
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