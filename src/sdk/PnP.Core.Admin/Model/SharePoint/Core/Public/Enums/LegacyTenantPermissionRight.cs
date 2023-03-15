namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Right that an <see cref="ILegacyPrincipal"/> can have on the tenant
    /// </summary>
    public enum LegacyTenantPermissionRight
    {
        /// <summary>
        /// Full control right
        /// </summary>
        FullControl,
        
        /// <summary>
        /// Manage right
        /// </summary>
        Manage,

        /// <summary>
        /// Write right
        /// </summary>
        Write,

        /// <summary>
        /// Read right
        /// </summary>
        Read,

        /// <summary>
        /// Guest right
        /// </summary>
        Guests,

        /// <summary>
        /// 
        /// </summary>
        QueryAsUserIgnoreAppPrincipal,

        /// <summary>
        /// 
        /// </summary>
        SubmitStatus,

        /// <summary>
        /// 
        /// </summary>
        BypassDelegate,

        /// <summary>
        /// 
        /// </summary>
        AllRights,

        /// <summary>
        /// 
        /// </summary>
        Elevate

    }
}
