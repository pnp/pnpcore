using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a ListBloomFilter object
    /// </summary>
    [ConcreteType(typeof(ListBloomFilter))]
    public interface IListBloomFilter : IDataModel<IListBloomFilter>, IDataModelGet<IListBloomFilter>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public int BloomFilterSize { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemProcessedCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int K { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LastListItemIdProcessed { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int MaxItemCount { get; set; }

        #endregion

    }
}
