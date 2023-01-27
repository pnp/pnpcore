using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a Folder object
    /// </summary>
    [ConcreteType(typeof(Folder))]
    public interface IFolder : IDataModel<IFolder>, IDataModelGet<IFolder>, IDataModelLoad<IFolder>, IDataModelUpdate, IDataModelDelete, IDataModelSupportingGetChanges, IQueryableDataModel
    {
        /// <summary>
        /// Gets whether the folder exists,
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// Indicate whether the folder is enabled for WOPI default action.
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
        /// Gets a string that identifies the application in which the folder was created.
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

        /// <summary>
        /// Default ordered list of content types on a list, before adjustments
        /// </summary>
        public IContentTypeIdCollection ContentTypeOrder { get; }

        /// <summary>
        /// Ordered list of content types on a list: controls order of items in the "New" menu and "List Settings" page
        /// </summary>
        public IContentTypeIdCollection UniqueContentTypeOrder { get; }

        /// <summary>
        /// Gets the list item field values for the list item corresponding to the file.
        /// </summary>
        public IListItem ListItemAllFields { get; }

        /// <summary>
        /// Gets the parent list folder of the folder.
        /// </summary>
        public IFolder ParentFolder { get; }

        /// <summary>
        /// Gets the collection of all properties defined for this folder.
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// Get the storage metrics of the folder.
        /// </summary>
        public IStorageMetrics StorageMetrics { get; }

        /// <summary>
        /// Gets the collection of list folders contained in the list folder.
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IFolderCollection Folders { get; }

        /// <summary>
        /// Gets the collection of files contained in the folder
        /// </summary>
        public IFileCollection Files { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #region Add Folder
        /// <summary>
        /// Add a folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public Task<IFolder> AddFolderAsync(string name);

        /// <summary>
        /// Add a folder to the current folder.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolder(string name);

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public Task<IFolder> AddFolderBatchAsync(string name);

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolderBatch(string name);

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <param name="batch">Batch to add the reques to</param>
        /// <returns>The added folder.</returns>
        public Task<IFolder> AddFolderBatchAsync(Batch batch, string name);

        /// <summary>
        /// Add a folder to the current folder via batch.
        /// </summary>
        /// <param name="name">The name of the folder to add.</param>
        /// <param name="batch">Batch to add the reques to </param>
        /// <returns>The added folder.</returns>
        public IFolder AddFolderBatch(Batch batch, string name);
        #endregion

        #region CopyTo
        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        Task CopyToAsync(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        void CopyTo(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        Task CopyToBatchAsync(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        void CopyToBatch(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        Task CopyToBatchAsync(Batch batch, string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a folder to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the copy operation.</param>
        void CopyToBatch(Batch batch, string destinationUrl, MoveCopyOptions options = null);
        #endregion

        #region MoveTo
        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        Task MoveToAsync(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        void MoveTo(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        Task MoveToBatchAsync(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        void MoveToBatch(string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        Task MoveToBatchAsync(Batch batch, string destinationUrl, MoveCopyOptions options = null);

        /// <summary>
        /// Moves a folder to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="options">options of the move operation.</param>
        void MoveToBatch(Batch batch, string destinationUrl, MoveCopyOptions options = null);
        #endregion

        #region EnsureFolder
        /// <summary>
        /// Ensures a (hiarchy) of folders exists on a given folder
        /// </summary>
        /// <param name="folderRelativeUrl">a (hiarchy) of folders (e.g. folderA/folderB/FolderC) </param>
        /// <returns>The <see cref="IFolder"/> representing the final folder in the hiarchy (e.g. FolderC)</returns>
        public Task<IFolder> EnsureFolderAsync(string folderRelativeUrl);

        /// <summary>
        /// Ensures a (hiarchy) of folders exists on a given folder
        /// </summary>
        /// <param name="folderRelativeUrl">a (hiarchy) of folders (e.g. folderA/folderB/FolderC) </param>
        /// <returns>The <see cref="IFolder"/> representing the final folder in the hiarchy (e.g. FolderC)</returns>
        public IFolder EnsureFolder(string folderRelativeUrl);
        #endregion

        #region Rename
        /// <summary>
        /// Renames a folder
        /// </summary>
        /// <param name="name">New folder name</param>
        /// <returns></returns>
        public Task RenameAsync(string name);

        /// <summary>
        /// Renames a folder
        /// </summary>
        /// <param name="name">New folder name</param>
        /// <returns></returns>
        public void Rename(string name);
        #endregion

        #region Syntex support
        /// <summary>
        /// Classifies and extracts all unprocessed files in this folder and it's sub folders via the Syntex off-peak queue
        /// </summary>
        /// <returns>Information about the created classify and extract requests</returns>
        Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractOffPeakAsync();

        /// <summary>
        /// Classifies and extracts all unprocessed files in this folder and it's sub folders via the Syntex off-peak queue
        /// </summary>
        /// <returns>Information about the created classify and extract requests</returns>
        ISyntexClassifyAndExtractResult ClassifyAndExtractOffPeak();

        #endregion

        #region Files
        /// <summary>
        /// Find files in the folder, can be slow as it iterates over all the files in the folder and it's sub folders. If performance
        /// is key, then try using a search based solution
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of found files as type <see cref="IFile"/></returns>
        public Task<List<IFile>> FindFilesAsync(string match);

        /// <summary>
        /// Find files in the folder, can be slow as it iterates over all the files in the folder and it's sub folders. If performance
        /// is key, then try using a search based solution
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of found files as type <see cref="IFile"/></returns>
        public List<IFile> FindFiles(string match);

        #endregion

        #region GraphPermissions

        /// <summary>
        /// Gets the share links on the file item
        /// </summary>
        /// <returns>Collection of share links existing on the file</returns>
        Task<IGraphPermissionCollection> GetShareLinksAsync();

        /// <summary>
        /// Gets the share links on the file item
        /// </summary>
        /// <returns>Collection of share links existing on the file</returns>
        IGraphPermissionCollection GetShareLinks();

        /// <summary>
        /// Deletes the share links on the file item
        /// </summary>
        Task DeleteShareLinksAsync();

        /// <summary>
        /// Deletes the share links on the file item
        /// </summary>
        void DeleteShareLinks();

        /// <summary>
        /// Creates an anonymous sharing link for a file
        /// </summary>
        /// <param name="anonymousLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateAnonymousSharingLinkAsync(AnonymousLinkOptions anonymousLinkOptions);

        /// <summary>
        /// Creates an anonymous sharing link for a file
        /// </summary>
        /// <param name="anonymousLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateAnonymousSharingLink(AnonymousLinkOptions anonymousLinkOptions);

        /// <summary>
        /// Creates an organization sharing link for a file
        /// </summary>
        /// <param name="organizationalLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateOrganizationalSharingLinkAsync(OrganizationalLinkOptions organizationalLinkOptions);

        /// <summary>
        /// Creates an organization sharing link for a file
        /// </summary>
        /// <param name="organizationalLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateOrganizationalSharingLink(OrganizationalLinkOptions organizationalLinkOptions);

        /// <summary>
        /// Creates a user sharing link for a file
        /// </summary>
        /// <param name="userLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateUserSharingLinkAsync(UserLinkOptions userLinkOptions);

        /// <summary>
        /// Creates a user sharing link for a file
        /// </summary>
        /// <param name="userLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateUserSharingLink(UserLinkOptions userLinkOptions);

        /// <summary>
        /// Creates a sharing invite to a specific user
        /// </summary>
        /// <param name="inviteOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateSharingInviteAsync(InviteOptions inviteOptions);

        /// <summary>
        /// Creates a sharing invite to a specific user
        /// </summary>
        /// <param name="inviteOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateSharingInvite(InviteOptions inviteOptions);

        #endregion
    }
}
