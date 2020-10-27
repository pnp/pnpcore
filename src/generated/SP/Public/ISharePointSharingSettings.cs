using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a SharePointSharingSettings object
    /// </summary>
    [ConcreteType(typeof(SharePointSharingSettings))]
    public interface ISharePointSharingSettings : IDataModel<ISharePointSharingSettings>, IDataModelGet<ISharePointSharingSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region New properties

        /// <summary>
        /// To update...
        /// </summary>
        public string AddToGroupModeName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IsMobileView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PanelGivePermissionsVisible { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PanelShowHideMoreOptionsVisible { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool PanelSimplifiedRoleSelectorVisible { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SelectedGroup { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool SharedWithEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string SharingCssLink { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool TabbedDialogEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int TabToShow { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TxtEmailSubjectText { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string UserDisplayUrl { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public IPickerSettings PickerProperties { get; }

        #endregion

    }
}
