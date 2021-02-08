using Microsoft.Extensions.Logging;
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
            Requested = false;
            Metadata.Clear();
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

        #endregion

        #region Get with filter

        public IEnumerable<TModel> Get(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            return GetAsync(predicate, expressions).GetAwaiter().GetResult();
        }

        public async Task<IEnumerable<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate, params Expression<Func<TModel, object>>[] expressions)
        {
            (Guid batchRequestId, string receivingProperty) = await GetImplementationAsync(PnPContext.CurrentBatch, "Get").ConfigureAwait(false);

            // and execute the request
            await PnPContext.ExecuteAsync().ConfigureAwait(false);

            // Get the resulting property from the parent object
            var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

            return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
        }

        public async Task GetBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            await GetImplementationAsync(batch, "GetBatch").ConfigureAwait(false);
        }

        private async Task<Tuple<Guid, string>> GetImplementationAsync(Batch batch, string operationName)
        {
            if (!(this is IQueryable<TModel>))
            {
                throw new ClientException(PnPCoreResources.Exception_Unsupported_CollectionModelIsNotQueryable);
            }

            var dataModelQueryTranslator = new DataModelQueryTranslator<TModel>();
            var expression = ((IQueryable<TModel>)this).Expression;
            var query = dataModelQueryTranslator.Translate(expression);

            // Get the parent (container) entity info (e.g. for Lists this is Web)
            var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(Parent.GetType());

            // and cast it to the IDataModelMappingHandler interface
            var parentEntityWithMappingHandlers = (IDataModelMappingHandler)Parent;

            // Create a concrete entity of what we expect to get back (e.g. for Lists this is List)
            var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), Parent);
            (concreteEntity as BaseDataModel<TModel>).PnPContext = PnPContext;

            // Get class info for the given concrete entity and the passed expressions
            var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>, query.Fields.ToArray());

            // Determine the receiving property
            var receivingProperty = GetReceivingProperty(parentEntityInfo);
            if (string.IsNullOrEmpty(receivingProperty))
            {
                throw new ClientException(ErrorType.ModelMetadataIncorrect,
                    PnPCoreResources.Exception_ModelMetadataIncorrect_ModelOutOfSync);
            }

            // Team Channel querying will not work with $top...let's rely on client side filtering instead
            // TODO: handle this scenario
            //if (firstOrDefault && typeof(TModel) != typeof(ITeamChannel))
            //{
            //    selectionTarget = selectionTarget.Take(1);
            //}


            var apiCallRequest = await QueryClient.BuildGetAPICallAsync(concreteEntity as BaseDataModel<TModel>, concreteEntityClassInfo, query, default, false, true).ConfigureAwait(false);
            var apiCall = apiCallRequest.ApiCall;
            apiCall.ReceivingProperty = receivingProperty;

            // Prepare a variable to hold tha batch request ID
            Guid batchRequestId = Guid.Empty;

            // Add the request to the current batch
            var id = batch.Add(
                Parent as TransientObject,
                parentEntityInfo,
                HttpMethod.Get,
                apiCall,
                default,
                parentEntityWithMappingHandlers.MappingHandler,
                parentEntityWithMappingHandlers.PostMappingHandler,
                operationName
                );

            if (batchRequestId == Guid.Empty)
            {
                batchRequestId = id;
            }


            return new Tuple<Guid, string>(batchRequestId, receivingProperty);
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
            //if (pageSize < 1)
            //{
            //    throw new ArgumentException(PnPCoreResources.Exception_InvalidPageSize);
            //}

            //// Get the parent (container) entity info (e.g. for Lists this is Web)
            //var parentEntityInfo = EntityManager.Instance.GetStaticClassInfo(Parent.GetType());

            //// and cast it to the IDataModelMappingHandler interface
            //var parentEntityWithMappingHandlers = (IDataModelMappingHandler)Parent;

            //// Create a concrete entity of what we expect to get back (e.g. for Lists this is List)
            //var concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), Parent);
            //(concreteEntity as BaseDataModel<TModel>).PnPContext = PnPContext;

            //// Get class info for the given concrete entity and the passed expressions
            //var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>, expressions);

            //// Determine the receiving property
            //var receivingProperty = GetReceivingProperty(parentEntityInfo);
            //if (string.IsNullOrEmpty(receivingProperty))
            //{
            //    throw new ClientException(ErrorType.ModelMetadataIncorrect,
            //        PnPCoreResources.Exception_ModelMetadataIncorrect_ModelOutOfSync);
            //}

            //// Build the query
            //ODataQuery<TModel> query;
            //if (predicate != null)
            //{
            //    IQueryable<TModel> selectionTarget = this as IQueryable<TModel>;
            //    selectionTarget = selectionTarget.Where(predicate);
            //    query = DataModelQueryProvider<TModel>.Translate(selectionTarget.Expression);
            //}
            //else
            //{
            //    query = DataModelQueryProvider<TModel>.Translate(null);
            //}

            //var apiCalls = await QueryClient.BuildODataGetQueryAsync(concreteEntity, concreteEntityClassInfo, PnPContext, query, receivingProperty).ConfigureAwait(false);
            //string nextLink = apiCalls[0].Request;
            //ApiType nextLinkApiType = apiCalls[0].Type;

            //// Build the API Get request, we'll require the LinqGet decoration to be set
            ////var apiCallRequest = await Query.BuildGetAPICallAsync(concreteEntity as BaseDataModel<TModel>, concreteEntityClassInfo, default, useLinqGet: true).ConfigureAwait(false);
            ////string nextLink = apiCallRequest.ApiCall.Request;
            ////ApiType nextLinkApiType = apiCallRequest.ApiCall.Type;

            //if (string.IsNullOrEmpty(nextLink))
            //{
            //    throw new ClientException(ErrorType.ModelMetadataIncorrect,
            //        PnPCoreResources.Exception_ModelMetadataIncorrect_MissingLinqGet);
            //}

            //// Ensure $top is added/updated to reflect the page size
            //nextLink = QueryClient.EnsureTopUrlParameter(nextLink, nextLinkApiType, pageSize);

            //// Make the server request
            //var batchRequestId = PnPContext.CurrentBatch.Add(
            //    Parent as TransientObject,
            //    parentEntityInfo,
            //    HttpMethod.Get,
            //    new ApiCall
            //    {
            //        Type = nextLinkApiType,
            //        ReceivingProperty = receivingProperty,
            //        Request = nextLink
            //    },
            //    default,
            //    parentEntityWithMappingHandlers.MappingHandler,
            //    parentEntityWithMappingHandlers.PostMappingHandler,
            //    "GetPaged"
            //    );

            //await PnPContext.ExecuteAsync().ConfigureAwait(false);

            //// Get the resulting property from the parent object
            //var resultValue = Parent.GetPublicInstancePropertyValue(receivingProperty) as IEnumerable<TModel>;

            //return resultValue.Where(p => (p as TransientObject).BatchRequestId == batchRequestId);
            // TODO: remove this implementation
            throw new NotImplementedException();
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
                (var nextLink, var nextLinkApiType) = QueryClient.BuildNextPageLink(this);

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
                    parentEntityWithMappingHandlers.PostMappingHandler,
                    "GetNextPage"
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

        #region Delete by id

        public void DeleteById(int id)
        {
            DeleteByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(intId: id).ConfigureAwait(false);

            await DeleteById(query, model).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(int id)
        {
            DeleteByIdBatchAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(int id)
        {
            await DeleteByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(Batch batch, int id)
        {
            DeleteByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(Batch batch, int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(intId: id).ConfigureAwait(false);

            await DeleteById(batch, query, model).ConfigureAwait(false);
        }

        public void DeleteById(string id)
        {
            DeleteByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(stringId: id).ConfigureAwait(false);

            await DeleteById(query, model).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(string id)
        {
            DeleteByIdBatchAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(string id)
        {
            await DeleteByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(Batch batch, string id)
        {
            DeleteByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(Batch batch, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(stringId: id).ConfigureAwait(false);

            await DeleteById(batch, query, model).ConfigureAwait(false);
        }

        public void DeleteById(Guid id)
        {
            DeleteByIdAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            if (id.Equals(default))
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(guidId: id).ConfigureAwait(false);

            await DeleteById(query, model).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(Guid id)
        {
            DeleteByIdBatchAsync(id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(Guid id)
        {
            await DeleteByIdBatchAsync(PnPContext.CurrentBatch, id).ConfigureAwait(false);
        }

        public void DeleteByIdBatch(Batch batch, Guid id)
        {
            DeleteByIdBatchAsync(batch, id).GetAwaiter().GetResult();
        }

        public async Task DeleteByIdBatchAsync(Batch batch, Guid id)
        {
            if (id.Equals(default))
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            (var query, var model) = await DeleteByIdImplementationAsync(guidId: id).ConfigureAwait(false);

            await DeleteById(batch, query, model).ConfigureAwait(false);
        }

        private async Task DeleteById(ApiCallRequest query, BaseDataModel<TModel> model)
        {
            if (query.Cancelled)
            {
                // If the query was cancelled via an override then stop the delete
                PnPContext.Logger.LogWarning(query.CancellationReason);
            }
            else
            {
                // Perform an interactive delete
                await model.RawRequestAsync(query.ApiCall, HttpMethod.Delete).ConfigureAwait(false);
            }
        }

        private async Task DeleteById(Batch batch, ApiCallRequest query, BaseDataModel<TModel> model)
        {
            if (query.Cancelled)
            {
                // If the query was cancelled via an override then stop the delete
                PnPContext.Logger.LogWarning(query.CancellationReason);
            }
            else
            {
                // Perform an interactive delete
                await model.RawRequestBatchAsync(batch, query.ApiCall, HttpMethod.Delete, "DeleteByIdBatch").ConfigureAwait(false);
            }
        }

        private async Task<Tuple<ApiCallRequest, BaseDataModel<TModel>>> DeleteByIdImplementationAsync(int intId = 0, string stringId = null, Guid guidId = default)
        {

            // First check if we've a model instance loaded with the given key, if so let's use that to do the delete 
            // as then the model instance will also automatically be removed from the collection
            object keyValue = null;

            if (intId > 0)
            {
                keyValue = intId;
            }
            else if (!guidId.Equals(default))
            {
                keyValue = guidId;
            }
            else if (!string.IsNullOrEmpty(stringId))
            {
                keyValue = stringId;
            }

            object concreteEntity = null;
            var modelToDeleteIndex = items.FindIndex(i => ((IDataModelWithKey)i).Key.Equals(keyValue));
            if (modelToDeleteIndex >= 0)
            {
                // Use the existing concrete entity for the delete
                concreteEntity = items[modelToDeleteIndex];
            }
            else
            {
                // Create a concrete entity of what we expect to delete (e.g. for Lists this is List)
                concreteEntity = EntityManager.GetEntityConcreteInstance<TModel>(typeof(TModel), Parent);
                (concreteEntity as BaseDataModel<TModel>).PnPContext = PnPContext;

                // Ensure the key property of the created model is populated
                if (intId > 0)
                {
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, intId.ToString());
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataGraphId, intId.ToString());
                }
                else if (!guidId.Equals(default))
                {
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, guidId.ToString());
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataGraphId, guidId.ToString());
                }
                else if (!string.IsNullOrEmpty(stringId))
                {
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataRestId, stringId);
                    (concreteEntity as IMetadataExtensible).Metadata.Add(PnPConstants.MetaDataGraphId, stringId);
                }
            }

            // Get class info for the given concrete entity
            var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>, null);

            // Build the delete call
            var query = await QueryClient.BuildDeleteAPICallAsync(concreteEntity as BaseDataModel<TModel>, concreteEntityClassInfo).ConfigureAwait(false);

            return new Tuple<ApiCallRequest, BaseDataModel<TModel>>(query, concreteEntity as BaseDataModel<TModel>);
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

    }
}
