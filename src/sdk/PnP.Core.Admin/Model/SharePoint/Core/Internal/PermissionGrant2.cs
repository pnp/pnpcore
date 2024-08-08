using System;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal record PermissionGrant2 : IPermissionGrant2
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