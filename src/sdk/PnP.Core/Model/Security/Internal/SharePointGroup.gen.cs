using System;

namespace PnP.Core.Model.Security
{
    internal partial class SharePointGroup : BaseDataModel<ISharePointGroup>, ISharePointGroup
    {
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public bool AllowMembersEditMembership { get => GetValue<bool>(); set => SetValue(value); }
        public bool AllowRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }
        public bool AutoAcceptRequestToJoinLeave { get => GetValue<bool>(); set => SetValue(value); }
        public bool CanCurrentUserEditMembership { get => GetValue<bool>(); set => SetValue(value); }
        public bool CanCurrentUserManageGroup { get => GetValue<bool>(); set => SetValue(value); }
        public bool CanCurrentUserViewMembership { get => GetValue<bool>(); set => SetValue(value); }
        public string Description { get => GetValue<string>(); set => SetValue(value); }
        public bool OnlyAllowMembersViewMembership { get => GetValue<bool>(); set => SetValue(value); }
        public string OwnerTitle { get => GetValue<string>(); set => SetValue(value); }
        public bool RequestToJoinLeaveEmailSetting { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = int.Parse(value.ToString()); }
    }
}
