namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site footer chrome.
    /// </summary>
    public interface IFooterOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the footer is enabled on the site.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer layout.
        /// </summary>
        FooterLayoutType Layout { get; set; }

        /// <summary>
        /// Gets or sets the value of the footer emphasis.
        /// </summary>
        FooterVariantThemeType Emphasis { get; set; }
    }
}
