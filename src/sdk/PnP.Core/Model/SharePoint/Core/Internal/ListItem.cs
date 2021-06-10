using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests.ListItems;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItem", Target = typeof(List), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})", LinqGet = "_api/web/lists(guid'{Parent.Id}')/items")]
    [SharePointType("SP.ListItem", Target = typeof(File), Uri = "_api/web/getFileById('{Parent.Id}')/listitemallfields")]
    [SharePointType("SP.ListItem", Target = typeof(Folder), Uri = "_api/Web/getFolderById('{Parent.Id}')/listitemallfields")]
    //[GraphType(OverflowProperty = "fields")]
    internal partial class ListItem : ExpandoBaseDataModel<IListItem>, IListItem
    {
        internal const string FolderPath = "folderPath";
        internal const string UnderlyingObjectType = "underlyingObjectType";

        #region Construction
        public ListItem()
        {
            MappingHandler = (FromJson input) =>
            {
                // The AddValidateUpdateItemUsingPath call returns the id of the added list item
                if (input.FieldName == "AddValidateUpdateItemUsingPath")
                {
                    if (input.JsonElement.TryGetProperty("results", out JsonElement resultsProperty))
                    {
                        foreach (var field in resultsProperty.EnumerateArray())
                        {
                            var fieldName = field.GetProperty("FieldName").GetString();
                            var fieldValue = field.GetProperty("FieldValue").GetString();

                            if (fieldName == "Id")
                            {
                                if (int.TryParse(fieldValue, out int id))
                                {
                                    Id = id;

                                    // Flag the parent collection as requested, most requests return a "full" json structure and then the standard json parsing 
                                    // is used, which sets the collection as requested. Since in this case we get back a special structure we use custom
                                    // parsing and hence we need to flag the collection as requested ourselves
                                    (Parent as ListItemCollection).Requested = true;

                                    // populate the uri and type metadata fields to enable actions upon 
                                    // this item without having to read it again from the server
                                    var parentList = Parent.Parent as List;
                                    AddMetadata(PnPConstants.MetaDataRestId, $"{id}");
                                    MetadataSetup();

                                    // Ensure the values are committed to the model when an item is being added
                                    Values.Commit();

                                    // We're currently only interested in the Id property
                                    continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Handle the mapping from json to the domain model for the cases which are not generically handled           
                    input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);
                }

                return null;
            };

            PostMappingHandler = (string json) =>
            {
                // Extra processing of returned json
            };
            AddApiCallHandler = async (keyValuePairs) =>
            {
                var parentList = Parent.Parent as List;
                // sample parent list uri: https://bertonline.sharepoint.com/sites/modern/_api/Web/Lists(guid'b2d52a36-52f1-48a4-b499-629063c6a38c')
                var parentListUri = parentList.GetMetadata(PnPConstants.MetaDataUri);

                // sample parent list entity type name: DemolistList (skip last 4 chars)
                // Sample parent library type name: MyDocs
                var parentListTitle = !string.IsNullOrEmpty(parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName)) ? parentList.GetMetadata(PnPConstants.MetaDataRestEntityTypeName) : null;

                // If this list we're adding items to was not fetched from the server than throw an error
                string serverRelativeUrl = null;
                if (string.IsNullOrEmpty(parentListTitle) || string.IsNullOrEmpty(parentListUri) || !parentList.IsPropertyAvailable(p => p.TemplateType))
                {
                    // Fall back to loading the RootFolder property if we can't determine the list name
                    await parentList.EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
                    serverRelativeUrl = parentList.RootFolder.ServerRelativeUrl;
                }
                else
                {
                    serverRelativeUrl = ListMetaDataMapper.RestEntityTypeNameToUrl(PnPContext.Uri, parentListTitle, parentList.TemplateType);
                }

                // drop the everything in front of _api as the batching logic will add that automatically
                var baseApiCall = parentListUri.Substring(parentListUri.IndexOf("_api"));

                // Define the JSON body of the update request based on the actual changes
                dynamic body = new ExpandoObject();

                var decodedUrlFolderPath = serverRelativeUrl;
                if (keyValuePairs.ContainsKey(FolderPath))
                {
                    if (keyValuePairs[FolderPath] != null)
                    {
                        var folderPath = keyValuePairs[FolderPath].ToString();
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            if (folderPath.ToLower().StartsWith(serverRelativeUrl))
                            {
                                decodedUrlFolderPath = folderPath;
                            }
                            else
                            {
                                decodedUrlFolderPath = $"{serverRelativeUrl}/{folderPath.TrimStart('/')}";
                            }
                        }
                    }
                }

                var underlyingObjectType = (int)FileSystemObjectType.File;

                if (keyValuePairs.ContainsKey(UnderlyingObjectType))
                {
                    if (keyValuePairs[UnderlyingObjectType] != null)
                    {
                        underlyingObjectType = (int)((FileSystemObjectType)keyValuePairs[UnderlyingObjectType]);
                    }
                }

                body.listItemCreateInfo = new
                {
                    FolderPath = new
                    {
                        DecodedUrl = decodedUrlFolderPath
                    },
                    UnderlyingObjectType = underlyingObjectType
                };

                body.bNewDocumentUpdate = false;

                if (Values.Any())
                {
                    // Verify the needed locale settings are loaded
                    await EnsureRegionalSettingsAsync(PnPContext).ConfigureAwait(false);
                }

                // Add fields to the payload
                dynamic itemValues = new List<dynamic>();
                foreach (var item in Values)
                {
                    dynamic field = new ExpandoObject();
                    field.FieldName = item.Key;

                    BuildValidateUpdateItemPayload(PnPContext, item, field);

                    itemValues.Add(field);
                }
                body.formValues = itemValues;

                // Serialize object to json
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), new JsonSerializerOptions { WriteIndented = true });

                // Return created api call
                return new ApiCall($"{baseApiCall}/AddValidateUpdateItemUsingPath", ApiType.SPORest, bodyContent);
            };
        }
        #endregion

        #region Properties
        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string Title { get => (string)Values["Title"]; set => Values["Title"] = value; }

        public bool CommentsDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public CommentsDisabledScope CommentsDisabledScope { get => GetValue<CommentsDisabledScope>(); set => SetValue(value); }

        public IContentType ContentType { get => GetModelValue<IContentType>(); }

        public IFieldStringValues FieldValuesAsHtml { get => GetModelValue<IFieldStringValues>(); }

        public IFieldStringValues FieldValuesAsText { get => GetModelValue<IFieldStringValues>(); }

        public IFieldStringValues FieldValuesForEdit { get => GetModelValue<IFieldStringValues>(); }

        public IFile File { get => GetModelValue<IFile>(); }

        public FileSystemObjectType FileSystemObjectType
        {
            get => HasValue("FSObjType") ? GetValue<FileSystemObjectType>("FSObjType") : GetValue<FileSystemObjectType>();
            set => SetValue(value);
        }

        public IFolder Folder { get => GetModelValue<IFolder>(); }

        public IList ParentList { get => GetModelValue<IList>(); }

        public IPropertyValues Properties { get => GetModelValue<IPropertyValues>(); }

        public string ServerRedirectedEmbedUri { get => GetValue<string>(); set => SetValue(value); }

        public string ServerRedirectedEmbedUrl { get => GetValue<string>(); set => SetValue(value); }

        public Guid UniqueId { get => GetValue<Guid>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }

        // Not in public interface as Comments is not an expandable property in REST
        public ICommentCollection Comments { get => GetModelCollectionValue<ICommentCollection>(); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Methods

        #region Common

        private string GetItemUri()
        {
            if (Parent is Folder)
            {
                return "_api/Web/getFolderById('{Parent.Id}')/listitemallfields";
            }
            else if (Parent is File)
            {
                return "_api/web/getFileById('{Parent.Id}')/listitemallfields";
            }

            return "_api/Web/lists/getbyid(guid'{Parent.Id}')/items({Id})";
        }

        #endregion

        #region Display Name

        public async Task<string> GetDisplayNameAsync()
        {
            var apiCall = new ApiCall($"{GetItemUri()}/DisplayName", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonDocument.Parse(response.Json).RootElement.GetProperty("d");

                if (json.TryGetProperty("DisplayName", out JsonElement displayName))
                {
                    return displayName.GetString();
                }
            }

            return null;
        }

        public string GetDisplayName()
        {
            return GetDisplayNameAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region File
        public async Task<bool> IsFileAsync()
        {
            if (!Values.ContainsKey("ContentTypeId"))
            {
                await LoadKeyListItemProperties().ConfigureAwait(false);
            }

            return Values["ContentTypeId"].ToString().StartsWith("0x0101", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsFile()
        {
            return IsFileAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Folder
        public async Task<bool> IsFolderAsync()
        {
            if (!Values.ContainsKey("ContentTypeId"))
            {
                await LoadKeyListItemProperties().ConfigureAwait(false);
            }

            return Values["ContentTypeId"].ToString().StartsWith("0x0120", StringComparison.InvariantCultureIgnoreCase);
        }

        public bool IsFolder()
        {
            return IsFolderAsync().GetAwaiter().GetResult();
        }

        public async Task<IFolder> GetParentFolderAsync()
        {
            if (!Values.ContainsKey("FileDirRef"))
            {
                await LoadKeyListItemProperties().ConfigureAwait(false);
            }

            return await PnPContext.Web.GetFolderByServerRelativeUrlAsync(Values["FileDirRef"].ToString()).ConfigureAwait(false);
        }

        public IFolder GetParentFolder()
        {
            return GetParentFolderAsync().GetAwaiter().GetResult();
        }

        private async Task LoadKeyListItemProperties()
        {
            ApiCall apiCall = new ApiCall($"{GetItemUri()}?$select=ContentTypeId,FileDirRef", ApiType.SPORest);
            await RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);
        }
        #endregion

        #region Item updates

        internal override async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch and execute the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null, "Update");
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }


        internal override async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            var api = await BuildUpdateApiCallAsync(PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            batch.Add(this, entityInfo, HttpMethod.Post, api, default, null, null, "UpdateBatch");
        }

        private async Task<ApiCall> BuildUpdateApiCallAsync(PnPContext context)
        {
            // Verify the needed locale settings are loaded
            await EnsureRegionalSettingsAsync(context).ConfigureAwait(false);

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            // increment version
            updateMessage.bNewDocumentUpdate = false;

            dynamic itemValues = new List<dynamic>();

            foreach (KeyValuePair<string, object> changedProp in Values.ChangedProperties)
            {
                dynamic field = new ExpandoObject();
                field.FieldName = changedProp.Key;

                BuildValidateUpdateItemPayload(context, changedProp, field);

                itemValues.Add(field);
            }

            updateMessage.formValues = itemValues;

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                new JsonSerializerOptions { WriteIndented = true });

            var itemUri = GetMetadata(PnPConstants.MetaDataUri);

            // If this list we're adding items to was not fetched from the server than throw an error
            if (string.IsNullOrEmpty(itemUri))
            {
                throw new ClientException(ErrorType.PropertyNotLoaded, PnPCoreResources.Exception_PropertyNotLoaded_List);
            }

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(this, $"{itemUri}/ValidateUpdateListItem").ConfigureAwait(false);
            var api = new ApiCall(updateUrl, ApiType.SPORest, jsonUpdateMessage)
            {
                Commit = true
            };
            return api;
        }

        private static async Task EnsureRegionalSettingsAsync(PnPContext context)
        {
            bool loadRegionalSettings = false;
            if (context.Web.IsPropertyAvailable(p => p.RegionalSettings))
            {
                if (!context.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeZone) ||
                    !context.Web.RegionalSettings.IsPropertyAvailable(p => p.DecimalSeparator) ||
                    !context.Web.RegionalSettings.IsPropertyAvailable(p => p.DateSeparator))
                {
                    loadRegionalSettings = true;
                }
            }
            else
            {
                loadRegionalSettings = true;
            }

            if (loadRegionalSettings)
            {
                await context.Web.RegionalSettings.EnsurePropertiesAsync(RegionalSettings.LocaleSettingsExpression).ConfigureAwait(false);
            }
        }

        private static void BuildValidateUpdateItemPayload(PnPContext context, KeyValuePair<string, object> changedProp, dynamic field)
        {
            // Only include FieldValue properties when they signal they've changed
            if (changedProp.Value is FieldValue fieldItemValue && fieldItemValue.HasChanges)
            {
                field.FieldValue = fieldItemValue.ToValidateUpdateItemJson();
            }
            else if (changedProp.Value is FieldValueCollection fieldValueCollection)
            {
                // Only process if there were changes in the field value collection
                if (fieldValueCollection.HasChanges)
                {
                    if (fieldValueCollection.Field.TypeAsString == "UserMulti")
                    {
                        field.FieldValue = fieldValueCollection.UserMultiToValidateUpdateItemJson();
                    }
                    else if (fieldValueCollection.Field.TypeAsString == "TaxonomyFieldTypeMulti")
                    {
                        field.FieldValue = fieldValueCollection.TaxonomyMultiToValidateUpdateItemJson();
                    }
                    else if (fieldValueCollection.Field.TypeAsString == "LookupMulti")
                    {
                        field.FieldValue = fieldValueCollection.LookupMultiToValidateUpdateItemJson();
                    }
                }
                else
                {
                    field.FieldValue = null;
                }
            }
            else if (changedProp.Value is List<string> stringList)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var choice in stringList)
                {
                    sb.Append($"{choice};#");
                }
                field.FieldValue = sb.ToString();
            }
            else if (changedProp.Value is DateTime dateValue)
            {
                field.FieldValue = DateTimeToSharePointString(context, dateValue);
            }
            else if (changedProp.Value != null && (changedProp.Value is int))
            {
                field.FieldValue = changedProp.Value.ToString();
            }
            else if (changedProp.Value != null && (changedProp.Value is double doubleValue))
            {
                field.FieldValue = DoubleToSharePointString(context, doubleValue);
            }
            else if (changedProp.Value != null && (changedProp.Value is bool boolValue))
            {
                field.FieldValue = $"{(boolValue ? "1" : "0")}";
            }
            else
            {
                field.FieldValue = changedProp.Value;
            }
        }

        private static string DateTimeToSharePointString(PnPContext context, DateTime input)
        {
            // Convert incoming date to UTC
            DateTime inputInUTC = input.ToUniversalTime();

            // Convert to the time zone used by the SharePoint site, take in account the daylight savings delta
            bool isDaylight = TimeZoneInfo.Local.IsDaylightSavingTime(input);
            TimeSpan utcDelta = new TimeSpan(0, context.Web.RegionalSettings.TimeZone.Bias + (isDaylight ? context.Web.RegionalSettings.TimeZone.DaylightBias : context.Web.RegionalSettings.TimeZone.StandardBias), 0);

            // Apply the delta from UTC to get the date used by the site and apply formatting 
            return (inputInUTC - utcDelta).ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private static string DoubleToSharePointString(PnPContext context, double input)
        {
            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = context.Web.RegionalSettings.DecimalSeparator
            };

            return input.ToString(nfi);
        }

        #endregion

        #region UpdateOverwriteVersion

        public async Task UpdateOverwriteVersionAsync()
        {
            UpdateListItemRequest request = new UpdateOverwriteVersionRequest(PnPContext.Site.Id.ToString(), PnPContext.Web.Id.ToString(), "", Id);

            await PrepareUpdateCall(request).ConfigureAwait(false);

            if (request.FieldsToUpdate.Count > 0)
            {
                ApiCall updateCall = new ApiCall(new List<Services.Core.CSOM.Requests.IRequest<object>>() { request })
                {
                    Commit = true,
                };
                await RawRequestAsync(updateCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        private List<CSOMItemField> GetFieldsToUpdate()
        {
            CSOMFieldHelper helper = new CSOMFieldHelper(this);

            return helper.GetUpdatedFieldValues(ChangedProperties);
        }

        public void UpdateOverwriteVersion()
        {
            UpdateOverwriteVersionAsync().GetAwaiter().GetResult();
        }

        public async Task UpdateOverwriteVersionBatchAsync()
        {
            await UpdateOverwriteVersionBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void UpdateOverwriteVersionBatch()
        {
            UpdateOverwriteVersionBatchAsync().GetAwaiter().GetResult();
        }

        protected async Task PrepareUpdateCall(UpdateListItemRequest request)
        {
            string listId = "";
            if ((this as IDataModelParent).Parent is IFile file)
            {
                // When it's a file then we need to resolve the {Parent.Id} token manually as otherwise this 
                // will point to the File id while we need to list Id here
                await file.EnsurePropertiesAsync(p => p.ListId).ConfigureAwait(false);
                listId = file.ListId.ToString();
            }

            if ((this as IDataModelParent).Parent.Parent is IList)
            {
                listId = ((this as IDataModelParent).Parent.Parent as IList).Id.ToString();
            }

            request.ListId = listId;
            List<CSOMItemField> fieldsToUpdate = GetFieldsToUpdate();
            request.FieldsToUpdate.AddRange(fieldsToUpdate);
        }

        public async Task UpdateOverwriteVersionBatchAsync(Batch batch)
        {
            UpdateListItemRequest request = new UpdateOverwriteVersionRequest(PnPContext.Site.Id.ToString(), PnPContext.Web.Id.ToString(), "", Id);
            await PrepareUpdateCall(request).ConfigureAwait(false);
            if (request.FieldsToUpdate.Count > 0)
            {
                ApiCall updateCall = new ApiCall(new List<Services.Core.CSOM.Requests.IRequest<object>>() { request })
                {
                    Commit = true,
                };

                await RawRequestBatchAsync(batch, updateCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void UpdateOverwriteVersionBatch(Batch batch)
        {
            UpdateOverwriteVersionBatchAsync(batch).GetAwaiter().GetResult();
        }

        #endregion

        #region SystemUpdate

        public async Task SystemUpdateAsync()
        {
            UpdateListItemRequest request = new SystemUpdateListItemRequest(PnPContext.Site.Id.ToString(), PnPContext.Web.Id.ToString(), "", Id);
            await PrepareUpdateCall(request).ConfigureAwait(false);
            if (request.FieldsToUpdate.Count > 0)
            {
                ApiCall updateCall = new ApiCall(new List<Services.Core.CSOM.Requests.IRequest<object>>() { request })
                {
                    Commit = true,
                };

                await RawRequestAsync(updateCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdate()
        {
            SystemUpdateAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync()
        {
            await SystemUpdateBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void SystemUpdateBatch()
        {
            SystemUpdateBatchAsync().GetAwaiter().GetResult();
        }

        public async Task SystemUpdateBatchAsync(Batch batch)
        {
            UpdateListItemRequest request = new SystemUpdateListItemRequest(PnPContext.Site.Id.ToString(), PnPContext.Web.Id.ToString(), "", Id);
            await PrepareUpdateCall(request).ConfigureAwait(false);
            if (request.FieldsToUpdate.Count > 0)
            {
                ApiCall updateCall = new ApiCall(new List<Services.Core.CSOM.Requests.IRequest<object>>() { request })
                {
                    Commit = true,
                };

                await RawRequestBatchAsync(batch, updateCall, HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                PnPContext.Logger.LogInformation(PnPCoreResources.Log_Information_NoChangesSkipSystemUpdate);
            }
        }

        public void SystemUpdateBatch(Batch batch)
        {
            SystemUpdateBatchAsync(batch).GetAwaiter().GetResult();
        }
        #endregion

        #region Comment handling

        public bool AreCommentsDisabled()
        {
            return AreCommentsDisabledAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> AreCommentsDisabledAsync()
        {
            var apiCall = new ApiCall($"{GetItemUri()}/CommentsDisabled", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonDocument.Parse(response.Json).RootElement.GetProperty("d");

#pragma warning disable CA1507 // Use nameof to express symbol names
                if (json.TryGetProperty("CommentsDisabled", out JsonElement commentsDisabled))
#pragma warning restore CA1507 // Use nameof to express symbol names
                {
                    return commentsDisabled.GetBoolean();
                }
            }

            return false;
        }

        public void SetCommentsDisabled(bool commentsDisabled)
        {
            SetCommentsDisabledAsync(commentsDisabled).GetAwaiter().GetResult();
        }

        public async Task SetCommentsDisabledAsync(bool commentsDisabled)
        {
            // Build the API call to ensure this graph user as a SharePoint User in this site collection
            var parameters = new
            {
                value = commentsDisabled
            };

            string body = JsonSerializer.Serialize(parameters);

            var apiCall = new ApiCall($"{GetItemUri()}/SetCommentsDisabled", ApiType.SPORest, body);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        #endregion

        #region Compliance Tag handling

        public void SetComplianceTag(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagAsync(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            ApiCall apiCall = SetComplianceTagApiCall(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagBatchAsync(PnPContext.CurrentBatch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            await SetComplianceTagBatchAsync(PnPContext.CurrentBatch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            SetComplianceTagBatchAsync(batch, complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(Batch batch, string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            ApiCall apiCall = SetComplianceTagApiCall(complianceTag, isTagPolicyHold, isTagPolicyRecord, isEventBasedTag, isTagSuperLock);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall SetComplianceTagApiCall(string complianceTag, bool isTagPolicyHold, bool isTagPolicyRecord, bool isEventBasedTag, bool isTagSuperLock)
        {
            var parameters = new
            {
                complianceTag,
                isTagPolicyHold,
                isTagPolicyRecord,
                isEventBasedTag,
                isTagSuperLock
            };
            string body = JsonSerializer.Serialize(parameters);
            var apiCall = new ApiCall($"{GetItemUri()}/SetComplianceTag", ApiType.SPORest, body);
            return apiCall;
        }
        #endregion

        #region Recycle
        public Guid Recycle()
        {
            return RecycleAsync().GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleAsync()
        {
            ApiCall apiCall = BuildRecycleApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                return ProcessRecyleResponse(response.Json);
            }

            return Guid.Empty;
        }

        private static Guid ProcessRecyleResponse(string json)
        {
            var document = JsonSerializer.Deserialize<JsonElement>(json);
            if (document.TryGetProperty("d", out JsonElement root))
            {
                if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
                {
                    // return the recyclebin item id
                    return recycleBinItemId.GetGuid();
                }
            }

            return Guid.Empty;
        }

        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch()
        {
            return RecycleBatchAsync().GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync()
        {
            return await RecycleBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IBatchSingleResult<BatchResultValue<Guid>> RecycleBatch(Batch batch)
        {
            return RecycleBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<BatchResultValue<Guid>>> RecycleBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildRecycleApiCall();
            apiCall.RawSingleResult = new BatchResultValue<Guid>(Guid.Empty);
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                (apiCall.RawSingleResult as BatchResultValue<Guid>).Value = ProcessRecyleResponse(json);
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<BatchResultValue<Guid>>(batch, batchRequest.Id, apiCall.RawSingleResult as BatchResultValue<Guid>);
        }

        private ApiCall BuildRecycleApiCall()
        {
            return new ApiCall($"{GetItemUri()}/recycle", ApiType.SPORest)
            {
                RemoveFromModel = true
            };
        }
        #endregion

        #region Graph/Rest interoperability overrides
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async override Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (IsPropertyAvailable(p => p.Id) && Id > 0)
            {
                if (!Metadata.ContainsKey(PnPConstants.MetaDataRestId))
                {
                    Metadata.Add(PnPConstants.MetaDataRestId, Id.ToString());
                }

                if (!Metadata.ContainsKey(PnPConstants.MetaDataGraphId))
                {
                    Metadata.Add(PnPConstants.MetaDataGraphId, Id.ToString());
                }
            }

            MetadataSetup();
        }

        internal void MetadataSetup()
        {
            if (Parent != null && Parent.Parent != null && Parent.Parent is IList)
            {
                if (!Metadata.ContainsKey(PnPConstants.MetaDataUri))
                {
                    AddMetadata(PnPConstants.MetaDataUri, $"{(Parent.Parent as List).GetMetadata(PnPConstants.MetaDataUri)}/Items({Id})");
                }
                if (!Metadata.ContainsKey(PnPConstants.MetaDataType))
                {
                    if ((Parent.Parent as List).IsPropertyAvailable(p => p.ListItemEntityTypeFullName))
                    {
                        AddMetadata(PnPConstants.MetaDataType, (Parent.Parent as List).ListItemEntityTypeFullName);
                    }
                    else if (!string.IsNullOrEmpty((Parent.Parent as List).GetMetadata(PnPConstants.MetaDataRestEntityTypeName)))
                    {
                        AddMetadata(PnPConstants.MetaDataType, $"SP.Data.{(Parent.Parent as List).GetMetadata(PnPConstants.MetaDataRestEntityTypeName)}Item");
                    }
                    else
                    {
                        AddMetadata(PnPConstants.MetaDataType, $"SP.Data.ListItem");
                    }
                }
            }
        }
        #endregion

        #region Special field handling
        public IFieldUrlValue NewFieldUrlValue(IField fieldToUpdate, string url, string description = null)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            return new FieldUrlValue()
            {
                Url = url,
                Description = description ?? url,
                Field = fieldToUpdate
            };
        }

        public IFieldLookupValue NewFieldLookupValue(IField fieldToUpdate, int lookupId)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (lookupId < -1)
            {
                throw new ArgumentNullException(nameof(lookupId));
            }

            return new FieldLookupValue()
            {
                LookupId = lookupId,
                Field = fieldToUpdate
            };
        }

        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, int userId)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (userId < -1)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return new FieldUserValue()
            {
                LookupId = userId,
                Field = fieldToUpdate
            };
        }

        public IFieldUserValue NewFieldUserValue(IField fieldToUpdate, ISharePointPrincipal principal)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return new FieldUserValue()
            {
                Principal = principal,
                Field = fieldToUpdate
            };
        }

        public IFieldTaxonomyValue NewFieldTaxonomyValue(IField fieldToUpdate, Guid termId, string label, int wssId = -1)
        {
            if (fieldToUpdate == null)
            {
                throw new ArgumentNullException(nameof(fieldToUpdate));
            }

            if (termId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(termId));
            }

            if (label == null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            return new FieldTaxonomyValue()
            {
                TermId = termId,
                Label = label,
                WssId = wssId,
                Field = fieldToUpdate
            };
        }

        public IFieldValueCollection NewFieldValueCollection(IField fieldToUpdate)
        {
            return new FieldValueCollection(fieldToUpdate, fieldToUpdate.InternalName);
        }
        #endregion

        #region Permissions

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceAsync(copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes)
        {
            ApiCall apiCall = BuildBreakRoleInheritanceApiCall(copyRoleAssignments, clearSubscopes);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void BreakRoleInheritanceBatch(Batch batch, bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceBatchAsync(batch, copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceBatchAsync(Batch batch, bool copyRoleAssignments, bool clearSubscopes)
        {
            ApiCall apiCall = BuildBreakRoleInheritanceApiCall(copyRoleAssignments, clearSubscopes);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void BreakRoleInheritanceBatch(bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceBatchAsync(copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceBatchAsync(bool copyRoleAssignments, bool clearSubscopes)
        {
            await BreakRoleInheritanceBatchAsync(PnPContext.CurrentBatch, copyRoleAssignments, clearSubscopes).ConfigureAwait(false);
        }

        private ApiCall BuildBreakRoleInheritanceApiCall(bool copyRoleAssignments, bool clearSubscopes)
        {
            return new ApiCall($"{GetItemUri()}/BreakRoleInheritance(copyRoleAssignments=" + copyRoleAssignments.ToString().ToLower() + ",clearSubscopes=" + clearSubscopes.ToString().ToLower() + ")", ApiType.SPORest);
        }

        public void ResetRoleInheritance()
        {
            ResetRoleInheritanceAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceAsync()
        {
            ApiCall apiCall = BuildResetRoleInheritanceApiCall();
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void ResetRoleInheritanceBatch(Batch batch)
        {
            ResetRoleInheritanceBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildResetRoleInheritanceApiCall();
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void ResetRoleInheritanceBatch()
        {
            ResetRoleInheritanceBatchAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceBatchAsync()
        {
            await ResetRoleInheritanceBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        private ApiCall BuildResetRoleInheritanceApiCall()
        {
            return new ApiCall($"{GetItemUri()}/ResetRoleInheritance", ApiType.SPORest);
        }

        public IRoleDefinitionCollection GetRoleDefinitions(int principalId)
        {
            return GetRoleDefinitionsAsync(principalId).GetAwaiter().GetResult();
        }

        public async Task<IRoleDefinitionCollection> GetRoleDefinitionsAsync(int principalId)
        {
            await EnsurePropertiesAsync(l => l.RoleAssignments).ConfigureAwait(false);
            var roleAssignment = await RoleAssignments
                .QueryProperties(r => r.RoleDefinitions)
                .FirstOrDefaultAsync(p => p.PrincipalId == principalId)
                .ConfigureAwait(false);
            return roleAssignment?.RoleDefinitions;
        }

        public bool AddRoleDefinitions(int principalId, params string[] names)
        {
            return AddRoleDefinitionsAsync(principalId, names).GetAwaiter().GetResult();
        }

        public async Task<bool> AddRoleDefinitionsAsync(int principalId, params string[] names)
        {
            foreach (var name in names)
            {
                var roleDefinition = await PnPContext.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == name).ConfigureAwait(false);
                if (roleDefinition != null)
                {
                    await AddRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found.");
                }
            }
            return false;
        }

        private ApiCall BuildAddRoleDefinitionsApiCall(int principalId, IRoleDefinition roleDefinition)
        {
            return new ApiCall($"{GetItemUri()}/roleassignments/addroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
        }

        public async Task AddRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildAddRoleDefinitionsApiCall(principalId, roleDefinition);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddRoleDefinition(int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public void AddRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionBatchAsync(batch, principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task AddRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildAddRoleDefinitionsApiCall(principalId, roleDefinition);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition)
        {
            AddRoleDefinitionBatchAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task AddRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition)
        {
            await AddRoleDefinitionBatchAsync(PnPContext.CurrentBatch, principalId, roleDefinition).ConfigureAwait(false);
        }

        public bool RemoveRoleDefinitions(int principalId, params string[] names)
        {
            return RemoveRoleDefinitionsAsync(principalId, names).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names)
        {
            foreach (var name in names)
            {
                var roleDefinitions = await GetRoleDefinitionsAsync(principalId).ConfigureAwait(false);

                var roleDefinition = roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == name);
                if (roleDefinition != null)
                {
                    await RemoveRoleDefinitionAsync(principalId, roleDefinition).ConfigureAwait(false);
                    return true;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found for this group.");
                }
            }
            return false;
        }

        public async Task RemoveRoleDefinitionAsync(int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildRemoveRoleDefinitionApiCall(principalId, roleDefinition);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void RemoveRoleDefinition(int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public void RemoveRoleDefinitionBatch(int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionBatchAsync(principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task RemoveRoleDefinitionBatchAsync(int principalId, IRoleDefinition roleDefinition)
        {
            await RemoveRoleDefinitionBatchAsync(PnPContext.CurrentBatch, principalId, roleDefinition).ConfigureAwait(false);
        }

        public void RemoveRoleDefinitionBatch(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            RemoveRoleDefinitionBatchAsync(batch, principalId, roleDefinition).GetAwaiter().GetResult();
        }

        public async Task RemoveRoleDefinitionBatchAsync(Batch batch, int principalId, IRoleDefinition roleDefinition)
        {
            ApiCall apiCall = BuildRemoveRoleDefinitionApiCall(principalId, roleDefinition);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall BuildRemoveRoleDefinitionApiCall(int principalId, IRoleDefinition roleDefinition)
        {
            return new ApiCall($"{GetItemUri()}/roleassignments/removeroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
        }

        #endregion

        #region Get Changes

        public async Task<IList<IChange>> GetChangesAsync(ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList();
        }

        public IList<IChange> GetChanges(ChangeQueryOptions query)
        {
            return GetChangesAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #region Get Comments
        public ICommentCollection GetComments(params Expression<Func<IComment, object>>[] selectors)
        {
            return GetCommentsAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<ICommentCollection> GetCommentsAsync(params Expression<Func<IComment, object>>[] selectors)
        {
            var apiCall = await GetApiCallAsync(this, PnPContext, selectors);

            // Since Get methods return a disconnected set of data and we've just loaded data into the 
            // not exposed Comments property we're replicating this ListItem and return the replicated
            // comments collection
            IDataModelParent replicatedParent = null;

            // Create a replicated parent
            if (this.Parent != null)
            {
                // Replicate the parent object in order to keep original collection as is
                replicatedParent = EntityManager.ReplicateParentHierarchy(this.Parent, this.PnPContext);
            }
            // Create a new object with a replicated parent
            var newDataModel = (BaseDataModel<IListItem>)EntityManager.GetEntityConcreteInstance(this.GetType(), replicatedParent, this.PnPContext);

            // Replicate metadata and key between the objects
            EntityManager.ReplicateKeyAndMetadata(this, newDataModel);

            //return await BaseDataModelExtensions.BaseGetAsync(this, new ApiCall($"_api/web/getlist('{serverRelativeUrl}')", ApiType.SPORest), selectors).ConfigureAwait(false);

            await newDataModel.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return (newDataModel as ListItem).Comments;
        }

        private static async Task<ApiCall> GetApiCallAsync<TModel>(IDataModel<TModel> listItem, PnPContext context, params Expression<Func<IComment, object>>[] selectors)
        {
            string itemApi = null;
            if (listItem is IDataModelParent && listItem.Parent is IListItemCollection)
            {
                itemApi = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})";
            }

            var apiCall = new ApiCall($"{itemApi}/getcomments", ApiType.SPORest, receivingProperty: nameof(Comments));
            if (selectors != null && selectors.Any())
            {
                var tempComment = new Comment()
                {
                    PnPContext = context
                };

                var entityInfo = EntityManager.GetClassInfo(tempComment.GetType(), tempComment, expressions: selectors);
                var query = await QueryClient.BuildGetAPICallAsync(tempComment, entityInfo, apiCall).ConfigureAwait(false);
                return new ApiCall(query.ApiCall.Request, ApiType.SPORest, receivingProperty: nameof(Comments));
            }
            else
            {
                return apiCall;
            }
        }
        
        #endregion

        #endregion
    }
}
