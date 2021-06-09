using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal partial class CommentCollection : QueryableDataModelCollection<IComment>, ICommentCollection
    {
        public CommentCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public async Task<IComment> AddAsync(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var newComment = CreateNewAndAdd() as Comment;

            newComment.Text = text;

            return await newComment.AddAsync().ConfigureAwait(false) as Comment;
        }

        #endregion
    }
}
