using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// userEntity class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Likes.userEntity", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class CommentUserEntity : BaseDataModel<ICommentUserEntity>, ICommentUserEntity
    {
        #region Construction
        public CommentUserEntity()
        {
        }
        #endregion

        #region Properties

        [SharePointProperty("email")]
        public string Mail { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }

        #endregion

        #region Extension methods
        #endregion
    }
}

