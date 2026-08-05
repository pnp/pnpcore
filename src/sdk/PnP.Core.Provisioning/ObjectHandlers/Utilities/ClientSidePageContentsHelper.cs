using Microsoft.Extensions.Logging;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Provisioning.Model;
using PnP.Core.Provisioning.Model.Configuration;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CanvasControlModel = PnP.Core.Provisioning.Model.CanvasControl;
using CanvasSectionModel = PnP.Core.Provisioning.Model.CanvasSection;
using ClientSidePageModel = PnP.Core.Provisioning.Model.ClientSidePage;
using CoreIconAlignment = PnP.Core.Model.SharePoint.IconAlignment;
using FileLevelModel = PnP.Core.Provisioning.Model.FileLevel;
using TemplateFile = PnP.Core.Provisioning.Model.File;
using TemplateIconAlignment = PnP.Core.Provisioning.Model.IconAlignment;

namespace PnP.Core.Provisioning.ObjectHandlers.Utilities
{
    /// <summary>
    /// Reads one modern page off a site and writes it into a provisioning template.
    /// </summary>
    internal sealed class ClientSidePageContentsHelper
    {
        internal const string PromotedStateField = "PromotedState";
        internal const string SpaceContentField = "SpaceContent";
        internal const string TopicEntityId = "_EntityId";
        internal const string TopicEntityRelations = "_EntityRelations";
        internal const string TopicEntityType = "_EntityType";

        private const string ContentTypeIdField = "ContentTypeId";

        /// <summary>Content type id of the out-of-the-box modern page. Never written to a template.</summary>
        private const string ModernArticlePageContentTypeId = "0x0101009D1CB255DA76424F860D91F20E6C4118";

        #region Compiled patterns

        // A quoted GUID, optionally braced.
        private static readonly Regex GuidPattern =
            new Regex("\"{?[a-fA-F0-9]{8}-([a-fA-F0-9]{4}-){3}[a-fA-F0-9]{12}}?\"", RegexOptions.Compiled);

        // A GUID inside a query string, where the dashes may be percent encoded.
        private static readonly Regex GuidPatternEncoded =
            new Regex("=[a-fA-F0-9]{8}(?:%2D|-)([a-fA-F0-9]{4}(?:%2D|-)){3}[a-fA-F0-9]{12}", RegexOptions.Compiled);

        // A GUID with the dashes stripped, as thumbnail urls store it.
        private static readonly Regex GuidPatternNoDashes =
            new Regex("[a-fA-F0-9]{32}", RegexOptions.Compiled);

        private static readonly Regex GuidPatternOptionalBrackets =
            new Regex("(?<Bracket>\\{)?[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}(?(Bracket)\\}|)",
                RegexOptions.Compiled);

        // Urls into SiteAssets/SitePages, which is where a page's own images live.
        //
        // The character class is deliberately narrow. An earlier ".*" version of this pattern
        // backtracked catastrophically on long control data and hung the export.
        private static readonly Regex SiteAssetUrls =
            new Regex("(?:\")(?<AssetUrl>[\\w|\\.|\\/|:|-]*\\/SiteAssets\\/SitePages\\/[\\w|\\.|\\/|:|-]*)(?:\")",
                RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// Extracts one page into the template.
        /// </summary>
        internal async Task ExtractClientSidePageAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, PageToExport page, ProvisioningMessagesDelegate messagesDelegate)
        {
            bool excludeAuthorInformation = configuration?.Pages?.ExcludeAuthorInformation ?? false;
            bool persistFiles = configuration?.ToCreationInformation()?.PersistBrandingFiles ?? false;

            IPage pageToExtract;
            try
            {
                pageToExtract = await PageLookup.LoadAsync(context, page.PageName).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                context.Logger?.LogWarning(ex, "{Source}: '{Page}' is not a valid modern page and was skipped.",
                    Constants.LOGGING_SOURCE, page.PageName);
                return;
            }

            if (pageToExtract == null)
            {
                return;
            }

            if (pageToExtract.Sections.Count == 0 && pageToExtract.Controls.Count == 0 && page.IsHomePage)
            {
                // An uncustomised default home page has no page definition stored on the list item.
                // Exporting it would produce an empty page that overwrites a perfectly good default.
                context.Logger?.LogInformation("{Source}: the home page is the uncustomised default and was not extracted.",
                    Constants.LOGGING_SOURCE);
                return;
            }

            var errorneousOrNonImageFileGuids = new List<string>();

            BaseClientSidePage extractedPage = await BuildPageAsync(
                context, template, configuration, page, pageToExtract, excludeAuthorInformation, persistFiles).ConfigureAwait(false);

            await AddSectionsAsync(context, template, configuration, pageToExtract, extractedPage,
                errorneousOrNonImageFileGuids, persistFiles).ConfigureAwait(false);

            // Editing a modern home page can leave sections ordered 0.5, 0.75, ... Renumbering from
            // 1 keeps the template readable and re-applies in the same order.
            int sectionOrder = 1;
            foreach (CanvasSectionModel section in extractedPage.Sections)
            {
                section.Order = sectionOrder++;
            }

            if (pageToExtract.LayoutType == PageLayoutType.Spaces && !string.IsNullOrEmpty(pageToExtract.SpaceContent))
            {
                extractedPage.FieldValues.Add(SpaceContentField, pageToExtract.SpaceContent);
            }

            if (pageToExtract.LayoutType == PageLayoutType.Topic)
            {
                await AddTopicHeaderControlsAsync(context, template, configuration, pageToExtract, extractedPage,
                    errorneousOrNonImageFileGuids, persistFiles).ConfigureAwait(false);

                extractedPage.FieldValues.Add(TopicEntityId, pageToExtract.EntityId ?? "");
                extractedPage.FieldValues.Add(TopicEntityType, pageToExtract.EntityType ?? "");
                extractedPage.FieldValues.Add(TopicEntityRelations, pageToExtract.EntityRelations ?? "");
            }

            AddToTemplate(context, template, page, extractedPage);

            _ = messagesDelegate;
        }

        #region The page itself

        private async Task<BaseClientSidePage> BuildPageAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, PageToExport page, IPage pageToExtract, bool excludeAuthorInformation, bool persistFiles)
        {
            string pageContentTypeId = pageToExtract.PageListItem?[ContentTypeIdField]?.ToString();
            if (!string.IsNullOrEmpty(pageContentTypeId))
            {
                pageContentTypeId = GetParentIdValue(pageContentTypeId);
            }

            int promotedState = 0;
            if (pageToExtract.PageListItem?[PromotedStateField] != null)
            {
                int.TryParse(pageToExtract.PageListItem[PromotedStateField].ToString(),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out promotedState);
            }

            bool isNews = pageToExtract.LayoutType != PageLayoutType.Home
                && promotedState == (int)PromotedState.Promoted;

            BaseClientSidePage extractedPage;
            if (page.IsTranslation)
            {
                extractedPage = new TranslatedClientSidePage { PageName = page.PageName };
            }
            else
            {
                extractedPage = new ClientSidePageModel { PageName = page.PageName };
            }

            extractedPage.PromoteAsNewsArticle = isNews;
            extractedPage.PromoteAsTemplate = page.IsTemplate;
            extractedPage.Overwrite = true;
            extractedPage.Publish = true;
            extractedPage.Layout = pageToExtract.LayoutType.ToString();
            extractedPage.EnableComments = !await pageToExtract.AreCommentsDisabledAsync().ConfigureAwait(false);
            extractedPage.Title = pageToExtract.PageTitle;
            extractedPage.ContentTypeID = !string.IsNullOrEmpty(pageContentTypeId)
                && !pageContentTypeId.Equals(ModernArticlePageContentTypeId, StringComparison.InvariantCultureIgnoreCase)
                    ? pageContentTypeId
                    : null;
            extractedPage.ThumbnailUrl = pageToExtract.ThumbnailUrl != null
                ? await TokenizeJsonControlDataAsync(context, pageToExtract.ThumbnailUrl).ConfigureAwait(false)
                : "";

            await AddHeaderAsync(context, template, configuration, pageToExtract, extractedPage, excludeAuthorInformation, persistFiles).ConfigureAwait(false);

            if (persistFiles && !string.IsNullOrEmpty(extractedPage.ThumbnailUrl))
            {
                await PersistThumbnailAsync(context, template, configuration, extractedPage).ConfigureAwait(false);
            }

            return extractedPage;
        }

        private async Task AddHeaderAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration,
            IPage pageToExtract, BaseClientSidePage extractedPage, bool excludeAuthorInformation, bool persistFiles)
        {
            if (pageToExtract.PageHeader == null || pageToExtract.LayoutType == PageLayoutType.Topic)
            {
                return;
            }

            // The schema has no PageTitleWebPart header type. Falling back to Default rather than
            // changing the XSD keeps every schema version readable; the apply side re-derives the
            // PageTitle case from the presence of a PageTitle web part.
            ClientSidePageHeaderType headerType = pageToExtract.PageHeader.Type != PageHeaderType.PageTitleWebPart
                && Enum.TryParse(pageToExtract.PageHeader.Type.ToString(), out ClientSidePageHeaderType parsedType)
                    ? parsedType
                    : ClientSidePageHeaderType.Default;

            extractedPage.Header = new ClientSidePageHeader
            {
                Type = headerType,
                ServerRelativeImageUrl = await TokenizeJsonControlDataAsync(context, pageToExtract.PageHeader.ImageServerRelativeUrl).ConfigureAwait(false),
                TranslateX = pageToExtract.PageHeader.TranslateX,
                TranslateY = pageToExtract.PageHeader.TranslateY,
                LayoutType = ParseOr(pageToExtract.PageHeader.LayoutType.ToString(), ClientSidePageHeaderLayoutType.FullWidthImage),
                TextAlignment = ParseOr(pageToExtract.PageHeader.TextAlignment.ToString(), ClientSidePageHeaderTextAlignment.Left),
                ShowTopicHeader = pageToExtract.PageHeader.ShowTopicHeader,
                ShowPublishDate = pageToExtract.PageHeader.ShowPublishDate,
                TopicHeader = pageToExtract.PageHeader.TopicHeader,
                AlternativeText = pageToExtract.PageHeader.AlternativeText,
                Authors = !excludeAuthorInformation ? pageToExtract.PageHeader.Authors : "",
                AuthorByLine = !excludeAuthorInformation ? pageToExtract.PageHeader.AuthorByLine : "",
                AuthorByLineId = !excludeAuthorInformation ? pageToExtract.PageHeader.AuthorByLineId : -1,
            };

            if (persistFiles && !string.IsNullOrEmpty(pageToExtract.PageHeader.ImageServerRelativeUrl))
            {
                await IncludePageHeaderImageAsync(context, pageToExtract.PageHeader.ImageServerRelativeUrl, template, configuration).ConfigureAwait(false);
            }
        }

        private async Task PersistThumbnailAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, BaseClientSidePage extractedPage)
        {
            var thumbnailFileIds = new List<Guid>();
            CollectGuids(GuidPatternNoDashes, null, GuidPatternOptionalBrackets, extractedPage.ThumbnailUrl, thumbnailFileIds);

            if (thumbnailFileIds.Count != 1)
            {
                return;
            }

            try
            {
                IFile file = await context.Web.GetFileByIdAsync(thumbnailFileIds[0],
                    f => f.Level, f => f.ServerRelativeUrl, f => f.UniqueId).ConfigureAwait(false);

                (bool added, _) = await LoadAndAddPageImageAsync(context, file, template, configuration).ConfigureAwait(false);

                if (added)
                {
                    string relative = file.ServerRelativeUrl.Substring(context.Web.ServerRelativeUrl.Length).TrimStart('/');
                    extractedPage.ThumbnailUrl = Regex.Replace(extractedPage.ThumbnailUrl,
                        file.UniqueId.ToString("N"), $"{{fileuniqueid:{relative}}}");
                }
            }
            catch (Exception ex)
            {
                // A thumbnail url can contain a GUID that is not a file at all - that is not an error.
                context.Logger?.LogDebug(ex, "{Source}: no file with id {Id}, so the thumbnail was not tokenized.",
                    Constants.LOGGING_SOURCE, thumbnailFileIds[0]);
            }
        }

        #endregion

        #region Sections and controls

        private async Task AddSectionsAsync(PnPContext context, ProvisioningTemplate template, ExtractConfiguration configuration,
            IPage pageToExtract, BaseClientSidePage extractedPage, List<string> errorneousOrNonImageFileGuids, bool persistFiles)
        {
            foreach (ICanvasSection section in pageToExtract.Sections)
            {
                var sectionInstance = new CanvasSectionModel
                {
                    Order = section.Order,
                    BackgroundEmphasis = (Emphasis)section.ZoneEmphasis,
                    Collapsible = section.Collapsible,
                    IsExpanded = section.IsExpanded,
                    ShowDividerLine = section.ShowDividerLine,
                    DisplayName = section.DisplayName,
                    Type = ToTemplateSectionType(section.Type),
                };

                if (section.IconAlignment != null)
                {
                    sectionInstance.IconAlignment = (TemplateIconAlignment)(int)section.IconAlignment.Value;
                }

                if (section.VerticalSectionColumn != null)
                {
                    sectionInstance.VerticalSectionEmphasis = (Emphasis)(section.VerticalSectionColumn.VerticalSectionEmphasis ?? 0);
                }

                foreach (ICanvasColumn column in section.Columns)
                {
                    foreach (ICanvasControl control in column.Controls)
                    {
                        CanvasControlModel controlInstance = await ToControlModelAsync(
                            context, template, configuration, pageToExtract, section, column, control,
                            errorneousOrNonImageFileGuids, persistFiles).ConfigureAwait(false);

                        sectionInstance.Controls.Add(controlInstance);
                    }
                }

                extractedPage.Sections.Add(sectionInstance);
            }
        }

        private async Task<CanvasControlModel> ToControlModelAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, IPage pageToExtract, ICanvasSection section, ICanvasColumn column,
            ICanvasControl control, List<string> errorneousOrNonImageFileGuids, bool persistFiles)
        {
            var controlInstance = new CanvasControlModel
            {
                // A vertical section column is created last on import but can appear anywhere in the
                // parsed HTML on export, so its position is derived rather than read.
                Column = column.IsVerticalSectionColumn ? section.Columns.Count : column.Order,
                ControlId = control.InstanceId,
                Order = control.Order,
            };

            if (control is IPageText textControl)
            {
                controlInstance.Type = WebPartType.Text;
                textControl.BuildControlData(textControl.Order);
                controlInstance.JsonControlData = textControl.JsonControlData;
                controlInstance.ControlProperties = new Dictionary<string, string>(1)
                {
                    { "Text", TokenizeJsonTextData(context, textControl.Text) },
                };

                return controlInstance;
            }

            if (control is IEmptySection emptySection)
            {
                emptySection.BuildControlData(0);
                controlInstance.JsonControlData = emptySection.JsonControlData;
                controlInstance.Type = WebPartType.Custom;

                return controlInstance;
            }

            if (control is IPageWebPart webPart)
            {
                if (Guid.TryParse(webPart.WebPartId, out Guid webPartId))
                {
                    controlInstance.ControlId = webPartId;
                    controlInstance.Type = DefaultWebPartMap.GetWebPartType(
                        pageToExtract.WebPartIdToDefaultWebPart(webPartId.ToString()));
                }
                else if (webPart.ControlType != 14)
                {
                    // Control type 14 is the section background control, which legitimately has no
                    // web part id. Anything else without one is worth reporting.
                    context.Logger?.LogWarning("{Source}: a web part of control type {ControlType} has no valid web part id.",
                        Constants.LOGGING_SOURCE, webPart.ControlType);
                }

                webPart.BuildControlData(webPart.Order);
                string untokenized = MergeSettingsIntoJsonControlData(webPart);

                controlInstance.JsonControlData = await TokenizeJsonControlDataAsync(context, untokenized).ConfigureAwait(false);

                if (persistFiles)
                {
                    await TokenizeBeforeExportAsync(context, template, configuration, errorneousOrNonImageFileGuids,
                        controlInstance, untokenized).ConfigureAwait(false);
                }

                return controlInstance;
            }

            // Unknown control type - carry it as a custom control with no data rather than dropping it.
            controlInstance.Type = WebPartType.Custom;
            controlInstance.JsonControlData = "{}";

            return controlInstance;
        }

        /// <summary>
        /// Extracts a topic page's header controls into a section with a sentinel order.
        /// </summary>
        private async Task AddTopicHeaderControlsAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, IPage pageToExtract, BaseClientSidePage extractedPage,
            List<string> errorneousOrNonImageFileGuids, bool persistFiles)
        {
            var sectionInstance = new CanvasSectionModel
            {
                Order = ObjectHandlers.ObjectClientSidePages.TopicHeaderControlSectionOrder,
                Type = CanvasSectionType.OneColumn,
            };

            foreach (ICanvasControl headerControl in pageToExtract.HeaderControls)
            {
                if (!(headerControl is IPageWebPart webPart))
                {
                    continue;
                }

                var controlInstance = new CanvasControlModel
                {
                    Column = 1,
                    Order = headerControl.Order,
                    ControlId = Guid.TryParse(webPart.WebPartId, out Guid webPartId) ? webPartId : headerControl.InstanceId,
                    Type = WebPartType.Custom,
                };

                var json = new JsonObject
                {
                    ["id"] = webPart.WebPartId,
                    ["instanceId"] = webPart.InstanceId.ToString(),
                    ["title"] = webPart.Title,
                    ["description"] = webPart.Description,
                    ["dataVersion"] = webPart.DataVersion,
                };

                AddRawJson(json, "properties", webPart.PropertiesJson);

                // Some controls will not render without their serverProcessedContent, so it travels
                // with the template even though nothing in the engine reads it.
                if (HasValue(webPart.ServerProcessedContent))
                {
                    AddRawJson(json, "serverProcessedContent", webPart.ServerProcessedContent.ToString());
                }

                string untokenized = json.ToJsonString();
                controlInstance.JsonControlData = await TokenizeJsonControlDataAsync(context, untokenized).ConfigureAwait(false);

                if (persistFiles)
                {
                    await TokenizeBeforeExportAsync(context, template, configuration, errorneousOrNonImageFileGuids,
                        controlInstance, untokenized).ConfigureAwait(false);
                }

                sectionInstance.Controls.Add(controlInstance);
            }

            extractedPage.Sections.Add(sectionInstance);
        }

        private static void AddToTemplate(PnPContext context, ProvisioningTemplate template, PageToExport page, BaseClientSidePage extractedPage)
        {
            if (page.IsTranslation)
            {
                ClientSidePageModel parentPage = template.ClientSidePages
                    .FirstOrDefault(p => p.PageName == page.SourcePageName);

                if (parentPage != null)
                {
                    var translated = (TranslatedClientSidePage)extractedPage;
                    translated.LCID = new CultureInfo(page.Language).LCID;
                    parentPage.Translations.Add(translated);
                }

                return;
            }

            var clientSidePage = (ClientSidePageModel)extractedPage;
            if (page.TranslatedLanguages != null && page.TranslatedLanguages.Count > 0)
            {
                clientSidePage.CreateTranslations = true;
                clientSidePage.LCID = (int)context.Web.Language;
            }

            template.ClientSidePages.Add(clientSidePage);

            if (!page.IsHomePage)
            {
                return;
            }

            template.WebSettings ??= new Model.WebSettings();
            template.WebSettings.WelcomePage = page.PageUrl.StartsWith(context.Web.ServerRelativeUrl, StringComparison.InvariantCultureIgnoreCase)
                ? page.PageUrl.Replace(context.Web.ServerRelativeUrl.TrimEnd('/') + "/", "")
                : page.PageUrl;
        }

        #endregion

        #region Tokenizing

        /// <summary>
        /// Rebuilds a web part's stored JSON with everything the template needs to re-create it.
        /// </summary>
        private static string MergeSettingsIntoJsonControlData(IPageWebPart webPart)
        {
            if (string.IsNullOrWhiteSpace(webPart.JsonControlData))
            {
                return null;
            }

            if (!(JsonNode.Parse(webPart.JsonControlData) is JsonObject json))
            {
                return webPart.JsonControlData;
            }

            AddRawJson(json, "properties", webPart.PropertiesJson);

            if (HasValue(webPart.ServerProcessedContent))
            {
                AddRawJson(json, "serverProcessedContent", webPart.ServerProcessedContent.ToString());
            }

            if (HasValue(webPart.DynamicDataPaths))
            {
                AddRawJson(json, "dynamicDataPaths", webPart.DynamicDataPaths.ToString());
            }

            if (HasValue(webPart.DynamicDataValues))
            {
                AddRawJson(json, "dynamicDataValues", webPart.DynamicDataValues.ToString());
            }

            if (!string.IsNullOrEmpty(webPart.DataVersion))
            {
                json["dataVersion"] = webPart.DataVersion;
            }

            return json.ToJsonString();
        }

        /// <summary>
        /// Finds the files a control refers to, copies them into the template, and replaces their
        /// ids with tokens.
        /// </summary>
        private async Task TokenizeBeforeExportAsync(PnPContext context, ProvisioningTemplate template,
            ExtractConfiguration configuration, List<string> errorneousOrNonImageFileGuids,
            CanvasControlModel controlInstance, string untokenizedJsonControlData)
        {
            if (string.IsNullOrEmpty(untokenizedJsonControlData))
            {
                return;
            }

            var fileGuids = new List<Guid>();
            var exportedFiles = new Dictionary<string, string>();
            var exportedPages = new Dictionary<string, string>();

            await CollectSiteAssetImageFilesAsync(context, untokenizedJsonControlData, fileGuids).ConfigureAwait(false);
            CollectGuids(GuidPattern, GuidPatternEncoded, GuidPatternOptionalBrackets, untokenizedJsonControlData, fileGuids);

            foreach (Guid uniqueId in fileGuids)
            {
                if (exportedFiles.ContainsKey(uniqueId.ToString()) || errorneousOrNonImageFileGuids.Contains(uniqueId.ToString()))
                {
                    continue;
                }

                try
                {
                    IFile file = await context.Web.GetFileByIdAsync(uniqueId,
                        f => f.Level, f => f.ServerRelativeUrl).ConfigureAwait(false);

                    (bool added, string fileName) = await LoadAndAddPageImageAsync(context, file, template, configuration).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    string relative = file.ServerRelativeUrl.Substring(context.Web.ServerRelativeUrl.Length).TrimStart('/');

                    if (fileName.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        exportedPages[uniqueId.ToString()] = relative;
                    }
                    else if (added)
                    {
                        exportedFiles[uniqueId.ToString()] = relative;
                    }
                }
                catch (Exception ex)
                {
                    // Most GUIDs in control data are not files at all. Remember the ones that are
                    // not so the next control does not pay for the same lookup.
                    context.Logger?.LogDebug(ex, "{Source}: {Id} is not an exportable file.", Constants.LOGGING_SOURCE, uniqueId);
                    errorneousOrNonImageFileGuids.Add(uniqueId.ToString());
                }
            }

            foreach (KeyValuePair<string, string> exportedFile in exportedFiles)
            {
                controlInstance.JsonControlData = Regex.Replace(controlInstance.JsonControlData,
                    exportedFile.Key.Replace("-", "%2D"), $"{{fileuniqueidencoded:{exportedFile.Value}}}", RegexOptions.IgnoreCase);
                controlInstance.JsonControlData = Regex.Replace(controlInstance.JsonControlData,
                    exportedFile.Key, $"{{fileuniqueid:{exportedFile.Value}}}", RegexOptions.IgnoreCase);
            }

            foreach (KeyValuePair<string, string> exportedPage in exportedPages)
            {
                controlInstance.JsonControlData = Regex.Replace(controlInstance.JsonControlData,
                    exportedPage.Key.Replace("-", "%2D"), $"{{pageuniqueidencoded:{exportedPage.Value}}}", RegexOptions.IgnoreCase);
                controlInstance.JsonControlData = Regex.Replace(controlInstance.JsonControlData,
                    exportedPage.Key, $"{{pageuniqueid:{exportedPage.Value}}}", RegexOptions.IgnoreCase);
                controlInstance.JsonControlData = Regex.Replace(controlInstance.JsonControlData,
                    exportedPage.Key.Replace("-", ""), $"{{pageuniqueid:{exportedPage.Value}}}", RegexOptions.IgnoreCase);
            }
        }

        private static void CollectGuids(Regex plain, Regex encoded, Regex optionalBrackets, string json, List<Guid> fileGuids)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            void Collect(Regex pattern, Func<string, string> clean)
            {
                if (pattern == null)
                {
                    return;
                }

                foreach (Match match in pattern.Matches(json))
                {
                    if (Guid.TryParse(clean(match.Value), out Guid uniqueId) && !fileGuids.Contains(uniqueId))
                    {
                        fileGuids.Add(uniqueId);
                    }
                }
            }

            Collect(plain, value => value.Trim('"'));
            Collect(encoded, value => value.TrimStart('='));
            Collect(optionalBrackets, value => value);
        }

        /// <summary>
        /// Resolves urls pointing into <c>SiteAssets/SitePages</c> to their file ids.
        /// </summary>
        private static async Task CollectSiteAssetImageFilesAsync(PnPContext context, string untokenizedJsonControlData, List<Guid> fileGuids)
        {
            foreach (Match match in SiteAssetUrls.Matches(untokenizedJsonControlData))
            {
                string url = match.Groups["AssetUrl"]?.Value;
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                if (url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    var webUri = new Uri(context.Web.Url.ToString());
                    string hostUrl = $"{webUri.Scheme}://{webUri.Authority}";

                    if (!url.StartsWith(hostUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        // Another tenant or a CDN - nothing to export.
                        continue;
                    }

                    url = url.Substring(hostUrl.Length);
                }

                try
                {
                    IFile file = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(url, f => f.UniqueId).ConfigureAwait(false);
                    if (file != null && !fileGuids.Contains(file.UniqueId))
                    {
                        fileGuids.Add(file.UniqueId);
                    }
                }
                catch (Exception)
                {
                    // The url may point outside this web; that is not an error worth reporting.
                }
            }
        }

        /// <summary>
        /// Replaces every site-specific id and url in a control's JSON with a token.
        /// </summary>
        private async Task<string> TokenizeJsonControlDataAsync(PnPContext context, string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return json;
            }

            await context.Web.LoadAsync(w => w.Id, w => w.Url, w => w.ServerRelativeUrl,
                w => w.Lists.QueryProperties(l => l.Id, l => l.Title, l => l.Views.QueryProperties(v => v.Id, v => v.Title)))
                .ConfigureAwait(false);
            await context.Site.LoadAsync(s => s.Id, s => s.GroupId).ConfigureAwait(false);

            foreach (IList list in context.Web.Lists.AsRequested())
            {
                json = Regex.Replace(json, list.Id.ToString(),
                    $"{{listid:{System.Security.SecurityElement.Escape(list.Title)}}}", RegexOptions.IgnoreCase);

                foreach (IView view in list.Views.AsRequested())
                {
                    json = Regex.Replace(json, view.Id.ToString(),
                        $"{{viewid:{System.Security.SecurityElement.Escape(list.Title)},{System.Security.SecurityElement.Escape(view.Title)}}}",
                        RegexOptions.IgnoreCase);
                }
            }

            var uri = new Uri(context.Web.Url.ToString());
            bool isRootWeb = context.Web.ServerRelativeUrl == "/";

            string hostReplacement = isRootWeb ? $"{uri.Scheme}://{{fqdn}}{{site}}" : $"{uri.Scheme}://{{fqdn}}";
            json = Regex.Replace(json, $"{uri.Scheme}://{uri.DnsSafeHost}:{uri.Port}", hostReplacement, RegexOptions.IgnoreCase);
            json = Regex.Replace(json, $"{uri.Scheme}://{uri.DnsSafeHost}", hostReplacement, RegexOptions.IgnoreCase);
            json = Regex.Replace(json, uri.DnsSafeHost, "{fqdn}", RegexOptions.IgnoreCase);

            json = Regex.Replace(json, context.Site.Id.ToString(), "{sitecollectionid}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, context.Site.Id.ToString().Replace("-", "%2D"), "{sitecollectionidencoded}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, context.Site.Id.ToString("N"), "{sitecollectionid}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, context.Web.Id.ToString(), "{siteid}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, context.Web.Id.ToString().Replace("-", "%2D"), "{siteidencoded}", RegexOptions.IgnoreCase);
            json = Regex.Replace(json, context.Web.Id.ToString("N"), "{siteid}", RegexOptions.IgnoreCase);

            string serverRelativeUrl = context.Web.ServerRelativeUrl;
            string siteToken = isRootWeb ? "{site}/" : "{site}";

            json = Regex.Replace(json, "(\"" + serverRelativeUrl + ")(?!&)", "\"" + siteToken, RegexOptions.IgnoreCase);
            json = Regex.Replace(json, "'" + serverRelativeUrl, "'" + siteToken, RegexOptions.IgnoreCase);
            json = Regex.Replace(json, ">" + serverRelativeUrl, ">" + siteToken, RegexOptions.IgnoreCase);

            if (!isRootWeb)
            {
                json = Regex.Replace(json, serverRelativeUrl, "{site}", RegexOptions.IgnoreCase);
            }

            if (context.Site.GroupId != Guid.Empty)
            {
                json = Regex.Replace(json, context.Site.GroupId.ToString(),
                    "{sitecollectionconnectedoffice365groupid}", RegexOptions.IgnoreCase);
            }

            return json;
        }

        /// <summary>
        /// Tokenizes a text control's HTML.
        /// </summary>
        private static string TokenizeJsonTextData(PnPContext context, string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            // Guarded rather than assumed loaded. This is deep inside the extract and the property is
            // only read here, so on a context whose first operation is a page extract it has never
            // been loaded and the getter throws - taking the whole extract with it. The per-handler
            // tests never saw it because they run against a context that has done other work first;
            // phase 9's scenario 2, which extracts through a context of its own, found it at once.
            if (!context.Web.IsPropertyAvailable(w => w.ServerRelativeUrl)
                || string.IsNullOrEmpty(context.Web.ServerRelativeUrl))
            {
                return html;
            }

            return Regex.Replace(html, "href=\"" + context.Web.ServerRelativeUrl, "href=\"{site}", RegexOptions.IgnoreCase);
        }

        #endregion

        #region Files

        private async Task IncludePageHeaderImageAsync(PnPContext context, string imageServerRelativeUrl,
            ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            if (imageServerRelativeUrl.StartsWith("/_LAYOUTS", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                IFile pageHeaderImage = await context.Web.GetFileByServerRelativeUrlOrDefaultAsync(
                    imageServerRelativeUrl, f => f.Level, f => f.ServerRelativeUrl).ConfigureAwait(false);

                if (pageHeaderImage != null)
                {
                    await LoadAndAddPageImageAsync(context, pageHeaderImage, template, configuration).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // Header images regularly point outside the site - other site collections, _layouts,
                // a CDN, the open internet. None of those are exportable and none are errors.
            }
        }

        /// <summary>
        /// Records an image in the template and copies its bytes into the connector.
        /// </summary>
        /// <returns>Whether the file was added, and its name when it was found at all</returns>
        private async Task<(bool Added, string FileName)> LoadAndAddPageImageAsync(PnPContext context, IFile pageImage,
            ProvisioningTemplate template, ExtractConfiguration configuration)
        {
            var fullUri = new Uri(new Uri(context.Web.Url.ToString()), pageImage.ServerRelativeUrl);
            string folderPath = Uri.UnescapeDataString(
                fullUri.Segments.Take(fullUri.Segments.Length - 1).Aggregate((i, x) => i + x).TrimEnd('/'));
            string fileName = Uri.UnescapeDataString(fullUri.Segments[fullUri.Segments.Length - 1]);

            // Pages are reported back to the caller so it can tokenize them, but never exported as
            // files - a page is provisioned by ObjectClientSidePages, not copied as bytes.
            if (fileName.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                return (false, fileName);
            }

            string templateFolderPath = folderPath.Substring(context.Web.ServerRelativeUrl.Length).TrimStart('/');

            bool alreadyExported = template.Files.Any(f =>
                f.Folder.Equals(templateFolderPath, StringComparison.CurrentCultureIgnoreCase)
                && f.Src.Equals(fileName, StringComparison.CurrentCultureIgnoreCase));

            if (alreadyExported)
            {
                return (false, fileName);
            }

            template.Files.Add(new TemplateFile
            {
                Folder = templateFolderPath,
                Src = $"{templateFolderPath}/{fileName}",
                Overwrite = true,
                Level = Enum.TryParse(pageImage.Level.ToString(), out FileLevelModel level) ? level : FileLevelModel.Published,
            });

            await PersistFileAsync(context, configuration, pageImage, templateFolderPath, fileName).ConfigureAwait(false);

            return (true, fileName);
        }

        private static async Task PersistFileAsync(PnPContext context, ExtractConfiguration configuration,
            IFile file, string container, string fileName)
        {
            if (configuration?.FileConnector == null)
            {
                context.Logger?.LogError("{Source}: no connector is configured, so '{File}' was recorded in the template but not exported.",
                    Constants.LOGGING_SOURCE, fileName);
                return;
            }

            string connectorContainer = container.Trim('/').Replace("/", "\\");

            if (!string.IsNullOrEmpty(configuration.FileConnector.GetContainer()))
            {
                connectorContainer = System.IO.Path.Combine(configuration.FileConnector.GetContainer(), connectorContainer);
            }

            using (Stream content = await file.GetContentAsync(true).ConfigureAwait(false))
            {
                using (var buffer = new MemoryStream())
                {
                    await content.CopyToAsync(buffer).ConfigureAwait(false);
                    buffer.Position = 0;

                    configuration.FileConnector.SaveFileStream(fileName, connectorContainer, buffer);
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Strips a content type id back to its parent.
        /// </summary>
        private static string GetParentIdValue(string contentTypeId)
        {
            int length = 0;
            string value = contentTypeId.Substring(2); // drop the leading 0x

            for (int i = 0; i < value.Length; i += 2)
            {
                length = i;
                if (value.Substring(i, 2).Equals("00", StringComparison.OrdinalIgnoreCase))
                {
                    i += 32;
                }
            }

            return length > 0 ? "0x" + value.Substring(0, length) : string.Empty;
        }

        private static CanvasSectionType ToTemplateSectionType(CanvasSectionTemplate sectionTemplate)
        {
            switch (sectionTemplate)
            {
                case CanvasSectionTemplate.TwoColumn:
                    return CanvasSectionType.TwoColumn;
                case CanvasSectionTemplate.TwoColumnLeft:
                    return CanvasSectionType.TwoColumnLeft;
                case CanvasSectionTemplate.TwoColumnRight:
                    return CanvasSectionType.TwoColumnRight;
                case CanvasSectionTemplate.ThreeColumn:
                    return CanvasSectionType.ThreeColumn;
                case CanvasSectionTemplate.OneColumnFullWidth:
                    return CanvasSectionType.OneColumnFullWidth;
                case CanvasSectionTemplate.OneColumnVerticalSection:
                    return CanvasSectionType.OneColumnVerticalSection;
                case CanvasSectionTemplate.TwoColumnVerticalSection:
                    return CanvasSectionType.TwoColumnVerticalSection;
                case CanvasSectionTemplate.TwoColumnLeftVerticalSection:
                    return CanvasSectionType.TwoColumnLeftVerticalSection;
                case CanvasSectionTemplate.TwoColumnRightVerticalSection:
                    return CanvasSectionType.TwoColumnRightVerticalSection;
                case CanvasSectionTemplate.ThreeColumnVerticalSection:
                    return CanvasSectionType.ThreeColumnVerticalSection;

                // The schema has no flexible layout section. Falling back to the nearest column
                // layout is lossy on paper, but the section factor stored in each control's
                // JsonControlData is what actually restores the flexible layout on apply.
                case CanvasSectionTemplate.FlexibleLayoutSection:
                    return CanvasSectionType.OneColumn;
                case CanvasSectionTemplate.FlexibleLayoutVerticalSection:
                    return CanvasSectionType.OneColumnVerticalSection;

                default:
                    return CanvasSectionType.OneColumn;
            }
        }

        private static T ParseOr<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, out T parsed) ? parsed : fallback;
        }

        private static bool HasValue(JsonElement element)
        {
            return element.ValueKind != JsonValueKind.Undefined && element.ValueKind != JsonValueKind.Null;
        }

        /// <summary>
        /// Adds a raw JSON fragment as a property, or the string itself when it is not valid JSON.
        /// </summary>
        private static void AddRawJson(JsonObject json, string name, string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return;
            }

            try
            {
                json[name] = JsonNode.Parse(rawJson);
            }
            catch (JsonException)
            {
                json[name] = rawJson;
            }
        }

        #endregion
    }
}
