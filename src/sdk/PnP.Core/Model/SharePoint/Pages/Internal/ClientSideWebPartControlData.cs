using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Control data for controls of type 3 (= client side web parts)
    /// </summary>
    internal class ClientSideWebPartControlData : ClientSideCanvasControlData
    {
        /// <summary>
        /// Gets or sets JsonProperty "webPartId"
        /// </summary>
        [JsonPropertyName("webPartId")]
        public string WebPartId { get; set; }
    }
}
