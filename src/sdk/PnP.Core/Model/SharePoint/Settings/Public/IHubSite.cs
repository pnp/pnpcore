using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a HubSite object
    /// </summary>
    [ConcreteType(typeof(HubSite))]
    public interface IHubSite : IDataModel<IHubSite>, IDataModelGet<IHubSite>, IDataModelLoad<IHubSite>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// The description of the HubSite.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets if permission sync to associated sites is enabled or not.
        /// </summary>
        public bool EnablePermissionsSync { get; set; }

        /// <summary>
        /// Gets or sets the list of Enforced ECTs on the HubSite
        /// </summary>
        public string EnforcedECTs { get; set; }

        /// <summary>
        /// Gets or sets EnforcedECTs version
        /// </summary>
        public int EnforcedECTsVersion { get; set; }

        /// <summary>
        /// Gets or sets if the HubSite nav includes the name as well as the logo.
        /// </summary>
        public bool HideNameInNavigation { get; set; }

        /// <summary>
        /// The HubSite ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The logo URL of the HubSite
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// The parent hub site id to which this hub connects
        /// </summary>
        public Guid ParentHubSiteId { get; set; }

        /// <summary>
        /// Hub permissions sync tag
        /// </summary>
        public int PermissionsSyncTag { get; set; }

        /// <summary>
        /// Gets or sets if the HubSite Join requires approval.
        /// </summary>
        public bool RequiresJoinApproval { get; set; }

        /// <summary>
        /// The site design to be executed when the hub is joined.
        /// </summary>
        public Guid SiteDesignId { get; set; }

        /// <summary>
        /// The Site Collection ID of the HubSite
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// The URL of the HubSite
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// List of security groups with access to join the HubSite.
        /// Null if everyone has permission.
        /// </summary>
        public string Targets { get; set; }

        /// <summary>
        /// The tenant instance ID in which the site hub is located.
        /// Use empty guid for the default tenant instance.
        /// </summary>
        public Guid TenantInstanceId { get; set; }

        /// <summary>
        /// Specifies the name of the HubSite.
        /// </summary>
        public string Title { get; set; }

        #endregion

    }
}
