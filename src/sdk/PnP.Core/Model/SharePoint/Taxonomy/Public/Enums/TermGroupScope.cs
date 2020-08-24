namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Returns type of group. Possible values are 'global', 'system' and 'siteCollection'.
    /// </summary>
    public enum TermGroupScope
    {
        /// <summary>
        /// Global term group
        /// </summary>
        Global,

        /// <summary>
        /// System term group
        /// </summary>
        System,

        /// <summary>
        /// Site collection term group
        /// </summary>
        SiteCollection
    }
}
