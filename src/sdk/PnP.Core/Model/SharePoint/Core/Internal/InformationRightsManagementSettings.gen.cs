using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a InformationRightsManagementSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    internal partial class InformationRightsManagementSettings : BaseDataModel<IInformationRightsManagementSettings>, IInformationRightsManagementSettings
    {

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
        public override object Key { get => this.PolicyTitle; set => this.PolicyTitle = value.ToString(); }
    }
}
