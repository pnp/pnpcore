using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.WebParts;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreFile = PnP.Core.Model.SharePoint.IFile;
using PageModel = PnP.Core.Provisioning.Model.Page;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Creates the classic wiki pages a template declares, and places the web parts on them.
    /// </summary>
    internal class ObjectPages : ObjectHandlerBase
    {
        /// <summary>The wiki page content type, whose <c>WikiField</c> holds the layout.</summary>
        private const string WikiPageContentTypeId = "0x010108";

        public override string Name => "Pages";

        public override string InternalName => "Pages";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.Pages.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            // Classic wiki pages are not enumerated on extract, matching PnP Framework - see the
            // note on ExtractObjectsAsync.
            _willExtract ??= false;
            return _willExtract.Value;
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template,
            TokenParser parser, ApplyConfiguration configuration)
        {
            if (!template.Pages.Any())
            {
                return parser;
            }

            IWeb web = context.Web;
            await web.LoadAsync(w => w.ServerRelativeUrl,
                w => w.RootFolder.QueryProperties(f => f.WelcomePage)).ConfigureAwait(false);

            if (await web.IsNoScriptSiteAsync().ConfigureAwait(false))
            {
                string warning = "This is a NoScript site, so classic wiki pages cannot be created. " +
                    "The pages were skipped; use <pnp:ClientSidePages> for modern pages instead.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
                return parser;
            }

            string webUrl = web.ServerRelativeUrl.TrimEnd('/');
            string welcomePage = web.RootFolder.WelcomePage;

            int index = 0;

            foreach (PageModel page in template.Pages)
            {
                index++;
                string url = parser.ParseString(page.Url);

                if (!url.StartsWith(webUrl, StringComparison.OrdinalIgnoreCase))
                {
                    url = UrlUtility.Combine(webUrl, url);
                }

                WriteSubProgress("Page", url, index, template.Pages.Count);

                await ProvisionPageAsync(context, page, url, webUrl, welcomePage, parser).ConfigureAwait(false);
            }

            WriteMessage("Done processing pages", ProvisioningMessageType.Completed);

            return parser;
        }

        private async Task ProvisionPageAsync(PnPContext context, PageModel page, string url, string webUrl,
            string welcomePage, TokenParser parser)
        {
            try
            {
                CoreFile existing = await TryGetFileAsync(context, url).ConfigureAwait(false);

                if (existing != null && !page.Overwrite)
                {
                    // Present and the template does not overwrite: the web parts and field values
                    // below still apply, which is how a delta template adds to an existing page.
                    await FinaliseAsync(context, page, url, parser).ConfigureAwait(false);
                    return;
                }

                bool isWelcomePage = welcomePage != null
                    && string.Equals(url, UrlUtility.Combine(webUrl, welcomePage), StringComparison.OrdinalIgnoreCase);

                if (existing != null)
                {
                    // A page that is the welcome page cannot be deleted while it holds that role.
                    if (isWelcomePage)
                    {
                        await SetWelcomePageAsync(context, string.Empty).ConfigureAwait(false);
                    }

                    await existing.DeleteAsync().ConfigureAwait(false);
                }

                await CreateWikiPageAsync(context, url, webUrl, page).ConfigureAwait(false);

                if (existing != null && isWelcomePage)
                {
                    await SetWelcomePageAsync(context, welcomePage).ConfigureAwait(false);
                }

                await FinaliseAsync(context, page, url, parser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The page '{url}' could not be provisioned: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Creates the page file and writes its layout into the wiki field.
        /// </summary>
        private static async Task CreateWikiPageAsync(PnPContext context, string url, string webUrl, PageModel page)
        {
            int lastSlash = url.LastIndexOf('/');
            string folderUrl = url.Substring(0, lastSlash);
            string fileName = url.Substring(lastSlash + 1);

            IFolder folder = await context.Web.GetFolderByServerRelativeUrlAsync(folderUrl,
                f => f.ServerRelativeUrl).ConfigureAwait(false);

            // An empty file first: the wiki field is a list item property, so the item has to exist
            // before the layout can be written into it.
            using (var empty = new MemoryStream(Array.Empty<byte>()))
            {
                await folder.Files.AddAsync(fileName, empty, true).ConfigureAwait(false);
            }

            CoreFile created = await context.Web.GetFileByServerRelativeUrlAsync(url,
                f => f.ListItemAllFields).ConfigureAwait(false);

            IListItem item = created.ListItemAllFields;

            item.Values["ContentTypeId"] = WikiPageContentTypeId;
            item.Values["WikiField"] = WikiPageLayouts.HtmlFor(page.Layout.ToString());

            await item.UpdateOverwriteVersionAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Web parts and field values, once the page exists.
        /// </summary>
        private async Task FinaliseAsync(PnPContext context, PageModel page, string url, TokenParser parser)
        {
            await AddWebPartsAsync(context, page, url, parser).ConfigureAwait(false);
            await ApplyFieldsAsync(context, page, url, parser).ConfigureAwait(false);
        }

        private async Task AddWebPartsAsync(PnPContext context, PageModel page, string url, TokenParser parser)
        {
            if (page.WebParts == null || !page.WebParts.Any())
            {
                return;
            }

            try
            {
                (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);

                List<WebPartDefinitionInfo> existing = await CsomRequestSender.SendAsync(context,
                    new GetWebPartDefinitionsRequest(siteId, webId, url)).ConfigureAwait(false);

                var titles = new HashSet<string>(
                    (existing ?? new List<WebPartDefinitionInfo>()).Select(w => w.Title).Where(t => t != null),
                    StringComparer.Ordinal);

                foreach (WebPart webPart in page.WebParts)
                {
                    string title = parser.ParseString(webPart.Title);

                    // Already there under the same title: adding again would stack duplicates on
                    // every re-apply, which is the failure this whole engine keeps running into.
                    if (titles.Contains(title))
                    {
                        continue;
                    }

                    await CsomRequestSender.SendAsync(context, new AddWebPartRequest(
                        siteId, webId, url,
                        parser.ParseXmlString(webPart.Contents).Trim('\n', ' '),
                        webPart.Zone, (int)webPart.Order)).ConfigureAwait(false);

                    titles.Add(title);
                }
            }
            catch (Exception ex)
            {
                string warning = $"The web parts on '{url}' could not be added: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private async Task ApplyFieldsAsync(PnPContext context, PageModel page, string url, TokenParser parser)
        {
            if (page.Fields == null || page.Fields.Count == 0)
            {
                return;
            }

            try
            {
                CoreFile file = await context.Web.GetFileByServerRelativeUrlAsync(url,
                    f => f.ListItemAllFields).ConfigureAwait(false);

                IListItem item = file.ListItemAllFields;

                foreach (KeyValuePair<string, string> field in page.Fields)
                {
                    item.Values[parser.ParseString(field.Key)] = parser.ParseString(field.Value);
                }

                await item.UpdateOverwriteVersionAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string warning = $"The field values on '{url}' could not be set: {ErrorText.Describe(ex)}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, warning);
                WriteMessage(warning, ProvisioningMessageType.Warning);
            }
        }

        private static async Task SetWelcomePageAsync(PnPContext context, string welcomePage)
        {
            await context.Web.LoadAsync(w => w.RootFolder.QueryProperties(f => f.WelcomePage)).ConfigureAwait(false);

            context.Web.RootFolder.WelcomePage = welcomePage;
            await context.Web.RootFolder.UpdateAsync().ConfigureAwait(false);
        }

        private static async Task<CoreFile> TryGetFileAsync(PnPContext context, string url)
        {
            try
            {
                return await context.Web.GetFileByServerRelativeUrlAsync(url, f => f.UniqueId, f => f.ServerRelativeUrl)
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Extract

        /// <summary>
        /// Not implemented, matching PnP Framework.
        /// </summary>
        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        #endregion
    }
}
