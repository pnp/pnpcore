using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of comment objects
    /// </summary>
    [ConcreteType(typeof(CommentCollection))]
    public interface ICommentCollection : IQueryable<IComment>, IDataModelCollection<IComment>
    {
        #region Add Methods

        /// <summary>
        /// Adds a comment to a list item
        /// </summary>
        /// <param name="text">Comment to add</param>
        /// <returns>Newly added comment</returns>
        public Task<IComment> AddAsync(string text);

        #endregion
    }
}