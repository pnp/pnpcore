using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Records in the site's property bag which template was applied, and when.
    /// </summary>
    internal class ObjectPersistTemplateInfo : ObjectHandlerBase
    {
        public ObjectPersistTemplateInfo()
        {
            ReportProgress = false;
        }

        public override string Name => "Persist Template Info";

        public override string InternalName => "PersistTemplateInfo";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return true;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return false;
        }

        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
                {
                    context.Logger?.LogInformation(
                        "{Source}: this is a NoScript site, so the applied template's id and info were not recorded in the property bag.",
                        Constants.LOGGING_SOURCE);
                    return parser;
                }

                IWeb web = await context.Web.GetAsync(w => w.AllProperties).ConfigureAwait(false);

                string templateId = template.Id ?? string.Empty;

                var info = new ProvisioningTemplateInfo
                {
                    TemplateId = templateId,
                    TemplateVersion = template.Version,
                    TemplateSitePolicy = template.SitePolicy,
                    Result = true,
                    ProvisioningTime = DateTime.Now,
                };

                web.AllProperties[ObjectRetrieveTemplateInfo.TemplateIdKey] = templateId;
                web.AllProperties[ObjectRetrieveTemplateInfo.TemplateInfoKey] = JsonSerializer.Serialize(info);

                await web.AllProperties.UpdateAsync().ConfigureAwait(false);

                if (!await web.AddIndexedPropertyAsync(ObjectRetrieveTemplateInfo.TemplateIdKey).ConfigureAwait(false))
                {
                    string message = $"Could not mark '{ObjectRetrieveTemplateInfo.TemplateIdKey}' as an indexed property.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                }

                return parser;
            }
        }
    }
}
