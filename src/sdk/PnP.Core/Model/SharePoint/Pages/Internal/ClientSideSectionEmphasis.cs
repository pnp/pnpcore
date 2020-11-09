using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal class ClientSideSectionEmphasis
    {
        [JsonPropertyName("zoneEmphasis")]
        [JsonConverter(typeof(EmphasisJsonConverter))]
        public int ZoneEmphasis
        {
            get; set;
        }
    }
}
