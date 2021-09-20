using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// userEntity class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Likes.userEntity", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class userEntity : BaseDataModel<IuserEntity>, IuserEntity
    {
        #region Construction
        public userEntity()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public DateTime CreationDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public string Email { get => GetValue<string>(); set => SetValue(value); }

        public int Id { get => GetValue<int>(); set => SetValue(value); }

        public string LoginName { get => GetValue<string>(); set => SetValue(value); }

        public string Name { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        [KeyProperty(nameof(Id))]
        public override object Key { get => Id; set => Id = (int)value; }


        #endregion

        #region Extension methods
        #endregion
    }
}
