using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Page component pre configuration properties
    /// </summary>
    /// <typeparam name="T">Type of component properties</typeparam>
    internal sealed class PageComponentPreconfiguration<T>
    {
        /// <summary>
        /// Preconfiguration icon image url
        /// </summary>
        [JsonPropertyName("iconImageUrl")]
        public string IconImageUrl { get; set; }

        /// <summary>
        /// Preconfiguration properties
        /// </summary>
        [JsonPropertyName("properties")]
        public T Properties { get; set; }
    }
}
