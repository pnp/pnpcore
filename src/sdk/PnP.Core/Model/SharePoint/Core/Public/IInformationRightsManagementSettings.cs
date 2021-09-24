using System;

namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a InformationRightsManagementSettings object
    /// </summary>
    [ConcreteType(typeof(InformationRightsManagementSettings))]
    public interface IInformationRightsManagementSettings : IDataModel<IInformationRightsManagementSettings>, IDataModelGet<IInformationRightsManagementSettings>, IDataModelLoad<IInformationRightsManagementSettings>
    {

        /// <summary>
        /// Allow viewers to print the IRM protected Office document
        /// </summary>
        public bool AllowPrint { get; set; }

        /// <summary>
        /// Allow viewers to run script and screen reader to function on the IRM protected Office document
        /// </summary>
        public bool AllowScript { get; set; }

        /// <summary>
        /// Allow viewers to write on a copy of the downloaded document
        /// </summary>
        public bool AllowWriteCopy { get; set; }

        /// <summary>
        /// Prevent opening documents in the browser for this Document Library
        /// </summary>
        public bool DisableDocumentBrowserView { get; set; }

        /// <summary>
        /// After download, document access rights will expire after these number of days (1-365)
        /// </summary>
        public int DocumentAccessExpireDays { get; set; }

        /// <summary>
        /// Stop restricting access to the library at
        /// </summary>
        public DateTime DocumentLibraryProtectionExpireDate { get; set; }

        /// <summary>
        /// Enable restricting access to the library at a given date (<see cref="DocumentLibraryProtectionExpireDate"/>)
        /// </summary>
        public bool EnableDocumentAccessExpire { get; set; }

        /// <summary>
        /// Prevent opening documents in the browser for this Document Library
        /// </summary>
        public bool EnableDocumentBrowserPublishingView { get; set; }

        /// <summary>
        /// Allow group protection
        /// </summary>
        public bool EnableGroupProtection { get; set; }

        /// <summary>
        /// Enable document access rights expiration. Set the number of days via <see cref="DocumentAccessExpireDays"/>
        /// </summary>
        public bool EnableLicenseCacheExpire { get; set; }

        /// <summary>
        /// Group name used for group protection
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Users must verify their credentials using this interval (days)
        /// </summary>
        public int LicenseCacheExpireDays { get; set; }

        /// <summary>
        /// Permission policy description:
        /// </summary>
        public string PolicyDescription { get; set; }

        /// <summary>
        /// Permission policy title
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// IRM template ID (not used?)
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// A special property used to add an asterisk to a $select statement
        /// </summary>
        public object All { get; }
    }
}
