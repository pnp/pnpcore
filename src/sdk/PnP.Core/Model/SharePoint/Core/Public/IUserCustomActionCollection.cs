using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of UserCustomAction objects
    /// </summary>
    [ConcreteType(typeof(UserCustomActionCollection))]
    public interface IUserCustomActionCollection : IQueryable<IUserCustomAction>, IDataModelCollection<IUserCustomAction>
    {
        #region Add
        /// <summary>
        /// Add a new User Custom Action with the specified options
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <returns>The created User Custom Action object.</returns>
        Task<IUserCustomAction> AddAsync(AddUserCustomActionOptions options);

        /// <summary>
        /// Add a new User Custom Action with the specified options
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <returns>The created User Custom Action object.</returns>
        IUserCustomAction Add(AddUserCustomActionOptions options);

        /// <summary>
        /// Add a new User Custom Action with the specified options in the specified batch instance
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <param name="batch">The instance of the batch to use</param>
        /// <returns>The created User Custom Action object.</returns>
        Task<IUserCustomAction> AddBatchAsync(Batch batch, AddUserCustomActionOptions options);

        /// <summary>
        /// Add a new User Custom Action with the specified options in the specified batch instance
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <param name="batch">The instance of the batch to use</param>
        /// <returns>The created User Custom Action object.</returns>
        IUserCustomAction AddBatch(Batch batch, AddUserCustomActionOptions options);

        /// <summary>
        /// Add a new User Custom Action with the specified options in the current batch
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <returns>The created User Custom Action object.</returns>
        Task<IUserCustomAction> AddBatchAsync(AddUserCustomActionOptions options);

        /// <summary>
        /// Add a new User Custom Action with the specified options in the current batch
        /// </summary>
        /// <param name="options">The options to specify to create the User Custom Action</param>
        /// <returns>The created User Custom Action object.</returns>
        IUserCustomAction AddBatch(AddUserCustomActionOptions options);
        #endregion
    }
}