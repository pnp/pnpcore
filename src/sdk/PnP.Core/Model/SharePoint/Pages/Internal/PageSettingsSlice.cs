using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class PageSettingsSlice
    {
        [JsonPropertyName("isDefaultDescription")]
        public bool? IsDefaultDescription { get; set; }

        [JsonPropertyName("isDefaultThumbnail")]
        public bool? IsDefaultThumbnail { get; set; }

        [JsonPropertyName("isSpellCheckEnabled")]
        public bool? IsSpellCheckEnabled { get; set; }

        [JsonPropertyName("globalRichTextStylingVersion")]
        public int? GlobalRichTextStylingVersion { get; set; }

        [JsonPropertyName("rtePageSettings")]
        public RtePageSettings RtePageSettings { get; set; }

        [JsonPropertyName("isEmailReady")]
        public bool? IsEmailReady { get; set; }
    }
}
