using PnP.Core.Provisioning.ObjectHandlers;

namespace PnP.Core.Services
{
    /// <summary>
    /// Default implementation of <see cref="IPnPContextProvisioningExtensions"/>.
    /// </summary>
    public class PnPContextProvisioningExtensionsImplementation : IPnPContextProvisioningExtensions
    {
        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with the provisioning engine.
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="IProvisioningManager"/> that can apply and extract templates</returns>
        public IProvisioningManager GetProvisioningManager(IPnPContext context)
        {
            return new ProvisioningManager(context as PnPContext);
        }
    }
}