namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Options to configure a the site navigation chrome.
    /// </summary>
    public interface INavigationOptions
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the megamenu is enabled on the site.
        /// </summary>
        bool MegaMenuEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value that specifies whether the site navigation is shown on the site.
        /// </summary>
        bool Visible { get; set; }
    }
}
