using PnP.Core.Model.Teams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint.Pages.Public.Viva.AdaptiveCardExtensions
{
    /// <summary>
    /// Teams ACE Properties
    /// </summary>
    public class TeamsACEProperties : ACEProperties
    {
        [JsonPropertyName("selectedApp")]
        public TeamsACEApp SelectedApp { get; set; }
    }
    public class TeamsACEApp
    {
        [JsonPropertyName("appId")]
        public string AppId { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("distributionMethod")]
        public string DistributionMethod { get; set; }
        [JsonPropertyName("iconProperties")]
        public TeamsACEAppIconProperties IconProperties { get; set; }
        [JsonPropertyName("isBot")]
        public bool IsBot { get; set; }
    }
    public class TeamsACEAppIconProperties
    {
        [JsonPropertyName("colorIconWebUrl")]
        public string ColorIconWebUrl { get; set; }
        [JsonPropertyName("outlineIconWebUrl")]
        public string OutlineIconWebUrl { get; set; }
    }
}
