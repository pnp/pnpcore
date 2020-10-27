using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ListHomeItemCollection class, write your custom code here
    /// </summary>
    [SharePointType("SP.ListHomeItemCollection", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ListHomeItemCollection : BaseDataModel<IListHomeItemCollection>, IListHomeItemCollection
    {
        #region Construction
        public ListHomeItemCollection()
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
