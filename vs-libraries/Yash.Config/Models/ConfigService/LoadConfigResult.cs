using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yash.Config.ConfigurationFile;

namespace Yash.Config.ConfigurationService
{
    public class LoadConfigResult
    {
        public Dictionary<string, object> Config { get; set; } = new();
        public Dictionary<string, Dictionary<string, object>> ConfigByScope { get; set; } = new();
        public Dictionary<string, ConfigFile> ConfigFileByScope => Metadata.ConfigByScope;
        public ConfigFileMetadata Metadata { get; set; } = new();
    }
}
