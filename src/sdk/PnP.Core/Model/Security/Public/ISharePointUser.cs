using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a SharePoint User 
    /// </summary>
    [ConcreteType(typeof(SharePointUser))]
    public interface ISharePointUser : IDataModel<ISharePointUser>, ISharePointPrincipal, IQueryableDataModel
    {
        /// <summary>
        /// Id of the underlying graph object (if any)
        /// </summary>
        public string AadObjectId { get; set; }

        /// <summary>
        /// User principle name (UPN) of the user
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Email adress of the user
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// Department of the user
        /// </summary>
        public string Department { get; set; }

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
        /// Returns this Graph user as a SharePoint user for the connected site collection
        /// </summary>
        /// <returns></returns>
        public Task<IGraphUser> AsGraphUserAsync();

        /// <summary>
        /// Returns this Graph user as a SharePoint user for the connected site collection
        /// </summary>
        /// <returns></returns>
        public IGraphUser AsGraphUser();
    }
}
