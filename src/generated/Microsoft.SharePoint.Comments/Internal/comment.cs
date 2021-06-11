using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// comment class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Comments.comment", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class comment : BaseDataModel<Icomment>, Icomment
    {
        #region Construction
        public comment()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public DateTime CreatedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Id { get => GetValue<string>(); set => SetValue(value); }

        public bool IsLikedByUser { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsReply { get => GetValue<bool>(); set => SetValue(value); }

        public int ItemId { get => GetValue<int>(); set => SetValue(value); }

        public int LikeCount { get => GetValue<int>(); set => SetValue(value); }

        public Guid ListId { get => GetValue<Guid>(); set => SetValue(value); }

        public string ParentId { get => GetValue<string>(); set => SetValue(value); }

        public string RelativeCreatedDate { get => GetValue<string>(); set => SetValue(value); }

        public int ReplyCount { get => GetValue<int>(); set => SetValue(value); }

        public string Text { get => GetValue<string>(); set => SetValue(value); }

        public IuserEntityCollection LikedBy { get => GetModelCollectionValue<IuserEntityCollection>(); }


        public IcommentCollection Replies { get => GetModelCollectionValue<IcommentCollection>(); }


        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = value.ToString(); }


        #endregion

        #region Extension methods
        #endregion
    }
}
