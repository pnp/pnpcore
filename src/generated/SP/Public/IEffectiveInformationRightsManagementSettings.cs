using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a EffectiveInformationRightsManagementSettings object
    /// </summary>
    [ConcreteType(typeof(EffectiveInformationRightsManagementSettings))]
    public interface IEffectiveInformationRightsManagementSettings : IDataModel<IEffectiveInformationRightsManagementSettings>, IDataModelGet<IEffectiveInformationRightsManagementSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowPrint { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowScript { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowWriteCopy { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableDocumentBrowserView { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DocumentAccessExpireDays { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public DateTime DocumentLibraryProtectionExpireDate { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableDocumentAccessExpire { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableDocumentBrowserPublishingView { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableGroupProtection { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableLicenseCacheExpire { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmEnabled { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LicenseCacheExpireDays { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PolicyDescription { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PolicyTitle { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public int SettingSource { get; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TemplateId { get; }

        #endregion

        #region New properties

        #endregion

    }
}
