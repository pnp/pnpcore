using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Publishing
{
    public class ExtractPublishingConfiguration
    {
        public ExtractPublishingConfiguration()
        {
            // Needed for backwards compability
            ExtractPageAsPublished = true;
        }

        [JsonPropertyName("includeNativePublishingFiles")]
        public bool IncludeNativePublishingFiles { get; set; }

        [JsonPropertyName("persist")]
        public bool Persist { get; set; }

        [JsonPropertyName("extractPageAsPublished")]
        public bool ExtractPageAsPublished { get; set; }
    }
}
