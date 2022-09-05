using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Model for PageComponentManifest
    /// </summary>
    /// <typeparam name="T">Type of component properties</typeparam>
    internal sealed class PageComponentManifest<T>
    {
        /// <summary>
        /// Component alias
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; set; }

        /// <summary>
        /// Component type
        /// </summary>
        [JsonPropertyName("componentType")]
        public string ComponentType { get; set; }
        
        /// <summary>
        /// Component id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// Preconfigured component entries
        /// </summary>
        [JsonPropertyName("preconfiguredEntries")]
        public List<PageComponentPreconfiguration<T>> PreconfiguredEntries { get; set; }
    }
}
