using PnP.Core.Admin.Model.Microsoft365;
using PnP.Core.Model.Security;
using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Generic site collection creation options that apply for all types of site collections that are backed by a Microsoft 365 group
    /// </summary>
    public abstract class CommonGroupSiteOptions : CommonSiteOptions
    {
        /// <summary>
        /// Default constructor to configure the common options for group connected sites
        /// </summary>
        /// <param name="alias">Alias for the group to create</param>
        /// <param name="displayName">Display name for the group to create</param>
        public CommonGroupSiteOptions(string alias, string displayName)
        {
            if (alias == null)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            if (displayName == null)
            {
                throw new ArgumentNullException(nameof(displayName));
            }

            Alias = alias;
            DisplayName = displayName;
        }

        /// <summary>
        /// Alias of the underlying Office 365 Group
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The title of the site to create
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Defines whether the Office 365 Group will be public (default), or private.
        /// </summary>
        [Obsolete("Use the Visibility property instead")]
        public bool IsPublic { get; set; } = true;

        /// <summary>
        /// Defines whether the Office 365 Group will be public (default), or private or hiddenMembership.
        /// </summary>
        public GroupVisibility Visibility { get; set; } = GroupVisibility.Public;

        /// <summary>
        /// The description of the site to be created.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Site classification to use. For instance 'Contoso Classified'. See https://www.youtube.com/watch?v=E-8Z2ggHcS0 for more information
        /// </summary>
        public string Classification { get; set; }

        /// <summary>
        /// The Guid of the hub site to be used. If specified will associate the modern team site to the hub site. (not applicable when application permissions are used)
        /// </summary>
        public Guid HubSiteId { get; set; }

        /// <summary>
        /// The Sensitivity label to use. See https://www.youtube.com/watch?v=NxvUXBiPFcw for more information. (not applicable when application permissions are used)
        /// </summary>
        public Guid SensitivityLabelId { get; set; }

        /// <summary>
        /// SiteAlias of the underlying Office 365 Group, i.e. the site part of the url: https://contoso.sharepoint.com/sites/&lt;SiteAlias&gt; (not applicable when application permissions are used)
        /// </summary>
        public string SiteAlias { get; set; }

        /// <summary>
        /// The geography in which to create the site collection. Only applicable to multi-geo enabled tenants
        /// </summary>
        public GeoLocation? PreferredDataLocation { get; set; }

        /// <summary>
        /// Option to add custom data to the post request for creating a group like the custom property for a EducationClass
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = null;

    }
}
