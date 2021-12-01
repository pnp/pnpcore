using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SectionZoneGroupMetadata
    {
        [JsonPropertyName("type")]
        public int Type
        {
            get; set;
        }

        [JsonPropertyName("isExpanded")]
        public bool IsExpanded
        {
            get; set;
        }

        [JsonPropertyName("showDividerLine")]
        public bool ShowDividerLine
        {
            get; set;
        }

        [JsonPropertyName("iconAlignment")]
        [JsonConverter(typeof(IconAlignmentJsonConverter))]
        public string IconAlignment
        {
            get; set;
        }

        [JsonPropertyName("displayName")]
        public string DisplayName
        {
            get; set;
        }
    }
}
