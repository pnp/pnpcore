using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Teams ACE Properties
    /// </summary>
    public class TeamsACEProperties : ACEProperties
    {
        /// <summary>
        /// Teams ACE app
        /// </summary>
        [JsonPropertyName("selectedApp")]
        public TeamsACEApp SelectedApp { get; set; }
    }

    /// <summary>
    /// Teams ACE app
    /// </summary>
    public class TeamsACEApp
    {
        /// <summary>
        /// ACE app id
        /// </summary>
        [JsonPropertyName("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// ACE description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// ACE title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// ACE distributionmethod
        /// </summary>
        [JsonPropertyName("distributionMethod")]
        public string DistributionMethod { get; set; }

        /// <summary>
        /// ACE icon properties
        /// </summary>
        [JsonPropertyName("iconProperties")]
        public TeamsACEAppIconProperties IconProperties { get; set; }

        /// <summary>
        /// ACE is bot
        /// </summary>
        [JsonPropertyName("isBot")]
        public bool IsBot { get; set; }
    }

    /// <summary>
    /// Teams ACE app icon properties
    /// </summary>
    public class TeamsACEAppIconProperties
    {
        /// <summary>
        /// Color icon web url
        /// </summary>
        [JsonPropertyName("colorIconWebUrl")]
        public string ColorIconWebUrl { get; set; }

        /// <summary>
        /// Outline icon web url
        /// </summary>
        [JsonPropertyName("outlineIconWebUrl")]
        public string OutlineIconWebUrl { get; set; }
    }
}
