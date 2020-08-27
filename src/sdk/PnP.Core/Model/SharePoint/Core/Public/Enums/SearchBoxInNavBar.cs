namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Searchbox in navigation options
    /// </summary>
    public enum SearchBoxInNavBar
    {
        /// <summary>
        /// Inherit site config
        /// </summary>
        Inherit = 0,

        /// <summary>
        /// Show on all pages
        /// </summary>
        AllPages = 1,

        /// <summary>
        /// Show on modern pages only
        /// </summary>
        ModernOnly = 2,

        /// <summary>
        /// Don't show the search box in the navigation bar
        /// </summary>
        Hidden = 3
    }
}
