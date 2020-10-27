using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ChangeSite class, write your custom code here
    /// </summary>
    [SharePointType("SP.ChangeSite", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ChangeSite : BaseDataModel<IChangeSite>, IChangeSite
    {
        #region Construction
        public ChangeSite()
        {
        }
        #endregion

        #region Properties
        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
