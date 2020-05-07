using System.Threading.Tasks;

namespace PnP.Core.Model
{
    /// <summary>
    /// Paged data retrieval can be done using the methods provided via this interface
    /// </summary>
    public interface ISupportPaging
    {
        /// <summary>
        /// Determines whether paging is possible
        /// </summary>
        bool CanPage
        {
            get;
        }

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
