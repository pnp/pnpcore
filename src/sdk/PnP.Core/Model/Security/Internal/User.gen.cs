namespace PnP.Core.Model.Security
{
    internal partial class User: BaseDataModel<IUser>, IUser
    {
        [GraphProperty("id")]
        public string GraphId { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Id")]
        public int SharePointId { get => GetValue<int>(); set => SetValue(value); }

        [GraphProperty("displayName")]
        [SharePointProperty("Title")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }
        
        public string Department { get => GetValue<string>(); set => SetValue(value); }
        
        [SharePointProperty("Email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }
        
        public string MailNickname { get => GetValue<string>(); set => SetValue(value); }
        
        public string OfficeLocation { get => GetValue<string>(); set => SetValue(value); }
        
        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        public string Expiration { get => GetValue<string>(); set => SetValue(value); }

        public bool IsEmailAuthenticationGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsShareByEmailGuestUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHiddenInUI { get => GetValue<bool>(); set => SetValue(value); }

        public PrincipalType PrincipalType { get => GetValue<PrincipalType>(); set => SetValue(value); }

        [SharePointProperty("LoginName")]
        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("UserPrincipalName")]
        public override object Key { get => this.UserPrincipalName; set => this.UserPrincipalName = value.ToString(); }
    }
}
