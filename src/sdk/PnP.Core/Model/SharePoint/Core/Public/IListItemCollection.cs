using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItem objects of SharePoint Online
    /// </summary>
    public interface IListItemCollection : IQueryable<IListItem>, IDataModelCollection<IListItem>, ISupportPaging<IListItem>
    {
        #region Add methods
        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddAsync(Dictionary<string, object> values);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public IListItem Add(Dictionary<string, object> values);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddBatchAsync(Batch batch, Dictionary<string, object> values);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="batch">Batch to use</param>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public IListItem AddBatch(Batch batch, Dictionary<string, object> values);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public Task<IListItem> AddBatchAsync(Dictionary<string, object> values);

        /// <summary>
        /// Adds a new list item
        /// </summary>
        /// <param name="values">Values to add to list item</param>
        /// <returns>Newly added list item</returns>
        public IListItem AddBatch(Dictionary<string, object> values);

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
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public IListItem GetById(int id, params Expression<Func<IListItem, object>>[] selectors);

        /// <summary>
        /// Method to select a list item by Id asynchronously
        /// </summary>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public Task<IListItem> GetByIdAsync(int id, params Expression<Func<IListItem, object>>[] selectors);

        #endregion
    }
}
