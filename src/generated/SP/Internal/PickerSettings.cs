using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// PickerSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.PickerSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class PickerSettings : BaseDataModel<IPickerSettings>, IPickerSettings
    {
        #region Construction
        public PickerSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public bool AllowEmailAddresses { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowOnlyEmailAddresses { get => GetValue<bool>(); set => SetValue(value); }

        public string PrincipalAccountType { get => GetValue<string>(); set => SetValue(value); }

        public int PrincipalSource { get => GetValue<int>(); set => SetValue(value); }

        public bool UseSubstrateSearch { get => GetValue<bool>(); set => SetValue(value); }

        public int VisibleSuggestions { get => GetValue<int>(); set => SetValue(value); }

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
