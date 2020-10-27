using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a HubSite object
    /// </summary>
    [ConcreteType(typeof(HubSite))]
    public interface IHubSite : IDataModel<IHubSite>, IDataModelGet<IHubSite>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnablePermissionsSync { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string EnforcedECTs { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HideNameInNavigation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ParentHubSiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool RequiresJoinApproval { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteDesignId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SiteUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Targets { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid TenantInstanceId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Title { get; set; }

        #endregion

    }
}
