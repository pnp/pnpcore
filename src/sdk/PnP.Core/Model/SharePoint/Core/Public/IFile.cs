using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a File object
    /// </summary>
    [ConcreteType(typeof(File))]
    public interface IFile : IDataModel<IFile>, IDataModelGet<IFile>, IDataModelLoad<IFile>, IDataModelUpdate, IDataModelDelete, IQueryableDataModel
    {
        #region Properties

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
        /// Indicates whether this file has alternate streams with content.
        /// </summary>
        public bool HasAlternateContentStreams { get; }

        /// <summary>
        /// Gets or sets whether Irm is enabled on the file.
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// Gets the file size of the file.
        /// </summary>
        public long Length { get; }

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
        /// Gets the URL which opens the document in Office Web Apps.
        /// </summary>
        public string ServerRedirectedUrl { get; }

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
        /// Get the Graph Drive ID of the drive to which the file belongs.
        /// </summary>
        public string VroomDriveID { get; }

        /// <summary>
        /// Get the Graph DriveItem ID of the file.
        /// </summary>
        public string VroomItemID { get; }

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
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
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

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

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

        #region Approve

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// </summary>
        Task ApproveAsync(string comment = null);

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// </summary>
        void Approve(string comment = null);

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// </summary>
        void ApproveBatch(string comment = null);

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        void ApproveBatch(Batch batch, string comment = null);

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// </summary>
        Task ApproveBatchAsync(string comment = null);

        /// <summary>
        /// Approves the file.
        /// <param name="comment">The approval comment</param>
        /// <param name="batch">The batch instance to use.</param>
        /// </summary>
        Task ApproveBatchAsync(Batch batch, string comment = null);

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
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        void MoveTo(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToBatchAsync(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        void MoveToBatch(string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="destinationUrl">The destination URL including file name.</param>
        /// <param name="moveOperations">combinable flags to indicate the type of move operations.</param>
        /// <param name="options">Options of the move operation.</param>
        Task MoveToBatchAsync(Batch batch, string destinationUrl, MoveOperations moveOperations = MoveOperations.None, MoveCopyOptions options = null);

        /// <summary>
        /// Move a file to the destination URL.
        /// </summary>
        /// <remarks>
        /// If <see cref="MoveOperations.AllowBrokenThickets"/> or <see cref="MoveOperations.BypassSharedLock"/> are used and the move 
        /// is inside the same site then these are respected but the <see cref="MoveCopyOptions"/> are not used. For cross site moves the <see cref="MoveCopyOptions"/> options
        /// are always used, if you want to use for example <see cref="MoveCopyOptions.KeepBoth"/> for a move inside the same site then omit the earlier mentioned <see cref="MoveOperations"/>.
        /// </remarks>
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
        Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch();

        /// <summary>
        /// Send the file to recycle bin.
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync(Batch batch);

        /// <summary>
        /// Send the file to recycle bin
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch(Batch batch);
        #endregion

        #region Syntex

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <returns>Information about the classify and extract request</returns>
        Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractAsync();

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <returns>Information about the classify and extract request</returns>
        ISyntexClassifyAndExtractResult ClassifyAndExtract();

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>Information about the classify and extract request</returns>
        Task<IBatchSingleResult<ISyntexClassifyAndExtractResult>> ClassifyAndExtractBatchAsync(Batch batch);

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <param name="batch">Batch to add this request to</param>
        /// <returns>Information about the classify and extract request</returns>
        IBatchSingleResult<ISyntexClassifyAndExtractResult> ClassifyAndExtractBatch(Batch batch);

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <returns>Information about the classify and extract request</returns>
        Task<IBatchSingleResult<ISyntexClassifyAndExtractResult>> ClassifyAndExtractBatchAsync();

        /// <summary>
        /// Requests Syntex AI models to classify and extract information from this file 
        /// </summary>
        /// <returns>Information about the classify and extract request</returns>
        IBatchSingleResult<ISyntexClassifyAndExtractResult> ClassifyAndExtractBatch();
        #endregion

        #region Thumbnails
        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        Task<List<IThumbnail>> GetThumbnailsAsync(ThumbnailOptions options = null);

        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        List<IThumbnail> GetThumbnails(ThumbnailOptions options = null);

        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        Task<IEnumerableBatchResult<IThumbnail>> GetThumbnailsBatchAsync(ThumbnailOptions options = null);

        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        IEnumerableBatchResult<IThumbnail> GetThumbnailsBatch(ThumbnailOptions options = null);

        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        Task<IEnumerableBatchResult<IThumbnail>> GetThumbnailsBatchAsync(Batch batch, ThumbnailOptions options = null);

        /// <summary>
        /// Returns a list of thumbnails for this file
        /// </summary>
        /// <param name="batch">The batch instance to use.</param>
        /// <param name="options">Optionally specify which thumbnails you need</param>
        /// <returns>The requested thumbnails</returns>
        IEnumerableBatchResult<IThumbnail> GetThumbnailsBatch(Batch batch, ThumbnailOptions options = null);
        #endregion

        #region Analytics

        /// <summary>
        /// Gets file analytics
        /// </summary>
        /// <param name="options">Defines which analytics are needed</param>
        /// <returns>The requested analytics data</returns>
        public Task<List<IActivityStat>> GetAnalyticsAsync(AnalyticsOptions options = null);

        /// <summary>
        /// Gets file analytics
        /// </summary>
        /// <param name="options">Defines which analytics are needed</param>
        /// <returns>The requested analytics data</returns>
        public List<IActivityStat> GetAnalytics(AnalyticsOptions options = null);

        #endregion

        #region Convert

        /// <summary>
        /// Converts the file to PDF, JPG, Html or Glb
        /// </summary>
        /// <param name="options">Defines the file conversion options</param>
        /// <returns>Stream of the converted file</returns>
        Task<Stream> ConvertToAsync(ConvertToOptions options);

        /// <summary>
        /// Converts the file to PDF, JPG, Html or Glb
        /// </summary>
        /// <param name="options">Defines the file conversion options</param>
        /// <returns>Stream of the converted file</returns>
        Stream ConvertTo(ConvertToOptions options);

        #endregion

        #region Preview

        /// <summary>
        /// This action allows you to obtain short-lived embeddable URLs for an item in order to render a temporary preview.
        /// The 'page' and 'zoom' options may not be available for all preview apps, but will be applied if the preview app supports it.
        /// </summary>
        /// <param name="options">Options for configuring the created preview URL</param>
        /// <returns>FilePreview object. Either getUrl, postUrl, or both might be returned depending on the current state of embed support for the specified options.</returns>
        Task<IFilePreview> GetPreviewAsync(PreviewOptions options = null);

        /// <summary>
        /// This action allows you to obtain short-lived embeddable URLs for an item in order to render a temporary preview.
        /// The 'page' and 'zoom' options may not be available for all preview apps, but will be applied if the preview app supports it.
        /// </summary>
        /// <param name="options">Options for configuring the created preview URL</param>
        /// <returns>FilePreview object. Either getUrl, postUrl, or both might be returned depending on the current state of embed support for the specified options.</returns>
        IFilePreview GetPreview(PreviewOptions options = null);

        #endregion

        #region Rename
        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="name">New file name</param>
        /// <returns></returns>
        public Task RenameAsync(string name);

        /// <summary>
        /// Renames a file
        /// </summary>
        /// <param name="name">New file name</param>
        /// <returns></returns>
        public void Rename(string name);
        #endregion
    }
}
