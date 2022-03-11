using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class ACEWebPartData : WebPartData
    {
        /// <summary>
        /// Gets or sets JsonProperty "iconProperty"
        /// </summary>
        [JsonPropertyName("iconProperty")]
        public string IconProperty { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "cardSize"
        /// </summary>
        [JsonPropertyName("cardSize")]
        public string CardSize { get; set; }
    }
}
