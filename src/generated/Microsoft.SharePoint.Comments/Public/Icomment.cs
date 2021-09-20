using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a comment object
    /// </summary>
    [ConcreteType(typeof(comment))]
    public interface Icomment : IDataModel<Icomment>, IDataModelGet<Icomment>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsLikedByUser { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsReply { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string RelativeCreatedDate { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IuserEntityCollection LikedBy { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public IcommentCollection Replies { get; }

        #endregion

    }
}
