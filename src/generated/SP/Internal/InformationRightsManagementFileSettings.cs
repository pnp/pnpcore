using System;
using PnP.Core.Services;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// InformationRightsManagementFileSettings class, write your custom code here
    /// </summary>
    [SharePointType("SP.InformationRightsManagementFileSettings", Uri = "_api/xxx", LinqGet = "_api/xxx")]
    internal partial class InformationRightsManagementFileSettings : BaseDataModel<IInformationRightsManagementFileSettings>, IInformationRightsManagementFileSettings
    {
        #region Construction
        public InformationRightsManagementFileSettings()
        {
        }
        #endregion

        #region Properties
        #region Existing properties

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

        #endregion

        #region New properties

        #endregion

        #endregion

        #region Extension methods
        #endregion
    }
}
