using Microsoft.Extensions.Logging;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base abstract class for every Domain Model objects collection
    /// </summary>
    internal abstract class BaseDataModelCollection<TModel> : IDataModelCollection<TModel>, IManageableCollection<TModel>, ISupportPaging, IMetadataExtensible, IDataModelCollectionLoad<TModel>
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
            TModel newModel = (TModel)EntityManager.GetEntityConcreteInstance(typeof(TModel), this);
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

        /// <summary>
        /// Replaces an item of the collection
        /// </summary>
        /// <param name="itemIndex">The index of the item to replace</param>
        /// <param name="newItem">The new item to replace the existing one with</param>
        public virtual void Replace(int itemIndex, object newItem)
        {
            Replace(itemIndex, (TModel)newItem);
        }

        /// <summary>
        /// Replaces an item of the collection
        /// </summary>
        /// <param name="itemIndex">The index of the item to replace</param>
        /// <param name="newItem">The new item to replace the existing one with</param>
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

        #region Paging (ISupportPaging implementation)

        public bool CanPage
        {
            get
            {
                return Metadata.ContainsKey(PnPConstants.GraphNextLink) || Metadata.ContainsKey(PnPConstants.SharePointRestListItemNextLink);
            }
        }

        #endregion

        #region Delete by id

        // TODO: Do we really need all these methods with all these options for ID type?
        // Can't we make a unique generic method where we provide/infer the type of the ID?

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
                concreteEntity = EntityManager.GetEntityConcreteInstance(typeof(TModel), Parent);
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
            var concreteEntityClassInfo = EntityManager.GetClassInfo(typeof(TModel), concreteEntity as BaseDataModel<TModel>);

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

        #region Load

        private EntityFieldInfo GetReceivingField(EntityInfo parentEntityInfo)
        {
            var internalCollectionType = GetType();
            var publicCollectionType = Type.GetType($"{internalCollectionType.Namespace}.I{internalCollectionType.Name}");

            return parentEntityInfo.Fields.FirstOrDefault(p => p.DataType == publicCollectionType);
        }

        private LambdaExpression[] ConvertExpressionToParent(Expression<Func<TModel, object>>[] expressions)
        {
            // Get interface type, like IList
            Type interfaceType = Parent.GetType().BaseType.GetGenericArguments()[0];

            var parentEntityInfo = EntityManager.GetClassInfo<object>(interfaceType, null);
            var field = GetReceivingField(parentEntityInfo);
            var listProperty = interfaceType.GetRuntimeProperty(field.Name);

            // p
            var parentParameter = Expression.Parameter(interfaceType);
            // p.Items
            var collectionMemberExpression = Expression.MakeMemberAccess(parentParameter, listProperty);
            var lambdaType = typeof(Func<,>).MakeGenericType(interfaceType, typeof(object));
            
            var result = new List<LambdaExpression>(expressions.Length + 1);
            // Add expression for current collection
            var collectionExpression = Expression.Lambda(lambdaType, collectionMemberExpression, parentParameter);
            result.Add(collectionExpression);

            if (expressions.Length > 0)
            {
                // QueryProperties method
                var itemType = field.DataType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    .GetGenericArguments()[0];
                var queryPropertiesMethod = QueryableMethods.QueryProperties.MakeGenericMethod(itemType);

                // new [] {}
                var arrayExpression = Expression.NewArrayInit(typeof(Expression<Func<TModel, object>>), expressions.AsEnumerable());

                // p.Items.QueryProperies
                var queryPropertiesExpression = Expression.Call(null, queryPropertiesMethod, collectionMemberExpression, arrayExpression);

                // Convert all other expression in order to change the source object with the collection
                //var newBody = new ParameterReplaceVisitor(this.GetType(), collectionMemberExpression).Visit(queryPropertiesExpression);
                result.Add(Expression.Lambda(lambdaType, queryPropertiesExpression, parentParameter));
            }

            return result.ToArray();
        }

        public Task LoadAsync(params Expression<Func<TModel, object>>[] expressions)
        {
            if (Parent is IDataModelLoad l)
            {
                return l.LoadAsync(ConvertExpressionToParent(expressions));
            }

            throw new NotSupportedException();
        }

        public Task<IBatchResult> LoadBatchAsync(Batch batch, params Expression<Func<TModel, object>>[] expressions)
        {
            if (Parent is IDataModelLoad l)
            {
                return l.LoadBatchAsync(batch, ConvertExpressionToParent(expressions));
            }

            throw new NotSupportedException();
        }

        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly Type type;
            private readonly Expression to;

            public ParameterReplaceVisitor(Type type, Expression to)
            {
                this.type = type;
                this.to = to;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression.Type == type)
                {
                    return node.Update(to);
                }
                return base.VisitMember(node);
            }

        }

        #endregion
    }
}
