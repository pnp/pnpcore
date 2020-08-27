using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    internal partial class List
    {
        internal static Expression<Func<IList, object>>[] GetListDataAsStreamExpression = new Expression<Func<IList, object>>[] { p => p.Fields.Include(p => p.InternalName, p => p.FieldTypeKind) };

        public List()
        {
            MappingHandler = (FromJson input) =>
            {
                // Handle the mapping from json to the domain model for the cases which are not generically handled
                switch (input.TargetType.Name)
                {
                    case "ListExperience": return JsonMappingHelper.ToEnum<ListExperience>(input.JsonElement);
                    case "ListReadingDirection": return JsonMappingHelper.ToEnum<ListReadingDirection>(input.JsonElement);
                    case "ListTemplateType": return JsonMappingHelper.ToEnum<ListTemplateType>(input.JsonElement);
                }

                input.Log.LogDebug($"Field {input.FieldName} could not be mapped when converting from JSON");

                return null;
            };

            // Handler to construct the Add request for this list
            AddApiCallHandler = async (additionalInformation) =>
            {
                var entity = EntityManager.Instance.GetClassInfo(GetType(), this);

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

        #region Extension methods

        #region BatchGetByTitle
        private static ApiCall GetByTitleApiCall(string title)
        {
            return new ApiCall($"_api/web/lists/getbytitle('{title}')", ApiType.SPORest);
        }

        internal async Task<IList> BatchGetByTitleAsync(Batch batch, string title, params Expression<Func<IList, object>>[] expressions)
        {
            await BaseBatchGetAsync(batch, apiOverride: GetByTitleApiCall(title), fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return this;
        }
        #endregion

        #region RecycleAsync
        public async Task<Guid> RecycleAsync()
        {
            var apiCall = new ApiCall($"_api/Web/Lists(guid'{Id}')/recycle", ApiType.SPORest);

            var response = await RawRequestAsync(apiCall, HttpMethod.Post).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response.Json))
            {
                var document = JsonSerializer.Deserialize<JsonElement>(response.Json);
                if (document.TryGetProperty("d", out JsonElement root))
                {
                    if (root.TryGetProperty("Recycle", out JsonElement recycleBinItemId))
                    {
                        // Remove this item from the lists collection
                        RemoveFromParentCollection();

                        // return the recyclebin item id
                        return recycleBinItemId.GetGuid();
                    }
                }
            }

            return Guid.Empty;
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
            var camlQuery = new
            {
                query = new
                {
                    __metadata = new { type = "SP.CamlQuery" },
                    queryOptions.ViewXml,
                    queryOptions.AllowIncrementalResults,
                    queryOptions.DatesInUtc,
                    queryOptions.FolderServerRelativeUrl,
                    queryOptions.ListItemCollectionPosition
                }
            }.AsExpando();
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

            if (!this.ArePropertiesAvailable(GetListDataAsStreamExpression))
            {
                // Get field information via batch
                await GetBatchAsync(batch, GetListDataAsStreamExpression).ConfigureAwait(false);
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

        #endregion
    }
}
