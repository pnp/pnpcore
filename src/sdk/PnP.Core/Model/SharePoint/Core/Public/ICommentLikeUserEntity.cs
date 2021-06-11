using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a single user within a comment's likedBy list in the comments API.
    /// </summary>
    [ConcreteType(typeof(CommentLikeUserEntity))]
    public interface ICommentLikeUserEntity : IDataModel<ICommentLikeUserEntity>
    {

        /// <summary>
        /// when did the user do the "like".
        /// </summary>
        public DateTime CreationDate { get; }

        /// <summary>
        /// The user's email.
        /// SPO REST property : Email
        /// </summary>
        public string Mail { get; }

        /// <summary>
        /// The user's numerical ID.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The user's loginName.
        /// </summary>
        public string LoginName { get;  }

        /// <summary>
        /// The user's name.
        /// </summary>
        public string Name { get; }

    }
}