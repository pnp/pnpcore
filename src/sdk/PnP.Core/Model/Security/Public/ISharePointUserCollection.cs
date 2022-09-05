using PnP.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.Security
{
    /// <summary>
    /// Public interface to define a collection of SharePoint users
    /// </summary>
    [ConcreteType(typeof(SharePointUserCollection))]
    public interface ISharePointUserCollection : IQueryable<ISharePointUser>, IAsyncEnumerable<ISharePointUser>, IDataModelCollection<ISharePointUser>, IDataModelCollectionLoad<ISharePointUser>, ISupportModules<ISharePointUserCollection>
    {

        /// <summary>
        /// Adds a new user
        /// </summary>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public ISharePointUser Add(string userLoginName);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public ISharePointUser AddBatch(string userLoginName);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public ISharePointUser AddBatch(Batch batch, string userLoginName);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public Task<ISharePointUser> AddAsync(string userLoginName);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public Task<ISharePointUser> AddBatchAsync(string userLoginName);

        /// <summary>
        /// Adds a new group
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="userLoginName">login name of user to add e.g. "i:0#.f|membership|user@domain.com"</param>
        /// <returns></returns>
        public Task<ISharePointUser> AddBatchAsync(Batch batch, string userLoginName);

    }
}
