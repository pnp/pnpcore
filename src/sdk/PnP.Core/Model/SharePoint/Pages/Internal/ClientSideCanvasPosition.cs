using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Base class representing the json control data that will describe a control versus the zones and sections on a page
    /// </summary>
    public class ClientSideCanvasPosition
    {

        /// <summary>
        /// Gets or sets JsonProperty "zoneIndex"
        /// </summary>
        [JsonPropertyName("zoneIndex")]
        public float ZoneIndex { get; set; }
        /// <summary>
        /// Gets or sets JsonProperty "sectionIndex"
        /// </summary>
        [JsonPropertyName("sectionIndex")]
        public int SectionIndex { get; set; }
        /// <summary>
        /// Gets or sets JsonProperty "sectionFactor"
        /// </summary>
        //[JsonProperty(PropertyName = "sectionFactor", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("sectionFactor")]
        public int? SectionFactor { get; set; }
        /// <summary>
        /// Gets or sets JsonProperty "layoutIndex"
        /// </summary>
        //[JsonProperty(PropertyName = "layoutIndex", NullValueHandling = NullValueHandling.Ignore)]
        [JsonPropertyName("layoutIndex")]
        public int? LayoutIndex { get; set; }
    }
}
