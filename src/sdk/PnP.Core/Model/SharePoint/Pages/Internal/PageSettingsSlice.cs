using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class PageSettingsSlice
    {
        [JsonPropertyName("isDefaultDescription")]
        public bool? IsDefaultDescription { get; set; }

        [JsonPropertyName("isDefaultThumbnail")]
        public bool? IsDefaultThumbnail { get; set; }
    }
}
