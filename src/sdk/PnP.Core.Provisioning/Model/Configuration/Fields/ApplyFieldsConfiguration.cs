using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Fields
{
    public class ApplyFieldsConfiguration
    {
        [JsonPropertyName("provisionFieldsToSubWebs")]
        public bool ProvisionFieldsToSubWebs { get; set; }
    }
}
