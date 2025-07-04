using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class SectionZoneReflowStrategy
    {
        /// <summary>
        /// Specifies the strategy used to reflow web parts within a zone.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("axis")]
        public ZoneReflowStrategy? Axis
        {
            get; set;
        }
    }
}
