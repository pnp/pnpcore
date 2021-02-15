using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItem objects of SharePoint Online
    /// </summary>
    [ConcreteType(typeof(ListItemCollection))]
    public interface IListItemCollection : IQueryable<IListItem>, IDataModelCollection<IListItem>, IDataModelCollectionLoad<IListItem>, ISupportPaging<IListItem>, IDataModelCollectionDeleteByIntegerId
    {
        #region Add methods
        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddAsync(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public IListItem Add(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddBatchAsync(Batch batch, Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public IListItem AddBatch(Batch batch, Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddBatchAsync(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <param name="folderPath">Optional folder path to add the item too.</param>
        /// <param name="underlyingObjectType">Type of object to create. Defaults to File/ListItem</param>
        /// <returns>Newly added list item</returns>
        public IListItem AddBatch(Dictionary<string, object> values, string folderPath = null, FileSystemObjectType underlyingObjectType = FileSystemObjectType.File);

        #endregion

        #region Contains method

        /// <summary>
        /// Checks if the collection contains a listitem with a given id
        /// </summary>
        /// <param name="id">Id to check for</param>
        /// <returns>True if found, false otherwise</returns>
        bool Contains(int id);

        #endregion

        #region GetById methods

        /// <summary>
        /// Method to select a list item by Id
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting list item instance, if any</returns>
        public IListItem GetById(int id);

        /// <summary>
        /// Method to select a list item by Id asynchronously
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting list item instance, if any</returns>
        public Task<IListItem> GetByIdAsync(int id);

        #endregion

        #region RecycleById methods

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public Guid RecycleById(int id);

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public Task<Guid> RecycleByIdAsync(int id);

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public void RecycleByIdBatch(int id);

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public Task RecycleByIdBatchAsync(int id);

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public void RecycleByIdBatch(Batch batch, int id);

        /// <summary>
        /// Recycle the list item
        /// </summary>
        /// <param name="batch">Batch to add the request to</param>
        /// <param name="id">Id of the item to recycle</param>
        /// <returns></returns>
        public Task RecycleByIdBatchAsync(Batch batch, int id);
        #endregion
    }
}
