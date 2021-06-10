using PnP.Core.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of comment objects.
    /// </summary>
    [ConcreteType(typeof(CommentCollection))]
    public interface ICommentCollection : IQueryable<IComment>, IDataModelCollection<IComment>
    {
        #region Add Methods

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <returns>Newly added comment</returns>
        public Task<IComment> AddAsync(string text);

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <returns>Newly added comment</returns>
        public IComment Add(string text);

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <returns>Newly added comment</returns>
        public Task<IComment> AddBatchAsync(string text);

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <returns>Newly added comment</returns>
        public IComment AddBatch(string text);

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <param name="batch">Batch to use</param>
        /// <returns>Newly added comment</returns>
        public Task<IComment> AddBatchAsync(Batch batch, string text);

        /// <summary>
        /// Adds a comment to a list item.
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <param name="batch">Batch to use</param>
        /// <returns>Newly added comment</returns>
        public IComment AddBatch(Batch batch, string text);

        #endregion

        #region DeleteAll method

        /// <summary>
        /// Delete all comments in this collection.
        /// </summary>
        /// <returns></returns>
        public Task DeleteAllAsync();

        /// <summary>
        /// Delete all comments in this collection.
        /// </summary>
        public void DeleteAll();

        #endregion
    }
}