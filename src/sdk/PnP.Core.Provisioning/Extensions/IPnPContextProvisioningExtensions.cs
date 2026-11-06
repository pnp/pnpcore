using PnP.Core.Provisioning.ObjectHandlers;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends <see cref="PnPContext"/> with provisioning functionality.
    /// </summary>
    public interface IPnPContextProvisioningExtensions
    {
        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with the provisioning engine.
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="IProvisioningManager"/> that can apply and extract templates</returns>
        IProvisioningManager GetProvisioningManager(IPnPContext context);
    }
}