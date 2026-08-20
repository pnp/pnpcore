using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// specify font to use for header / footer
    /// </summary>
    public interface IFontOption
    {
        /// <summary>
        /// fontFamilyKey
        /// </summary>
        [JsonPropertyName("fontFamilyKey")]
        string FamilyKey { get; set; }

        /// <summary>
        /// fontFace
        /// </summary>
        [JsonPropertyName("fontFace")]
        string Face { get; set; }

        /// <summary>
        /// fontVariantWeight
        /// </summary>
        [JsonPropertyName("fontVariantWeight")]
        string VariantWeight { get; set; }
    }
}
