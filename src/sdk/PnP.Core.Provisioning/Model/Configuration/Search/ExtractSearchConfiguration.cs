using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.SearchSettings
{
    public class ExtractSearchConfiguration
    {
        [JsonPropertyName("include")]
        public bool Include { get; set; }
    }
}
