using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a EffectiveInformationRightsManagementSettings object
    /// </summary>
    [ConcreteType(typeof(EffectiveInformationRightsManagementSettings))]
    public interface IEffectiveInformationRightsManagementSettings : IDataModel<IEffectiveInformationRightsManagementSettings>, IDataModelLoad<IEffectiveInformationRightsManagementSettings>, IDataModelGet<IEffectiveInformationRightsManagementSettings>
    {
        /// <summary>
        /// Gets whether print is allowed for this file.
        /// </summary>
        public bool AllowPrint { get; }

        /// <summary>
        /// Gets whether script is alloed for this file.
        /// </summary>
        public bool AllowScript { get; }

        /// <summary>
        /// Gets whether writing copy is allowed for this file.
        /// </summary>
        public bool AllowWriteCopy { get; }

        /// <summary>
        /// Gets whether document browser view is disabled for this file.
        /// </summary>
        public bool DisableDocumentBrowserView { get; }

        /// <summary>
        /// Gets the access expiry in days for this file.
        /// </summary>
        public int DocumentAccessExpireDays { get; }

        /// <summary>
        /// Gets the document library protection expiry date.
        /// </summary>
        public DateTime DocumentLibraryProtectionExpireDate { get; }

        /// <summary>
        /// Gets access expiry for this file.
        /// </summary>
        public bool EnableDocumentAccessExpire { get; }

        /// <summary>
        /// Gets whether document browser publishing view is enabled for this file.
        /// </summary>
        public bool EnableDocumentBrowserPublishingView { get; }

        /// <summary>
        /// Gets whether group protection is enabled on this file.
        /// </summary>
        public bool EnableGroupProtection { get; }

        /// <summary>
        /// Gets whether license cache expiry is enabled on this file.
        /// </summary>
        public bool EnableLicenseCacheExpire { get; }

        /// <summary>
        /// Gets the IRM group name of the file.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets whether IRM is enabled on this file.
        /// </summary>
        public bool IrmEnabled { get; }

        /// <summary>
        /// Gets the license cache expiry in days for this file.
        /// </summary>
        public int LicenseCacheExpireDays { get; }

        /// <summary>
        /// Gets the policy description for this file.
        /// </summary>
        public string PolicyDescription { get; }

        /// <summary>
        /// Gets the policy title for this file.
        /// </summary>
        public string PolicyTitle { get; }

        /// <summary>
        /// Gets the source of this setting.
        /// </summary>
        public SPEffectiveInformationRightsManagementSettingsSource SettingSource { get; }

        /// <summary>
        /// Gets the IRM template id for this file.
        /// </summary>
        public string TemplateId { get; }
    }
}
