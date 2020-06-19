using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Paged data retrieval can be done using the methods provided via this interface
    /// </summary>
    public interface ISupportPaging<TModel>
    {
        /// <summary>
        /// Determines whether paging is possible
        /// </summary>
        bool CanPage
        {
            get;
        }

        /// <summary>
        /// Gets the first page with a given size
        /// </summary>
        /// <param name="pageSize">Size of the page to request, minimal size is 1</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns></returns>
        Task GetPagedAsync(int pageSize, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Gets the next page in a series
        /// </summary>
        /// <returns></returns>
        Task GetNextPageAsync();

        /// <summary>
        /// Retrieves all pages
        /// </summary>
        /// <returns></returns>
        Task GetAllPagesAsync();
    }
}
