using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    internal sealed class CommentCollection : QueryableDataModelCollection<IComment>, ICommentCollection
    {
        public CommentCollection(PnPContext context, IDataModelParent parent, string memberName) : base(context, parent, memberName)
        {
            PnPContext = context;
            Parent = parent;
        }

        #region Add methods

        public IComment Add(string text)
        {
            return AddAsync(text).GetAwaiter().GetResult();
        }

        public async Task<IComment> AddAsync(string text)
        {
            return await CreateCommentToAdd(text).AddAsync().ConfigureAwait(false) as Comment;
        }

        public IComment AddBatch(string text)
        {
            return AddBatchAsync(text).GetAwaiter().GetResult();
        }

        public async Task<IComment> AddBatchAsync(string text)
        {
            return await AddBatchAsync(PnPContext.CurrentBatch, text).ConfigureAwait(false);
        }

        public IComment AddBatch(Batch batch, string text)
        {
            return AddBatchAsync(batch, text).GetAwaiter().GetResult();
        }

        public async Task<IComment> AddBatchAsync(Batch batch, string text)
        {
            return await CreateCommentToAdd(text).AddBatchAsync(batch).ConfigureAwait(false) as Comment;
        }

        private Comment CreateCommentToAdd(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var newComment = CreateNewAndAdd() as Comment;
            newComment.Text = text;
            return newComment;
        }
        #endregion

        #region DeleteAll method

        public async Task DeleteAllAsync()
        {
            var comment = new Comment()
            {
                PnPContext = PnPContext,
                Parent = this
            };
            var entity = EntityManager.GetClassInfo(comment.GetType(), comment);

            await comment.RequestAsync(new ApiCall($"{entity.SharePointLinqGet}/deleteall", apiType: ApiType.SPORest) { Commit = true }, HttpMethod.Post).ConfigureAwait(false);

            // remove the comments from the in-memory collection
            Clear();
        }

        public void DeleteAll()
        {
            DeleteAllAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region At mentioning

        public string GetAtMentioningString(string userName, string userPrincipalName, string email = null)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException(null, nameof(userName));
            }

            if (string.IsNullOrEmpty(userPrincipalName))
            {
                throw new ArgumentException(null, nameof(userPrincipalName));
            }

            if (string.IsNullOrEmpty(email))
            {
                email = userPrincipalName;
            }

            return $"<a data-sp-mention-user-id=\"{userPrincipalName}\" contenteditable=\"false\" access-type=\"0\" href=\"mailto:{email}\" tabindex=\"-1\">@{userName}</a>";
        }

        #endregion
    }
}
