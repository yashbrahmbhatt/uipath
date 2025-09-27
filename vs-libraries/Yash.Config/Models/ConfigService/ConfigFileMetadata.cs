using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yash.Config.ConfigurationFile;
using Yash.Config.Models.Config;

namespace Yash.Config.ConfigurationService
{
    public class ConfigFileMetadata
    {
        public string FilePath { get; set; } = "";
        public int SheetCount
        {
            get
            {
                return Sheets.Count();
            }
        }

        public ConfigFileError? ConfigFileError { get; set; } = null;
        public List<SheetMetadata> Sheets { get; set; } = new();
        public ConfigFile ConfigFile { get; set; } = new();
        public List<string> Scopes { get; set; } = new();
        public Dictionary<string, ConfigFile> ConfigByScope { get; set; } = new();
    }
}
