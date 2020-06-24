using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a PickerSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class PickerSettings : BaseDataModel<IPickerSettings>, IPickerSettings
    {

        #region New properties

        public bool AllowEmailAddresses { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowOnlyEmailAddresses { get => GetValue<bool>(); set => SetValue(value); }

        public string PrincipalAccountType { get => GetValue<string>(); set => SetValue(value); }

        public int PrincipalSource { get => GetValue<int>(); set => SetValue(value); }

        public int VisibleSuggestions { get => GetValue<int>(); set => SetValue(value); }

        #endregion

    }
}
