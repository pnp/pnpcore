namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Right that an <see cref="ILegacyPrincipal"/> can have on a site collection
    /// </summary>
    public enum LegacySiteCollectionPermissionRight
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
        Guest
    }
}
