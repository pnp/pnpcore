using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Extensibility
{
    /// <summary>
    /// Contributes extra token definitions to a provisioning run.
    /// </summary>
    public interface IProvisioningExtensibilityTokenProvider
    {
        /// <summary>
        /// Returns the token definitions this provider contributes.
        /// </summary>
        /// <param name="context">Context of the site being provisioned</param>
        /// <param name="template">The template being applied</param>
        /// <param name="configurationData">The configuration string declared on the handler, if any</param>
        Task<IEnumerable<TokenDefinition>> GetTokensAsync(PnPContext context, ProvisioningTemplate template, string configurationData);
    }
}