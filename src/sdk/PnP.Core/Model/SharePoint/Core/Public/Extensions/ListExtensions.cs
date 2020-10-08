using PnP.Core.QueryModel;
using PnP.Core.Services;
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

        #region GetByServerRelativeUrl implementation
        /// <summary>
        /// Extension method to select a list (IList) by server relative url
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="serverRelativeUrl">The server relative url of the list to return</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static async Task<IList> GetByServerRelativeUrlAsync(this IQueryable<IList> source, string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrEmpty(serverRelativeUrl))
            {
                throw new ArgumentException(PnPCoreResources.Exception_GetListByServerRelativeUrl_ServerRelativeUrl);
            }

            return await BaseDataModelExtensions.BaseGetAsync(source, new ApiCall($"_api/web/getlist('{serverRelativeUrl}')", ApiType.SPORest), selectors).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a list (IList) by server relative url
        /// </summary>
        /// <param name="source">The collection of lists to get the list by title from</param>
        /// <param name="serverRelativeUrl">The server relative url of the list to return</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting list instance, if any</returns>
        public static IList GetByServerRelativeUrl(this IQueryable<IList> source, string serverRelativeUrl, params Expression<Func<IList, object>>[] selectors)
        {
            if (serverRelativeUrl == null)
            {
                throw new ArgumentNullException(nameof(serverRelativeUrl));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return GetByServerRelativeUrlAsync(source, serverRelativeUrl, selectors).GetAwaiter().GetResult();
        }
        #endregion

    }
}
