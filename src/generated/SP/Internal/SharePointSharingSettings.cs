using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// SharePointSharingSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.SharePointSharingSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class SharePointSharingSettings : BaseDataModel<ISharePointSharingSettings>, ISharePointSharingSettings
    {
        #region Construction
        public SharePointSharingSettings()
        {
        }
        #endregion

        #region Properties
        #region New properties

        public string AddToGroupModeName { get => GetValue<string>(); set => SetValue(value); }

        public bool IsMobileView { get => GetValue<bool>(); set => SetValue(value); }

        public bool PanelGivePermissionsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public bool PanelShowHideMoreOptionsVisible { get => GetValue<bool>(); set => SetValue(value); }

        public bool PanelSimplifiedRoleSelectorVisible { get => GetValue<bool>(); set => SetValue(value); }

        public string SelectedGroup { get => GetValue<string>(); set => SetValue(value); }

        public bool SharedWithEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public string SharingCssLink { get => GetValue<string>(); set => SetValue(value); }

        public bool TabbedDialogEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int TabToShow { get => GetValue<int>(); set => SetValue(value); }

        public string TxtEmailSubjectText { get => GetValue<string>(); set => SetValue(value); }

        public string UserDisplayUrl { get => GetValue<string>(); set => SetValue(value); }

        public IPickerSettings PickerProperties { get => GetModelValue<IPickerSettings>(); }


        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
