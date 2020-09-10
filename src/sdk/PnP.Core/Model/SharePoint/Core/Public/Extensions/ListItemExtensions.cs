using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListItem extension methods
    /// </summary>
    public static class ListItemExtensions
    {
        #region GetById for List Items implementation

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static IListItem GetById(this IQueryable<IListItem> source, int id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static IListItem GetById(this IQueryable<IListItem> source, int id, params Expression<Func<IListItem, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, l => l.Id == id, selectors);
        }

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id asynchronously
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static async Task<IListItem> GetByIdAsync(this IQueryable<IListItem> source, int id)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByIdAsync(id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a list item (IListItem) by Id asynchronously
        /// </summary>
        /// <param name="source">The collection of lists items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list item instance, if any</returns>
        public static async Task<IListItem> GetByIdAsync(this IQueryable<IListItem> source, int id, params Expression<Func<IListItem, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, l => l.Id == id, selectors).ConfigureAwait(false);
        }

        #endregion

    }
}
