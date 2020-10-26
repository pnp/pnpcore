namespace PnP.Core.Model.Security
{
    internal partial class SharePointUser: BaseDataModel<ISharePointUser>, ISharePointUser
    {
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("AadObjectId", JsonPath = "NameId")]
        public string AadObjectId { get => GetValue<string>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        public string Department { get => GetValue<string>(); set => SetValue(value); }
        
        [SharePointProperty("Email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }
        
        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = int.Parse(value.ToString()); }
    }
}
