namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies the type of default sharing link for the tenant
    /// </summary>
    public enum SharingLinkType
    {
        /// <summary>
        /// Not set
        /// </summary>
        None = 0,

        /// <summary>
        /// Restricted sharing link
        /// </summary>
        Direct = 1,

        /// <summary>
        /// OrganizationEdit sharing link
        /// </summary>
        Internal = 2,

        /// <summary>
        /// AnonymousEdit sharing link
        /// </summary>
        AnonymousAccess = 3
    }
}
