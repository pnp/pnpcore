using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a Folder object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class Folder : BaseDataModel<IFolder>, IFolder
    {
        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsWOPIEnabled { get => GetValue<bool>(); set => SetValue(value); }

        // NOTE: Is implemented only using SPO Rest for now
        //[GraphProperty("folder.childCount")]
        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string ProgID { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRelativeUrl { get => GetValue<string>(); set => SetValue(value); }

        public DateTime TimeCreated { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime TimeLastModified { get => GetValue<DateTime>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        public string WelcomePage { get => GetValue<string>(); set => SetValue(value); }

        public IFileCollection Files { get => GetModelCollectionValue<IFileCollection>(); }

        public IListItem ListItemAllFields { get => GetModelValue<IListItem>(); }

        public IFolder ParentFolder { get => GetModelValue<IFolder>(); }

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public IStorageMetrics StorageMetrics { get => GetModelValue<IStorageMetrics>(); }

        public IFolderCollection Folders { get => GetModelCollectionValue<IFolderCollection>(); }

        [KeyProperty(nameof(UniqueId))]
        public override object Key { get => this.UniqueId; set => this.UniqueId = Guid.Parse(value.ToString()); }
    }
}
