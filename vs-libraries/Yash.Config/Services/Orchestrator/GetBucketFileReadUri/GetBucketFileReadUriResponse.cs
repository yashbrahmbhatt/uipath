using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.Services.Orchestrator.GetBucketFileReadUri
{
    public class GetBucketFileReadUriResponse
    {
        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore)]
        public string? OdataContext { get; set; }

        [JsonProperty("Uri", NullValueHandling = NullValueHandling.Ignore)]
        public string? Uri { get; set; }

        [JsonProperty("Verb", NullValueHandling = NullValueHandling.Ignore)]
        public string? Verb { get; set; }

        [JsonProperty("RequiresAuth", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RequiresAuth { get; set; }

        [JsonProperty("Headers", NullValueHandling = NullValueHandling.Ignore)]
        public GetBucketFileUriResponseHeaders? Headers { get; set; }
    }

    public class GetBucketFileUriResponseHeaders
    {
        [JsonProperty("Keys", NullValueHandling = NullValueHandling.Ignore)]
        public List<object>? Keys { get; set; }

        [JsonProperty("Values", NullValueHandling = NullValueHandling.Ignore)]
        public List<object>? Values { get; set; }
    }
}
