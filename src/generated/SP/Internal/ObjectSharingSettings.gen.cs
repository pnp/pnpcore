using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ObjectSharingSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ObjectSharingSettings : BaseDataModel<IObjectSharingSettings>, IObjectSharingSettings
    {

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

        public bool SupportsAclPropagation { get => GetValue<bool>(); set => SetValue(value); }

        public string WebUrl { get => GetValue<string>(); set => SetValue(value); }

        public IObjectSharingInformation ObjectSharingInformation
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new ObjectSharingInformation
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IObjectSharingInformation>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        public ISharePointSharingSettings SharePointSettings
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new SharePointSharingSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<ISharePointSharingSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        [SharePointProperty("SharingPermissions", Expandable = true)]
        public ISharingPermissionInformationCollection SharingPermissions
        {
            get
            {
                if (!HasValue(nameof(SharingPermissions)))
                {
                    var collection = new SharingPermissionInformationCollection(this.PnPContext, this, nameof(SharingPermissions));
                    SetValue(collection);
                }
                return GetValue<ISharingPermissionInformationCollection>();
            }
        }

        #endregion

    }
}
