using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListBloomFilter class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListBloomFilter", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ListBloomFilter : BaseDataModel<IListBloomFilter>, IListBloomFilter
    {
        #region Construction
        public ListBloomFilter()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public int BloomFilterSize { get => GetValue<int>(); set => SetValue(value); }

        public int ItemProcessedCount { get => GetValue<int>(); set => SetValue(value); }

        public int K { get => GetValue<int>(); set => SetValue(value); }

        public int LastListItemIdProcessed { get => GetValue<int>(); set => SetValue(value); }

        public int MaxItemCount { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
