using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// RecycleBinItem extension methods
    /// </summary>
    public static class RecycleBinItemExtensions
    {
        #region GetById for RecycleBin Items implementation

        /// <summary>
        /// Extension method to select a recycle bin item (<c>IRecycleBinItem</c>) by id
        /// </summary>
        /// <param name="source">The collection of recycle bin items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting recycle bin item instance, if any</returns>
        public static IRecycleBinItem GetById(this IQueryable<IRecycleBinItem> source, Guid id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a recycle bin item (<c>IRecycleBinItem</c>) by id
        /// </summary>
        /// <param name="source">The collection of recycle bin items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting recycle bin item instance, if any</returns>
        public static IRecycleBinItem GetById(this IQueryable<IRecycleBinItem> source, Guid id, params Expression<Func<IRecycleBinItem, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, l => l.Id == id, selectors);
        }

        /// <summary>
        /// Extension method to select a recycle bin item (<c>IRecycleBinItem</c>) by id asynchronously
        /// </summary>
        /// <param name="source">The collection of recycle bin items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <returns>The resulting recycle bin item instance, if any</returns>
        public static async Task<IRecycleBinItem> GetByIdAsync(this IQueryable<IRecycleBinItem> source, Guid id)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByIdAsync(id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a recycle bin item (<c>IRecycleBinItem</c>) by id asynchronously
        /// </summary>
        /// <param name="source">The collection of recycle bin items to get the item by Id from</param>
        /// <param name="id">The Id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting recycle bin item instance, if any</returns>
        public static async Task<IRecycleBinItem> GetByIdAsync(this IQueryable<IRecycleBinItem> source, Guid id, params Expression<Func<IRecycleBinItem, object>>[] selectors)
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
