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
        private readonly PnPContext context;
        private readonly IDataModelParent parent;
        private readonly string memberName;

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
        /// <param name="context">The PnPContext instance to use for executing the queries</param>
        /// <param name="parent">The parent Domain Model object for the current query</param>
        /// <param name="memberName">Optional name of the member behind this query service</param>
        public DataModelQueryService(PnPContext context, IDataModelParent parent, string memberName)
        {
            this.context = context;
            this.parent = parent;
            this.memberName = memberName;
        }

        internal EntityInfo EntityInfo { get; set; }

        public async Task<Guid> AddToCurrentBatchAsync(Type expressionType, ODataQuery<TModel> query)
        {
            // Get the entity info, depending on how we enter EntityInfo was already created
            if (EntityInfo == null)
            {
                EntityInfo = EntityManager.GetClassInfo<TModel>(typeof(TModel), null, query.Fields.ToArray());
            }

            // In case a model can be used from different contexts (e.g. ContentType can be used from Web, but also from List)
            // it's required to let the entity know this context so that it can provide the correct information when requested
            if (parent != null)
            {
                EntityInfo.Target = parent.GetType();
            }

            // and its concrete instance
            var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), parent);
            // Ensure the passed model has a PnPContext set
            if (concreteEntity is IDataModelWithContext d && d.PnPContext == null)
            {
                d.PnPContext = context;
            }

            // Get the parent (container) entity info
            var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(parent.GetType());
            // and cast it to the IDataModelMappingHandler interface
            var parentEntityWithMappingHandlers = (IDataModelMappingHandler)parent;


            // Build the needed API call
            ApiCallRequest apiCallRequest = await QueryClient.BuildGetAPICallAsync<TModel>(concreteEntity as BaseDataModel<TModel>, EntityInfo, query, default, false, true).ConfigureAwait(false);
            var apiCall = apiCallRequest.ApiCall;
            apiCall.ReceivingProperty = memberName;

            // Add the request to the current batch
            return context.CurrentBatch.Add(
                parent as TransientObject,
                parentEntityInfo,
                HttpMethod.Get,
                apiCall,
                default,
                parentEntityWithMappingHandlers.MappingHandler,
                parentEntityWithMappingHandlers.PostMappingHandler,
                "Linq"
                );
        }

        public async Task<object> ExecuteQueryAsync(Type expressionType, ODataQuery<TModel> query)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ClientException(ErrorType.LinqError,
                    string.Format(PnPCoreResources.Exception_LinqError_MissingValue, nameof(memberName), GetType().Name));
            }

            // At this point in time we support querying collections for which the model implements IQueryableDataModel
            if (typeof(TModel).ImplementsInterface(typeof(IQueryableDataModel)))
            {
                // Prepare request and add to the current batch
                var batchRequestId = await AddToCurrentBatchAsync(expressionType, query);

                // and execute the request
                await context.ExecuteAsync().ConfigureAwait(false);

                // Get the resulting property from the parent object
                var resultValue = parent.GetPublicInstancePropertyValue(memberName) as IEnumerable<TModel>;

                // If the expression type implements IQueryable, we need to return
                // the whole collection of results
                if (expressionType.ImplementsInterface(typeof(IQueryable)))
                {
                    // TODO: With the new querying model, where we always create a new container
                    // for the result, here we don't need anymore to filter by BatchRequestId
                    // but we can simply return the result
                    return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
                }
                // Otherwise if the expression type is the type of TModel, we need
                // to return a single item
                else
                {
                    // In case we need to retrieve just one item make
                    // sure that the result will be just one item
                    if (query.Top == 1)
                    {
                        // TODO: Here as well it will suffice to return FirstOrDefault without 
                        // any specific predicate
                        return resultValue.FirstOrDefault(p => (p as TransientObject).BatchRequestId == batchRequestId);
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
