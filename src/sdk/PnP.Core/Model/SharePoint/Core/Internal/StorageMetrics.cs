using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// StorageMetrics class, write your custom code here
    /// </summary>
    [SharePointType("SP.StorageMetrics", Target = typeof(IFolder), Uri = "_api/web/getFolderById('{Parent.Id}')/StorageMetrics", LinqGet = "_api/web/getFolderById('{Parent.Id}')/StorageMetrics")]
    internal partial class StorageMetrics : BaseDataModel<IStorageMetrics>, IStorageMetrics
    {
        #region Construction
        public StorageMetrics()
        {

        }
        #endregion

        #region Properties
        public DateTime LastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public long TotalFileCount { get => GetValue<long>(); set => SetValue(value); }

        public long TotalFileStreamSize { get => GetValue<long>(); set => SetValue(value); }

        public long TotalSize { get => GetValue<long>(); set => SetValue(value); }

        [KeyProperty(nameof(LastModified))]
        public override object Key { get => LastModified; set => LastModified = DateTime.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion
    }
}
