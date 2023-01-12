namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Enumerates the various TLS token binding policy settings
    /// </summary>
    public enum SPOTlsTokenBindingPolicyValue : int
    {
        /// <summary>
        /// There are no settings for this policy
        /// </summary>
        None = 0,

        /// <summary>
        /// Emit audit logs, no token binding enforcement.
        /// </summary>
        Audit = 1,

        /// <summary>
        /// Bound sessions are evaluated and rejected upon failure, but
        /// unbound sessions are allowed access.
        /// </summary>
        PassiveEnforcement = 2,

        /// <summary>
        /// Bound sessions are evaluated and rejected upon failure,
        /// unbound sessions are rejected.
        /// </summary>
        StrictEnforcement = 3,
    }
}
