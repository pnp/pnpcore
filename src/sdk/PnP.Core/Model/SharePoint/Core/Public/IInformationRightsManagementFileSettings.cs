namespace PnP.Core.Model.SharePoint
{
    /// <summary>
    /// Public interface to define a InformationRightsManagementFileSettings object
    /// </summary>
    [ConcreteType(typeof(InformationRightsManagementFileSettings))]
    public interface IInformationRightsManagementFileSettings : IComplexType
    {
        /// <summary>
        /// Gets or sets whether print is allowed for this file.
        /// </summary>
        public bool AllowPrint { get; set; }

        /// <summary>
        /// Gets or sets whether script is alloed for this file.
        /// </summary>
        public bool AllowScript { get; set; }

        /// <summary>
        /// Gets or sets whether writing copy is allowed for this file.
        /// </summary>
        public bool AllowWriteCopy { get; set; }

        /// <summary>
        /// Gets or sets whether document browser view is disabled for this file.
        /// </summary>
        public bool DisableDocumentBrowserView { get; set; }

        /// <summary>
        /// Gets or sets the access expiry in days for this file.
        /// </summary>
        public int DocumentAccessExpireDays { get; set; }

        /// <summary>
        /// Gets or sets access expiry for this file.
        /// </summary>
        public bool EnableDocumentAccessExpire { get; set; }

        /// <summary>
        /// Gets or sets whether document browser publishing view is enabled for this file.
        /// </summary>
        public bool EnableDocumentBrowserPublishingView { get; set; }

        /// <summary>
        /// Gets or sets whether group protection is enabled on this file.
        /// </summary>
        public bool EnableGroupProtection { get; set; }

        /// <summary>
        /// Gets or sets whether license cache expiry is enabled on this file.
        /// </summary>
        public bool EnableLicenseCacheExpire { get; set; }

        /// <summary>
        /// Gets or sets the IRM group name of the file.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets whether IRM is enabled on this file.
        /// </summary>
        public bool IrmEnabled { get; set; }

        /// <summary>
        /// Gets or sets the license cache expiry in days for this file.
        /// </summary>
        public int LicenseCacheExpireDays { get; set; }

        /// <summary>
        /// Gets or sets the policy description for this file.
        /// </summary>
        public string PolicyDescription { get; set; }

        /// <summary>
        /// Gets or sets the policy title for this file.
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// Gets or sets the IRM template id for this file.
        /// </summary>
        public string TemplateId { get; set; }
    }
}
