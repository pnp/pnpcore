namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Available footer layouts for modern sites
    /// </summary>
    public enum FooterLayoutType
    {
        /// <summary>
        /// Simple footer. This is the default layout type with one single row of links. ( Value = 0 )
        /// </summary>
        Simple = 0,
        /// <summary>
        /// Extended footer. This layout type supports columns of links with multiple links in each column. ( Value = 1 )
        /// </summary>
        Extended = 1,
        /// <summary>
        /// Stacked footer. This layout type is a combination of simple and extended, with the extended stacked above the simple. ( Value = 2 )
        /// </summary>
        Stacked = 2
    }
}