using PnP.Core.Model.Security;
using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a RecycleBinItem object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class RecycleBinItem : BaseDataModel<IRecycleBinItem>, IRecycleBinItem
    {
        public string AuthorEmail { get => GetValue<string>(); set => SetValue(value); }

        public string AuthorName { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByEmail { get => GetValue<string>(); set => SetValue(value); }

        public string DeletedByName { get => GetValue<string>(); set => SetValue(value); }

        public DateTime DeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string DeletedDateLocalFormatted { get => GetValue<string>(); set => SetValue(value); }

        public string DirName { get => GetValue<string>(); set => SetValue(value); }

        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        public RecycleBinItemState ItemState { get => GetValue<RecycleBinItemState>(); set => SetValue(value); }

        public RecycleBinItemType ItemType { get => GetValue<RecycleBinItemType>(); set => SetValue(value); }

        public string LeafName { get => GetValue<string>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public long Size { get => GetValue<long>(); set => SetValue(value); }

        public ISharePointUser Author { get => GetModelValue<ISharePointUser>(); }

        public ISharePointUser DeletedBy { get => GetModelValue<ISharePointUser>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
    }
}
