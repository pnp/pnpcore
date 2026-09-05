using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.SyntexModels
{
    public class ExtractSyntexModelsConfiguration
    {
        [JsonPropertyName("models")]
        public List<Models.ExtractSyntexModelsModelsConfiguration> Models { get; set; } = new List<Models.ExtractSyntexModelsModelsConfiguration>();

        public bool HasModels
        {
            get
            {
                return this.Models != null && this.Models.Count > 0;
            }
        }
    }
}
