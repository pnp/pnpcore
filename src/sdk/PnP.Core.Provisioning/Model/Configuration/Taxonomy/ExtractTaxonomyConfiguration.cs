using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Taxonomy
{
    public class ExtractTaxonomyConfiguration
    {
        [JsonPropertyName("includeSecurity")]
        public bool IncludeSecurity { get; set; }

        [JsonPropertyName("includeSiteCollectionTermGroup")]
        public bool IncludeSiteCollectionTermGroup { get; set; }

        [JsonPropertyName("includeAllTermGroups")]
        public bool IncludeAllTermGroups { get; set; }
    }
}
