using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a InformationRightsManagementFileSettings object
    /// </summary>
    [ConcreteType(typeof(InformationRightsManagementFileSettings))]
    public interface IInformationRightsManagementFileSettings : IDataModel<IInformationRightsManagementFileSettings>, IDataModelGet<IInformationRightsManagementFileSettings>, IDataModelUpdate, IDataModelDelete
    {

        #region Existing properties

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowPrint { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowScript { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool AllowWriteCopy { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool DisableDocumentBrowserView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int DocumentAccessExpireDays { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableDocumentAccessExpire { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableDocumentBrowserPublishingView { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableGroupProtection { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool EnableLicenseCacheExpire { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public int LicenseCacheExpireDays { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PolicyDescription { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// To update...
        /// </summary>
        public string TemplateId { get; set; }

        #endregion

        #region New properties

        #endregion

    }
}
