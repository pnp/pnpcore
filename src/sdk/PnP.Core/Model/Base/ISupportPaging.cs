using System;
using System.Collections.Generic;
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
        Task<IEnumerable<TModel>> GetPagedAsync(int pageSize, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Gets the first page with a given size
        /// </summary>
        /// <param name="pageSize">Size of the page to request, minimal size is 1</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns></returns>
        IEnumerable<TModel> GetPaged(int pageSize, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Gets the first page with a given size
        /// </summary>
        /// <param name="predicate">linq filter to use while getting the first page</param>
        /// <param name="pageSize">Size of the page to request, minimal size is 1</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns></returns>
        Task<IEnumerable<TModel>> GetPagedAsync(Expression<Func<TModel, bool>> predicate, int pageSize, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Gets the first page with a given size
        /// </summary>
        /// <param name="predicate">linq filter to use while getting the first page</param>
        /// <param name="pageSize">Size of the page to request, minimal size is 1</param>
        /// <param name="expressions">The properties to select</param>
        /// <returns></returns>
        IEnumerable<TModel> GetPaged(Expression<Func<TModel, bool>> predicate, int pageSize, params Expression<Func<TModel, object>>[] expressions);

        /// <summary>
        /// Gets the next page in a series
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TModel>> GetNextPageAsync();

        /// <summary>
        /// Gets the next page in a series
        /// </summary>
        /// <returns></returns>
        IEnumerable<TModel> GetNextPage();

        /// <summary>
        /// Retrieves all pages
        /// </summary>
        /// <returns></returns>
        Task GetAllPagesAsync();

        /// <summary>
        /// Retrieves all pages
        /// </summary>
        /// <returns></returns>
        void GetAllPages();
    }
}
