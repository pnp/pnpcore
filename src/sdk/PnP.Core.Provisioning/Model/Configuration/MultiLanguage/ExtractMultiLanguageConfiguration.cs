using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.MultiLanguage
{
    public class ExtractMultiLanguageConfiguration
    {
        [JsonPropertyName("persistMultiLanguageResources")]
        public bool PersistResources { get; set; }

        [JsonPropertyName("resourceFilePrefix")]
        public string ResourceFilePrefix { get; set; }
    }
}
