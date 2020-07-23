using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the delete interface for Domain Model objects that need delete.
    /// </summary>
    public interface IDataModelDelete
    {
        /// <summary>
        /// Collects the request to delete a Domain Model object into the remote data source 
        /// </summary>
        Task DeleteBatchAsync();

        /// <summary>
        /// Collects the request to delete a Domain Model object into the remote data source using a batch
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        Task DeleteBatchAsync(Batch batch);

        /// <summary>
        /// Deletes a Domain Model object into the remote data source 
        /// </summary>
        Task DeleteAsync();

        /// <summary>
        /// Collects the request to delete a Domain Model object into the remote data source 
        /// </summary>
        void DeleteBatch();

        /// <summary>
        /// Collects the request to delete a Domain Model object into the remote data source using a batch
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        void DeleteBatch(Batch batch);

        /// <summary>
        /// Deletes a Domain Model object into the remote data source 
        /// </summary>
        void Delete();
    }
}
