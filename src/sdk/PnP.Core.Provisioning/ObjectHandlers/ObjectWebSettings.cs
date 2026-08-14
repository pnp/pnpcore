using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreSearchBoxInNavBar = PnP.Core.Model.SharePoint.SearchBoxInNavBar;
using CoreSearchScope = PnP.Core.Model.SharePoint.SearchScope;
using TemplateFile = PnP.Core.Provisioning.Model.File;
using TemplateSearchBoxInNavBar = PnP.Core.Provisioning.Model.SearchBoxInNavBar;
using TemplateSearchScopes = PnP.Core.Provisioning.Model.SearchScopes;
using WebSettingsModel = PnP.Core.Provisioning.Model.WebSettings;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Reads and writes the web-level settings: crawling, sharing, master pages, the site logo,
    /// the welcome page, the search centre and hub site association.
    /// </summary>
    internal class ObjectWebSettings : ObjectHandlerBase
    {
        /// <summary>
        /// Master page names SharePoint ships. These are never persisted into a template - only a
        /// genuinely custom master page is worth carrying.
        /// </summary>
        private static readonly string[] OutOfTheBoxMasterPages =
        {
            "default.master", "custom.master", "v4.master", "seattle.master", "oslo.master",
        };

        public override string Name => "Web Settings";

        public override string InternalName => "WebSettings";

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return true;
        }

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            return template.WebSettings != null;
        }

        #region Extract

        public override async Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                IWeb web = await context.Web.GetAsync(
                    w => w.NoCrawl, w => w.CommentsOnSitePagesDisabled, w => w.ExcludeFromOfflineClient,
                    w => w.MembersCanShare, w => w.DisableFlows, w => w.DisableAppViews,
                    w => w.HorizontalQuickLaunch, w => w.QuickLaunchEnabled, w => w.SearchScope,
                    w => w.SearchBoxInNavBar, w => w.MasterUrl, w => w.CustomMasterUrl, w => w.SiteLogoUrl,
                    w => w.RequestAccessEmail, w => w.AlternateCssUrl, w => w.WelcomePage,
                    w => w.ServerRelativeUrl, w => w.Url, w => w.AllProperties).ConfigureAwait(false);

                await context.Site.LoadAsync(s => s.ServerRelativeUrl, s => s.Id, s => s.HubSiteId).ConfigureAwait(false);

                string webUrl = web.Url.ToString();

                var webSettings = new WebSettingsModel
                {
                    NoCrawl = web.NoCrawl,
                    CommentsOnSitePagesDisabled = web.CommentsOnSitePagesDisabled,
                    ExcludeFromOfflineClient = web.ExcludeFromOfflineClient,
                    MembersCanShare = web.MembersCanShare,
                    DisableFlows = web.DisableFlows,
                    DisableAppViews = web.DisableAppViews,
                    HorizontalQuickLaunch = web.HorizontalQuickLaunch,
                    QuickLaunchEnabled = web.QuickLaunchEnabled,
                    SearchScope = ToTemplateSearchScope(web.SearchScope),
                    SearchBoxInNavBar = ToTemplateSearchBox(web.SearchBoxInNavBar),
                    SearchCenterUrl = GetSearchCenterUrl(web, urlOnly: true),
                    MasterPageUrl = Tokenize(web.MasterUrl, webUrl),
                    CustomMasterPageUrl = Tokenize(web.CustomMasterUrl, webUrl),
                    SiteLogo = TokenizeHost(webUrl, Tokenize(web.SiteLogoUrl, webUrl)),

                    WelcomePage = web.WelcomePage,
                    AlternateCSS = Tokenize(web.AlternateCssUrl, webUrl),
                    RequestAccessEmail = web.RequestAccessEmail,

                };

                webSettings.HubSiteUrl = await GetHubSiteUrlAsync(context, webUrl).ConfigureAwait(false);

                ProvisioningTemplateCreationInformation creationInformation = configuration?.ToCreationInformation();

                if (creationInformation?.PersistBrandingFiles == true)
                {
                    await PersistBrandingFilesAsync(context, web, template, creationInformation).ConfigureAwait(false);
                    template.WebSettings = webSettings;
                }
                else if (creationInformation?.BaseTemplate != null)
                {
                    if (!webSettings.Equals(creationInformation.BaseTemplate.WebSettings))
                    {
                        template.WebSettings = webSettings;
                    }
                }
                else
                {
                    template.WebSettings = webSettings;
                }

                return template;
            }
        }

        /// <summary>
        /// Reads the url of the hub site this site is joined to, if any.
        /// </summary>
        private async Task<string> GetHubSiteUrlAsync(PnPContext context, string webUrl)
        {
            Guid hubSiteId = context.Site.HubSiteId;

            if (hubSiteId == Guid.Empty || hubSiteId == context.Site.Id)
            {
                return null;
            }

            try
            {
                IHubSite hubSite = await context.Site.GetHubSiteDataAsync(hubSiteId).ConfigureAwait(false);
                return TokenizeHost(webUrl, hubSite?.SiteUrl);
            }
            catch (Exception ex)
            {
                string message = $"Could not read the hub site association: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
                return null;
            }
        }

        /// <summary>
        /// Copies the site's custom branding assets into the template's connector and records them
        /// as template files.
        /// </summary>
        private async Task PersistBrandingFilesAsync(PnPContext context, IWeb web, ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInformation)
        {
            var candidates = new List<string>();

            AddIfCustomMasterPage(candidates, web.MasterUrl);
            AddIfCustomMasterPage(candidates, web.CustomMasterUrl);

            if (!string.IsNullOrEmpty(web.SiteLogoUrl) && web.SiteLogoUrl.IndexOf("_api/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                var webUri = new Uri(web.Url.ToString());
                candidates.Add(RemoveIgnoreCase(web.SiteLogoUrl, $"{webUri.Scheme}://{webUri.DnsSafeHost}"));
            }

            if (!string.IsNullOrEmpty(web.AlternateCssUrl))
            {
                candidates.Add(web.AlternateCssUrl);
            }

            foreach (string serverRelativeUrl in candidates)
            {
                if (await PersistFileAsync(context, web, creationInformation, serverRelativeUrl).ConfigureAwait(false))
                {
                    template.Files.Add(GetTemplateFile(web, Uri.UnescapeDataString(serverRelativeUrl)));
                }
            }

            List<TemplateFile> distinct = template.Files.Distinct().ToList();
            template.Files.Clear();
            template.Files.AddRange(distinct);
        }

        private static void AddIfCustomMasterPage(List<string> candidates, string masterUrl)
        {
            if (string.IsNullOrEmpty(masterUrl))
            {
                return;
            }

            if (OutOfTheBoxMasterPages.Any(m => masterUrl.EndsWith(m, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            candidates.Add(masterUrl);
        }

        /// <summary>
        /// Copies one file from the site into the template's connector.
        /// </summary>
        /// <returns><c>true</c> when the file was saved and should be listed in the template</returns>
        private async Task<bool> PersistFileAsync(PnPContext context, IWeb web, ProvisioningTemplateCreationInformation creationInformation, string serverRelativeUrl)
        {
            if (creationInformation.FileConnector == null)
            {
                const string message = "No connector is configured, so branding files cannot be persisted.";
                context.Logger?.LogError("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Error);
                return false;
            }

            if (UrlUtility.IsIisVirtualDirectory(serverRelativeUrl))
            {
                context.Logger?.LogWarning("{Source}: {Url} is served from the file system, not the content database - not retrieving it.",
                    Constants.LOGGING_SOURCE, serverRelativeUrl);
                return false;
            }

            try
            {
                IFile file = await web.GetFileByServerRelativeUrlOrDefaultAsync(serverRelativeUrl, f => f.ServerRelativeUrl).ConfigureAwait(false);

                if (file == null)
                {
                    context.Logger?.LogWarning("{Source}: {Url} is not in this web - not retrieving it.",
                        Constants.LOGGING_SOURCE, serverRelativeUrl);
                    return false;
                }

                var fullUri = new Uri(new Uri(web.Url.ToString()), file.ServerRelativeUrl);
                string folderPath = Uri.UnescapeDataString(
                    fullUri.Segments.Take(fullUri.Segments.Length - 1).Aggregate((i, x) => i + x).TrimEnd('/'));
                string fileName = Uri.UnescapeDataString(fullUri.Segments[fullUri.Segments.Length - 1]);

                string container = Uri.UnescapeDataString(
                    RemoveIgnoreCase(folderPath, web.ServerRelativeUrl)).Trim('/').Replace("/", "\\");

                using (Stream content = await file.GetContentAsync(true).ConfigureAwait(false))
                {
                    using (var buffer = new MemoryStream())
                    {
                        await content.CopyToAsync(buffer).ConfigureAwait(false);
                        buffer.Position = 0;

                        if (!string.IsNullOrEmpty(container))
                        {
                            creationInformation.FileConnector.SaveFileStream(fileName, container, buffer);
                        }
                        else
                        {
                            creationInformation.FileConnector.SaveFileStream(fileName, buffer);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: could not persist {Url}.", Constants.LOGGING_SOURCE, serverRelativeUrl);
                return false;
            }
        }

        /// <summary>
        /// Builds the template's <c>&lt;pnp:File&gt;</c> entry for a persisted branding asset.
        /// </summary>
        private TemplateFile GetTemplateFile(IWeb web, string serverRelativeUrl)
        {
            var serverUri = new Uri(web.Url.ToString());
            var fullUri = new Uri(UrlUtility.Combine($"{serverUri.Scheme}://{serverUri.Authority}", serverRelativeUrl));

            string folderPath = Uri.UnescapeDataString(
                fullUri.Segments.Take(fullUri.Segments.Length - 1).Aggregate((i, x) => i + x).TrimEnd('/'));
            string fileName = fullUri.Segments[fullUri.Segments.Length - 1];

            folderPath = RemoveIgnoreCase(folderPath, web.ServerRelativeUrl).Trim('/');

            return new TemplateFile
            {
                Folder = Tokenize(folderPath, web.Url.ToString()),
                Src = !string.IsNullOrEmpty(folderPath) ? $"{folderPath}/{fileName}" : fileName,
                Overwrite = true,
            };
        }

        #endregion

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (template.WebSettings == null)
                {
                    return parser;
                }

                WebSettingsModel webSettings = template.WebSettings;

                bool isNoScriptSite = await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false);

                IWeb web = await context.Web.GetAsync(
                    w => w.NoCrawl, w => w.CommentsOnSitePagesDisabled, w => w.ExcludeFromOfflineClient,
                    w => w.MembersCanShare, w => w.DisableFlows, w => w.DisableAppViews,
                    w => w.HorizontalQuickLaunch, w => w.SearchScope, w => w.SearchBoxInNavBar,
                    w => w.Title, w => w.Description, w => w.AlternateCssUrl, w => w.MasterUrl,
                    w => w.CustomMasterUrl, w => w.SiteLogoUrl, w => w.RequestAccessEmail,
                    w => w.WebTemplate, w => w.HasUniqueRoleAssignments, w => w.Url,
                    w => w.ServerRelativeUrl, w => w.AllProperties).ConfigureAwait(false);

                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                ApplyRequestAccessEmail(web, webSettings, parser);
                ApplyFlags(context, web, webSettings, isNoScriptSite);
                ApplyMasterPages(context, web, webSettings, parser, isNoScriptSite);
                ApplyTitleAndDescription(web, webSettings, parser);

                if (!string.IsNullOrEmpty(webSettings.AlternateCSS))
                {
                    string alternateCss = parser.ParseString(webSettings.AlternateCSS);
                    if (alternateCss != web.AlternateCssUrl)
                    {
                        web.AlternateCssUrl = alternateCss;
                    }
                }


                await web.UpdateAsync().ConfigureAwait(false);

                await ApplyWelcomePageAsync(context, webSettings, parser).ConfigureAwait(false);

                await ApplySiteLogoAsync(context, template, webSettings, parser).ConfigureAwait(false);

                await ApplySearchCenterUrlAsync(context, web, webSettings, parser).ConfigureAwait(false);

                await ApplyHubSiteAsync(context, webSettings, parser).ConfigureAwait(false);

                return parser;
            }
        }

        /// <summary>
        /// Sets the access request email, when this web owns its own permissions.
        /// </summary>
        private static void ApplyRequestAccessEmail(IWeb web, WebSettingsModel webSettings, TokenParser parser)
        {
            if (IsSubSite(web) && !web.HasUniqueRoleAssignments)
            {
                return;
            }

            string requestAccessEmail = parser.ParseString(webSettings.RequestAccessEmail);
            if (string.IsNullOrEmpty(requestAccessEmail))
            {
                return;
            }

            if (requestAccessEmail.Length > 255)
            {
                requestAccessEmail = requestAccessEmail.Substring(0, 255);
            }

            web.RequestAccessEmail = requestAccessEmail;
        }

        private void ApplyFlags(PnPContext context, IWeb web, WebSettingsModel webSettings, bool isNoScriptSite)
        {
            if (!isNoScriptSite)
            {
                web.NoCrawl = webSettings.NoCrawl;
            }
            else
            {
                LogNoScriptSkip(context, "NoCrawl");
            }

            SetIfChanged(web.CommentsOnSitePagesDisabled, webSettings.CommentsOnSitePagesDisabled, v => web.CommentsOnSitePagesDisabled = v);
            SetIfChanged(web.ExcludeFromOfflineClient, webSettings.ExcludeFromOfflineClient, v => web.ExcludeFromOfflineClient = v);
            SetIfChanged(web.MembersCanShare, webSettings.MembersCanShare, v => web.MembersCanShare = v);
            SetIfChanged(web.DisableFlows, webSettings.DisableFlows, v => web.DisableFlows = v);
            SetIfChanged(web.DisableAppViews, webSettings.DisableAppViews, v => web.DisableAppViews = v);
            SetIfChanged(web.HorizontalQuickLaunch, webSettings.HorizontalQuickLaunch, v => web.HorizontalQuickLaunch = v);

            CoreSearchScope searchScope = ToCoreSearchScope(webSettings.SearchScope);
            if (web.SearchScope != searchScope)
            {
                web.SearchScope = searchScope;
            }

            CoreSearchBoxInNavBar searchBox = ToCoreSearchBox(webSettings.SearchBoxInNavBar);
            if (web.SearchBoxInNavBar != searchBox)
            {
                web.SearchBoxInNavBar = searchBox;
            }
        }

        private void ApplyMasterPages(PnPContext context, IWeb web, WebSettingsModel webSettings, TokenParser parser, bool isNoScriptSite)
        {
            string masterUrl = parser.ParseString(webSettings.MasterPageUrl);
            if (!string.IsNullOrEmpty(masterUrl))
            {
                if (!isNoScriptSite)
                {
                    web.MasterUrl = masterUrl;
                }
                else
                {
                    LogNoScriptSkip(context, "MasterPageUrl");
                }
            }

            string customMasterUrl = parser.ParseString(webSettings.CustomMasterPageUrl);
            if (!string.IsNullOrEmpty(customMasterUrl))
            {
                if (!isNoScriptSite)
                {
                    web.CustomMasterUrl = customMasterUrl;
                }
                else
                {
                    LogNoScriptSkip(context, "CustomMasterPageUrl");
                }
            }
        }

        private static void ApplyTitleAndDescription(IWeb web, WebSettingsModel webSettings, TokenParser parser)
        {
            if (!string.IsNullOrEmpty(webSettings.Title))
            {
                string title = parser.ParseString(webSettings.Title);
                if (title != web.Title)
                {
                    web.Title = title;
                }
            }

            if (!string.IsNullOrEmpty(webSettings.Description))
            {
                string description = parser.ParseString(webSettings.Description);
                if (description != web.Description)
                {
                    web.Description = description;
                }
            }
        }

        private static async Task ApplyWelcomePageAsync(PnPContext context, WebSettingsModel webSettings, TokenParser parser)
        {
            string welcomePage = parser.ParseString(webSettings.WelcomePage);
            if (string.IsNullOrEmpty(welcomePage))
            {
                return;
            }

            IFolder rootFolder = await context.Web.GetFolderByServerRelativeUrlAsync(
                context.Web.ServerRelativeUrl, f => f.WelcomePage).ConfigureAwait(false);

            if (rootFolder.WelcomePage == welcomePage)
            {
                return;
            }

            rootFolder.WelcomePage = welcomePage;
            await rootFolder.UpdateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Applies the site logo and, when the template carries one, its thumbnail.
        /// </summary>
        private async Task ApplySiteLogoAsync(PnPContext context, ProvisioningTemplate template, WebSettingsModel webSettings, TokenParser parser)
        {
            if (webSettings.SiteLogo == null && webSettings.SiteLogoThumbnail == null)
            {
                return;
            }

            IChromeOptions chrome = await context.Web.GetBrandingManager().GetChromeOptionsAsync().ConfigureAwait(false);
            bool changed = false;

            if (!string.IsNullOrEmpty(webSettings.SiteLogo))
            {
                string logoPath = parser.ParseString(webSettings.SiteLogo);

                if (logoPath.IndexOf("_api/groupservice/getgroupimage", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    changed |= await UploadLogoAsync(context, template, logoPath,
                        (name, stream) => chrome.Header.SetSiteLogoAsync(name, stream, true)).ConfigureAwait(false);
                }
            }

            if (!string.IsNullOrEmpty(webSettings.SiteLogoThumbnail))
            {
                string thumbnailPath = parser.ParseString(webSettings.SiteLogoThumbnail);
                changed |= await UploadLogoAsync(context, template, thumbnailPath,
                    (name, stream) => chrome.Header.SetSiteLogoThumbnailAsync(name, stream, true)).ConfigureAwait(false);
            }

            if (changed)
            {
                await context.Web.GetBrandingManager().SetChromeOptionsAsync(chrome).ConfigureAwait(false);
            }
        }

        private async Task<bool> UploadLogoAsync(PnPContext context, ProvisioningTemplate template, string path, Func<string, Stream, Task> upload)
        {
            if (template.Connector == null)
            {
                string message = $"The template has no connector, so '{path}' cannot be read.";
                context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
                return false;
            }

            byte[] bytes;
            try
            {
                bytes = ConnectorFileHelper.GetFileBytes(template.Connector, path);
            }
            catch (ArgumentException ex)
            {
                string message = $"The site logo '{path}' is not in the template's connector - it was not applied.";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
                return false;
            }

            if (bytes == null || bytes.Length == 0)
            {
                return false;
            }

            string fileName = GetLogoFileName(path);

            using (var stream = new MemoryStream(bytes))
            {
                await upload(fileName, stream).ConfigureAwait(false);
            }

            return true;
        }

        private static string GetLogoFileName(string path)
        {
            int separator = path.LastIndexOfAny(new[] { '/', '\\' });
            string fileName = separator >= 0 ? path.Substring(separator + 1) : path;

            int query = fileName.IndexOf('?');
            if (query > 0)
            {
                fileName = fileName.Substring(0, query);
            }

            return string.IsNullOrEmpty(fileName) ? "logo.jpg" : fileName;
        }

        private async Task ApplySearchCenterUrlAsync(PnPContext context, IWeb web, WebSettingsModel webSettings, TokenParser parser)
        {
            string searchCenterUrl = parser.ParseString(webSettings.SearchCenterUrl);
            if (string.IsNullOrEmpty(searchCenterUrl) || GetSearchCenterUrl(web, urlOnly: true) == searchCenterUrl)
            {
                return;
            }

            if (await context.Web.IsNoScriptSiteAsync().ConfigureAwait(false))
            {
                LogNoScriptSkip(context, "SearchCenterUrl");
                return;
            }

            string keyName = IsSubSite(web) ? "SRCH_SB_SET_WEB" : "SRCH_SB_SET_SITE";

            web.AllProperties[keyName] = JsonSerializer.Serialize(new SearchSettingsPayload
            {
                Inherit = false,
                ResultsPageAddress = searchCenterUrl,
                ShowNavigation = false,
            });

            await web.AllProperties.UpdateAsync().ConfigureAwait(false);
        }

        private async Task ApplyHubSiteAsync(PnPContext context, WebSettingsModel webSettings, TokenParser parser)
        {
            if (string.IsNullOrEmpty(webSettings.HubSiteUrl))
            {
                return;
            }

            string hubSiteUrl = parser.ParseString(webSettings.HubSiteUrl);

            try
            {
                Guid hubSiteId = await ResolveHubSiteIdAsync(context, hubSiteUrl).ConfigureAwait(false);

                if (hubSiteId == Guid.Empty)
                {
                    string notAHub = $"'{hubSiteUrl}' is not a registered hub site - no association was made.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, notAHub);
                    WriteMessage(notAHub, ProvisioningMessageType.Warning);
                    return;
                }

                await context.Site.JoinHubSiteAsync(hubSiteId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string message = $"Hub site association failed: {ex.Message}";
                context.Logger?.LogWarning(ex, "{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                WriteMessage(message, ProvisioningMessageType.Warning);
            }
        }

        /// <summary>
        /// Turns a hub site url into the id <see cref="ISite.JoinHubSiteAsync"/> needs.
        /// </summary>
        private static async Task<Guid> ResolveHubSiteIdAsync(PnPContext context, string hubSiteUrl)
        {
            using (PnPContext hubContext = await context.CloneAsync(new Uri(hubSiteUrl)).ConfigureAwait(false))
            {
                await hubContext.Site.LoadAsync(s => s.IsHubSite, s => s.HubSiteId).ConfigureAwait(false);

                return hubContext.Site.IsHubSite ? hubContext.Site.HubSiteId : Guid.Empty;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Removes every case-insensitive occurrence of a substring.
        /// </summary>
        private static string RemoveIgnoreCase(string value, string toRemove)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(toRemove))
            {
                return value;
            }

            return Regex.Replace(value, Regex.Escape(toRemove), string.Empty, RegexOptions.IgnoreCase);
        }

        private static void SetIfChanged(bool current, bool wanted, Action<bool> set)
        {
            if (current != wanted)
            {
                set(wanted);
            }
        }

        private void LogNoScriptSkip(PnPContext context, string setting)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                "This is a NoScript site, so '{0}' could not be applied.", setting);

            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        /// <summary>
        /// Reads the web's search results page from its property bag.
        /// </summary>
        private static string GetSearchCenterUrl(IWeb web, bool urlOnly)
        {
            bool isSubSite = IsSubSite(web);
            string raw = web.AllProperties.GetString(isSubSite ? "SRCH_SB_SET_WEB" : "SRCH_SB_SET_SITE", string.Empty);

            if (!isSubSite && string.IsNullOrWhiteSpace(raw))
            {
                raw = web.AllProperties.GetString("SRCH_SB_SET_WEB", string.Empty);
            }

            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            SearchSettingsPayload settings;
            try
            {
                settings = JsonSerializer.Deserialize<SearchSettingsPayload>(raw);
            }
            catch (JsonException)
            {
                return null;
            }

            if (settings == null || settings.Inherit)
            {
                return null;
            }

            return urlOnly ? settings.ResultsPageAddress : raw;
        }

        /// <summary>
        /// Replaces the host part of a url with the <c>{hosturl}</c> token.
        /// </summary>
        private static string TokenizeHost(string webUrl, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var uri = new Uri(webUrl);
            value = Regex.Replace(value, $"{uri.Scheme}://{uri.DnsSafeHost}:{uri.Port}", "{hosturl}", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, $"{uri.Scheme}://{uri.DnsSafeHost}", "{hosturl}", RegexOptions.IgnoreCase);

            return value;
        }

        private static TemplateSearchScopes ToTemplateSearchScope(CoreSearchScope scope)
        {
            return Enum.TryParse(scope.ToString(), true, out TemplateSearchScopes parsed) ? parsed : TemplateSearchScopes.DefaultScope;
        }

        private static CoreSearchScope ToCoreSearchScope(TemplateSearchScopes scope)
        {
            return Enum.TryParse(scope.ToString(), true, out CoreSearchScope parsed) ? parsed : CoreSearchScope.DefaultScope;
        }

        private static TemplateSearchBoxInNavBar ToTemplateSearchBox(CoreSearchBoxInNavBar searchBox)
        {
            return Enum.TryParse(searchBox.ToString(), true, out TemplateSearchBoxInNavBar parsed) ? parsed : TemplateSearchBoxInNavBar.Inherit;
        }

        private static CoreSearchBoxInNavBar ToCoreSearchBox(TemplateSearchBoxInNavBar searchBox)
        {
            return Enum.TryParse(searchBox.ToString(), true, out CoreSearchBoxInNavBar parsed) ? parsed : CoreSearchBoxInNavBar.Inherit;
        }

        /// <summary>
        /// The shape of the JSON the SharePoint UI stores in <c>SRCH_SB_SET_SITE</c> /
        /// <c>SRCH_SB_SET_WEB</c>.
        /// </summary>
        private sealed class SearchSettingsPayload
        {
            public bool Inherit { get; set; }

            public string ResultsPageAddress { get; set; }

            public bool ShowNavigation { get; set; }
        }

        #endregion
    }
}
