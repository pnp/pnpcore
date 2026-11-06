using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.Extensibility
{
    public class ApplyExtensibilityConfiguration
    {
        [JsonPropertyName("handlers")]
        public List<ExtensibilityHandler> Handlers { get; set; } = new List<ExtensibilityHandler>();
    }
}
