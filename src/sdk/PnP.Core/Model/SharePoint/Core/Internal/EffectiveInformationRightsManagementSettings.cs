using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// EffectiveInformationRightsManagementSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.EffectiveInformationRightsManagementSettings", Target = typeof(IFile))]
    internal partial class EffectiveInformationRightsManagementSettings : BaseDataModel<IEffectiveInformationRightsManagementSettings>, IEffectiveInformationRightsManagementSettings
    {
        #region Construction
        public EffectiveInformationRightsManagementSettings()
        {
        }
        #endregion

        #region Properties
        public bool AllowPrint { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowScript { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowWriteCopy { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableDocumentBrowserView { get => GetValue<bool>(); set => SetValue(value); }

        public int DocumentAccessExpireDays { get => GetValue<int>(); set => SetValue(value); }

        public DateTime DocumentLibraryProtectionExpireDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public bool EnableDocumentAccessExpire { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableDocumentBrowserPublishingView { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableGroupProtection { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableLicenseCacheExpire { get => GetValue<bool>(); set => SetValue(value); }

        public string GroupName { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int LicenseCacheExpireDays { get => GetValue<int>(); set => SetValue(value); }

        public string PolicyDescription { get => GetValue<string>(); set => SetValue(value); }

        public string PolicyTitle { get => GetValue<string>(); set => SetValue(value); }

        public SPEffectiveInformationRightsManagementSettingsSource SettingSource { get => GetValue<SPEffectiveInformationRightsManagementSettingsSource>(); set => SetValue(value); }

        public string TemplateId { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(PolicyTitle))]
        public override object Key { get => PolicyTitle; set => PolicyTitle = value.ToString(); }
        #endregion

    }
}
