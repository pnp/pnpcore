namespace PnP.Core.Provisioning.Model
{
    /// <summary>
    /// A site policy as reported by the information management policy CSOM API.
    /// </summary>
    internal sealed class SitePolicyInfo
    {
        /// <summary>
        /// The policy's name, which is what a template's <c>&lt;pnp:SitePolicy&gt;</c> element carries.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The policy's description.
        /// </summary>
        internal string Description { get; set; }
    }
}
