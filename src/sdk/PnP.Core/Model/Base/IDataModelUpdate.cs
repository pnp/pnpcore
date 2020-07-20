using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the update interface for Domain Model objects that need update.
    /// </summary>
    public interface IDataModelUpdate
    {
        /// <summary>
        /// Collects the request to update a Domain Model object into the remote data source 
        /// </summary>
        Task UpdateBatchAsync();

        /// <summary>
        /// Collects the request to update a Domain Model object into the remote data source using a batch
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        Task UpdateBatchAsync(Batch batch);

        /// <summary>
        /// Updates a Domain Model object into the remote data source 
        /// </summary>
        Task UpdateAsync();
    }
}
