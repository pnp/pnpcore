using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// DlpPolicyTip class, write your custom code here
    /// </summary>
    [SharePointType("SP.DlpPolicyTip", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class DlpPolicyTip : BaseDataModel<IDlpPolicyTip>, IDlpPolicyTip
    {
        #region Construction
        public DlpPolicyTip()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AppliedActionsText { get => GetValue<string>(); set => SetValue(value); }

        public string ComplianceUrl { get => GetValue<string>(); set => SetValue(value); }

        public string GeneralText { get => GetValue<string>(); set => SetValue(value); }

        public DateTime LastProcessedTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public int OverrideOptions { get => GetValue<int>(); set => SetValue(value); }

        public string TwoLetterISOLanguageName { get => GetValue<string>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
