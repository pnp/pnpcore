namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Defines if and who liked a list item.
    /// </summary>
    [ConcreteType(typeof(LikedByInformation))]
    public interface ILikedByInformation : IDataModel<ILikedByInformation>, IDataModelGet<ILikedByInformation>, IDataModelLoad<ILikedByInformation>
    {
        /// <summary>
        /// Is this list item liked?
        /// </summary>
        public bool IsLikedByUser { get; }

        /// <summary>
        /// Number of likes this list item got.
        /// </summary>
        public string LikeCount { get; }

        /// <summary>
        /// The people that liked this list item.
        /// </summary>
        public ICommentLikeUserEntityCollection LikedBy { get; }
    }
}
