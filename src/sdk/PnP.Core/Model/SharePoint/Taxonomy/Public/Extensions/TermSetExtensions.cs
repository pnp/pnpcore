using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TermSet extension methods
    /// </summary>
    public static class TermSetExtensions
    {
        #region GetById for TermSets implementation
        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="source">The collection of sets to get term set by id from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting term set instance, if any</returns>
        public static ITermSet GetById(this IQueryable<ITermSet> source, string id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="source">The collection of sets to get term set by id from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term set instance, if any</returns>
        public static ITermSet GetById(this IQueryable<ITermSet> source, string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, c => c.Id == id, selectors);
        }

        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="source">The collection of sets to get term set by id from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting term set instance, if any</returns>
        public static async Task<ITermSet> GetByIdAsync(this IQueryable<ITermSet> source, string id)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByIdAsync(id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a term set by id
        /// </summary>
        /// <param name="source">The collection of sets to get term set by id from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term set instance, if any</returns>
        public static async Task<ITermSet> GetByIdAsync(this IQueryable<ITermSet> source, string id, params Expression<Func<ITermSet, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, l => l.Id == id, selectors).ConfigureAwait(false);
        }
        #endregion

    }
}
