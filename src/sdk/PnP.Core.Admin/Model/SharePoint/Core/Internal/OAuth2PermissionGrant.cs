using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal record OAuth2PermissionGrant : IOAuth2PermissionGrant
    {
        public string ClientId { get; set; }
        public string ConsentType { get; set; }
        public DateTime ExpiryTime { get; set; }
        public string Id { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string Scope { get; set; }
        public DateTime StartTime { get; set; }
    }
}