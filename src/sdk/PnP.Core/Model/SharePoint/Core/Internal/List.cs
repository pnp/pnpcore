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
    [GraphType(Get = "sites/{Parent.GraphId}/lists/{GraphId}", LinqGet = "sites/{Parent.GraphId}/lists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2243:Attribute string literals should parse correctly", Justification = "<Pending>")]
    internal partial class List : BaseDataModel<IList>, IList
    {
        // List of fields that loaded when the Lists collection is requested. This approach is needed as 
        // Graph requires the "system" field to be loaded as trigger to return all lists 
        internal const string SystemFacet = "system";
        internal const string DefaultGraphFieldsToLoad = "system,createdDateTime,description,eTag,id,lastModifiedDateTime,name,webUrl,displayName,createdBy,lastModifiedBy,parentReference,list";
        internal static Expression<Func<IList, object>>[] LoadFieldsExpression = new Expression<Func<IList, object>>[] { p => p.Fields.QueryProperties(p => p.InternalName, p => p.FieldTypeKind, p => p.TypeAsString, p => p.Title) };

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

        [GraphProperty("displayName")]
        public string Title { get => GetValue<string>(); set => SetValue(value); }

        [GraphProperty("description")]
        public string Description { get => GetValue<string>(); set => SetValue(value); }

        [SharePointProperty("DocumentTemplateUrl")]
        public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

        public bool OnQuickLaunch { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("BaseTemplate")]
        [GraphProperty("list", JsonPath = "template")]
        public ListTemplateType TemplateType { get => GetValue<ListTemplateType>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public bool EnableVersioning { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableMinorVersions { get => GetValue<bool>(); set => SetValue(value); }

        public int DraftVersionVisibility { get => GetValue<int>(); set => SetValue(value); }

        public bool EnableModeration { get => GetValue<bool>(); set => SetValue(value); }

        [SharePointProperty("MajorWithMinorVersionsLimit")]
        public int MinorVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [SharePointProperty("MajorVersionLimit")]
        public int MaxVersionLimit { get => GetValue<int>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "contentTypesEnabled")]
        public bool ContentTypesEnabled { get => GetValue<bool>(); set => SetValue(value); }

        [GraphProperty("list", JsonPath = "hidden")]
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

        public int ReadSecurity { get => GetValue<int>(); set => SetValue(value); }

        public int WriteSecurity { get => GetValue<int>(); set => SetValue(value); }

        public string ValidationFormula { get => GetValue<string>(); set => SetValue(value); }

        public string ValidationMessage { get => GetValue<string>(); set => SetValue(value); }

        // Internal property, not visible to the library users
        [GraphProperty("name", UseCustomMapping = false)]
        public string NameToConstructEntityType { get => GetValue<string>(); set => SetValue(value); }

        public string ListItemEntityTypeFullName { get => GetValue<string>(); set => SetValue(value); }

        public IFolder RootFolder { get => GetModelValue<IFolder>(); }

        public IInformationRightsManagementSettings InformationRightsManagementSettings { get => GetModelValue<IInformationRightsManagementSettings>(); }

        // BERT/PAOLO: not possible at this moment after refactoring, somethign to reassess later on
        //[GraphProperty("items", Get = "/sites/{Web.GraphId}/lists/{GraphId}/items?expand=fields")]
        public IListItemCollection Items { get => GetModelCollectionValue<IListItemCollection>(); }

        public IContentTypeCollection ContentTypes { get => GetModelCollectionValue<IContentTypeCollection>(); }

        public IFieldCollection Fields { get => GetModelCollectionValue<IFieldCollection>(); }

        public IViewCollection Views { get => GetModelCollectionValue<IViewCollection>(); }

        public bool HasUniqueRoleAssignments { get => GetValue<bool>(); set => SetValue(value); }

        public IRoleAssignmentCollection RoleAssignments { get => GetModelCollectionValue<IRoleAssignmentCollection>(); }


        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = Guid.Parse(value.ToString()); }

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
            return GetBatchAsync(batch, apiOverride: GetByTitleApiCall(title), selectors: expressions);
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
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (document.TryGetProperty("d", out JsonElement root))
                {
                    if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
                    {
                        // return the recyclebin item id
                        return recycleBinItemId.GetGuid();
                    }
                }
            }

            return Guid.Empty;
        }

        public void RecycleBatch()
        {
            RecycleBatchAsync().GetAwaiter().GetResult();
        }

        public async Task RecycleBatchAsync()
        {
            await RecycleBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        public void RecycleBatch(Batch batch)
        {
            RecycleBatchAsync(batch).GetAwaiter().GetResult();
        }

        public async Task RecycleBatchAsync(Batch batch)
        {
            ApiCall apiCall = GetRecycleApiCall();

            await RawRequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
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
        public async Task GetItemsByCamlQueryAsync(string query)
        {
            await GetItemsByCamlQueryAsync(new CamlQueryOptions() { ViewXml = query }).ConfigureAwait(false);
        }

        public void GetItemsByCamlQuery(string query)
        {
            GetItemsByCamlQueryAsync(query).GetAwaiter().GetResult();
        }

        public async Task GetItemsByCamlQueryAsync(CamlQueryOptions queryOptions)
        {
            ApiCall apiCall = BuildGetItemsByCamlQueryApiCall(queryOptions);

            await RequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void GetItemsByCamlQuery(CamlQueryOptions queryOptions)
        {
            GetItemsByCamlQueryAsync(queryOptions).GetAwaiter().GetResult();
        }

        public async Task GetItemsByCamlQueryBatchAsync(string query)
        {
            await GetItemsByCamlQueryBatchAsync(new CamlQueryOptions() { ViewXml = query }).ConfigureAwait(false);
        }

        public void GetItemsByCamlQueryBatch(string query)
        {
            GetItemsByCamlQueryBatchAsync(query).GetAwaiter().GetResult();
        }

        public async Task GetItemsByCamlQueryBatchAsync(CamlQueryOptions queryOptions)
        {
            ApiCall apiCall = BuildGetItemsByCamlQueryApiCall(queryOptions);

            await RequestBatchAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void GetItemsByCamlQueryBatch(CamlQueryOptions queryOptions)
        {
            GetItemsByCamlQueryBatchAsync(queryOptions).GetAwaiter().GetResult();
        }

        public async Task GetItemsByCamlQueryBatchAsync(Batch batch, string query)
        {
            await GetItemsByCamlQueryBatchAsync(batch, new CamlQueryOptions() { ViewXml = query }).ConfigureAwait(false);
        }

        public void GetItemsByCamlQueryBatch(Batch batch, string query)
        {
            GetItemsByCamlQueryBatchAsync(batch, query).GetAwaiter().GetResult();
        }

        public async Task GetItemsByCamlQueryBatchAsync(Batch batch, CamlQueryOptions queryOptions)
        {
            ApiCall apiCall = BuildGetItemsByCamlQueryApiCall(queryOptions);

            await RequestBatchAsync(batch, apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void GetItemsByCamlQueryBatch(Batch batch, CamlQueryOptions queryOptions)
        {
            GetItemsByCamlQueryBatchAsync(batch, queryOptions).GetAwaiter().GetResult();
        }

        private ApiCall BuildGetItemsByCamlQueryApiCall(CamlQueryOptions queryOptions)
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
            string body = JsonSerializer.Serialize(camlQuery, typeof(ExpandoObject), new JsonSerializerOptions() { IgnoreNullValues = true });

            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/GetItems", ApiType.SPORest, body, "Items");
            return apiCall;
        }
        #endregion

        #region GetListDataAsStream
        public async Task<Dictionary<string, object>> GetListDataAsStreamAsync(RenderListDataOptions renderOptions)
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
                return await ListDataAsStreamHandler.Deserialize(responseJson, this).ConfigureAwait(false);
            }

            return null;
        }

        public Dictionary<string, object> GetListDataAsStream(RenderListDataOptions renderOptions)
        {
            return GetListDataAsStreamAsync(renderOptions).GetAwaiter().GetResult();
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
            string body = JsonSerializer.Serialize(renderListDataParameters, typeof(ExpandoObject), new JsonSerializerOptions() { IgnoreNullValues = true });

            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/RenderListDataAsStream", ApiType.SPORest, body);
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
                var json = JsonDocument.Parse(response.Json).RootElement.GetProperty("d");

                if (json.TryGetProperty("GetListComplianceTag", out JsonElement getAvailableTagsForSite))
                {
                    var tag = getAvailableTagsForSite.ToObject<ComplianceTag>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/breakroleinheritance(copyRoleAssignments={copyRoleAssignments.ToString().ToLower()},clearSubscopes={clearSubscopes.ToString().ToLower()})", ApiType.SPORest);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
        }

        public void ResetRoleInheritance()
        {
            ResetRoleInheritanceAsync().GetAwaiter().GetResult();
        }

        public async Task ResetRoleInheritanceAsync()
        {
            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/resetroleinheritance", ApiType.SPORest);
            await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
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
            var result = false;
            foreach (var name in names)
            {
                var roleDefinition = await PnPContext.Web.RoleDefinitions.FirstOrDefaultAsync(d => d.Name == name).ConfigureAwait(false);
                if (roleDefinition != null)
                {
                    var apiCall = new ApiCall($"_api/web/lists(guid'{Id}')/roleassignments/addroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
                    var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                    result = response.StatusCode == System.Net.HttpStatusCode.OK;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found.");
                }
            }
            return result;
        }

        public bool RemoveRoleDefinitions(int principalId, params string[] names)
        {
            return RemoveRoleDefinitionsAsync(principalId, names).GetAwaiter().GetResult();
        }

        public async Task<bool> RemoveRoleDefinitionsAsync(int principalId, params string[] names)
        {
            var result = false;
            foreach (var name in names)
            {
                var roleDefinitions = await GetRoleDefinitionsAsync(principalId).ConfigureAwait(false);

                var roleDefinition = roleDefinitions.AsEnumerable().FirstOrDefault(r => r.Name == name);
                if (roleDefinition != null)
                {
                    var apiCall = new ApiCall($"_api/web/lists(guid'{Id}')/roleassignments/removeroleassignment(principalid={principalId},roledefid={roleDefinition.Id})", ApiType.SPORest);
                    var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);
                    result = response.StatusCode == System.Net.HttpStatusCode.OK;
                }
                else
                {
                    throw new ArgumentException($"Role definition '{name}' not found for this group.");
                }
            }
            return result;
        }
        #endregion

        #region Folders

        public IListItem AddListFolder(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return AddListFolderAsync(path, parentFolder, contentTypeId).GetAwaiter().GetResult();
        }

        public async Task<IListItem> AddListFolderAsync(string path, string parentFolder = null, string contentTypeId = "0x0120")
        {
            return await this.Items.AddAsync(new Dictionary<string, object>
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
        #endregion
    }
}
