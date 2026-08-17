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
        string FamilyKey { get; set; }

        /// <summary>
        /// fontFace
        /// </summary>
        string Face { get; set; }

        /// <summary>
        /// fontVariantWeight
        /// </summary>
        string VariantWeight { get; set; }
    }
}
