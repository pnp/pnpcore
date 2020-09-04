namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Internal class representing a InformationRightsManagementFileSettings object
    /// 
    /// Note: this class is generated, please don't modify this code by hand!
    /// 
    /// </summary>
    [SharePointType("SP.InformationRightsManagementFileSettings")]
    internal partial class InformationRightsManagementFileSettings : BaseComplexType<IInformationRightsManagementFileSettings>, IInformationRightsManagementFileSettings
    {
        public bool AllowPrint { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowScript { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowWriteCopy { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableDocumentBrowserView { get => GetValue<bool>(); set => SetValue(value); }

        public int DocumentAccessExpireDays { get => GetValue<int>(); set => SetValue(value); }

        public bool EnableDocumentAccessExpire { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableDocumentBrowserPublishingView { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableGroupProtection { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableLicenseCacheExpire { get => GetValue<bool>(); set => SetValue(value); }

        public string GroupName { get => GetValue<string>(); set => SetValue(value); }

        public bool IrmEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int LicenseCacheExpireDays { get => GetValue<int>(); set => SetValue(value); }

        public string PolicyDescription { get => GetValue<string>(); set => SetValue(value); }

        public string PolicyTitle { get => GetValue<string>(); set => SetValue(value); }

        public string TemplateId { get => GetValue<string>(); set => SetValue(value); }
    }
}
