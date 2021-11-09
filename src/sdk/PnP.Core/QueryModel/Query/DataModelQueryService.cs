using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{
    internal sealed class DataModelQueryService<TModel>
    {
        public PnPContext PnPContext { get; }

        public IDataModelParent Parent { get; }

        public string MemberName { get; }

        internal EntityInfo EntityInfo { get; set; }

        /// <summary>
        /// Protected default constructor, to force creation using
        /// the PnPContext instance
        /// </summary>
        internal DataModelQueryService()
        {
        }

        /// <summary>
        /// Constructor based on a PnPContext instance
        /// </summary>
        /// <param name="pnPContext">The PnPContext instance to use for executing the queries</param>
        /// <param name="parent">The parent Domain Model object for the current query</param>
        /// <param name="memberName">Optional name of the member behind this query service</param>
        public DataModelQueryService(PnPContext pnPContext, IDataModelParent parent, string memberName)
        {
            PnPContext = pnPContext;
            Parent = parent;
            MemberName = memberName;
        }

        public async Task<BatchRequest> AddToBatchAsync(ODataQuery<TModel> query, Batch batch)
        {
            // We always refresh the EntityInfo object accordingly to the current query
            EntityInfo = EntityManager.GetClassInfo(typeof(TModel), null, Parent, query.Fields.ToArray());

            var batchParent = Parent;

            // In case a model can be used from different contexts (e.g. ContentType can be used from Web, but also from List)
            // it's required to let the entity know this context so that it can provide the correct information when requested
            if (batchParent != null)
            {
                // Replicate the parent object in order to keep original collection as is
                batchParent = EntityManager.ReplicateParentHierarchy(batchParent, PnPContext);
                EntityInfo.Target = batchParent.GetType();
            }

            // and its concrete instance
            var concreteEntity = EntityManager.GetEntityConcreteInstance(typeof(TModel), batchParent);
            // Ensure the passed model has a PnPContext set
            if (concreteEntity is IDataModelWithContext d && d.PnPContext == null)
            {
                d.PnPContext = PnPContext;
            }

            // Get the parent (container) entity info
            var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(batchParent.GetType());
            // and cast it to the IDataModelMappingHandler interface
            var parentEntityWithMappingHandlers = (IDataModelMappingHandler)batchParent;

            // Build the needed API call
            ApiCallRequest apiCallRequest = await QueryClient.BuildGetAPICallAsync(concreteEntity as BaseDataModel<TModel>, EntityInfo, query, default, false, true).ConfigureAwait(false);
            var apiCall = apiCallRequest.ApiCall;
            apiCall.ReceivingProperty = MemberName;

            // Add the request to the current batch
            Guid batchRequestId = batch.Add(
                batchParent as TransientObject,
                parentEntityInfo,
                HttpMethod.Get,
                apiCall,
                default,
                parentEntityWithMappingHandlers.MappingHandler,
                parentEntityWithMappingHandlers.PostMappingHandler,
                "Linq"
                );

            PnPContext.RequestModules?.Clear();

            return batch.GetRequest(batchRequestId);
        }

        public async Task<object> ExecuteQueryAsync(Type expressionType, ODataQuery<TModel> query, CancellationToken token)
        {
            if (string.IsNullOrEmpty(MemberName))
            {
                throw new ClientException(ErrorType.LinqError,
                    string.Format(PnPCoreResources.Exception_LinqError_MissingValue, nameof(MemberName), GetType().Name));
            }

            // At this point in time we support querying collections for which the model implements IQueryableDataModel
            if (typeof(TModel).ImplementsInterface(typeof(IQueryableDataModel)))
            {
                // Prepare request and add to the current batch
                BatchRequest batchRequest = await AddToBatchAsync(query, PnPContext.CurrentBatch).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();

                // Cleanup the collection of results, before loading the new results
                var collection = batchRequest.Model.GetPublicInstancePropertyValue(MemberName) as IRequestableCollection;
                collection.Clear();

                // and execute the request
                await PnPContext.ExecuteAsync().ConfigureAwait(false);
                token.ThrowIfCancellationRequested();

                // If the expression type implements IQueryable, we need to return
                // the whole collection of results
                if (expressionType.ImplementsInterface(typeof(IQueryable)))
                {
                    return GetAsyncEnumerable(collection, query, batchRequest, token);
                }
                // Otherwise if the expression type is the type of TModel, we need
                // to return a single item
                else
                {
                    // In case we need to retrieve just one item make
                    // sure that the result will be just one item
                    if (query.Top == 1)
                    {
                        // Get the resulting property from the parent object
                        var resultValue = (IEnumerable<TModel>)collection.RequestedItems;

                        // Here as well it will suffice to return FirstOrDefault without 
                        // any specific predicate
                        return resultValue.FirstOrDefault();
                    }
                    else
                    {
                        return default(TModel);
                    }
                }
            }
            // For what is not supported, we return an empty collection
            else
            {
                return Enumerable.Empty<TModel>();
            }
        }

        private async IAsyncEnumerable<TModel> GetAsyncEnumerable(IRequestableCollection collection, ODataQuery<TModel> query, BatchRequest originalBatchRequest, [EnumeratorCancellation] CancellationToken token)
        {
            // Due some Graph limitations, ODataQuery can choose to implement skip on client side
            bool applyClientSkip = !originalBatchRequest.ApiCall.Request.Contains("$skip");

            // Special case for items of the list which does't support skip
            if (!applyClientSkip && query.Skip.HasValue && collection is Model.SharePoint.IListItemCollection)
            {
                throw new InvalidOperationException(PnPCoreResources.Exception_Unsupported_SkipOnItems);
            }

            int count = 0;
            while (true)
            {
                // Get the resulting property from the parent object
                var requestedItems = (IEnumerable<TModel>)collection.RequestedItems;

                foreach (var item in requestedItems)
                {
                    token.ThrowIfCancellationRequested();

                    count++;

                    // Skip items
                    if (applyClientSkip && query.Skip.HasValue && query.Skip > count) continue;

                    // Stop to enumerate or load other pages
                    if (query.Top.HasValue && count > query.Top) yield break;

                    // Return current item
                    yield return item;
                }

                // Check if collection supports pagination
                var pageableCollection = collection as ISupportPaging;
                var typedCollection = collection as BaseDataModelCollection<TModel>;

                if (pageableCollection == null || // If the result set is not pageable
                    !pageableCollection.CanPage || // or if the result set is pageable, but there is no support for paging (no nextLink metadata)
                    (typedCollection != null && originalBatchRequest.ApiCall.Type == ApiType.SPORest && pageableCollection.CanPage && !query.Top.HasValue && count < typedCollection.PnPContext.GlobalOptions.HttpSharePointRestDefaultPageSize) || // or if the result set comes from SPO REST, is pageable, there is support for nextLink, there is no Top constraint and but the results are the whole result set
                    (query.Top.HasValue && count >= query.Top)) // or there is Top constraint and the result set is bigger than the Top constraint
                {
                    yield break;
                }

                // Prepare api call
                IMetadataExtensible metadataExtensible = (IMetadataExtensible)pageableCollection;
                (var nextLink, var nextLinkApiType) = QueryClient.BuildNextPageLink(metadataExtensible);

                // Clear the MetaData paging links to avoid loading the collection again via paging
                metadataExtensible.Metadata.Remove(PnPConstants.GraphNextLink);
                metadataExtensible.Metadata.Remove(PnPConstants.SharePointRestListItemNextLink);

                // Create a request for the next page
                PnPContext.CurrentBatch.Add(
                    originalBatchRequest.Model,
                    originalBatchRequest.EntityInfo,
                    HttpMethod.Get,
                    new ApiCall
                    {
                        Type = nextLinkApiType,
                        ReceivingProperty = originalBatchRequest.ApiCall.ReceivingProperty,
                        Request = nextLink,
                        LoadPages = true
                    },
                    default,
                    originalBatchRequest.FromJsonCasting,
                    originalBatchRequest.PostMappingJson,
                    "GetNextPage"
                );

                // Clear collection, in order to not fill it
                collection.Clear();

                PnPContext.RequestModules?.Clear();

                // and execute the request
                await PnPContext.ExecuteAsync().ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
