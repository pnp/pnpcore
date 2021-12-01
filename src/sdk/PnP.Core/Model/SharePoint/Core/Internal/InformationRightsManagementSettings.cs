using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// InformationRightsManagementSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.InformationRightsManagementSettings", Target = typeof(InformationRightsManagementSettings), Uri = "_api/web/lists/getbyid(guid'{Parent.Id}')/InformationRightsManagementSettings")]
    internal sealed class InformationRightsManagementSettings : BaseDataModel<IInformationRightsManagementSettings>, IInformationRightsManagementSettings
    {
        #region Construction
        public InformationRightsManagementSettings()
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

        public int LicenseCacheExpireDays { get => GetValue<int>(); set => SetValue(value); }

        public string PolicyDescription { get => GetValue<string>(); set => SetValue(value); }

        public string PolicyTitle { get => GetValue<string>(); set => SetValue(value); }

        public string TemplateId { get => GetValue<string>(); set => SetValue(value); }

        [KeyProperty(nameof(PolicyTitle))]
        public override object Key { get => PolicyTitle; set => PolicyTitle = value.ToString(); }

        [SharePointProperty("*")]
        public object All { get => null; }
        #endregion
    }
}
