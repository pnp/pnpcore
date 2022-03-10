using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions
{
    /// <summary>
    /// Representation of ACE QuickView
    /// </summary>
    public class QuickView
    {
        /// <summary>
        /// Serialized data to be used with quick view template
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; }
        /// <summary>
        /// Serialized Adaptive Card template
        /// </summary>
        [JsonPropertyName("template")]
        public string Template { get; set; }
        /// <summary>
        /// Identificator of the quick view
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Display name of the quick view
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}
