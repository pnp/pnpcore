using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// StorageMetrics class, write your custom code here
    /// </summary>
    [SharePointType("SP.StorageMetrics", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class StorageMetrics : BaseDataModel<IStorageMetrics>, IStorageMetrics
    {
        #region Construction
        public StorageMetrics()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public DateTime LastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
