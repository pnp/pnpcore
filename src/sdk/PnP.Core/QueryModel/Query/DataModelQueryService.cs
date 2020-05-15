using Microsoft.IdentityModel.Tokens;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel.OData;
using PnP.Core.Services;
using PnP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace PnP.Core.QueryModel.Query
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
            if (String.IsNullOrEmpty(this.memberName))
            {
                throw new ClientException(ErrorType.LinqError, $"Missing value for {nameof(this.memberName)} in {this.GetType().Name}");
            }

            // At this point in time we support querying collections of 
            // IList and IListItem or single elements of those collections
            if (typeof(TModel).IsAssignableFrom(typeof(IList)) ||
                typeof(TModel).IsAssignableFrom(typeof(IListItem)) ||
                typeof(TModel).IsAssignableFrom(typeof(IWeb)))
            {
                // Get the entity info
                var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                // and its concrete instance
                var concreteEntity = EntityManager.Instance
                    .GetEntityConcreteInstance<TModel>(typeof(TModel), this.parent);

                // Get the parent (container) entity info
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                // and cast it to the IDataModelMappingHandler interface
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)this.parent;

                // Build the request URL
                var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";
                requestUrl = Core.Model.TokenHandler.ResolveTokensAsync(concreteEntity as IMetadataExtensible, requestUrl).GetAwaiter().GetResult();

                // Add the request to the current batch
                var batchRequestId = this.context.CurrentBatch.Add(
                    this.parent as TransientObject,
                    parentEntityInfo,
                    HttpMethod.Get,
                    new ApiCall // First option is Graph
                    {
                        Type = ApiType.SPORest,
                        ReceivingProperty = this.memberName,
                        Request = requestUrl
                    },
                    default(ApiCall),
                    parentEntityWithMappingHandlers.MappingHandler,
                    parentEntityWithMappingHandlers.PostMappingHandler
                    );

                // and execute the request
                this.context.ExecuteAsync().GetAwaiter().GetResult();

                // Get the resulting property from the parent object
                var resultValue = this.parent.GetPublicInstancePropertyValue(this.memberName) as IEnumerable<TModel>;

                // If the expression type implements IQueryable, we need to return
                // the whole collection of results
                if (expressionType.ImplementsInterface(typeof(IQueryable)))
                {
                    return resultValue;
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
            // We do not yet support querying collections of IWeb
            else if (typeof(TModel).IsAssignableFrom(typeof(IWeb)))
            {
                return Enumerable.Empty<IWeb>();
            }

            // So far we just provide a fake empty response
            return Enumerable.Empty<TModel>();
        }
    }
}
