﻿using PnP.Core.Model.Security;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePoint Online list item
    /// </summary>
    [ConcreteType(typeof(ListItem))]
    public interface IListItem : IDataModel<IListItem>, IDataModelGet<IListItem>, IDataModelLoad<IListItem>, IDataModelUpdate, IDataModelDelete, IExpandoDataModel, IQueryableDataModel
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
        /// Role assignments of the list item
        /// </summary>
        public IRoleAssignmentCollection RoleAssignments { get; }

        #region Extension methods

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
        public Task<IFolder> GetFolderAsync();

        /// <summary>
        /// Returns the <see cref="IFolder"/> that holds this item
        /// </summary>
        /// <returns>The <see cref="IFolder"/> for this item is returned, if the item itself is a folder then the item is returned as <see cref="IFolder"/>.</returns>
        public IFolder GetFolder();
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

        #region Permissions

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public void BreakRoleInheritanceBatch(Batch batch, bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public Task BreakRoleInheritanceBatchAsync(Batch batch, bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public void BreakRoleInheritanceBatch(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Break role inheritance on the current item
        /// </summary>
        /// <param name="copyRoleAssignments"></param>
        /// <param name="clearSubscopes"></param>
        /// <returns></returns>
        public Task BreakRoleInheritanceBatchAsync(bool copyRoleAssignments, bool clearSubscopes);

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <returns></returns>
        public void ResetRoleInheritance();

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <returns></returns>
        public Task ResetRoleInheritanceAsync();

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <returns></returns>
        public void ResetRoleInheritanceBatch(Batch batch);

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <returns></returns>
        public Task ResetRoleInheritanceBatchAsync(Batch batch);

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <returns></returns>
        public void ResetRoleInheritanceBatch();

        /// <summary>
        /// Reset role inheritance on the current item
        /// </summary>
        /// <returns></returns>
        public Task ResetRoleInheritanceBatchAsync();

        /// <summary>
        /// Returns the role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        public IRoleDefinitionCollection GetRoleDefinitions(int principalId);

        /// <summary>
        /// Returns the role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <returns></returns>
        public Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync(int principalId);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool AddRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public Task<bool> AddRoleDefinitionsAsync(int principalId, params string[] names);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinition(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Add role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public void AddRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Adds role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to add</param>
        /// <returns></returns>
        public Task AddRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool RemoveRoleDefinitions(int principalId, params string[] names);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinition(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public void RemoveRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition);

        /// <summary>
        /// Removes role definitions for a specific principal id (IUser.Id or ISharePointGroup.Id)
        /// </summary>
        /// <param name="batch">The batch to add this request to</param>
        /// <param name="principalId"></param>
        /// <param name="roleDefinition">Role definition to remove</param>
        /// <returns></returns>
        public Task RemoveRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition);
        #endregion

        #endregion

    }
}
