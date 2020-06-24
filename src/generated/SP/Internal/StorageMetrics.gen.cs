using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a StorageMetrics object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class StorageMetrics : BaseDataModel<IStorageMetrics>, IStorageMetrics
    {

        #region New properties

        public DateTime LastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        #endregion

    }
}
