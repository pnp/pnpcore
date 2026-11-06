using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.PropertyBag
{
    public class ApplyPropertyBagConfiguration
    {
        [JsonPropertyName("overwriteSystemValues")]
        public bool OverwriteSystemValues { get; set; }
    }
}
