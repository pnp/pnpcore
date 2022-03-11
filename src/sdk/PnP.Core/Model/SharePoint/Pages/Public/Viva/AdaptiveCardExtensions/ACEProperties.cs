using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
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
        public ACEData AceData { get; internal set; } = new ACEData();
        
        /// <summary>
        /// ACE Title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; internal set; }
        
        /// <summary>
        /// ACE Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; internal set; }
        
        /// <summary>
        /// ACE Icon property - usually a link to icon
        /// </summary>
        [JsonPropertyName("iconProperty")]
        public string IconProperty { get; internal set; }
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
