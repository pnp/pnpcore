using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will describe a control versus the zones and sections on a page
    /// </summary>
    internal class CanvasPosition
    {

        /// <summary>
        /// Gets or sets JsonProperty "zoneIndex"
        /// </summary>
        [JsonPropertyName("zoneIndex")]
        [JsonConverter(typeof(IndexJsonConverter))]
        public float ZoneIndex { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "sectionIndex"
        /// </summary>
        [JsonPropertyName("sectionIndex")]
        [JsonConverter(typeof(IndexJsonConverter))] 
        public float SectionIndex { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "sectionFactor"
        /// </summary>
        [JsonPropertyName("sectionFactor")]
        public int? SectionFactor { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "layoutIndex"
        /// </summary>
        [JsonPropertyName("layoutIndex")]
        public int? LayoutIndex { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "zoneId"
        /// </summary>
        [JsonPropertyName("zoneId")]
        public string ZoneId { get; set; }
    }
}
