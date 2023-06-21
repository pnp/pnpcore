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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListItem", Target = typeof(List), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/items({Id})", LinqGet = "_api/web/lists(guid'{Parent.Id}')/items")]
    [SharePointType("SP.ListItem", Target = typeof(File), Uri = "_api/web/getFileById('{Parent.Id}')/listitemallfields")]
    [SharePointType("SP.ListItem", Target = typeof(Folder), Uri = "_api/Web/getFolderById('{Parent.Id}')/listitemallfields")]
    //[GraphType(OverflowProperty = "fields")]
    internal sealed class ListItem : ExpandoBaseDataModel<IListItem>, IListItem
    {
        internal const string FolderPath = "folderPath";
        internal const string UnderlyingObjectType = "underlyingObjectType";

        #region Construction
        public ListItem()
        {
            MappingHandler = (FromJson input) =>
            {
                // The AddValidateUpdateItemUsingPath call returns the id of the added list item
                if (input.FieldName == "value")
                {
                    if (input.JsonElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var field in input.JsonElement.EnumerateArray())
                        {
                            var fieldName = field.GetProperty("FieldName").GetString();
                            var fieldValue = field.GetProperty("FieldValue").GetString();

                            // In some cases SharePoint will return HTTP 200 indicating the list item was added ok, but one or more fields could 
                            // not be added which is indicated via the HasException property on the field. If so then throw an error.
                            if (field.TryGetProperty("HasException", out JsonElement hasExceptionProperty) && hasExceptionProperty.GetBoolean() == true)
                            {
                                bool handled = false;
                                if (!input.ApiResponse.Equals(default) && input.ApiResponse.BatchRequestId != Guid.Empty)
                                {
                                    var actualBatch = PnPContext.BatchClient.GetBatchByBatchRequestId(input.ApiResponse.BatchRequestId);
                                    if (!actualBatch.ThrowOnError)
                                    {
                                        // Add error to used batch
                                        actualBatch.AddBatchResult(actualBatch.GetRequest(input.ApiResponse.BatchRequestId),
                                                                 System.Net.HttpStatusCode.OK,
                                                                 input.JsonElement.ToString(),
                                                                 new SharePointRestError(ErrorType.SharePointRestServiceError, (int)System.Net.HttpStatusCode.OK, string.Format(PnPCoreResources.Exception_ListItemAdd_WrongInternalFieldName, fieldName)));
                                        handled = true;     
                                    }
                                }

                                if (!handled)
                                {
                                    throw new SharePointRestServiceException(string.Format(PnPCoreResources.Exception_ListItemAdd_WrongInternalFieldName, fieldName));
                                }
                            }

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

                                    // Ensure the values are committed to the model when an item is being added: this
                                    // will ensure there's no pending changes anymore
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
                var parentListUri = $"{PnPContext.Uri.AbsoluteUri}/_api/Web/Lists(guid'{parentList.Id}')";

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
                var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedTrue);

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

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }

        // Not in public interface as Comments is not an expandable property in REST
        public ICommentCollection Comments { get => GetModelCollectionValue<ICommentCollection>(); }

        public ILikedByInformation LikedByInformation { get => GetModelValue<ILikedByInformation>(); }

        public IListItemVersionCollection Versions { get => GetModelCollectionValue<IListItemVersionCollection>(); }

        public IAttachmentCollection AttachmentFiles { get => GetModelCollectionValue<IAttachmentCollection>(); }

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
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement displayName))
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
                await LoadKeyListItemPropertiesAsync().ConfigureAwait(false);
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
                await LoadKeyListItemPropertiesAsync().ConfigureAwait(false);
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
                await LoadKeyListItemPropertiesAsync().ConfigureAwait(false);
            }

            return await PnPContext.Web.GetFolderByServerRelativeUrlAsync(Values["FileDirRef"].ToString()).ConfigureAwait(false);
        }

        public IFolder GetParentFolder()
        {
            return GetParentFolderAsync().GetAwaiter().GetResult();
        }
        
        #endregion

        #region MoveTo

        public async Task MoveToAsync(string destinationFolderUrl)
        {
            if (destinationFolderUrl.StartsWith("/"))
            {
                destinationFolderUrl = destinationFolderUrl.TrimStart('/');
            }
            
            if (destinationFolderUrl.EndsWith("/"))
            {
                destinationFolderUrl = destinationFolderUrl.TrimEnd('/');
            }
            
            await EnsurePropertiesAsync(item =>
                item.ParentList.QueryProperties(
                    l => l.Id,
                    l => l.RootFolder.QueryProperties(f => f.ServerRelativeUrl)
                )).ConfigureAwait(false);

            IFolder folder = await PnPContext.Web
                .GetFolderByServerRelativeUrlAsync($"{ParentList.RootFolder.ServerRelativeUrl}/{destinationFolderUrl}",
                    f => f.ServerRelativeUrl).ConfigureAwait(false);

            if (!Values.ContainsKey("FileRef"))
            {
                await LoadKeyListItemPropertiesAsync().ConfigureAwait(false);
            }

            var filename = Path.GetFileName(Values["FileRef"].ToString());
            
            string destinationUrl =
                $"{UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, UrlUtility.EnsureTrailingSlash(folder.ServerRelativeUrl))}{filename}";

            ApiCall apiCall = await GetMoveToApiCallAsync(destinationUrl).ConfigureAwait(false);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }
        
        public void MoveTo(string destinationFolderUrl)
        {
            MoveToAsync(destinationFolderUrl).GetAwaiter().GetResult();
        }

        private async Task<ApiCall> GetMoveToApiCallAsync(string destinationUrl)
        {
            MoveCopyOptions options = new();
            
            string destUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, destinationUrl).ToString();

            if (!Values.ContainsKey("FileRef"))
            {
                await LoadKeyListItemPropertiesAsync().ConfigureAwait(false);
            }

            string srcUrl = UrlUtility.EnsureAbsoluteUrl(PnPContext.Uri, Values["FileRef"].ToString()).ToString();

            ExpandoObject parameters = new
            {
                destPath = new {__metadata = new {type = "SP.ResourcePath"}, DecodedUrl = destUrl},
                srcPath = new {__metadata = new {type = "SP.ResourcePath"}, DecodedUrl = srcUrl},
                options = new
                {
                    __metadata = new {type = "SP.MoveCopyOptions"},
                    options.KeepBoth,
                    options.ResetAuthorAndCreatedOnCopy,
                    options.RetainEditorAndModifiedOnMove,
                    options.ShouldBypassSharedLocks
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(parameters, typeof(ExpandoObject));
            string copyToEndpointUrl =
                $"_api/SP.MoveCopyUtil.MoveFileByPath(overwrite=@a1)?@a1=true";

            return new ApiCall(copyToEndpointUrl, ApiType.SPORest, body);
        }

        private async Task LoadKeyListItemPropertiesAsync()
        {
            ApiCall apiCall = new ApiCall($"{GetItemUri()}?$select=ContentTypeId,FileDirRef,FileRef", ApiType.SPORest);
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
                PnPConstants.JsonSerializer_WriteIndentedTrue);

            var entityInfo = EntityManager.Instance.GetStaticClassInfo(this.GetType());
            
            if (Parent is IManageableCollection)
            {
                // Parent is a collection, so jump one level up
                entityInfo.Target = Parent.Parent.GetType();
            }
            else
            {
                entityInfo.Target = Parent.GetType();
            }
            
            var itemUri = $"{PnPContext.Uri}/{entityInfo.SharePointUri}";

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
                    if (fieldValueCollection.Field != null)
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
                        if (fieldValueCollection.Values.Count == 0)
                        {
                            field.FieldValue = "";
                        }
                        else
                        {
                            var first = fieldValueCollection.Values.First();
                            if (first is IFieldUserValue)
                            {
                                field.FieldValue = fieldValueCollection.UserMultiToValidateUpdateItemJson();
                            }
                            else if (first is IFieldLookupValue)
                            {
                                field.FieldValue = fieldValueCollection.LookupMultiToValidateUpdateItemJson();
                            }
                            else if (first is IFieldTaxonomyValue)
                            {
                                field.FieldValue = fieldValueCollection.TaxonomyMultiToValidateUpdateItemJson();
                            }
                        }
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
                field.FieldValue = DateTimeToSharePointWebDateTimeString(context, dateValue);
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

        private static string DateTimeToSharePointWebDateTimeString(PnPContext context, DateTime input)
        {
            // Convert incoming date to UTC
            DateTime inputInUTC = input.ToUniversalTime();

            // Convert to the time zone used by the SharePoint site, take in account the daylight savings delta
            DateTime localDateTime = context.Web.RegionalSettings.TimeZone.UtcToLocalTime(inputInUTC);

            // Apply the delta from UTC to get the date used by the site and apply formatting 
            return (localDateTime).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
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

        internal async Task PrepareUpdateCall(UpdateListItemRequest request)
        {
            string listId = "";
            if (this.Parent is IFile file)
            {
                // When it's a file then we need to resolve the {Parent.Id} token manually as otherwise this 
                // will point to the File id while we need to list Id here
                await file.EnsurePropertiesAsync(p => p.ListId).ConfigureAwait(false);
                listId = file.ListId.ToString();
            }

            if (this.Parent.Parent is IList)
            {
                listId = (this.Parent.Parent as IList).Id.ToString();
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
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

                if (json.TryGetProperty("value", out JsonElement commentsDisabled))
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
            if (document.TryGetProperty("value", out JsonElement recycleBinItemId))
            {
                // return the recyclebin item id
                return recycleBinItemId.GetGuid();
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
            if (names == null || names.Length == 0)
            {
                return false;
            }

            var roleDefinitions = await PnPContext.Web.RoleDefinitions.ToListAsync().ConfigureAwait(false);
            var batch = PnPContext.NewBatch();
            foreach (var name in names)
            {
                var roleDefinition = roleDefinitions.FirstOrDefault(d => d.Name == name);
                if (roleDefinition != null)
                {
                    await AddRoleDefinitionBatchAsync(batch, principalId, roleDefinition).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException(string.Format(PnPCoreResources.Exception_RoleDefinition_NotFound, name));
                }
            }
            // Send role updates to server
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            return true;
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
            if (names == null || names.Length == 0)
            {
                return false;
            }

            var roleDefinitions = await GetRoleDefinitionsAsync(principalId).ConfigureAwait(false);
            var batch = PnPContext.NewBatch();
            foreach (var name in names)
            {
                var roleDefinition = roleDefinitions.AsRequested().FirstOrDefault(r => r.Name == name);
                if (roleDefinition != null)
                {
                    await RemoveRoleDefinitionBatchAsync(batch, principalId, roleDefinition).ConfigureAwait(false);
                }
                else
                {
                    throw new ArgumentException(string.Format(PnPCoreResources.Exception_RoleDefinition_NotFound, name));
                }
            }
            
            // Send role updates to server
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            return true;
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

        #region Graph Permissions

        // Currently these are not supported using Microsoft Graph

        //public async Task<IGraphPermissionCollection> GetShareLinksAsync()
        //{
        //    var listId = await GetListIdAsync().ConfigureAwait(false);

        //    var apiCall = new ApiCall($"sites/{PnPContext.Uri.DnsSafeHost},{PnPContext.Site.Id},{PnPContext.Web.Id}/lists/{listId}/items/{Id}/permissions?$filter=Link ne null", ApiType.GraphBeta);
        //    var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

        //    if (string.IsNullOrEmpty(response.Json))
        //    {
        //        throw new Exception("No values found");
        //    }

        //    var graphPermissions = SharingManager.DeserializeGraphPermissionsResponse(response.Json, PnPContext, this);

        //    return graphPermissions;
        //}


        //public IGraphPermissionCollection GetShareLinks()
        //{
        //    return GetShareLinksAsync().GetAwaiter().GetResult();
        //}

        //public async Task DeleteShareLinksAsync()
        //{
        //    var shareLinks = await GetShareLinksAsync().ConfigureAwait(false);
        //    foreach (var shareLink in shareLinks)
        //    {
        //        await shareLink.DeletePermissionAsync().ConfigureAwait(false);
        //    }
        //}

        //public void DeleteShareLinks()
        //{
        //    DeleteShareLinksAsync().GetAwaiter().GetResult();
        //}

        public async Task<IGraphPermission> CreateOrganizationalSharingLinkAsync(OrganizationalLinkOptions organizationalLinkOptions)
        {
            if (!IsValidShareTypeForListItem(organizationalLinkOptions.Type))
            {
                throw new ArgumentException("List item sharing only supports the types: View, Review and Embed");
            }

            dynamic body = new ExpandoObject();
            body.scope = ShareScope.Organization;
            body.type = organizationalLinkOptions.Type;

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);

        }

        public IGraphPermission CreateOrganizationalSharingLink(OrganizationalLinkOptions organizationalLinkOptions)
        {
            return CreateOrganizationalSharingLinkAsync(organizationalLinkOptions).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateAnonymousSharingLinkAsync(AnonymousLinkOptions anonymousLinkOptions)
        {
            if (!IsValidShareTypeForListItem(anonymousLinkOptions.Type))
            {
                throw new ArgumentException("List item sharing only supports the types: View, Review and Embed");
            }

            dynamic body = new ExpandoObject();

            body.scope = ShareScope.Anonymous;
            body.type = anonymousLinkOptions.Type;
            body.password = anonymousLinkOptions.Password;

            if (anonymousLinkOptions.ExpirationDateTime != DateTime.MinValue)
            {
                body.expirationDateTime = anonymousLinkOptions.ExpirationDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            }

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
        }

        public IGraphPermission CreateAnonymousSharingLink(AnonymousLinkOptions anonymousLinkOptions)
        {
            return CreateAnonymousSharingLinkAsync(anonymousLinkOptions).GetAwaiter().GetResult();
        }

        private async Task<IGraphPermission> CreateSharingLinkAsync(dynamic body)
        {
            var listId = await GetListIdAsync().ConfigureAwait(false);

            var apiCall = new ApiCall($"sites/{PnPContext.Uri.DnsSafeHost},{PnPContext.Site.Id},{PnPContext.Web.Id}/lists/{listId}/items/{Id}/createLink", ApiType.GraphBeta, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse_CamelCase_JsonStringEnumConverter));
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
                return SharingManager.DeserializeGraphPermission(json, PnPContext, this);
            }
            else
            {
                throw new Exception("Error occured during creation");
            }
        }

        public IGraphPermission CreateAnonymousSharingLink(OrganizationalLinkOptions organizationalLinkOptions)
        {
            return CreateOrganizationalSharingLinkAsync(organizationalLinkOptions).GetAwaiter().GetResult();
        }

        public async Task<IGraphPermission> CreateUserSharingLinkAsync(UserLinkOptions userLinkOptions)
        {
            if (!IsValidShareTypeForListItem(userLinkOptions.Type))
            {
                throw new ArgumentException("List item sharing only supports the types: View, Review and Embed");
            }

            if (userLinkOptions.Recipients == null || userLinkOptions.Recipients.Count == 0)
            {
                throw new ArgumentException("We need to have atleast one recipient with whom we want to share the link");
            }

            dynamic body = new ExpandoObject();

            body.scope = ShareScope.Users;
            body.type = userLinkOptions.Type;
            body.recipients = userLinkOptions.Recipients;

            return await CreateSharingLinkAsync(body).ConfigureAwait(false);
        }

        public IGraphPermission CreateUserSharingLink(UserLinkOptions userLinkOptions)
        {
            return CreateUserSharingLinkAsync(userLinkOptions).GetAwaiter().GetResult();
        }

        private static bool IsValidShareTypeForListItem(ShareType shareType)
        {
            if (shareType == ShareType.View || shareType == ShareType.Edit || shareType == ShareType.Embed)
            {
                return true;
            }

            return false;
        }

        internal async Task<Guid> GetListIdAsync()
        {
            // Option A: Use the loaded parent list property
            if (IsPropertyAvailable(p => p.ParentList) && ParentList.IsPropertyAvailable(p => p.Id))
            {
                return ParentList.Id;
            }

            var listId = Guid.Empty;

            //Option B: walk the parent tree
            listId = GetListIdFromParent(this);

            if (listId != Guid.Empty)
            {
                return listId;
            }

            // Option C: load parent list (requires server roundtrip)
            await LoadAsync(p => p.ParentList).ConfigureAwait(false);
            if (IsPropertyAvailable(p => p.ParentList) && ParentList.IsPropertyAvailable(p => p.Id))
            {
                return ParentList.Id;
            }

            // We should never get here...
            return Guid.Empty;
        }

        private static Guid GetListIdFromParent(IDataModelParent listItem)
        {
            if (listItem != null)
            {
                if (listItem.Parent != null && listItem.Parent is List)
                {
                    return (listItem.Parent as List).Id;
                }
                else
                {
                    if (listItem.Parent == null)
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        return GetListIdFromParent(listItem.Parent);
                    }
                }
            }

            return Guid.Empty;
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

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(Batch batch, ChangeQueryOptions query)
        {
            var apiCall = ChangeCollectionHandler.GetApiCall(this, query);
            apiCall.RawEnumerableResult = new List<IChange>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var batchFirstRequest = batch.Requests.First().Value;
                ApiCallResponse response = new ApiCallResponse(apiCall, json, System.Net.HttpStatusCode.OK, batchFirstRequest.Id, batchFirstRequest.ResponseHeaders);
                ((List<IChange>)apiCall.RawEnumerableResult).AddRange(ChangeCollectionHandler.Deserialize(response, this, PnPContext).ToList());
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IChange>(batch, batchRequest.Id, (IReadOnlyList<IChange>)apiCall.RawEnumerableResult);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(Batch batch, ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IChange>> GetChangesBatchAsync(ChangeQueryOptions query)
        {
            return await GetChangesBatchAsync(PnPContext.CurrentBatch, query).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IChange> GetChangesBatch(ChangeQueryOptions query)
        {
            return GetChangesBatchAsync(query).GetAwaiter().GetResult();
        }

        #endregion

        #region Comments and liking
        public ICommentCollection GetComments(params Expression<Func<IComment, object>>[] selectors)
        {
            return GetCommentsAsync(selectors).GetAwaiter().GetResult();
        }

        public async Task<ICommentCollection> GetCommentsAsync(params Expression<Func<IComment, object>>[] selectors)
        {
            var apiCall = await BuildGetCommentsApiCallAsync(this, PnPContext, selectors).ConfigureAwait(false);

            // Since Get methods return a disconnected set of data and we've just loaded data into the 
            // not exposed Comments property we're replicating this ListItem and return the replicated
            // comments collection
            IDataModelParent replicatedParent = null;

            // Create a replicated parent
            if (Parent != null)
            {
                // Replicate the parent object in order to keep original collection as is
                replicatedParent = EntityManager.ReplicateParentHierarchy(Parent, PnPContext);
            }
            // Create a new object with a replicated parent
            var newDataModel = (BaseDataModel<IListItem>)EntityManager.GetEntityConcreteInstance(GetType(), replicatedParent, PnPContext);

            // Replicate metadata and key between the objects
            EntityManager.ReplicateKeyAndMetadata(this, newDataModel);

            // Load the comments on the replicated model
            await newDataModel.RequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            return (newDataModel as ListItem).Comments;
        }

        private static async Task<ApiCall> BuildGetCommentsApiCallAsync<TModel>(IDataModel<TModel> listItem, PnPContext context, params Expression<Func<IComment, object>>[] selectors)
        {
            string itemApi = null;
            if (listItem.Parent is IListItemCollection)
            {
                itemApi = "_api/web/lists/getbyid(guid'{List.Id}')/items({Id})";
            }
            else if (listItem.Parent is IFile)
            {
                itemApi = "_api/web/lists(guid'{List.Id}')/getitembyid({Id})";
            }

            var apiCall = new ApiCall($"{itemApi}/getcomments", ApiType.SPORest, receivingProperty: nameof(Comments));
            if (selectors != null && selectors.Any())
            {
                // Use the query client to translate the seletors into the needed query string
                var tempComment = new Comment()
                {
                    PnPContext = context,
                    // Ensure the temp comment has a parent as otherwise token resolving can fail when the page
                    // list item was not reloaded after comments were added                    
                    Parent = (listItem as ListItem).Comments
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

        public async Task LikeAsync()
        {
            await LikeUnlike(true).ConfigureAwait(false);
        }

        public void Like()
        {
            LikeAsync().GetAwaiter().GetResult();
        }

        public async Task UnlikeAsync()
        {
            await LikeUnlike(false).ConfigureAwait(false);
        }

        public void Unlike()
        {
            UnlikeAsync().GetAwaiter().GetResult();
        }

        private async Task LikeUnlike(bool like)
        {
            var baseApiCall = $"_api/web/lists(guid'{{List.Id}}')/getitembyid({{Id}})";

            if (like)
            {
                await RequestAsync(new ApiCall($"{baseApiCall}/like", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
            else
            {
                await RequestAsync(new ApiCall($"{baseApiCall}/unlike", ApiType.SPORest), HttpMethod.Post).ConfigureAwait(false);
            }
        }

        #endregion

        #region User effective permissions

        public IBasePermissions GetUserEffectivePermissions(string userPrincipalName)
        {
            return GetUserEffectivePermissionsAsync(userPrincipalName).GetAwaiter().GetResult();
        }

        public async Task<IBasePermissions> GetUserEffectivePermissionsAsync(string userPrincipalName)
        {
            if (string.IsNullOrEmpty(userPrincipalName))
            {
                throw new ArgumentNullException(PnPCoreResources.Exception_UserPrincipalNameEmpty);
            }

            var listId = await GetListIdAsync().ConfigureAwait(false);

            var apiCall = BuildGetUserEffectivePermissionsApiCall(userPrincipalName, listId);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Json))
            {
                throw new Exception(PnPCoreResources.Exception_EffectivePermissionsNotFound);
            }

            return EffectivePermissionsHandler.ParseGetUserEffectivePermissionsResponse(response.Json);
        }

        private ApiCall BuildGetUserEffectivePermissionsApiCall(string userPrincipalName, Guid parentListId)
        {
            return new ApiCall($"_api/web/lists(guid'{parentListId}')/items({Id})/getusereffectivepermissions('{HttpUtility.UrlEncode("i:0#.f|membership|")}{userPrincipalName}')", ApiType.SPORest);
        }

        public bool CheckIfUserHasPermissions(string userPrincipalName, PermissionKind permissionKind)
        {
            return CheckIfUserHasPermissionsAsync(userPrincipalName, permissionKind).GetAwaiter().GetResult();
        }

        public async Task<bool> CheckIfUserHasPermissionsAsync(string userPrincipalName, PermissionKind permissionKind)
        {
            var basePermissions = await GetUserEffectivePermissionsAsync(userPrincipalName).ConfigureAwait(false);
            return basePermissions.Has(permissionKind);
        }

        #endregion

        #endregion
    }
}
