using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// ModernizeHomepageResult class, write your custom code here
    /// </summary>
    [SharePointType("SP.ModernizeHomepageResult", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class ModernizeHomepageResult : BaseDataModel<IModernizeHomepageResult>, IModernizeHomepageResult
    {
        #region Construction
        public ModernizeHomepageResult()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool CanModernizeHomepage { get => GetValue<bool>(); set => SetValue(value); }

        public string Reason { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
