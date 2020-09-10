using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of Users of Azure Active Directory
    /// </summary>
    public interface IUserCollection : IQueryable<IUser>, IDataModelCollection<IUser>, ISupportPaging<IUser>
    {
        /// <summary>
        /// Get a user by its User Principal Name (UPN)
        /// </summary>
        /// <param name="userPrincipalName">The identifier of the user</param>
        /// <param name="expressions">The properties to load</param>
        /// <returns>The found user. <b>null</b> if not found</returns>
        public Task<IUser> GetByUserPrincipalNameAsync(string userPrincipalName, params Expression<Func<IUser, object>>[] expressions);
    }
}
