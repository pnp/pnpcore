using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a DlpPolicyTip object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class DlpPolicyTip : BaseDataModel<IDlpPolicyTip>, IDlpPolicyTip
    {

        #region New properties

        public string AppliedActionsText { get => GetValue<string>(); set => SetValue(value); }

        public string ComplianceUrl { get => GetValue<string>(); set => SetValue(value); }

        public string GeneralText { get => GetValue<string>(); set => SetValue(value); }

        public DateTime LastProcessedTime { get => GetValue<DateTime>(); set => SetValue(value); }

        public int OverrideOptions { get => GetValue<int>(); set => SetValue(value); }

        public string TwoLetterISOLanguageName { get => GetValue<string>(); set => SetValue(value); }

        #endregion

    }
}
