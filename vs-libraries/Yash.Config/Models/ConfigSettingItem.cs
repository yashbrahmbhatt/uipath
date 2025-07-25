using System;
using System.Collections.Generic;
using System.Data;

namespace Yash.Config.Models
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
        public string Name = "";

        /// <summary>
        /// The value of the key.
        /// </summary>
        public string Value = "";

        /// <summary>
        /// A description of the key.
        /// </summary>
        public string Description = "";

        /// <summary>
        /// The type to associate the value of the configuration key (for autogen purposes)
        /// </summary>
        public string Type = "";

        public ConfigSettingItem() { }
    }
}