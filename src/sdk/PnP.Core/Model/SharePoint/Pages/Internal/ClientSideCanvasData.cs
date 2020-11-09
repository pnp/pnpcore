using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will be included in each client side control (de-)serialization (data-sp-controldata attribute)
    /// </summary>
    internal class ClientSideCanvasData
    {
        /// <summary>
        /// Gets or sets JsonProperty "position"
        /// </summary>
        [JsonPropertyName("position")]
        public ClientSideCanvasPosition Position { get; set; }

        [JsonPropertyName("emphasis")]
        public ClientSideSectionEmphasis Emphasis { get; set; }

        [JsonPropertyName("pageSettingsSlice")]
        public ClientSidePageSettingsSlice PageSettingsSlice { get; set; }
    }
}
