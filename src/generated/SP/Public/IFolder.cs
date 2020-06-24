using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Folder object
    /// </summary>
    [ConcreteType(typeof(Folder))]
    public interface IFolder : IDataModel<IFolder>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsWOPIEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ProgID { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ServerRelativeUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeCreated { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime TimeLastModified { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid UniqueId { get; set; }

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

    }
}
