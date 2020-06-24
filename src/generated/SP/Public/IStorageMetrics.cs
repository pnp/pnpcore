using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a StorageMetrics object
    /// </summary>
    [ConcreteType(typeof(StorageMetrics))]
    public interface IStorageMetrics : IDataModel<IStorageMetrics>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime LastModified { get; set; }

        #endregion

    }
}
