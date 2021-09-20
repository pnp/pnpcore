using PnP.Core.QueryModel;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    internal partial class CommentLikeUserEntityCollection : QueryableDataModelCollection<ICommentLikeUserEntity>, ICommentLikeUserEntityCollection
    {
        public CommentLikeUserEntityCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }
    }
}
