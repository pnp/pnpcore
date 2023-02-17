namespace PnP.Core.Admin.Model.SharePoint.Core.Internal
{
    internal sealed class PermissionGrant : IPermissionGrant
    {
        public string ClientId { get; set; }
        public string ConsentType { get; set; }
        public bool IsDomainIsolated { get; set; }
        public string ObjectId { get; set; }
        public string PackageName { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string Scope { get; set; }
    }
}
