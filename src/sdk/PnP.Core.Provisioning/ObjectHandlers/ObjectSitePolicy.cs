using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.InformationPolicy;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:SitePolicy&gt;</c> element - the information management
    /// policy governing the site's retention and closure.
    /// </summary>
    internal class ObjectSitePolicy : ObjectHandlerBase
    {
        public override string Name => "Site policy";

        public override string InternalName => "SitePolicy";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= !string.IsNullOrEmpty(template.SitePolicy);
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // Unlike PnP Framework, this does not go to the server to decide. WillExtract is called
            // once per handler purely to size the progress bar, and paying a CSOM round trip for
            // that - on every extract, whether or not policies are in scope - is not worth it. The
            // extract itself reports when there is nothing to read.
            _willExtract ??= true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            string wanted = parser.ParseString(template.SitePolicy);

            if (string.IsNullOrEmpty(wanted))
            {
                return parser;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

            List<SitePolicyInfo> available;
            try
            {
                available = await CsomRequestSender.SendAsync(context,
                    new GetProjectPoliciesRequest(siteId, webId)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The site policies available to this site could not be read, so '{wanted}' was not applied: " +
                    ErrorText.Describe(ex);
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            available ??= new List<SitePolicyInfo>();

            int index = available.FindIndex(p => string.Equals(p.Name, wanted, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                string warning = available.Count == 0
                    ? $"The site policy '{wanted}' was not applied: this site has no policies available to it. " +
                      "Policies are defined in the Content Type Hub's Policy Definitions and published to sites."
                    : $"The site policy '{wanted}' was not applied: it is not among this site's available policies " +
                      $"({string.Join(", ", available.Select(p => p.Name))}).";

                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            try
            {
                await CsomRequestSender.SendAsync(context,
                    new ApplyProjectPolicyRequest(siteId, webId, index)).ConfigureAwait(false);

                context.Logger?.LogInformation("{Source}: applied the site policy '{Policy}'.",
                    Constants.LOGGING_SOURCE, wanted);
            }
            catch (Exception ex)
            {
                string warning = $"The site policy '{wanted}' could not be applied: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }

            return parser;
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                SitePolicyInfo applied = await CsomRequestSender.SendAsync(context,
                    new GetCurrentlyAppliedProjectPolicyRequest(siteId, webId)).ConfigureAwait(false);

                // No policy applied is the normal case, and produces no element rather than an
                // empty one - an empty <pnp:SitePolicy/> is not something a template can act on.
                if (!string.IsNullOrEmpty(applied?.Name))
                {
                    template.SitePolicy = applied.Name;
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the site's applied policy could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            return template;
        }

        #endregion
    }
}
