using PnP.Core.Model;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.QueryModel
{
    internal class DataModelQueryService<TModel>
    {
        public PnPContext PnPContext { get; }

        public IDataModelParent Parent { get; }

        public string MemberName { get; }

        internal EntityInfo EntityInfo { get; set; }

        /// <summary>
        /// Protected default constructor, to force creation using
        /// the PnPContext instance
        /// </summary>
        protected DataModelQueryService()
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

        public async Task<BatchRequest> AddToCurrentBatchAsync(ODataQuery<TModel> query)
        {
            // We always refresh the EntityInfo object accordingly to the current query
            EntityInfo = EntityManager.GetClassInfo<TModel>(typeof(TModel), null, this.Parent, query.Fields.ToArray());

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
            ApiCallRequest apiCallRequest = await QueryClient.BuildGetAPICallAsync<TModel>(concreteEntity as BaseDataModel<TModel>, EntityInfo, query, default, false, true).ConfigureAwait(false);
            var apiCall = apiCallRequest.ApiCall;
            apiCall.ReceivingProperty = MemberName;

            // Add the request to the current batch
            Guid batchRequestId = PnPContext.CurrentBatch.Add(
                batchParent as TransientObject,
                parentEntityInfo,
                HttpMethod.Get,
                apiCall,
                default,
                parentEntityWithMappingHandlers.MappingHandler,
                parentEntityWithMappingHandlers.PostMappingHandler,
                "Linq"
                );

            return PnPContext.CurrentBatch.GetRequest(batchRequestId);
        }

        public async Task<object> ExecuteQueryAsync(Type expressionType, ODataQuery<TModel> query)
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
                BatchRequest batchRequest = await AddToCurrentBatchAsync(query).ConfigureAwait(false);

                // Cleanup the collection of results, before loading the new results
                var collection = batchRequest.Model.GetPublicInstancePropertyValue(MemberName) as IRequestableCollection;
                collection.Clear();

                // and execute the request
                await PnPContext.ExecuteAsync().ConfigureAwait(false);

                // Get the resulting property from the parent object
                var resultValue = (IEnumerable<TModel>)collection.RequestedItems;

                // If the expression type implements IQueryable, we need to return
                // the whole collection of results
                if (expressionType.ImplementsInterface(typeof(IQueryable)))
                {
                    if (query.Top > 0 && !query.Skip.HasValue)
                    {
                        // Here will suffice to return the first N items
                        return resultValue.Take(query.Top.Value);
                    }
                    else if (query.Top > 0 && query.Skip > 0)
                    {
                        // Here will return the first N items, skipping the Z items
                        return resultValue.Skip(query.Skip.Value).Take(query.Top.Value);
                    }
                    if (!query.Top.HasValue && query.Skip > 0)
                    {
                        // Here will suffice to return the N items from Skip offeset
                        return resultValue.Skip(query.Skip.Value);
                    }
                    else
                    {
                        // With the new querying model, where we always create a new container
                        // as such, we simply return the whole set of results
                        return resultValue;
                    }
                }
                // Otherwise if the expression type is the type of TModel, we need
                // to return a single item
                else
                {
                    // In case we need to retrieve just one item make
                    // sure that the result will be just one item
                    if (query.Top == 1)
                    {
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
    }
}
