using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Tenant.Sequence
{
    public class ExtractSequenceConfiguration
    {
        [JsonPropertyName("siteUrls")]
        public List<string> SiteUrls { get; set; } = new List<string>();

        [JsonPropertyName("maxSubsiteDepth")]
        public int MaxSubsiteDepth { get; set; }

        [JsonPropertyName("includeJoinedSites")]
        public bool IncludeJoinedSites { get; set; }

        [JsonPropertyName("includeSubsites")]
        public bool IncludeSubsites { get; set; }
    }
}
