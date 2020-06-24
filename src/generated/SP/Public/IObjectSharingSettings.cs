using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ObjectSharingSettings object
    /// </summary>
    [ConcreteType(typeof(ObjectSharingSettings))]
    public interface IObjectSharingSettings : IDataModel<IObjectSharingSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AccessRequestMode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool BlockPeoplePickerAndSharing { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserManageOrganizationReadonlyLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserManageOrganizationReadWriteLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserManageReadonlyLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserManageReadWriteLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserRetrieveOrganizationReadonlyLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserRetrieveOrganizationReadWriteLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserRetrieveReadonlyLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserRetrieveReadWriteLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserShareExternally { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanCurrentUserShareInternally { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanSendEmail { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanSendLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool CanShareFolder { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DefaultShareLinkPermission { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DefaultShareLinkType { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnforceIBSegmentFiltering { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasEditRole { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool HasReadRole { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string InheritingWebLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsGuestUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsPictureLibrary { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsUserSiteAdmin { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ItemUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PermissionsOnlyMode { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int RequiredAnonymousLinkExpirationInDays { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShareByEmailEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool ShowExternalSharingWarning { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool SupportsAclPropagation { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IObjectSharingInformation ObjectSharingInformation { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISharePointSharingSettings SharePointSettings { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public ISharingPermissionInformationCollection SharingPermissions { get; }

        #endregion

    }
}
