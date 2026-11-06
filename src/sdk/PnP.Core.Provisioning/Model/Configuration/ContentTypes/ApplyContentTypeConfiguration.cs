using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.ContentTypes
{
    public class ApplyContentTypeConfiguration
    {
        [JsonPropertyName("provisionContentTypesToSubWebs")]
        public bool ProvisionContentTypesToSubWebs { get; set; }
    }
}
