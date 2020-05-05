using PnP.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Model
{
    /// <summary>
    /// Base abstract class for every Domain Model objects collection
    /// </summary>
    internal abstract class BaseDataModelCollection<TModel> : IDataModelCollection<TModel>, IManageableCollection<TModel>
    {
        #region Core properties

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

        public int Count => this.items.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Indicates whether this collection was fetched from the server
        /// </summary>
        private bool requested = false;

        public bool Requested { get { return requested; } set { requested = value; } }

        public int Length => this.items.Count;

        public System.Collections.IEnumerable RequestedItems { get => this.items; }

        #endregion

        #region ICollection methods

        /// <summary>
        /// Standard add method to add a model to the collection
        /// </summary>
        /// <param name="model">Model to add</param>
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
            this.items.Clear();
        }

        public virtual bool Contains(TModel item)
        {
            return this.items.Contains(item);
        }

        public virtual void CopyTo(TModel[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
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
            return this.GetEnumerator();
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
        /// Parameter less new, needs to be overriden by the collections using this base class
        /// </summary>
        /// <returns>Model entity</returns>
        public virtual TModel CreateNew()
        {
            return default;
        }

        /// <summary>
        /// Parameter less new, needs to be overriden by the collections using this base class
        /// </summary>
        /// <returns>Model entity</returns>
        object IManageableCollection.CreateNew()
        {
            return this.CreateNew();
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
                throw new InvalidCastException("Invalid type for object to add to the collection");
            }

            // And if it is correct, add it through the typed method
            this.Add((TModel)item);
        }

        public void AddOrUpdate(TModel newItem, Predicate<TModel> selector)
        {
            AddOrUpdateInternal(newItem, selector);
        }

        public void AddOrUpdate(object newItem, Predicate<object> selector)
        {
            this.AddOrUpdateInternal((TModel)newItem, p => selector.Invoke(p));
        }

        public void AddOrUpdateInternal(TModel newItem, Predicate<TModel> selector)
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
            return this.Remove((TModel)item);
        }

        #endregion
    }
}
