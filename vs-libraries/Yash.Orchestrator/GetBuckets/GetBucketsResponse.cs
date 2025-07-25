using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Orchestrator.GetBuckets
{
    public class GetBucketsResponse
    {
        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore)]
        public string? OdataContext { get; set; }

        [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
        public int? OdataCount { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public List<Bucket>? Buckets { get; set; }
    }
}
