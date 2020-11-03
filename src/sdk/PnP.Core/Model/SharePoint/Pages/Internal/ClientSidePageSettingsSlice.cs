using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    public class ClientSidePageSettingsSlice
    {
        //[JsonProperty(PropertyName = "isDefaultDescription", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("isDefaultDescription")]
        public bool? IsDefaultDescription { get; set; }

        //[JsonProperty(PropertyName = "isDefaultThumbnail", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("isDefaultThumbnail")]
        public bool? IsDefaultThumbnail { get; set; }
    }
}
