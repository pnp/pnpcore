using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// When implemented it provides a DeleteById method on the collection
    /// </summary>
    public interface IDataModelCollectionDeleteByGuidId
    {
        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        void DeleteById(Guid id);

        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        Task DeleteByIdAsync(Guid id);

        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        void DeleteByIdBatch(Guid id);

        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        Task DeleteByIdBatchAsync(Guid id);

        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        void DeleteByIdBatch(Batch batch, Guid id);

        /// <summary>
        /// Delete an item from the collection via it's id
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the item to delete</param>
        /// <returns></returns>
        Task DeleteByIdBatchAsync(Batch batch, Guid id);
    }
}
