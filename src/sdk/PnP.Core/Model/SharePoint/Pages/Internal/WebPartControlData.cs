using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Control data for controls of type 3 (= client side web parts)
    /// </summary>
    internal class WebPartControlData : CanvasControlData
    {
        /// <summary>
        /// Gets or sets JsonProperty "webPartId"
        /// </summary>
        [JsonPropertyName("webPartId")]
        public string WebPartId { get; set; }

        [JsonPropertyName("rteInstanceId")]
        public string RteInstanceId { get; set; }

        [JsonPropertyName("addedFromPersistedData")]
        [JsonConverter(typeof(Services.JsonMappingHelper.BoolJsonConverter))]
        public bool AddedFromPersistedData { get; set; }

        [JsonPropertyName("reservedHeight")]
        public int ReservedHeight { get; set; }

        [JsonPropertyName("reservedWidth")]
        public int ReservedWidth { get; set; }
    }
}
