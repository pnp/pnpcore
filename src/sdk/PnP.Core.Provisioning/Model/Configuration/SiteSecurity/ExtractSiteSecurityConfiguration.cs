using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.SiteSecurity
{
    public class ExtractConfiguration
    {
        [JsonPropertyName("includeSiteGroups")]
        public bool IncludeSiteGroups { get; set; }
    }
}
