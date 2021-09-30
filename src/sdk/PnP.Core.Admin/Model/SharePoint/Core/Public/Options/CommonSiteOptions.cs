namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Generic site collection creation options that apply for all types of site collections
    /// </summary>
    public abstract class CommonSiteOptions
    {
        /// <summary>
        /// The language to use for the site. 
        /// </summary>
        public Language Language { get; set; }

        /// <summary>
        /// The Web template to use for the site.
        /// </summary>
        public string WebTemplate { get; protected set; }

    }
}
