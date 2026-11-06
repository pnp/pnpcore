using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.Extensibility
{
    /// <summary>
    /// Plugs custom logic into the template extraction and provisioning pipeline.
    /// </summary>
    public interface IProvisioningExtensibilityHandler : IProvisioningExtensibilityTokenProvider
    {
        /// <summary>
        /// Runs custom logic while a template is being applied.
        /// </summary>
        /// <param name="context">Context of the site being provisioned</param>
        /// <param name="template">The template being applied</param>
        /// <param name="configuration">The apply configuration in force</param>
        /// <param name="tokenParser">The parser threaded through the run</param>
        /// <param name="logger">Logger for the current pipeline step</param>
        /// <param name="configurationData">The configuration string declared on the handler, if any</param>
        Task ProvisionAsync(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration, TokenParser tokenParser, ILogger logger, string configurationData);

        /// <summary>
        /// Runs custom logic while a template is being extracted.
        /// </summary>
        /// <param name="context">Context of the site being extracted</param>
        /// <param name="template">The template built so far</param>
        /// <param name="configuration">The extract configuration in force</param>
        /// <param name="logger">Logger for the current pipeline step</param>
        /// <param name="configurationData">The configuration string declared on the handler, if any</param>
        /// <returns>The template, enriched by the handler</returns>
        Task<ProvisioningTemplate> ExtractAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration, ILogger logger, string configurationData);
    }
}
