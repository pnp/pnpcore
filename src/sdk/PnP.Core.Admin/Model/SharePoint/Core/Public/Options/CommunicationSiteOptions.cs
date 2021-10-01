using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Contains the available options for creating a communication site collection
    /// </summary>
    public class CommunicationSiteOptions : CommonNoGroupSiteOptions
    {
        /// <summary>
        /// Default constuctor for creating a <see cref="CommunicationSiteOptions"/> object used to define a communication site collection creation
        /// </summary>
        /// <param name="url">Url of the communication site to create</param>
        /// <param name="title">Title of the communication site to create</param>
        public CommunicationSiteOptions(Uri url, string title): base(url, title)
        {
            WebTemplate = PnPAdminConstants.CommunicationSiteTemplate;
        }

        /// <summary>
        /// The built-in site design to used. If both SiteDesignId and SiteDesign have been specified, the GUID specified as SiteDesignId will be used.
        /// </summary>
        public CommunicationSiteDesign SiteDesign { get; set; }

    }
}
