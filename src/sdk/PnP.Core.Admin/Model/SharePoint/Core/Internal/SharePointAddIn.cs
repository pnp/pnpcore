using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class SharePointAddIn : ISharePointAddIn
    {
        public Guid AppInstanceId { get; set; }

        public SharePointAddInSource AppSource { get; set; }

        public string AppWebFullUrl { get; set; }

        public Guid AppWebId { get; set; }

        public string AssetId { get; set; }

        public DateTime CreationTime { get; set; }

        public string InstalledBy { get; set; }

        public Guid InstalledSiteId { get; set; }

        public Guid InstalledWebId { get; set; }

        public string InstalledWebFullUrl { get; set; }

        public string InstalledWebName { get; set; }

        public Guid CurrentSiteId { get; set; }

        public Guid CurrentWebId { get; set; }

        public string CurrentWebFullUrl { get; set; }

        public string CurrentWebName { get; set; }

        public string LaunchUrl { get; set; }

        public DateTime LicensePurchaseTime { get; set; }

        public string PurchaserIdentity { get; set; }

        public string Locale { get; set; }

        public Guid ProductId { get; set; }

        public SharePointAddInStatus Status { get; set; }

        public string TenantAppData { get; set; }

        public DateTime TenantAppDataUpdateTime { get; set; }

        public string AppIdentifier { get; set; }

        public string ServerRelativeUrl { get; set; }

        public string Title { get; set; }

        public bool AllowAppOnly { get; set; }

        public IEnumerable<ILegacySiteCollectionPermission> SiteCollectionScopedPermissions { get; set; }

        public IEnumerable<ILegacyTenantPermission> TenantScopedPermissions { get; set; }
    }
}
