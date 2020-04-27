namespace PnP.Core.Model.AzureActiveDirectory
{
    internal partial class User: BaseDataModel<IUser>, IUser
    {
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ExternalId { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayName { get => GetValue<string>(); set => SetValue(value); }
        
        public string Department { get => GetValue<string>(); set => SetValue(value); }
        
        public string Mail { get => GetValue<string>(); set => SetValue(value); }
        
        public string MailNickname { get => GetValue<string>(); set => SetValue(value); }
        
        public string OfficeLocation { get => GetValue<string>(); set => SetValue(value); }
        
        public string UserPrincipalName { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty("Id")]
        public override object Key { get => this.Id; set => this.Id = value.ToString(); }
    }
}
