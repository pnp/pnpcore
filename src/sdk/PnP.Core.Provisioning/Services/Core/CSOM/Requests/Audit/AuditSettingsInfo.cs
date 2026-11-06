using PnP.Core.Provisioning.Model;

namespace PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit
{
    /// <summary>
    /// The site collection's audit configuration, as read back from CSOM.
    /// </summary>
    internal sealed class AuditSettingsInfo
    {
        /// <summary>
        /// The events being audited.
        /// </summary>
        internal AuditMaskType AuditFlags { get; set; }
    }
}
