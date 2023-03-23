using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelGet<IListItem>, IDataModelLoad<IListItem>, IDataModelUpdate, IDataModelDelete, IDataModelSupportingGetChanges, ISecurableObject, IExpandoDataModel, IQueryableDataModel
    {
        /// <summary>
        /// Id of the list item
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Title value of the list item
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Determines if comments are disabled for the list item
        /// </summary>
        public bool CommentsDisabled { get; }

        /// <summary>
        /// The scope for which comments are disabled
        /// </summary>
        public CommentsDisabledScope CommentsDisabledScope { get; }

        /// <summary>
        /// The content type for the list item
        /// </summary>
        public IContentType ContentType { get; }

        /// <summary>
        /// All the field values for the list item as HTML
        /// </summary>
        public IFieldStringValues FieldValuesAsHtml { get; }

        /// <summary>
        /// All the field values for the list item as text
        /// </summary>
        public IFieldStringValues FieldValuesAsText { get; }

        /// <summary>
        /// All the field values for the list item for editing
        /// </summary>
        public IFieldStringValues FieldValuesForEdit { get; }

        /// <summary>
        /// The file, if any, associated with the list item
        /// </summary>
        public IFile File { get; }

        /// <summary>
        /// The <seealso cref="SharePoint.FileSystemObjectType"/> for the list item, such as Folder or File
        /// </summary>
        public FileSystemObjectType FileSystemObjectType { get; }

        /// <summary>
        /// The folder, if any, represented by the list item
        /// </summary>
        public IFolder Folder { get; }

        /// <summary>
        /// The list for the list item
        /// </summary>
        public IList ParentList { get; }

        /// <summary>
        /// The properties of the list item
        /// </summary>
        public IPropertyValues Properties { get; }

        /// <summary>
        /// The URI used to render the WOPI (Web Application Open Platform Interface) frame
        /// </summary>
        public string ServerRedirectedEmbedUri { get; }

        /// <summary>
        /// The URL used to render the WOPI (Web Application Open Platform Interface) frame
        /// </summary>
        public string ServerRedirectedEmbedUrl { get; }

        /// <summary>
        /// The unique identifier of the list item
        /// </summary>
        public Guid UniqueId { get; }

        /// <summary>
        /// Information about the likes on this list item
        /// </summary>
        public ILikedByInformation LikedByInformation { get; }

        /// <summary>
        /// Gets a value that returns a collection of list item version objects that represent the versions of the list item
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IListItemVersionCollection Versions { get; }

        /// <summary>
        /// Collection of attachments for this list item
        /// Implements <see cref="IQueryable{T}"/>. <br />
        /// See <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-getdata.html#requesting-model-collections">Requesting model collections</see> 
        /// and <see href="https://pnp.github.io/pnpcore/using-the-sdk/basics-iqueryable.html">IQueryable performance considerations</see> to learn more.
        /// </summary>
        public IAttachmentCollection AttachmentFiles { get; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }

        #region Extension methods

        #region Display Name

        /// <summary>
        /// Gets the display name of the list item.
        /// </summary>
        /// <returns>The display name or <c>null</c>.</returns>
        public Task<string> GetDisplayNameAsync();

        /// <summary>
        /// Gets the display name of the list item.
        /// </summary>
        /// <returns>The display name or <c>null</c>.</returns>
        public string GetDisplayName();

        #endregion

        #region File

        /// <summary>
        /// Checks if this <see cref="IListItem"/> is a file
        /// </summary>
        /// <returns>Returns true if this <see cref="IListItem"/> is a file.</returns>
        public Task<bool> IsFileAsync();

        /// <summary>
        /// Checks if this <see cref="IListItem"/> is a file
        /// </summary>
        /// <returns>Returns true if this <see cref="IListItem"/> is a file.</returns>
        public bool IsFile();

        #endregion

        #region Folder

        /// <summary>
        /// Checks if this <see cref="IListItem"/> is a folder
        /// </summary>
        /// <returns>Returns true if this <see cref="IListItem"/> is a folder.</returns>
        public Task<bool> IsFolderAsync();

        /// <summary>
        /// Checks if this <see cref="IListItem"/> is a folder
        /// </summary>
        /// <returns>Returns true if this <see cref="IListItem"/> is a folder.</returns>
        public bool IsFolder();

        /// <summary>
        /// Returns the <see cref="IFolder"/> that holds this item
        /// </summary>
        /// <returns>The <see cref="IFolder"/> for this item is returned, if the item itself is a folder then the item is returned as <see cref="IFolder"/>.</returns>
        public Task<IFolder> GetParentFolderAsync();

        /// <summary>
        /// Returns the <see cref="IFolder"/> that holds this item
        /// </summary>
        /// <returns>The <see cref="IFolder"/> for this item is returned, if the item itself is a folder then the item is returned as <see cref="IFolder"/>.</returns>
        public IFolder GetParentFolder();

        #endregion

        #region MoveTo

        /// <summary>
        /// Moves ListItem to the destination folder URL.
        /// </summary>
        /// <param name="destinationFolderUrl">folder path within the list, e.g. 'subfolder1/subfolder2'</param>
        Task MoveToAsync(string destinationFolderUrl);

        /// <summary>
        /// Moves ListItem to the destination folder Folder.
        /// </summary>
        /// <param name="destinationFolderUrl">folder path within the list, e.g. 'subfolder1/subfolder2'</param>
        void MoveTo(string destinationFolderUrl);

        #endregion

        #region SystemUpdate

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public Task SystemUpdateAsync();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public void SystemUpdate();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public Task SystemUpdateBatchAsync();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        public void SystemUpdateBatch();

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        /// <param name="batch">Batch to add the systemupdate request to</param>
        public Task SystemUpdateBatchAsync(Batch batch);

        /// <summary>
        /// Performs a system update of the list item
        /// </summary>
        /// <param name="batch">Batch to add the systemupdate request to</param>
        public void SystemUpdateBatch(Batch batch);

        #endregion

        #region UpdateOverwriteVersion

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public void UpdateOverwriteVersion();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public Task UpdateOverwriteVersionBatchAsync();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        public void UpdateOverwriteVersionBatch();

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        /// <param name="batch">Batch to add the UpdateOverwriteVersion request to</param>
        public Task UpdateOverwriteVersionBatchAsync(Batch batch);

        /// <summary>
        /// Performs a UpdateOverwriteVersion of the list item
        /// </summary>
        /// <param name="batch">Batch to add the UpdateOverwriteVersion request to</param>
        public void UpdateOverwriteVersionBatch(Batch batch);

        #endregion

        #region Comments handling
        /// <summary>
        /// Are comments disabled for this <see cref="IListItem"/>?
        /// </summary>
        /// <returns>True if disabled, false otherwise</returns>
        public Task<bool> AreCommentsDisabledAsync();

        /// <summary>
        /// Are comments disabled for this <see cref="IListItem"/>?
        /// </summary>
        /// <returns>True if disabled, false otherwise</returns>
        public bool AreCommentsDisabled();

        /// <summary>
        /// Enable/Disable comments for this list item
        /// </summary>
        /// <param name="commentsDisabled">Do comments need to enabled or disabled</param>
        /// <returns></returns>
        public Task SetCommentsDisabledAsync(bool commentsDisabled);

        /// <summary>
        /// Enable/Disable comments for this list item
        /// </summary>
        /// <param name="commentsDisabled">Do comments need to enabled or disabled</param>
        /// <returns></returns>
        public void SetCommentsDisabled(bool commentsDisabled);

        #endregion

        #region ComplianceTag / Label handling

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public Task SetComplianceTagAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public void SetComplianceTagBatch(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public Task SetComplianceTagBatchAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public void SetComplianceTagBatch(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        /// <summary>
        /// Sets a compliancetag / retention label for this list item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="complianceTag">The tag/label to set for this list item</param>
        /// <param name="isTagPolicyHold">Whether the tag is hold</param>
        /// <param name="isTagPolicyRecord">Whether the tag is record</param>
        /// <param name="isEventBasedTag">Whether the tag is Event based, this is not used</param>
        /// <param name="isTagSuperLock">Whether the tag is Sec 17 tag,no allow change even for site admin</param>
        /// <returns></returns>
        public Task SetComplianceTagBatchAsync(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock);

        #endregion

        #region New field value

        /// <summary>
        /// Creates a new <see cref="IFieldUrlValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="url">Url value</param>
        /// <param name="description">Optional description value</param>
        /// <returns>Configured <see cref="IFieldUrlValue"/> object</returns>
        public IFieldUrlValue NewFieldUrlValue(IField fieldToUpdate, string url, string description = null);

        /// <summary>
        /// Creates a new <see cref="IFieldLookupValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="lookupId">Id of the lookup value</param>
        /// <returns>Configured <see cref="IFieldLookupValue"/> object</returns>
        public IFieldLookupValue NewFieldLookupValue(IField fieldToUpdate, int lookupId);

        /// <summary>
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="userId">Id of the user</param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, int userId);

        /// <summary>
        /// Creates a new <see cref="IFieldUserValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="principal"><see cref="ISharePointUser"/> or <see cref="ISharePointGroup"/></param>
        /// <returns>Configured <see cref="IFieldUserValue"/> object</returns>
        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, ISharePointPrincipal principal);

        /// <summary>
        /// Creates a new <see cref="IFieldTaxonomyValue"/> object
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <param name="termId">Name of the term to set</param>
        /// <param name="label">Label of the term to set</param>
        /// <param name="wssId">Optionally provide the wssId value</param>
        /// <returns>Configured <see cref="IFieldTaxonomyValue"/> object</returns>
        public IFieldTaxonomyValue NewFieldTaxonomyValue(IField fieldToUpdate, Guid termId, string label, int wssId = -1);

        /// <summary>
        /// Creates a new collection to hold <see cref="IFieldValue"/> objects
        /// </summary>
        /// <param name="fieldToUpdate"><see cref="IField"/> representing the field to set</param>
        /// <returns></returns>
        public IFieldValueCollection NewFieldValueCollection(IField fieldToUpdate);

        #endregion

        #region Recycle

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <returns></returns>
        public Guid Recycle();

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <returns></returns>
        public Task<Guid> RecycleAsync();

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <returns></returns>
        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch();

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <returns></returns>
        public Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync();

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns></returns>
        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch(Batch batch);

        /// <summary>
        /// Recycle the current item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <returns></returns>
        public Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync(Batch batch);
        #endregion

        #region Comments and liking
        /// <summary>
        /// Get list item comments
        /// </summary>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        public Task<ICommentCollection> GetCommentsAsync(params Expression<Func<IComment, object>>[] selectors);

        /// <summary>
        /// Get list item comments
        /// </summary>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        public ICommentCollection GetComments(params Expression<Func<IComment, object>>[] selectors);
        #endregion

        #region Graph permissions

        ///// <summary>
        ///// Gets the share links on the list item
        ///// </summary>
        ///// <returns>Collection of share links existing on the list item</returns>
        //Task<IGraphPermissionCollection> GetShareLinksAsync();

        ///// <summary>
        ///// Gets the share links on the list item
        ///// </summary>
        ///// <returns>Collection of share links existing on the list item</returns>
        //IGraphPermissionCollection GetShareLinks();

        ///// <summary>
        ///// Deletes the share links on the list item
        ///// </summary>
        //Task DeleteShareLinksAsync();

        ///// <summary>
        ///// Deletes the share links on the list item
        ///// </summary>
        //void DeleteShareLinks();

        /// <summary>
        /// Creates an anonymous sharing link for a list item
        /// </summary>
        /// <param name="anonymousLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateAnonymousSharingLinkAsync(AnonymousLinkOptions anonymousLinkOptions);

        /// <summary>
        /// Creates an anonymous sharing link for a list item
        /// </summary>
        /// <param name="anonymousLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateAnonymousSharingLink(AnonymousLinkOptions anonymousLinkOptions);

        /// <summary>
        /// Creates an organization sharing link for a list item
        /// </summary>
        /// <param name="organizationalLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateOrganizationalSharingLinkAsync(OrganizationalLinkOptions organizationalLinkOptions);

        /// <summary>
        /// Creates an organization sharing link for a list item
        /// </summary>
        /// <param name="organizationalLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateOrganizationalSharingLink(OrganizationalLinkOptions organizationalLinkOptions);

        /// <summary>
        /// Creates a user sharing link for a list item
        /// </summary>
        /// <param name="userLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        Task<IGraphPermission> CreateUserSharingLinkAsync(UserLinkOptions userLinkOptions);

        /// <summary>
        /// Creates a user sharing link for a list item
        /// </summary>
        /// <param name="userLinkOptions"></param>
        /// <returns>Permission that has been created</returns>
        IGraphPermission CreateUserSharingLink(UserLinkOptions userLinkOptions);

        #endregion

        #region Effective user permissions

        /// <summary>
        /// Gets the user effective permissions of a user for a listitem
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        IBasePermissions GetUserEffectivePermissions(string userPrincipalName);

        /// <summary>
        /// Gets the user effective permissions of a user for a listitem
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to retrieve the permissions of</param>
        /// <returns>Base permissions object that contains the High and the Low permissions</returns>
        Task<IBasePermissions> GetUserEffectivePermissionsAsync(string userPrincipalName);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a listitem
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        bool CheckIfUserHasPermissions(string userPrincipalName, PermissionKind permissionKind);

        /// <summary>
        /// Checks if a user has a specific kind of permissions to a listitem
        /// </summary>
        /// <param name="userPrincipalName">Login name of the user you wish to check if he has a specific permission</param>
        /// <param name="permissionKind">Permission kind to check</param>
        /// <returns>Boolean that says if the user has permissions or not</returns>
        Task<bool> CheckIfUserHasPermissionsAsync(string userPrincipalName, PermissionKind permissionKind);


        #endregion

        #endregion

    }
}
