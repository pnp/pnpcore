using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a unified Azure Active Directory and SharePoint User 
    /// </summary>
    [ConcreteType(typeof(User))]
    public interface IUser : IDataModel<IUser>, ISharePointPrincipal
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public string GraphId { get; }

        /// <summary>
        /// Department of the user
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Email adress of the user
        /// </summary>
        [SharePointProperty("Email")]
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

        // TODO: Review doc and type of property
        /// <summary>
        /// Gets the expiration value of the current user
        /// </summary>
        public string Expiration { get; }

        /// <summary>
        /// Indicates whether the user is a guest user authenticated via an e-mail address
        /// </summary>
        public bool IsEmailAuthenticationGuestUser { get; }

        /// <summary>
        /// Indicates whether the user is a guest user shared by e-mail
        /// </summary>
        public bool IsShareByEmailGuestUser { get; }

        /// <summary>
        /// Indicates whether the user is a site collection administrator
        /// </summary>
        public bool IsSiteAdmin { get; }

        /// <summary>
        /// Load all properties of the user in the context of SharePoint
        /// </summary>
        /// <param name="ensureUser">
        /// Indicates whether the user should be ensured in the current site. Default value is <c>true</c>. 
        /// If not set and the current user does not exist in the current site, the SharePoint specific properties won't be loaded. 
        /// </param>
        public Task LoadSharePointPropertiesAsync(bool ensureUser = true);
    }
}
