namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Defines the permissions a legacy principal (<see cref="ILegacyPrincipal"/>) has on the tenant
    /// </summary>
    public interface ILegacyTenantPermission
    {
        /// <summary>
        /// The feature name of the permissions (Taxonomy/ Social/ ProjectServer/ Search/ BcsConnection/ Content)
        /// </summary>
        string ProductFeature { get; }

        /// <summary>
        /// The scope of the permission. E.g. content/tenant or projectserver/projects
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// The granted right
        /// </summary>
        LegacyTenantPermissionRight Right { get; }

        /// <summary>
        /// The specific resource id given to the app. For example, if the permission given to the specific project server, then this is the project server id. 
        /// </summary>
        string ResourceId { get; }
    }
}
