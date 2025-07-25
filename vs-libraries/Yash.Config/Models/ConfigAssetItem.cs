using System;
using System.Collections.Generic;
using System.Data;

namespace Yash.Config.Models
{
    /// <summary>
    /// Represents an asset stored in UiPath Orchestrator.
    /// </summary>
    [Serializable]
    public class ConfigAssetItem
    {
        /// <summary>
        /// The name of the key the asset will map to in the config dictionary.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The name of the asset in Orchestrator.
        /// </summary>
        public string Value = "";

        /// <summary>
        /// The folder where the asset is stored in Orchestrator.
        /// </summary>
        public string Folder = "";

        /// <summary>
        /// A description of the asset.
        /// </summary>
        public string Description = "";

        /// <summary>
        /// The type to associate the value of the configuration key (for autogen purposes)
        /// </summary>
        public string Type = "";

        public ConfigAssetItem() { }
    }
}