using System;
using System.Collections.Generic;
using System.Data;

namespace Yash.Config.Models
{
    /// <summary>
    /// Represents a file entry in the configuration.
    /// </summary>
    [Serializable]
    public class ConfigFileItem
    {
        /// <summary>
        /// The name of the key the file will map to in the config dictionary.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The local or remote path of the file. If local, this can be a relative or absolute path to the file. If remote, this is the absolute path in the storage bucket in Orchestrator.
        /// </summary>
        public string Path = "";

        /// <summary>
        /// The folder where the file is stored (if applicable).
        /// </summary>
        public string Folder = "";

        /// <summary>
        /// The storage bucket name (if stored in a cloud location).
        /// </summary>
        public string Bucket = "";

        /// <summary>
        /// A description of the file.
        /// </summary>
        public string Description = "";

        /// <summary>
        /// The type to associate the value of the configuration key (for autogen purposes)
        /// </summary>
        public string Type = "";

        /// <summary>
        /// The file type (e.g., CSV, XLSX, TXT).
        /// </summary>
        public string FileType = "";

        public ConfigFileItem() { }
    }

}