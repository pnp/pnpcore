using System.Linq;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a collection of userEntity objects
    /// </summary>
    [ConcreteType(typeof(CommentUserEntityCollection))]
    public interface ICommentUserEntityCollection : IQueryable<ICommentUserEntity>, IDataModelCollection<ICommentUserEntity>
    {
    }
}
