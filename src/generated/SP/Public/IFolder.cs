using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Folder object
    /// </summary>
    [ConcreteType(typeof(Folder))]
    public interface IFolder : IDataModel<IFolder>, IDataModelGet<IFolder>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsWOPIEnabled { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemCount { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ProgID { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeLastModified { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string WelcomePage { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFileCollection Files { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IListItem ListItemAllFields { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolder ParentFolder { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IStorageMetrics StorageMetrics { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IFolderCollection Folders { get; }

        #endregion

        #region New properties

        #endregion

    }
}
