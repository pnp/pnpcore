using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// CommunityModeration class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Portal.CommunityModeration", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class CommunityModeration : BaseDataModel<ICommunityModeration>, ICommunityModeration
    {
        #region Construction
        public CommunityModeration()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string Id4a81de82eeb94d6080ea5bf63e27023a { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
