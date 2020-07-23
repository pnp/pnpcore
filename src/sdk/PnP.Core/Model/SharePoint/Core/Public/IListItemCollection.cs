using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of ListItem objects of SharePoint Online
    /// </summary>
    public interface IListItemCollection : IQueryable<IListItem>, IDataModelCollection<IListItem>, ISupportPaging<IListItem>
    {
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

        /// <summary>
        /// Checks if the collection contains a listitem with a given id
        /// </summary>
        /// <param name="id">Id to check for</param>
        /// <returns>True if found, false otherwise</returns>
        bool Contains(int id);
    }
}
