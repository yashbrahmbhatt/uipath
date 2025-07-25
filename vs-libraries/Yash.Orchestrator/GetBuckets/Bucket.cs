using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Orchestrator.GetBuckets
{
    public class Bucket
    {
        
        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("Identifier", NullValueHandling = NullValueHandling.Ignore)]
        public string? Identifier { get; set; }

        [JsonProperty("StorageProvider", NullValueHandling = NullValueHandling.Ignore)]
        public object? StorageProvider { get; set; }

        [JsonProperty("StorageParameters", NullValueHandling = NullValueHandling.Ignore)]
        public object? StorageParameters { get; set; }

        [JsonProperty("StorageContainer", NullValueHandling = NullValueHandling.Ignore)]
        public object? StorageContainer { get; set; }

        [JsonProperty("Options", NullValueHandling = NullValueHandling.Ignore)]
        public string? Options { get; set; }

        [JsonProperty("CredentialStoreId", NullValueHandling = NullValueHandling.Ignore)]
        public object? CredentialStoreId { get; set; }

        [JsonProperty("ExternalName", NullValueHandling = NullValueHandling.Ignore)]
        public object? ExternalName { get; set; }

        [JsonProperty("Password", NullValueHandling = NullValueHandling.Ignore)]
        public object? Password { get; set; }

        [JsonProperty("FoldersCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? FoldersCount { get; set; }

        [JsonProperty("Encrypted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Encrypted { get; set; }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty("Tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<object>? Tags { get; set; }
    }
}
