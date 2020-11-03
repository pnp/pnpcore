using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will be included in each client side control (de-)serialization (data-sp-controldata attribute)
    /// </summary>
    public class ClientSideCanvasData
    {
        /// <summary>
        /// Gets or sets JsonProperty "position"
        /// </summary>
        [JsonPropertyName("position")]
        public ClientSideCanvasPosition Position { get; set; }

        //[JsonProperty(PropertyName = "emphasis", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("emphasis")]
        public ClientSideSectionEmphasis Emphasis { get; set; }

        //[JsonProperty(PropertyName = "pageSettingsSlice", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("pageSettingsSlice")]
        public ClientSidePageSettingsSlice PageSettingsSlice { get; set; }
    }
}
