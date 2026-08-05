using Microsoft.Extensions.Logging;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Publishing;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageRenditionModel = PnP.Core.Provisioning.Model.ImageRendition;
using PublishingModel = PnP.Core.Provisioning.Model.Publishing;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the image renditions a publishing site defines.
    /// </summary>
    internal class ObjectImageRenditions : ObjectHandlerBase
    {
        public override string Name => "Image renditions";

        public override string InternalName => "ImageRenditions";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Publishing != null && template.Publishing.ImageRenditions.Any();
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
            PublishingModel publishing = template.Publishing;

            if (publishing == null || !publishing.ImageRenditions.Any())
            {
                return parser;
            }

            if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
            {
                string warning = "This is a NoScript site, so the publishing features image renditions depend on cannot be used. " +
                    "The renditions were skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            if (!await PublishingRequirements.EnsureAsync(context, publishing.AutoCheckRequirements,
                m => WriteMessage(m, ProvisioningMessageType.Warning), "image renditions").ConfigureAwait(false))
            {
                return parser;
            }

            try
            {
                List<ImageRenditionInfo> existing = await CsomRequestSender.SendAsync(context,
                    new GetImageRenditionsRequest()).ConfigureAwait(false) ?? new List<ImageRenditionInfo>();

                var merged = new List<ImageRenditionInfo>(existing);
                bool added = false;

                foreach (ImageRenditionModel rendition in publishing.ImageRenditions)
                {
                    string name = parser.ParseString(rendition.Name);

                    // Matched on all three, as PnP Framework did: two renditions may legitimately
                    // share a name at different sizes, and re-adding an identical one is a no-op.
                    if (merged.Any(r => string.Equals(r.Name, name, StringComparison.Ordinal)
                        && r.Width == rendition.Width && r.Height == rendition.Height))
                    {
                        continue;
                    }

                    merged.Add(new ImageRenditionInfo
                    {
                        Name = name,
                        Width = rendition.Width,
                        Height = rendition.Height,
                    });

                    added = true;
                }

                if (!added)
                {
                    return parser;
                }

                await CsomRequestSender.SendAsync(context, new SetImageRenditionsRequest(merged)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The image renditions could not be applied: {ErrorText.Describe(ex)}";
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
            if (!await PublishingRequirements.IsWebPublishingActiveAsync(context).ConfigureAwait(false))
            {
                // Not a publishing site, so there are no renditions to describe. Reporting that as a
                // warning would fire on the majority of modern sites for no reason.
                return template;
            }

            try
            {
                List<ImageRenditionInfo> renditions = await CsomRequestSender.SendAsync(context,
                    new GetImageRenditionsRequest()).ConfigureAwait(false);

                if (renditions == null || renditions.Count == 0)
                {
                    return template;
                }

                // ObjectPublishing may already have created the element; both write into it.
                template.Publishing ??= new PublishingModel();

                foreach (ImageRenditionInfo rendition in renditions)
                {
                    template.Publishing.ImageRenditions.Add(new ImageRenditionModel
                    {
                        Name = rendition.Name,
                        Width = rendition.Width,
                        Height = rendition.Height,
                    });
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the site's image renditions could not be read.",
                    Constants.LOGGING_SOURCE);
            }

            return template;
        }

        #endregion
    }
}
