namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines the type of resource that was provided via the link to unfurl
    /// </summary>
    public enum UnfurlLinkType
    {
        /// <summary>
        /// The provided link was for a list
        /// </summary>
        List,

        /// <summary>
        /// The provided link was for a list item
        /// </summary>
        ListItem,

        /// <summary>
        /// The provided link was for document library
        /// </summary>
        Library,

        /// <summary>
        /// The provided link was for a file
        /// </summary>
        File,

        /// <summary>
        /// The provided link was for a pages library of page
        /// </summary>
        SitePagesLibrary,

        /// <summary>
        /// The provided link was for a page
        /// </summary>
        SitePage,

        /// <summary>
        /// The provided link was something else or was invalid
        /// </summary>
        Unknown
    }
}
