using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Provisioning.Model.Configuration.PropertyBag
{
    public class ExtractPropertyBagConfiguration
    {
        [JsonPropertyName("valuesToPreserve")]
        internal List<string> ValuesToPreserve { get; set; }
    }
}
