using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SiteLinkingManager class, write your custom code here
    /// </summary>
    [SharePointType("Microsoft.SharePoint.Portal.SiteLinkingManager", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SiteLinkingManager : BaseDataModel<ISiteLinkingManager>, ISiteLinkingManager
    {
        #region Construction
        public SiteLinkingManager()
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
