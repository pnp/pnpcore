using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// userEntity class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Likes.userEntity")]
    internal partial class CommentLikeUserEntity : BaseDataModel<ICommentLikeUserEntity>, ICommentLikeUserEntity
    {
        #region Construction
        public CommentLikeUserEntity()
        {
        }
        #endregion

        #region Properties

        public DateTime CreationDate { get => GetValue<DateTime>(); set => SetValue(value); }

        [SharePointProperty("email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }

        #endregion
    }
}

