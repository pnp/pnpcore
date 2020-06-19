namespace PnP.Core.Model.AzureActiveDirectory
{
    /// <summary>
    /// Public interface to define a User of Azure Active Directory
    /// </summary>
    [ConcreteType(typeof(User))]
    public interface IUser : IDataModel<IUser>
    {
        public string Id { get; }

        public string DisplayName { get; set; }

        public string Department { get; set; }

        public string Mail { get; set; }

        public string MailNickname { get; set; }

        public string OfficeLocation { get; set; }

        public string UserPrincipalName { get; set; }
    }
}
