using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.SyntexModels.Models
{
    public class ExtractSyntexModelsModelsConfiguration
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("excludeTrainingData")]
        public bool ExcludeTrainingData { get; set; }

    }
}
