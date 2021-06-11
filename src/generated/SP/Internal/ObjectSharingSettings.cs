using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ObjectSharingSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.ObjectSharingSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ObjectSharingSettings : BaseDataModel<IObjectSharingSettings>, IObjectSharingSettings
    {
        #region Construction
        public ObjectSharingSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AccessRequestMode { get => GetValue<bool>(); set => SetValue(value); }

        public bool BlockPeoplePickerAndSharing { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageOrganizationReadonlyLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageOrganizationReadWriteLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageReadonlyLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageReadWriteLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserRetrieveOrganizationReadonlyLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserRetrieveOrganizationReadWriteLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserRetrieveReadonlyLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserRetrieveReadWriteLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserShareExternally { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserShareInternally { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanSendEmail { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanSendLink { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanShareFolder { get => GetValue<bool>(); set => SetValue(value); }

        public int DefaultShareLinkPermission { get => GetValue<int>(); set => SetValue(value); }

        public int DefaultShareLinkType { get => GetValue<int>(); set => SetValue(value); }

        public bool EnforceIBSegmentFiltering { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnforceSPOSearch { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasEditRole { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasReadRole { get => GetValue<bool>(); set => SetValue(value); }

        public string InheritingWebLink { get => GetValue<string>(); set => SetValue(value); }

        public bool IsGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsPictureLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsUserSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public string ItemId { get => GetValue<string>(); set => SetValue(value); }

        public string ItemName { get => GetValue<string>(); set => SetValue(value); }

        public string ItemUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool PermissionsOnlyMode { get => GetValue<bool>(); set => SetValue(value); }

        public int RequiredAnonymousLinkExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public bool ShareByEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowExternalSharingWarning { get => GetValue<bool>(); set => SetValue(value); }

        public string SiteIBMode { get => GetValue<string>(); set => SetValue(value); }

        public bool SupportsAclPropagation { get => GetValue<bool>(); set => SetValue(value); }

        public string WebUrl { get => GetValue<string>(); set => SetValue(value); }

        public IObjectSharingInformation ObjectSharingInformation { get => GetModelValue<IObjectSharingInformation>(); }


        public ISharePointSharingSettings SharePointSettings { get => GetModelValue<ISharePointSharingSettings>(); }


        public ISharingPermissionInformationCollection SharingPermissions { get => GetModelCollectionValue<ISharingPermissionInformationCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
