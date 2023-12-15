using PnP.Core.Admin.Model.Microsoft365;
using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Generic site collection creation options that apply for all types of site collections that are not backed by a Microsoft 365 group
    /// </summary>
    public abstract class CommonNoGroupSiteOptions : CommonSiteOptions
    {
        /// <summary>
        /// Default constructor, sets url and title
        /// </summary>
        /// <param name="url">Url of the site to create</param>
        /// <param name="title">Title of the site to create</param>
        public CommonNoGroupSiteOptions(Uri url, string title)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            Url = url;
            Title = title;
        }

        /// <summary>
        /// The fully qualified URL (e.g. https://yourtenant.sharepoint.com/sites/mysitecollection) of the site.
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// The title of the site to create
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The owner of the site. Reserved for future use.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// If set to true sharing files by email is enabled. Defaults to false.
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// The Site classification to use. For instance 'Contoso Classified'. See https://www.youtube.com/watch?v=E-8Z2ggHcS0 for more information
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// The description to use for the site.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Guid of the site design to be used.
        /// </summary>
        public Guid SiteDesignId { get; set; }

        /// <summary>
        /// The Guid of the hub site to be used. If specified will associate the communication site to the hub site
        /// </summary>
        public Guid HubSiteId { get; set; }

        /// <summary>
        /// The Sensitivity label to use. See https://www.youtube.com/watch?v=NxvUXBiPFcw for more information.
        /// </summary>
        public Guid SensitivityLabelId { get; set; }

        /// <summary>
        /// The time zone to use for the site.
        /// </summary>
        public TimeZone? TimeZone { get; set; }
    }
}
