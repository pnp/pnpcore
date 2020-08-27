namespace PnP.Core.Model.AzureActiveDirectory
{
    /// <summary>
    /// Public interface to define a User of Azure Active Directory
    /// </summary>
    [ConcreteType(typeof(User))]
    public interface IUser : IDataModel<IUser>
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Name of the user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Department of the user
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Email adress of the user
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// Mail nickname of the user
        /// </summary>
        public string MailNickname { get; set; }

        /// <summary>
        /// Office location of the user
        /// </summary>
        public string OfficeLocation { get; set; }

        /// <summary>
        /// User principle name (UPN) of the user
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}
