using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.Services.Orchestrator.GetAssets
{
    public class Asset
    {
        [JsonProperty("Key", NullValueHandling = NullValueHandling.Ignore)]
        public string? Key { get; set; }

        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("CanBeDeleted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanBeDeleted { get; set; }

        [JsonProperty("ValueScope", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValueScope { get; set; }

        [JsonProperty("ValueType", NullValueHandling = NullValueHandling.Ignore)]
        public string? ValueType { get; set; }

        [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
        public string? Value { get; set; }

        [JsonProperty("StringValue", NullValueHandling = NullValueHandling.Ignore)]
        public string? StringValue { get; set; }

        [JsonProperty("BoolValue", NullValueHandling = NullValueHandling.Ignore)]
        public bool? BoolValue { get; set; }

        [JsonProperty("IntValue", NullValueHandling = NullValueHandling.Ignore)]
        public int? IntValue { get; set; }

        [JsonProperty("CredentialUsername", NullValueHandling = NullValueHandling.Ignore)]
        public string? CredentialUsername { get; set; }

        [JsonProperty("CredentialPassword", NullValueHandling = NullValueHandling.Ignore)]
        public string? CredentialPassword { get; set; }

        [JsonProperty("ExternalName", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExternalName { get; set; }

        [JsonProperty("CredentialStoreId", NullValueHandling = NullValueHandling.Ignore)]
        public int? CredentialStoreId { get; set; }

        [JsonProperty("HasDefaultValue", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasDefaultValue { get; set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        [JsonProperty("FoldersCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? FoldersCount { get; set; }

        [JsonProperty("LastModificationTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LastModificationTime { get; set; }

        [JsonProperty("LastModifierUserId", NullValueHandling = NullValueHandling.Ignore)]
        public int? LastModifierUserId { get; set; }

        [JsonProperty("CreationTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreationTime { get; set; }

        [JsonProperty("CreatorUserId", NullValueHandling = NullValueHandling.Ignore)]
        public int? CreatorUserId { get; set; }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty("KeyValueList", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> KeyValueList { get; set; }

        [JsonProperty("Tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Tags { get; set; }
    }
}
