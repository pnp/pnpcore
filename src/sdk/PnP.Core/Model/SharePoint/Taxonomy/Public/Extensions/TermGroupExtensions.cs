using PnP.Core.QueryModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// TermGroup extension methods
    /// </summary>
    public static class TermGroupExtensions
    {
        #region GetById implementation
        
        /// <summary>
        /// Extension method to select a term group by id
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static ITermGroup GetById(this IQueryable<ITermGroup> source, string id)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetById(id, null);
        }

        /// <summary>
        /// Extension method to select a term group by id
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static ITermGroup GetById(this IQueryable<ITermGroup> source, string id, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, c => c.Id == id, selectors);
        }

        /// <summary>
        /// Extension method to select a term group by id
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="id">The id to search for</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static async Task<ITermGroup> GetByIdAsync(this IQueryable<ITermGroup> source, string id)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByIdAsync(id, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a term group by id
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="id">The id to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static async Task<ITermGroup> GetByIdAsync(this IQueryable<ITermGroup> source, string id, params Expression<Func<ITermGroup, object>>[] selectors)
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

        #region GetByName implementation

        /// <summary>
        /// Extension method to select a term group by name
        /// </summary>
        /// <param name="source">The collection of groups to get the group by name from</param>
        /// <param name="name">The name to search for</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static ITermGroup GetByName(this IQueryable<ITermGroup> source, string name)
        {
            // Just rely on the below overload, without providing any selector
            return source.GetByName(name, null);
        }

        /// <summary>
        /// Extension method to select a term group by name
        /// </summary>
        /// <param name="source">The collection of groups to get the group by name from</param>
        /// <param name="name">The name to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static ITermGroup GetByName(this IQueryable<ITermGroup> source, string name, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return BaseDataModelExtensions.BaseLinqGet(source, c => c.Name == name, selectors);
        }

        /// <summary>
        /// Extension method to select a term group by name
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="name">The name to search for</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static async Task<ITermGroup> GetByNameAsync(this IQueryable<ITermGroup> source, string name)
        {
            // Just rely on the below overload, without providing any selector
            return await source.GetByNameAsync(name, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Extension method to select a term group by name
        /// </summary>
        /// <param name="source">The collection of groups to get the group by id from</param>
        /// <param name="name">The name to search for</param>
        /// <param name="selectors">The expressions declaring the fields to select</param>
        /// <returns>The resulting term group instance, if any</returns>
        public static async Task<ITermGroup> GetByNameAsync(this IQueryable<ITermGroup> source, string name, params Expression<Func<ITermGroup, object>>[] selectors)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return await BaseDataModelExtensions.BaseLinqGetAsync(source, l => l.Name == name, selectors).ConfigureAwait(false);
        }

        #endregion
    }
}
