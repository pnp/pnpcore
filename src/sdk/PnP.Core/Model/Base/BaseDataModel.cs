using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#if DEBUG
[assembly: InternalsVisibleTo("PnP.Core.Test")]
#endif
namespace PnP.Core.Model
{

    /// <summary>
    /// Delegate for requesting the Api call for doing an ADD operation
    /// </summary>
    /// <returns>API call for adding a model entity</returns>
    internal delegate Task<ApiCall> AddApiCall(Dictionary<string, object> additionalInformation = null);

    /// <summary>
    /// Delegate for overriding the default API call in case of a GET request
    /// </summary>
    /// <param name="input">Generated API call</param>
    /// <returns>Changed API call</returns>
    internal delegate ApiCallRequest GetApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Delegate for overriding the default API call in case of a UPDATE request
    /// </summary>
    /// <param name="input">Generated API call</param>
    /// <returns>Changed API call</returns>
    internal delegate ApiCallRequest UpdateApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Delegate for overriding the default API call in case of a DELETE request
    /// </summary>
    /// <param name="input">Generated API call</param>
    /// <returns>Changed API call</returns>
    internal delegate ApiCallRequest DeleteApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Base class for all model classes
    /// </summary>
    /// <typeparam name="TModel">Model class</typeparam>
    internal class BaseDataModel<TModel> : TransientObject, IDataModel<TModel>, IRequestable, IMetadataExtensible, IDataModelWithKey, IDataModelMappingHandler
    {
        #region Core properties

        /// <summary>
        /// Dictionary to access the domain model object Metadata
        /// </summary>
        [SystemProperty]
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        /// <summary>
        /// PnP Context
        /// </summary>
        [SystemProperty]
        public PnPContext PnPContext { get; set; }

        /// <summary>
        /// Returns the parent of this model
        /// </summary>
        [SystemProperty]
        public IDataModelParent Parent { get; set; }

        /// <summary>
        /// Connected logger
        /// </summary>
        [SystemProperty]
        internal ILogger Log
        {
            get
            {
                return PnPContext.Logger;
            }
        }

        /// <summary>
        /// Indicates whether this model was fetched from the server
        /// </summary>
        [SystemProperty]
        public bool Requested { get; set; } = false;

        #endregion

        #region Core data access methods

        #region Get

        public async Task<object> GetBatchAsync(Batch batch, params Expression<Func<object, object>>[] expressions)
        {
            var newExpressions = expressions.Select(e => Expression.Lambda<Func<TModel, object>>(e.Body, e.Parameters)).ToArray();
            return await GetInternalBatchAsync(batch, newExpressions).ConfigureAwait(false);
        }

        public async Task<object> GetBatchAsync(params Expression<Func<object, object>>[] expressions)
        {
            var newExpressions = expressions.Select(e => Expression.Lambda<Func<TModel, object>>(e.Body, e.Parameters)).ToArray();
            return await GetInternalBatchAsync(PnPContext.CurrentBatch, newExpressions).ConfigureAwait(false);
        }

        public object GetBatch(Batch batch, params Expression<Func<object, object>>[] expressions)
        {
            return GetBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        public object GetBatch(params Expression<Func<object, object>>[] expressions)
        {
            return GetBatchAsync(expressions).GetAwaiter().GetResult();
        }

        public async Task<object> GetAsync(params Expression<Func<object, object>>[] expressions)
        {
            var newExpressions = expressions.Select(e => Expression.Lambda<Func<TModel, object>>(e.Body, e.Parameters)).ToArray();
            return await GetAsyncInternal(default, newExpressions).ConfigureAwait(true);
        }

        public async Task<object> GetAsync(ApiResponse apiResponse, params Expression<Func<object, object>>[] expressions)
        {
            var newExpressions = expressions.Select(e => Expression.Lambda<Func<TModel, object>>(e.Body, e.Parameters)).ToArray();
            return await GetAsyncInternal(apiResponse, newExpressions).ConfigureAwait(true);
        }

        public object Get(params Expression<Func<object, object>>[] expressions)
        {
            return GetAsync(expressions).GetAwaiter().GetResult();
        }

        public object Get(ApiResponse apiResponse, params Expression<Func<object, object>>[] expressions)
        {
            return GetAsync(apiResponse, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public async virtual Task<TModel> GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetInternalBatchAsync(batch, expressions).ConfigureAwait(false);
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public async virtual Task<TModel> GetBatchAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetInternalBatchAsync(PnPContext.CurrentBatch, expressions).ConfigureAwait(false);
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual TModel GetBatch(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual TModel GetBatch(params Expression<Func<TModel, object>>[] expressions)
        {
            return GetBatchAsync(expressions).GetAwaiter().GetResult();
        }

        private async Task<TModel> GetInternalBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            await BaseBatchGetAsync(batch, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return ToDynamic(this);
        }

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual async Task<TModel> GetAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetAsyncInternal(default, expressions).ConfigureAwait(true);
        }

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual async Task<TModel> GetAsync(ApiResponse apiResponse, params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetAsyncInternal(apiResponse, expressions).ConfigureAwait(true);
        }

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual TModel Get(params Expression<Func<TModel, object>>[] expressions)
        {
            return GetAsync(expressions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Special override the load child model classes ==> if apiResponse != default then 
        /// no get will be done but the system will handle the mapping from json to model
        /// </summary>
        /// <param name="apiResponse">Json response (when in recursive mapping of json to model), default otherwise</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual TModel Get(ApiResponse apiResponse, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetAsync(apiResponse, expressions).GetAwaiter().GetResult();
        }

        private async Task<TModel> GetAsyncInternal(ApiResponse apiResponse, params Expression<Func<TModel, object>>[] expressions)
        {
            await BaseGet(apiResponse, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, expressions: expressions).ConfigureAwait(false);
            return ToDynamic(this);
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <returns>The added domain model</returns>
        internal async virtual Task<BaseDataModel<TModel>> AddBatchAsync(Dictionary<string, object> keyValuePairs = null)
        {
            //var call = AddApiCallHandler?.Invoke(keyValuePairs);
            //if (call.HasValue)
            //{
            //    await BaseBatchAddAsync(call.Value, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
            //}
            //else
            //{
            //    throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            //}
            //return this;


            if (AddApiCallHandler != null)
            {
                var call = await AddApiCallHandler.Invoke(keyValuePairs).ConfigureAwait(false);
                await BaseBatchAddAsync(call, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
                return this;
            }
            else
            {
                throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            }
        }

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="keyValuePairs">Properties to control add</param>
        /// <returns>The added domain model</returns>
        internal async virtual Task<BaseDataModel<TModel>> AddBatchAsync(Batch batch, Dictionary<string, object> keyValuePairs = null)
        {
            //var call = AddApiCallHandler?.Invoke(keyValuePairs);
            //if (call.HasValue)
            //{
            //    await BaseBatchAddAsync(batch, call.Value, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
            //}
            //else
            //{
            //    throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            //}
            //return this;

            if (AddApiCallHandler != null)
            {
                var call = await AddApiCallHandler.Invoke(keyValuePairs).ConfigureAwait(false);
                await BaseBatchAddAsync(batch, call, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
                return this;
            }
            else
            {
                throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            }
        }

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <returns>The added domain model</returns>
        internal virtual async Task<BaseDataModel<TModel>> AddAsync(Dictionary<string, object> keyValuePairs = null)
        {
            //var call = AddApiCallHandler?.Invoke(keyValuePairs);
            //if (call.HasValue)
            //{
            //    await BaseAdd(call.Value, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
            //}
            //else
            //{
            //    throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            //}
            //return this;

            if (AddApiCallHandler != null)
            {
                var call = await AddApiCallHandler.Invoke(keyValuePairs).ConfigureAwait(false);
                await BaseAdd(call, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
                return this;
            }
            else
            {
                throw new ClientException(ErrorType.MissingAddApiHandler, "Adding requires the implementation of an AddApiCallHandler handler returning an add ApiCall");
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates a domain model
        /// </summary>
        public async virtual Task UpdateBatchAsync()
        {
            await UpdateBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public async virtual Task UpdateBatchAsync(Batch batch)
        {
            await BaseBatchUpdateAsync(batch, MappingHandler, PostMappingHandler).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        public virtual async Task UpdateAsync()
        {
            await BaseUpdate(MappingHandler, PostMappingHandler).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        public virtual void UpdateBatch()
        {
            UpdateBatchAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public virtual void UpdateBatch(Batch batch)
        {
            UpdateBatchAsync(batch).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        public virtual void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        public async virtual Task DeleteBatchAsync()
        {
            await DeleteBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public async virtual Task DeleteBatchAsync(Batch batch)
        {
            await BaseBatchDeleteAsync(batch).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        public virtual async Task DeleteAsync()
        {
            await BaseDelete().ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        public virtual void DeleteBatch()
        {
            DeleteBatchAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public virtual void DeleteBatch(Batch batch)
        {
            DeleteBatchAsync(batch).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        public virtual void Delete()
        {
            DeleteAsync().GetAwaiter().GetResult();
        }

        #endregion

        #endregion

        #region Core data access implementation

        #region Get

        /// <summary>
        /// API call override handler for get requests
        /// </summary>
        [SystemProperty]
        internal GetApiCallOverride GetApiCallOverrideHandler { get; set; } = null;

        /// <summary>
        /// Handler that will fire when a property mapping does cannot be done automatically
        /// </summary>
        [SystemProperty]
        public Func<FromJson, object> MappingHandler { get; set; } = null;

        /// <summary>
        /// Handler that will fire after the full json to model operation was done
        /// </summary>
        [SystemProperty]
        public Action<string> PostMappingHandler { get; set; } = null;

        internal virtual async Task BaseGet(ApiCall apiOverride = default, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null, params Expression<Func<TModel, object>>[] expressions)
        {
            await BaseGet(default, apiOverride, fromJsonCasting, postMappingJson, expressions).ConfigureAwait(false);
        }

        internal virtual async Task BaseGet(ApiResponse apiResponse, ApiCall apiOverride = default, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null, params Expression<Func<TModel, object>>[] expressions)
        {
            // Get entity information for the entity to load
            var entityInfo = GetClassInfo(expressions);

            // This method was invoked from loading another entity object 
            // e.g. Loading Site.RootWeb will handle the load via the Site entity Get method but will call 
            //      into the Web entity Get method to load the RootWeb property
            if (!apiResponse.Equals(default(ApiResponse)))
            {
                await JsonMappingHelper.FromJson(this, entityInfo, apiResponse, fromJsonCasting).ConfigureAwait(false);
                postMappingJson?.Invoke(apiResponse.JsonElement.ToString());
            }
            else
            {
                // Construct the API call to make
                var api = await BuildGetAPICallAsync(entityInfo, apiOverride).ConfigureAwait(false);

                if (api.Cancelled)
                {
                    ApiCancellationMessage(api);
                    return;
                }

                var batch = PnPContext.BatchClient.EnsureBatch();
                batch.Add(this, entityInfo, HttpMethod.Get, api.ApiCall, default, fromJsonCasting, postMappingJson);

                // The domain model for Graph can have non expandable collections, hence these require an additional API call to populate. 
                // Let's ensure these additional API calls's are included in a single batch
                if (api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta)
                {
                    await AddBatchRequestsForNonExpandableCollectionsAsync(batch, entityInfo, expressions, fromJsonCasting, postMappingJson).ConfigureAwait(false);
                }

                await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates and adds a Get request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiOverride">Override for the API call</param>
        /// <param name="expressions">Expressions needed to create the request</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchGetAsync(Batch batch, ApiCall apiOverride = default, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null, params Expression<Func<TModel, object>>[] expressions)
        {
            // Get entity information for the entity to load
            var entityInfo = GetClassInfo(expressions);
            // Construct the API call to make
            var api = await BuildGetAPICallAsync(entityInfo, apiOverride).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            // Also try to build the rest equivalent, this will be used in case we encounter mixed rest/graph batches
            ApiCallRequest apiRestBackup = new ApiCallRequest(default);
            // Only build a backup call when the type can be requested via SharePoint REST
            if ((api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta) && !string.IsNullOrEmpty(entityInfo.SharePointType))
            {
                // Try to get the API call, but this time using rest
                apiRestBackup = await BuildGetAPICallAsync(entityInfo, apiOverride, true).ConfigureAwait(false);
            }

            batch.Add(this, entityInfo, HttpMethod.Get, api.ApiCall, apiRestBackup.ApiCall, fromJsonCasting, postMappingJson);

            // The domain model for Graph can have non expandable collections, hence these require an additional API call to populate. 
            // Let's ensure these additional API calls's are included in a single batch
            if (api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta)
            {
                await AddBatchRequestsForNonExpandableCollectionsAsync(batch, entityInfo, expressions, fromJsonCasting, postMappingJson).ConfigureAwait(false);
            }
        }

        internal async Task<ApiCallRequest> BuildGetAPICallAsync(EntityInfo entity, ApiCall apiOverride, bool forceSPORest = false, bool useLinqGet = false)
        {
            // Usefull links:
            // - https://www.odata.org/documentation/odata-version-3-0/
            // - https://s-kainet.github.io/sp-rest-explorer/#/entity/SP.Web
            // - https://github.com/koltyakov/sp-metadata
            // - http://anomepani.github.io
            // - https://platinumdogs.me/2013/03/14/sharepoint-adventures-with-the-rest-api-part-1/#columns
            // - https://platinumdogs.me/2013/05/14/client-and-server-driven-paging-with-the-sharepoint-rest-api/

            // Can we use Microsoft Graph for this GET request?

            bool useGraph = PnPContext.GraphFirst &&    // See if Graph First is enabled/configured
                !forceSPORest &&                        // and if we are not forced to use SPO REST
                entity.CanUseGraphGet;                  // and if the entity supports GET via Graph 

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }
            // Else if we've overriden the query then simply take what was set in the query override
            else if (!apiOverride.Equals(default(ApiCall)))
            {
                useGraph = apiOverride.Type == ApiType.Graph;
            }

            if (useGraph)
            {
                return await BuildGetAPICallGraphAsync(entity, apiOverride, useLinqGet).ConfigureAwait(false);
            }
            else
            {
                return await BuildGetAPICallRestAsync(entity, apiOverride, useLinqGet).ConfigureAwait(false);
            }
        }

        private void ApiCancellationMessage(ApiCallRequest api)
        {
            Log.LogInformation($"API call {api.ApiCall.Request} cancelled: {api.CancellationReason}");
        }

        private async Task<ApiCallRequest> BuildGetAPICallRestAsync(EntityInfo entity, ApiCall apiOverride, bool useLinqGet)
        {
            string getApi = useLinqGet ? entity.SharePointLinqGet : entity.SharePointGet;

            IEnumerable<EntityFieldInfo> fields = entity.Fields.Where(p => p.Load);

            Dictionary<string, string> urlParameters = new Dictionary<string, string>(2);

            StringBuilder sb = new StringBuilder();

            // Only add select statement whenever there was a filter specified
            if (entity.SharePointFieldsLoadedViaExpression)
            {
                // $select
                foreach (var field in fields)
                {
                    // If there was a selection on which fields to include in an expand (via the Include() option) then add those fields
                    if (field.SharePointExpandable && field.ExpandFieldInfo != null)
                    {
                        AddExpandableSelectRest(sb, field, null, "");
                    }
                    else
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)},");
                    }
                }

                urlParameters.Add("$select", sb.ToString().TrimEnd(new char[] { ',' }));
                sb.Clear();
            }

            // $expand
            foreach (var field in fields.Where(p => p.SharePointExpandable))
            {
                if (entity.SharePointFieldsLoadedViaExpression)
                {
                    sb.Append($"{JsonMappingHelper.GetRestField(field)},");

                    // If there was a selection on which fields to include in an expand (via the Include() option) and the included field was expandable itself then add it 
                    if (field.ExpandFieldInfo != null)
                    {
                        string path = "";
                        AddExpandableExpandRest(sb, field, null, path);
                    }
                }
                else
                {
                    if (field.ExpandableByDefault)
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)},");
                    }
                }
            }
            urlParameters.Add("$expand", sb.ToString().TrimEnd(new char[] { ',' }));
            sb.Clear();

            // Build the API call
            string baseApiCall = "";
            if (apiOverride.Equals(default(ApiCall)))
            {
                baseApiCall = $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{getApi}";
            }
            else
            {
                baseApiCall = $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{apiOverride.Request}";
            }

            // Parse tokens in the base api call
            baseApiCall = await ApiHelper.ParseApiCallAsync(this, baseApiCall).ConfigureAwait(false);

            sb.Append(baseApiCall);

            // Build the querystring parameters
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            foreach (var urlParameter in urlParameters.Where(i => !string.IsNullOrEmpty(i.Value)))
            {
                // Add key and value, which will be automatically URL-encoded, if needed
                queryString.Add(urlParameter.Key, urlParameter.Value);
            }

            // Build the whole URL
            if (queryString.AllKeys.Length > 0)
            {
                sb.Append($"?{queryString}");
            }

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(sb.ToString(), ApiType.SPORest));
            if (GetApiCallOverrideHandler != null)
            {
                call = GetApiCallOverrideHandler.Invoke(call);
            }

            return call;
        }

        private static void AddExpandableExpandRest(StringBuilder sb, EntityFieldInfo field, EntityFieldExpandInfo expandFields, string path)
        {
            EntityInfo collectionEntityInfo = null;
            if (expandFields == null)
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(field.ExpandFieldInfo.Type);
                expandFields = field.ExpandFieldInfo;
            }
            else
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(expandFields.Type);
            }

            foreach (var expandableField in expandFields.Fields.OrderBy(p => p.Expandable))
            {
                var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == expandableField.Name);

                if (expandableFieldInfo.SharePointExpandable)
                {
                    path = path + "/" + JsonMappingHelper.GetRestField(expandableFieldInfo);
                    sb.Append($"{JsonMappingHelper.GetRestField(field)}{path},");
                    if (expandableField.Fields.Any())
                    {
                        AddExpandableExpandRest(sb, field, expandableField, path);
                    }
                }
            }
        }

        private static void AddExpandableSelectRest(StringBuilder sb, EntityFieldInfo field, EntityFieldExpandInfo expandFields, string path)
        {
            EntityInfo collectionEntityInfo = null;

            if (expandFields == null)
            {
                collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(field.ExpandFieldInfo.Type);
                expandFields = field.ExpandFieldInfo;
            }
            else
            {
                if (expandFields.Type != null)
                {
                    collectionEntityInfo = EntityManager.Instance.GetStaticClassInfo(expandFields.Type);
                }
            }

            if (collectionEntityInfo != null)
            {
                foreach (var expandableField in expandFields.Fields.OrderBy(p => p.Expandable))
                {
                    var expandableFieldInfo = collectionEntityInfo.Fields.First(p => p.Name == expandableField.Name);
                    if (!expandableFieldInfo.SharePointExpandable)
                    {
                        sb.Append($"{JsonMappingHelper.GetRestField(field)}{path}/{JsonMappingHelper.GetRestField(expandableFieldInfo)},");
                    }
                    else
                    {
                        path = path + "/" + JsonMappingHelper.GetRestField(expandableFieldInfo);
                        AddExpandableSelectRest(sb, field, expandableField, path);
                    }
                }
            }
        }

        private async Task<ApiCallRequest> BuildGetAPICallGraphAsync(EntityInfo entity, ApiCall apiOverride, bool useLinqGet)
        {
            string getApi = useLinqGet ? entity.GraphLinqGet : entity.GraphGet;

            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Getting {getApi} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            IEnumerable<EntityFieldInfo> fields = entity.Fields.Where(p => p.Load);

            Dictionary<string, string> urlParameters = new Dictionary<string, string>(2);

            StringBuilder sb = new StringBuilder();

            // Only add select statement whenever there was a filter specified
            if (entity.GraphFieldsLoadedViaExpression)
            {
                // $select
                bool graphIdFieldAdded = false;
                foreach (var field in fields)
                {
                    // Don't add the field in the select if it will be added as expandable field
                    if (!string.IsNullOrEmpty(field.GraphName))
                    {
                        bool addExpand = true;
                        if (field.GraphBeta)
                        {
                            if (CanUseGraphBeta(field))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Field will be skipped as we're forced to use v1
                                addExpand = false;
                            }
                        }
                        else
                        {
                            // What if the complex type we're loading contains a beta property
                            apiType = VerifyIfUsedComplexTypeRequiresBeta(apiType, field);
                        }

                        if (addExpand)
                        {
                            sb.Append(JsonMappingHelper.GetGraphField(field));
                        }
                    }

                    sb.Append(",");

                    if (!graphIdFieldAdded && !string.IsNullOrEmpty(entity.GraphId))
                    {
                        if (JsonMappingHelper.GetGraphField(field) == entity.GraphId)
                        {
                            graphIdFieldAdded = true;
                        }
                    }
                }

                if (!graphIdFieldAdded && !string.IsNullOrEmpty(entity.GraphId))
                {
                    sb.Append($"{entity.GraphId},");
                }

                urlParameters.Add("$select", sb.ToString().TrimEnd(new char[] { ',' }));
                sb.Clear();
            }

            // $expand
            foreach (var field in fields.Where(p => p.GraphExpandable && string.IsNullOrEmpty(p.GraphGet)))
            {
                if (!string.IsNullOrEmpty(field.GraphName))
                {
                    if (entity.GraphFieldsLoadedViaExpression)
                    {
                        bool addExpand = true;

                        if (field.GraphBeta)
                        {
                            if (CanUseGraphBeta(field))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Expand will be skipped since we're bound to v1 graph
                                addExpand = false;
                            }
                        }

                        if (addExpand)
                        {
                            sb.Append($"{JsonMappingHelper.GetGraphField(field)},");
                        }
                    }
                    else
                    {
                        if (field.ExpandableByDefault)
                        {
                            bool addExpand = true;

                            if (field.GraphBeta)
                            {
                                if (CanUseGraphBeta(field))
                                {
                                    apiType = ApiType.GraphBeta;
                                }
                                else
                                {
                                    // Expand will be skipped since we're bound to v1 graph
                                    addExpand = false;
                                }
                            }

                            if (addExpand)
                            {
                                sb.Append($"{JsonMappingHelper.GetGraphField(field)},");
                            }
                        }
                    }
                }
            }
            urlParameters.Add("$expand", sb.ToString().TrimEnd(new char[] { ',' }));
            sb.Clear();

            // Build the API call
            string baseApiCall = "";
            if (apiOverride.Equals(default(ApiCall)))
            {
                if (string.IsNullOrEmpty(getApi))
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect, $"Specify the GraphGet/GraphGetLinq field of the ClassMapping property.");
                }

                // Ensure tokens in the base url are replaced
                baseApiCall = await TokenHandler.ResolveTokensAsync(this, getApi, PnPContext).ConfigureAwait(false);
            }
            else
            {
                // Ensure tokens in the base url are replaced
                baseApiCall = await TokenHandler.ResolveTokensAsync(this, apiOverride.Request, PnPContext).ConfigureAwait(false);
            }

            // Parse tokens in the base api call
            baseApiCall = await ApiHelper.ParseApiCallAsync(this, baseApiCall).ConfigureAwait(false);

            sb.Append(baseApiCall);

            // Build the querystring parameters
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            foreach (var urlParameter in urlParameters.Where(i => !string.IsNullOrEmpty(i.Value)))
            {
                // Add key and value, which will be automatically URL-encoded, if needed
                queryString.Add(urlParameter.Key, urlParameter.Value);
            }

            // Build the whole URL
            if (queryString.AllKeys.Length > 0)
            {
                sb.Append($"?{queryString}");
            }

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(sb.ToString(), apiType));
            if (GetApiCallOverrideHandler != null)
            {
                call = GetApiCallOverrideHandler.Invoke(call);
            }

            return call;
        }

        private async Task AddBatchRequestsForNonExpandableCollectionsAsync(Batch batch, EntityInfo entityInfo, Expression<Func<TModel, object>>[] expressions, Func<FromJson, object> fromJsonCasting, Action<string> postMappingJson)
        {
            ApiType apiType = ApiType.Graph;

            var nonExpandableFields = entityInfo.GraphNonExpandableCollections;
            if (nonExpandableFields.Any())
            {
                foreach (var nonExpandableField in nonExpandableFields)
                {
                    // Was this non expandable field requested?
                    bool needed = nonExpandableField.ExpandableByDefault;

                    if (!needed)
                    {
                        // Check passed in expressions
                        needed = IsDefinedInExpression(expressions, nonExpandableField.Name);
                    }

                    if (needed)
                    {
                        bool addExpandableCollection = true;
                        if (nonExpandableField.GraphBeta)
                        {
                            if (CanUseGraphBeta(nonExpandableField))
                            {
                                apiType = ApiType.GraphBeta;
                            }
                            else
                            {
                                // Can't add this non expanable collection request since we're bound to Graph v1
                                addExpandableCollection = false;
                            }
                        }

                        if (addExpandableCollection)
                        {
                            var parsedApiRequest = await ApiHelper.ParseApiRequestAsync(this, nonExpandableField.GraphGet).ConfigureAwait(false);

                            ApiCall extraApiCall = new ApiCall(parsedApiRequest, apiType, receivingProperty: nonExpandableField.Name);
                            batch.Add(this, entityInfo, HttpMethod.Get, extraApiCall, default, fromJsonCasting, postMappingJson);
                        }
                    }
                }
            }
        }

        private static bool IsDefinedInExpression(Expression<Func<TModel, object>>[] expressions, string field)
        {
            foreach (var expression in expressions)
            {
                if (expression.Body.NodeType == ExpressionType.Call && expression.Body is MethodCallExpression)
                {
                    // Future use? (includes)
                }
                else
                {
                    var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;
                    if (body != null && body.Member.Name.Equals(field, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Add

        /// <summary>
        /// API call handler for add requests
        /// </summary>
        internal AddApiCall AddApiCallHandler { get; set; } = null;

        /// <summary>
        /// Creates and adds a Post request to the given batch
        /// </summary>
        /// <param name="postApiCall">Api call to execute</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchAddAsync(ApiCall postApiCall, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            await BaseBatchAddAsync(PnPContext.CurrentBatch, postApiCall, fromJsonCasting, postMappingJson).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates and adds a Post request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="postApiCall">Api call to execute</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchAddAsync(Batch batch, ApiCall postApiCall, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            // Prefix API request with context url if needed
            postApiCall = PrefixAddApiCall(postApiCall, entityInfo);

            // Ensure token replacement is done
            postApiCall.Request = await TokenHandler.ResolveTokensAsync(this, postApiCall.Request, PnPContext).ConfigureAwait(false);

            // Ensure there's no Graph beta endpoint being used when that was not allowed
            if (!CanUseGraphBetaForAdd(postApiCall, entityInfo))
            {
                return;
            }

            // Add the request to the batch
            batch.Add(this, entityInfo, HttpMethod.Post, postApiCall, default, fromJsonCasting, postMappingJson);
        }

        /// <summary>
        /// Creates and adds a Post request to the given batch
        /// </summary>
        /// <param name="postApiCall">Api call to execute</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseAdd(ApiCall postApiCall, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            // Prefix API request with context url if needed
            postApiCall = PrefixAddApiCall(postApiCall, entityInfo);

            // Ensure token replacement is done
            postApiCall.Request = await TokenHandler.ResolveTokensAsync(this, postApiCall.Request, PnPContext).ConfigureAwait(false);

            // Ensure there's no Graph beta endpoint being used when that was not allowed
            if (!CanUseGraphBetaForAdd(postApiCall, entityInfo))
            {
                throw new ClientException(ErrorType.GraphBetaNotAllowed, "Adding this entity requires the use of the Graph beta endpoint");
            }

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Post, postApiCall, default, fromJsonCasting, postMappingJson);
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        private ApiCall PrefixAddApiCall(ApiCall postApiCall, EntityInfo entityInfo)
        {
            if (!string.IsNullOrEmpty(entityInfo.SharePointType))
            {
                // Prefix API request with context url
                postApiCall.Request = $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{postApiCall.Request}";
            }

            return postApiCall;
        }

        private bool CanUseGraphBetaForAdd(ApiCall postApiCall, EntityInfo entityInfo)
        {
            if (postApiCall.Type == ApiType.GraphBeta && !PnPContext.GraphCanUseBeta)
            {
                ApiCallRequest addRequest = new ApiCallRequest(postApiCall);
                addRequest.CancelRequest($"Adding {entityInfo.GraphGet} requires the Graph Beta endpoint which was not configured to be allowed");
                ApiCancellationMessage(addRequest);
                return false;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// API call override handler for update requests
        /// </summary>
        [SystemProperty]
        internal UpdateApiCallOverride UpdateApiCallOverrideHandler { get; set; } = null;

        /// <summary>
        /// Creates and adds an Update request to the given batch
        /// </summary>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchUpdateAsync(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            await BaseBatchUpdateAsync(PnPContext.CurrentBatch, fromJsonCasting, postMappingJson).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates and adds an Update request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await BuildUpdateAPICallAsync(entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            batch.Add(this, entityInfo, HttpMethod.Patch, api.ApiCall, default, fromJsonCasting, postMappingJson);
        }

        /// <summary>
        /// Creates and adds an Update request to the given batch
        /// </summary>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseUpdate(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await BuildUpdateAPICallAsync(entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Patch, api.ApiCall, default, fromJsonCasting, postMappingJson);
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        internal async Task<ApiCallRequest> BuildUpdateAPICallAsync(EntityInfo entity)
        {
            bool useGraph = false;

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }

            if (useGraph)
            {
                return await BuildUpdateAPICallGraphAsync(entity).ConfigureAwait(false);
            }
            else
            {
                return await BuildUpdateAPICallRestAsync(entity).ConfigureAwait(false);
            }
        }

        internal async Task<ApiCallRequest> BuildUpdateAPICallGraphAsync(EntityInfo entity)
        {
            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Updating {entity.GraphUpdate} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            var changedProperties = this.GetChangedProperties();
            foreach (PropertyDescriptor cp in changedProperties)
            {
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    bool addField = true;
                    if (changedField.GraphBeta)
                    {
                        if (CanUseGraphBeta(changedField))
                        {
                            apiType = ApiType.GraphBeta;
                        }
                        else
                        {
                            addField = false;
                        }
                    }
                    else
                    {
                        // What if the complex type we're updating contains a beta property
                        if (ComplexTypeHasBetaProperty(changedField) && !PnPContext.GraphCanUseBeta)
                        {
                            addField = false;
                        }
                    }

                    if (addField)
                    {
                        if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                        {
                            // Get the changed properties in the dictionary
                            var dictionaryObject = (TransientDictionary)cp.GetValue(this);
                            foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                            {
                                // Let's set its value into the update message
                                ((ExpandoObject)updateMessage).SetProperty(changedProp.Key, changedProp.Value);
                            }
                        }
                        else if (JsonMappingHelper.IsComplexType(changedField.PropertyInfo.PropertyType))
                        {
                            // Build a new dynamic object that will hold the changed properties of the complex type
                            dynamic updateMessageComplexType = new ExpandoObject();
                            var complexObject = this.GetValue(changedField.Name) as TransientObject;
                            // Get the properties that have changed in the complex type
                            foreach (string changedProp in complexObject.ChangedProperties)
                            {
                                ((ExpandoObject)updateMessageComplexType).SetProperty(changedProp, complexObject.GetValue(changedProp));
                            }
                            // Add this as value to the original changed property
                            ((ExpandoObject)updateMessage).SetProperty(changedField.GraphName, updateMessageComplexType as object);
                        }
                        else
                        {
                            // Let's set its value into the update message
                            ((ExpandoObject)updateMessage).SetProperty(changedField.GraphName, this.GetValue(changedField.Name));
                        }
                    }
                }
            }

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    WriteIndented = false,
                    // For some reason the naming policy is not applied on ExpandoObjects
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(this, entity.GraphUpdate).ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(updateUrl, apiType, jsonUpdateMessage)
            {
                Commit = true
            });
            if (UpdateApiCallOverrideHandler != null)
            {
                call = UpdateApiCallOverrideHandler.Invoke(call);
            }
            // If the call was cancelled by the override then bail out
            if (call.Cancelled)
            {
                return call;
            }

            // If a field validation prevented updating a field it might mean there's nothing to update, if so then don't do a server request
            if (string.IsNullOrEmpty(call.ApiCall.JsonBody) || call.ApiCall.JsonBody == "{}")
            {
                call.CancelRequest("No changes (empty body), so nothing to update");
            }

            return call;
        }


        internal async Task<ApiCallRequest> BuildUpdateAPICallRestAsync(EntityInfo entity)
        {
            IEnumerable<EntityFieldInfo> fields = entity.Fields;

            // Define the JSON body of the update request based on the actual changes
            dynamic updateMessage = new ExpandoObject();

            // Configure the metadata section of the update request
            if (Metadata.ContainsKey(PnPConstants.MetaDataType))
            {
                updateMessage.__metadata = new
                {
                    type = Metadata[PnPConstants.MetaDataType]
                };
            }

            var changedProperties = this.GetChangedProperties();
            foreach (PropertyDescriptor cp in changedProperties)
            {
                // Look for the corresponding property in the type
                var changedField = fields.FirstOrDefault(f => f.Name == cp.Name);

                // If we found a field 
                if (changedField != null)
                {
                    if (changedField.DataType.FullName == typeof(TransientDictionary).FullName)
                    {
                        // Get the changed properties in the dictionary
                        var dictionaryObject = (TransientDictionary)cp.GetValue(this);
                        foreach (KeyValuePair<string, object> changedProp in dictionaryObject.ChangedProperties)
                        {
                            // Let's set its value into the update message
                            ((ExpandoObject)updateMessage).SetProperty(changedProp.Key, changedProp.Value);
                        }
                    }
                    else
                    {
                        // Let's set its value into the update message
                        ((ExpandoObject)updateMessage).SetProperty(changedField.SharePointName, this.GetValue(changedField.Name));
                    }
                }
            }

            // Get the corresponding JSON text content
            var jsonUpdateMessage = JsonSerializer.Serialize(updateMessage,
                typeof(ExpandoObject),
                new JsonSerializerOptions { WriteIndented = true });

            // Prepare the variable to contain the target URL for the update operation
            var updateUrl = await ApiHelper.ParseApiCallAsync(this, $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointUpdate}").ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(updateUrl, ApiType.SPORest, jsonUpdateMessage)
            {
                Commit = true
            });

            if (UpdateApiCallOverrideHandler != null)
            {
                call = UpdateApiCallOverrideHandler.Invoke(call);
            }

            // If the call was cancelled by the override then bail out
            if (call.Cancelled)
            {
                return call;
            }

            // If a field validation prevented updating a field it might mean there's nothing to update, if so then don't do a server request
            if (string.IsNullOrEmpty(call.ApiCall.JsonBody) || call.ApiCall.JsonBody == "{}")
            {
                call.CancelRequest("No changes (empty body), so nothing to update");
            }

            return call;
        }
        #endregion

        #region Delete

        /// <summary>
        /// API call override handler for delete requests
        /// </summary>
        [SystemProperty]
        internal DeleteApiCallOverride DeleteApiCallOverrideHandler { get; set; } = null;

        /// <summary>
        /// Creates and adds an Delete request to the given batch
        /// </summary>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual void BaseBatchDeleteAsync(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            await BaseBatchDeleteAsync(PnPContext.CurrentBatch, fromJsonCasting, postMappingJson).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates and adds an Delete request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal async virtual Task BaseBatchDeleteAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await BuildDeleteAPICallAsync(entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            // Also try to build the rest equivalent, this will be used in case we encounter mixed rest/graph batches
            batch.Add(this, entityInfo, HttpMethod.Delete, api.ApiCall, default, fromJsonCasting, postMappingJson);
        }

        /// <summary>
        /// Creates and adds an Delete request to the given batch
        /// </summary>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseDelete(Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await BuildDeleteAPICallAsync(entityInfo).ConfigureAwait(false);

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            batch.Add(this, entityInfo, HttpMethod.Delete, api.ApiCall, default, fromJsonCasting, postMappingJson);
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        internal async Task<ApiCallRequest> BuildDeleteAPICallAsync(EntityInfo entity)
        {
            bool useGraph = false;

            // If entity cannot be surfaced with SharePoint Rest then force graph
            if (string.IsNullOrEmpty(entity.SharePointType))
            {
                useGraph = true;
            }

            if (useGraph)
            {
                return await BuildDeleteAPICallGraphAsync(entity).ConfigureAwait(false);
            }
            else
            {
                return await BuildDeleteAPICallRestAsync(entity).ConfigureAwait(false);
            }
        }

        internal async Task<ApiCallRequest> BuildDeleteAPICallGraphAsync(EntityInfo entity)
        {
            ApiType apiType = ApiType.Graph;

            if (entity.GraphBeta)
            {
                if (CanUseGraphBeta(entity))
                {
                    apiType = ApiType.GraphBeta;
                }
                else
                {
                    // we can't make this request
                    var cancelledApiCallRequest = new ApiCallRequest(default);
                    cancelledApiCallRequest.CancelRequest($"Deleting {entity.GraphDelete} requires the Graph Beta endpoint which was not configured to be allowed");
                    return cancelledApiCallRequest;
                }
            }

            // Prepare the variable to contain the target URL for the delete operation
            var deleteUrl = await ApiHelper.ParseApiCallAsync(this, entity.GraphDelete).ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(deleteUrl, apiType));
            if (DeleteApiCallOverrideHandler != null)
            {
                call = DeleteApiCallOverrideHandler.Invoke(call);
            }

            return call;
        }

        internal async Task<ApiCallRequest> BuildDeleteAPICallRestAsync(EntityInfo entity)
        {
            // Prepare the variable to contain the target URL for the delete operation
            var deleteUrl = await ApiHelper.ParseApiCallAsync(this, $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{entity.SharePointDelete}").ConfigureAwait(false);

            // Create ApiCall instance and call the override option if needed
            var call = new ApiCallRequest(new ApiCall(deleteUrl, ApiType.SPORest));
            if (DeleteApiCallOverrideHandler != null)
            {
                call = DeleteApiCallOverrideHandler.Invoke(call);
            }

            return call;
        }
        #endregion

        #region Generic
        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task RequestBatchAsync(ApiCall apiCall, HttpMethod method)
        {
            await RequestBatchAsync(PnPContext.CurrentBatch, apiCall, method).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task RequestBatchAsync(Batch batch, ApiCall apiCall, HttpMethod method)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            // Prefix API request with context url if needed
            apiCall = PrefixApiCall(apiCall, entityInfo);

            // Ensure there's no Graph beta endpoint being used when that was not allowed
            if (!CanUseGraphBetaForRequest(apiCall, entityInfo))
            {
                return;
            }

            // Ensure token replacement is done
            apiCall.Request = await TokenHandler.ResolveTokensAsync(this, apiCall.Request, PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler);
        }

        /// <summary>
        /// Executes a given request
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task<Batch> RequestAsync(ApiCall apiCall, HttpMethod method)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            // Prefix API request with context url if needed
            apiCall = PrefixApiCall(apiCall, entityInfo);

            // Ensure there's no Graph beta endpoint being used when that was not allowed
            if (!CanUseGraphBetaForRequest(apiCall, entityInfo))
            {
                throw new ClientException(ErrorType.GraphBetaNotAllowed, "Adding this entity requires the use of the Graph beta endpoint");
            }

            // Ensure token replacement is done
            if (apiCall.Type == ApiType.CSOM)
            {
                apiCall.XmlBody = await TokenHandler.ResolveTokensAsync(this, apiCall.XmlBody, PnPContext).ConfigureAwait(false);
            }
            else
            {
                apiCall.Request = await TokenHandler.ResolveTokensAsync(this, apiCall.Request, PnPContext).ConfigureAwait(false);
            }

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler);
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
            return batch;
        }

        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task RawRequestBatchAsync(ApiCall apiCall, HttpMethod method)
        {
            await RawRequestBatchAsync(PnPContext.CurrentBatch, apiCall, method).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task RawRequestBatchAsync(Batch batch, ApiCall apiCall, HttpMethod method)
        {
            // Mark request as raw
            apiCall.RawRequest = true;

            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();

            // Prefix API request with context url if needed
            apiCall = PrefixApiCall(apiCall, entityInfo);

            // Ensure there's no Graph beta endpoint being used when that was not allowed
            if (!CanUseGraphBetaForRequest(apiCall, entityInfo))
            {
                return;
            }

            // Ensure token replacement is done
            if (apiCall.Type == ApiType.CSOM)
            {
                apiCall.XmlBody = await TokenHandler.ResolveTokensAsync(this, apiCall.XmlBody, PnPContext).ConfigureAwait(false);
            }
            else
            {
                apiCall.Request = await TokenHandler.ResolveTokensAsync(this, apiCall.Request, PnPContext).ConfigureAwait(false);
            }

            // Add the request to the batch
            batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler);
        }

        /// <summary>
        /// Executes a given request, not loading the response json in the model but simply returning it
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        internal async Task<ApiCallResponse> RawRequestAsync(ApiCall apiCall, HttpMethod method)
        {
            // Mark request as raw
            apiCall.RawRequest = true;

            var batch = await RequestAsync(apiCall, method).ConfigureAwait(false);
            return new ApiCallResponse(batch.Requests.First().Value.ResponseJson, 
                                       batch.Requests.First().Value.ResponseHttpStatusCode, 
                                       batch.Requests.First().Value.ResponseHeaders, 
                                       batch.Requests.First().Value.CsomResponseJson);
        }

        private ApiCall PrefixApiCall(ApiCall apiCall, EntityInfo entityInfo)
        {
            if (!string.IsNullOrEmpty(entityInfo.SharePointType))
            {
                // Prefix API request with context url
                apiCall.Request = $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{apiCall.Request}";
            }

            return apiCall;
        }

        private bool CanUseGraphBetaForRequest(ApiCall apiCall, EntityInfo entityInfo)
        {
            if (apiCall.Type == ApiType.GraphBeta && !PnPContext.GraphCanUseBeta)
            {
                ApiCallRequest request = new ApiCallRequest(apiCall);
                request.CancelRequest($"Request for {entityInfo.GraphGet} requires the Graph Beta endpoint which was not configured to be allowed");
                ApiCancellationMessage(request);
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

        #region Graph vs GraphBeta handling

        private bool CanUseGraphBeta(EntityFieldInfo field)
        {
            return PnPContext.GraphCanUseBeta && field.GraphBeta;
        }

        private bool CanUseGraphBeta(EntityInfo entity)
        {
            return PnPContext.GraphCanUseBeta && entity.GraphBeta;
        }

        private ApiType VerifyIfUsedComplexTypeRequiresBeta(ApiType apiType, EntityFieldInfo field)
        {
            if (ComplexTypeHasBetaProperty(field))
            {
                // Note: if we find one field that requires beta the complex type will trigger to use beta, assuming beta is allowed
                if (PnPContext.GraphCanUseBeta)
                {
                    apiType = ApiType.GraphBeta;
                }
            }

            // If a complex type also contains another complex type then we're ignoring the potential need to switch to beta

            return apiType;
        }

        private static bool ComplexTypeHasBetaProperty(EntityFieldInfo field)
        {
            if (JsonMappingHelper.IsComplexType(field.PropertyInfo.PropertyType))
            {
                var typeToCheck = Type.GetType($"{field.PropertyInfo.PropertyType.Namespace}.{field.PropertyInfo.PropertyType.Name.Substring(1)}");
                var complexTypeEntityInfo = EntityManager.Instance.GetStaticClassInfo(typeToCheck);
                return complexTypeEntityInfo.Fields.Any(p => p.GraphBeta);
            }

            return false;
        }
        #endregion

        #region Metadata handling

        internal string GetMetadata(string property)
        {
            if (Metadata.ContainsKey(property))
            {
                return Metadata[property];
            }

            return null;
        }

        internal void AddMetadata(string key, string value)
        {
            if (!Metadata.ContainsKey(key))
            {
                Metadata.Add(key, value);
            }
        }

        #endregion

        #region Translate model into a set of classes that are used to drive CRUD operations

        /// <summary>
        /// Translates model into a set of classes that are used to drive CRUD operations, this takes into account the passed expressions
        /// </summary>
        /// <param name="expressions">Data load expressions</param>
        /// <returns>Entity model class describing this model instance</returns>
        private EntityInfo GetClassInfo(params Expression<Func<TModel, object>>[] expressions)
        {
            return EntityManager.Instance.GetClassInfo<TModel>(GetType(), this, expressions);
        }

        #endregion

        #endregion

        #region Implementation of IDataModelWithKey

        [SystemProperty]
        public virtual object Key
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Implementation of Navigation property instantiation check
        private Dictionary<string, bool> navigationPropertyInstantiated = new Dictionary<string, bool>();

        internal protected bool NavigationPropertyInstantiated([CallerMemberName] string propertyName = "")
        {
            if (navigationPropertyInstantiated.ContainsKey(propertyName))
            {
                return navigationPropertyInstantiated[propertyName];
            }

            return false;
        }

        internal protected void InstantiateNavigationProperty([CallerMemberName] string propertyName = "")
        {
            if (navigationPropertyInstantiated.ContainsKey(propertyName))
            {
                navigationPropertyInstantiated[propertyName] = true;
            }
            else
            {
                navigationPropertyInstantiated.Add(propertyName, true);
            }
        }
        #endregion
    }
}
