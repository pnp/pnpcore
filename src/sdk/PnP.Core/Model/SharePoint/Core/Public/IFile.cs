using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a File object
    /// </summary>
    [ConcreteType(typeof(File))]
    public interface IFile : IDataModel<IFile>, IDataModelGet<IFile>, IDataModelLoad<IFile>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        /// <summary>
        /// Returns the comment that was specified when the document was checked into the document library
        /// </summary>
        public string CheckInComment { get; }

        /// <summary>
        /// Gets a value that specifies the type of check out associated with the file.
        /// </summary>
        public CheckOutType CheckOutType { get; }

        /// <summary>
        /// Returns internal version of content, used to validate document equality for read purposes.
        /// </summary>
        public string ContentTag { get; }

        /// <summary>
        /// Gets a value that specifies the customization status of the file.
        /// </summary>
        public CustomizedPageStatus CustomizedPageStatus { get; }

        /// <summary>
        /// Gets the id of the list containing the file.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Gets a value that specifies the ETag value.
        /// </summary>
        public string ETag { get; }

        /// <summary>
        /// Gets a value that specifies whether the file exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// Gets or sets whether Irm is enabled on the file.
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// Gets the linking URI of the file.
        /// </summary>
        public string LinkingUri { get; }

        /// <summary>
        /// Gets the linking URL of the file.
        /// </summary>
        public string LinkingUrl { get; }

        /// <summary>
        /// Gets a value that specifies the major version of the file.
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        /// Gets a value that specifies the minor version of the file.
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        /// Gets the name of the file including the extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the list page render type of the file.
        /// </summary>
        public ListPageRenderType PageRenderType { get; }

        /// <summary>
        /// Level of the file (published or draft)
        /// </summary>
        public PublishedStatus Level { get; }

        /// <summary>
        /// Gets the relative URL of the file based on the URL for the server.
        /// </summary>
        public string ServerRelativeUrl { get; }

        /// <summary>
        /// Gets the Id of the Site collection in which the file is stored.
        /// </summary>
        public Guid SiteId { get; }

        /// <summary>
        ///	Gets a value that specifies when the file was created.
        /// </summary>
        public DateTime TimeCreated { get; }

        /// <summary>
        /// Gets a value that specifies when the file was last modified.
        /// </summary>
        public DateTime TimeLastModified { get; }

        /// <summary>
        /// Gets a value that specifies the display name of the file.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a value that specifies the implementation-specific version identifier of the file.
        /// </summary>
        public int UIVersion { get; }

        /// <summary>
        /// Gets a value that specifies the implementation-specific version identifier of the file.
        /// </summary>
        public string UIVersionLabel { get; }

        /// <summary>
        /// Gets the unique Id of the file.
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Gets the Id of the site in which the file is stored.
        /// </summary>
        public Guid WebId { get; }

        /// <summary>
        /// Gets a value that specifies the list item field values for the list item corresponding to the file.
        /// </summary>
        public IListItem ListItemAllFields { get; }

        /// <summary>
        /// Gets the Information Rights Management settings of the file.
        /// </summary>
        public IEffectiveInformationRightsManagementSettings EffectiveInformationRightsManagementSettings { get; }

        /// <summary>
        /// Gets the Information Rights Management settings of the file.
        /// </summary>
        public IInformationRightsManagementFileSettings InformationRightsManagementSettings { get; }

        /// <summary>
        /// Gets the properties of the file.
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// Gets a value that returns a collection of file version event objects that represent the version events of the file.
        /// </summary>
        public IFileVersionEventCollection VersionEvents { get; }

        /// <summary>
        /// Gets a value that returns a collection of file version objects that represent the versions of the file.
        /// </summary>
        public IFileVersionCollection Versions { get; }

        /// <summary>
        /// Gets a value that specifies the user who added the file.
        /// </summary>
        public ISharePointUser Author { get; }

        /// <summary>
        /// Gets a value that returns the user who has checked out the file.
        /// </summary>
        public ISharePointUser CheckedOutByUser { get; }

        /// <summary>
        /// Gets a value that returns the user who has locked the file.
        /// </summary>
        public ISharePointUser LockedByUser { get; }

        /// <summary>
        /// Gets a value that returns the last user who has modified the file.
        /// </summary>
        public ISharePointUser ModifiedBy { get; }

        #region GetContent
        /// <summary>
        /// Get the content of the file.
        /// </summary>
        /// <param name="streamContent">Already return the content before all bytes are read, needed for large file downloads</param>
        /// <returns>Stream containing the binary content of the file</returns>
        Task<Stream> GetContentAsync(bool streamContent = false);

        /// <summary>
        /// Get the content of the file.
        /// </summary>
        /// <param name="streamContent">Already return the content before all bytes are read, needed for large file downloads</param>
        /// <returns>Stream containing the binary content of the file</returns>
        Stream GetContent(bool streamContent = false);

        /// <summary>
        /// Get the content of the file.
        /// </summary>
        /// <returns>The binary content of the file</returns>
        Task<byte[]> GetContentBytesAsync();

        /// <summary>
        /// Get the content of the file.
        /// </summary>
        /// <returns>The binary content of the file</returns>
        byte[] GetContentBytes();
        #endregion

        #region Publish
        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        Task PublishAsync(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        void Publish(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        Task PublishBatchAsync(string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        void PublishBatch(string comment = null);


        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        Task PublishBatchAsync(Batch batch, string comment = null);

        /// <summary>
        /// Publish a major version of the current file.
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="comment">The comments to add on file publishing.</param>
        /// </summary>
        void PublishBatch(Batch batch, string comment = null);
        #endregion

        #region Unpublish
        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="comment">The comments to add on file unpublishing.</param>
        /// </summary>
        Task UnpublishAsync(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="comment">The comments to add on file unpublishing.</param>
        /// </summary>
        void Unpublish(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="comment">The comments to add on file unpublishing.</param>
        /// </summary>
        Task UnpublishBatchAsync(string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="comments">The comments to add on file unpublishing.</param>
        /// </summary>
        void UnpublishBatch(string comments = null);


        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="comment">The comments to add on file unpublishing.</param>
        /// </summary>
        Task UnpublishBatchAsync(Batch batch, string comment = null);

        /// <summary>
        /// Unpublish the latest major version of the current file.
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="comment">The comments to add on file unpublishing.</param>
        /// </summary>
        void UnpublishBatch(Batch batch, string comment = null);
        #endregion

        #region Checkout
        /// <summary>
        /// Checks out the file.
        /// </summary>
        Task CheckoutAsync();

        /// <summary>
        /// Checks out the file.
        /// </summary>
        void Checkout();

        /// <summary>
        /// Checks out the file.
        /// </summary>
        Task CheckoutBatchAsync();

        /// <summary>
        /// Checks out the file.
        /// </summary>
        void CheckoutBatch();

        /// <summary>
        /// Checks out the file.
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        Task CheckoutBatchAsync(Batch batch);

        /// <summary>
        /// Checks out the file.
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        void CheckoutBatch(Batch batch);
        #endregion

        #region UndoCheckout
        /// <summary>
        /// Release the checked out file without saving the changes.
        /// </summary>
        Task UndoCheckoutAsync();

        /// <summary>
        /// Release the checked out file without saving the changes.
        /// </summary>
        void UndoCheckout();

        /// <summary>
        /// Release the checked out file without saving the changes.
        /// </summary>
        Task UndoCheckoutBatchAsync();

        /// <summary>
        /// Release the checked out file without saving the changes.
        /// </summary>
        void UndoCheckoutBatch();

        /// <summary>
        /// Release the checked out file without saving the changes.
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        Task UndoCheckoutBatchAsync(Batch batch);

        /// <summary>
        /// Release the checked out file without saving the changes.
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        void UndoCheckoutBatch(Batch batch);
        #endregion

        #region Checkin
        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// </summary>
        Task CheckinAsync(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);

        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// </summary>
        void Checkin(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);

        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// </summary>
        Task CheckinBatchAsync(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);

        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// </summary>
        void CheckinBatch(string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);

        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        Task CheckinBatchAsync(Batch batch, string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);

        /// <summary>
        /// Checks in the file.
        /// <param name="comment">The check in comment.</param>
        /// <param name="checkinType">The type of check in to use.</param>
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        void CheckinBatch(Batch batch, string comment = null, CheckinType checkinType = CheckinType.MinorCheckIn);
        #endregion

        #region CopyTo
        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        Task CopyToAsync(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        void CopyTo(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination  URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        Task CopyToBatchAsync(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        void CopyToBatch(string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        Task CopyToBatchAsync(Batch batch, string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        /// <summary>
        /// Copies a file to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="overwrite">Indicates whether the file should be overwritten if already existing.</param>
        /// <param name="options">Options of the copy operation.</param>
        void CopyToBatch(Batch batch, string destinationUrl, bool overwrite = false, MoveCopyOptions options = null);

        #endregion

        #region MoveTo
        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        void MoveTo(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToBatchAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        void MoveToBatch(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToBatchAsync(Batch batch, string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        void MoveToBatch(Batch batch, string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);
        #endregion

        #region Recycle
        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        /// <returns>The Id of the created recycle bin item</returns>
        Task<Guid> RecycleAsync();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        /// <returns>The Id of the created recycle bin item</returns>
        Guid Recycle();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        Task RecycleBatchAsync();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        void RecycleBatch();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        Task RecycleBatchAsync(Batch batch);

        /// <summary>
        /// Send the file to recycle bin
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        void RecycleBatch(Batch batch);
        #endregion
    }
}
