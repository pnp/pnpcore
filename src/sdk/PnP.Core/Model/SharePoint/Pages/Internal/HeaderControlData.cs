using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Json header control data that will be included in each client side web part (de-)serialization (data-sp-controldata)
    /// </summary>
    internal class HeaderControlData
    {
        /// <summary>
        /// Gets or sets JsonProperty "id"
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "instanceId"
        /// </summary>
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "title"
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "description"
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }


        [JsonPropertyName("serverProcessedContent")]
        public string ServerProcessedContent { get; internal set; }

        /// <summary>
        /// Gets or sets JsonProperty "dataVersion"
        /// </summary>
        [JsonPropertyName("dataVersion")]
        public string DataVersion { get; set; }

        /// <summary>
        /// Gets or sets JsonProperty "properties"
        /// </summary>
        [JsonPropertyName("properties")]
        public string Properties { get; set; }

    }
}
