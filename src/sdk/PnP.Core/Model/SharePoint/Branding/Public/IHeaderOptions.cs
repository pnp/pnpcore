namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site header chrome
    /// </summary>
    public interface IHeaderOptions
    {
        /// <summary>
        /// Gets or sets the value of the header layout.
        /// </summary>
        HeaderLayoutType Layout { get; set; }

        /// <summary>
        /// Gets or sets the value of the header emphasis.
        /// </summary>
        VariantThemeType Emphasis { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the title in header is hidden on the site.
        /// </summary>
        bool HideTitle { get; set; }

        /// <summary>
        /// Gets or sets the logo alignment of the site.
        /// </summary>
        public LogoAlignment LogoAlignment { get; set; }
    }
}
