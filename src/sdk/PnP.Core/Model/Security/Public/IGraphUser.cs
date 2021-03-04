using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a Microsoft 365 user
    /// </summary>
    [ConcreteType(typeof(GraphUser))]
    public interface IGraphUser : IDataModel<IGraphUser>, IDataModelGet<IGraphUser>, IDataModelLoad<IGraphUser>, IGraphPrincipal, IQueryableDataModel
    {
        /// <summary>
        /// User principle name (UPN) of the user
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Email adress of the user
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// Office location of the user
        /// </summary>
        public string OfficeLocation { get; set; }

        /// <summary>
        /// Returns this Graph user as a SharePoint user for the connected site collection
        /// </summary>
        /// <returns></returns>
        public Task<ISharePointUser> AsSharePointUserAsync();

        /// <summary>
        /// Returns this Graph user as a SharePoint user for the connected site collection
        /// </summary>
        /// <returns></returns>
        public ISharePointUser AsSharePointUser();
    }
}
