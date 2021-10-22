namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies what type of restriction mode is enabled for the tenant
    /// </summary>
    public enum SharingDomainRestrictionModes
    {
        /// <summary>
        /// No restriction mode is enable for the tenant
        /// </summary>
        None = 0,

        /// <summary>
        /// Allowed domain list is enabled for the tenant
        /// </summary>
        AllowList = 1,

        /// <summary>
        /// Blocked domain list is enabled for the tenant
        /// </summary>
        BlockList = 2
    }
}
