using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Represents a single user within a comment's likedBy list in the comments API.
    /// </summary>
    [ConcreteType(typeof(CommentUserEntity))]
    public interface ICommentUserEntity : IDataModel<ICommentUserEntity>, IDataModelGet<ICommentUserEntity>, IDataModelUpdate, IDataModelDelete
    {

        /// <summary>
        /// The user's email.
        /// SPO REST property : Email
        /// </summary>
        public string Mail { get; set; }

        /// <summary>
        /// The user's numerical ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The user's loginName.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// The user's name.
        /// </summary>
        public string Name { get; set; }

    }
}