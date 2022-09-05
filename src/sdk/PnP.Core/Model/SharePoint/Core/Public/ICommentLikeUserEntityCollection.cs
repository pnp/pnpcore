using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of userEntity objects
    /// </summary>
    [ConcreteType(typeof(CommentLikeUserEntityCollection))]
    public interface ICommentLikeUserEntityCollection : IQueryable<ICommentLikeUserEntity>, IDataModelCollection<ICommentLikeUserEntity>, ISupportModules<ICommentLikeUserEntity>
    {
    }
}
