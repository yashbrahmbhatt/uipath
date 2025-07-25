using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.Services.Services.Orchestrator.GetBucketFiles
{
    public class BucketFile
    {
        [JsonProperty("FullPath", NullValueHandling = NullValueHandling.Ignore)]
        public string? FullPath { get; set; }

        [JsonProperty("ContentType", NullValueHandling = NullValueHandling.Ignore)]
        public string? ContentType { get; set; }

        [JsonProperty("Size", NullValueHandling = NullValueHandling.Ignore)]
        public int? Size { get; set; }

        [JsonProperty("IsDirectory", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDirectory { get; set; }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public object? Id { get; set; }
    }
}
