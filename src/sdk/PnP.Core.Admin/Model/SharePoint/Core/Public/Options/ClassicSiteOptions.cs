namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the available options for creating a classic site collection (e.g. classic team site)
    /// </summary>
    public class ClassicSiteOptions: CommonSiteOptions
    {
        /// <summary>
        /// Default constuctor for creating a <see cref="ClassicSiteOptions"/> object used to define a classic site collection creation
        /// </summary>
        /// <param name="url">Url of the classic site collection to create</param>
        /// <param name="title">Title of the classic site collection to create</param>
        /// <param name="webTemplate">Web template of the classic site collection to create</param>
        /// <param name="siteOwnerLogin">Owner of the classic site collection to create</param>
        /// <param name="timeZone">Time zone of the classic site collection to create</param>
        public ClassicSiteOptions(string url, string title, string webTemplate, string siteOwnerLogin, TimeZone timeZone)
        {
            Url = url;
            Title = title;
            WebTemplate = webTemplate;
            Owner = siteOwnerLogin;
            TimeZone = timeZone;
        }

        /// <summary>
        /// Title of the classic site
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Url of the classic site
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Owner of the classic site
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Time zone id for the classic site
        /// </summary>
        public TimeZone TimeZone { get; set; }
    }
}
