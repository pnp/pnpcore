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
        public DateTime LastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public long TotalFileCount { get => GetValue<long>(); set => SetValue(value); }

        public long TotalFileStreamSize { get => GetValue<long>(); set => SetValue(value); }

        public long TotalSize { get => GetValue<long>(); set => SetValue(value); }

        [KeyProperty(nameof(LastModified))]
        public override object Key { get => this.LastModified; set => this.LastModified = DateTime.Parse(value.ToString()); }
    }
}
