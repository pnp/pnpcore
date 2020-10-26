using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a StorageMetrics object
    /// </summary>
    [ConcreteType(typeof(StorageMetrics))]
    public interface IStorageMetrics : IDataModel<IStorageMetrics>
    {
        /// <summary>
        /// Gets the last modified date and time of the storage resource.
        /// </summary>
        public DateTime LastModified { get; }

        /// <summary>
        /// Gets the total count of files in the storage resource.
        /// </summary>
        public long TotalFileCount { get; }

        /// <summary>
        /// Gets the total stream size of the storage resource.
        /// </summary>
        public long TotalFileStreamSize { get; }

        /// <summary>
        /// Gets the total size of the storage resource.
        /// </summary>
        public long TotalSize { get; }
    }
}
