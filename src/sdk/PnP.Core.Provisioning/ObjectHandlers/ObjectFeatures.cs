using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FeatureModel = PnP.Core.Provisioning.Model.Feature;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Activates and deactivates site and web scoped features.
    /// </summary>
    internal class ObjectFeatures : ObjectHandlerBase
    {
        public override string Name => "Features";

        public override string InternalName => "Features";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return false;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.Features != null
                && ((template.Features.SiteFeatures != null && template.Features.SiteFeatures.Count > 0)
                    || (template.Features.WebFeatures != null && template.Features.WebFeatures.Count > 0));
        }

        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (template.Features == null)
                {
                    return parser;
                }

                IWeb web = await context.Web.GetAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                // Site scoped features live on the site collection, so they are only meaningful
                // from the root web. Applying a template containing them to a sub site silently
                // skips them, exactly as PnP Framework did.
                if (!IsSubSite(web) && template.Features.SiteFeatures != null)
                {
                    await context.Site.LoadAsync(s => s.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                    await ApplyFeaturesAsync(
                        context,
                        template.Features.SiteFeatures,
                        context.Site.Features.AsRequested().Select(f => f.DefinitionId).ToList(),
                        context.Site.Features,
                        "site").ConfigureAwait(false);
                }

                if (template.Features.WebFeatures != null)
                {
                    await context.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                    await ApplyFeaturesAsync(
                        context,
                        template.Features.WebFeatures,
                        context.Web.Features.AsRequested().Select(f => f.DefinitionId).ToList(),
                        context.Web.Features,
                        "web").ConfigureAwait(false);
                }

                return parser;
            }
        }

        /// <summary>
        /// Brings one scope's features to the state the template asks for.
        /// </summary>
        private async Task ApplyFeaturesAsync(PnPContext context, IEnumerable<FeatureModel> wanted,
            List<Guid> active, IFeatureCollection features, string scope)
        {
            foreach (FeatureModel feature in wanted)
            {
                bool isActive = active.Contains(feature.Id);

                try
                {
                    if (!feature.Deactivate && !isActive)
                    {
                        await features.EnableAsync(feature.Id).ConfigureAwait(false);
                        context.Logger?.LogInformation("{Source}: activated {Scope} feature {Feature}",
                            Constants.LOGGING_SOURCE, scope, feature.Id);
                    }
                    else if (feature.Deactivate && isActive)
                    {
                        await features.DisableAsync(feature.Id).ConfigureAwait(false);
                        context.Logger?.LogInformation("{Source}: deactivated {Scope} feature {Feature}",
                            Constants.LOGGING_SOURCE, scope, feature.Id);
                    }
                }
                catch (Exception ex)
                {
                    // A feature can be unavailable on this site type, blocked by tenant policy, or
                    // already in flux. PnP Framework swallowed ServerException here and continued;
                    // failing the whole run over one optional feature is the worse outcome.
                    string message = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                        "Could not {0} {1} feature {2}: {3}",
                        feature.Deactivate ? "deactivate" : "activate", scope, feature.Id, ex.Message);

                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                }
            }
        }
    }
}
