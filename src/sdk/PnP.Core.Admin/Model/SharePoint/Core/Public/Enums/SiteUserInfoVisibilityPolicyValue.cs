namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Indicates what the state of the browse user info policy in the site
    /// </summary>
    public enum SiteUserInfoVisibilityPolicyValue
    {
        /// <summary>
        /// Respect the tenant's default policy for Block User Info visibility
        /// </summary>
        OrganizationDefault = 0,

        /// <summary>
        /// Do not apply Block User Info visibility policy to anyone
        /// </summary>
        ApplyToNoUsers = 1,

        /// <summary>
        /// Apply Block User Info visibility policy to guest and external users
        /// </summary>
        ApplyToGuestAndExternalUsers = 2,
    }
}
