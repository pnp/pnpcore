namespace PnP.Core.Admin.Model.SharePoint
{
    internal class LegacyTenantPermission : ILegacyTenantPermission
    {
        public string ProductFeature { get; set; }

        public string Scope { get; set; }

        public LegacyTenantPermissionRight Right { get; set; }

        public string ResourceId { get; set; }
    }
}
