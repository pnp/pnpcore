namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Specifies what type of external user and guest link sharing is enabled for the tenant
    /// </summary>
    public enum SharingCapabilities
    {
        /// <summary>
        /// External user sharing (share by email) and guest link sharing are both disabled for all site collections 
        /// in the tenant.  No new external user invitations or sharing links can be created, and any content previously 
        /// shared becomes inaccessible to external users.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// External user sharing is enabled for the tenancy, but guest link sharing is disabled.  Each individual 
        /// site collection's sharing properties govern whether the site collection has sharing disabled or allows 
        /// external user sharing, but a site collection cannot enable guest link sharing.
        /// </summary>
        ExternalUserSharingOnly = 1,

        /// <summary>
        /// External user sharing and guest link sharing are enabled for the tenant. Each individual site 
        /// collection's sharing properties govern whether the site collection has sharing disabled, allows external user 
        /// sharing only, or allows both external user sharing and guest link sharing.
        /// </summary>
        ExternalUserAndGuestSharing = 2,

        /// <summary>
        /// External user sharing and guest link sharing are both disabled for the tenant, but AllowGuestUserSignIn is enabled.
        /// Each individual site collection's sharing properties govern whether the site collection has sharing disabled or allows
        /// existing external user signing in, but a site collection cannot enable guest link sharing and cannot share with new external users.
        /// </summary>
        ExistingExternalUserSharingOnly = 3
    }
}
