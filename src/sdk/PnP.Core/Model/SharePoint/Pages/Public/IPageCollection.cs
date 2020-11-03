using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// A collection of modern pages
    /// </summary>
    [ConcreteType(typeof(PageCollection))]
    public interface IPageCollection : IQueryable<IPage>, IDataModelCollection<IPage>
    {
        #region GetByTitle methods
        /// <summary>
        /// Select a page by title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <returns>The resulting page instance, if any</returns>
        public IPage GetByTitle(string title);

        /// <summary>
        /// Select a page by title
        /// </summary>
        /// <param name="title">The title to search for</param>
        /// <returns>The resulting page instance, if any</returns>
        public Task<IPage> GetByTitleAsync(string title);
        #endregion
    }
}
