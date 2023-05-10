using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Requests.Web;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    [SharePointType("SP.ContentType", Target = typeof(ContentTypeHub), Uri = "_api/Web/ContentTypes('{Id}')", LinqGet = "_api/web/ContentTypes")]
    [SharePointType("SP.ContentType", Target = typeof(Web), Uri = "_api/Web/ContentTypes('{Id}')", LinqGet = "_api/web/ContentTypes")]
    [SharePointType("SP.ContentType", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/ContentTypes('{Id}')", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/ContentTypes")]
    internal sealed class ContentType : BaseDataModel<IContentType>, IContentType
    {
        #region Construction
        public ContentType()
        {
            PostMappingHandler = (json) =>
            {
                // Trying to load values with the selectors will result in this not having a StringId sometimes.
                // Prevent a ClientException by checking first
                if (HasValue(nameof(StringId)))
                {
                    // In the case of Add operation, the CSOM api is used and JSON mapping to model is not possible
                    // So here, populate the Rest Id metadata field to enable actions upon 
                    // this content type without having to read it again from the server
                    Id = StringId;
                    AddMetadata(PnPConstants.MetaDataRestId, StringId);
                    AddMetadata(PnPConstants.MetaDataType, "SP.ContentType");
                    Requested = true;
                }
            };

            // Handler to construct the Add request for this content type
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (keyValuePairs) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                // Content type creation by specifying the parent id is not possible using SharePoint REST
                //https://github.com/pnp/pnpjs/issues/457
                //https://github.com/SharePoint/sp-dev-docs/issues/3276
                //https://stackoverflow.com/questions/55529315/how-to-create-site-content-type-with-id-using-rest-api

                // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
                // automatically provide the correct 'parent'
                var entity = EntityManager.GetClassInfo(GetType(), this);

                // Adding new content types on a list is not something we should allow
                if (entity.Target == typeof(List))
                {
                    throw new ClientException(ErrorType.Unsupported,
                        PnPCoreResources.Exception_Unsupported_AddingContentTypeToList);
                }

                // Fallback to CSOM call
                CreateContentTypeRequest request = new CreateContentTypeRequest(new ContentTypeCreationInfo()
                {
                    Id = StringId,
                    Description = Description,
                    Group = Group,
                    Name = Name
                });


                string requestUrl = PnPContext.Uri.ToString();
                if (IsContentTypeHub())
                {
                    requestUrl = requestUrl.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                }

                return new ApiCall(new List<IRequest<object>>() { request })
                {
                    Request = requestUrl
                };
            };

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            GetApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                if (IsContentTypeHub())
                {
                    var request = api.ApiCall.Request.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                    api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                }

                return api;
            };


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            UpdateApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                if (IsContentTypeHub())
                {
                    var request = api.ApiCall.Request.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                    api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                }
                return api;
            };


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            DeleteApiCallOverrideHandler = async (ApiCallRequest api) =>
            {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                if (IsContentTypeHub())
                {
                    var request = api.ApiCall.Request.Replace(PnPContext.Uri.AbsolutePath, PnPConstants.ContentTypeHubUrl);
                    api.ApiCall = new ApiCall(request, api.ApiCall.Type, api.ApiCall.JsonBody, api.ApiCall.ReceivingProperty);
                }
                return api;
            };

        }
        #endregion

        #region Properties
        public string StringId { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("Id", JsonPath = "StringValue")]
        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public string ClientFormCustomFormatter { get => GetValue<string>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string DisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public string DocumentTemplateUrl { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string EditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Group { get => GetValue<string>(); set => SetValue(value); }

        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public string JSLink { get => GetValue<string>(); set => SetValue(value); }

        public string MobileDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string MobileNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormTemplateName { get => GetValue<string>(); set => SetValue(value); }

        public string NewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool ReadOnly { get => GetValue<bool>(); set => SetValue(value); }

        public string SchemaXml { get => GetValue<string>(); set => SetValue(value); }

        public string SchemaXmlWithResourceTokens { get => GetValue<string>(); set => SetValue(value); }

        public string Scope { get => GetValue<string>(); set => SetValue(value); }

        public bool Sealed { get => GetValue<bool>(); set => SetValue(value); }

        public IFieldLinkCollection FieldLinks { get => GetModelCollectionValue<IFieldLinkCollection>(); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        [KeyProperty(nameof(StringId))]
        public override object Key { get => StringId; set => StringId = value.ToString(); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion

        #region Methods

        public async Task<IDocumentSet> AsDocumentSetAsync()
        {
            if (!Id.StartsWith("0x0120D520"))
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_ContentType_NoDocumentSet);
            }

            var apiCall = await GetDocumentSetApiCallAsync().ConfigureAwait(false);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException(PnPCoreResources.Exception_ContentType_ErrorObtaining);
            }

            var documentSet = new DocumentSet
            {
                PnPContext = PnPContext,
                Parent = this,
                ContentTypeId = Id,
            };

            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);
            DeserializeDocumentSet(json, documentSet);
            
            return documentSet;
        }

        public IDocumentSet AsDocumentSet()
        {
            return AsDocumentSetAsync().GetAwaiter().GetResult();
        }

        private async Task<ApiCall> GetDocumentSetApiCallAsync()
        {
            var siteId = await GetSiteIdAsync().ConfigureAwait(false);

            var requestUrl = $"sites/{siteId}/contentTypes/{Id}?$expand=DocumentSet/SharedColumns,DocumentSet/WelcomePageColumns&$select=documentSet";

            return new ApiCall(requestUrl, ApiType.Graph);
        }

        internal async Task AddFileToDefaultContentLocationAsync(string listId, string listItemId, string destinationName)
        {
            var apiCall = await AddFileToDefaultContentLocationApiCallAsync(listId, listItemId, destinationName).ConfigureAwait(false);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private async Task<ApiCall> AddFileToDefaultContentLocationApiCallAsync(string listId, string listItemId, string destinationName)
        {
            var siteId = await GetSiteIdAsync().ConfigureAwait(false);

            var requestUrl = $"sites/{siteId}/contentTypes/{Id}/copyToDefaultContentLocation";

            dynamic body = new ExpandoObject();
            body.destinationFileName = destinationName;

            dynamic sourceFile = new ExpandoObject();
            dynamic sharePointIds = new ExpandoObject();

            sharePointIds.listId = listId;
            sharePointIds.listItemId = listItemId;

            sourceFile.sharepointIds = sharePointIds;

            body.sourceFile = sourceFile;

            var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

            return new ApiCall(requestUrl, ApiType.Graph, bodyContent);
        }

        #region Deserialization of document set

        private static void DeserializeDocumentSet(JsonElement json, DocumentSet documentSet)
        {
            if (json.TryGetProperty("documentSet", out JsonElement documentSetJson))
            {
                if (documentSetJson.TryGetProperty("shouldPrefixNameToFile", out JsonElement shouldPrefix))
                {
                    documentSet.ShouldPrefixNameToFile = shouldPrefix.GetBoolean();
                }

                if (documentSetJson.TryGetProperty("welcomePageUrl", out JsonElement welcomePageUrl))
                {
                    documentSet.WelcomePageUrl = welcomePageUrl.GetString();
                }

                if (documentSetJson.TryGetProperty("allowedContentTypes", out JsonElement allowedContentTypes))
                {
                    var allowedContentTypesList = new List<IContentTypeInfo>();

                    foreach (var allowedContentType in allowedContentTypes.EnumerateArray())
                    {
                        allowedContentTypesList.Add(DeserializeContentType(allowedContentType));
                    }

                    documentSet.AllowedContentTypes = allowedContentTypesList;
                }
                if (documentSetJson.TryGetProperty("defaultContents", out JsonElement defaultContents))
                {
                    var defaultContentsList = new List<IDocumentSetContent>();

                    foreach (var defaultContent in defaultContents.EnumerateArray())
                    {
                        defaultContentsList.Add(DeseralizeDefaultContent(defaultContent));
                    }

                    documentSet.DefaultContents = defaultContentsList;
                } 
                else
                {
                    documentSet.DefaultContents = new List<IDocumentSetContent>();
                }
                if (documentSetJson.TryGetProperty("sharedColumns", out JsonElement sharedColumns))
                {
                    var sharedColumnsList = new List<IField>();

                    foreach (var sharedColumn in sharedColumns.EnumerateArray())
                    {
                        sharedColumnsList.Add(DeseralizeField(sharedColumn));
                    }

                    documentSet.SharedColumns = sharedColumnsList;
                }
                else
                {
                    documentSet.SharedColumns = new List<IField>();
                }
                if (documentSetJson.TryGetProperty("welcomePageColumns", out JsonElement welcomePageColumns))
                {
                    var welcomePageColumnsList = new List<IField>();

                    foreach (var welcomePageColumn in welcomePageColumns.EnumerateArray())
                    {
                        welcomePageColumnsList.Add(DeseralizeField(welcomePageColumn));
                    }

                    documentSet.WelcomePageColumns = welcomePageColumnsList;
                }
                else
                {
                    documentSet.WelcomePageColumns = new List<IField>();
                }
            }
        }

        private static IField DeseralizeField(JsonElement sharedColumn)
        {
            var field = new Field();

            if (sharedColumn.TryGetProperty("name", out JsonElement name))
            {
                field.InternalName = name.GetString();
            }

            if (sharedColumn.TryGetProperty("id", out JsonElement id))
            {
                field.Id = id.GetGuid();
            }

            return field;
        }

        private static IDocumentSetContent DeseralizeDefaultContent(JsonElement defaultContent)
        {
            var documentSetContent = new DocumentSetContent();

            if (defaultContent.TryGetProperty("fileName", out JsonElement fileName))
            {
                documentSetContent.FileName = fileName.GetString();
            }

            if (defaultContent.TryGetProperty("folderName", out JsonElement folderName))
            {
                documentSetContent.FolderName = folderName.GetString();
            }

            if (defaultContent.TryGetProperty("contentType", out JsonElement contentType))
            {
                var contentTypeInfo = new ContentTypeInfo();

                if (contentType.TryGetProperty("id", out JsonElement ctId))
                {
                    contentTypeInfo.Id = ctId.GetString();
                }

                if (contentType.TryGetProperty("name", out JsonElement ctName))
                {
                    contentTypeInfo.Name = ctName.GetString();
                }

                documentSetContent.ContentType = contentTypeInfo;
            }

            return documentSetContent;
        }

        private static IContentTypeInfo DeserializeContentType(JsonElement allowedContentType)
        {
            var contentTypeInfo = new ContentTypeInfo();

            if (allowedContentType.TryGetProperty("id", out JsonElement id))
            {
                contentTypeInfo.Id = id.GetString();
            }

            if (allowedContentType.TryGetProperty("name", out JsonElement name))
            {
                contentTypeInfo.Name = name.GetString();
            }

            return contentTypeInfo;
        }

        #endregion

        #endregion

        #region Extension methods

        #region Add Fields

        public async Task AddFieldAsync(IField field)
        {
            var apiCall = await GetFieldAddApiCallAsync(field).ConfigureAwait(false);

            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void AddField(IField field)
        {
            AddFieldAsync(field).GetAwaiter().GetResult();
        }

        private async Task<ApiCall> GetFieldAddApiCallAsync(IField field)
        {
            var siteId = await GetSiteIdAsync().ConfigureAwait(false);

            var requestUrl = $"sites/{siteId}/contentTypes/{Id}/columns";

            dynamic body = new ExpandoObject();

            ((IDictionary<string, object>)body)["sourceColumn@odata.bind"] = $"https://graph.microsoft.com/v1.0/sites/{siteId}/columns/{field.Id}";

            return new ApiCall(requestUrl, ApiType.Graph, jsonBody: JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues_CamelCase));
        }

        #endregion

        #region AddAvailableContentType
        private ApiCall AddAvailableContentTypeApiCall(string id)
        {
            if (IsContentTypeHub())
            {
                throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_AddingContentTypesToListOnContentTypeHub);
            }

            dynamic body = new ExpandoObject();
            body.contentTypeId = id;

            var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

            // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
            // automatically provide the correct 'parent'
            var entity = EntityManager.GetClassInfo(GetType(), this);

            return new ApiCall($"{entity.SharePointLinqGet}/AddAvailableContentType", ApiType.SPORest, bodyContent);
        }

        internal async Task<IContentType> AddAvailableContentTypeBatchAsync(Batch batch, string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            await RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return this;
        }

        internal async Task<IContentType> AddAvailableContentTypeAsync(string id)
        {
            var apiCall = AddAvailableContentTypeApiCall(id);
            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            return this;
        }

        #endregion

        #region AddAvailableContentTypeFromHub
        private ApiCall AddAvailableContentTypeFromHubApiCall(string id)
        {
            if (IsContentTypeHub())
            {
                throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_AddingContentTypesToListOnContentTypeHub);
            }

            dynamic body = new ExpandoObject();
            body.contentTypeId = id;

            var bodyContent = JsonSerializer.Serialize(body, typeof(ExpandoObject), PnPConstants.JsonSerializer_WriteIndentedFalse);

            // Given this method can apply on both Web.ContentTypes as List.ContentTypes we're getting the entity info which will 
            // automatically provide the correct 'parent'
            if (EntityManager.GetClassInfo(GetType(), this).SharePointTarget == typeof(Web))
            {
                return new ApiCall($"/sites/{PnPContext.Site.Id}/contentTypes/addCopyFromContentTypeHub", ApiType.Graph, bodyContent);
            }
            else if (EntityManager.GetClassInfo(GetType(), this).SharePointTarget == typeof(List))
            {
                Guid listId = GetListIdFromParent(this);

                return new ApiCall($"/sites/{PnPContext.Site.Id}/lists/{listId}/contentTypes/addCopyFromContentTypeHub", ApiType.Graph, bodyContent);
            }

            throw new ClientException(ErrorType.Unsupported, "You can only add content types from the content type hub to sites and lists");
        }

        internal async Task<ILongRunningOperation> AddAvailableContentTypeFromHubAsync(string id, AddContentTypeFromHubOptions options)
        {
            var apiCall = AddAvailableContentTypeFromHubApiCall(id);
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return null;
            }
            else if (response.StatusCode == HttpStatusCode.Accepted && response.Headers != null && response.Headers.ContainsKey("Location"))
            {
                options = EnsureAddContentTypeFromHubOptions(options);

                LongRunningOperation operation = new LongRunningOperation(response.Headers["Location"], PnPContext);

                if (options.WaitForCompletion)
                {
                    await operation.WaitForCompletionAsync(options.LongRunningOperationOptions).ConfigureAwait(false);
                    return null;
                }
                else
                {
                    return operation;
                }
            }
           
            throw new MicrosoftGraphServiceException(ErrorType.GraphServiceError, (int)response.StatusCode, response.Json);           
        }

        private static AddContentTypeFromHubOptions EnsureAddContentTypeFromHubOptions(AddContentTypeFromHubOptions options)
        {
            if (options == null)
            {
                return new AddContentTypeFromHubOptions();
            }

            return options;
        }

        private static Guid GetListIdFromParent(IDataModelParent contentType)
        {
            if (contentType != null)
            {
                if (contentType.Parent != null && contentType.Parent is List)
                {
                    return (contentType.Parent as List).Id;
                }
                else
                {
                    if (contentType.Parent == null)
                    {
                        return Guid.Empty;
                    }
                    else
                    {
                        return GetListIdFromParent(contentType.Parent);
                    }
                }
            }

            return Guid.Empty;
        }

        #endregion

        #region Publish

        public async Task PublishAsync()
        {
            CheckTarget();

            var apiCall = await GetApiCallAsync("publish").ConfigureAwait(false);

            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Publish()
        {
            PublishAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Unpublish

        public async Task UnpublishAsync()
        {
            CheckTarget();

            if (!await IsPublishedAsync().ConfigureAwait(false))
            {
                throw new Exception("An unpublish of a content type can only be done on already published content-types");
            }

            var apiCall = await GetApiCallAsync("unpublish").ConfigureAwait(false);

            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void Unpublish()
        {
            UnpublishAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region IsPublished

        public async Task<bool> IsPublishedAsync()
        {
            CheckTarget();

            var apiCall = await GetApiCallAsync("ispublished").ConfigureAwait(false);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error occured on executing the request");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(response.Json);

            var isPublished = false;

            if (json.TryGetProperty("value", out JsonElement value))
            {
                isPublished = value.GetBoolean();
            }

            return isPublished;
        }

        public bool IsPublished()
        {
            return IsPublishedAsync().GetAwaiter().GetResult();
        }

        private async Task<ApiCall> GetApiCallAsync(string urlToCall)
        {
            var contentTypeHubSiteId = await PnPContext.ContentTypeHub.GetSiteIdAsync().ConfigureAwait(false);

            return new ApiCall($"sites/{contentTypeHubSiteId}/contentTypes/{StringId}/{urlToCall}", ApiType.Graph);
        }

        #endregion

        private void CheckTarget()
        {
            if (!IsContentTypeHub())
            { 
                throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_PublishingContentTypeOutsideContentTypeHub);
            }
        }

        internal async Task<string> GetSiteIdAsync()
        {
            var siteId = PnPContext.Site.Id.ToString();

            if (IsContentTypeHub())
            {
                siteId = await PnPContext.ContentTypeHub.GetSiteIdAsync().ConfigureAwait(false);
            }

            return siteId;
        }

        #endregion

        #region Helper methods
        private bool IsContentTypeHub()
        {
            return EntityManager.GetClassInfo(GetType(), this).SharePointTarget == typeof(ContentTypeHub);
        }
        #endregion
    }
}
