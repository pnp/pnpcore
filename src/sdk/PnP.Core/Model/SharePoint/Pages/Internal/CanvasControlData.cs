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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("emphasis")]
        public SectionEmphasis Emphasis { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("zoneGroupMetadata")]
        public SectionZoneGroupMetadata ZoneGroupMetadata { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("zoneReflowStrategy")]
        public CanvasColumnZoneReflowStrategy ZoneReflowStrategy { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("flexibleLayoutPosition")]
        public CanvasControlFlexibleLayoutPosition FlexibleLayoutPosition { get; set; }
    }
}
