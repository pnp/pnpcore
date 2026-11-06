using Microsoft.Extensions.Logging;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Audit;
using PnP.Core.Services;
using System;
using System.Threading.Tasks;
using AuditSettingsModel = PnP.Core.Provisioning.Model.AuditSettings;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:AuditSettings&gt;</c> element - what the site
    /// collection audits, and how long it keeps the log.
    /// </summary>
    internal class ObjectAuditSettings : ObjectHandlerBase
    {
        public override string Name => "Audit settings";

        public override string InternalName => "AuditSettings";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.AuditSettings != null;
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            _willExtract ??= true;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            AuditSettingsModel wanted = template.AuditSettings;

            if (wanted == null)
            {
                return parser;
            }

            if (await IsSubSiteAsync(context).ConfigureAwait(false))
            {
                return parser;
            }

            ISite site = context.Site;
            await site.LoadAsync(s => s.AuditLogTrimmingRetention, s => s.TrimAuditLog).ConfigureAwait(false);

            bool isNoScriptSite = await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);
            bool dirty = false;

            if (site.TrimAuditLog != wanted.TrimAuditLog)
            {
                site.TrimAuditLog = wanted.TrimAuditLog;
                dirty = true;
            }

            if (site.AuditLogTrimmingRetention != wanted.AuditLogTrimmingRetention)
            {
                if (isNoScriptSite)
                {
                    string warning = "This is a NoScript site, so the audit log trimming retention was not changed.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
                else
                {
                    site.AuditLogTrimmingRetention = wanted.AuditLogTrimmingRetention;
                    dirty = true;
                }
            }

            if (dirty)
            {
                try
                {
                    await site.UpdateAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string warning = $"The audit log retention settings could not be applied: {ErrorText.Describe(ex)}";
                    context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                }
            }

            await ApplyAuditFlagsAsync(context, wanted).ConfigureAwait(false);

            return parser;
        }

        /// <summary>
        /// Writes the audit mask through CSOM.
        /// </summary>
        private async Task ApplyAuditFlagsAsync(PnPContext context, AuditSettingsModel wanted)
        {
            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                AuditSettingsInfo current = await CsomRequestSender.SendAsync(context,
                    new GetAuditRequest(siteId, webId)).ConfigureAwait(false);

                if (current != null && current.AuditFlags == wanted.AuditFlags)
                {
                    return;
                }

                await CsomRequestSender.SendAsync(context,
                    new UpdateAuditRequest(siteId, webId, wanted.AuditFlags)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The audit flags could not be applied: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            if (await IsSubSiteAsync(context).ConfigureAwait(false))
            {
                return template;
            }

            var settings = new AuditSettingsModel();
            var defaults = new AuditSettingsModel();
            bool anythingSet = false;

            try
            {
                await context.Site.LoadAsync(s => s.AuditLogTrimmingRetention, s => s.TrimAuditLog).ConfigureAwait(false);

                if (context.Site.AuditLogTrimmingRetention != defaults.AuditLogTrimmingRetention)
                {
                    settings.AuditLogTrimmingRetention = context.Site.AuditLogTrimmingRetention;
                    anythingSet = true;
                }

                if (context.Site.TrimAuditLog != defaults.TrimAuditLog)
                {
                    settings.TrimAuditLog = context.Site.TrimAuditLog;
                    anythingSet = true;
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the audit retention settings could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                AuditSettingsInfo audit = await CsomRequestSender.SendAsync(context,
                    new GetAuditRequest(siteId, webId)).ConfigureAwait(false);

                if (audit != null && audit.AuditFlags != defaults.AuditFlags)
                {
                    settings.AuditFlags = audit.AuditFlags;
                    anythingSet = true;
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the audit flags could not be read.", Constants.LOGGING_SOURCE);
            }

            if (!anythingSet)
            {
                return template;
            }

            ProvisioningTemplate baseTemplate = configuration?.ToCreationInformation()?.BaseTemplate;

            if (baseTemplate == null || !settings.Equals(baseTemplate.AuditSettings))
            {
                template.AuditSettings = settings;
            }

            return template;
        }

        #endregion
    }
}
