using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    public class ClientSideSectionEmphasis
    {
        //[JsonProperty(PropertyName = "zoneEmphasis", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("zoneEmphasis")]
        [JsonConverter(typeof(EmphasisJsonConverter))]
        public int ZoneEmphasis
        {
            get; set;
        }
    }
}
