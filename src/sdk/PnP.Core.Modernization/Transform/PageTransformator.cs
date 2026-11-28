using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Provisioning.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace PnP.Core.Modernization.Transform
{

    /// <summary>
    /// Transforms a classic wiki/webpart page into a modern client side page
    /// </summary>
    public class PageTransformator : BasePageTransformator
    {
        private readonly Regex invalidCharsRegex = new Regex(@"[\*\?\|\\\t/:""'<>#{}%~&]", RegexOptions.Compiled);

        private readonly Regex invalidRulesRegex = new Regex(@"\.{2,}", RegexOptions.Compiled);

        private readonly Regex startEndRegex = new Regex(@"^[\. ]|[\. ]$", RegexOptions.Compiled);

        private readonly Regex extraSpacesRegex = new Regex(" {2,}", RegexOptions.Compiled);

        #region Construction
        /// <summary>
        /// Creates a page transformator instance with a target destination of a target web e.g. Modern/Communication Site
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        /// <param name="targetClientContext">ClientContext of the site that will receive the modernized page</param>
        public PageTransformator(PnPContext sourceClientContext, PnPContext targetClientContext) : this(sourceClientContext, targetClientContext, "webpartmapping.xml")
        {

        }

        /// <summary>
        /// Creates a page transformator instance
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        public PageTransformator(PnPContext sourceClientContext) : this(sourceClientContext, null, "webpartmapping.xml")
        {
        }

        /// <summary>
        /// Creates a page transformator instance
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        /// <param name="pageTransformationFile">Used page mapping file</param>
        public PageTransformator(PnPContext sourceClientContext, string pageTransformationFile) : this(sourceClientContext, null, pageTransformationFile)
        {

        }

        /// <summary>
        /// Creates a page transformator instance
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        /// <param name="targetClientContext">ClientContext of the site that will receive the modernized page</param>
        /// <param name="pageTransformationFile">Used page mapping file</param>
        public PageTransformator(PnPContext sourceClientContext, PnPContext targetClientContext, string pageTransformationFile)
        {

#if DEBUG && MEASURE && MEASURE
            InitMeasurement();
#endif

            this.sourceClientContext = sourceClientContext;
            this.targetClientContext = targetClientContext;
            SharePointOnlineSupport.Validate(sourceClientContext);
            if (targetClientContext != null)
            {
                SharePointOnlineSupport.Validate(targetClientContext);
            }

            this.version = GetVersion();
            this.pageTelemetry = new PageTelemetry(version);

            using (Stream schema = typeof(PageTransformator).Assembly.GetManifestResourceStream("PnP.Core.Modernization.webpartmapping.xsd"))
            {
                // Load xml mapping data
                XmlSerializer xmlMapping = new XmlSerializer(typeof(PageTransformation));

                var fileToLoad = pageTransformationFile;
                if (!File.Exists(pageTransformationFile))
                {
                    fileToLoad = "Modernization" + Path.AltDirectorySeparatorChar + pageTransformationFile;
                }

                using (var stream = new FileStream(fileToLoad, FileMode.Open))
                {
                    // Ensure the provided file complies with the current schema
                    ValidateSchema(schema, stream);

                    // All good so it seems
                    this.pageTransformation = (PageTransformation)xmlMapping.Deserialize(stream);
                }
            }
        }

        /// <summary>
        /// Creates a page transformator instance
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        /// <param name="pageTransformationModel">Page transformation model</param>
        public PageTransformator(PnPContext sourceClientContext, PageTransformation pageTransformationModel) : this(sourceClientContext, null, pageTransformationModel)
        {

        }

        /// <summary>
        /// Creates a page transformator instance
        /// </summary>
        /// <param name="sourceClientContext">ClientContext of the site holding the page</param>
        /// <param name="targetClientContext">ClientContext of the site that will receive the modernized page</param>
        /// <param name="pageTransformationModel">Page transformation model</param>
        public PageTransformator(PnPContext sourceClientContext, PnPContext targetClientContext, PageTransformation pageTransformationModel)
        {

#if DEBUG && MEASURE
            InitMeasurement();
#endif

            this.sourceClientContext = sourceClientContext;
            this.targetClientContext = targetClientContext;
            SharePointOnlineSupport.Validate(sourceClientContext);
            if (targetClientContext != null)
            {
                SharePointOnlineSupport.Validate(targetClientContext);
            }

            this.version = GetVersion();
            this.pageTelemetry = new PageTelemetry(version);

            this.pageTransformation = pageTransformationModel;
        }
        #endregion

        /// <summary>
        /// Transform the page
        /// </summary>
        /// <param name="pageTransformationInformation">Information about the page to transform</param>
        /// <returns>The path to the created modern page</returns>
        /// <exception cref="ArgumentException">The source and target are different webs in the same site collection.</exception>
        public string Transform(PageTransformationInformation pageTransformationInformation)
        {
            SetPageId(Guid.NewGuid().ToString());

            var logsForSettings = pageTransformationInformation.DetailSettingsAsLogEntries(this.version);
            logsForSettings?.ForEach(o => Log(o, LogLevel.Information));

            #region Check for Target Site Context
            var hasTargetContext = this.targetClientContext != null;
            #endregion

            #region Input validation
            string pageType = null;
            if (pageTransformationInformation.SourceFile != null && pageTransformationInformation.SourcePage == null)
            {
                //TODO: extend check to ensure it's a real web part page
                isRootPage = IsRootPage(pageTransformationInformation.SourceFile);

                if (isRootPage)
                {
                    LogInfo(LogStrings.PageLivesOutsideOfALibrary, LogStrings.Heading_InputValidation);

                    // This always is a web part page
                    pageType = "WebPartPage";

                    // Item level permission copy makes no sense here
                    pageTransformationInformation.KeepPageSpecificPermissions = false;

                    // Same for swap pages, we don't support this as the pages live in a different location
                    pageTransformationInformation.TargetPageTakesSourcePageName = false;
                }
                else
                {
                    LogError(LogStrings.Error_BasicASPXPageCannotTransform, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_BasicASPXPageCannotTransform);
                }
            }
            else
            {
                if (pageTransformationInformation.SourcePage == null)
                {
                    LogError(LogStrings.Error_SourcePageNotFound, LogStrings.Heading_InputValidation);
                    throw new ArgumentNullException(LogStrings.Error_SourcePageNotFound);
                }

                pageTransformationInformation.SourcePage = LoadSourcePageFields(pageTransformationInformation.SourcePage);

                // Validate page and it's eligibility for transformation
                if (!pageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileRefField) || !pageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileLeafRefField))
                {
                    LogError(LogStrings.Error_PageNotValidMissingFileRef, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_PageNotValidMissingFileRef);
                }

                pageType = pageTransformationInformation.SourcePage.PageType();
                LogInfo(string.Format(LogStrings.TransformationMode, pageType.FormatAsFriendlyTitle()), LogStrings.Heading_Summary, LogEntrySignificance.TransformMode);

                if (IsClientSidePage(pageType))
                {
                    LogError(LogStrings.Error_SourcePageIsModern, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_SourcePageIsModern);
                }

                if (IsAspxPage(pageType))
                {
                    LogError(LogStrings.Error_BasicASPXPageCannotTransform, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_BasicASPXPageCannotTransform);
                }

                if (IsPublishingPage(pageType))
                {
                    LogError(LogStrings.Error_PublishingPagesNotYetSupported, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_PublishingPagesNotYetSupported);
                }

                if (IsDelveBlogPage(pageType))
                {
                    LogError(LogStrings.Error_DelveBlogPagesNotSupported, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_DelveBlogPagesNotSupported);
                }
            }

            if (hasTargetContext)
            {
                // If we're transforming into another site collection the "revert to old page" model does not exist as the 
                // old page is not present in there. Also adding the page transformation banner does not make sense for the same reason
                if (pageTransformationInformation.ModernizationCenterInformation != null && pageTransformationInformation.ModernizationCenterInformation.AddPageAcceptBanner)
                {
                    LogError(LogStrings.Error_CannotUsePageAcceptBannerCrossSite, LogStrings.Heading_InputValidation);
                    throw new ArgumentException(LogStrings.Error_CannotUsePageAcceptBannerCrossSite);
                }
            }

            if (IsBlogPage(pageType) && !hasTargetContext)
            {
                LogError(LogStrings.Error_BlogPageTransformationHasToBeCrossSite, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_BlogPageTransformationHasToBeCrossSite);
            }

            // Disable cross-farm item level permissions from copying
            CrossFarmTransformationValidation(pageTransformationInformation);

            LogDebug(LogStrings.ValidationChecksComplete, LogStrings.Heading_InputValidation);

            #endregion

            try
            {

                #region Telemetry
#if DEBUG && MEASURE
            Start();
#endif
                DateTime transformationStartDateTime = DateTime.Now;

                LogDebug(LogStrings.LoadingClientContextObjects, LogStrings.Heading_SharePointConnection);
                LoadClientObject(sourceClientContext, false);

                LogInfo($"{sourceClientContext.Web.GetUrl()}", LogStrings.Heading_Summary, LogEntrySignificance.SourceSiteUrl);

                if (hasTargetContext)
                {
                    LogDebug(LogStrings.LoadingTargetClientContext, LogStrings.Heading_SharePointConnection);
                    LoadClientObject(targetClientContext, true);

                    if (IsBlogPage(pageType))
                    {
                        if (sourceClientContext.Site.Id.Equals(targetClientContext.Site.Id))
                        {
                            // Oops, seems source and target point to the same site collection...not allowed for blog page transformation
                            LogError(LogStrings.Error_SameSiteTransferNoAllowedForBlogPages, LogStrings.Heading_SharePointConnection);
                            throw new ArgumentNullException(LogStrings.Error_SameSiteTransferNoAllowedForBlogPages);
                        }

                        // Mark this web as blog web
                        CacheManager.Instance.SetBlogWeb(this.sourceClientContext.Web.GetUrl());

                    }
                    else
                    {
                        if (NormalizeSameSiteTargetContext(sourceClientContext.Site.Id, targetClientContext.Site.Id,
                            sourceClientContext.Web.Id, targetClientContext.Web.Id, pageTransformationInformation))
                        {
                            hasTargetContext = false;
                            LogWarning(LogStrings.Error_FallBackToSameSiteTransfer, LogStrings.Heading_SharePointConnection);
                        }
                        else
                        {
                            // Ensure that the newly created page in the other site collection gets the same name as the source page
                            LogInfo(LogStrings.Error_OverridingTagePageTakesSourcePageName, LogStrings.Heading_SharePointConnection);
                            pageTransformationInformation.TargetPageTakesSourcePageName = true;
                        }
                    }

                    if (hasTargetContext)
                    {
                        LogInfo($"{targetClientContext.Web.GetUrl()}", LogStrings.Heading_Summary, LogEntrySignificance.TargetSiteUrl);
                    }
                }

                PopulateGlobalProperties(sourceClientContext, targetClientContext);

                // Need to add further validation for target template
                if (hasTargetContext &&
                   (targetClientContext.Web.WebTemplate != "SITEPAGEPUBLISHING" && targetClientContext.Web.WebTemplate != "STS" && 
                    targetClientContext.Web.WebTemplate != "GROUP" && targetClientContext.Web.WebTemplate != "BDR" && targetClientContext.Web.WebTemplate != "DEV"))
                {

                    LogError(LogStrings.Error_CrossSiteTransferTargetsNonModernSite);
                    throw new ArgumentException(LogStrings.Error_CrossSiteTransferTargetsNonModernSite, LogStrings.Heading_SharePointConnection);
                }

                // Ensure PostAsNews is used together with PagePublishing
                if (pageTransformationInformation.PostAsNews && !pageTransformationInformation.PublishCreatedPage)
                {
                    pageTransformationInformation.PublishCreatedPage = true;
                    LogWarning(LogStrings.Warning_PostingAPageAsNewsRequiresPagePublishing, LogStrings.Heading_Summary);
                }

                // Store the information of the source page we do want to retain
                if (pageTransformationInformation.SourcePage != null)
                {
                    StoreSourcePageInformationToKeep(pageTransformationInformation.SourcePage);
                }

                LogInfo($"{GetFieldValue(pageTransformationInformation, Constants.FileRefField)}", LogStrings.Heading_Summary, LogEntrySignificance.SourcePage);

                var spVersion = pageTransformationInformation.SourceVersion;
                var exactSpVersion = pageTransformationInformation.SourceVersionNumber;
                LogInfo($"{spVersion.DisplaySharePointVersion()} ({exactSpVersion})", LogStrings.Heading_Summary, LogEntrySignificance.SharePointVersion);

                if (hasTargetContext)
                {
                    //Load User Mapping File
                    InitializeUserMapping(pageTransformationInformation);
                }

#if DEBUG && MEASURE
            Stop("Telemetry");
#endif
                #endregion

                #region Page creation
                // Detect if the page is living inside a folder
                LogDebug(LogStrings.DetectIfPageIsInFolder, LogStrings.Heading_PageCreation);
                string pageFolder = "";

                if (FieldExistsAndIsUsed(pageTransformationInformation, Constants.FileDirRefField))
                {
                    var fileRefFieldValue = GetFieldValue(pageTransformationInformation, Constants.FileDirRefField);

                    if (IsBlogPage(pageType))
                    {
                        if (fileRefFieldValue.ContainsIgnoringCasing($"/lists/{CacheManager.Instance.GetBlogListName(sourceClientContext)}"))
                        {
                            pageFolder = fileRefFieldValue.Replace($"{sourceClientContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/Lists/{CacheManager.Instance.GetBlogListName(sourceClientContext)}", "", StringComparison.InvariantCultureIgnoreCase).Trim();
                        }
                        else
                        {
                            // Page was living in another list, leave the list name as that will be the folder hosting the modern file in SitePages.
                            // This convention is used to avoid naming conflicts
                            pageFolder = fileRefFieldValue.Replace($"{sourceClientContext.Web.ServerRelativeUrl}", "").Trim();
                        }
                    }
                    else
                    {
                        if (fileRefFieldValue.ContainsIgnoringCasing("/sitepages"))
                        {
                            pageFolder = fileRefFieldValue.Replace($"{sourceClientContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/SitePages", "").Trim();
                        }
                        else
                        {
                            // Page was living in another list, leave the list name as that will be the folder hosting the modern file in SitePages.
                            // This convention is used to avoid naming conflicts
                            pageFolder = fileRefFieldValue.Replace($"{sourceClientContext.Web.ServerRelativeUrl}", "").Trim();
                        }
                    }

                    if (pageFolder.Length > 0)
                    {
                        if (pageFolder.StartsWith("/"))
                        {
                            if (pageFolder == "/")
                            {
                                pageFolder = "";
                            }
                            else
                            {
                                pageFolder = pageFolder.Substring(1);
                            }
                        }

                        // Add a trailing slash
                        pageFolder = pageFolder + "/";

                        LogInfo(LogStrings.PageIsLocatedInFolder, LogStrings.Heading_PageCreation);
                    }

                    if (isRootPage)
                    {
                        pageFolder = "Root/";
                        if (string.IsNullOrEmpty(pageTransformationInformation.TargetPageName))
                        {
                            pageTransformationInformation.TargetPageName = $"{GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField)}";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pageTransformationInformation.TargetPageFolder))
                        {
                            // Handle special case <root> to indicate page should be created in the root folder
                            if (pageTransformationInformation.TargetPageFolder.Equals("<root>", StringComparison.InvariantCultureIgnoreCase))
                            {
                                pageTransformationInformation.TargetPageFolder = "";
                            }

                            if (pageTransformationInformation.TargetPageFolderOverridesDefaultFolder)
                            {
                                pageFolder = pageTransformationInformation.TargetPageFolder;
                            }
                            else
                            {
                                pageFolder = Path.Combine(pageFolder, pageTransformationInformation.TargetPageFolder);
                            }

                            if (!pageFolder.EndsWith("/"))
                            {
                                // Add a trailing slash
                                pageFolder = pageFolder + "/";
                            }
                        }
                    }
                }
                pageTransformationInformation.Folder = pageFolder;

                // If no targetname specified then we'll come up with one
                if (string.IsNullOrEmpty(pageTransformationInformation.TargetPageName))
                {
                    if (string.IsNullOrEmpty(pageTransformationInformation.TargetPagePrefix))
                    {
                        LogInfo(LogStrings.NoTargetNameUsingDefaultPrefix, LogStrings.Heading_PageCreation);
                        pageTransformationInformation.SetDefaultTargetPagePrefix();
                    }

                    if (hasTargetContext)
                    {
                        LogInfo(LogStrings.CrossSiteInUseUsingOriginalFileName, LogStrings.Heading_PageCreation);
                        if (IsBlogPage(pageType))
                        {
                            var generatedBlogPageName = $"{GetFieldValue(pageTransformationInformation, Constants.TitleField).Replace(" ", "-")}-{GetFieldValue(pageTransformationInformation, Constants.IDField)}.aspx";

                            // Based on this blog - http://www.simplyaprogrammer.com/2008/05/importing-files-into-sharepoint.html
                            string sanitizedName = extraSpacesRegex.Replace(invalidRulesRegex.Replace(invalidCharsRegex.Replace(input: generatedBlogPageName, replacement: string.Empty).Trim(), "."), " ");

                            while (startEndRegex.IsMatch(sanitizedName))
                            {
                                sanitizedName = startEndRegex.Replace(sanitizedName, string.Empty);
                            }

                            pageTransformationInformation.TargetPageName = sanitizedName;
                        }
                        else
                        {
                            pageTransformationInformation.TargetPageName = $"{GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField)}";
                        }
                    }
                    else
                    {
                        LogInfo(LogStrings.UsingSuppliedPrefix, LogStrings.Heading_PageCreation);
                        pageTransformationInformation.TargetPageName = $"{pageTransformationInformation.TargetPagePrefix}{GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField)}";
                    }

                }

                // Canonicalize the target page name once, before it is used to probe for a collision, to save the
                // page and to read the saved page back. Saving appends the .aspx extension when it is missing, so a
                // caller supplied name without the extension would otherwise probe and read back a different file
                // than the one that is written, and an existing target page could be overwritten unnoticed.
                pageTransformationInformation.TargetPageName = EnsureAspxExtension(pageTransformationInformation.TargetPageName);

                // Check if page name is free to use
#if DEBUG && MEASURE
            Start();
#endif
                bool pageExists = false;
                IPage targetPage = null;
                IList pagesLibrary = null;
                IFile existingFile = null;

                //The determines of the target client context has been specified and use that to generate the target page
                PnPContext context = hasTargetContext ? targetClientContext : sourceClientContext;

                try
                {
                    LogDebug(LogStrings.LoadingExistingPageIfExists, LogStrings.Heading_PageCreation);

                    // Just try to load the page in the fastest possible manner, we only want to see if the page exists or not
                    existingFile = Load(sourceClientContext, pageTransformationInformation, pageType, out pagesLibrary, targetClientContext);
                    pageExists = true;
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException)
                    {
                        LogInfo(LogStrings.CheckPageExistsError, LogStrings.Heading_PageCreation);
                    }
                    else
                    {
                        LogError(LogStrings.CheckPageExistsError, LogStrings.Heading_PageCreation, ex, true);
                    }

                }
#if DEBUG && MEASURE
            Stop("Load Page");
#endif

                if (pageExists)
                {
                    LogInfo(LogStrings.PageAlreadyExistsInTargetLocation, LogStrings.Heading_PageCreation);

                    if (!pageTransformationInformation.Overwrite)
                    {
                        var message = $"{LogStrings.PageNotOverwriteIfExists}  {pageTransformationInformation.TargetPageName}.";
                        LogError(message, LogStrings.Heading_PageCreation);
                        throw new ArgumentException(message);
                    }
                }

                // Create the client side page

                targetPage = context.Web.NewPage();
                LogInfo($"{LogStrings.ModernPageCreated} ", LogStrings.Heading_PageCreation);
                #endregion

                #region Home page handling
#if DEBUG && MEASURE
            Start();
#endif
                LogDebug(LogStrings.TransformCheckIfPageIsHomePage, LogStrings.Heading_HomePageHandling);

                bool replacedByOOBHomePage = false;
                // Check if the transformed page is the web's home page
                if (sourceClientContext.Web.RootFolder.IsPropertyAvailable(f => f.WelcomePage) && !string.IsNullOrEmpty(sourceClientContext.Web.RootFolder.WelcomePage))
                {
                    LogInfo(LogStrings.WelcomePageSettingsIsPresent, LogStrings.Heading_HomePageHandling);

                    var homePageUrl = sourceClientContext.Web.RootFolder.WelcomePage;
                    var homepageName = Path.GetFileName(sourceClientContext.Web.RootFolder.WelcomePage);
                    if (homepageName.Equals(GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField), StringComparison.InvariantCultureIgnoreCase))
                    {
                        LogInfo(LogStrings.TransformSourcePageIsHomePage, LogStrings.Heading_HomePageHandling);

                        targetPage.LayoutType = PageLayoutType.Home;
                        if (pageTransformationInformation.ReplaceHomePageWithDefaultHomePage)
                        {
                            targetPage.KeepDefaultWebParts = true;
                            replacedByOOBHomePage = true;

                            LogInfo(LogStrings.TransformSourcePageHomePageUsingStock,
                                LogStrings.Heading_HomePageHandling);
                        }
                    }
                    else
                    {
                        LogInfo(LogStrings.TransformSourcePageIsNotHomePage, LogStrings.Heading_HomePageHandling);
                    }
                }
#if DEBUG && MEASURE
            Stop(LogStrings.Heading_HomePageHandling);
#endif
                #endregion

                #region Article page handling

                if (!replacedByOOBHomePage)
                {
                    LogInfo(LogStrings.TransformSourcePageAsArticlePage, LogStrings.Heading_ArticlePageHandling);

                    #region Configure header from target page
#if DEBUG && MEASURE
                Start();
#endif
                    if (pageTransformationInformation.PageHeader == null || (pageTransformationInformation.PageHeader as IPageHeader).Type == PageHeaderType.None)
                    {
                        LogInfo(LogStrings.TransformArticleSetHeaderToNone, LogStrings.Heading_ArticlePageHandling);

                        if (pageTransformationInformation.SetAuthorInPageHeader && pageTransformationInformation.SourcePage != null)
                        {
                            targetPage.SetDefaultPageHeader();
                            targetPage.PageHeader.LayoutType = PageHeaderLayoutType.NoImage;

                            LogInfo(LogStrings.TransformArticleSetHeaderToNoneWithAuthor, LogStrings.Heading_ArticlePageHandling);
                            SetAuthorInPageHeader(context, targetPage);
                        }
                        else
                        {
                            targetPage.RemovePageHeader();
                        }
                    }
                    else if ((pageTransformationInformation.PageHeader as IPageHeader).Type == PageHeaderType.Default)
                    {
                        LogInfo(LogStrings.TransformArticleSetHeaderToDefault, LogStrings.Heading_ArticlePageHandling);

                        targetPage.SetDefaultPageHeader();
                    }
                    else if ((pageTransformationInformation.PageHeader as IPageHeader).Type == PageHeaderType.Custom)
                    {
                        LogInfo($"{LogStrings.TransformArticleSetHeaderToCustom} " +
                                $"{LogStrings.TransformArticleHeaderImageUrl} {pageTransformationInformation.PageHeader.ImageServerRelativeUrl} ", LogStrings.Heading_ArticlePageHandling);

                        targetPage.SetCustomPageHeader(pageTransformationInformation.PageHeader.ImageServerRelativeUrl, pageTransformationInformation.PageHeader.TranslateX, pageTransformationInformation.PageHeader.TranslateY);
                    }
#if DEBUG && MEASURE
                Stop("Target page header");
#endif
                    #endregion

                    #region Analysis of the source page
#if DEBUG && MEASURE
                Start();
#endif
                    // Analyze the source page
                    Tuple<PageLayout, List<WebPartEntity>> pageData = null;

                    if (IsWikiPage(pageType))
                    {
                        LogInfo($"{LogStrings.TransformSourcePageIsWikiPage} - {LogStrings.TransformSourcePageAnalysing}", LogStrings.Heading_ArticlePageHandling);

                        pageData = new WikiPage(pageTransformationInformation.SourcePage, pageTransformation, base.RegisteredLogObservers).Analyze();

                        // Images stay inline; embedded iframes are handled as separate video web parts.
                        LogInfo(LogStrings.WikiTextContainsImagesVideosReferences, LogStrings.Heading_ArticlePageHandling);
                    }
                    else if (IsBlogPage(pageType))
                    {
                        pageData = new WikiPage(pageTransformationInformation.SourcePage, pageTransformation, base.RegisteredLogObservers).Analyze(isBlogPage: true);
                    }
                    else if (IsWebPartPage(pageType))
                    {
                        LogInfo($"{LogStrings.TransformSourcePageIsWebPartPage} {LogStrings.TransformSourcePageAnalysing}", LogStrings.Heading_ArticlePageHandling);

                        pageData = new WebPartPage(pageTransformationInformation.SourcePage, pageTransformationInformation.SourceFile, pageTransformation, base.RegisteredLogObservers).Analyze(true);
                    }

                    // Analyze the "text" parts (wikitext parts and text in content editor web parts)
                    pageData = new Tuple<PageLayout, List<WebPartEntity>>(pageData.Item1, new WikiHtmlTransformator(this.sourceClientContext, context, targetPage, pageTransformationInformation as BaseTransformationInformation, base.RegisteredLogObservers).TransformPlusSplit(pageData.Item2, pageTransformationInformation.HandleWikiImagesAndVideos, pageTransformationInformation.AddTableListImageAsImageWebPart));

#if DEBUG && MEASURE
                Stop("Analyze page");
#endif
                    #endregion

                    #region Page title configuration
#if DEBUG && MEASURE
                Start();
#endif
                    // Set page title
                    if (IsWikiPage(pageType))
                    {
                        SetPageTitle(pageTransformationInformation, targetPage);
                    }
                    else if (IsBlogPage(pageType))
                    {
                        targetPage.PageTitle = GetFieldValue(pageTransformationInformation, Constants.TitleField);
                        if (targetPage.PageTitle == null)
                        {
                            targetPage.PageTitle = "";
                        }
                    }
                    else if (IsWebPartPage(pageType))
                    {
                        bool titleFound = false;
                        var titleBarWebPart = pageData.Item2.Where(p => p.Type == WebParts.TitleBar).FirstOrDefault();
                        if (titleBarWebPart != null)
                        {
                            if (titleBarWebPart.Properties.ContainsKey("HeaderTitle") && !string.IsNullOrEmpty(titleBarWebPart.Properties["HeaderTitle"]))
                            {
                                var title = titleBarWebPart.Properties["HeaderTitle"];

                                LogInfo($"{LogStrings.TransformPageModernTitle} {title}", LogStrings.Heading_ArticlePageHandling);
                                targetPage.PageTitle = title;
                                titleFound = true;
                            }
                        }

                        if (!titleFound)
                        {
                            SetPageTitle(pageTransformationInformation, targetPage);
                        }
                    }

                    if (pageTransformationInformation.PageTitleOverride != null)
                    {
                        var title = pageTransformationInformation.PageTitleOverride(targetPage.PageTitle);
                        targetPage.PageTitle = title;

                        LogInfo($"{LogStrings.TransformPageTitleOverride} - page title: {title}", LogStrings.Heading_ArticlePageHandling);
                    }
#if DEBUG && MEASURE
                Stop("Set page title");
#endif
                    #endregion

                    #region Page layout configuration
#if DEBUG && MEASURE
                Start();
#endif
                    // Use the default layout transformator
                    ILayoutTransformator layoutTransformator = new LayoutTransformator(targetPage);

                    // Do we have an override?
                    if (pageTransformationInformation.LayoutTransformatorOverride != null)
                    {
                        LogInfo(LogStrings.TransformLayoutTransformatorOverride, LogStrings.Heading_ArticlePageHandling);
                        layoutTransformator = pageTransformationInformation.LayoutTransformatorOverride(targetPage);
                    }

                    // Apply the layout to the page
                    layoutTransformator.Transform(pageData);
#if DEBUG && MEASURE
                Stop("Page layout");
#endif
                    #endregion

                    #region Page Banner creation
//                    if (!pageTransformationInformation.TargetPageTakesSourcePageName)
//                    {

//                        if (pageTransformationInformation.ModernizationCenterInformation != null && pageTransformationInformation.ModernizationCenterInformation.AddPageAcceptBanner)
//                        {

//#if DEBUG && MEASURE
//                        Start();
//#endif

//                            // Bump the row values for the existing web parts as we've inserted a new section
//                            foreach (var section in targetPage.Sections)
//                            {
//                                section.Order = section.Order + 1;
//                            }

//                            // Add new section for banner part
//                            //BERT: find alternative approach for this
//                            //targetPage.Sections.Insert(0, new PnPCore.CanvasSection(targetPage, PnPCore.CanvasSectionTemplate.OneColumn, 0));

//                            // Bump the row values for the existing web parts as we've inserted a new section
//                            foreach (var webpart in pageData.Item2.Where(c => !c.IsClosed))
//                            {
//                                webpart.Row = webpart.Row + 1;
//                            }


//                            var sourcePageUrl = GetFieldValue(pageTransformationInformation, Constants.FileRefField);
//                            var orginalSourcePageName = GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField);
//                            Uri host = new Uri(sourceClientContext.Web.GetUrl());

//                            string path = $"{host.Scheme}://{host.DnsSafeHost}{sourcePageUrl.Replace(GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField), "")}";

//                            // Add "fake" banner web part that then will be transformed onto the page
//                            Dictionary<string, string> props = new Dictionary<string, string>(2)
//                        {
//                            { "SourcePage", $"{path}{orginalSourcePageName}" },
//                            { "TargetPage", $"{path}{pageTransformationInformation.TargetPageName}" }
//                        };

//                            WebPartEntity bannerWebPart = new WebPartEntity()
//                            {
//                                Type = WebParts.PageAcceptanceBanner,
//                                Column = 1,
//                                Row = 1,
//                                Title = "",
//                                Order = 0,
//                                Properties = props,
//                            };
//                            pageData.Item2.Insert(0, bannerWebPart);
//                            LogInfo(LogStrings.TransformAddedPageAcceptBanner, LogStrings.Heading_ArticlePageHandling);

//#if DEBUG && MEASURE
//                        Stop("Page Banner");
//#endif
//                        }
//                    }
                    #endregion

                    #region Content transformation

                    LogDebug(LogStrings.PreparingContentTransformation, LogStrings.Heading_ArticlePageHandling);

#if DEBUG && MEASURE
                Start();
#endif
                    // Use the default content transformator
                    IContentTransformator contentTransformator = new ContentTransformator(sourceClientContext, context, targetPage, pageTransformation, pageTransformationInformation as BaseTransformationInformation, base.RegisteredLogObservers);

                    // Do we have an override?
                    if (pageTransformationInformation.ContentTransformatorOverride != null)
                    {
                        LogInfo(LogStrings.TransformUsingContentTransformerOverride, LogStrings.Heading_ArticlePageHandling);

                        contentTransformator = pageTransformationInformation.ContentTransformatorOverride(targetPage, pageTransformation);
                    }

                    LogInfo(LogStrings.TransformingContentStart, LogStrings.Heading_ArticlePageHandling);

                    // Run the content transformator
                    contentTransformator.Transform(pageData.Item2.Where(c => !c.IsClosed).ToList());

                    LogInfo(LogStrings.TransformingContentEnd, LogStrings.Heading_ArticlePageHandling);
#if DEBUG && MEASURE
                Stop("Content transformation");
#endif
                    #endregion

                    #region Text/Section/Column cleanup
                    // Drop "empty" text parts. Wiki pages tend to have a lot of text parts just containing div's and BR's...no point in keep those as they generate to much whitespace
                    RemoveEmptyTextParts(targetPage);

                    // Remove empty sections and columns to optimize screen real estate
                    if (pageTransformationInformation.RemoveEmptySectionsAndColumns)
                    {
                        RemoveEmptySectionsAndColumns(targetPage);
                    }
                    #endregion
                }
                #endregion

                #region Page persisting + permissions
                #region Save the page
#if DEBUG && MEASURE
            Start();
#endif
                // Persist the client side page
                if (hasTargetContext)
                {
                    var pageName = $"{pageTransformationInformation.Folder}{pageTransformationInformation.TargetPageName}";

                    SavePage(targetPage, pageName, pageTransformationInformation, context);

                    LogInfo($"{LogStrings.TransformSavedPageInCrossSiteCollection}: {pageName}", LogStrings.Heading_ArticlePageHandling);
                }
                else
                {
                    var pageName = $"{pageTransformationInformation.Folder}{pageTransformationInformation.TargetPageName}";

                    SavePage(targetPage, pageName, pageTransformationInformation, context);

                    LogInfo($"{LogStrings.TransformSavedPage}: {pageName}", LogStrings.Heading_ArticlePageHandling);
                }


                // Load the page list item
                // Load the page list item
                string pageNameToLoad = $"{pageTransformationInformation.Folder}{pageTransformationInformation.TargetPageName}";
                if (pageNameToLoad.StartsWith("/"))
                {
                    pageNameToLoad = pageNameToLoad.Substring(1);
                }

                IFile savedTargetPage = context.Web.GetFileByServerRelativeUrl($"{targetPage.PagesLibrary.RootFolder.ServerRelativeUrl}/{pageNameToLoad}");
                savedTargetPage.LoadBatch(p => p.ListItemAllFields);
                context.Web.PnPContext.Execute();
                savedTargetPage.ListItemAllFields.EnsureProperties(item => item.ParentList.QueryProperties(list => list.Id));


#if DEBUG && MEASURE
            Stop("Persist page");
#endif
                #endregion

                #region Page metadata handling
                // Temporary removal of metadata copy for cross site.
                if (pageTransformationInformation.CopyPageMetadata)
                {
#if DEBUG && MEASURE
                Start();
#endif
                    // Copy the page metadata 
                    CopyPageMetadata(pageTransformationInformation, pageType, savedTargetPage, pagesLibrary);
#if DEBUG && MEASURE
                Stop("Page metadata handling");
#endif
                }
                #endregion

                #region Permission handling
                ListItemPermission listItemPermissionsToKeep = null;
                if (pageTransformationInformation.KeepPageSpecificPermissions)
                {
#if DEBUG && MEASURE
                Start();
#endif
                    // Check if we do have item level permissions we want to take over
                    listItemPermissionsToKeep = GetItemLevelPermissions(hasTargetContext, pagesLibrary, pageTransformationInformation.SourcePage, savedTargetPage.ListItemAllFields);
                    ApplyItemLevelPermissions(hasTargetContext, savedTargetPage.ListItemAllFields, listItemPermissionsToKeep);
#if DEBUG && MEASURE
                Stop("Permission handling");
#endif
                }
                #endregion

                #region Page Publishing
                // Tag the file with a page modernization version stamp
                string serverRelativePathForModernPage = savedTargetPage.ServerRelativeUrl;
                bool pageListItemWasReloaded = false;
                try
                {
                    IFile targetPageFile = context.Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
                    targetPageFile.LoadBatch(p => p.Properties);
                    targetPageFile.Properties["sharepointpnp_pagemodernization"] = this.version;
                    targetPageFile.Update();

                    if (!pageTransformationInformation.KeepPageCreationModificationInformation &&
                        !pageTransformationInformation.PostAsNews &&
                        pageTransformationInformation.PublishCreatedPage)
                    {
                        // Try to publish, if publish is not needed/possible (e.g. when no minor/major versioning set) then this will return an error that we'll be ignoring
                        targetPageFile.Publish(LogStrings.PublishMessage);
                    }

                    // Ensure we've the most recent page list item loaded, must be last statement before calling ExecuteQuery
                    savedTargetPage.ListItemAllFields.LoadBatch();
                    // Send both the property update and publish as a single operation to SharePoint
                    context.Execute();
                    pageListItemWasReloaded = true;
                }
                catch (Exception)
                {
                    // Eat exceptions as this is not critical for the generated page
                    LogWarning(LogStrings.Warning_NonCriticalErrorDuringVersionStampAndPublish, LogStrings.Heading_ArticlePageHandling);
                }

                // Update flags field to indicate this is a "migrated" page
                try
                {
                    // If for some reason the reload batched with the previous request did not finish then do it again
                    if (!pageListItemWasReloaded)
                    {
                        savedTargetPage.ListItemAllFields.LoadBatch();
                        context.Execute();
                    }

                    // Only perform the update when the field was not yet set
                    bool skipSettingMigratedFromServerRendered = false;
                    if (savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] != null)
                    {
                        skipSettingMigratedFromServerRendered = (savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] as string[]).Contains("MigratedFromServerRendered");
                    }

                    if (!skipSettingMigratedFromServerRendered)
                    {
                        savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] = ";#MigratedFromServerRendered;#";
                        // Don't use UpdateOverWriteVersion as the listitem already exists 
                        // resulting in an "Additions to this Web site have been blocked" error
                        savedTargetPage.ListItemAllFields.SystemUpdate();
                        savedTargetPage.ListItemAllFields.LoadBatch();
                        context.Execute();
                    }
                }
                catch (Exception)
                {
                    // Eat any exception
                }

                // Disable page comments on the create page, if needed
                if (pageTransformationInformation.DisablePageComments)
                {
                    targetPage.DisableComments();
                    LogInfo(LogStrings.TransformDisablePageComments, LogStrings.Heading_ArticlePageHandling);
                }

                #endregion

                IListItem finalListItemToUpdate;

                #region Page name switching
                // All went well so far...swap pages if that's needed. When copying to another site collection this step is not needed
                // as the created page already has the final name
                if (pageTransformationInformation.TargetPageTakesSourcePageName && !hasTargetContext)
                {
#if DEBUG && MEASURE
                Start();
#endif
                    serverRelativePathForModernPage = SwapPages(pageTransformationInformation, serverRelativePathForModernPage);

                    // Reload the target page list item for future updates because the existing reference is invalid due to the moveto operations from swappages
                    IFile targetPageFile = context.Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
                    targetPageFile.LoadBatch(p => p.ListItemAllFields);
                    context.Execute();

                    finalListItemToUpdate = targetPageFile.ListItemAllFields;
                    finalListItemToUpdate.EnsureProperties(item => item.ParentList.QueryProperties(list => list.Id));

#if DEBUG && MEASURE
                Stop("Pagename swap");
#endif
                }
                else
                {
                    finalListItemToUpdate = savedTargetPage.ListItemAllFields;
                }
                #endregion

                #region Restore page author/editor/created/modified
                if ((pageTransformationInformation.SourcePage != null && pageTransformationInformation.KeepPageCreationModificationInformation && this.SourcePageAuthor != null && this.SourcePageEditor != null) ||
                    pageTransformationInformation.PostAsNews)
                {
                    UpdateTargetPageWithSourcePageInformation(finalListItemToUpdate, pageTransformationInformation, serverRelativePathForModernPage, hasTargetContext);
                }
                #endregion

                // NO page updates are allowed anymore past this point as otherwise the set page usage information and published/posted state will be impacted!

                #region Telemetry
                if (!pageTransformationInformation.SkipTelemetry && this.pageTelemetry != null)
                {
                    TimeSpan duration = DateTime.Now.Subtract(transformationStartDateTime);
                    this.pageTelemetry.LogTransformationDone(duration, pageType, pageTransformationInformation);
                    this.pageTelemetry.Flush();
                }

                LogInfo(LogStrings.TransformComplete, LogStrings.Heading_PageCreation);
                #endregion

                #region Closing
                CacheManager.Instance.SetLastUsedTransformator(this);
                LogInfo(serverRelativePathForModernPage, LogStrings.Heading_Summary, LogEntrySignificance.TargetPage);
                return Uri.EscapeUriString(serverRelativePathForModernPage);
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                LogError(LogStrings.CriticalError_ErrorOccurred, LogStrings.Heading_Summary, ex, isCriticalException: true);

                // Throw exception if there's no registered log observers
                if (base.RegisteredLogObservers.Count == 0)
                {
                    throw;
                }
            }

            return string.Empty;
        }

        public string SwapPages(PageTransformationInformation pageTransformationInformation)
        {
            return SwapPages(pageTransformationInformation, null);
        }

        /// <summary>
        /// Performs the logic needed to swap a genered Migrated_Page.aspx to Page.aspx and then Page.aspx to Old_Page.aspx
        /// </summary>
        /// <param name="pageTransformationInformation">Information about the page to transform</param>
        /// <param name="targetPageServerRelativeUrl"></param>
        internal string SwapPages(PageTransformationInformation pageTransformationInformation, string targetPageServerRelativeUrl)
        {
            LogInfo("Swapping pages", LogStrings.Heading_SwappingPages);

            // Prep url's
            var sourcePageUrl = GetFieldValue(pageTransformationInformation, Constants.FileRefField);
            var orginalSourcePageName = GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField);

            string sourcePath = sourcePageUrl.Replace(GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField), "");
            string targetPath = sourcePath;

            if (!sourcePath.ContainsIgnoringCasing("/sitepages"))
            {
                // Source file was living outside of the site pages library
                targetPath = sourcePath.Replace(sourceClientContext.Web.ServerRelativeUrl, "");
                targetPath = $"{sourceClientContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/SitePages{targetPath}";
            }

            IFile sourcePage = this.sourceClientContext.Web.GetFileByServerRelativeUrl(sourcePageUrl);
            sourcePage.LoadBatch(p => p.ListItemAllFields);
            this.sourceClientContext.Execute();

            if (string.IsNullOrEmpty(pageTransformationInformation.SourcePagePrefix))
            {
                LogInfo("Using default source page prefix", LogStrings.Heading_SwappingPages);
                pageTransformationInformation.SetDefaultSourcePagePrefix();
            }
            var newSourcePageUrl = $"{pageTransformationInformation.SourcePagePrefix}{GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField)}";

            // Check if the page that we're working on was listed in current or global navigation
            this.sourceClientContext.Web.LoadBatch(w => w.Navigation.QueryProperties(n => n.QuickLaunch, n => n.TopNavigationBar));
            this.sourceClientContext.Web.PnPContext.Execute();
            IQueryable<INavigationNode> currentNavNodesBefore = this.sourceClientContext.Web.Navigation.QuickLaunch;
            IQueryable<INavigationNode> globalNavNodesBefore = this.sourceClientContext.Web.Navigation.TopNavigationBar;

            bool navNodesRequireFixing = false;
            var currentNavNodes = currentNavNodesBefore.Where(n => n.Url.Equals(sourcePageUrl, StringComparison.InvariantCultureIgnoreCase));
            var globalNavNodes = globalNavNodesBefore.Where(n => n.Url.Equals(sourcePageUrl, StringComparison.InvariantCultureIgnoreCase));

            if (currentNavNodes.Any() || globalNavNodes.Any())
            {
                navNodesRequireFixing = true;
            }

            // Rename source page using the sourcepageprefix
            // STEP1: First move the source page to a new name.
            var step1Path = $"{sourcePath}{newSourcePageUrl}";
            sourcePage.MoveTo(step1Path, MoveOperations.Overwrite);
            this.sourceClientContext.Execute();
            LogInfo($"{LogStrings.TransformSwappingPageStep1}: {step1Path}", LogStrings.Heading_SwappingPages);

            // Ensure the original author/editor/created/modified is retained on the rename of the original page
            sourcePage.ListItemAllFields[Constants.CreatedByField] = this.SourcePageAuthor;
            sourcePage.ListItemAllFields[Constants.ModifiedByField] = this.SourcePageEditor;
            sourcePage.ListItemAllFields[Constants.CreatedField] = this.SourcePageCreated;
            sourcePage.ListItemAllFields[Constants.ModifiedField] = this.SourcePageModified;
            sourcePage.ListItemAllFields.UpdateOverwriteVersion();
            sourceClientContext.Execute();

            //Load the created target page
            string targetPageUrl;
            if (!string.IsNullOrEmpty(targetPageServerRelativeUrl))
            {
                targetPageUrl = targetPageServerRelativeUrl;
            }
            else
            {
                targetPageUrl = $"{targetPath}{pageTransformationInformation.TargetPageName}";
            }

            IFile targetPageFile = this.sourceClientContext.Web.GetFileByServerRelativeUrl(targetPageUrl);
            targetPageFile.LoadBatch();
            this.sourceClientContext.Execute();

            LogInfo(LogStrings.TransformSwappingPageStep2, LogStrings.Heading_SwappingPages);
            // STEP2: Now move the created modern page over the original source page, at this point the new page has the same name as the original page had before transformation
            var step2Path = $"{targetPath}{orginalSourcePageName}";
            targetPageFile.MoveTo(step2Path, MoveOperations.Overwrite);
            this.sourceClientContext.Execute();
            LogInfo($"{LogStrings.TransformSwappingPageStep2Path} :{step2Path}", LogStrings.Heading_SwappingPages);

            LogInfo(LogStrings.TransformSwappingPageStep3, LogStrings.Heading_SwappingPages);
            // STEP 3: Load navigation and fix
            if (navNodesRequireFixing)
            {
                this.sourceClientContext.Web.LoadBatch(w => w.Navigation.QueryProperties(n => n.QuickLaunch, n => n.TopNavigationBar));
                this.sourceClientContext.Web.PnPContext.Execute();
                IQueryable<INavigationNode> currentNavNodesAfter = this.sourceClientContext.Web.Navigation.QuickLaunch;
                IQueryable<INavigationNode> globalNavNodesAfter = this.sourceClientContext.Web.Navigation.TopNavigationBar;
                this.sourceClientContext.Execute();

                // Check for nav nodes
                currentNavNodes = currentNavNodesAfter.Where(n => n.Url.Equals(step1Path, StringComparison.InvariantCultureIgnoreCase));
                globalNavNodes = globalNavNodesAfter.Where(n => n.Url.Equals(step1Path, StringComparison.InvariantCultureIgnoreCase));

                if (currentNavNodes.Any() || globalNavNodes.Any())
                {
                    foreach (INavigationNode node in currentNavNodes)
                    {
                        node.Url = sourcePageUrl;
                        node.Update();
                    }
                    foreach (INavigationNode node in globalNavNodes)
                    {
                        node.Url = sourcePageUrl;
                        node.Update();
                    }
                    this.sourceClientContext.Execute();
                    LogInfo(LogStrings.TransformSwappingPageUpdateNavigation, LogStrings.Heading_SwappingPages);
                }
            }

            return step2Path;
        }

        /// <summary>
        /// Loads a page transformation model from file
        /// </summary>
        /// <param name="pageTransformationFile">File holding the page transformation model</param>
        /// <returns>Page transformation model</returns>
        public static PageTransformation LoadPageTransformationModel(string pageTransformationFile)
        {
            // Load xml mapping data
            XmlSerializer xmlMapping = new XmlSerializer(typeof(PageTransformation));
            using (var stream = new FileStream(pageTransformationFile, FileMode.Open))
            {
                return (PageTransformation)xmlMapping.Deserialize(stream);
            }
        }

        #region Helper methods
        internal bool NormalizeSameSiteTargetContext(Guid sourceSiteId, Guid targetSiteId, Guid sourceWebId, Guid targetWebId,
            BaseTransformationInformation transformationInformation)
        {
            if (!sourceSiteId.Equals(targetSiteId))
            {
                return false;
            }

            if (!sourceWebId.Equals(targetWebId))
            {
                LogError(LogStrings.Error_SameSiteTransferDifferentWeb, LogStrings.Heading_SharePointConnection);
                throw new ArgumentException(LogStrings.Error_SameSiteTransferDifferentWeb);
            }

            targetClientContext = null;
            // Validation ran before the site and web IDs were loaded; discard its cross-site state too.
            transformationInformation.IsCrossSiteTransformation = false;
            transformationInformation.IsCrossFarmTransformation = false;
            transformationInformation.TargetVersion = transformationInformation.SourceVersion;
            transformationInformation.TargetVersionNumber = transformationInformation.SourceVersionNumber;
            return true;
        }

        private void SetPageTitle(PageTransformationInformation pageTransformationInformation, IPage targetPage)
        {
            if (FieldExistsAndIsUsed(pageTransformationInformation, Constants.FileLeafRefField))
            {
                string pageTitle = Path.GetFileNameWithoutExtension((GetFieldValue(pageTransformationInformation, Constants.FileLeafRefField)));
                if (!string.IsNullOrEmpty(pageTitle))
                {
                    pageTitle = pageTitle.First().ToString().ToUpper() + pageTitle.Substring(1);
                    targetPage.PageTitle = pageTitle;
                    LogInfo($"{LogStrings.TransformPageModernTitle} {pageTitle}", LogStrings.Heading_SetPageTitle);
                }
            }
        }

        private IFile Load(PnPContext sourceContext, PageTransformationInformation pageTransformationInformation, string pageType, out IList pagesLibrary, PnPContext targetContext = null)
        {
            sourceContext.Web.EnsureProperty(w => w.ServerRelativeUrl);

            // Load the pages library and page file (if exists) in one go 
            if (GetVersion(sourceClientContext) == SPVersion.SP2010)
            {
                if (IsBlogPage(pageType))
                {
                    pagesLibrary = sourceContext.Web.GetPostsLibrary();
                }
                else
                {
                    pagesLibrary = sourceContext.Web.GetSitePagesLibrary();
                }
            }
            else
            {

                if (IsBlogPage(pageType))
                {
                    var listServerRelativeUrl = UrlUtility.Combine(sourceContext.Web.ServerRelativeUrl, $"lists/{CacheManager.Instance.GetBlogListName(sourceContext)}");
                    pagesLibrary = sourceContext.Web.GetList(listServerRelativeUrl);
                }
                else
                {
                    var listServerRelativeUrl = UrlUtility.Combine(sourceContext.Web.ServerRelativeUrl, "SitePages");
                    pagesLibrary = sourceContext.Web.GetList(listServerRelativeUrl);
                }
            }

            if (pageTransformationInformation.CopyPageMetadata)
            {
                pagesLibrary.LoadBatch(l => l.DefaultViewUrl,l => l.Id,l => l.TemplateType,l => l.OnQuickLaunch,l => l.Title, l => l.Hidden,l => l.RootFolder, l => l.Fields.QueryProperties(f => f.Id, f => f.Title, f => f.Hidden, f => f.InternalName, f => f.DefaultValue, f => f.Required));
            }
            else
            {
                pagesLibrary.LoadBatch(l => l.DefaultViewUrl,l => l.Id,l => l.TemplateType,l => l.OnQuickLaunch,l => l.Title, l => l.Hidden,l => l.RootFolder);
            }

            PnPContext contextForFile = targetClientContext == null ? sourceClientContext : targetClientContext;
            var sitePagesServerRelativeUrl = UrlUtility.Combine(contextForFile.Web.ServerRelativeUrl, "sitepages");

            IFile file = null;
            try
            {
                file = contextForFile.Web.GetFileByServerRelativeUrl($"{sitePagesServerRelativeUrl}/{pageTransformationInformation.Folder}{pageTransformationInformation.TargetPageName}", f => f.ListItemAllFields);
            }
            catch (ServiceException ex) when (ex.IsNotFound())
            {
                // Preserve the missing-page signal expected by the transformation flow.
            }

            if (pageTransformationInformation.KeepPageSpecificPermissions)
            {
                pageTransformationInformation.SourcePage.LoadBatch(p => p.HasUniqueRoleAssignments);
            }

            try
            {
                sourceContext.Execute();
            }
            catch (ServiceException se)
            {
                if (se.IsNotFound())
                {
                    pagesLibrary = null;
                }
                else
                {
                    throw;
                }
            }

            if (pagesLibrary == null)
            {
                LogError(LogStrings.Error_MissingSitePagesLibrary, LogStrings.Heading_Load);
                throw new ArgumentException(LogStrings.Error_MissingSitePagesLibrary);
            }

            if (file == null)
            {
                LogInfo(LogStrings.TransformPageDoesNotExistInWeb, LogStrings.Heading_Load);
                throw new PageDoesNotExistInWebException($"{pageTransformationInformation.TargetPageName} - {LogStrings.TransformPageDoesNotExistInWeb}");
            }

            return file;
        }

        private void ValidateSchema(Stream schema, FileStream stream)
        {
            // Load the template into an XDocument
            XDocument xml = XDocument.Load(stream);

            // Prepare the XML Schema Set
            XmlSchemaSet schemas = new XmlSchemaSet();
            schema.Seek(0, SeekOrigin.Begin);
            schemas.Add(Constants.PageTransformationSchema, new XmlTextReader(schema));

            // Set stream back to start
            stream.Seek(0, SeekOrigin.Begin);

            xml.Validate(schemas, (o, e) =>
            {
                LogError(string.Format(LogStrings.Error_WebPartMappingSchemaValidation, e.Message), LogStrings.Heading_PageTransformationInfomation, e.Exception);
                throw new Exception(string.Format(LogStrings.Error_MappingFileSchemaValidation, e.Message));
            });
        }
        #endregion

    }
}
