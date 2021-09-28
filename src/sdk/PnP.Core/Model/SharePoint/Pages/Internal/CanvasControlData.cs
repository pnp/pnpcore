using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will be included in each client side control (de-)serialization (data-sp-controldata attribute)
    /// </summary>
    internal class CanvasControlData
    {

        [JsonPropertyName("controlType")]
        public int ControlType { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("position")]
        public CanvasControlPosition Position { get; set; }

        [JsonPropertyName("emphasis")]
        public SectionEmphasis Emphasis { get; set; }

        [JsonPropertyName("zoneGroupMetadata")]
        public SectionZoneGroupMetadata ZoneGroupMetadata { get; set; }
    }
}
