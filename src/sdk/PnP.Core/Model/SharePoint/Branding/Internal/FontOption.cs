using System.Text.Json.Serialization;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// JsonPropertyName are defined to match the property names in the JSON when deserializing the WebProperties in BrandingManager.cs
    /// </summary>
    internal sealed class FontOption : IFontOption
    {
        /// <summary>
        /// fontFamilyKey
        /// </summary>
        [JsonPropertyName("fontFamilyKey")]
        public string FamilyKey { get; set; }
        /// <summary>
        /// fontFace
        /// </summary>
        [JsonPropertyName("fontFace")]
        public string Face { get; set; }
        /// <summary>
        /// fontVariantWeight
        /// </summary>
        [JsonPropertyName("fontVariantWeight")]
        public string VariantWeight { get; set; }
    }
}
