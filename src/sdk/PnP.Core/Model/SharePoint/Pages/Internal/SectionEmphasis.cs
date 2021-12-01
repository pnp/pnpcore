using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SectionEmphasis
    {
        [JsonPropertyName("zoneEmphasis")]
        [JsonConverter(typeof(EmphasisJsonConverter))]
        public int ZoneEmphasis
        {
            get; set;
        }
    }
}
