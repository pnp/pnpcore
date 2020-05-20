using Microsoft.IdentityModel.Tokens;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.Teams;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

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

        public object ExecuteQuery(Type expressionType, ODataQuery<TModel> query)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ClientException(ErrorType.LinqError, $"Missing value for {nameof(memberName)} in {GetType().Name}");
            }

            // At this point in time we support querying collections of 
            // IList and IListItem or single elements of those collections
            if (typeof(TModel).IsAssignableFrom(typeof(IList)) ||
                typeof(TModel).IsAssignableFrom(typeof(IListItem)) ||
                typeof(TModel).IsAssignableFrom(typeof(IWeb)) ||
                typeof(TModel).IsAssignableFrom(typeof(ITeamChannel))||
                typeof(TModel).IsAssignableFrom(typeof(ITeamChatMessage)))
            {
                // Get the entity info
                var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                // and its concrete instance
                var concreteEntity = EntityManager.Instance
                    .GetEntityConcreteInstance<TModel>(typeof(TModel), parent);

                // Get the parent (container) entity info
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(parent.GetType());
                // and cast it to the IDataModelMappingHandler interface
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)parent;

                // Prepare a variable to hold tha batch request ID
                Guid batchRequestId = Guid.Empty;

                // We try to use Graph First, if selected by the user and if the query is supported by Graph
                if (context.GraphFirst && !string.IsNullOrEmpty(entityInfo.GraphLinqGet))
                {
                    // Build the Graph request URL
                    var requestUrl = $"{entityInfo.GraphLinqGet}?{query.ToQueryString(ODataTargetPlatform.Graph, urlEncode: false)}";
                    requestUrl = Core.Model.TokenHandler.ResolveTokensAsync(concreteEntity as IMetadataExtensible, requestUrl, context).GetAwaiter().GetResult();

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
                    // Build the SPO REST request URL
                    var requestUrl = $"{context.Uri}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";
                    requestUrl = Core.Model.TokenHandler.ResolveTokensAsync(concreteEntity as IMetadataExtensible, requestUrl).GetAwaiter().GetResult();

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
                context.ExecuteAsync().GetAwaiter().GetResult();

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
