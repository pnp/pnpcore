using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ObjectSharingInformation class, write your custom code here
    /// </summary>
    [SharePointType("SP.ObjectSharingInformation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ObjectSharingInformation : BaseDataModel<IObjectSharingInformation>, IObjectSharingInformation
    {
        #region Construction
        public ObjectSharingInformation()
        {
        }
        #endregion

        #region Properties
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

        public IObjectSharingInformationUserCollection SharedWithUsersCollection { get => GetModelCollectionValue<IObjectSharingInformationUserCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
