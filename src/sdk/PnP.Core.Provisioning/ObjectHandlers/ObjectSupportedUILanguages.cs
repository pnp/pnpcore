using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts the web's supported UI languages.
    /// </summary>
    internal class ObjectSupportedUILanguages : ObjectHandlerBase
    {
        public override string Name => "Supported UI Languages";

        public override string InternalName => "SupportedUILanguages";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.SupportedUILanguages != null && template.SupportedUILanguages.Count > 0;
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.SupportedUILanguageIds).ConfigureAwait(false);

                template.SupportedUILanguages.Clear();
                foreach (int lcid in web.SupportedUILanguageIds ?? new List<int>())
                {
                    template.SupportedUILanguages.Add(new SupportedUILanguage { LCID = lcid });
                }

                // MIGRATION PHASE 9: PnP Framework compared this against the site's base template
                // and omitted the collection entirely when it matched. That needs BaseTemplateManager
                // wired into ExtractConfiguration - a phase 9 item.

                return template;
            }
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(w => w.SupportedUILanguageIds, w => w.IsMultilingual).ConfigureAwait(false);

                // Adding a language has no effect unless the web is multilingual, and SharePoint
                // does not flip this for you.
                if (!web.IsMultilingual)
                {
                    web.IsMultilingual = true;
                    await web.UpdateAsync().ConfigureAwait(false);
                }

                var wanted = template.SupportedUILanguages.Select(l => l.LCID).ToList();
                var current = new List<int>(web.SupportedUILanguageIds ?? new List<int>());

                // Remove first, then add. The template is the intended final state, so a language
                // the template omits should go - but removing the *default* language would leave
                // the site in an invalid state, so it is always kept.
                int defaultLcid = (await context.Web.GetAsync(w => w.Language).ConfigureAwait(false)).Language;

                var toRemove = current.Where(l => !wanted.Contains(l) && l != defaultLcid).ToList();
                var toAdd = wanted.Where(l => !current.Contains(l)).ToList();

                if (toRemove.Count == 0 && toAdd.Count == 0)
                {
                    return parser;
                }

                // CSOM, not REST: the REST endpoint accepts the call and does nothing, because
                // AddSupportedUILanguage only stages the change and REST cannot express the
                // Web.Update() that persists it. See backlog T2.
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                await SafelyAsync(context, "update the supported UI languages", () =>
                    CsomRequestSender.SendAsync(context,
                        new SetSupportedUILanguagesRequest(siteId, webId, toAdd, toRemove))).ConfigureAwait(false);

                return parser;
            }
        }

        /// <summary>
        /// Runs one language change, warning rather than aborting if SharePoint refuses it.
        /// </summary>
        private async Task SafelyAsync(PnPContext context, string what, Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"Could not {what}: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }
    }
}
