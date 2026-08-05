using PnP.Core.Provisioning.ObjectHandlers;

namespace PnP.Core.Services
{
    /// <summary>
    /// Extends the <see cref="PnPContext"/> with the PnP provisioning engine.
    /// </summary>
    public static class PnPContextProvisioningExtensions
    {
        private static readonly IPnPContextProvisioningExtensions defaultImplementation = new PnPContextProvisioningExtensionsImplementation();

        /// <summary>
        /// Replaces the default implementation of <see cref="IPnPContextProvisioningExtensions"/>
        /// with your own.
        /// </summary>
        public static IPnPContextProvisioningExtensions Implementation { private get; set; } = defaultImplementation;

        /// <summary>
        /// Reverts <see cref="Implementation"/> to the default.
        /// </summary>
        public static void RevertToDefaultImplementation()
        {
            Implementation = defaultImplementation;
        }

        /// <summary>
        /// Extends a <see cref="IPnPContext"/> with the PnP provisioning engine.
        /// </summary>
        /// <param name="context"><see cref="IPnPContext"/> to extend</param>
        /// <returns>An <see cref="IProvisioningManager"/> that can apply and extract templates</returns>
        /// <example>
        /// <code>
        /// IProvisioningManager manager = context.GetProvisioningManager();
        /// ProvisioningTemplate template = await manager.GetTemplateAsync();
        /// await manager.ApplyTemplateAsync(template);
        /// </code>
        /// </example>
        public static IProvisioningManager GetProvisioningManager(this IPnPContext context)
        {
            return Implementation.GetProvisioningManager(context);
        }
    }
}
