using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    internal delegate Task<ApiCallRequest> GetApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Delegate for overriding the default API call in case of a UPDATE request
    /// </summary>
    /// <param name="input">Generated API call</param>
    /// <returns>Changed API call</returns>
    internal delegate Task<ApiCallRequest> UpdateApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Delegate for overriding the default API call in case of a DELETE request
    /// </summary>
    /// <param name="input">Generated API call</param>
    /// <returns>Changed API call</returns>
    internal delegate Task<ApiCallRequest> DeleteApiCallOverride(ApiCallRequest input);

    /// <summary>
    /// Base class for all model classes
    /// </summary>
    /// <typeparam name="TModel">Model class</typeparam>
    internal class BaseDataModel<TModel> : TransientObject, IDataModel<TModel>, IDataModelProcess, IDataModelLoad, IRequestable, IMetadataExtensible, IDataModelWithKey, IDataModelMappingHandler
    {
        #region Core properties

        /// <summary>
        /// Dictionary to access the domain model object Metadata
        /// </summary>
        [SystemProperty]
        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

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

        #region Public core data access methods

        #region Process

        /// <summary>
        /// This method processes an API response to populate the current domain model object
        /// </summary>
        /// <param name="apiResponse">The API response to process</param>
        /// <param name="expressions">List of expressions to define the fields to retrieve during deserialization of the API response</param>
        /// <returns>The deserialized domain model object</returns>
        public async Task ProcessResponseAsync(ApiResponse apiResponse, Expression<Func<object, object>>[] expressions)
        {
            var newExpressions = expressions.CastExpressions<TModel>();

            // Get entity information for the entity to load
            var entityInfo = GetClassInfo(newExpressions);

            await JsonMappingHelper.FromJson(this, entityInfo, apiResponse, MappingHandler).ConfigureAwait(false);
            PostMappingHandler?.Invoke(apiResponse.JsonElement.ToString());
        }

        #endregion

        #region Get

        /// <summary>
        /// Retrieves a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="expressions">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual async Task<TModel> GetAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            IDataModelParent replicatedParent = null;

            // Create a replicated parent
            if (Parent != null)
            {
                // Replicate the parent object in order to keep original collection as is
                replicatedParent = EntityManager.ReplicateParentHierarchy(Parent, PnPContext);
            }
            // Create a new object with a replicated parent
            var newDataModel = (BaseDataModel<TModel>)EntityManager.GetEntityConcreteInstance(GetType(), replicatedParent, PnPContext);

            // Replicate metadata and key between the objects
            EntityManager.ReplicateKeyAndMetadata(this, newDataModel);

            await newDataModel.BaseRetrieveAsync(expressions: expressions).ConfigureAwait(false);

            return (TModel)(object)newDataModel;
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual Task<IBatchSingleResult<TModel>> GetBatchAsync(Batch batch,
            params Expression<Func<TModel, object>>[] selectors)
        {
            return GetBatchAsync(batch, default, selectors);
        }

        /// <summary>
        /// Batches the retrieval of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="apiOverride"></param>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        internal virtual async Task<IBatchSingleResult<TModel>> GetBatchAsync(Batch batch,
            ApiCall apiOverride,
            params Expression<Func<TModel, object>>[] selectors)
        {
            IDataModelParent replicatedParent = null;

            // Create a replicated parent
            if (Parent != null)
            {
                // Replicate the parent object in order to keep original collection as is
                replicatedParent = EntityManager.ReplicateParentHierarchy(Parent, PnPContext);
            }
            // Create a new object with a replicated parent
            var newDataModel = (BaseDataModel<TModel>)EntityManager.GetEntityConcreteInstance(GetType(), replicatedParent, PnPContext);

            // Replicate metadata and key between the objects
            EntityManager.ReplicateKeyAndMetadata(this, newDataModel);

            // Make the actual request
            var batchResult = await newDataModel.BaseBatchRetrieveAsync(batch, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, selectors: selectors).ConfigureAwait(false);

            return batchResult;
        }

        #endregion

        #region ExecuteRequest
        public async Task<ApiRequestResponse> ExecuteRequestAsync(ApiRequest request)
        {
            ConfigureApiTypeAndRequest(request, out ApiType apiType, out string apiRequest);

            var apiResponse = await RawRequestAsync(new ApiCall(apiRequest, apiType, request.Body)
                                {
                                    ExecuteRequestApiCall = true,
                                    SkipCollectionClearing = true,
                                    RawRequest = true,
                                    Headers = request.Headers
                                } , request.HttpMethod).ConfigureAwait(false);

            return new ApiRequestResponse()
            {
                ApiRequest = request,
                Response = apiResponse.Json,
                StatusCode = apiResponse.StatusCode,
                Headers = apiResponse.Headers
            };
        }

        private void ConfigureApiTypeAndRequest(ApiRequest request, out ApiType apiType, out string apiRequest)
        {
            apiType = ApiType.SPORest;
            apiRequest = request.Request;
            switch (request.Type)
            {
                case ApiRequestType.SPORest:
                    {
                        if (apiRequest != null && !apiRequest.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                        {
                            apiRequest = $"{PnPContext.Uri.ToString().TrimEnd(new char[] { '/' })}/{apiRequest}";
                        }
                        break;
                    }
                case ApiRequestType.Graph:
                    {
                        apiType = ApiType.Graph;
                        break;
                    }
                case ApiRequestType.GraphBeta:
                    {
                        apiType = ApiType.GraphBeta;
                        break;
                    }
            }
        }

        public ApiRequestResponse ExecuteRequest(ApiRequest request)
        {
            return ExecuteRequestAsync(request).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<BatchResultValue<string>>> ExecuteRequestBatchAsync(Batch batch, ApiRequest request)
        {
            ConfigureApiTypeAndRequest(request, out ApiType apiType, out string apiRequest);

            var apiCall = new ApiCall(apiRequest, apiType, request.Body)
            {
                ExecuteRequestApiCall = true,
                SkipCollectionClearing = true,
                RawRequest = true,
                Headers = request.Headers,
                RawSingleResult = new BatchResultValue<string>(null),
                RawResultsHandler = (json, apiCall) =>
                {
                    (apiCall.RawSingleResult as BatchResultValue<string>).Value = json;
                }
            };

            var batchRequest = await RawRequestBatchAsync(batch, apiCall, request.HttpMethod).ConfigureAwait(false);

            return new BatchSingleResult<BatchResultValue<string>>(batch, batchRequest.Id, apiCall.RawSingleResult as BatchResultValue<string>);
        }

        public IBatchSingleResult<BatchResultValue<string>> ExecuteRequestBatch(Batch batch, ApiRequest request)
        {
            return ExecuteRequestBatchAsync(batch, request).GetAwaiter().GetResult();
        }

        public async Task<IBatchSingleResult<BatchResultValue<string>>> ExecuteRequestBatchAsync(ApiRequest request)
        {
            return await ExecuteRequestBatchAsync(PnPContext.CurrentBatch, request).ConfigureAwait(false);
        }

        public IBatchSingleResult<BatchResultValue<string>> ExecuteRequestBatch(ApiRequest request)
        {
            return ExecuteRequestBatchAsync(request).GetAwaiter().GetResult();
        }
        #endregion

        #region Load

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual Task LoadAsync(params LambdaExpression[] selectors)
        {
            // Cast expressions
            var newExpressions = selectors.CastExpressions<TModel>();
            return BaseRetrieveAsync(expressions: newExpressions);
        }

        /// <summary>
        /// Loads a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual Task LoadAsync(params Expression<Func<TModel, object>>[] selectors)
        {
            return BaseRetrieveAsync(expressions: selectors);
        }

        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch to use for the current request</param>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual async Task<IBatchResult> LoadBatchAsync(Batch batch, params LambdaExpression[] selectors)
        {
            var newSelectors = selectors.CastExpressions<TModel>();

            return await BaseBatchRetrieveAsync(batch, selectors: newSelectors).ConfigureAwait(false);
        }

        /// <summary>
        /// Batches the load of a Domain Model object from the remote data source, eventually selecting custom properties or using a default set of properties
        /// </summary>
        /// <param name="batch">Batch to use for the current request</param>
        /// <param name="selectors">The properties to select</param>
        /// <returns>The Domain Model object</returns>
        public virtual async Task<IBatchResult> LoadBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] selectors)
        {
            return await BaseBatchRetrieveAsync(batch, default, MappingHandler, PostMappingHandler, selectors).ConfigureAwait(false);
        }

        #endregion

        #region Add
        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="keyValuePairs">Properties to control add</param>
        /// <returns>The added domain model</returns>
        internal virtual async Task<BaseDataModel<TModel>> AddBatchAsync(Batch batch = null, Dictionary<string, object> keyValuePairs = null)
        {
            if (batch == null)
            {
                batch = PnPContext.CurrentBatch;
            }

            if (AddApiCallHandler != null)
            {
                var call = await AddApiCallHandler.Invoke(keyValuePairs).ConfigureAwait(false);
                await BaseAddBatchAsync(batch, call, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
                return this;
            }
            else
            {
                throw new ClientException(ErrorType.MissingAddApiHandler,
                    PnPCoreResources.Exception_MissingAddApiHandler);
            }
        }

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <returns>The added domain model</returns>
        internal virtual async Task<BaseDataModel<TModel>> AddAsync(Dictionary<string, object> keyValuePairs = null)
        {
            if (AddApiCallHandler != null)
            {
                var call = await AddApiCallHandler.Invoke(keyValuePairs).ConfigureAwait(false);
                await BaseAdd(call, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler).ConfigureAwait(false);
                return this;
            }
            else
            {
                throw new ClientException(ErrorType.MissingAddApiHandler,
                    PnPCoreResources.Exception_MissingAddApiHandler);
            }
        }

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        /// <param name="keyValuePairs">Properties to control add</param>
        /// <returns>The added domain model</returns>
        internal virtual BaseDataModel<TModel> AddBatch(Batch batch = null, Dictionary<string, object> keyValuePairs = null)
        {
            return AddBatchAsync(batch, keyValuePairs).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a domain model instance
        /// </summary>
        /// <returns>The added domain model</returns>
        internal virtual BaseDataModel<TModel> Add(Dictionary<string, object> keyValuePairs = null)
        {
            return AddAsync(keyValuePairs).GetAwaiter().GetResult();
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates a domain model
        /// </summary>
        public virtual async Task UpdateBatchAsync()
        {
            await UpdateBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public virtual async Task UpdateBatchAsync(Batch batch)
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
        public virtual async Task DeleteBatchAsync()
        {
            await DeleteBatchAsync(PnPContext.CurrentBatch).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a domain model
        /// </summary>
        /// <param name="batch">Batch add this request to</param>
        public virtual async Task DeleteBatchAsync(Batch batch)
        {
            await BaseDeleteBatchAsync(batch).ConfigureAwait(false);
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

        #region Internal core data access implementation

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

        internal virtual async Task BaseRetrieveAsync(ApiCall apiOverride = default, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null, params Expression<Func<TModel, object>>[] expressions)
        {
            // DONE: Accordingly to the suggested above refactoring,
            // I would rename this method into BaseRequest/BaseRetrieve/???
            // to avoid a reference to the Get concept, which is something else
            // ApiCall is structure and we always create a new type

            fromJsonCasting ??= MappingHandler;
            postMappingJson ??= PostMappingHandler;

            // Get entity information for the entity to load
            var entityInfo = GetClassInfo(expressions);
            // Construct the API call to make
            var api = await QueryClient.BuildGetAPICallAsync(this, entityInfo, apiOverride, loadPages: true).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Get, api.ApiCall, default, fromJsonCasting, postMappingJson, "Get");

            // The domain model for Graph can have non expandable collections, hence these require an additional API call to populate. 
            // Let's ensure these additional API calls's are included in a single batch
            if (api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta)
            {
                await QueryClient.AddGraphBatchRequestsForNonExpandableCollectionsAsync(this, batch, entityInfo, expressions, fromJsonCasting, postMappingJson).ConfigureAwait(false);
            }

            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates and adds a Get request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiOverride">Override for the API call</param>
        /// <param name="selectors">Expressions needed to create the request</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task<IBatchSingleResult<TModel>> BaseBatchRetrieveAsync(Batch batch, ApiCall apiOverride = default, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null, params Expression<Func<TModel, object>>[] selectors)
        {
            // DONE: Accordingly to the suggested above refactoring,
            // I would rename this method into BaseRequest/BaseRetrieve/???
            // to avoid a reference to the Get concept, which is something else
            // ApiCall is structure and we always create a new type

            fromJsonCasting ??= MappingHandler;
            postMappingJson ??= PostMappingHandler;
            batch ??= PnPContext.CurrentBatch;

            // Get entity information for the entity to load
            var entityInfo = GetClassInfo(selectors);
            // Construct the API call to make
            var api = await QueryClient.BuildGetAPICallAsync(this, entityInfo, apiOverride, loadPages: true).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return BatchSingleResult<TModel>.None;
            }

            // Also try to build the rest equivalent, this will be used in case we encounter mixed rest/graph batches
            ApiCallRequest apiRestBackup = new ApiCallRequest(default);
            // Only build a backup call when the type can be requested via SharePoint REST
            if ((api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta) && !string.IsNullOrEmpty(entityInfo.SharePointType))
            {
                // Try to get the API call, but this time using rest
                apiRestBackup = await QueryClient.BuildGetAPICallAsync(this, entityInfo, apiOverride, true, loadPages: true).ConfigureAwait(false);
            }

            Guid batchRequestId = batch.Add(this, entityInfo, HttpMethod.Get, api.ApiCall, apiRestBackup.ApiCall, fromJsonCasting, postMappingJson, "GetBatch");

            // The domain model for Graph can have non expandable collections, hence these require an additional API call to populate. 
            // Let's ensure these additional API calls's are included in a single batch
            if (api.ApiCall.Type == ApiType.Graph || api.ApiCall.Type == ApiType.GraphBeta)
            {
                await QueryClient.AddGraphBatchRequestsForNonExpandableCollectionsAsync(this, batch, entityInfo, selectors, fromJsonCasting, postMappingJson).ConfigureAwait(false);
            }

            return new BatchSingleResult<TModel>(batch, batchRequestId);
        }

        private void ApiCancellationMessage(ApiCallRequest api)
        {
            Log.LogInformation(PnPCoreResources.Log_Information_ApiCallCancelled, api.ApiCall.Request, api.CancellationReason);
        }

        async Task IMetadataExtensible.SetGraphToRestMetadataAsync()
        {
            await GraphToRestMetadataAsync().ConfigureAwait(false);
        }

        async Task IMetadataExtensible.SetRestToGraphMetadataAsync()
        {
            await RestToGraphMetadataAsync().ConfigureAwait(false);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal virtual async Task GraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // No default logic, needs to be implemented in the actual model class by overriding this method
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        internal virtual async Task RestToGraphMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // No default logic, needs to be implemented in the actual model class by overriding this method
        }

        #endregion

        #region Add

        /// <summary>
        /// API call handler for add requests
        /// </summary>
        internal AddApiCall AddApiCallHandler { get; set; }

        /// <summary>
        /// Creates and adds a Post request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="postApiCall">Api call to execute</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseAddBatchAsync(Batch batch, ApiCall postApiCall, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            var parent = TokenHandler.GetParentDataModel(this);

            if (parent is IRequestable && !(parent as IRequestable).Requested && batch.Requests.Count > 0)
            {
                throw new ClientException(ErrorType.UnsupportedViaBatch,
                    PnPCoreResources.Exception_Unsupported_ViaBatch);
            }

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
            batch.Add(this, entityInfo, HttpMethod.Post, postApiCall, default, fromJsonCasting, postMappingJson, "AddBatch");
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
                throw new ClientException(ErrorType.GraphBetaNotAllowed,
                    PnPCoreResources.Exception_GraphBetaNotAllowed);
            }

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, HttpMethod.Post, postApiCall, default, fromJsonCasting, postMappingJson, "Add");
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

        [SystemProperty]
        internal Action<ExpandoObject> ExpandUpdatePayLoad { get; set; } = null;

        /// <summary>
        /// Creates and adds an Update request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseBatchUpdateAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await QueryClient.BuildUpdateAPICallAsync(this, entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            batch.Add(this, entityInfo, new HttpMethod("PATCH"), api.ApiCall, default, fromJsonCasting, postMappingJson, "UpdateBatch");
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
            var api = await QueryClient.BuildUpdateAPICallAsync(this, entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, new HttpMethod("PATCH"), api.ApiCall, default, fromJsonCasting, postMappingJson, "Update");
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
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
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="fromJsonCasting">Delegate for type mapping when the request is executed</param>
        /// <param name="postMappingJson">Delegate for post mapping</param>
        internal virtual async Task BaseDeleteBatchAsync(Batch batch, Func<FromJson, object> fromJsonCasting = null, Action<string> postMappingJson = null)
        {
            // Get entity information for the entity to update
            var entityInfo = GetClassInfo();
            // Construct the API call to make
            var api = await QueryClient.BuildDeleteAPICallAsync(this, entityInfo).ConfigureAwait(false);

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            // Also try to build the rest equivalent, this will be used in case we encounter mixed rest/graph batches
            batch.Add(this, entityInfo, HttpMethod.Delete, api.ApiCall, default, fromJsonCasting, postMappingJson, "DeleteBatch");
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
            var api = await QueryClient.BuildDeleteAPICallAsync(this, entityInfo).ConfigureAwait(false);

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();

            if (api.Cancelled)
            {
                ApiCancellationMessage(api);
                return;
            }

            batch.Add(this, entityInfo, HttpMethod.Delete, api.ApiCall, default, fromJsonCasting, postMappingJson, "Delete");
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
        }

        #endregion

        #region Generic

        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        internal async Task RequestBatchAsync(Batch batch, ApiCall apiCall, HttpMethod method, [CallerMemberName] string operationName = null)
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
            batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, CleanupOperationName(operationName));
        }

        /// <summary>
        /// Executes a given request
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        internal async Task<Batch> RequestAsync(ApiCall apiCall, HttpMethod method, [CallerMemberName] string operationName = null)
        {
            EntityInfo entityInfo = null;

            if (!apiCall.ExecuteRequestApiCall)
            {
                // Get entity information for the entity to update
                entityInfo = GetClassInfo();

                // Prefix API request with context url if needed
                apiCall = PrefixApiCall(apiCall, entityInfo);

                // Ensure there's no Graph beta endpoint being used when that was not allowed
                if (!CanUseGraphBetaForRequest(apiCall, entityInfo))
                {
                    throw new ClientException(ErrorType.GraphBetaNotAllowed,
                        PnPCoreResources.Exception_GraphBetaNotAllowed);
                }
            }

            // Ensure token replacement is done
            apiCall.Request = await TokenHandler.ResolveTokensAsync(this, apiCall.Request, PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            var batch = PnPContext.BatchClient.EnsureBatch();
            batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, CleanupOperationName(operationName));
            await PnPContext.BatchClient.ExecuteBatch(batch).ConfigureAwait(false);
            return batch;
        }

        /// <summary>
        /// Adds a request to the given batch
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        internal async Task<BatchRequest> RawRequestBatchAsync(Batch batch, ApiCall apiCall, HttpMethod method, [CallerMemberName] string operationName = null)
        {
            EntityInfo entityInfo = null;

            if (!apiCall.ExecuteRequestApiCall)
            {
                // Get entity information for the entity to update
                entityInfo = GetClassInfo();
                
                // Mark request as raw
                apiCall.RawRequest = true;

                // Prefix API request with context url if needed
                apiCall = PrefixApiCall(apiCall, entityInfo);

                // Ensure there's no Graph beta endpoint being used when that was not allowed
                if (!CanUseGraphBetaForRequest(apiCall, entityInfo))
                {
                    return null;
                }
            }

            // Ensure token replacement is done
            apiCall.Request = await TokenHandler.ResolveTokensAsync(this, apiCall.Request, PnPContext).ConfigureAwait(false);

            // Add the request to the batch
            Guid batchRequestId = batch.Add(this, entityInfo, method, apiCall, default, fromJsonCasting: MappingHandler, postMappingJson: PostMappingHandler, CleanupOperationName(operationName));
            return batch.GetRequest(batchRequestId);
        }

        /// <summary>
        /// Executes a given request, not loading the response json in the model but simply returning it
        /// </summary>
        /// <param name="apiCall">Api call to execute</param>
        /// <param name="method"><see cref="HttpMethod"/> to use for this request</param>
        /// <param name="operationName">Name of the operation, used for telemetry purposes</param>
        internal async Task<ApiCallResponse> RawRequestAsync(ApiCall apiCall, HttpMethod method, [CallerMemberName] string operationName = null)
        {
            // Mark request as raw
            apiCall.RawRequest = true;

            var batch = await RequestAsync(apiCall, method, CleanupOperationName(operationName)).ConfigureAwait(false);
            var batchFirstRequest = batch.Requests.First().Value;
            return new ApiCallResponse(apiCall,
                                       batchFirstRequest.ResponseJson,
                                       batchFirstRequest.ResponseHttpStatusCode,
                                       batch.Id,
                                       batchFirstRequest.ResponseHeaders,
                                       binaryContent: apiCall.ExpectBinaryResponse ? batchFirstRequest.ResponseBinaryContent : null);
        }

        private static string CleanupOperationName(string operationName)
        {
            if (!string.IsNullOrEmpty(operationName))
            {
                if (operationName.EndsWith("Async"))
                {
                    return operationName.Substring(0, operationName.Length - 5);
                }
                else
                {
                    return operationName;
                }
            }

            return "N/A";
        }

        private ApiCall PrefixApiCall(ApiCall apiCall, EntityInfo entityInfo)
        {
            if (!string.IsNullOrEmpty(entityInfo.SharePointType) && (apiCall.Type == ApiType.SPORest || apiCall.Type == ApiType.CSOM))
            {
                // The request is populated and already has a fully qualified url
                if (apiCall.Request != null && apiCall.Request.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    return apiCall;
                }

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
        internal EntityInfo GetClassInfo(params Expression<Func<TModel, object>>[] expressions)
        {
            return EntityManager.GetClassInfo(GetType(), this, expressions: expressions);
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

        private readonly Dictionary<string, bool> navigationPropertyInstantiated = new Dictionary<string, bool>();

        protected internal bool NavigationPropertyInstantiated([CallerMemberName] string propertyName = "")
        {
            if (navigationPropertyInstantiated.ContainsKey(propertyName))
            {
                return navigationPropertyInstantiated[propertyName];
            }

            return false;
        }

        protected internal void InstantiateNavigationProperty([CallerMemberName] string propertyName = "")
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

        protected internal T GetModelValue<T>([CallerMemberName] string propertyName = "")
        {
            if (!NavigationPropertyInstantiated(propertyName))
            {
                var propertyValue = EntityManager.GetEntityConcreteInstance(typeof(T), this);
                (propertyValue as IDataModelWithContext).PnPContext = PnPContext;

                SetValue(propertyValue, propertyName);
                InstantiateNavigationProperty(propertyName);
            }
            return GetValue<T>(propertyName);
        }

        protected internal void SetModelValue<T>(T value, [CallerMemberName] string propertyName = "")
        {
            InstantiateNavigationProperty(propertyName);
            SetValue(value, propertyName);
        }

        protected internal T GetModelCollectionValue<T>([CallerMemberName] string propertyName = "")
        {
            if (!HasValue(propertyName))
            {
                var collection = EntityManager.GetEntityCollectionConcreteInstance(typeof(T), PnPContext, this, propertyName);
                SetValue(collection, propertyName);
            }
            return GetValue<T>(propertyName);
        }

        #endregion

        #region Parent traversal logic

        internal IDataModelParent GetParentByType(Type parentType, IDataModelParent parent = null)
        {

            if (Parent == null)
            {
                return null;
            }

            if (parent == null)
            {
                parent = Parent;
            }

            // Bingo, we're good
            if (parent.GetType() == parentType)
            {
                return parent;
            }
            // Let's check the parent
            else if (parent.Parent != null)
            {
                return GetParentByType(parentType, parent.Parent);
            }
            // We're at the top of the tree...nothing found afterall
            else
            {
                return null;
            }
        }

        #endregion

        #region Check properties logic

        public bool IsPropertyAvailable(Expression<Func<TModel, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var body = expression.Body as MemberExpression ?? ((UnaryExpression)expression.Body).Operand as MemberExpression;

            if (body.Expression is MemberExpression)
            {
                throw new ClientException(ErrorType.Unsupported, PnPCoreResources.Exception_PropertyNotLoaded_NestedProperties);
            }

            if (HasValue(body.Member.Name))
            {
                if (GetValue(body.Member.Name) is IRequestable)
                {
                    if ((GetValue(body.Member.Name) as IRequestable).Requested)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public void EnsureProperties(params Expression<Func<TModel, object>>[] expressions)
        {
            EnsurePropertiesAsync(expressions).GetAwaiter().GetResult();
        }

        public async Task EnsurePropertiesAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            var dirty = false;
            List<Expression<Func<TModel, object>>> expressionsToLoad = VerifyProperties(this, expressions, ref dirty);

            if (dirty)
            {
                await BaseRetrieveAsync(expressions: expressionsToLoad.ToArray()).ConfigureAwait(false);
            }
        }

        public bool ArePropertiesAvailable(params Expression<Func<TModel, object>>[] expressions)
        {
            var dirty = false;
            VerifyProperties(this, expressions, ref dirty);

            return !dirty;
        }

        private static List<Expression<Func<TModel, object>>> VerifyProperties(IDataModel<TModel> model, Expression<Func<TModel, object>>[] expressions, ref bool dirty)
        {
            List<Expression<Func<TModel, object>>> expressionsToLoad = new List<Expression<Func<TModel, object>>>();
            foreach (Expression<Func<TModel, object>> expression in expressions)
            {
                if (expression.Body.NodeType == ExpressionType.Call && expression.Body is MethodCallExpression)
                {
                    // Future use? (includes)
                    var body = (MethodCallExpression)expression.Body;
                    if (body.Method.Name == "QueryProperties")
                    {
                        if (body.Arguments.Count != 2)
                        {
                            throw new Exception(PnPCoreResources.Exception_InvalidArgumentsNumber);
                        }

                        // Parse the expressions and get the relevant entity information
                        var entityInfo = EntityManager.GetClassInfo(model.GetType(), (model as BaseDataModel<TModel>), expressions: expressions);

                        string fieldToLoad = ((expression.Body as MethodCallExpression).Arguments[0] as MemberExpression).Member.Name;

                        var collectionToCheck = entityInfo.Fields.FirstOrDefault(p => p.Name == fieldToLoad);
                        if (collectionToCheck != null)
                        {
                            var collection = model.GetPublicInstancePropertyValue(fieldToLoad);
                            if (collection is IRequestableCollection)
                            {
                                if (!(collection as IRequestableCollection).Requested)
                                {
                                    // Collection was not requested at all, so let's load it again
                                    expressionsToLoad.Add(expression);
                                    dirty = true;
                                }
                                else
                                {
                                    if (collectionToCheck.ExpandFieldInfo != null && (collection as IRequestableCollection).Length > 0)
                                    {
                                        // Collection was requested and there's at least one item to check, let's see if we can figure out if all needed properties were loaded as well
                                        if (!WereFieldsRequested(collectionToCheck, collectionToCheck.ExpandFieldInfo, collection as IRequestableCollection))
                                        {
                                            expressionsToLoad.Add(expression);
                                            dirty = true;
                                        }
                                    }
                                    else
                                    {
                                        expressionsToLoad.Add(expression);
                                        dirty = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            expressionsToLoad.Add(expression);
                            dirty = true;
                        }
                    }
                    else
                    {
                        throw new ClientException(ErrorType.PropertyNotLoaded,
                            PnPCoreResources.Exception_PropertyNotLoaded_OnlyQueryPropertiesSupported);
                    }
                }
                else if (!model.IsPropertyAvailable(expression))
                {
                    // Property was not available, add it the expressions to load
                    expressionsToLoad.Add(expression);
                    dirty = true;
                }
            }

            return expressionsToLoad;
        }

        private static bool WereFieldsRequested(EntityFieldInfo collectionToCheck, EntityFieldExpandInfo expandFields, IRequestableCollection collection)
        {
            var enumerator = collection.RequestedItems.GetEnumerator();

            enumerator.Reset();
            enumerator.MoveNext();

            // If no item available or collection was not requested then we need to reload
            if (!collection.Requested || enumerator.Current == null)
            {
                return false;
            }

            TransientObject itemToCheck = enumerator.Current as TransientObject;

            foreach (var fieldInExpression in expandFields.Fields)
            {
                if (!fieldInExpression.Fields.Any())
                {
                    if (!itemToCheck.HasValue(fieldInExpression.Name))
                    {
                        return false;
                    }
                }
                else
                {
                    // this is another collection, so perform a recursive check
                    var collectionRecursive = itemToCheck.GetPublicInstancePropertyValue(fieldInExpression.Name) as IRequestableCollection;
                    if (!WereFieldsRequested(collectionToCheck, fieldInExpression, collectionRecursive))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
