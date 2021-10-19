using PnP.Core.Model.Security;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List class, write your custom code here
    /// </summary>
    [SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
    //[GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}", LinqGet = "sites/{Parent.GraphId}/lists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class List : BaseDataModel<IList>, IList
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

        public IFolder RootFolder { get => GetModelValue<IFolder>(); }

        public IInformationRightsManagementSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementSettings>(); }

        // BERT/PAOLO: not possible at this moment after refactoring, something to reassess later on
        //[GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items { get => GetModelCollectionValue<IListItemCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IViewCollection Views { get => GetModelCollectionValue<IViewCollection>(); }

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }

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

        #region BatchGetByTitle
        private static ApiCall GetByTitleApiCall(string title)
        {
            return new ApiCall($"_api/web/lists/getbytitle('{title}')", ApiType.SPORest);
        }

        internal Task<IBatchSingleResult<IList>> BatchGetByTitleAsync(Batch batch, string title, params Expression<Func<IList, object>>[] expressions)
        {
            return GetBatchAsync(batch, GetByTitleApiCall(title), expressions);
        }
        #endregion

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
                baseRequestUri = apiCallRequest.ApiCall.Request;
            }

            return new ApiCall(baseRequestUri, ApiType.SPORest, body, "Items")
            {
                SkipCollectionClearing = true
            };
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

        private ApiCall BuildGetListDataAsStreamApiCall(RenderListDataOptions renderOptions)
        {
            // Build body
            var renderListDataParameters = new
            {
                parameters = new
                {
                    __metadata = new { type = "SP.RenderListDataParameters" },
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
            var apiCall = new ApiCall($"_api/SP.CompliancePolicy.SPPolicyStoreProxy.GetListComplianceTag(listUrl='{listUrl}')", ApiType.SPORest);
            var response = await RawRequestAsync(apiCall, HttpMethod.Get).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var json = JsonSerializer.Deserialize<JsonElement>(response.Json).GetProperty("d");

                if (json.TryGetProperty("GetListComplianceTag", out JsonElement getAvailableTagsForSite))
                {
                    var tag = getAvailableTagsForSite.ToObject<ComplianceTag>(PnPConstants.JsonSerializer_PropertyNameCaseInsensitiveTrue);
                    return tag;
                }
            }

            return null;
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
            return await Items.AddAsync(new Dictionary<string, object>
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
            }, parentFolder, FileSystemObjectType.Folder).ConfigureAwait(false);
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

        #endregion

        #endregion
    }
}
