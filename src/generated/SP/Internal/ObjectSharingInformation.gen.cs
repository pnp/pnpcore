using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ObjectSharingInformation object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ObjectSharingInformation : BaseDataModel<IObjectSharingInformation>, IObjectSharingInformation
    {

        #region New properties

        public string AnonymousEditLink { get => GetValue<string>(); set => SetValue(value); }

        public string AnonymousViewLink { get => GetValue<string>(); set => SetValue(value); }

        public bool CanBeShared { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanBeUnshared { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanManagePermissions { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasPendingAccessRequests { get => GetValue<bool>(); set => SetValue(value); }

        public bool HasPermissionLevels { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsFolder { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSharedWithCurrentUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSharedWithGuest { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSharedWithMany { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSharedWithSecurityGroup { get => GetValue<bool>(); set => SetValue(value); }

        public string PendingAccessRequestsLink { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("SharedWithUsersCollection", Expandable = true)]
        public IObjectSharingInformationUserCollection SharedWithUsersCollection
        {
            get
            {
                if (!HasValue(nameof(SharedWithUsersCollection)))
                {
                    var collection = new ObjectSharingInformationUserCollection(this.PnPContext, this, nameof(SharedWithUsersCollection));
                    SetValue(collection);
                }
                return GetValue<IObjectSharingInformationUserCollection>();
            }
        }

        #endregion

    }
}
