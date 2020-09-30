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

        public async Task<object> ExecuteQueryAsync(Type expressionType, ODataQuery<TModel> query)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ClientException(ErrorType.LinqError, 
                    string.Format(PnPCoreResources.Exception_LinqError_MissingValue, nameof(memberName), GetType().Name));
            }

            // At this point in time we support querying collections for which the model implements IQueryableModel
            if (typeof(TModel).ImplementsInterface(typeof(IQueryableDataModel)))
            {
                // Get the entity info
                var entityInfo = EntityManager.GetClassInfo<TModel>(typeof(TModel), null);

                // In case a model can be used from different contexts (e.g. ContentType can be used from Web, but also from List)
                // it's required to let the entity know this context so that it can provide the correct information when requested
                if (parent != null)
                {
                    entityInfo.Target = parent.GetType();
                }

                // and its concrete instance
                var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), parent);

                // Get the parent (container) entity info
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(parent.GetType());
                // and cast it to the IDataModelMappingHandler interface
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)parent;

                // Prepare a variable to hold tha batch request ID
                Guid batchRequestId = Guid.Empty;

                // Verify if we're not asking fields which anyhow cannot (yet) be served via Graph
                bool canUseGraph = true;
                // Ensure the model's keyfield was requested, this is needed to ensure the loaded model can be added/merged into an existing collection
                // Only do this when there was no field filtering, without filtering all default fields are anyhow returned
                if (query.Select.Any())
                {
                    if (!query.Select.Contains(entityInfo.ActualKeyFieldName))
                    {
                        query.Select.Add(entityInfo.ActualKeyFieldName);
                    }
                    
                    foreach (var selectProperty in query.Select)
                    {
                        var field = entityInfo.Fields.FirstOrDefault(p => p.Name == selectProperty);
                        if (string.IsNullOrEmpty(field.GraphName))
                        {
                            canUseGraph = false;
                            break;
                        }
                    }
                }

                // We try to use Graph First, if selected by the user and if the query is supported by Graph
                if (canUseGraph && context.GraphFirst && !string.IsNullOrEmpty(entityInfo.GraphLinqGet))
                {
                    // Ensure that selected properties which are marked as expandable are also used in that manner
                    foreach (var selectProperty in query.Select)
                    {
                        var prop = entityInfo.Fields.FirstOrDefault(p => p.Name == selectProperty);
                        if (prop != null && !string.IsNullOrEmpty(prop.GraphName) && prop.GraphExpandable && !query.Expand.Contains(prop.GraphName))
                        {
                            query.Expand.Add(prop.GraphName);
                        }
                    }

                    // Build the Graph request URL
                    var requestUrl = $"{entityInfo.GraphLinqGet}?{query.ToQueryString(ODataTargetPlatform.Graph, urlEncode: false)}";
                    requestUrl = await TokenHandler.ResolveTokensAsync(concreteEntity as IMetadataExtensible, requestUrl, context).ConfigureAwait(false);

                    // Add the request to the current batch
                    batchRequestId = context.CurrentBatch.Add(
                        parent as TransientObject,
                        parentEntityInfo,
                        HttpMethod.Get,
                        new ApiCall // First option is Graph
                        {
                            Type = entityInfo.GraphBeta ? ApiType.GraphBeta : ApiType.Graph,
                            ReceivingProperty = memberName,
                            Request = requestUrl
                        },
                        default,
                        parentEntityWithMappingHandlers.MappingHandler,
                        parentEntityWithMappingHandlers.PostMappingHandler
                        );
                }
                else
                {
                    // Ensure that selected properties which are marked as expandable are also used in that manner
                    foreach (var selectProperty in query.Select)
                    {
                        var prop = entityInfo.Fields.FirstOrDefault(p => p.Name == selectProperty);
                        if (prop != null && !string.IsNullOrEmpty(prop.SharePointName) && prop.SharePointExpandable && !query.Expand.Contains(prop.SharePointName))
                        {
                            query.Expand.Add(prop.SharePointName);
                        }
                    }

                    // Build the SPO REST request URL
                    var requestUrl = $"{context.Uri.ToString().TrimEnd('/')}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";
                    requestUrl = await TokenHandler.ResolveTokensAsync(concreteEntity as IMetadataExtensible, requestUrl).ConfigureAwait(false);

                    // Add the request to the current batch
                    batchRequestId = context.CurrentBatch.Add(
                        parent as TransientObject,
                        parentEntityInfo,
                        HttpMethod.Get,
                        new ApiCall // Secondo option is SPO REST
                        {
                            Type = ApiType.SPORest,
                            ReceivingProperty = memberName,
                            Request = requestUrl
                        },
                        default,
                        parentEntityWithMappingHandlers.MappingHandler,
                        parentEntityWithMappingHandlers.PostMappingHandler
                        );
                }

                // and execute the request
                await context.ExecuteAsync().ConfigureAwait(false);

                // Get the resulting property from the parent object
                var resultValue = parent.GetPublicInstancePropertyValue(memberName) as IEnumerable<TModel>;

                // If the expression type implements IQueryable, we need to return
                // the whole collection of results
                if (expressionType.ImplementsInterface(typeof(IQueryable)))
                {
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
