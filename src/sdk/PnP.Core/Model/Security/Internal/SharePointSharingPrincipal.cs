namespace PnP.Core.Model.Security
{
    [SharePointType("SP.Sharing.Principal")]
    internal partial class SharePointSharingPrincipal : BaseDataModel<ISharePointSharingPrincipal>, ISharePointSharingPrincipal
    {
        #region Construction
        public SharePointSharingPrincipal()
        {
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public string JobTitle { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsActive { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsExternal { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = int.Parse(value.ToString()); }
        #endregion

    }
}
