using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a SharePointSharingSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class SharePointSharingSettings : BaseDataModel<ISharePointSharingSettings>, ISharePointSharingSettings
    {

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

        public IPickerSettings PickerProperties
        {
            get
            {
                if (!NavigationPropertyInstantiated())
                {
                    var propertyValue = new PickerSettings
                    {
                        PnPContext = this.PnPContext,
                        Parent = this,
                    };
                    SetValue(propertyValue);
                    InstantiateNavigationProperty();
                }
                return GetValue<IPickerSettings>();
            }
            set
            {
                InstantiateNavigationProperty();
                SetValue(value);                
            }
        }


        #endregion

    }
}
