namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates what the state of the browse user info policy in the tenant
    /// </summary>
    public enum TenantBrowseUserInfoPolicyValue
    {
        /// <summary>
        /// Do not apply Block User Info visibility policy to anyone
        /// </summary>
        ApplyToNoUsers = 0,

        /// <summary>
        /// Apply Block User Info visibility policy to guest and external users
        /// </summary>
        ApplyToGuestAndExternalUsers
    }
}
