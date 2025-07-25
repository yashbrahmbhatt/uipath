using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Orchestrator.GetFolders
{
    public class Folder
    {
        [JsonProperty("Key", NullValueHandling = NullValueHandling.Ignore)]
        public string? Key { get; set; }

        [JsonProperty("DisplayName", NullValueHandling = NullValueHandling.Ignore)]
        public string? DisplayName { get; set; }

        [JsonProperty("FullyQualifiedName", NullValueHandling = NullValueHandling.Ignore)]
        public string? FullyQualifiedName { get; set; }

        [JsonProperty("FullyQualifiedNameOrderable", NullValueHandling = NullValueHandling.Ignore)]
        public string? FullyQualifiedNameOrderable { get; set; }

        [JsonProperty("Description", NullValueHandling = NullValueHandling.Ignore)]
        public object Description { get; set; }

        [JsonProperty("FolderType", NullValueHandling = NullValueHandling.Ignore)]
        public string? FolderType { get; set; }

        [JsonProperty("ProvisionType", NullValueHandling = NullValueHandling.Ignore)]
        public string? ProvisionType { get; set; }

        [JsonProperty("PermissionModel", NullValueHandling = NullValueHandling.Ignore)]
        public string? PermissionModel { get; set; }

        [JsonProperty("ParentId", NullValueHandling = NullValueHandling.Ignore)]
        public string? ParentId { get; set; }

        [JsonProperty("ParentKey", NullValueHandling = NullValueHandling.Ignore)]
        public string? ParentKey { get; set; }

        [JsonProperty("IsActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty("FeedType", NullValueHandling = NullValueHandling.Ignore)]
        public string? FeedType { get; set; }

        [JsonProperty("ReservedOptions", NullValueHandling = NullValueHandling.Ignore)]
        public object? ReservedOptions { get; set; }

        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }
    }

}
