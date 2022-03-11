namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents data transfer object for HomeSiteReference
    /// </summary>
    internal sealed class HomeSiteReference
    {
        /// <summary>
        /// Home site id
        /// </summary>
        public string SiteId { get; set; }

        /// <summary>
        /// Home site title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Home site url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Home site web id
        /// </summary>
        public string WebId { get; set; }
    }
}
