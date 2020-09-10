using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// List extension methods
    /// </summary>
    public static class ListExtensions
    {
        #region GetByTitle implementation

        /// <summary>
        /// Extension method to select a list (IList) by title
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <returns>The resulting list instance, if any</returns>
        public static IList GetByTitle(this IQueryable<IList> source, string title)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetByTitle(title, null);
        }

        /// <summary>
        /// Extension method to select a list (IList) by title
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static IList GetByTitle(this IQueryable<IList> source, string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, l => l.Title == title, selectors);
        }

        /// <summary>
        /// Extension method to select a list (IList) by title asynchronously
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <returns>The resulting list instance, if any</returns>
        public static async Task<IList> GetByTitleAsync(this IQueryable<IList> source, string title)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByTitleAsync(title, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a list (IList) by title asynchronously
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="title">The title to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static async Task<IList> GetByTitleAsync(this IQueryable<IList> source, string title, params Expression<Func<IList, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, l => l.Title == title, selectors).ConfigureAwait(false);
        }

        #endregion

        #region GetById implementation

        /// <summary>
        /// Extension method to select a list (IList) by id
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting list instance, if any</returns>
        public static IList GetById(this IQueryable<IList> source, Guid id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a list (IList) by id
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static IList GetById(this IQueryable<IList> source, Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, l => l.Id == id, selectors);
        }

        /// <summary>
        /// Extension method to select a list (IList) by id
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting list instance, if any</returns>
        public static async Task<IList> GetByIdAsync(this IQueryable<IList> source, Guid id)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByIdAsync(id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a list (IList) by id
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static async Task<IList> GetByIdAsync(this IQueryable<IList> source, Guid id, params Expression<Func<IList, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, l => l.Id == id, selectors).ConfigureAwait(false);
        }

        #endregion

    }
}
