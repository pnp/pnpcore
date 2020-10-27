using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// CompatibilityRange class, write your custom code here
    /// </summary>
    [SharePointType("SP.CompatibilityRange", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class CompatibilityRange : BaseDataModel<ICompatibilityRange>, ICompatibilityRange
    {
        #region Construction
        public CompatibilityRange()
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
