using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Describes the properties of a SharePoint AddIn when installed in a site
    /// </summary>
    public interface ISharePointAddIn : ILegacyPrincipal
    {
        /// <summary>
        /// Instance ID of this app
        /// </summary>
        Guid AppInstanceId { get; }

        /// <summary>
        /// The source of this addin
        /// </summary>
        SharePointAddInSource AppSource { get; }

        /// <summary>
        /// The full URL of the app web created by the addin
        /// </summary>
        string AppWebFullUrl { get; }

        /// <summary>
        /// Id of the app web created by the addin
        /// </summary>
        Guid AppWebId { get; }

        /// <summary>
        /// The id of the app in the office store, this will be empty for user uploaded apps
        /// </summary>
        string AssetId { get; }

        /// <summary>
        /// Date and time when the addin was installed
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Name of the user who installed the addin
        /// </summary>
        string InstalledBy { get; }

        /// <summary>
        /// Id of the site collection where the addin actually is installed. This can be different from the site collection where the addin was listed as being available
        /// </summary>
        Guid InstalledSiteId { get; }

        /// <summary>
        /// Id of the web where the addin actually is installed. This can be different from the web where the addin was listed as being available
        /// </summary>
        Guid InstalledWebId { get; }

        /// <summary>
        /// Fully qualified URL of the web where the addin actually is installed. This can be different from the web where the addin was listed as being available
        /// </summary>
        string InstalledWebFullUrl { get; }

        /// <summary>
        /// Name of the web where the addin actually is installed. This can be different from the web where the addin was listed as being available
        /// </summary>
        string InstalledWebName { get; }

        /// <summary>
        /// Id of the site collection where the addin actually is listed for
        /// </summary>
        Guid CurrentSiteId { get; }

        /// <summary>
        /// Id of the web where the addin actually is listed for
        /// </summary>
        Guid CurrentWebId { get; }

        /// <summary>
        /// Fully qualified URL of the web where the addin actually is listed for
        /// </summary>
        string CurrentWebFullUrl { get; }

        /// <summary>
        /// Name of the web where the addin actually is listed for
        /// </summary>
        string CurrentWebName { get; }

        /// <summary>
        /// Where to redirect after clicking on the add-in (e.g. ~appWebUrl/Pages/Default.aspx?{StandardTokens})
        /// </summary>
        string LaunchUrl { get; } 
        
        /// <summary>
        /// When was the app license purchased
        /// </summary>
        DateTime LicensePurchaseTime { get; }

        /// <summary>
        /// Identity of the user that purchased the app
        /// </summary>
        string PurchaserIdentity { get; }

        /// <summary>
        /// Locale used by the web where the add-in is installed
        /// </summary>
        string Locale { get; }

        /// <summary>
        /// The global unique id of the add-in. It is same for all tenants
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Status of the addin
        /// </summary>
        SharePointAddInStatus Status { get; }

        /// <summary>
        /// After the add-in installed in the tenant app catalog site. It could enable tenant level usage. This data indicates the tenant the conditions how to filter the webs. 
        /// See https://learn.microsoft.com/en-us/sharepoint/dev/sp-add-ins/tenancies-and-deployment-scopes-for-sharepoint-add-ins for more details
        /// </summary>
        string TenantAppData { get; }

        /// <summary>
        /// When was the <see cref="TenantAppData"/> last updated?
        /// </summary>
        DateTime TenantAppDataUpdateTime { get; }

    }
}
