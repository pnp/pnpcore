using Microsoft.IdentityModel.Tokens;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel.OData;
using PnP.Core.Services;
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
        public DataModelQueryService(PnPContext context, IDataModelParent parent)
        {
            this.context = context;
            this.parent = parent;
        }

        public object ExecuteQuery(Type expressionType, ODataQuery<TModel> query)
        {
            // TODO: Implement real method
            Console.WriteLine($"{expressionType.Name} - {typeof(TModel)} => {query}");

            // If the expression type implements IQueryable, we need to return
            // a collection of results
            if (expressionType.ImplementsInterface(typeof(IQueryable)))
            {
                if (typeof(TModel).IsAssignableFrom(typeof(IList)))
                {
                    // Get the concrete entity that the query targets
                    var concreteEntity = EntityManager.Instance
                        .GetEntityConcreteInstance<TModel>(typeof(TModel), this.parent);
                    var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                    var entityWithMappingHandlers = (IDataModelMappingHandler)concreteEntity;

                    // Get the parent (container) instance, if any
                    var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                    var concreteParentEntity = EntityManager.Instance
                        .GetEntityConcreteInstance(this.parent.GetType());
                    var parentEntityWithMappingHandlers = (IDataModelMappingHandler)concreteParentEntity;

                    var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";

                    this.context.CurrentBatch.Add(
                        this.parent as TransientObject,
                        parentEntityInfo,
                        HttpMethod.Get,
                        new ApiCall // First option is Graph
                    {
                            Type = ApiType.SPORest,
                            ReceivingProperty = "Lists",
                            Request = requestUrl
                        },
                        default(ApiCall),
                        parentEntityWithMappingHandlers.MappingHandler,
                        parentEntityWithMappingHandlers.PostMappingHandler
                        );

                    this.context.ExecuteAsync().GetAwaiter().GetResult();

                    return (this.parent as dynamic).Lists as IEnumerable<TModel>;
                }
                else if (typeof(TModel).IsAssignableFrom(typeof(IListItem)))
                {
                    // Get the concrete entity that the query targets
                    var concreteEntity = EntityManager.Instance
                        .GetEntityConcreteInstance<TModel>(typeof(TModel), this.parent);
                    var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                    var entityWithMappingHandlers = (IDataModelMappingHandler)concreteEntity;

                    //// Get the parent (container) instance, if any
                    var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                    //var concreteParentEntity = EntityManager.Instance
                    //    .GetEntityConcreteInstance(this.parent.GetType());
                    var parentEntityWithMappingHandlers = (IDataModelMappingHandler)this.parent;

                    // Resolve tokens
                    // EnsureParent (recursive chain via batch)
                    // Final resolve

                    // var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointLinqGet.Replace("{Parent.Id}", ((IDataModelWithKey)this.parent).Key.ToString())}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";
                    var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";
                    requestUrl = TokensHandler.ResolveTokens(concreteEntity as IMetadataExtensible, requestUrl).GetAwaiter().GetResult();

                    this.context.CurrentBatch.Add(
                        this.parent as TransientObject,
                        parentEntityInfo,
                        HttpMethod.Get,
                        new ApiCall // First option is Graph
                        {
                            Type = ApiType.SPORest,
                            ReceivingProperty = "Items",
                            Request = requestUrl
                        },
                        default(ApiCall),
                        parentEntityWithMappingHandlers.MappingHandler,
                        parentEntityWithMappingHandlers.PostMappingHandler
                        );

                    this.context.ExecuteAsync().GetAwaiter().GetResult();

                    return (this.parent as dynamic).Items as IEnumerable<TModel>;
                }
                else if (typeof(TModel).IsAssignableFrom(typeof(IWeb)))
                {
                    return Enumerable.Empty<IWeb>();
                }
            }
            // Otherwise if the expression type is the type of TModel, we need
            // to return a single item
            else
            {
                if (typeof(TModel).IsAssignableFrom(typeof(IList)))
                {
                    // Get the concrete entity that the query targets
                    var concreteEntity = EntityManager.Instance
                        .GetEntityConcreteInstance<TModel>(typeof(TModel), this.parent);
                    var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                    var entityWithMappingHandlers = (IDataModelMappingHandler)concreteEntity;

                    // Get the parent (container) instance, if any
                    var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                    var concreteParentEntity = EntityManager.Instance
                        .GetEntityConcreteInstance(this.parent.GetType());
                    var parentEntityWithMappingHandlers = (IDataModelMappingHandler)concreteParentEntity;

                    var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointLinqGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";

                    this.context.CurrentBatch.Add(
                        this.parent as TransientObject,
                        parentEntityInfo,
                        HttpMethod.Get,
                        new ApiCall // First option is Graph
                        {
                            Type = ApiType.SPORest,
                            ReceivingProperty = "Lists",
                            Request = requestUrl
                        },
                        default(ApiCall),
                        parentEntityWithMappingHandlers.MappingHandler,
                        parentEntityWithMappingHandlers.PostMappingHandler
                        );

                    this.context.ExecuteAsync().GetAwaiter().GetResult();

                    // In case we need to retrieve just one item make
                    // sure that the result will be just one item
                    if (query.Top == 1)
                    {
                        return ((this.parent as dynamic).Lists as IEnumerable<TModel>).FirstOrDefault();
                    }
                    else
                    {
                        return default(TModel);
                    }
                }
                else
                {
                    return default(TModel);
                }
            }

            // So far we just provide a fake empty response
            return Enumerable.Empty<TModel>();
        }
    }
}
