using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions
{
    /// <summary>
    /// Represents common properties of ACEs
    /// </summary>
    public class ACEProperties
    {
        /// <summary>
        /// Shared Adaptive Card properties
        /// </summary>
        [JsonPropertyName("aceData")]
        public ACEData AceData { get; set; } = new ACEData();
        /// <summary>
        /// ACE Title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }
        /// <summary>
        /// ACE Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }
        /// <summary>
        /// ACE Icon property - usually a link to icon
        /// </summary>
        [JsonPropertyName("iconProperty")]
        public string IconProperty { get; set; }
    }
    /// <summary>
    /// Represents aceDate part of ACE properties
    /// </summary>
    public class ACEData
    {
        /// <summary>
        /// Large or null for small
        /// </summary>
        [JsonPropertyName("cardSize")]
        public string CardSize { get; set; }
    }
}
