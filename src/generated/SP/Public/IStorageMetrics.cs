using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a StorageMetrics object
    /// </summary>
    [ConcreteType(typeof(StorageMetrics))]
    public interface IStorageMetrics : IDataModel<IStorageMetrics>, IDataModelGet<IStorageMetrics>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastModified { get; }

        #endregion

        #region New properties

        #endregion

    }
}
