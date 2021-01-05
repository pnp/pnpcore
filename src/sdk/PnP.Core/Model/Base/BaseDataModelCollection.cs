using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base abstract class for every Domain Model objects collection
    /// </summary>
    internal abstract class BaseDataModelCollection<TModel> : IDataModelCollection<TModel>, IManageableCollection<TModel>, ISupportPaging<TModel>, IMetadataExtensible
    {
        private QueryClient query;

        /// <summary>
        /// Connected query client
        /// </summary>
        internal QueryClient Query
        {
            get
            {
                if (query == null)
                {
                    query = new QueryClient();
                }

                return query;
            }
        }

        #region Core properties

        /// <summary>
        /// Dictionary to access the domain model object Metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        /// <summary>
        /// List of items in the collection
        /// </summary>
        protected List<TModel> items = new List<TModel>();

        /// <summary>
        /// Each item and each collection requires a PnPContext
        /// </summary>
        public PnPContext PnPContext { get; set; }

        /// <summary>
        /// Parent of this collection
        /// </summary>
        public IDataModelParent Parent { get; set; }

        /// <summary>
        /// Number of items in the collection
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Was this collectioned loaded with data from a server request
        /// </summary>
        public bool Requested { get; set; }

        /// <summary>
        /// Number of items in the collection
        /// </summary>
        public int Length => items.Count;

        /// <summary>
        /// Items in the collection
        /// </summary>
        public IEnumerable RequestedItems { get => items; }

        #endregion

        #region ICollection methods

        /// <summary>
        /// Standard add method to add a model to the collection
        /// </summary>
        /// <param name="item">Model instance to add</param>
        public virtual void Add(TModel item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            items.Add(item);
            Requested = true;
        }

        public virtual void Clear()
        {
            items.Clear();
        }

        public virtual bool Contains(TModel item)
        {
            return items.Contains(item);
        }

        public virtual void CopyTo(TModel[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator, will also purge deleted items
        /// </summary>
        /// <returns>Enumerator for this collection</returns>
        public virtual IEnumerator<TModel> GetEnumerator()
        {
            PurgeDeletedItems();
            return items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator, will also purge deleted items
        /// </summary>
        /// <returns>Enumerator for this collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Removes a model from the collection, will be called for example by the logic that consolidates batch requests
        /// </summary>
        /// <param name="item">Model to remove</param>
        public virtual bool Remove(TModel item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return items.Remove(item);
        }

        #endregion

        #region Items management methods

        /// <summary>
        /// Purges the items marked as deleted
        /// </summary>
        internal void PurgeDeletedItems()
        {
            foreach (var item in items.Where(p => (p as TransientObject).Deleted).ToArray())
            {
                // remove item from collection as it was purged                       
                items.Remove(item);
            }
        }

        /// <summary>
        /// Creates a new instance of the model handled by this collection. The instance will not be added to the collection
        /// </summary>
        /// <returns>Model entity</returns>
        public TModel CreateNew()
        {
            TModel newModel = (TModel)EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), this);
            (newModel as IDataModelWithContext).PnPContext = PnPContext;
            return newModel;
        }

        /// <summary>
        /// Creates a new instance of the model handled by this collection and adds it to the collection. This 
        /// add will not mark the collection as requested
        /// </summary>
        /// <returns>Model entity</returns>
        public TModel CreateNewAndAdd()
        {
            var newModel = CreateNew();
            items.Add(newModel);
            return newModel;
        }

        /// <summary>
        /// Parameter less new, needs to be overriden by the collections using this base class
        /// </summary>
        /// <returns>Model entity</returns>
        object IManageableCollection.CreateNew()
        {
            return CreateNew();
        }

        /// <summary>
        /// Creates a new instance of the model handled by this collection and adds it to the collection. This 
        /// add will not mark the collection as requested
        /// </summary>
        /// <returns>Model entity</returns>
        object IManageableCollection.CreateNewAndAdd()
        {
            return CreateNewAndAdd();
        }

        public void Add(object item)
        {
            // Check that the item is not null
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            // And that the item is of the correct type
            if (!(item is TModel))
            {
                throw new InvalidCastException(
                    PnPCoreResources.Exception_InvalidTypeForCollection);
            }

            // And if it is correct, add it through the typed method
            Add((TModel)item);
        }

        public void AddOrUpdate(TModel newItem, Predicate<TModel> selector)
        {
            AddOrUpdateInternal(newItem, selector);
        }

        public void AddOrUpdate(object newItem, Predicate<object> selector)
        {
            AddOrUpdateInternal((TModel)newItem, p => selector.Invoke(p));
        }

        public void AddOrUpdateInternal(TModel newItem, Predicate<TModel> selector)
        {
            // When the collection was never requested then just add the item, no point 
            // in trying to check if the item exists as collections that were not requested
            // typically do not have the key loaded. If the key property was not loaded the FindIndex will fail
            if (!Requested)
            {
                items.Add(newItem);
            }
            else
            {
                var itemToUpdateIndex = items.FindIndex(selector);
                if (itemToUpdateIndex >= 0)
                {
                    Replace(itemToUpdateIndex, newItem);
                }
                else
                {
                    items.Add(newItem);
                }
            }
        }

        public virtual void Replace(int itemIndex, TModel newItem)
        {
            items[itemIndex] = newItem;
        }

        /// <summary>
        /// Removes a model from the collection, will be called for example by the logic that consolidates batch requests
        /// </summary>
        /// <param name="item">Model to remove</param>
        public virtual bool Remove(object item)
        {
            return Remove((TModel)item);
        }

        #endregion

        #region Get

        public async Task<IEnumerable<TModel>> GetAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetAsync(null, expressions).ConfigureAwait(false);
        }

        public IEnumerable<TModel> Get(params Expression<Func<TModel, object>>[] expressions)
        {
            return GetAsync(expressions).GetAwaiter().GetResult();
        }

        public async Task GetBatchAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            await GetBatchAsync(PnPContext.CurrentBatch, expressions).ConfigureAwait(false);
        }

        public void GetBatch(params Expression<Func<TModel, object>>[] expressions)
        {
            GetBatchAsync(expressions).GetAwaiter().GetResult();
        }

        public async Task GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetBatchAsync(batch, null, expressions).ConfigureAwait(false);
        }

        public void GetBatch(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            GetBatchAsync(batch, expressions).GetAwaiter().GetResult();
        }

        #endregion

        #region Get with filter

        public IEnumerable<TModel> Get(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetAsync(predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            (Guid batchRequestId, string receivingProperty) = await GetImplementationAsync(PnPContext.CurrentBatch, predicate, expressions, false).ConfigureAwait(false);

            // and execute the request
            await PnPContext.ExecuteAsync().ConfigureAwait(false);

            // Get the resulting property from the parent object
            var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

            return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
        }

        public void GetBatch(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            GetBatchAsync(predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task GetBatchAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetBatchAsync(PnPContext.CurrentBatch, predicate, expressions).ConfigureAwait(false);
        }

        public void GetBatch(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            GetBatchAsync(batch, predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task GetBatchAsync(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetImplementationAsync(batch, predicate, expressions, false).ConfigureAwait(false);
        }

        private async Task<Tuple<Guid, string>> GetImplementationAsync(Batch batch, Expression<Func<TModel, bool>> predicate, Expression<Func<TModel, object>>[] expressions, bool firstOrDefault)
        {
            if (!(this is IQueryable<TModel>))
            {
                throw new ClientException(PnPCoreResources.Exception_Unsupported_CollectionModelIsNotQueryable);
            }

            // Get the parent (container) entity info (e.g. for Lists this is Web)
            var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(Parent.GetType());

            // and cast it to the IDataModelMappingHandler interface
            var parentEntityWithMappingHandlers = (IDataModelMappingHandler)Parent;

            // Create a concrete entity of what we expect to get back (e.g. for Lists this is List)
            var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), Parent);
            (concreteEntity as BaseDataModel<TModel>).PnPContext = PnPContext;

            // Get class info for the given concrete entity and the passed expressions
            var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>, expressions);

            // Determine the receiving property
            var receivingProperty = GetReceivingProperty(parentEntityInfo);
            if (string.IsNullOrEmpty(receivingProperty))
            {
                throw new ClientException(ErrorType.ModelMetadataIncorrect,
                    PnPCoreResources.Exception_ModelMetadataIncorrect_ModelOutOfSync);
            }

            // Build the query
            IQueryable<TModel> selectionTarget = this as IQueryable<TModel>;

            //if (expressions != null)
            //{
            //    selectionTarget = QueryClient.ProcessExpression(selectionTarget, concreteEntityClassInfo, expressions);
            //}

            if (predicate != null)
            {
                selectionTarget = selectionTarget.Where(predicate);
            }

            // Team Channel querying will not work with $top...let's rely on client side filtering instead
            if (firstOrDefault && typeof(TModel) != typeof(ITeamChannel))
            {
                selectionTarget = selectionTarget.Take(1);
            }

            var query = DataModelQueryProvider<TModel>.Translate(selectionTarget.Expression);

            var apiCalls = await QueryClient.BuildODataGetQueryAsync(concreteEntity, concreteEntityClassInfo, PnPContext, query, receivingProperty).ConfigureAwait(false);

            // Prepare a variable to hold tha batch request ID
            Guid batchRequestId = Guid.Empty;

            foreach (var apiCall in apiCalls)
            {
                // Add the request to the current batch
                var id = batch.Add(
                    Parent as TransientObject,
                    parentEntityInfo,
                    HttpMethod.Get,
                    apiCall,
                    default,
                    parentEntityWithMappingHandlers.MappingHandler,
                    parentEntityWithMappingHandlers.PostMappingHandler
                    );

                if (batchRequestId == Guid.Empty)
                {
                    batchRequestId = id;
                }
            }

            return new Tuple<Guid, string>(batchRequestId, receivingProperty);
        }

        #endregion

        #region Get first or default with filter

        public TModel GetFirstOrDefault(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetFirstOrDefaultAsync(predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task<TModel> GetFirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            (Guid batchRequestId, string receivingProperty) = await GetImplementationAsync(PnPContext.CurrentBatch, predicate, expressions, true).ConfigureAwait(false);

            // and execute the request
            await PnPContext.ExecuteAsync().ConfigureAwait(false);

            // Get the resulting property from the parent object
            var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

            return resultValue.FirstOrDefault(p => (p as TransientObject).BatchRequestId == batchRequestId);
        }

        public void GetFirstOrDefaultBatch(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            GetFirstOrDefaultBatchAsync(predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task GetFirstOrDefaultBatchAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetFirstOrDefaultBatchAsync(PnPContext.CurrentBatch, predicate, expressions).ConfigureAwait(false);
        }

        public void GetFirstOrDefaultBatch(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            GetFirstOrDefaultBatchAsync(batch, predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task GetFirstOrDefaultBatchAsync(Batch batch, Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetImplementationAsync(batch, predicate, expressions, true).ConfigureAwait(false);
        }

        #endregion


        #region Paging (ISupportPaging implementation)

        public bool CanPage
        {
            get
            {
                return Metadata.ContainsKey(PnPConstants.GraphNextLink) || Metadata.ContainsKey(PnPConstants.SharePointRestListItemNextLink);
            }
        }

        public IEnumerable<TModel> GetPaged(int pageSize, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetPagedAsync(pageSize, expressions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<TModel>> GetPagedAsync(int pageSize, params Expression<Func<TModel, object>>[] expressions)
        {
            return await GetPagedAsync(null, pageSize, expressions).ConfigureAwait(false);
        }

        public IEnumerable<TModel> GetPaged(Expression<Func<TModel, bool>> predicate, int pageSize, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetPagedAsync(predicate, pageSize, expressions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<TModel>> GetPagedAsync(Expression<Func<TModel, bool>> predicate, int pageSize, params Expression<Func<TModel, object>>[] expressions)
        {
            if (pageSize < 1)
            {
                throw new ArgumentException(PnPCoreResources.Exception_InvalidPageSize);
            }

            // Get the parent (container) entity info (e.g. for Lists this is Web)
            var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(Parent.GetType());

            // and cast it to the IDataModelMappingHandler interface
            var parentEntityWithMappingHandlers = (IDataModelMappingHandler)Parent;

            // Create a concrete entity of what we expect to get back (e.g. for Lists this is List)
            var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), Parent);
            (concreteEntity as BaseDataModel<TModel>).PnPContext = PnPContext;

            // Get class info for the given concrete entity and the passed expressions
            var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>, expressions);

            // Determine the receiving property
            var receivingProperty = GetReceivingProperty(parentEntityInfo);
            if (string.IsNullOrEmpty(receivingProperty))
            {
                throw new ClientException(ErrorType.ModelMetadataIncorrect,
                    PnPCoreResources.Exception_ModelMetadataIncorrect_ModelOutOfSync);
            }

            // Build the query
            ODataQuery<TModel> query;
            if (predicate != null)
            {
                IQueryable<TModel> selectionTarget = this as IQueryable<TModel>;
                selectionTarget = selectionTarget.Where(predicate);
                query = DataModelQueryProvider<TModel>.Translate(selectionTarget.Expression);
            }
            else
            {
                query = DataModelQueryProvider<TModel>.Translate(null);
            }

            var apiCalls = await QueryClient.BuildODataGetQueryAsync(concreteEntity, concreteEntityClassInfo, PnPContext, query, receivingProperty).ConfigureAwait(false);
            string nextLink = apiCalls[0].Request;
            ApiType nextLinkApiType = apiCalls[0].Type;

            // Build the API Get request, we'll require the LinqGet decoration to be set
            //var apiCallRequest = await Query.BuildGetAPICallAsync(concreteEntity as BaseDataModel<TModel>, concreteEntityClassInfo, default, useLinqGet: true).ConfigureAwait(false);
            //string nextLink = apiCallRequest.ApiCall.Request;
            //ApiType nextLinkApiType = apiCallRequest.ApiCall.Type;

            if (string.IsNullOrEmpty(nextLink))
            {
                throw new ClientException(ErrorType.ModelMetadataIncorrect,
                    PnPCoreResources.Exception_ModelMetadataIncorrect_MissingLinqGet);
            }

            // Ensure $top is added/updated to reflect the page size
            nextLink = QueryClient.EnsureTopUrlParameter(nextLink, nextLinkApiType, pageSize);

            // Make the server request
            var batchRequestId = PnPContext.CurrentBatch.Add(
                Parent as TransientObject,
                parentEntityInfo,
                HttpMethod.Get,
                new ApiCall
                {
                    Type = nextLinkApiType,
                    ReceivingProperty = receivingProperty,
                    Request = nextLink
                },
                default,
                parentEntityWithMappingHandlers.MappingHandler,
                parentEntityWithMappingHandlers.PostMappingHandler
                );

            await PnPContext.ExecuteAsync().ConfigureAwait(false);

            // Get the resulting property from the parent object
            var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

            return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
        }

        public IEnumerable<TModel> GetNextPage()
        {
            return GetNextPageAsync().GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<TModel>> GetNextPageAsync()
        {
            if (CanPage)
            {
                // Get the parent (container) entity info
                var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(Parent.GetType());
                // and cast it to the IDataModelMappingHandler interface
                var parentEntityWithMappingHandlers = (IDataModelMappingHandler)Parent;

                // Determine the receiving property
                var receivingProperty = GetReceivingProperty(parentEntityInfo);
                if (string.IsNullOrEmpty(receivingProperty))
                {
                    throw new ClientException(ErrorType.ModelMetadataIncorrect,
                        PnPCoreResources.Exception_ModelMetadataIncorrect_ModelOutOfSync);
                }

                // Prepare api call
                (var nextLink, var nextLinkApiType) = Query.BuildNextPageLink(this);

                var batchRequestId = PnPContext.CurrentBatch.Add(
                    Parent as TransientObject,
                    parentEntityInfo,
                    HttpMethod.Get,
                    new ApiCall
                    {
                        Type = nextLinkApiType,
                        ReceivingProperty = receivingProperty,
                        Request = nextLink
                    },
                    default,
                    parentEntityWithMappingHandlers.MappingHandler,
                    parentEntityWithMappingHandlers.PostMappingHandler
                    );

                await PnPContext.ExecuteAsync().ConfigureAwait(false);

                // Get the resulting property from the parent object
                var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

                return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
            }
            else
            {
                throw new ClientException(ErrorType.CollectionNotLoaded,
                    PnPCoreResources.Exception_CollectionNotLoaded);
            }
        }

        public void GetAllPages()
        {
            GetAllPagesAsync().GetAwaiter().GetResult();
        }

        public async Task GetAllPagesAsync()
        {
            bool loadNextPage = true;
            int currentItemCount = items.Count;
            while (loadNextPage)
            {
                await GetNextPageAsync().ConfigureAwait(false);

                // Keep requesting pages until the total item count is not increasing anymore
                if (items.Count == currentItemCount)
                {
                    loadNextPage = false;

                    // Clear the MetaData paging links to avoid loading the collection again via paging
                    if (Metadata.ContainsKey(PnPConstants.GraphNextLink))
                    {
                        Metadata.Remove(PnPConstants.GraphNextLink);
                    }
                    else if (Metadata.ContainsKey(PnPConstants.SharePointRestListItemNextLink))
                    {
                        Metadata.Remove(PnPConstants.SharePointRestListItemNextLink);
                    }
                }
                else
                {
                    currentItemCount = items.Count;
                }
            }
        }

        private string GetReceivingProperty(EntityInfo parentEntityInfo)
        {
            var internalCollectionType = GetType();
            var publicCollectionType = Type.GetType($"{internalCollectionType.Namespace}.I{internalCollectionType.Name}");

            var propertyInParent = parentEntityInfo.Fields.FirstOrDefault(p => p.DataType == publicCollectionType);

            return propertyInParent?.Name;
        }
        #endregion

        #region Metadata Management
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IMetadataExtensible.SetGraphToRestMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        async Task IMetadataExtensible.SetRestToGraphMetadataAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
        #endregion

        #region LoadProperties
        /// <summary>
        /// Enables using the .LoadProperties lambda expression syntax on a collection
        /// </summary>
        /// <param name="expressions">Expression</param>
        /// <returns>Null...return value is not needed</returns>
        public IQueryable<TModel> LoadProperties(params Expression<Func<TModel, object>>[] expressions)
        {
            return null;
        }
        #endregion
    }
}
