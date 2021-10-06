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

        // TODO: Need to write test methods for this
        //#region RecycleById methods

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public Guid RecycleById(int id);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public Task<Guid> RecycleByIdAsync(int id);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public void RecycleByIdBatch(int id);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public Task RecycleByIdBatchAsync(int id);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public void RecycleByIdBatch(Batch batch, int id);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="id">Id of the file version to recycle</param>
        ///// <returns></returns>
        //public Task RecycleByIdBatchAsync(Batch batch, int id);

        //#endregion

        // TODO:
        //#region RecycleByLabel methods

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public Guid RecycleByLabel(string label);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public Task<Guid> RecycleByLabelAsync(string label);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public void RecycleByLabelBatch(string label);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public Task RecycleByLabelBatchAsync(string label);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public void RecycleByLabelBatch(Batch batch, string label);

        ///// <summary>
        ///// Recycle the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="label">Label of the file version to recycle</param>
        ///// <returns></returns>
        //public Task RecycleByLabelBatchAsync(Batch batch, string label);

        //#endregion

        //#region RestoreByLabel methods

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public Guid RestoreByLabel(string label);

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public Task<Guid> RestoreByLabelAsync(string label);

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public void RestoreByLabelBatch(string label);

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public Task RestoreByLabelBatchAsync(string label);

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public void RestoreByLabelBatch(Batch batch, string label);

        ///// <summary>
        ///// Restore the file version
        ///// </summary>
        ///// <param name="batch">Batch to add the request to</param>
        ///// <param name="label">Label of the file version to restore</param>
        ///// <returns></returns>
        //public Task RestoreByLabelBatchAsync(Batch batch, string label);

        //#endregion
    }
}
