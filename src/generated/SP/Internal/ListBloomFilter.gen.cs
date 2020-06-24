using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a ListBloomFilter object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class ListBloomFilter : BaseDataModel<IListBloomFilter>, IListBloomFilter
    {

        #region New properties

        public int BloomFilterSize { get => GetValue<int>(); set => SetValue(value); }

        public int ItemProcessedCount { get => GetValue<int>(); set => SetValue(value); }

        public int K { get => GetValue<int>(); set => SetValue(value); }

        public int LastListItemIdProcessed { get => GetValue<int>(); set => SetValue(value); }

        public int MaxItemCount { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
