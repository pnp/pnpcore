using PnP.Core.Model.Security;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a comment.
    /// </summary>
    [ConcreteType(typeof(Comment))]
    public interface IComment : IDataModel<IComment>, IDataModelGet<IComment>, IDataModelLoad<IComment> /*, IDataModelUpdate*/, IDataModelDelete
    {

        /// <summary>
        /// Comment creation date.
        /// </summary>
        public DateTime CreatedDate { get; }

        /// <summary>
        /// Comment id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Flag that indicates whether the user in the current context liked the comment.
        /// </summary>
        public bool IsLikedByUser { get; }

        /// <summary>
        /// Comment is a reply to another comment.
        /// </summary>
        public bool IsReply { get; }

        /// <summary>
        /// Comment item id.
        /// </summary>
        public int ItemId { get; }

        /// <summary>
        /// Number of likes for the comment.
        /// </summary>
        public int LikeCount { get; }

        /// <summary>
        /// Comment list id.
        /// </summary>
        public Guid ListId { get; }

        /// <summary>
        /// Comment parent ID (0 if not a reply).
        /// </summary>
        public string ParentId { get; }

        /// <summary>
        /// Gets a string that represents the relative value of the comment's creation date.
        /// </summary>
        public string RelativeCreatedDate { get; }

        /// <summary>
        /// Number of replies to the comment.
        /// </summary>
        public int ReplyCount { get; }

        /// <summary>
        /// Comment text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Comment author.
        /// </summary>
        public ISharePointSharingPrincipal Author { get; }

        /// <summary>
        /// List of users who have liked the comment.
        /// </summary>
        public ICommentUserEntityCollection LikedBy { get; }

        /// <summary>
        /// Replies to the comment.
        /// </summary>
        public ICommentCollection Replies { get; }

        #region Like/Unlike

        /// <summary>
        /// Likes the comment for the user in the current context.
        /// </summary>
        public Task LikeAsync();

        /// <summary>
        /// Likes the comment for the user in the current context.
        /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords
        public void Like();
#pragma warning restore CA1716 // Identifiers should not match keywords

        /// <summary>
        /// Unlikes the comment for the user in the current context.
        /// </summary>
        /// <returns></returns>
        public Task UnlikeAsync();

        /// <summary>
        /// Unlikes the comment for the user in the current context.
        /// </summary>
        /// <returns></returns>
        public void Unlike();

        #endregion
    }
}
