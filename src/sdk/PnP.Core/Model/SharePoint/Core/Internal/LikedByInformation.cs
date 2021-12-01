namespace PnP.Core.Model.SharePoint
{
    [SharePointType("Microsoft.SharePoint.Likes.likedByInformation")]
    internal sealed class LikedByInformation : BaseDataModel<ILikedByInformation>, ILikedByInformation
    {
        public bool IsLikedByUser { get => GetModelValue<bool>(); set => SetModelValue(value); }

        public string LikeCount { get => GetValue<string>(); set => SetValue(value); }

        public ICommentLikeUserEntityCollection LikedBy { get => GetModelCollectionValue<ICommentLikeUserEntityCollection>(); }

        [KeyProperty(nameof(LikeCount))]
        public override object Key { get => LikeCount; set => LikeCount = value.ToString(); }
    }
}
