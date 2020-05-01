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

        public IEnumerable<TModel> ExecuteQuery(ODataQuery<TModel> query)
        {
            // TODO: Implement real method
            Console.WriteLine($"{typeof(TModel)} => {query}");

            if (typeof(TModel).IsAssignableFrom(typeof(IList)))
            {
                // Get the concrete entity that the query targets
                var concreteEntity = EntityManager.Instance
                    .GetEntityConcreteInstance<TModel>(typeof(TModel));
                var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                var entityWithMappingHandlers = (IDataModelMappingHandler)concreteEntity;

                // Get the parent (container) instance, if any
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                var concreteParentEntity = EntityManager.Instance
                    .GetEntityConcreteInstance(this.parent.GetType());
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)concreteParentEntity;

                // var requestUrl = "https://piasysdev.sharepoint.com/sites/prov-1/_api/web?$select=Id%2cLists&$expand=Lists";
                // var requestUrl = "https://piasysdev.sharepoint.com/sites/prov-1/_api/web/lists?$select=Id";
                var requestUrl = $"{this.context.Uri}/{entityInfo.SharePointGet}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";

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
                    .GetEntityConcreteInstance<TModel>(typeof(TModel));
                var entityInfo = EntityManager.Instance.GetClassInfo<TModel>(typeof(TModel));
                var entityWithMappingHandlers = (IDataModelMappingHandler)concreteEntity;

                // Get the parent (container) instance, if any
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(this.parent.GetType());
                var concreteParentEntity = EntityManager.Instance
                    .GetEntityConcreteInstance(this.parent.GetType());
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)concreteParentEntity;

                var requestUrl = $"{this.context.Uri}/{parentEntityInfo.SharePointUri}?{query.ToQueryString(ODataTargetPlatform.SPORest)}";

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


            // So far we just provide a fake empty response
            return Enumerable.Empty<TModel>();
        }
    }
}
