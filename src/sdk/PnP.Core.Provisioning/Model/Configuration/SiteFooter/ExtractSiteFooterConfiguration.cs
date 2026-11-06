using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.SiteFooter
{
    public class ExtractSiteFooterConfiguration
    {
        [JsonPropertyName("removeExistingNodes")]
        public bool RemoveExistingNodes { get; set; }
    }
}
