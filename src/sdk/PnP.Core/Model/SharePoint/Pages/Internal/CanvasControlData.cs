using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will be included in each client side control (de-)serialization (data-sp-controldata attribute)
    /// </summary>
    internal class CanvasControlData
    {
        /// <summary>
        /// Gets or sets JsonProperty "controlType"
        /// </summary>
        [JsonPropertyName("controlType")]
        public int ControlType { get; set; }
        /// <summary>
        /// Gets or sets JsonProperty "id"
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets JsonProperty "position"
        /// </summary>
        [JsonPropertyName("position")]
        public CanvasControlPosition Position { get; set; }

        [JsonPropertyName("emphasis")]
        public SectionEmphasis Emphasis { get; set; }
    }
}
