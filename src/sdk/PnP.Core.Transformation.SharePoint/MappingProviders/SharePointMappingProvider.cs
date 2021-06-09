using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.Extensions;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using PnP.Core.Transformation.SharePoint.Model;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class SharePointMappingProvider : IMappingProvider
    {
        private ILogger<SharePointMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointMappingProvider(ILogger<SharePointMappingProvider> logger, IOptions<SharePointTransformationOptions> options, IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Maps an item from the source platform to the target platform
        /// </summary>
        /// <param name="input">The input for the mapping</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping</returns>
        public async Task<MappingProviderOutput> MapAsync(MappingProviderInput input, CancellationToken token = default)
        {
            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapAsync");

            // Validate input
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Validate source item
            var sourceItem = input.Context.SourceItem as SharePointSourceItem;
            if (sourceItem == null) throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);

            // Get a reference to the file and item that we need to transform
            var sourceContext = sourceItem.SourceContext;
            var pageFile = sourceItem.SourceContext.Web.GetFileByServerRelativeUrl(sourceItem.Id.ServerRelativeUrl);
            var pageItem = pageFile.ListItemAllFields;
            sourceContext.Load(pageFile);
            sourceContext.Load(pageItem);
            await sourceContext.ExecuteQueryAsync().ConfigureAwait(false);

            // Check if the target site is the same as the source site
            var crossSiteTransformation = IsCrossSiteTransformation(input, sourceItem);
            var crossTenantTransformation = IsCrossTenantTransformation(input, sourceItem);

            // Read source page information
            var sourcePage = ReadSourcePageInformation(pageItem);

            // Determine if the page is a Root Page
            sourcePage.IsRootPage = pageFile != null;

            // Evaluate the source page type
            sourcePage.PageType = EvaluatePageType(pageFile, pageItem, crossSiteTransformation, crossTenantTransformation);

            logger.LogInformation(SharePointTransformationResources.Info_ValidationChecksComplete);

            // Extract the list of Web Parts to process and the reference page layout
            var (layout, webPartsToProcess) = await AnalyzePageAsync(pageFile).ConfigureAwait(false);

            // Start the actual transformation
            var transformationStartDateTime = DateTime.Now;

            // Load information from source context
            LoadClientObjects(sourceContext);

            // TODO: Let's see if we really need to load tenant global properties or not
            // see line 299 of PageTransformator.cs in PnP Framework

            // Retrieve Version and ExactVersion of source and target
            (sourcePage.SourceVersion, sourcePage.SourceVersionNumber) = sourceContext.GetVersions();

            // Retrieve the parent Folder of the page, if any and the target page name
            string targetPageName = null;
            (sourcePage.Folder, targetPageName) = DeterminePageFolder(sourceContext, sourcePage, pageItem, input.Context.TargetPageUri);

            // Transform the web parts
            var webPartMappingProvider = serviceProvider.GetService<IWebPartMappingProvider>();
            if (webPartMappingProvider != null)
            {
                // TODO: prepare webpart
                foreach (var webPart in webPartsToProcess)
                {
                    var webPartInput = new WebPartMappingProviderInput(input.Context);
                    var output = await webPartMappingProvider
                        .MapWebPartAsync(webPartInput, token)
                        .ConfigureAwait(false);
                }
            }

            var htmlMappingProvider = serviceProvider.GetService<IHtmlMappingProvider>();
            if (htmlMappingProvider != null)
            {
                // TODO: get the html content
                var htmlInput = new HtmlMappingProviderInput(input.Context, "TODO");
                var output = await htmlMappingProvider
                    .MapHtmlAsync(htmlInput, token)
                    .ConfigureAwait(false);
            }

            var metadataMappingProvider = serviceProvider.GetService<IMetadataMappingProvider>();
            if (metadataMappingProvider != null)
            {
                // TODO: prepare input
                var metadataInput = new MetadataMappingProviderInput(input.Context);
                var output = await metadataMappingProvider
                    .MapMetadataFieldAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var urlMappingProvider = serviceProvider.GetService<IUrlMappingProvider>();
            if (urlMappingProvider != null)
            {
                // TODO: prepare uri
                var metadataInput = new UrlMappingProviderInput(input.Context, new Uri("http://dummy"));
                var output = await urlMappingProvider
                    .MapUrlAsync(metadataInput, token)
                    .ConfigureAwait(false);
            }

            var pageLayoutMappingProvider = serviceProvider.GetService<IPageLayoutMappingProvider>();
            if (pageLayoutMappingProvider != null)
            {
                // TODO: prepare page layout
                var pageLayoutInput = new PageLayoutMappingProviderInput(input.Context);
                var output = await pageLayoutMappingProvider
                    .MapPageLayoutAsync(pageLayoutInput, token)
                    .ConfigureAwait(false);
            }

            var taxonomyMappingProvider = serviceProvider.GetService<ITaxonomyMappingProvider>();
            if (taxonomyMappingProvider != null)
            {
                // TODO: prepare term id
                var taxonomyInput = new TaxonomyMappingProviderInput(input.Context, "");
                var output = await taxonomyMappingProvider
                    .MapTermAsync(taxonomyInput, token)
                    .ConfigureAwait(false);
            }

            var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();
            if (userMappingProvider != null)
            {
                // TODO: prepare user
                var userInput = new UserMappingProviderInput(input.Context, "");
                var output = await userMappingProvider
                    .MapUserAsync(userInput, token)
                    .ConfigureAwait(false);
            }

            return new MappingProviderOutput();
        }

        private static (string, string) DeterminePageFolder(ClientContext sourceContext, SourcePageInformation sourcePage, ListItem pageItem, Uri targetPageUri)
        {
            string pageFolder = null;
            string targetPageName = targetPageUri.Segments[targetPageUri.Segments.Length - 1];

            string fileRefFieldValue;
            if (pageItem.TryGetFieldValue(SharePointConstants.FileDirRefField, out fileRefFieldValue))
            {
                if (sourcePage.IsRootPage)
                {
                    pageFolder = "Root/";
                    if (string.IsNullOrEmpty(targetPageName))
                    {
                        targetPageName = fileRefFieldValue;
                    }
                }
                else if (sourcePage.PageType == SourcePageType.BlogPage)
                {
                    var blogsListName = sourceContext.Web.GetLocalizedListName(ListType.Blogs, "posts");
                    if (fileRefFieldValue.ContainsIgnoringCasing($"/lists/{blogsListName}"))
                    {
                        pageFolder = fileRefFieldValue.Replace($"{sourceContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/Lists/{blogsListName}", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                    }
                    else
                    {
                        // Page was living in another list, leave the list name as that will be the folder hosting the modern file in SitePages.
                        // This convention is used to avoid naming conflicts
                        pageFolder = fileRefFieldValue.Replace($"{sourceContext.Web.ServerRelativeUrl}", "").Trim();
                    }
                }
                else
                {
                    if (fileRefFieldValue.ContainsIgnoringCasing("/sitepages"))
                    {
                        pageFolder = fileRefFieldValue.Replace($"{sourceContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/SitePages", "").Trim();
                    }
                    else
                    {
                        // Page was living in another list, leave the list name as that will be the folder hosting the modern file in SitePages.
                        // This convention is used to avoid naming conflicts
                        pageFolder = fileRefFieldValue.Replace($"{sourceContext.Web.ServerRelativeUrl}", "").Trim();
                    }
                }

                // Ensure that the page Folder does not start with "/" and does end with "/"
                pageFolder = pageFolder.TrimStart('/').TrimEnd('/') + "/";
            }

            return (pageFolder, targetPageName);
        }

        private static bool IsCrossSiteTransformation(MappingProviderInput input, SharePointSourceItem sourceItem)
        {
            var sourceUrl = sourceItem.SourceContext.Url;
            var targetUrl = input.Context.TargetPageUri.AbsoluteUri;

            return !targetUrl.StartsWith(sourceUrl, StringComparison.InvariantCulture);
        }

        private static bool IsCrossTenantTransformation(MappingProviderInput input, SharePointSourceItem sourceItem)
        {
            var sourceUrl = sourceItem.SourceContext.Url;
            var targetUrl = input.Context.TargetPageUri.AbsoluteUri;

            var sourceTenant = new Uri(sourceUrl).Host;
            var targetTenant = new Uri(targetUrl).Host;

            return sourceTenant != targetTenant;
        }

        private SourcePageType EvaluatePageType(File pageFile, ListItem pageItem, bool crossSiteTransformation, bool crossTenantTransformation)
        {
            SourcePageType result;

            if (pageFile != null && pageItem == null)
            {
                logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_PageLivesOutsideOfALibrary, pageFile.ServerRelativeUrl));

                // This is always a web part page
                result = SourcePageType.WebPartPage;

                // Item level permission copy makes no sense here
                this.options.Value.KeepPageSpecificPermissions = false;

                // Same for swap pages, we don't support this as the pages live in a different location
                this.options.Value.TargetPageTakesSourcePageName = false;
            }
            else
            {
                // Validate page item existence
                if (pageItem == null)
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_SourcePageNotFound, pageFile.ServerRelativeUrl);
                    logger.LogInformation(errorMessage);
                    throw new ArgumentNullException(errorMessage);
                }

                // Validate page and it's eligibility for transformation
                if (!pageItem.FieldExistsAndUsed(SharePointConstants.FileRefField) ||
                    !pageItem.FieldExistsAndUsed(SharePointConstants.FileLeafRefField))
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_PageNotValidMissingFileRef, pageFile.ServerRelativeUrl);
                    logger.LogInformation(errorMessage);
                    throw new ArgumentNullException(errorMessage);
                }

                // Store the page type
                result = pageItem.PageType();

                logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_TransformationMode,
                    pageFile.ServerRelativeUrl,
                    result.FormatAsFriendlyTitle()));

                ValidatePageType(pageFile, result, crossSiteTransformation, crossTenantTransformation);
            }

            return result;
        }

        private void ValidatePageType(File pageFile, SourcePageType pageType, bool crossSiteTransformation, bool crossTenantTransformation)
        {
            string pageTypeExceptionMessage = null;

            if (pageType == SourcePageType.ClientSidePage)
            {
                pageTypeExceptionMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_SourcePageIsModern,
                    pageFile.ServerRelativeUrl);
            }
            else if (pageType == SourcePageType.AspxPage)
            {
                pageTypeExceptionMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_BasicASPXPageCannotTransform,
                    pageFile.ServerRelativeUrl);
            }
            else if (pageType == SourcePageType.BlogPage && !crossSiteTransformation)
            {
                pageTypeExceptionMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_BlogPageTransformationHasToBeCrossSite,
                    pageFile.ServerRelativeUrl);
            }

            if (!string.IsNullOrEmpty(pageTypeExceptionMessage))
            {
                logger.LogError(pageTypeExceptionMessage);
                throw new ArgumentNullException(pageTypeExceptionMessage);
            }
        }

        private async Task<Tuple<PageLayout, List<WebPartEntity>>> AnalyzePageAsync(File pageFile)
        {
            // Prepare the result variable
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            // Get a reference to the current source context
            var context = pageFile.Context;

            // Load web parts on web part page
            // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. There's no API that will return those,
            //       only possible option to add parsing of the raw page aspx file.
            var limitedWPManager = pageFile.GetLimitedWebPartManager(PersonalizationScope.Shared);
            context.Load(limitedWPManager);

            // Load page properties
            var pageProperties = pageFile.Properties;
            context.Load(pageProperties);

            // Load the web parts properties
            IEnumerable<WebPartDefinition> webParts = context.LoadQuery(
                limitedWPManager.WebParts.IncludeWithDefaultProperties(
                    wp => wp.Id, 
                    wp => wp.ZoneId, 
                    wp => wp.WebPart.ExportMode, 
                    wp => wp.WebPart.Title, 
                    wp => wp.WebPart.ZoneIndex, 
                    wp => wp.WebPart.IsClosed, 
                    wp => wp.WebPart.Hidden,
                    wp => wp.WebPart.Properties));
            await context.ExecuteQueryAsync().ConfigureAwait(false);

            // Check page type
            var layout = GetLayout(pageProperties);

            if (webParts.Any())
            {
                List<WebPartPlaceHolder> webPartsToRetrieve = new List<WebPartPlaceHolder>();

                foreach (var foundWebPart in webParts)
                {
                    webPartsToRetrieve.Add(new WebPartPlaceHolder()
                    {
                        WebPartDefinition = foundWebPart,
                        WebPartXml = null,
                        WebPartType = "",
                    });
                }

                bool isDirty = false;
                foreach (var foundWebPart in webPartsToRetrieve)
                {
                    // Skip Microsoft.SharePoint.WebPartPages.TitleBarWebPart webpart in TitleBar zone
                    if (foundWebPart.WebPartDefinition.ZoneId.Equals("TitleBar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!options.Value.IncludeTitleBarWebPart)
                        {
                            continue;
                        }
                    }

                    if (foundWebPart.WebPartDefinition.WebPart.ExportMode == WebPartExportMode.All)
                    {
                        foundWebPart.WebPartXml = limitedWPManager.ExportWebPart(foundWebPart.WebPartDefinition.Id);
                        isDirty = true;
                    }
                }
                if (isDirty)
                {
                    await context.ExecuteQueryAsync().ConfigureAwait(false);
                }

                // TODO: Ask Bert -> isn't this a repetition?
                foreach (var foundWebPart in webPartsToRetrieve)
                {
                    // Skip Microsoft.SharePoint.WebPartPages.TitleBarWebPart webpart in TitleBar zone
                    if (foundWebPart.WebPartDefinition.ZoneId.Equals("TitleBar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!options.Value.IncludeTitleBarWebPart)
                        {
                            continue;
                        }
                    }

                    if (foundWebPart.WebPartDefinition.WebPart.ExportMode != WebPartExportMode.All)
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        foundWebPart.WebPartType = GetTypeFromProperties(foundWebPart.WebPartDefinition.WebPart.Properties.FieldValues);
                    }
                    else
                    {
                        foundWebPart.WebPartType = GetType(foundWebPart.WebPartXml.Value);
                    }

                    logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Info_ContentTransformFoundSourceWebParts,
                        pageFile.ServerRelativeUrl,
                        foundWebPart.WebPartDefinition.WebPart.Title, 
                        foundWebPart.WebPartType.GetTypeShort()));

                    webparts.Add(new WebPartEntity()
                    {
                        Title = foundWebPart.WebPartDefinition.WebPart.Title,
                        Type = foundWebPart.WebPartType,
                        Id = foundWebPart.WebPartDefinition.Id,
                        ServerControlId = foundWebPart.WebPartDefinition.Id.ToString(),
                        Row = GetRow(foundWebPart.WebPartDefinition.ZoneId, layout),
                        Column = GetColumn(foundWebPart.WebPartDefinition.ZoneId, layout),
                        Order = foundWebPart.WebPartDefinition.WebPart.ZoneIndex,
                        ZoneId = foundWebPart.WebPartDefinition.ZoneId,
                        ZoneIndex = (uint)foundWebPart.WebPartDefinition.WebPart.ZoneIndex,
                        IsClosed = foundWebPart.WebPartDefinition.WebPart.IsClosed,
                        Hidden = foundWebPart.WebPartDefinition.WebPart.Hidden,
                        WebPartXml = foundWebPart.WebPartXml == null ? "" : foundWebPart.WebPartXml.Value,
                        //Properties = ExtractProperties(foundWebPart.WebPartDefinition.WebPart.Properties.FieldValues, 
                        //    foundWebPart.WebPartType, foundWebPart.WebPartXml == null ? "" : foundWebPart.WebPartXml.Value),
                    });
                }
            }
            else
            {
                logger.LogInformation(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_NoWebPartsFound, 
                    pageFile.ServerRelativeUrl));
            }


            return new Tuple<PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        /// <summary>
        /// Determines the used web part page layout
        /// </summary>
        /// <param name="pageProperties">Properties of the web part page file</param>
        /// <returns>Used layout</returns>
        private static PageLayout GetLayout(PropertyValues pageProperties)
        {
            if (pageProperties.FieldValues.ContainsKey("vti_setuppath"))
            {
                var setupPath = pageProperties["vti_setuppath"].ToString();
                if (!string.IsNullOrEmpty(setupPath))
                {
                    if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd1.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_FullPageVertical;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd2.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_HeaderFooterThreeColumns;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd3.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_HeaderLeftColumnBody;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd4.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_HeaderRightColumnBody;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd5.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_HeaderFooter2Columns4Rows;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd6.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_HeaderFooter4ColumnsTopRow;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd7.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns;
                    }
                    else if (setupPath.IndexOf(@"\STS\doctemp\smartpgs\spstd8.aspx", StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
                        return PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns;
                    }
                    else if (setupPath.Equals(@"SiteTemplates\STS\default.aspx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return PageLayout.WebPart_2010_TwoColumnsLeft;
                    }
                }
            }

            return PageLayout.WebPart_Custom;
        }

        /// <summary>
        /// Gets the type of the web part by detecting it from the available properties
        /// </summary>
        /// <param name="properties">Web part properties to analyze</param>
        /// <param name="isLegacy">If true tries additional webpart types used in legacy versions</param>
        /// <returns>Type of the web part as fully qualified name</returns>
        private static string GetTypeFromProperties(Dictionary<string, object> properties, bool isLegacy = false)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            // Check for XSLTListView web part
            string[] xsltWebPart = new string[] { "ListUrl", "ListId", "Xsl", "JSLink", "ShowTimelineIfAvailable" };
            if (CheckWebPartProperties(xsltWebPart, properties))
            {
                return WebParts.XsltListView;
            }

            // Check for ListView web part
            string[] listWebPart = new string[] { "ListViewXml", "ListName", "ListId", "ViewContentTypeId", "PageType" };
            if (CheckWebPartProperties(listWebPart, properties))
            {
                return WebParts.ListView;
            }

            // Check for Media web part
            string[] mediaWebPart = new string[] { "AutoPlay", "MediaSource", "Loop", "IsPreviewImageSourceOverridenForVideoSet", "PreviewImageSource" };
            if (CheckWebPartProperties(mediaWebPart, properties))
            {
                return WebParts.Media;
            }

            // Check for SlideShow web part
            string[] slideShowWebPart = new string[] { "LibraryGuid", "Layout", "Speed", "ShowToolbar", "ViewGuid" };
            if (CheckWebPartProperties(slideShowWebPart, properties))
            {
                return WebParts.PictureLibrarySlideshow;
            }

            // Check for Chart web part
            string[] chartWebPart = new string[] { "ConnectionPointEnabled", "ChartXml", "DataBindingsString", "DesignerChartTheme" };
            if (CheckWebPartProperties(chartWebPart, properties))
            {
                return WebParts.Chart;
            }

            // Check for Site Members web part
            string[] membersWebPart = new string[] { "NumberLimit", "DisplayType", "MembershipGroupId", "Toolbar" };
            if (CheckWebPartProperties(membersWebPart, properties))
            {
                return WebParts.Members;
            }

            // Check for Silverlight web part
            string[] silverlightWebPart = new string[] { "MinRuntimeVersion", "WindowlessMode", "CustomInitParameters", "Url", "ApplicationXml" };
            if (CheckWebPartProperties(silverlightWebPart, properties))
            {
                return WebParts.Silverlight;
            }

            // Check for Add-in Part web part
            string[] addinPartWebPart = new string[] { "FeatureId", "ProductWebId", "ProductId" };
            if (CheckWebPartProperties(addinPartWebPart, properties))
            {
                return WebParts.Client;
            }

            if (isLegacy)
            {
                // Content Editor Web Part
                string[] contentEditorWebPart = new string[] { "Content", "ContentLink", "PartStorage" };
                if (CheckWebPartProperties(contentEditorWebPart, properties))
                {
                    return WebParts.ContentEditor;
                }

                // Image Viewer Web Part
                string[] imageViewerWebPart = new string[] { "ImageLink", "AlternativeText", "VerticalAlignment", "HorizontalAlignment" };
                if (CheckWebPartProperties(imageViewerWebPart, properties))
                {
                    return WebParts.Image;
                }

                // Title Bar 
                if (properties.ContainsKey("TypeName") && properties["TypeName"].ToString() == "Microsoft.SharePoint.WebPartPages.TitleBarWebPart")
                {
                    return WebParts.TitleBar;
                }

                // Check for ListView web part
                string[] legacyListWebPart = new string[] { "ListViewXml", "ListName", "ListId", "ViewContentTypeId" };
                if (CheckWebPartProperties(legacyListWebPart, properties))
                {
                    return WebParts.ListView;
                }

                string[] legacyXsltWebPart = new string[] { "ListUrl", "ListId", "ListName", "CatalogIconImageUrl" };
                if (CheckWebPartProperties(legacyXsltWebPart, properties))
                {
                    // Too Many Lists are showing here, so extra filters are required
                    // Not the cleanest method, but options limited to filter list type without extra calls to SharePoint
                    var iconsToCheck = new string[]{
                    "images/itdl.png", "images/itissue.png", "images/itgen.png" };
                    var iconToRepresent = properties["CatalogIconImageUrl"];
                    foreach (var iconPath in iconsToCheck)
                    {
                        if (iconToRepresent.ToString().ContainsIgnoringCasing(iconPath))
                        {
                            return WebParts.XsltListView;
                        }
                    }
                }
            }

            // Check for Script Editor web part
            string[] scriptEditorWebPart = new string[] { "Content" };
            if (CheckWebPartProperties(scriptEditorWebPart, properties))
            {
                return WebParts.ScriptEditor;
            }

            // This needs to be last, but we still pages with sandbox user code web parts on them
            string[] sandboxWebPart = new string[] { "CatalogIconImageUrl", "AllowEdit", "TitleIconImageUrl", "ExportMode" };
            if (CheckWebPartProperties(sandboxWebPart, properties))
            {
                return WebParts.SPUserCode;
            }

            return "Unsupported Web Part Type";
        }

        private static bool CheckWebPartProperties(string[] propertiesToCheck, Dictionary<string, object> properties)
        {
            return properties.Keys.Intersect(propertiesToCheck).Count() == propertiesToCheck.Length;
        }

        /// <summary>
        /// Gets the type of the web part
        /// </summary>
        /// <param name="webPartXml">Web part xml to analyze</param>
        /// <returns>Type of the web part as fully qualified name</returns>
        private static string GetType(string webPartXml)
        {
            string type = "Unknown";

            if (!string.IsNullOrEmpty(webPartXml))
            {
                var xml = XElement.Parse(webPartXml);
                var xmlns = xml.XPathSelectElement("*").GetDefaultNamespace();
                if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v3", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = xml.Descendants(xmlns + "type").FirstOrDefault().Attribute("name").Value;
                }
                else if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = $"{xml.Descendants(xmlns + "TypeName").FirstOrDefault().Value}, {xml.Descendants(xmlns + "Assembly").FirstOrDefault().Value}";
                }
            }

            return type;
        }

        /// <summary>
        /// Translates the given zone value and page layout to a column number
        /// </summary>
        /// <param name="zoneId">Web part zone id</param>
        /// <param name="layout">Layout of the web part page</param>
        /// <returns>Column value</returns>
        private static int GetColumn(string zoneId, PageLayout layout)
        {
            switch (layout)
            {
                case PageLayout.WebPart_HeaderFooterThreeColumns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("MiddleColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_FullPageVertical:
                    {
                        return 1;
                    }
                case PageLayout.WebPart_HeaderLeftColumnBody:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderRightColumnBody:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderFooter2Columns4Rows:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("Row1", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row2", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row3", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row4", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderFooter4ColumnsTopRow:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("CenterColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("CenterColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_2010_TwoColumnsLeft:
                    {
                        if (zoneId.Equals("Left", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("Right", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        break;
                    }
                case PageLayout.WebPart_Custom:
                    {
                        return 1;
                    }
                default:
                    return 1;
            }

            return 1;
        }

        /// <summary>
        /// Translates the given zone value and page layout to a row number
        /// </summary>
        /// <param name="zoneId">Web part zone id</param>
        /// <param name="layout">Layout of the web part page</param>
        /// <returns>Row value</returns>
        private static int GetRow(string zoneId, PageLayout layout)
        {
            switch (layout)
            {
                case PageLayout.WebPart_HeaderFooterThreeColumns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("MiddleColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_FullPageVertical:
                case PageLayout.WebPart_2010_TwoColumnsLeft:
                    {
                        return 1;
                    }
                case PageLayout.WebPart_HeaderLeftColumnBody:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderRightColumnBody:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Body", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderFooter2Columns4Rows:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row1", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row2", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row3", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("Row4", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_HeaderFooter4ColumnsTopRow:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        break;
                    }
                case PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        else if (zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 4;
                        }
                        break;
                    }
                case PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase) ||
                            zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("TopRow", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 2;
                        }
                        else if (zoneId.Equals("CenterLeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("CenterRightColumn", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 3;
                        }
                        else if (zoneId.Equals("Footer", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 4;
                        }
                        break;
                    }
                case PageLayout.WebPart_Custom:
                    {
                        return 1;
                    }
                default:
                    return 1;
            }

            return 1;
        }

        /// <summary>
        /// Loads the telemetry and properties for the client objects
        /// </summary>
        /// <param name="clientContext"></param>
        private static void LoadClientObjects(ClientContext clientContext)
        {
            if (clientContext != null)
            {
                clientContext.ClientTag = $"SPDev:PageTransformator";

                // Load all web properties needed further one
                clientContext.Load(clientContext.Web, w => w.Id, w => w.Url, w => w.ServerRelativeUrl, w => w.RootFolder.WelcomePage, w => w.Language);
                clientContext.Load(clientContext.Site, s => s.RootWeb.ServerRelativeUrl, s => s.Id, s => s.Url);
                // Use regular ExecuteQuery as we want to send this custom clienttag
                clientContext.ExecuteQuery();
            }
        }

        private static SourcePageInformation ReadSourcePageInformation(ListItem sourcePage)
        {
            var result = new SourcePageInformation();

            result.Author = sourcePage[SharePointConstants.CreatedByField] as FieldUserValue;
            result.Editor = sourcePage[SharePointConstants.ModifiedByField] as FieldUserValue;

            // Ensure to interprete time correctly: SPO stores in UTC, but we'll need to push back in local
            if (DateTime.TryParse(sourcePage[SharePointConstants.CreatedField].ToString(), out DateTime created))
            {
                DateTime createdIsUtc = DateTime.SpecifyKind(created, DateTimeKind.Utc);
                result.Created = createdIsUtc.ToLocalTime();
            }
            if (DateTime.TryParse(sourcePage[SharePointConstants.ModifiedField].ToString(), out DateTime modified))
            {
                DateTime modifiedIsUtc = DateTime.SpecifyKind(modified, DateTimeKind.Utc);
                result.Modified = modifiedIsUtc.ToLocalTime();
            }

            return result;
        }

    }
}
