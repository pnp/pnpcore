namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of default link permission for the tenant
    /// </summary>
    public enum SharingPermissionType
    {
        /// <summary>
        /// Not set
        /// </summary>
        None = 0,

        /// <summary>
        /// View sharing link permission
        /// </summary>
        View = 1,

        /// <summary>
        /// Edit sharing link permission
        /// </summary>
        Edit = 2,
    }
}
