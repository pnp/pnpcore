using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// likedByInformation class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Likes.likedByInformation", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class likedByInformation : BaseDataModel<IlikedByInformation>, IlikedByInformation
    {
        #region Construction
        public likedByInformation()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool IsLikedByUser { get => GetValue<bool>(); set => SetValue(value); }

        public IuserEntityCollection LikedBy { get => GetModelCollectionValue<IuserEntityCollection>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
