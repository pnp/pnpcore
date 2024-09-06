using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Collection of file versions
    /// </summary>
    [ConcreteType(typeof(FileVersionCollection))]
    public interface IFileVersionCollection : IQueryable<IFileVersion>, IAsyncEnumerable<IFileVersion>, IDataModelCollection<IFileVersion>, IDataModelCollectionLoad<IFileVersion>, ISupportModules<IFileVersionCollection>
    {
        #region GetById methods

        /// <summary>
        /// Method to select a file version by Id
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting file version instance, if any</returns>
        public IFileVersion GetById(int id, params Expression<Func<IFileVersion, object>>[] selectors);

        /// <summary>
        /// Method to select a file version by Id asynchronously
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting file version instance, if any</returns>
        public Task<IFileVersion> GetByIdAsync(int id, params Expression<Func<IFileVersion, object>>[] selectors);

        #endregion

        #region DeleteAll methods

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <returns></returns>
        public void DeleteAll();

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllAsync();

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <returns></returns>
        public void DeleteAllBatch();

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllBatchAsync();

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns></returns>
        public void DeleteAllBatch(Batch batch);

        /// <summary>
        /// Delete all file versions
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns></returns>
        public Task DeleteAllBatchAsync(Batch batch);

        #endregion

        #region DeleteById methods

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteById(int id);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByIdAsync(int id);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteByIdBatch(int id);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByIdBatchAsync(int id);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteByIdBatch(Batch batch, int id);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByIdBatchAsync(Batch batch, int id);

        #endregion

        #region DeleteByLabel methods

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteByLabel(string label);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByLabelAsync(string label);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteByLabelBatch(string label);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByLabelBatchAsync(string label);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public void DeleteByLabelBatch(Batch batch, string label);

        /// <summary>
        /// Delete the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to Delete</param>
        /// <returns></returns>
        public Task DeleteByLabelBatchAsync(Batch batch, string label);

        #endregion


        #region RecycleById methods

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleById(int id);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByIdAsync(int id);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleByIdBatch(int id);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByIdBatchAsync(int id);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleByIdBatch(Batch batch, int id);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByIdBatchAsync(Batch batch, int id);

        #endregion

        #region RecycleByLabel methods

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleByLabel(string label);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByLabelAsync(string label);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleByLabelBatch(string label);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByLabelBatchAsync(string label);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public void RecycleByLabelBatch(Batch batch, string label);

        /// <summary>
        /// Recycle the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to recycle</param>
        /// <returns></returns>
        public Task RecycleByLabelBatchAsync(Batch batch, string label);

        #endregion

        #region RestoreByLabel methods

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public void RestoreByLabel(string label);

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public Task RestoreByLabelAsync(string label);

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public void RestoreByLabelBatch(string label);

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public Task RestoreByLabelBatchAsync(string label);

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public void RestoreByLabelBatch(Batch batch, string label);

        /// <summary>
        /// Restore the file version
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="label">Label of the file version to restore</param>
        /// <returns></returns>
        public Task RestoreByLabelBatchAsync(Batch batch, string label);

        #endregion
    }
}
