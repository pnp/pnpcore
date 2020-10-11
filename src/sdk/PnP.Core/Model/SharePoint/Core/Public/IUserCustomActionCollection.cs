using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of UserCustomAction objects
    /// </summary>
    public interface IUserCustomActionCollection : IQueryable<IUserCustomAction>, IDataModelCollection<IUserCustomAction>
    {
        #region Add
        /// <summary>
        /// Add a new User Custom Action with the specified options
        /// </summary>
        /// <param name="options">The options to specify </param>
        /// <returns>The created User Custom Action object.</returns>
        Task<IUserCustomAction> AddAsync(AddUserCustomActionOptions options);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IUserCustomAction Add(AddUserCustomActionOptions options);

        //Task<IUserCustomAction> AddBatchAsync(Batch batch, AddUserCustomActionOptions options);

        //IUserCustomAction AddBatch(Batch batch, AddUserCustomActionOptions options);

        //Task<IUserCustomAction> AddBatchAsync(AddUserCustomActionOptions options);

        //IUserCustomAction AddBatch(AddUserCustomActionOptions options);
        #endregion
    }
}