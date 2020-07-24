using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Folder object
    /// </summary>
    [ConcreteType(typeof(Folder))]
    public interface IFolder : IDataModel<IFolder>, IDataModelUpdate, IDataModelDelete
    {
        /// <summary>
        /// Gets whether the folder exists,
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// Gets whether is WOPI enabled.
        /// </summary>
        public bool IsWOPIEnabled { get; }

        /// <summary>
        /// Gets a value that specifies the count of items in the list folder.
        /// </summary>
        public int ItemCount { get; }

        /// <summary>
        /// Gets the name of the folder.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ProdId of the folder.
        /// </summary>
        public string ProgID { get; }


        /// <summary>
        /// Gets the server-relative URL of the list folder.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Gets the creation time of the folder.
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Gets the last modification time of the folder.
        /// </summary>
        public DateTime TimeLastModified { get; }

        /// <summary>
        /// Gets the Unique Id of the folder.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets or sets a value that specifies folder-relative URL for the list folder welcome page.
        /// </summary>
        public string WelcomePage { get; set; }

        //TODO: To implement...
        ///// <summary>
        ///// To update...
        ///// </summary>
        //public IFileCollection Files { get; }

        /// <summary>
        /// Gets the list item field values for the list item corresponding to the file.
        /// </summary>
        public IListItem ListItemAllFields { get; }

        /// <summary>
        /// Gets the parent list folder of the folder.
        /// </summary>
        public IFolder ParentFolder { get; }

        //TODO: To implement...
        ///// <summary>
        ///// Gets the collection of all files contained in the folder.
        ///// </summary>
        //public IPropertyValues Properties { get; }

        //TODO: To implement...
        /// <summary>
        /// Get the storage metrics of the folder.
        /// </summary>
        //public IStorageMetrics StorageMetrics { get; }

        /// <summary>
        /// Gets the collection of list folders contained in the list folder.
        /// </summary>
        public IFolderCollection Folders { get; }

        /// <summary>
        /// Add a sub folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public Task<IFolder> AddSubFolderAsync(string name);

        /// <summary>
        /// Add a sub folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public IFolder AddSubFolder(string name);

        /// <summary>
        /// Add a sub folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public Task<IFolder> AddSubFolderBatchAsync(string name);

        /// <summary>
        /// Add a sub folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <returns>The added sub folder.</returns>
        public IFolder AddSubFolderBatch(string name);

        /// <summary>
        /// Add a sub folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <param name="batch">Batch to add the reques to</param>
        /// <returns>The added sub folder.</returns>
        public Task<IFolder> AddSubFolderBatchAsync(Batch batch, string name);

        /// <summary>
        /// Add a sub folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the sub folder to add.</param>
        /// <param name="batch">Batch to add the reques to </param>
        /// <returns>The added sub folder.</returns>
        public IFolder AddSubFolderBatch(Batch batch, string name);
    }
}
