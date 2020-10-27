using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Folder class, write your custom code here
    /// </summary>
    [SharePointType("SP.Folder", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class Folder : BaseDataModel<IFolder>, IFolder
    {
        #region Construction
        public Folder()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

        public bool Exists { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsWOPIEnabled { get => GetValue<bool>(); set => SetValue(value); }

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


        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
