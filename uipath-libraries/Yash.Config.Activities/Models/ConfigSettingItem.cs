using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Config.Activities.Models
{
    /// <summary>
    /// Represents a direct key-value-pair in the config dictionary
    /// </summary>
    [Serializable]
    public class ConfigSettingItem
    {
        /// <summary>
        /// The name of the key the asset will map to in the config dictionary.
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the key.
        /// </summary>
        public string Value;

        /// <summary>
        /// A description of the key.
        /// </summary>
        public string Description;

        /// <summary>
        /// The type to associate the value of the configuration key (for autogen purposes)
        /// </summary>
        public string Type;
        
        public ConfigSettingItem(){}
    }
}