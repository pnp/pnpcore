using Microsoft.Extensions.Logging;
using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List class, write your custom code here
    /// </summary>
    [SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
    //[GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}", LinqGet = "sites/{Parent.GraphId}/lists")]
    internal sealed class List : BaseDataModel<IList>, IList
    {
        // List of fields that loaded when the Lists collection is requested. This approach is needed as 
        // Graph requires the "system" field to be loaded as trigger to return all lists 
        internal const string SystemFacet = "system";
        //internal const string DefaultGraphFieldsToLoad = "system,createdDateTime,description,eTag,id,lastModifiedDateTime,name,webUrl,displayName,createdBy,lastModifiedBy,parentReference,list";
        internal static readonly Expression<Func<IList, object>>[] LoadFieldsExpression = { p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title) };

        #region Construction
        public List()
        {
            //MappingHandler = (FromJson input) =>
            //{
            //    // Handle the mapping from json to the domain model for the cases which are not generically handled
            //    switch (input.TargetType.Name)
            //    {
            //        // Special case, needed because we force a callout out to the mapping handler in case we load TemplateType via a Graph query
            //        case "ListTemplateType": return JsonMappingHelper.ToEnum<ListTemplateType>(input.JsonElement);
            //    }

            //    input.Log.LogDebug(PnPCoreResources.Log_Debug_JsonCannotMapField, input.FieldName);

            //    return null;
            //};

            // Handler to construct the Add request for this list
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            AddApiCallHandler = async (additionalInformation) =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                var entity = EntityManager.GetClassInfo(GetType(), this);

                var addParameters = new
                {
                    __metadata = new { type = entity.SharePointType },
                    BaseTemplate = TemplateType,
                    Title
                }.AsExpando();
                string body = JsonSerializer.Serialize(addParameters, typeof(ExpandoObject));
                return new ApiCall($"_api/web/lists", ApiType.SPORest, body);
            };

            /*
            // Sample handler that shows how to override the API call used for the delete of this entity
            DeleteApiCallOverrideHandler = (ApiCall apiCall) =>
            {
                return apiCall;
            };
            */

            /*
            // Sample update validation handler, can be used to prevent updates to a field (e.g. field validation, make readonly field, ...)
            ValidateUpdateHandler = (FieldUpdateRequest fieldUpdateRequest) => 
            {
                if (fieldUpdateRequest.FieldName == "Description")
                {
                    // Cancel update
                    //fieldUpdateRequest.CancelUpdate();

                    // Set other value to the field
                    //fieldUpdateRequest.Value = "bla";
                }
            };
            */
        }
        #endregion

        #region Properties
        public Guid Id { get => GetValue<Guid>(); set => SetValue(value); }

        //[GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        //[GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("DocumentTemplateUrl")]
        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public bool OnQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("BaseTemplate")]
        //[GraphProperty("list", JsonPath = "template")]
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }

        public bool EnableVersioning { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinorVersions { get => GetValue<bool>(); set => SetValue(value); }

        public int DraftVersionVisibility { get => GetValue<int>(); set => SetValue(value); }

        public bool EnableModeration { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("MajorWithMinorVersionsLimit")]
        public int MinorVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("MajorVersionLimit")]
        public int MaxVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        //[GraphProperty("list", JsonPath = "contentTypesEnabled")]
        public bool ContentTypesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        //[GraphProperty("list", JsonPath = "hidden")]
        public bool Hidden { get => GetValue<bool>(); set => SetValue(value); }

        public bool ForceCheckout { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableAttachments { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableFolderCreation { get => GetValue<bool>(); set => SetValue(value); }

        public Guid TemplateFeatureId { get => GetValue<Guid>(); set => SetValue(value); }

        public Dictionary<string, string> FieldDefaults { get => GetValue<Dictionary<string, string>>(); set => SetValue(value); }

        public bool NoCrawl { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("ListExperienceOptions")]
        public ListExperience ListExperience { get => GetValue<ListExperience>(); set => SetValue(value); }

        public string DefaultDisplayFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultEditFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultNewFormUrl { get => GetValue<string>(); set => SetValue(value); }

        public string DefaultViewUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool DefaultItemOpenInBrowser { get => GetValue<bool>(); set => SetValue(value); }

        public ListReadingDirection Direction { get => GetValue<ListReadingDirection>(); set => SetValue(value); }

        public string ImageUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmExpire { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmReject { get => GetValue<bool>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsApplicationList { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsCatalog { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsDefaultDocumentLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsPrivate { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSiteAssetsLibrary { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsSystemList { get => GetValue<bool>(); set => SetValue(value); }

        public int ReadSecurity { get => GetValue<int>(); set => SetValue(value); }

        public int WriteSecurity { get => GetValue<int>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        // Internal property, not visible to the library users
        //[GraphProperty("name", UseCustomMapping = false)]
        public string NameToConstructEntityType { get => GetValue<string>(); set => SetValue(value); }

        public string ListItemEntityTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public int ItemCount { get => GetValue<int>(); set => SetValue(value); }

        public DateTime Created { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemDeletedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public DateTime LastItemUserModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public IFolder RootFolder { get => GetModelValue<IFolder>(); }

        public IInformationRightsManagementSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementSettings>(); }

        // BERT/PAOLO: not possible at this moment after refactoring, something to reassess later on
        //[GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items { get => GetModelCollectionValue<IListItemCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IViewCollection Views { get => GetModelCollectionValue<IViewCollection>(); }

        [SharePointProperty("subscriptions")]
        public IListSubscriptionCollection Webhooks { get => GetModelCollectionValue<IListSubscriptionCollection>(); }

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }

        public IEventReceiverDefinitionCollection EventReceivers { get => GetModelCollectionValue<IEventReceiverDefinitionCollection>(); }

        public IUserCustomActionCollection UserCustomActions { get => GetModelCollectionValue<IUserCustomActionCollection>(); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

        [SharePointProperty("*")]
        public object All { get => null; }

        #endregion

        #region Methods
        #region Graph/Rest interoperability overrides
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal async override Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Logic to set the EntityType metadata property
            if (HasValue(nameof(NameToConstructEntityType)) && IsPropertyAvailable(p => p.TemplateType) && !Metadata.ContainsKey(PnPConstants.MetaDataRestEntityTypeName))
            {
                if (!string.IsNullOrEmpty(NameToConstructEntityType))
                {
                    Metadata.Add(PnPConstants.MetaDataRestEntityTypeName, ListMetaDataMapper.MicrosoftGraphNameToRestEntityTypeName(NameToConstructEntityType, TemplateType));
                }
            }
        }

        // Sample override
        //internal override Task RestToGraphMetadataAsync()
        //{
        //    return base.RestToGraphMetadataAsync();
        //}
        #endregion

        #endregion

        #region Extension methods

        #region Recycle
        public Guid Recycle()
        {
            return RecycleAsync().GetAwaiter().GetResult();
        }

        public async Task<Guid> RecycleAsync()
        {
            ApiCall apiCall = GetRecycleApiCall();

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                return ParseRecycleResponse(response.Json);
            }

            return Guid.Empty;
        }

        private static Guid ParseRecycleResponse(string json)
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
            ApiCall apiCall = GetRecycleApiCall();
            apiCall.RawSingleResult = new BatchResultValue<Guid>(Guid.Empty);
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                (apiCall.RawSingleResult as BatchResultValue<Guid>).Value = ParseRecycleResponse(json);
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
            return new BatchSingleResult<BatchResultValue<Guid>>(batch, batchRequest.Id, apiCall.RawSingleResult as BatchResultValue<Guid>);
        }

        private ApiCall GetRecycleApiCall()
        {
            return new ApiCall($"_api/Web/Lists(guid'{Id}')/recycle", ApiType.SPORest)
            {
                RemoveFromModel = true
            };
        }
        #endregion

        #region GetItemsByCamlQuery
        public async Task LoadItemsByCamlQueryAsync(string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            await LoadItemsByCamlQueryAsync(new CamlQueryOptions() { ViewXml = query }, selectors).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQuery(string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryAsync(query, selectors).GetAwaiter().GetResult();
        }

        public async Task LoadItemsByCamlQueryAsync(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            ApiCall apiCall = await BuildGetItemsByCamlQueryApiCall(queryOptions, selectors).ConfigureAwait(false);

            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQuery(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryAsync(queryOptions, selectors).GetAwaiter().GetResult();
        }

        public async Task LoadItemsByCamlQueryBatchAsync(string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            await LoadItemsByCamlQueryBatchAsync(new CamlQueryOptions() { ViewXml = query }, selectors).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQueryBatch(string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryBatchAsync(query, selectors).GetAwaiter().GetResult();
        }

        public async Task LoadItemsByCamlQueryBatchAsync(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            ApiCall apiCall = await BuildGetItemsByCamlQueryApiCall(queryOptions, selectors).ConfigureAwait(false);

            await RequestBatchAsync(PnPContext.CurrentBatch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQueryBatch(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryBatchAsync(queryOptions, selectors).GetAwaiter().GetResult();
        }

        public async Task LoadItemsByCamlQueryBatchAsync(Batch batch, string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            await LoadItemsByCamlQueryBatchAsync(batch, new CamlQueryOptions() { ViewXml = query }, selectors).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQueryBatch(Batch batch, string query, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryBatchAsync(batch, query, selectors).GetAwaiter().GetResult();
        }

        public async Task LoadItemsByCamlQueryBatchAsync(Batch batch, CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            ApiCall apiCall = await BuildGetItemsByCamlQueryApiCall(queryOptions, selectors).ConfigureAwait(false);

            await RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void LoadItemsByCamlQueryBatch(Batch batch, CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            LoadItemsByCamlQueryBatchAsync(batch, queryOptions, selectors).GetAwaiter().GetResult();
        }

        private async Task<ApiCall> BuildGetItemsByCamlQueryApiCall(CamlQueryOptions queryOptions, params Expression<Func<IListItem, object>>[] selectors)
        {
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }

            // Build body
            ExpandoObject camlQuery;

            if (!string.IsNullOrEmpty(queryOptions.PagingInfo))
            {
                camlQuery = new
                {
                    query = new
                    {
                        __metadata = new { type = "SP.CamlQuery" },
                        queryOptions.ViewXml,
                        queryOptions.AllowIncrementalResults,
                        queryOptions.DatesInUtc,
                        queryOptions.FolderServerRelativeUrl,
                        ListItemCollectionPosition = new
                        {
                            queryOptions.PagingInfo
                        }
                    }
                }.AsExpando();
            }
            else
            {
                camlQuery = new
                {
                    query = new
                    {
                        __metadata = new { type = "SP.CamlQuery" },
                        queryOptions.ViewXml,
                        queryOptions.AllowIncrementalResults,
                        queryOptions.DatesInUtc,
                        queryOptions.FolderServerRelativeUrl,
                    }
                }.AsExpando();
            }

            var baseRequestUri = $"_api/Web/Lists(guid'{Id}')/GetItems";
            string body = JsonSerializer.Serialize(camlQuery, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

            if (EntityManager.GetEntityConcreteInstance(typeof(IListItem), this, PnPContext) is BaseDataModel<IListItem> concreteEntity)
            {
                var entityInfo = EntityManager.GetClassInfo(concreteEntity.GetType(), concreteEntity, null, selectors);
                var apiCallRequest = await QueryClient.BuildGetAPICallAsync(concreteEntity, entityInfo, new ApiCall(baseRequestUri, ApiType.SPORest), true).ConfigureAwait(false);
                baseRequestUri = RewriteGetItemsQueryString(queryOptions.ViewXml, apiCallRequest.ApiCall.Request);
            }

            return new ApiCall(baseRequestUri, ApiType.SPORest, body, "Items")
            {
                SkipCollectionClearing = true
            };
        }

        internal static string RewriteGetItemsQueryString(string viewXml, string query)
        {
            // Get the viewfields from the CAML query
            // sample: <View><ViewFields><FieldRef Name='Title' /><FieldRef Name='FileRef' /></ViewFields><RowLimit>5</RowLimit></View>

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(viewXml);
            XmlNode root = xmlDocument.DocumentElement;

            // Get FieldRef nodes
            XmlNodeList fieldRefNodes = root.SelectNodes("//View/ViewFields/FieldRef");

            List<string> fields = new List<string>();

            bool first = true;
            foreach (var fieldRefNode in fieldRefNodes)
            {
                if (fieldRefNode is XmlNode xmlNode)
                {
                    if (first)
                    {
                        fields.Add("*");
                        first = false;
                    }

                    fields.Add(xmlNode.Attributes["Name"].Value);
                }
            }

            if (fields.Count > 0)
            {
                return $"https://{UrlUtility.CombineRelativeUrlWithUrlParameters(query, $"$select={string.Join(",", fields)}")}".Replace("%2f", "/");
            }
            else
            {
                return query;
            }
        }

        #endregion

        #region GetListDataAsStream
        public async Task<Dictionary<string, object>> LoadListDataAsStreamAsync(RenderListDataOptions renderOptions)
        {
            ApiCall apiCall = BuildGetListDataAsStreamApiCall(renderOptions);

            // Since we need information about the fields in the list we're checking if the needed field data was loaded,
            // if not then the request for the fields is included in the batch, this ensures we only do a single roundtrip 
            int requestToUse = 0;
            var batch = PnPContext.BatchClient.EnsureBatch();

            if (!ArePropertiesAvailable(LoadFieldsExpression))
            {
                // Get field information via batch
                await LoadBatchAsync(batch, LoadFieldsExpression).ConfigureAwait(false);
                requestToUse++;
            }
            // GetListDataAsStream request via batch
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            // Execute the batch
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);

            // The field information request was regular request and as such the returned json was automatically handled
            // The GetListDataAsStream request is a raw request, so we'll need to process the returned json manually
            string responseJson = batch.Requests[requestToUse].ResponseJson;

            if (!string.IsNullOrEmpty(responseJson))
            {
                // Now do it again to properly transform the Values dictionary to proper objects
                return await ListDataAsStreamHandler.Deserialize(responseJson, this).ConfigureAwait(false);
            }

            return null;
        }

        public Dictionary<string, object> LoadListDataAsStream(RenderListDataOptions renderOptions)
        {
            return LoadListDataAsStreamAsync(renderOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<Dictionary<string, object>>> LoadListDataAsStreamBatchAsync(Batch batch, RenderListDataOptions renderOptions)
        {
            ApiCall apiCall = BuildGetListDataAsStreamApiCall(renderOptions);
            apiCall.RawRequest = true;
            apiCall.RawSingleResult = new Dictionary<string, object>();
            apiCall.RawResultsHandler = async (json, apiCall) =>
            {
                var result = await ListDataAsStreamHandler.Deserialize(json, this).ConfigureAwait(false);
                foreach (var item in result)
                {
                    (apiCall.RawSingleResult as Dictionary<string, object>).Add(item.Key, item.Value);
                }
            };

            if (!ArePropertiesAvailable(LoadFieldsExpression))
            {
                // Get field information via batch
                await LoadBatchAsync(batch, LoadFieldsExpression).ConfigureAwait(false);
            }
            // GetListDataAsStream request via batch
            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchSingleResult<Dictionary<string, object>>(batch, batchRequest.Id, apiCall.RawSingleResult as Dictionary<string, object>);
        }

        public IBatchSingleResult<Dictionary<string, object>> LoadListDataAsStreamBatch(Batch batch, RenderListDataOptions renderOptions)
        {
            return LoadListDataAsStreamBatchAsync(batch, renderOptions).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<Dictionary<string, object>>> LoadListDataAsStreamBatchAsync(RenderListDataOptions renderOptions)
        {
            return await LoadListDataAsStreamBatchAsync(PnPContext.CurrentBatch, renderOptions).ConfigureAwait(false);
        }

        public IBatchSingleResult<Dictionary<string, object>> LoadListDataAsStreamBatch(RenderListDataOptions renderOptions)
        {
            return LoadListDataAsStreamBatchAsync(renderOptions).GetAwaiter().GetResult();
        }

        private ApiCall BuildGetListDataAsStreamApiCall(RenderListDataOptions renderOptions)
        {
            // Build body
            var renderListDataParameters = new
            {
                parameters = new
                {
                    renderOptions.AddRequiredFields,
                    renderOptions.AllowMultipleValueFilterForTaxonomyFields,
                    renderOptions.AudienceTarget,
                    renderOptions.DatesInUtc,
                    renderOptions.DeferredRender,
                    renderOptions.ExpandGroups,
                    renderOptions.FirstGroupOnly,
                    renderOptions.FolderServerRelativeUrl,
                    renderOptions.ImageFieldsToTryRewriteToCdnUrls,
                    renderOptions.MergeDefaultView,
                    renderOptions.OriginalDate,
                    renderOptions.OverrideViewXml,
                    renderOptions.Paging,
                    renderOptions.RenderOptions,
                    renderOptions.ReplaceGroup,
                    renderOptions.ViewXml
                }
            }.AsExpando();
            string body = JsonSerializer.Serialize(renderListDataParameters, typeof(ExpandoObject), PnPConstants.JsonSerializer_IgnoreNullValues);

            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/RenderListDataAsStream", ApiType.SPORest, body)
            {
                SkipCollectionClearing = true
            };
            return apiCall;
        }
        #endregion

        #region Get and Set compliance tags

        public IComplianceTag GetComplianceTag()
        {
            return GetComplianceTagAsync().GetAwaiter().GetResult();
        }

        public async Task<IComplianceTag> GetComplianceTagAsync()
        {
            await EnsurePropertiesAsync(l => l.RootFolder).ConfigureAwait(false);
            await RootFolder.EnsurePropertiesAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);
            var listUrl = RootFolder.ServerRelativeUrl;
            ApiCall apiCall = BuildGetComplianceTagApiCall(listUrl);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var parsedJson = JsonSerializer.Deserialize<JsonElement>(response.Json, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                if (parsedJson.TryGetProperty("odata.null", out JsonElement odataNull))
                {
                    if (odataNull.GetBoolean())
                    {
                        return null;
                    }
                }

                return JsonSerializer.Deserialize<ComplianceTag>(response.Json, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
            }

            return null;
        }

        private static ApiCall BuildGetComplianceTagApiCall(string listUrl)
        {
            return new ApiCall($"_api/SP.CompliancePolicy.SPPolicyStoreProxy.GetListComplianceTag(listUrl='{listUrl}')", ApiType.SPORest);
        }

        public async Task<IBatchSingleResult<IComplianceTag>> GetComplianceTagBatchAsync(Batch batch)
        {
            await EnsurePropertiesAsync(l => l.RootFolder).ConfigureAwait(false);
            await RootFolder.EnsurePropertiesAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);
            var listUrl = RootFolder.ServerRelativeUrl;

            ApiCall apiCall = BuildGetComplianceTagApiCall(listUrl);
            apiCall.RawRequest = true;
            apiCall.RawSingleResult = new ComplianceTag();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var tag = JsonSerializer.Deserialize<ComplianceTag>(json, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                (apiCall.RawSingleResult as ComplianceTag).TagId = tag.TagId;
                (apiCall.RawSingleResult as ComplianceTag).TagName = tag.TagName;
                (apiCall.RawSingleResult as ComplianceTag).TagRetentionBasedOn = tag.TagRetentionBasedOn;
                (apiCall.RawSingleResult as ComplianceTag).TagDuration = tag.TagDuration;
                (apiCall.RawSingleResult as ComplianceTag).SuperLock = tag.SuperLock;
                (apiCall.RawSingleResult as ComplianceTag).SharingCapabilities = tag.SharingCapabilities;
                (apiCall.RawSingleResult as ComplianceTag).ReviewerEmail = tag.ReviewerEmail;
                (apiCall.RawSingleResult as ComplianceTag).RequireSenderAuthenticationEnabled = tag.RequireSenderAuthenticationEnabled;
                (apiCall.RawSingleResult as ComplianceTag).Notes = tag.Notes;
                (apiCall.RawSingleResult as ComplianceTag).IsEventTag = tag.IsEventTag;
                (apiCall.RawSingleResult as ComplianceTag).HasRetentionAction = tag.HasRetentionAction;
                (apiCall.RawSingleResult as ComplianceTag).EncryptionRMSTemplateId = tag.EncryptionRMSTemplateId;
                (apiCall.RawSingleResult as ComplianceTag).DisplayName = tag.DisplayName;
                (apiCall.RawSingleResult as ComplianceTag).ContainsSiteLabel = tag.ContainsSiteLabel;
                (apiCall.RawSingleResult as ComplianceTag).BlockEdit = tag.BlockEdit;
                (apiCall.RawSingleResult as ComplianceTag).BlockDelete = tag.BlockDelete;
                (apiCall.RawSingleResult as ComplianceTag).AutoDelete = tag.AutoDelete;
                (apiCall.RawSingleResult as ComplianceTag).AllowAccessFromUnmanagedDevice = tag.AllowAccessFromUnmanagedDevice;
                (apiCall.RawSingleResult as ComplianceTag).AccessType = tag.AccessType;
                (apiCall.RawSingleResult as ComplianceTag).AcceptMessagesOnlyFromSendersOrMembers = tag.AcceptMessagesOnlyFromSendersOrMembers;
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Get).ConfigureAwait(false);

            return new BatchSingleResult<ComplianceTag>(batch, batchRequest.Id, apiCall.RawSingleResult as ComplianceTag);
        }

        public IBatchSingleResult<IComplianceTag> GetComplianceTagBatch(Batch batch)
        {
            return GetComplianceTagBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<IComplianceTag>> GetComplianceTagBatchAsync()
        {
            return await GetComplianceTagBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IBatchSingleResult<IComplianceTag> GetComplianceTagBatch()
        {
            return GetComplianceTagBatchAsync().GetAwaiter().GetResult();
        }

        public void SetComplianceTag(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            SetComplianceTagAsync(complianceTagValue, blockDelete, blockEdit, syncToItems).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagAsync(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            ApiCall apiCall = await SetComplianceTagApiCall(complianceTagValue, blockDelete, blockEdit, syncToItems).ConfigureAwait(false);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            SetComplianceTagBatchAsync(complianceTagValue, blockDelete, blockEdit, syncToItems).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            await SetComplianceTagBatchAsync(PnPContext.CurrentBatch, complianceTagValue, blockDelete, blockEdit, syncToItems).ConfigureAwait(false);
        }

        public void SetComplianceTagBatch(Batch batch, string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            SetComplianceTagBatchAsync(batch, complianceTagValue, blockDelete, blockEdit, syncToItems).GetAwaiter().GetResult();
        }

        public async Task SetComplianceTagBatchAsync(Batch batch, string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            ApiCall apiCall = await SetComplianceTagApiCall(complianceTagValue, blockDelete, blockEdit, syncToItems).ConfigureAwait(false);
            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private async Task<ApiCall> SetComplianceTagApiCall(string complianceTagValue, bool blockDelete, bool blockEdit, bool syncToItems)
        {
            await EnsurePropertiesAsync(l => l.RootFolder).ConfigureAwait(false);
            await RootFolder.EnsurePropertiesAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);
            var listUrl = RootFolder.ServerRelativeUrl;
            var parameters = new
            {
                listUrl,
                complianceTagValue,
                blockDelete,
                blockEdit,
                syncToItems
            };
            string body = JsonSerializer.Serialize(parameters);
            var apiCall = new ApiCall($"_api/SP.CompliancePolicy.SPPolicyStoreProxy.SetListComplianceTag", ApiType.SPORest, body);
            return apiCall;
        }

        #endregion

        #region Security

        public void BreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            BreakRoleInheritanceAsync(copyRoleAssignments, clearSubscopes).GetAwaiter().GetResult();
        }

        public async Task BreakRoleInheritanceAsync(bool copyRoleAssignments, bool clearSubscopes)
        {
            ApiCall apiCall = BuildBreakRoleInheritanceApiCall(copyRoleAssignments, clearSubscopes);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall BuildBreakRoleInheritanceApiCall(bool copyRoleAssignments, bool clearSubscopes)
        {
            return new ApiCall($"_api/Web/Lists(guid'{Id}')/breakroleinheritance(copyRoleAssignments={copyRoleAssignments.ToString().ToLower()},clearSubscopes={clearSubscopes.ToString().ToLower()})", ApiType.SPORest);
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

        public void ResetRoleInheritance()
        {
            ResetRoleInheritanceAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceAsync()
        {
            ApiCall apiCall = BuildResetRoleInheritanceApiCall();
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        private ApiCall BuildResetRoleInheritanceApiCall()
        {
            return new ApiCall($"_api/Web/Lists(guid'{Id}')/resetroleinheritance", ApiType.SPORest);
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
            return new ApiCall($"_api/web/lists(guid'{Id}')/roleassignments/addroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
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
            return new ApiCall($"_api/web/lists(guid'{Id}')/roleassignments/removeroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
        }
        #endregion

        #region Folders

        public IListItem AddListFolder(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return AddListFolderAsync(path, parentFolder, contentTypeId).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddListFolderAsync(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return await Items.AddAsync(BuildAddListFolderPayLoad(path, contentTypeId), parentFolder, FileSystemObjectType.Folder).ConfigureAwait(false);
        }

        private static Dictionary<string, object> BuildAddListFolderPayLoad(string path, string contentTypeId)
        {
            return new Dictionary<string, object>
            {
                {
                    "Title",path
                },
                {
                    "FileLeafRef", path
                },
                {
                    "ContentTypeId", contentTypeId
                }
            };
        }

        public async Task<IListItem> AddListFolderBatchAsync(Batch batch, string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return await Items.AddBatchAsync(batch, BuildAddListFolderPayLoad(path, contentTypeId), parentFolder, FileSystemObjectType.Folder).ConfigureAwait(false);
        }

        public IListItem AddListFolderBatch(Batch batch, string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return AddListFolderBatchAsync(batch, path, parentFolder, contentTypeId).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddListFolderBatchAsync(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return await Items.AddBatchAsync(PnPContext.CurrentBatch, BuildAddListFolderPayLoad(path, contentTypeId), parentFolder, FileSystemObjectType.Folder).ConfigureAwait(false);
        }

        public IListItem AddListFolderBatch(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return AddListFolderBatchAsync(path, parentFolder, contentTypeId).GetAwaiter().GetResult();
        }
        #endregion

        #region Syntex
        public async Task<List<ISyntexClassifyAndExtractResult>> ClassifyAndExtractAsync(bool force = false, int pageSize = 500)
        {
            // load required fields in this list
            string viewXml = @"<View Scope='Recursive'>
                    <ViewFields>
                      <FieldRef Name='FileDirRef' />
                      <FieldRef Name='FileLeafRef' />
                      <FieldRef Name='UniqueId' />
                      <FieldRef Name='PrimeLastClassified' />
                    </ViewFields>   
                    <OrderBy Override='TRUE'><FieldRef Name= 'ID' Ascending= 'FALSE' /></OrderBy>
                    <RowLimit Paged='TRUE'>{0}</RowLimit>
                   </View>";

            bool paging = true;
            string nextPageInfo = null;

            while (paging)
            {
                var queryOptions = new RenderListDataOptions()
                {
                    ViewXml = string.Format(viewXml, pageSize),
                    RenderOptions = RenderListDataOptionsFlags.ListData
                };

                if (nextPageInfo != null)
                {
                    queryOptions.Paging = nextPageInfo;
                }

                // Execute the query for the requested page
                var output = await LoadListDataAsStreamAsync(queryOptions).ConfigureAwait(false);

                if (output.ContainsKey("NextHref"))
                {
                    nextPageInfo = output["NextHref"].ToString()?.Substring(1);
                    if (string.IsNullOrEmpty(nextPageInfo))
                    {
                        paging = false;
                    }
                }
                else
                {
                    paging = false;
                }
            }

            // Iterate over the results and request Syntex classification and extraction
            List<ISyntexClassifyAndExtractResult> classifyResults = new List<ISyntexClassifyAndExtractResult>();
            List<BatchSingleResult<ISyntexClassifyAndExtractResult>> classifyBatchRequests = new List<BatchSingleResult<ISyntexClassifyAndExtractResult>>();
            var batch = PnPContext.NewBatch();
            foreach (var file in Items.AsRequested())
            {
                // We use client side filtering to prevent re-processing already processed files, doing 
                // this filter via the CAML query will triggers a list scan and as such results in throttling/large list issues
                if (!force && file.Values["PrimeLastClassified"] != null)
                {
                    continue;
                }

                var apiCall = SyntexClassifyAndExtract.CreateClassifyAndExtractApiCall(PnPContext, Guid.Parse(file.Values["UniqueId"].ToString()), false);
                apiCall.RawSingleResult = new SyntexClassifyAndExtractResult();
                apiCall.RawResultsHandler = (json, call) =>
                {
                    var result = SyntexClassifyAndExtract.ProcessClassifyAndExtractResponse(json);
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).Created = result.Created;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).DeliverDate = result.DeliverDate;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).ErrorMessage = result.ErrorMessage;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).Id = result.Id;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).Status = result.Status;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).StatusCode = result.StatusCode;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).TargetServerRelativeUrl = result.TargetServerRelativeUrl;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).TargetSiteUrl = result.TargetSiteUrl;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).TargetWebServerRelativeUrl = result.TargetWebServerRelativeUrl;
                    (call.RawSingleResult as SyntexClassifyAndExtractResult).WorkItemType = result.WorkItemType;
                };

                var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
                classifyBatchRequests.Add(new BatchSingleResult<ISyntexClassifyAndExtractResult>(batch, batchRequest.Id, apiCall.RawSingleResult as ISyntexClassifyAndExtractResult));
            }

            // Execute the batch
            await PnPContext.ExecuteAsync(batch, false).ConfigureAwait(false);

            // Translate the batch results to the needed set of results
            foreach (var classifyBatchRequest in classifyBatchRequests)
            {
                classifyResults.Add(classifyBatchRequest.Result);
            }

            return classifyResults;
        }

        public List<ISyntexClassifyAndExtractResult> ClassifyAndExtract(bool force = false, int pageSize = 500)
        {
            return ClassifyAndExtractAsync(force, pageSize).GetAwaiter().GetResult();
        }

        public async Task<ISyntexClassifyAndExtractResult> ClassifyAndExtractOffPeakAsync()
        {
            // Ensure we do have the list's root folder id 
            await EnsurePropertiesAsync(p => p.RootFolder).ConfigureAwait(false);
            // Build the API call
            var apiCall = SyntexClassifyAndExtract.CreateClassifyAndExtractApiCall(PnPContext, RootFolder.UniqueId, true);
            // Send the call to server
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
            // Parse the response json
            return SyntexClassifyAndExtract.ProcessClassifyAndExtractResponse(response.Json);
        }

        public ISyntexClassifyAndExtractResult ClassifyAndExtractOffPeak()
        {
            return ClassifyAndExtractOffPeakAsync().GetAwaiter().GetResult();
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

        #region Flow
        public async Task<List<IFlowInstance>> GetFlowInstancesAsync()
        {
            ApiCall apiCall = BuildGetFlowInstancesApiCall();
            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            return ProcessFlowInstances(response.Json);
        }

        private List<IFlowInstance> ProcessFlowInstances(string json)
        {
            List<IFlowInstance> flowInstances = new List<IFlowInstance>();

            if (!string.IsNullOrEmpty(json))
            {
                var parsedJson = JsonSerializer.Deserialize<JsonElement>(json, PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                if (parsedJson.TryGetProperty("SynchronizationData", out JsonElement synchronizationData))
                {
                    if (synchronizationData.ValueKind != JsonValueKind.Null)
                    {
                        var cleanedSynchronizationData = synchronizationData.GetString().Replace("\\\"", "\"");
                        var parsedFlowInstances = JsonSerializer.Deserialize<JsonElement>(cleanedSynchronizationData);
                        if (parsedFlowInstances.TryGetProperty("value", out JsonElement flowInstanceDefinitions) && flowInstanceDefinitions.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var flowInstanceDefinition in flowInstanceDefinitions.EnumerateArray())
                            {
                                FlowInstance flowInstance = new FlowInstance
                                {
                                    Id = flowInstanceDefinition.GetProperty("id").GetString(),
                                    DisplayName = flowInstanceDefinition.GetProperty("properties").GetProperty("displayName").GetString(),
                                    Definition = flowInstanceDefinition.ToString(),
                                };

                                flowInstances.Add(flowInstance);
                            }
                        }
                    }
                }
            }

            return flowInstances;
        }

        public List<IFlowInstance> GetFlowInstances()
        {
            return GetFlowInstancesAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IFlowInstance>> GetFlowInstancesBatchAsync(Batch batch)
        {
            ApiCall apiCall = BuildGetFlowInstancesApiCall();
            apiCall.RawEnumerableResult = new List<IFlowInstance>();
            apiCall.RawResultsHandler = (json, apiCall) =>
            {
                var batchFirstRequest = batch.Requests.First().Value;
                ApiCallResponse response = new ApiCallResponse(apiCall, json, System.Net.HttpStatusCode.OK, batchFirstRequest.Id, batchFirstRequest.ResponseHeaders);
                ((List<IFlowInstance>)apiCall.RawEnumerableResult).AddRange(ProcessFlowInstances(response.Json).ToList());
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);

            return new BatchEnumerableBatchResult<IFlowInstance>(batch, batchRequest.Id, (IReadOnlyList<IFlowInstance>)apiCall.RawEnumerableResult);

        }

        public IEnumerableBatchResult<IFlowInstance> GetFlowInstancesBatch(Batch batch)
        {
            return GetFlowInstancesBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task<IEnumerableBatchResult<IFlowInstance>> GetFlowInstancesBatchAsync()
        {
            return await GetFlowInstancesBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public IEnumerableBatchResult<IFlowInstance> GetFlowInstancesBatch()
        {
            return GetFlowInstancesBatchAsync().GetAwaiter().GetResult();
        }

        private ApiCall BuildGetFlowInstancesApiCall()
        {
            return new ApiCall($"_api/web/lists(guid'{Id}')/syncflowinstances", ApiType.SPORest);
        }
        #endregion

        #region Content type ordering
        public async Task ReorderContentTypesAsync(List<string> contentTypeIdList)
        {
            if (contentTypeIdList == null)
            {
                throw new ArgumentNullException(nameof(contentTypeIdList));
            }

            await LoadAsync(p => p.ContentTypesEnabled,
                            p => p.RootFolder.QueryProperties(p => p.ContentTypeOrder, p => p.UniqueContentTypeOrder)).ConfigureAwait(false);

            if (ContentTypesEnabled && contentTypeIdList.Count > 0)
            {
                // Create the update body 
                List<ExpandoObject> contentTypeIds = new List<ExpandoObject>();
                foreach (var contentTypeId in contentTypeIdList)
                {
                    var contentTypeIdToAdd = new
                    {
                        __metadata = new { type = "SP.ContentTypeId" },
                        StringValue = contentTypeId
                    }.AsExpando();

                    contentTypeIds.Add(contentTypeIdToAdd);
                }

                var body = new
                {
                    __metadata = new { type = "SP.Folder" },
                    UniqueContentTypeOrder = new
                    {
                        __metadata = new { type = "Collection(SP.ContentTypeId)" },
                        results = contentTypeIds
                    }
                };

                // Patch the folder
                var apiCall = new ApiCall($"_api/web/lists(guid'{Id}')/rootfolder", ApiType.SPORest, JsonSerializer.Serialize(body));

                await (RootFolder as Folder).RequestAsync(apiCall, new HttpMethod("PATCH")).ConfigureAwait(false);

            }
        }

        public void ReorderContentTypes(List<string> contentTypeIdList)
        {
            ReorderContentTypesAsync(contentTypeIdList).GetAwaiter().GetResult();
        }

        public async Task<List<string>> GetContentTypeOrderAsync()
        {
            await LoadAsync(p => p.ContentTypesEnabled,
                            p => p.RootFolder.QueryProperties(p => p.ContentTypeOrder, p => p.UniqueContentTypeOrder)).ConfigureAwait(false);

            if (!ContentTypesEnabled)
            {
                return null;
            }
            else
            {
                List<string> contentTypeOrder = new List<string>();

                IContentTypeIdCollection contentTypeIdCollection;

                // If a custom order was set, then UniqueContentTypeOrder
                if (RootFolder.IsPropertyAvailable(p => p.UniqueContentTypeOrder) && RootFolder.UniqueContentTypeOrder.Length > 0)
                {
                    contentTypeIdCollection = RootFolder.UniqueContentTypeOrder;
                }
                else
                {
                    contentTypeIdCollection = RootFolder.ContentTypeOrder;
                }

                foreach (var contentTypeId in contentTypeIdCollection.AsRequested())
                {
                    contentTypeOrder.Add(contentTypeId.StringValue);
                }

                return contentTypeOrder;
            }
        }

        public List<string> GetContentTypeOrder()
        {
            return GetContentTypeOrderAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Files

        public List<IFile> FindFiles(string match)
        {
            return FindFilesAsync(match).GetAwaiter().GetResult();
        }

        public async Task<List<IFile>> FindFilesAsync(string match)
        {
            return await RootFolder.FindFilesAsync(match).ConfigureAwait(false);
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

            var apiCall = BuildGetUserEffectivePermissionsApiCall(userPrincipalName);

            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (string.IsNullOrEmpty(response.Json))
            {
                throw new Exception(PnPCoreResources.Exception_EffectivePermissionsNotFound);
            }

            return EffectivePermissionsHandler.ParseGetUserEffectivePermissionsResponse(response.Json);
        }

        private ApiCall BuildGetUserEffectivePermissionsApiCall(string userPrincipalName)
        {
            return new ApiCall($"_api/web/lists(guid'{Id}')/getusereffectivepermissions('{HttpUtility.UrlEncode("i:0#.f|membership|")}{userPrincipalName}')", ApiType.SPORest);
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

        #region Default column values

        private const string defaultValuesFileName = "client_LocationBasedDefaults.html";

        public async Task<List<DefaultColumnValueOptions>> GetDefaultColumnValuesAsync()
        {
            List<DefaultColumnValueOptions> results = new List<DefaultColumnValueOptions>();

            var listInfo = await GetAsync(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl)).ConfigureAwait(false);

            string defaultValuesFile = $"{listInfo.RootFolder.ServerRelativeUrl}/Forms/{defaultValuesFileName}";

            try
            {
                var defaultValueFile = await PnPContext.Web.GetFileByServerRelativeUrlAsync(defaultValuesFile).ConfigureAwait(false);

                // Download the file contents
                var fileContentsStream = await defaultValueFile.GetContentAsync().ConfigureAwait(false);

                // Parse the file contents
                XDocument document = XDocument.Load(fileContentsStream);
                var values = from a in document.Descendants("a") select a;

                foreach (var value in values)
                {
                    var href = value.Attribute("href").Value;
                    href = Uri.UnescapeDataString(href);
                    href = href.Replace(listInfo.RootFolder.ServerRelativeUrl, "/").Replace("//", "/");

                    var defaultValues = from d in value.Descendants("DefaultValue") select d;
                    foreach (var defaultValue in defaultValues)
                    {
                        var fieldName = defaultValue.Attribute("FieldName").Value;
                        var textValue = defaultValue.Value;

                        results.Add(new DefaultColumnValueOptions
                        {
                            DefaultValue = textValue,
                            FieldInternalName = fieldName,
                            FolderRelativePath = href
                        });

                    }
                }
            }
            catch (SharePointRestServiceException ex)
            {
                var error = ex.Error as SharePointRestError;

                if (File.ErrorIndicatesFileDoesNotExists(error))
                {
                    PnPContext.Logger.LogInformation($"Provided {defaultValuesFileName} file does not exist...no default values are set");
                    return results;
                }
                else
                {
                    throw;
                }
            }

            return results;
        }

        public async Task SetDefaultColumnValuesAsync(List<DefaultColumnValueOptions> defaultColumnValues)
        {

            if (defaultColumnValues == null || !defaultColumnValues.Any())
            {
                // Setting nothing means clearing the existing values
                await ClearDefaultColumnValuesAsync().ConfigureAwait(false);
                return;
            }

            var listInfo = await GetAsync(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl), p => p.EventReceivers).ConfigureAwait(false);

            // Prep the defaults file
            var xMetadataDefaults = new XElement("MetadataDefaults");
            var values = defaultColumnValues.ToList();

            while (values.Any())
            {
                // Get the first entry 
                DefaultColumnValueOptions defaultColumnValue = values.First();
                var path = defaultColumnValue.FolderRelativePath;
                if (string.IsNullOrEmpty(path))
                {
                    // Assume root folder
                    path = "/";
                }
                path = path.Equals("/") ? listInfo.RootFolder.ServerRelativeUrl : UrlUtility.CombineAsString(listInfo.RootFolder.ServerRelativeUrl, path);

                // Find all in the same path:
                var defaultColumnValuesInSamePath = defaultColumnValues.Where(x => x.FolderRelativePath == defaultColumnValue.FolderRelativePath);

                // Only URL encode the spaces. Other special characters should stay as they are or it will not work.
                path = path.Replace(" ", "%20");

                var xATag = new XElement("a", new XAttribute("href", path));

                foreach (var defaultColumnValueInSamePath in defaultColumnValuesInSamePath)
                {
                    var fieldName = defaultColumnValueInSamePath.FieldInternalName;
                    var xDefaultValue = new XElement("DefaultValue", new XAttribute("FieldName", fieldName));
                    xDefaultValue.SetValue(defaultColumnValueInSamePath.DefaultValue);
                    xATag.Add(xDefaultValue);

                    values.Remove(defaultColumnValueInSamePath);
                }
                xMetadataDefaults.Add(xATag);
            }

            var xmlSb = new StringBuilder();
            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                NewLineHandling = NewLineHandling.None,
                Indent = false
            };

            using (var xmlWriter = XmlWriter.Create(xmlSb, xmlSettings))
            {
                xMetadataDefaults.Save(xmlWriter);
            }

            // Get the folder to add the defaults file to
            var formsFolder = await PnPContext.Web.GetFolderByServerRelativeUrlAsync($"{listInfo.RootFolder.ServerRelativeUrl}/Forms", p => p.Files).ConfigureAwait(false);

            // Add defaults file
            using (var stream = GenerateStreamFromString(xmlSb.ToString()))
            {
                await formsFolder.Files.AddAsync(defaultValuesFileName, stream, true).ConfigureAwait(false);
            }

            // Verify the needed event receiver has been configured
            var locationBasedMetadataDefaultsReceiver = listInfo.EventReceivers.AsRequested()
                                                                      .FirstOrDefault(p => p.ReceiverName == "LocationBasedMetadataDefaultsReceiver ItemAdded" && 
                                                                                      p.EventType == EventReceiverType.ItemAdded);
            if (locationBasedMetadataDefaultsReceiver == null)
            {
                await listInfo.EventReceivers.AddAsync(new EventReceiverOptions
                {
                    EventType = EventReceiverType.ItemAdded,
                    Synchronization = EventReceiverSynchronization.Synchronous,
                    SequenceNumber = 1000,
                    ReceiverName = "LocationBasedMetadataDefaultsReceiver ItemAdded",
                    ReceiverAssembly = "Microsoft.Office.DocumentManagement, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c",
                    ReceiverClass = "Microsoft.Office.DocumentManagement.LocationBasedMetadataDefaultsReceiver"
                }).ConfigureAwait(false);
            }

        }

        public void SetDefaultColumnValues(List<DefaultColumnValueOptions> defaultColumnValues)
        {
            SetDefaultColumnValuesAsync(defaultColumnValues).GetAwaiter().GetResult();
        }

        public List<DefaultColumnValueOptions> GetDefaultColumnValues()
        {
            return GetDefaultColumnValuesAsync().GetAwaiter().GetResult();
        }

        public async Task ClearDefaultColumnValuesAsync()
        {
            var listInfo = await GetAsync(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl)).ConfigureAwait(false);

            string defaultValuesFile = $"{listInfo.RootFolder.ServerRelativeUrl}/Forms/{defaultValuesFileName}";

            try
            {
                // Get the default values file
                var defaultValueFile = await PnPContext.Web.GetFileByServerRelativeUrlAsync(defaultValuesFile).ConfigureAwait(false);

                // Reset file contents
                await defaultValueFile.DeleteAsync().ConfigureAwait(false);
            }
            catch (SharePointRestServiceException ex)
            {
                var error = ex.Error as SharePointRestError;

                if (File.ErrorIndicatesFileDoesNotExists(error))
                {
                    PnPContext.Logger.LogInformation($"Provided {defaultValuesFileName} file does not exist...no default values to reset");
                }
                else
                {
                    throw;
                }
            }
        }

        public void ClearDefaultColumnValues()
        {
            ClearDefaultColumnValuesAsync().GetAwaiter().GetResult();
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        #endregion

        #region Reindex list
        public async Task ReIndexAsync()
        {
            var listInfo = await GetAsync(p => p.Title, p => p.NoCrawl, p => p.RootFolder).ConfigureAwait(false);

            if (listInfo.NoCrawl)
            {
                // bail out
                PnPContext.Logger.LogInformation($"List {listInfo.Title} is configured as NoCrawl, reindex request will be skipped.");
                return;
            }

            // Load the properties
            await listInfo.RootFolder.EnsurePropertiesAsync(p => p.Properties).ConfigureAwait(false);

            const string reIndexKey = "vti_searchversion";
            int searchVersion = 0;

            if (listInfo.RootFolder.Properties.Values.ContainsKey(reIndexKey))
            {
                searchVersion = listInfo.RootFolder.Properties.GetInteger(reIndexKey, 0);
            }

            listInfo.RootFolder.Properties.Values[reIndexKey] = searchVersion + 1;

            await listInfo.RootFolder.Properties.UpdateAsync().ConfigureAwait(false);
        }

        public void ReIndex()
        {
            ReIndexAsync().GetAwaiter().GetResult();
        }
        #endregion

        #region Enable Audience targeting

        private const string modernAudienceTargetingInternalName = "_ModernAudienceTargetUserField";
        private const string modernAudienceTargetingMultiLookupInternalName = "_ModernAudienceAadObjectIds";

        public async Task EnableAudienceTargetingAsync()
        {
            // Load the needed data using a single roundtrip
            var batch = PnPContext.NewBatch();
            var listInfo = await GetBatchAsync(batch,
                                              p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl), 
                                              p => p.EventReceivers,                                               
                                              p => p.ContentTypesEnabled,
                                              p => p.Fields.QueryProperties(p => p.InternalName)).ConfigureAwait(false);
            var userInfoList = await PnPContext.Web.Lists.GetByTitleBatchAsync(batch, "User Information List").ConfigureAwait(false);
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);


            batch = PnPContext.NewBatch();
            // Verify if the needed fields exist
            var addOptions = listInfo.Result.ContentTypesEnabled
                                ? AddFieldOptionsFlags.AddFieldInternalNameHint | AddFieldOptionsFlags.AddToNoContentType
                                : AddFieldOptionsFlags.AddFieldInternalNameHint | AddFieldOptionsFlags.AddToDefaultContentType;
            string sourceId = listInfo.Result.Id.ToString("B");

            var firstModernTargetingFieldXml = @$"
                    <Field ID=""{{7f759147-c861-4cd6-a11f-5aa3037d9634}}"" Type=""UserMulti"" List=""UserInfo"" Name=""_ModernAudienceTargetUserField"" 
                        StaticName=""_ModernAudienceTargetUserField"" DisplayName=""Audience"" Required=""FALSE"" 
                        SourceID=""{sourceId}"" ColName=""int2"" RowOrdinal=""0"" ShowField=""ImnName"" ShowInDisplayForm=""TRUE"" ShowInListSettings=""FALSE"" UserSelectionMode=""GroupsOnly"" 
                        UserSelectionScope=""0"" Mult=""TRUE"" Sortable=""FALSE"" Version=""1""/>";

            var secondModernTargetingFieldFormat = @$"
                    <Field Type=""LookupMulti"" DisplayName=""AudienceIds"" 
                        List=""{userInfoList.Id:B}"" WebId=""{PnPContext.Web.Id}"" FieldRef=""7f759147-c861-4cd6-a11f-5aa3037d9634"" ReadOnly=""TRUE"" Mult=""TRUE"" Sortable=""FALSE"" 
                        UnlimitedLengthInDocumentLibrary=""FALSE"" ID=""{Guid.NewGuid():B}"" SourceID=""{sourceId}"" StaticName=""_ModernAudienceAadObjectIds"" 
                        Name=""_ModernAudienceAadObjectIds"" ShowField=""_AadObjectIdForUser"" ShowInListSettings=""FALSE"" Version=""1""/>";

            bool addFirstModernTargetingField = listInfo.Result.Fields.AsRequested().FirstOrDefault(p => p.InternalName == modernAudienceTargetingInternalName) == null;
            bool addSecondModernTargetingField = listInfo.Result.Fields.AsRequested().FirstOrDefault(p => p.InternalName == modernAudienceTargetingMultiLookupInternalName) == null;

            if (addFirstModernTargetingField)
            {
                await listInfo.Result.Fields.AddFieldAsXmlBatchAsync(batch, firstModernTargetingFieldXml, false, addOptions).ConfigureAwait(false);
            }

            if (addSecondModernTargetingField)
            {
                await listInfo.Result.Fields.AddFieldAsXmlBatchAsync(batch, secondModernTargetingFieldFormat, false, addOptions).ConfigureAwait(false);
            }

            // Verify the needed event receivers have been configured
            bool addItemAddingAudienceEventRecevier = listInfo.Result.EventReceivers.AsRequested()
                                                        .FirstOrDefault(p => p.ReceiverClass == "Microsoft.SharePoint.Portal.AudienceEventRecevier" && 
                                                                        p.EventType == EventReceiverType.ItemAdding) == null;
            bool addItemupdatingAudienceEventRecevier = listInfo.Result.EventReceivers.AsRequested()
                                                        .FirstOrDefault(p => p.ReceiverClass == "Microsoft.SharePoint.Portal.AudienceEventRecevier" && 
                                                                        p.EventType == EventReceiverType.ItemUpdating) == null;
            if (addItemAddingAudienceEventRecevier)
            {
                await AddAudienceEventReceiverAsync(batch, listInfo.Result, EventReceiverType.ItemAdding).ConfigureAwait(false);
            }

            if (addItemupdatingAudienceEventRecevier)
            {
                await AddAudienceEventReceiverAsync(batch, listInfo.Result, EventReceiverType.ItemUpdating).ConfigureAwait(false);
            }

            // Commit the needed changes
            await PnPContext.ExecuteAsync(batch).ConfigureAwait(false);
        }

        public void EnableAudienceTargeting()
        {
            EnableAudienceTargetingAsync().GetAwaiter().GetResult();
        }

        private static async Task AddAudienceEventReceiverAsync(Batch batch, IList listInfo, EventReceiverType eventReceiverType)
        {
            await listInfo.EventReceivers.AddBatchAsync(batch,
                new EventReceiverOptions
                {
                    EventType = eventReceiverType,
                    Synchronization = EventReceiverSynchronization.Synchronous,
                    SequenceNumber = 10000,
                    ReceiverName = "",
                    ReceiverAssembly = "Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c",
                    ReceiverClass = "Microsoft.SharePoint.Portal.AudienceEventRecevier"
                }).ConfigureAwait(false);
        }

        #endregion

        #endregion
    }
}
