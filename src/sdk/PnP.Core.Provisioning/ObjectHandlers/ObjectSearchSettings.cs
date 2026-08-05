using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Provisions and extracts the search configuration XML at site and web scope.
    /// </summary>
    internal class ObjectSearchSettings : ObjectHandlerBase
    {
        public override string Name => "Search Settings";

        public override string InternalName => "SearchSettings";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return !string.IsNullOrEmpty(template.SiteSearchSettings)
                || !string.IsNullOrEmpty(template.WebSearchSettings);
        }

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                // Search configuration is unavailable on some site types and under some permission
                // sets. PnP Framework swallowed ServerException here; the same tolerance applies -
                // a site with no search customization should extract cleanly, not fail.
                try
                {
                    string siteSearchSettings = await context.Site.GetSearchConfigurationXmlAsync().ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(siteSearchSettings))
                    {
                        template.SiteSearchSettings = siteSearchSettings;
                    }
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: could not read the site search configuration", Constants.LOGGING_SOURCE);
                }

                try
                {
                    string webSearchSettings = await context.Web.GetSearchConfigurationXmlAsync().ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(webSearchSettings))
                    {
                        template.WebSearchSettings = webSearchSettings;
                    }
                }
                catch (Exception ex)
                {
                    context.Logger?.LogWarning(ex, "{Source}: could not read the web search configuration", Constants.LOGGING_SOURCE);
                }

                return template;
            }
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                // Guarded per scope, which PnP Framework does not do - it lets the import throw and
                // takes the rest of the template with it.
                //
                // A search configuration is a single opaque blob that SharePoint accepts or rejects
                // whole, and it is quite capable of rejecting one exported from another site: an
                // Enterprise Search Centre's configuration carries result sources and query rules
                // that name the site they came from. SRCHCEN#0's own configuration, applied to a
                // fresh SRCHCEN#0, is refused with CSOM "Unknown Error" - HTTP 200, no code, no
                // message - and that took the three handlers after it down with it.
                //
                // Nothing else in this engine behaves that way. The extract already reports a search
                // configuration it cannot read rather than failing, and the apply now matches.
                await ApplySearchConfigurationAsync("site collection", () =>
                    context.Site.SetSearchConfigurationXmlAsync(parser.ParseString(template.SiteSearchSettings)),
                    template.SiteSearchSettings, context).ConfigureAwait(false);

                await ApplySearchConfigurationAsync("site", () =>
                    context.Web.SetSearchConfigurationXmlAsync(parser.ParseString(template.WebSearchSettings)),
                    template.WebSearchSettings, context).ConfigureAwait(false);

                return parser;
            }
        }

        /// <summary>
        /// Imports one scope's search configuration, reporting a refusal rather than failing.
        /// </summary>
        private async Task ApplySearchConfigurationAsync(string scope, Func<Task> import,
            string configuration, PnPContext context)
        {
            if (string.IsNullOrEmpty(configuration))
            {
                return;
            }

            try
            {
                await import().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"The {scope} search configuration was refused, so it was not applied: " +
                    $"{ErrorText.Describe(ex)}";

                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }
    }
}
