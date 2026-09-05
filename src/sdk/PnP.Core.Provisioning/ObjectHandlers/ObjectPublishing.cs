using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using PageLayoutModel = PnP.Core.Provisioning.Model.PageLayout;
using PublishingModel = PnP.Core.Provisioning.Model.Publishing;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    /// <summary>
    /// Applies and reads back the <c>&lt;pnp:Publishing&gt;</c> element - which web templates a
    /// subsite may use, and which page layouts a publishing site offers.
    /// </summary>
    internal class ObjectPublishing : ObjectHandlerBase
    {
        private const string AvailableWebTemplatesKey = PublishingPropertyBagXml.AvailableWebTemplatesKey;
        private const string InheritWebTemplatesKey = PublishingPropertyBagXml.InheritWebTemplatesKey;
        private const string AvailablePageLayoutsKey = PublishingPropertyBagXml.AvailablePageLayoutsKey;
        private const string DefaultPageLayoutKey = PublishingPropertyBagXml.DefaultPageLayoutKey;

        /// <summary>The master page gallery, where page layouts live, relative to a web.</summary>
        private const string MasterPageGalleryPath = "_catalogs/masterpage";

        public override string Name => "Publishing";

        public override string InternalName => "Publishing";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Publishing != null;
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

            if (publishing == null)
            {
                return parser;
            }

            WriteMessage($"Processing publishing settings: {publishing.AvailableWebTemplates.Count} web " +
                $"template(s), {publishing.PageLayouts.Count} page layout(s)", ProvisioningMessageType.Progress);

            if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
            {
                string warning = "This is a NoScript site, so the publishing settings were skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            if (!await PublishingRequirements.EnsureAsync(context, publishing.AutoCheckRequirements,
                m => WriteMessage(m, ProvisioningMessageType.Warning), "publishing settings").ConfigureAwait(false))
            {
                return parser;
            }

            if (publishing.DesignPackage != null)
            {
                string warning = "The template carries a design package. Sandboxed solution deployment is deprecated " +
                    "in SharePoint Online and this engine does not install one, so it was skipped.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }

            await ApplyAvailableWebTemplatesAsync(context, publishing, parser).ConfigureAwait(false);
            await ApplyPageLayoutsAsync(context, publishing, parser).ConfigureAwait(false);

            return parser;
        }

        /// <summary>
        /// Writes the <c>__WebTemplates</c> property bag XML.
        /// </summary>
        private async Task ApplyAvailableWebTemplatesAsync(PnPContext context, PublishingModel publishing, TokenParser parser)
        {
            if (!publishing.AvailableWebTemplates.Any())
            {
                return;
            }

            try
            {
                await SetPropertiesAsync(context, new Dictionary<string, string>
                {
                    [AvailableWebTemplatesKey] = PublishingPropertyBagXml.BuildWebTemplates(
                        publishing.AvailableWebTemplates, parser.ParseString),

                    [InheritWebTemplatesKey] = "False",
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The available web templates could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private async Task ApplyPageLayoutsAsync(PnPContext context, PublishingModel publishing, TokenParser parser)
        {
            if (!publishing.PageLayouts.Any())
            {
                return;
            }

            Dictionary<string, PageLayoutEntry> catalog = await ReadPageLayoutCatalogAsync(
                context, m => WriteMessage(m, ProvisioningMessageType.Warning)).ConfigureAwait(false);

            WriteMessage($"Matching {publishing.PageLayouts.Count} requested page layout(s) against " +
                $"{catalog.Count} in the master page gallery", ProvisioningMessageType.Progress);

            if (catalog.Count == 0)
            {
                string warning = "The master page gallery holds no page layouts, so none were set.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            var available = new List<XElement>();
            XElement defaultLayout = null;

            foreach (PageLayoutModel layout in publishing.PageLayouts)
            {
                string path = parser.ParseString(layout.Path);

                if (!catalog.TryGetValue(NameOf(path), out PageLayoutEntry entry))
                {
                    string warning = $"The page layout '{path}' is not in the master page gallery, so it was skipped.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                    WriteMessage(warning, ProvisioningMessageType.Warning);
                    continue;
                }

                XElement element = PublishingPropertyBagXml.BuildLayout(entry.UniqueId, entry.SiteRelativeUrl);
                available.Add(element);

                if (layout.IsDefault)
                {
                    defaultLayout = element;
                }
            }

            if (available.Count == 0)
            {
                string warning = $"None of the {publishing.PageLayouts.Count} page layout(s) in the " +
                    "template could be resolved, so the available page layouts were left unchanged.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return;
            }

            var properties = new Dictionary<string, string>
            {
                [AvailablePageLayoutsKey] = PublishingPropertyBagXml.BuildPageLayouts(available),
            };

            if (defaultLayout != null)
            {
                properties[DefaultPageLayoutKey] = defaultLayout.ToString(SaveOptions.DisableFormatting);
            }

            try
            {
                await SetPropertiesAsync(context, properties).ConfigureAwait(false);

                WriteMessage($"Wrote {available.Count} page layout(s) to {string.Join(" and ", properties.Keys)}",
                    ProvisioningMessageType.Progress);
            }
            catch (Exception ex)
            {
                string warning = $"The available page layouts could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private static async Task SetPropertiesAsync(PnPContext context, Dictionary<string, string> properties)
        {
            IWeb web = context.Web;
            await web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

            foreach (KeyValuePair<string, string> property in properties)
            {
                web.AllProperties[property.Key] = property.Value;
            }

            await web.AllProperties.UpdateAsync().ConfigureAwait(false);
        }

        #endregion

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            if (!await PublishingRequirements.IsWebPublishingActiveAsync(context).ConfigureAwait(false))
            {
                return template;
            }

            template.Publishing ??= new PublishingModel();
            template.Publishing.AutoCheckRequirements = AutoCheckRequirementsOptions.MakeCompliant;

            await ExtractAvailableWebTemplatesAsync(context, template.Publishing).ConfigureAwait(false);
            await ExtractPageLayoutsAsync(context, template.Publishing).ConfigureAwait(false);

            return template;
        }

        private static async Task ExtractAvailableWebTemplatesAsync(PnPContext context, PublishingModel publishing)
        {
            try
            {
                IWeb web = context.Web;
                await web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

                if (!web.AllProperties.Values.TryGetValue(AvailableWebTemplatesKey, out object raw))
                {
                    return;
                }

                foreach (AvailableWebTemplate webTemplate in PublishingPropertyBagXml.ReadWebTemplates(raw?.ToString()))
                {
                    publishing.AvailableWebTemplates.Add(webTemplate);
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the available web templates could not be read.",
                    Constants.LOGGING_SOURCE);
            }
        }

        private static async Task ExtractPageLayoutsAsync(PnPContext context, PublishingModel publishing)
        {
            try
            {
                IWeb web = context.Web;
                await web.LoadAsync(w => w.AllProperties).ConfigureAwait(false);

                web.AllProperties.Values.TryGetValue(DefaultPageLayoutKey, out object rawDefault);

                if (!web.AllProperties.Values.TryGetValue(AvailablePageLayoutsKey, out object raw))
                {
                    return;
                }

                foreach (PageLayoutModel layout in PublishingPropertyBagXml.ReadPageLayouts(
                    raw?.ToString(), rawDefault?.ToString()))
                {
                    publishing.PageLayouts.Add(layout);
                }
            }
            catch (Exception ex)
            {
                context.Logger?.LogDebug(ex, "{Source}: the available page layouts could not be read.",
                    Constants.LOGGING_SOURCE);
            }
        }

        #endregion

        #region Page layout catalog

        /// <summary>
        /// One page layout in the root web's master page gallery.
        /// </summary>
        private sealed class PageLayoutEntry
        {
            internal string UniqueId { get; set; }

            internal string SiteRelativeUrl { get; set; }
        }

        /// <summary>
        /// Reads the page layouts from the <b>root</b> web's master page gallery, keyed by file name.
        /// </summary>
        private static async Task<Dictionary<string, PageLayoutEntry>> ReadPageLayoutCatalogAsync(
            PnPContext context, Action<string> reportWarning)
        {
            var catalog = new Dictionary<string, PageLayoutEntry>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await context.Site.LoadAsync(s => s.RootWeb).ConfigureAwait(false);

                IWeb rootWeb = context.Site.RootWeb;
                await rootWeb.LoadAsync(w => w.ServerRelativeUrl).ConfigureAwait(false);

                string webUrl = rootWeb.ServerRelativeUrl.TrimEnd('/');

                IFolder gallery = await rootWeb.GetFolderByServerRelativeUrlAsync(
                    $"{webUrl}/{MasterPageGalleryPath}",
                    f => f.Files.QueryProperties(file => file.Name, file => file.UniqueId,
                        file => file.ServerRelativeUrl)).ConfigureAwait(false);

                foreach (IFile file in gallery.Files.AsRequested())
                {
                    if (string.IsNullOrEmpty(file.Name) || file.UniqueId == Guid.Empty)
                    {
                        continue;
                    }

                    catalog[file.Name] = new PageLayoutEntry
                    {
                        UniqueId = file.UniqueId.ToString(),

                        SiteRelativeUrl = file.ServerRelativeUrl.StartsWith(webUrl, StringComparison.OrdinalIgnoreCase)
                            ? file.ServerRelativeUrl.Substring(webUrl.Length).TrimStart('/')
                            : file.ServerRelativeUrl.TrimStart('/'),
                    };
                }
            }
            catch (Exception ex)
            {
                string warning = $"The master page gallery could not be read, so the page layouts " +
                    $"were not set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                reportWarning?.Invoke(warning);
            }

            return catalog;
        }

        private static string NameOf(string pathOrName)
        {
            if (string.IsNullOrEmpty(pathOrName))
            {
                return string.Empty;
            }

            int lastSlash = pathOrName.Replace('\\', '/').LastIndexOf('/');
            return lastSlash < 0 ? pathOrName : pathOrName.Substring(lastSlash + 1);
        }

        #endregion
    }
}
