using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Group class, write your custom code here
    /// </summary>
    [SharePointType("SP.Group", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Group : BaseDataModel<IGroup>, IGroup
    {
        #region Construction
        public Group()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AllowMembersEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool AutoAcceptRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserEditMembership { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserManageGroup { get => GetValue<bool>(); set => SetValue(value); }

        public bool CanCurrentUserViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public bool OnlyAllowMembersViewMembership { get => GetValue<bool>(); set => SetValue(value); }

        public string OwnerTitle { get => GetValue<string>(); set => SetValue(value); }

        public string RequestToJoinLeaveEmailSetting { get => GetValue<string>(); set => SetValue(value); }

        public IPrincipal Owner { get => GetModelValue<IPrincipal>(); }


        public IUserCollection Users { get => GetModelCollectionValue<IUserCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
