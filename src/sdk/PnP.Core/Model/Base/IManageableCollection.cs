using System;

namespace PnP.Core.Model
{
    /// <summary>
    /// Defines the behavior of a collection that can be managed
    /// with untyped prototype methods
    /// </summary>
    public interface IManageableCollection
    {
        /// <summary>
        /// Method to create a new item of the collection,
        /// ready to be added to the same collection
        /// </summary>
        /// <returns>The new item, ready to be added to the current collection</returns>
        object CreateNew();

        /// <summary>
        /// Method to create a new typed item of the collection and immediately add it the collection
        /// </summary>
        /// <returns>The new item, added to the collection</returns>
        object CreateNewAndAdd();

        /// <summary>
        /// Method to add a new item to the collection
        /// </summary>
        /// <param name="item">The untyped item to add</param>
        void Add(object item);

        /// <summary>
        /// Method to add a new untyped item or update an already existing one
        /// based on a selection predicate
        /// </summary>
        /// <param name="newItem">The untyped item to add</param>
        /// <param name="selector">The selection predicate for the already existing item, if any</param>
        void AddOrUpdate(object newItem, Predicate<object> selector);

        /// <summary>
        /// Method to remove an untyped item from the collection
        /// </summary>
        /// <param name="item">The untyped item to remove</param>
        /// <returns>True if the removal is successful</returns>
        bool Remove(object item);
    }

    /// <summary>
    /// Defines the behavior of a collection that can be managed
    /// with fully typed prototype methods
    /// </summary>
    public interface IManageableCollection<TModel> : IManageableCollection
    {
        /// <summary>
        /// Method to create a new typed item of the collection,
        /// ready to be added to the same collection
        /// </summary>
        /// <returns>The new item, ready to be added to the current collection</returns>
        new TModel CreateNew();

        /// <summary>
        /// Method to create a new typed item of the collection and immediately add it the collection
        /// </summary>
        /// <returns>The new item, added to the collection</returns>
        new TModel CreateNewAndAdd();

        /// <summary>
        /// Method to add a new item to the collection
        /// </summary>
        /// <param name="item">The untyped item to add</param>
        void Add(TModel item);

        /// <summary>
        /// Method to add a new fully typed item or update an already existing one
        /// based on a selection predicate
        /// </summary>
        /// <param name="newItem">The fully typed item to add</param>
        /// <param name="selector">The selection predicate for the already existing item, if any</param>
        void AddOrUpdate(TModel newItem, Predicate<TModel> selector);

        /// <summary>
        /// Method to remove an fully typed item from the collection
        /// </summary>
        /// <param name="item">The fully typed item to remove</param>
        /// <returns>True if the removal is successful</returns>
        bool Remove(TModel item);

        /// <summary>
        /// Replaces an item in the collection with a new one
        /// </summary>
        /// <param name="itemIndex">The index of the item to replace within the collection</param>
        /// <param name="newItem">New item to replace the old one with</param>
        void Replace(int itemIndex, TModel newItem);
    }
}
