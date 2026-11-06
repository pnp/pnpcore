using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.Provisioning.ObjectHandlers.TokenDefinitions;
using PnP.Core.Provisioning.ObjectHandlers.Utilities;
using PnP.Core.Provisioning.Services.Core.CSOM;
using PnP.Core.Provisioning.Services.Core.CSOM.Requests.Web;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CanvasControlModel = PnP.Core.Provisioning.Model.CanvasControl;
using CanvasSectionModel = PnP.Core.Provisioning.Model.CanvasSection;
using ClientSidePageModel = PnP.Core.Provisioning.Model.ClientSidePage;
using CoreIconAlignment = PnP.Core.Model.SharePoint.IconAlignment;

namespace PnP.Core.Provisioning.ObjectHandlers
{
    using UrlUtility = PnP.Core.Provisioning.Utilities.UrlUtility;

    /// <summary>
    /// Creates and updates the modern pages a template describes, with their sections, web parts,
    /// headers and translations.
    /// </summary>
    internal class ObjectClientSidePages : ObjectHandlerBase
    {
        private const string ContentTypeIdField = "ContentTypeId";
        private const string FileRefField = "FileRef";
        private const string SitePageFlagsField = "_SPSitePageFlags";

        /// <summary>The pages library, which is always at this url on a modern site.</summary>
        private const string PagesLibrary = "SitePages";

        /// <summary>
        /// The order a topic page's header controls are parked under.
        /// </summary>
        internal const float TopicHeaderControlSectionOrder = 999999;

        private static readonly Guid MultilingualPagesFeature = new Guid("24611c05-ee19-45da-955f-6602264abaf8");
        private static readonly Guid MixedRealityFeature = new Guid("2ac9c540-6db4-4155-892c-3273957f1926");

        /// <summary>The templates folder name, resolved once per run.</summary>
        private string templatesFolder;

        public override string Name => "ClientSidePages";

        public override string InternalName => "ClientSidePages";

        public override bool WillProvision(PnPContext context, ProvisioningTemplate template, ApplyConfiguration configuration)
        {
            _willProvision ??= template.ClientSidePages.Any();
            return _willProvision.Value;
        }

        public override bool WillExtract(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return false;
        }

        public override Task<ProvisioningTemplate> ExtractObjectsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            return Task.FromResult(template);
        }

        #region Apply

        public override async Task<TokenParser> ProvisionObjectsAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser, ApplyConfiguration configuration)
        {
            using (context.Logger?.BeginScope(Name))
            {
                if (!template.ClientSidePages.Any())
                {
                    return parser;
                }

                await context.Web.LoadAsync(w => w.ServerRelativeUrl, w => w.Url, w => w.Language, w => w.IsMultilingual).ConfigureAwait(false);
                await context.Site.LoadAsync(s => s.ServerRelativeUrl).ConfigureAwait(false);

                templatesFolder = await GetTemplatesFolderAsync(context).ConfigureAwait(false);

                await EnsureWebLanguagesAsync(context, template).ConfigureAwait(false);
                await EnsureSpacesAsync(context, template).ConfigureAwait(false);

                var preCreatedPages = new List<string>();
                int currentPageIndex = 0;

                foreach (ClientSidePageModel clientSidePage in template.ClientSidePages)
                {
                    string preCreated = await PreCreatePageAsync(context, template, parser, clientSidePage, ++currentPageIndex).ConfigureAwait(false);
                    if (preCreated != null)
                    {
                        preCreatedPages.Add(preCreated);
                    }

                    if (clientSidePage.Translations.Any())
                    {
                        await CreateTranslationsAsync(context, parser, clientSidePage, preCreatedPages).ConfigureAwait(false);
                    }
                }

                currentPageIndex = 0;
                int totalPages = template.ClientSidePages.Sum(p => 1 + p.Translations.Count);

                foreach (ClientSidePageModel clientSidePage in template.ClientSidePages)
                {
                    await CreatePageAsync(context, template, parser, clientSidePage, preCreatedPages, ++currentPageIndex, totalPages).ConfigureAwait(false);

                    foreach (TranslatedClientSidePage translated in clientSidePage.Translations)
                    {
                        await CreatePageAsync(context, template, parser, translated, preCreatedPages, ++currentPageIndex, totalPages).ConfigureAwait(false);
                    }
                }

                WriteMessage("Done processing Client Side Pages", ProvisioningMessageType.Completed);

                return parser;
            }
        }

        /// <summary>
        /// Enables mixed reality when the template contains a Spaces page.
        /// </summary>
        private async Task EnsureSpacesAsync(PnPContext context, ProvisioningTemplate template)
        {
            bool hasSpacesPage = template.ClientSidePages.Any(p =>
                PageLayoutType.Spaces.ToString().Equals(p.Layout, StringComparison.InvariantCultureIgnoreCase));

            if (!hasSpacesPage)
            {
                return;
            }

            await EnableFeatureAsync(context, MixedRealityFeature, "mixed reality").ConfigureAwait(false);
        }

        /// <summary>
        /// Makes the site multilingual and adds the languages the template's translations need.
        /// </summary>
        private async Task EnsureWebLanguagesAsync(PnPContext context, ProvisioningTemplate template)
        {
            var neededLanguages = new List<int>();
            int neededSourceLanguage = 0;

            foreach (ClientSidePageModel page in template.ClientSidePages.Where(p => p.Translations.Any()))
            {
                if (neededSourceLanguage == 0)
                {
                    neededSourceLanguage = page.LCID > 0 ? page.LCID : template.RegionalSettings?.LocaleId ?? 0;
                }
                else if (neededSourceLanguage != page.LCID)
                {
                    throw new InvalidOperationException(
                        "The pages in this template are based upon multiple source languages, while all pages in a site must share one.");
                }

                foreach (TranslatedClientSidePage translated in page.Translations)
                {
                    if (!neededLanguages.Contains(translated.LCID))
                    {
                        neededLanguages.Add(translated.LCID);
                    }
                }
            }

            if (neededLanguages.Count == 0)
            {
                return;
            }

            await EnableFeatureAsync(context, MultilingualPagesFeature, "multilingual pages").ConfigureAwait(false);

            int sourceLanguage = (int)context.Web.Language;
            if (sourceLanguage != neededSourceLanguage)
            {
                throw new InvalidOperationException(
                    $"The web's source language is {sourceLanguage} while the template expects {neededSourceLanguage}.");
            }

            if (!context.Web.IsMultilingual)
            {
                context.Web.IsMultilingual = true;
                await context.Web.UpdateAsync().ConfigureAwait(false);
            }

            await context.Web.LoadAsync(w => w.SupportedUILanguageIds).ConfigureAwait(false);
            List<int> toAdd = neededLanguages.Where(l => !context.Web.SupportedUILanguageIds.Contains(l)).ToList();

            if (toAdd.Count == 0)
            {
                return;
            }

            (Guid siteId, Guid webId) = await CsomRequestSender.GetSiteAndWebIdAsync(context).ConfigureAwait(false);
            await CsomRequestSender.SendAsync(context,
                new SetSupportedUILanguagesRequest(siteId, webId, toAdd, Array.Empty<int>())).ConfigureAwait(false);
        }

        private async Task EnableFeatureAsync(PnPContext context, Guid featureId, string description)
        {
            try
            {
                await context.Web.LoadAsync(w => w.Features.QueryProperties(f => f.DefinitionId)).ConfigureAwait(false);

                if (context.Web.Features.AsRequested().Any(f => f.DefinitionId == featureId))
                {
                    return;
                }

                await context.Web.Features.EnableAsync(featureId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger?.LogError(ex, "{Source}: the {Description} feature could not be enabled.",
                    Constants.LOGGING_SOURCE, description);
                throw;
            }
        }

        /// <summary>
        /// Triggers SharePoint's translation generation and registers a token for each result.
        /// </summary>
        private async Task CreateTranslationsAsync(PnPContext context, TokenParser parser, ClientSidePageModel clientSidePage, List<string> preCreatedPages)
        {
            string pageName = DeterminePageName(parser, clientSidePage);
            IPage page = await LoadForWriteAsync(context, clientSidePage, pageName, isTranslation: false).ConfigureAwait(false);

            if (page == null)
            {
                return;
            }

            IPageTranslationStatusCollection availableTranslations = await page.GetPageTranslationsAsync().ConfigureAwait(false);

            var request = new PageTranslationOptions();
            foreach (TranslatedClientSidePage translated in clientSidePage.Translations)
            {
                string culture = new CultureInfo(translated.LCID).Name;

                bool alreadyThere = availableTranslations.TranslatedLanguages
                    .Any(t => t.Culture.Equals(culture, StringComparison.InvariantCultureIgnoreCase));

                if (!alreadyThere)
                {
                    request.AddLanguage(translated.LCID);
                }
            }

            IPageTranslationStatusCollection results = null;
            if (request.LanguageCodes != null && request.LanguageCodes.Count > 0)
            {
                results = await page.TranslatePagesAsync(request).ConfigureAwait(false);
            }

            IEnumerable<IPageTranslationStatus> combined =
                results != null && results.TranslatedLanguages.Count > 0 ? results.TranslatedLanguages
                : availableTranslations?.TranslatedLanguages ?? Enumerable.Empty<IPageTranslationStatus>();

            foreach (IPageTranslationStatus translation in combined)
            {
                string url = UrlUtility.Combine(context.Web.ServerRelativeUrl, translation.Path);
                preCreatedPages.Add(url);

                await AddPageTokensAsync(context, parser, url).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a page as an empty stub, so its unique id exists before anything can reference it.
        /// </summary>
        /// <returns>The page's server relative url when it was created here, otherwise <c>null</c></returns>
        private async Task<string> PreCreatePageAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser,
            ClientSidePageModel clientSidePage, int currentPageIndex)
        {
            string pageName = DeterminePageName(parser, clientSidePage);
            string url = BuildPageUrl(context, clientSidePage, pageName);

            WriteSubProgress("ClientSidePage", $"Create {pageName} stub", currentPageIndex, template.ClientSidePages.Count);

            IFile existing = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url, f => f.UniqueId, f => f.ServerRelativeUrl).ConfigureAwait(false);

            if (existing != null)
            {
                AddPageTokens(context, parser, existing.ServerRelativeUrl, existing.UniqueId);
                return null;
            }

            IPage page = await context.Web.NewPageAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(clientSidePage.Layout) && Enum.TryParse(clientSidePage.Layout, out PageLayoutType layout))
            {
                page.LayoutType = layout;
            }

            if (IsPageTemplate(clientSidePage))
            {
                await page.SaveAsTemplateAsync(pageName).ConfigureAwait(false);
            }
            else
            {
                await page.SaveAsync(pageName).ConfigureAwait(false);
            }

            await AddPageTokensAsync(context, parser, url).ConfigureAwait(false);

            return url;
        }

        /// <summary>
        /// Applies one page: its layout, header, sections, controls and list item settings.
        /// </summary>
        private async Task CreatePageAsync(PnPContext context, ProvisioningTemplate template, TokenParser parser,
            BaseClientSidePage clientSidePage, List<string> preCreatedPages, int currentPageIndex, int totalPages)
        {
            string pageName = DeterminePageName(parser, clientSidePage);
            string url = BuildPageUrl(context, clientSidePage, pageName);

            WriteSubProgress("Provision ClientSidePage", pageName, currentPageIndex, totalPages);

            IFile existingFile = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url).ConfigureAwait(false);
            bool exists = existingFile != null;

            IPage page;
            if (exists)
            {
                if (!clientSidePage.Overwrite && !preCreatedPages.Contains(url))
                {
                    string message = $"The page '{pageName}' already exists and the template does not allow overwriting it - it was left as it is.";
                    context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
                    WriteMessage(message, ProvisioningMessageType.Warning);
                    return;
                }

                page = await LoadForWriteAsync(context, clientSidePage, pageName, clientSidePage is TranslatedClientSidePage).ConfigureAwait(false);

                if (page == null)
                {
                    return;
                }

                page.ClearPage();
            }
            else
            {
                page = await context.Web.NewPageAsync().ConfigureAwait(false);
            }

            string newTitle = parser.ParseString(clientSidePage.Title);
            if (page.PageTitle != newTitle)
            {
                page.PageTitle = newTitle;
            }

            if (!string.IsNullOrEmpty(clientSidePage.Layout) && Enum.TryParse(clientSidePage.Layout, out PageLayoutType layout))
            {
                page.LayoutType = layout;
            }

            ApplyHeader(page, clientSidePage, parser);

            if (!string.IsNullOrEmpty(clientSidePage.ThumbnailUrl))
            {
                page.ThumbnailUrl = parser.ParseString(clientSidePage.ThumbnailUrl);
            }

            if (page.LayoutType != PageLayoutType.RepostPage)
            {
                await ApplySectionsAsync(context, page, clientSidePage, parser).ConfigureAwait(false);
            }

            if (IsPageTemplate(clientSidePage))
            {
                await page.SaveAsTemplateAsync(pageName.Replace($"{templatesFolder}/", "")).ConfigureAwait(false);
            }
            else
            {
                await page.SaveAsync(pageName).ConfigureAwait(false);
            }

            await ApplyListItemSettingsAsync(context, page, clientSidePage, parser, url, exists).ConfigureAwait(false);
            await ApplyPagePropertiesAsync(context, clientSidePage, url).ConfigureAwait(false);
            await ApplyPublishingAndCommentsAsync(page, clientSidePage).ConfigureAwait(false);

            WarnOnUnsupportedSecurity(context, clientSidePage, pageName);
        }

        #endregion

        #region Header

        private void ApplyHeader(IPage page, BaseClientSidePage clientSidePage, TokenParser parser)
        {
            if (clientSidePage.Header == null)
            {
                return;
            }

            switch (clientSidePage.Header.Type)
            {
                case ClientSidePageHeaderType.None:
                    page.RemovePageHeader();
                    break;

                case ClientSidePageHeaderType.PageTitleWebPart:
                case ClientSidePageHeaderType.Default:
                    if (clientSidePage.Sections.Any(s => s.Controls.Any(c => c.Type == WebPartType.PageTitle)))
                    {
                        page.SetPageTitleWebPartPageHeader();
                    }
                    else
                    {
                        page.SetDefaultPageHeader();
                    }
                    break;

                case ClientSidePageHeaderType.Custom:
                    ApplyCustomHeader(page, clientSidePage, parser);
                    break;
            }
        }

        private static void ApplyCustomHeader(IPage page, BaseClientSidePage clientSidePage, TokenParser parser)
        {
            string imageUrl = parser.ParseString(clientSidePage.Header.ServerRelativeImageUrl);

            if (clientSidePage.Header.TranslateX.HasValue && clientSidePage.Header.TranslateY.HasValue)
            {
                page.SetCustomPageHeader(imageUrl, clientSidePage.Header.TranslateX.Value, clientSidePage.Header.TranslateY.Value);
            }
            else
            {
                page.SetCustomPageHeader(imageUrl);
            }

            if (Enum.TryParse(clientSidePage.Header.LayoutType.ToString(), out PageHeaderLayoutType headerLayout))
            {
                page.PageHeader.LayoutType = headerLayout;
            }

            if (Enum.TryParse(clientSidePage.Header.TextAlignment.ToString(), out PageHeaderTitleAlignment alignment))
            {
                page.PageHeader.TextAlignment = alignment;
            }

            page.PageHeader.ShowTopicHeader = clientSidePage.Header.ShowTopicHeader;
            page.PageHeader.ShowPublishDate = clientSidePage.Header.ShowPublishDate;
            page.PageHeader.TopicHeader = parser.ParseString(clientSidePage.Header.TopicHeader);
            page.PageHeader.AlternativeText = parser.ParseString(clientSidePage.Header.AlternativeText);
            page.PageHeader.Authors = clientSidePage.Header.Authors;
            page.PageHeader.AuthorByLine = clientSidePage.Header.AuthorByLine;
            page.PageHeader.AuthorByLineId = clientSidePage.Header.AuthorByLineId;
        }

        #endregion

        #region Sections and controls

        private async Task ApplySectionsAsync(PnPContext context, IPage page, BaseClientSidePage clientSidePage, TokenParser parser)
        {
            IEnumerable<IPageComponent> availableComponents = await page.AvailablePageComponentsAsync().ConfigureAwait(false);

            if (!clientSidePage.Sections.Any())
            {
                clientSidePage.Sections.Add(new CanvasSectionModel { Type = CanvasSectionType.OneColumn, Order = 10 });
            }

            int sectionIndex = -1;

            foreach (CanvasSectionModel section in clientSidePage.Sections)
            {
                if (section.Order == TopicHeaderControlSectionOrder)
                {
                    continue;
                }

                sectionIndex++;
                AddSection(page, section);
                ConfigureCollapsibleSection(page.Sections[sectionIndex], section);

                if (!section.Controls.Any())
                {
                    continue;
                }

                foreach (CanvasControlModel control in section.Controls.Where(c => c.Column <= 0))
                {
                    control.Column = 1;
                }

                foreach (CanvasControlModel control in section.Controls)
                {
                    await AddControlAsync(context, page, section, sectionIndex, control, parser, availableComponents).ConfigureAwait(false);
                }
            }
        }

        private static void AddSection(IPage page, CanvasSectionModel section)
        {
            CanvasSectionTemplate template = ToCanvasSectionTemplate(section.Type);

            bool hasVerticalSection = section.Type == CanvasSectionType.OneColumnVerticalSection
                || section.Type == CanvasSectionType.TwoColumnVerticalSection
                || section.Type == CanvasSectionType.TwoColumnLeftVerticalSection
                || section.Type == CanvasSectionType.TwoColumnRightVerticalSection
                || section.Type == CanvasSectionType.ThreeColumnVerticalSection
                || section.Type == CanvasSectionType.FlexibleLayoutSection
                || section.Type == CanvasSectionType.FlexibleLayoutVerticalSection;

            if (hasVerticalSection)
            {
                page.AddSection(template, section.Order, (int)section.BackgroundEmphasis, (int)section.VerticalSectionEmphasis);
            }
            else
            {
                page.AddSection(template, section.Order, (int)section.BackgroundEmphasis);
            }
        }

        private static void ConfigureCollapsibleSection(ICanvasSection target, CanvasSectionModel section)
        {
            if (!section.Collapsible)
            {
                return;
            }

            target.Collapsible = true;
            target.IsExpanded = section.IsExpanded;
            target.DisplayName = section.DisplayName;
            target.ShowDividerLine = section.ShowDividerLine;
            target.IconAlignment = (CoreIconAlignment)(int)section.IconAlignment;
        }

        private async Task AddControlAsync(PnPContext context, IPage page, CanvasSectionModel section, int sectionIndex,
            CanvasControlModel control, TokenParser parser, IEnumerable<IPageComponent> availableComponents)
        {
            ICanvasColumn column = page.Sections[sectionIndex].Columns[control.Column - 1];

            if (control.Type == WebPartType.Text)
            {
                AddTextControl(page, section, column, control, parser);
                return;
            }

            control.JsonControlData = parser.ParseString(control.JsonControlData);

            await CanvasControlPostProcessor.ProcessAsync(context, control).ConfigureAwait(false);

            IPageComponent baseControl = ResolveComponent(page, control, availableComponents);

            IPageWebPart webPart = null;
            if (baseControl != null)
            {
                webPart = page.NewWebPart(baseControl);
            }
            else if (string.IsNullOrWhiteSpace(control.JsonControlData))
            {
                webPart = page.NewWebPart();
            }

            ControlFlexLayoutPosition flexPosition = null;

            if (!string.IsNullOrEmpty(control.JsonControlData))
            {
                JsonObject json = TryParseObject(context, control.JsonControlData);
                if (json == null)
                {
                    return;
                }

                if (webPart == null)
                {
                    webPart = InstantiateFromControlType(page, column, json, parser);

                    if (webPart == null)
                    {
                        return;
                    }
                }

                SetDefaultProperties(column, webPart, json, parser);
                webPart.PropertiesJson = control.JsonControlData;

                if (baseControl == null)
                {
                    SetWebPartIdFromJson(webPart, json);
                }

                if (IsFlexibleLayout(section.Type))
                {
                    flexPosition = GetControlFlexLayoutPosition(json);
                }
            }

            if (webPart == null)
            {
                return;
            }

            webPart.Order = control.Order;
            page.AddControl(webPart, column, control.Order, flexPosition);
        }

        private static void AddTextControl(IPage page, CanvasSectionModel section, ICanvasColumn column, CanvasControlModel control, TokenParser parser)
        {
            IPageText textControl = page.NewTextPart();
            ControlFlexLayoutPosition flexPosition = null;

            if (control.ControlProperties.Any())
            {
                textControl.Text = parser.ParseString(control.ControlProperties.First().Value);

                if (!string.IsNullOrEmpty(control.JsonControlData)
                    && JsonNode.Parse(control.JsonControlData) is JsonObject json)
                {
                    SetZoneId(column, json);

                    if (IsFlexibleLayout(section.Type))
                    {
                        flexPosition = GetControlFlexLayoutPosition(json);
                        SetZoneReflowStrategy(column, json);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(control.JsonControlData))
            {
                if (JsonNode.Parse(control.JsonControlData) is JsonObject json && json.Count > 0)
                {
                    textControl.Text = parser.ParseString(json.First().Value?.ToString());
                }
            }

            page.AddControl(textControl, column, control.Order, flexPosition);
        }

        /// <summary>
        /// Finds the installed component a template control names.
        /// </summary>
        private static IPageComponent ResolveComponent(IPage page, CanvasControlModel control, IEnumerable<IPageComponent> availableComponents)
        {
            if (control.Type == WebPartType.Custom)
            {
                if (!string.IsNullOrEmpty(control.CustomWebPartName))
                {
                    return availableComponents.FirstOrDefault(c =>
                        c.Name.Equals(control.CustomWebPartName, StringComparison.InvariantCultureIgnoreCase));
                }

                if (control.ControlId != Guid.Empty)
                {
                    return availableComponents.FirstOrDefault(c => c.Id.Equals($"{{{control.ControlId}}}", StringComparison.CurrentCultureIgnoreCase))
                        ?? availableComponents.FirstOrDefault(c => c.Id.Equals(control.ControlId.ToString(), StringComparison.InvariantCultureIgnoreCase));
                }

                return null;
            }

            if (!DefaultWebPartMap.TryGetDefaultWebPart(control.Type, out DefaultWebPart defaultWebPart))
            {
                return null;
            }

            string webPartId = page.DefaultWebPartToWebPartId(defaultWebPart);

            return availableComponents.FirstOrDefault(c => c.Name.Equals(webPartId, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Builds a web part for control data whose component is not installed here.
        /// </summary>
        private static IPageWebPart InstantiateFromControlType(IPage page, ICanvasColumn column, JsonObject json, TokenParser parser)
        {
            int controlType = json.TryGetPropertyValue("controlType", out JsonNode node)
                && int.TryParse(node?.ToString(), out int parsed) ? parsed : 0;

            switch (controlType)
            {
                case 1: // empty section
                    SetDefaultProperties(column, null, json, parser);
                    return null;

                case 14: // section background control
                    return page.NewSectionBackgroundControl();

                default:
                    return page.NewWebPart();
            }
        }

        /// <summary>
        /// Restores a web part's id when the component itself is not installed on this site.
        /// </summary>
        private static void SetWebPartIdFromJson(IPageWebPart webPart, JsonObject json)
        {
            if (!json.TryGetPropertyValue("id", out JsonNode idNode)
                || !Guid.TryParse(idNode?.ToString(), out Guid webPartId))
            {
                return;
            }

            System.Reflection.PropertyInfo property = webPart.GetType().GetProperty("WebPartId");
            property?.SetValue(webPart, webPartId.ToString());
        }

        #endregion

        #region The page's list item

        /// <summary>
        /// Applies the settings that live on the page's list item rather than on its canvas.
        /// </summary>
        private async Task ApplyListItemSettingsAsync(PnPContext context, IPage page, BaseClientSidePage clientSidePage,
            TokenParser parser, string url, bool existed)
        {
            IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url,
                f => f.ListItemAllFields).ConfigureAwait(false);

            if (file?.ListItemAllFields == null)
            {
                return;
            }

            IListItem listItem = file.ListItemAllFields;
            bool dirty = false;

            if (!string.IsNullOrEmpty(clientSidePage.ContentTypeID))
            {
                string bestMatch = await FindBestMatchContentTypeIdAsync(context, page, clientSidePage.ContentTypeID).ConfigureAwait(false);
                string currentContentTypeId = listItem[ContentTypeIdField]?.ToString();

                if (!string.IsNullOrEmpty(bestMatch)
                    && (string.IsNullOrEmpty(currentContentTypeId)
                        || !currentContentTypeId.StartsWith(bestMatch, StringComparison.InvariantCultureIgnoreCase)))
                {
                    listItem[ContentTypeIdField] = bestMatch;
                    dirty = true;
                }
            }

            if (clientSidePage.PromoteAsTemplate && page.LayoutType == PageLayoutType.Article)
            {
                listItem[SitePageFlagsField] = ";#Template;#";
                dirty = true;
            }

            if (dirty)
            {
                if (existed)
                {
                    await listItem.SystemUpdateAsync().ConfigureAwait(false);
                }
                else
                {
                    await listItem.UpdateOverwriteVersionAsync().ConfigureAwait(false);
                }
            }

            WarnOnUnsupportedFieldValues(context, clientSidePage, parser);
        }

        /// <summary>
        /// Finds the content type on the pages library that best matches the template's id.
        /// </summary>
        private static async Task<string> FindBestMatchContentTypeIdAsync(PnPContext context, IPage page, string contentTypeId)
        {
            try
            {
                IList pagesLibrary = page.PagesLibrary;
                await pagesLibrary.LoadAsync(l => l.ContentTypes.QueryProperties(c => c.StringId)).ConfigureAwait(false);

                return pagesLibrary.ContentTypes.AsRequested()
                    .Where(c => c.StringId.StartsWith(contentTypeId, StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(c => c.StringId.Length)
                    .Select(c => c.StringId)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: could not resolve the page content type {ContentTypeId}.",
                    Constants.LOGGING_SOURCE, contentTypeId);
                return null;
            }
        }

        /// <summary>
        /// Writes the page file's own property bag entries.
        /// </summary>
        private static async Task ApplyPagePropertiesAsync(PnPContext context, BaseClientSidePage clientSidePage, string url)
        {
            if (clientSidePage.Properties == null || !clientSidePage.Properties.Any())
            {
                return;
            }

            IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url, f => f.Properties).ConfigureAwait(false);
            if (file == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> property in clientSidePage.Properties.Where(p => !string.IsNullOrEmpty(p.Key)))
            {
                file.Properties[property.Key] = property.Value;
            }

            await file.Properties.UpdateAsync().ConfigureAwait(false);
        }

        private static async Task ApplyPublishingAndCommentsAsync(IPage page, BaseClientSidePage clientSidePage)
        {
            if (page.LayoutType != PageLayoutType.SingleWebPartAppPage)
            {
                if (page.LayoutType != PageLayoutType.Home && !clientSidePage.PromoteAsTemplate && clientSidePage.PromoteAsNewsArticle)
                {
                    await page.PromoteAsNewsArticleAsync().ConfigureAwait(false);
                }

                if (page.LayoutType != PageLayoutType.RepostPage)
                {
                    if (clientSidePage.EnableComments)
                    {
                        await page.EnableCommentsAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await page.DisableCommentsAsync().ConfigureAwait(false);
                    }
                }
            }

            if (clientSidePage.Publish && !clientSidePage.PromoteAsTemplate)
            {
                await page.PublishAsync().ConfigureAwait(false);
            }
        }

        private void WarnOnUnsupportedFieldValues(PnPContext context, BaseClientSidePage clientSidePage, TokenParser parser)
        {
            if (clientSidePage.FieldValues == null || !clientSidePage.FieldValues.Any())
            {
                return;
            }

            _ = parser;

            string message = string.Format(CultureInfo.CurrentCulture,
                "The page carries {0} field value(s) ({1}). Writing those needs ListItemUtilities, which lands in phase 6 - they were NOT applied.",
                clientSidePage.FieldValues.Count, string.Join(", ", clientSidePage.FieldValues.Keys));

            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        private void WarnOnUnsupportedSecurity(PnPContext context, BaseClientSidePage clientSidePage, string pageName)
        {
            if (clientSidePage.Security == null || clientSidePage.Security.RoleAssignments.Count == 0)
            {
                return;
            }

            string message = $"The page '{pageName}' declares unique permissions. Applying those needs the security handler, which lands in phase 6 - they were NOT applied.";

            context.Logger?.LogWarning("{Source}: {Message}", Constants.LOGGING_SOURCE, message);
            WriteMessage(message, ProvisioningMessageType.Warning);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Loads a page for writing, looking in the templates folder when the template says so.
        /// </summary>
        private async Task<IPage> LoadForWriteAsync(PnPContext context, BaseClientSidePage clientSidePage, string pageName, bool isTranslation)
        {
            string lookupName = IsPageTemplate(clientSidePage) && !isTranslation
                ? $"{templatesFolder}/{pageName}"
                : pageName;

            return await PageLookup.LoadAsync(context, lookupName).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether this page is saved into the pages library's templates folder.
        /// </summary>
        private static bool IsPageTemplate(BaseClientSidePage clientSidePage)
        {
            return clientSidePage.PromoteAsTemplate
                && "Article".Equals(clientSidePage.Layout, StringComparison.Ordinal);
        }

        private string BuildPageUrl(PnPContext context, BaseClientSidePage clientSidePage, string pageName)
        {
            string url = IsPageTemplate(clientSidePage) && !(clientSidePage is TranslatedClientSidePage)
                ? $"{PagesLibrary}/{templatesFolder}/{pageName}"
                : $"{PagesLibrary}/{pageName}";

            return UrlUtility.Combine(context.Web.ServerRelativeUrl, url);
        }

        private static async Task<string> GetTemplatesFolderAsync(PnPContext context)
        {
            try
            {
                IPage dummyPage = await context.Web.NewPageAsync().ConfigureAwait(false);
                string folder = await dummyPage.GetTemplatesFolderAsync().ConfigureAwait(false);

                return !string.IsNullOrEmpty(folder) ? folder : "Templates";
            }
            catch (Exception)
            {
                return "Templates";
            }
        }

        /// <summary>
        /// The page's name as the template gives it, normalised to a single .aspx extension.
        /// </summary>
        private static string DeterminePageName(TokenParser parser, BaseClientSidePage clientSidePage)
        {
            if (clientSidePage is TranslatedClientSidePage translated)
            {
                return parser.ParseString(translated.PageName);
            }

            var page = (ClientSidePageModel)clientSidePage;
            string parsed = parser.ParseString(page.PageName);

            if (clientSidePage.PromoteAsTemplate)
            {
                return $"{System.IO.Path.GetFileNameWithoutExtension(parsed)}.aspx";
            }

            string nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(parsed);
            string folder = System.IO.Path.GetDirectoryName(parsed);

            return string.IsNullOrEmpty(folder)
                ? $"{nameWithoutExtension}.aspx"
                : $"{folder.Replace("\\", "/")}/{nameWithoutExtension}.aspx";
        }

        private static async Task AddPageTokensAsync(PnPContext context, TokenParser parser, string url)
        {
            IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url,
                f => f.UniqueId, f => f.ServerRelativeUrl).ConfigureAwait(false);

            if (file != null)
            {
                AddPageTokens(context, parser, file.ServerRelativeUrl, file.UniqueId);
            }
        }

        private static void AddPageTokens(PnPContext context, TokenParser parser, string serverRelativeUrl, Guid uniqueId)
        {
            string pageUrl = serverRelativeUrl.Substring(context.Web.ServerRelativeUrl.Length).TrimStart('/');

            parser.AddToken(new PageUniqueIdToken(context, pageUrl, uniqueId));
            parser.AddToken(new PageUniqueIdEncodedToken(context, pageUrl, uniqueId));
        }

        private static bool IsFlexibleLayout(CanvasSectionType sectionType)
        {
            return sectionType == CanvasSectionType.FlexibleLayoutSection
                || sectionType == CanvasSectionType.FlexibleLayoutVerticalSection;
        }

        private static CanvasSectionTemplate ToCanvasSectionTemplate(CanvasSectionType sectionType)
        {
            return Enum.TryParse(sectionType.ToString(), out CanvasSectionTemplate template)
                ? template
                : CanvasSectionTemplate.OneColumn;
        }

        private static JsonObject TryParseObject(PnPContext context, string json)
        {
            try
            {
                return JsonNode.Parse(json) as JsonObject;
            }
            catch (JsonException ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: a control's data is not valid JSON and the control was skipped.",
                    Constants.LOGGING_SOURCE);
                return null;
            }
        }

        private static void SetDefaultProperties(ICanvasColumn column, IPageWebPart webPart, JsonObject json, TokenParser parser)
        {
            if (webPart != null)
            {
                if (json.TryGetPropertyValue("instanceId", out JsonNode instanceNode)
                    && Guid.TryParse(instanceNode?.ToString(), out Guid instanceId))
                {
                    webPart.InstanceId = instanceId;
                }

                if (json.TryGetPropertyValue("title", out JsonNode titleNode) && titleNode != null)
                {
                    webPart.Title = parser.ParseString(titleNode.ToString());
                }

                if (json.TryGetPropertyValue("description", out JsonNode descriptionNode) && descriptionNode != null)
                {
                    webPart.Description = parser.ParseString(descriptionNode.ToString());
                }
            }

            SetZoneId(column, json);

            if (column.Section.Type == CanvasSectionTemplate.FlexibleLayoutSection
                || column.Section.Type == CanvasSectionTemplate.FlexibleLayoutVerticalSection)
            {
                SetZoneReflowStrategy(column, json);
            }
        }

        private static void SetZoneId(ICanvasColumn column, JsonObject json)
        {
            if (json.TryGetPropertyValue("position", out JsonNode positionNode)
                && positionNode is JsonObject position
                && position.TryGetPropertyValue("zoneId", out JsonNode zoneIdNode))
            {
                string zoneId = zoneIdNode?.ToString();
                if (!string.IsNullOrEmpty(zoneId))
                {
                    column.SetZoneId(zoneId);
                }
            }
        }

        private static void SetZoneReflowStrategy(ICanvasColumn column, JsonObject json)
        {
            if (!json.TryGetPropertyValue("zoneReflowStrategy", out JsonNode strategyNode)
                || !(strategyNode is JsonObject strategy)
                || !strategy.TryGetPropertyValue("axis", out JsonNode axisNode)
                || !int.TryParse(axisNode?.ToString(), out int axis))
            {
                return;
            }

            if (axis == 0)
            {
                column.ZoneReflowStrategy = ZoneReflowStrategy.TopToDown;
            }
            else if (axis == 1)
            {
                column.ZoneReflowStrategy = ZoneReflowStrategy.LeftToRight;
            }
        }

        private static ControlFlexLayoutPosition GetControlFlexLayoutPosition(JsonObject json)
        {
            if (!json.TryGetPropertyValue("flexibleLayoutPosition", out JsonNode positionNode)
                || !(positionNode is JsonObject position))
            {
                return null;
            }

            if (!position.TryGetPropertyValue("lg", out JsonNode largeNode) || !(largeNode is JsonObject large))
            {
                return null;
            }

            return new ControlFlexLayoutPosition
            {
                XPos = GetDouble(large, "x"),
                YPos = GetDouble(large, "y"),
                Width = GetDouble(large, "w"),
                Height = GetDouble(large, "h"),
                WpGroupId = position.TryGetPropertyValue("groupId", out JsonNode groupNode)
                    && Guid.TryParse(groupNode?.ToString(), out Guid groupId) ? groupId : null,
            };
        }

        private static double GetDouble(JsonObject json, string name)
        {
            return json.TryGetPropertyValue(name, out JsonNode node)
                && double.TryParse(node?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                    ? value
                    : 0;
        }

        #endregion
    }
}
