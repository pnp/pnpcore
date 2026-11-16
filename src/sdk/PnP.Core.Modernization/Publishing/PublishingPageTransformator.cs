using Microsoft.SharePoint.Client;
using PnP.Framework.Utilities;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Extensions;
using PnP.Core.Modernization.Pages;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using PnPCore = PnP.Core.Model.SharePoint;

namespace PnP.Core.Modernization.Publishing
{
    /// <summary>
    /// Transforms a classic publishing page into a modern client side page
    /// </summary>
    public class PublishingPageTransformator : BasePageTransformator
    {
        private PublishingPageTransformation publishingPageTransformation;
        private PublishingPageTransformation enterpriseWikiPublishingPageTransformation;
        private PageLayoutManager pageLayoutManager;
        private string publishingPagesLibraryName = null;
        private readonly EnterpriseWikiPagePreparation enterpriseWikiPreparation = new EnterpriseWikiPagePreparation();

        #region Construction
        public PublishingPageTransformator(ClientContext sourceClientContext, ClientContext targetClientContext) : this(sourceClientContext, targetClientContext, "webpartmapping.xml", null)
        {
        }

        public PublishingPageTransformator(ClientContext sourceClientContext, ClientContext targetClientContext, string publishingPageTransformationFile) : this(sourceClientContext, targetClientContext, "webpartmapping.xml", publishingPageTransformationFile)
        {
        }

        public PublishingPageTransformator(ClientContext sourceClientContext, ClientContext targetClientContext, string pageTransformationFile, string publishingPageTransformationFile)
        {
#if DEBUG && MEASURE
            InitMeasurement();
#endif

            this.sourceClientContext = sourceClientContext ?? throw new ArgumentException("sourceClientContext must be provided.");
            this.targetClientContext = targetClientContext ?? throw new ArgumentException("targetClientContext must be provided.");

            this.version = GetVersion();
            this.pageTelemetry = new PageTelemetry(version);
            this.pageLayoutManager = new PageLayoutManager(base.RegisteredLogObservers);

            // Load xml mapping data
            var fileToLoad = pageTransformationFile;
            if (!System.IO.File.Exists(pageTransformationFile))
            {
                fileToLoad = "Modernization" + Path.AltDirectorySeparatorChar + pageTransformationFile;
            }

            XmlSerializer xmlMapping = new XmlSerializer(typeof(PageTransformation));
            using (var stream = new FileStream(fileToLoad, FileMode.Open))
            {
                this.pageTransformation = (PageTransformation)xmlMapping.Deserialize(stream);
            }

            // Load the page layout mapping data            
            this.publishingPageTransformation = this.pageLayoutManager.LoadDefaultPageLayoutMappingFile();
            this.enterpriseWikiPublishingPageTransformation = this.pageLayoutManager.LoadDefaultPageLayoutMappingFile(true);

            if (!string.IsNullOrEmpty(publishingPageTransformationFile))
            {
                // Load page layout mapping from file
                var customPublishingPageTransformation = this.pageLayoutManager.LoadPageLayoutMappingFile(publishingPageTransformationFile);

                // Merge these custom layout mappings with the default ones
                this.publishingPageTransformation = this.pageLayoutManager.MergePageLayoutMappingFiles(this.publishingPageTransformation, customPublishingPageTransformation);
                this.enterpriseWikiPublishingPageTransformation = this.pageLayoutManager.MergePageLayoutMappingFiles(this.enterpriseWikiPublishingPageTransformation, customPublishingPageTransformation);
            }
        }

        public PublishingPageTransformator(ClientContext sourceClientContext, ClientContext targetClientContext, PageTransformation pageTransformationModel, PublishingPageTransformation customPublishingPageTransformationModel)
        {
#if DEBUG && MEASURE
            InitMeasurement();
#endif

            this.sourceClientContext = sourceClientContext ?? throw new ArgumentException("sourceClientContext must be provided.");
            this.targetClientContext = targetClientContext ?? throw new ArgumentException("targetClientContext must be provided.");

            this.version = GetVersion();
            this.pageTelemetry = new PageTelemetry(version);
            this.pageLayoutManager = new PageLayoutManager(base.RegisteredLogObservers);

            this.pageTransformation = pageTransformationModel;

            // Load the page layout mapping data            
            this.publishingPageTransformation = this.pageLayoutManager.LoadDefaultPageLayoutMappingFile();
            this.enterpriseWikiPublishingPageTransformation = this.pageLayoutManager.LoadDefaultPageLayoutMappingFile(true);

            // Merge these custom layout mappings with the default ones
            if (customPublishingPageTransformationModel != null)
            {
                this.publishingPageTransformation = this.pageLayoutManager.MergePageLayoutMappingFiles(this.publishingPageTransformation, customPublishingPageTransformationModel);
                this.enterpriseWikiPublishingPageTransformation = this.pageLayoutManager.MergePageLayoutMappingFiles(this.enterpriseWikiPublishingPageTransformation, customPublishingPageTransformationModel);
            }
        }

        /// <summary>
        /// Creates a transformator that writes the modern page into the Web that holds the publishing page.
        /// </summary>
        /// <param name="sourceClientContext">Client context of the Web holding the publishing page</param>
        /// <returns>A transformator configured for in-place transformation</returns>
        public static PublishingPageTransformator CreateInPlace(ClientContext sourceClientContext)
        {
            return new PublishingPageTransformator(sourceClientContext, sourceClientContext);
        }

        /// <summary>
        /// Creates a transformator that writes the modern page into the Web that holds the publishing page.
        /// </summary>
        /// <param name="sourceClientContext">Client context of the Web holding the publishing page</param>
        /// <param name="publishingPageTransformationFile">Page layout mapping file</param>
        /// <returns>A transformator configured for in-place transformation</returns>
        public static PublishingPageTransformator CreateInPlace(ClientContext sourceClientContext, string publishingPageTransformationFile)
        {
            return new PublishingPageTransformator(sourceClientContext, sourceClientContext, publishingPageTransformationFile);
        }

        /// <summary>
        /// Creates a transformator that writes the modern page into the Web that holds the publishing page.
        /// </summary>
        /// <param name="sourceClientContext">Client context of the Web holding the publishing page</param>
        /// <param name="pageTransformationFile">Web part mapping file</param>
        /// <param name="publishingPageTransformationFile">Page layout mapping file</param>
        /// <returns>A transformator configured for in-place transformation</returns>
        public static PublishingPageTransformator CreateInPlace(ClientContext sourceClientContext, string pageTransformationFile, string publishingPageTransformationFile)
        {
            return new PublishingPageTransformator(sourceClientContext, sourceClientContext, pageTransformationFile, publishingPageTransformationFile);
        }

        /// <summary>
        /// Creates a transformator that writes the modern page into the Web that holds the publishing page.
        /// </summary>
        /// <param name="sourceClientContext">Client context of the Web holding the publishing page</param>
        /// <param name="pageTransformationModel">Web part mapping model</param>
        /// <param name="customPublishingPageTransformationModel">Page layout mapping model</param>
        /// <returns>A transformator configured for in-place transformation</returns>
        public static PublishingPageTransformator CreateInPlace(ClientContext sourceClientContext, PageTransformation pageTransformationModel, PublishingPageTransformation customPublishingPageTransformationModel)
        {
            return new PublishingPageTransformator(sourceClientContext, sourceClientContext, pageTransformationModel, customPublishingPageTransformationModel);
        }
        #endregion

        /// <summary>
        /// Transform the publishing page
        /// </summary>
        /// <param name="publishingPageTransformationInformation">Information about the publishing page to transform</param>
        /// <returns>The path to the created modern page</returns>
        /// <remarks>
        /// In-place transformation requires the Site Pages web feature to be enabled before transformation.
        /// A manually created Web Page Library does not satisfy this prerequisite. The feature is not activated automatically.
        /// </remarks>
        public string Transform(PublishingPageTransformationInformation publishingPageTransformationInformation)
        {
            SetPageId(Guid.NewGuid().ToString());

            var logsForSettings = this.DetailSettingsAsLogEntries(publishingPageTransformationInformation);
            logsForSettings?.ForEach(o => Log(o, LogLevel.Information));

            #region Input validation
            if (publishingPageTransformationInformation.SourcePage == null)
            {
                LogError(LogStrings.Error_SourcePageNotFound, LogStrings.Heading_InputValidation);
                throw new ArgumentNullException(LogStrings.Error_SourcePageNotFound);
            }

            // Validate page and it's eligibility for transformation
            if (!publishingPageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileRefField) || !publishingPageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileLeafRefField))
            {
                LogError(LogStrings.Error_PageNotValidMissingFileRef, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_PageNotValidMissingFileRef);
            }

            string pageType = publishingPageTransformationInformation.SourcePage.PageType();

            if (pageType.Equals("ClientSidePage", StringComparison.InvariantCultureIgnoreCase))
            {
                LogError(LogStrings.Error_SourcePageIsModern, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_SourcePageIsModern);
            }

            if (pageType.Equals("AspxPage", StringComparison.InvariantCultureIgnoreCase))
            {
                LogError(LogStrings.Error_BasicASPXPageCannotTransform, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_BasicASPXPageCannotTransform);
            }

            if (pageType.Equals("WikiPage", StringComparison.InvariantCultureIgnoreCase) || pageType.Equals("WebPartPage", StringComparison.InvariantCultureIgnoreCase))
            {
                LogError(LogStrings.Error_PageIsNotAPublishingPage, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_PageIsNotAPublishingPage);
            }

            if (IsDelveBlogPage(pageType))
            {
                LogError(LogStrings.Error_DelveBlogPagesNotSupported, LogStrings.Heading_InputValidation);
                throw new ArgumentException(LogStrings.Error_DelveBlogPagesNotSupported);
            }

            // Disable cross-farm item level permissions from copying
            CrossFarmTransformationValidation(publishingPageTransformationInformation);

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

                LogDebug(LogStrings.LoadingTargetClientContext, LogStrings.Heading_SharePointConnection);
                LoadClientObject(targetClientContext, true);

                PopulateGlobalProperties(sourceClientContext, targetClientContext);

                var sameSiteCollection = sourceClientContext.Site.Id.Equals(targetClientContext.Site.Id);
                var sourceWebId = sameSiteCollection ? sourceClientContext.Web.EnsureProperty(p => p.Id) : Guid.Empty;
                // Reject unsupported destinations before preparation can change their schema.
                var targetBeforePreparation = PublishingPageTransformationValidator.ValidateTarget(
                    sourceClientContext.Site.Id, targetClientContext.Site.Id, sourceWebId, targetClientContext.Web.Id, true);
                EnsureTransformationTargetAllowed(targetBeforePreparation);
                if (targetBeforePreparation == PublishingPageTransformationTarget.CrossSiteCollection &&
                    targetClientContext.Web.WebTemplate != "SITEPAGEPUBLISHING" && targetClientContext.Web.WebTemplate != "STS" &&
                    targetClientContext.Web.WebTemplate != "GROUP" && targetClientContext.Web.WebTemplate != "BDR" && targetClientContext.Web.WebTemplate != "DEV")
                {
                    LogError(LogStrings.Error_CrossSiteTransferTargetsNonModernSite);
                    throw new ArgumentException(LogStrings.Error_CrossSiteTransferTargetsNonModernSite, LogStrings.Heading_SharePointConnection);
                }

                EnsureSitePagesFeatureEnabled(targetBeforePreparation);
                EnsureEnterpriseWikiCapabilities(publishingPageTransformationInformation);
                var hasWritableSitePages = false;

                if (sameSiteCollection && sourceWebId.Equals(targetClientContext.Web.Id))
                {
                    hasWritableSitePages = HasWritableSitePagesLibrary(sourceClientContext);
                }

                var transformationTarget = PublishingPageTransformationValidator.ValidateTarget(
                    sourceClientContext.Site.Id,
                    targetClientContext.Site.Id,
                    sourceWebId,
                    targetClientContext.Web.Id,
                    hasWritableSitePages);

                EnsureTransformationTargetAllowed(transformationTarget);

                if (transformationTarget == PublishingPageTransformationTarget.SameWeb)
                {
                    // Use the source context for all writes so existing in-place permission, URL and asset semantics apply.
                    targetClientContext = sourceClientContext;
                }

                LogInfo($"{targetClientContext.Web.GetUrl()}", LogStrings.Heading_Summary, LogEntrySignificance.TargetSiteUrl);

                // Ensure PostAsNews is used together with PagePublishing
                if (publishingPageTransformationInformation.PostAsNews && !publishingPageTransformationInformation.PublishCreatedPage)
                {
                    publishingPageTransformationInformation.PublishCreatedPage = true;
                    LogWarning(LogStrings.Warning_PostingAPageAsNewsRequiresPagePublishing, LogStrings.Heading_Summary);
                }

                // Mark this web as publishing web
                CacheManager.Instance.SetPublishingWeb(this.sourceClientContext.Web.GetUrl());

                // Store the information of the source page we do want to retain
                if (publishingPageTransformationInformation.KeepPageCreationModificationInformation)
                {
                    StoreSourcePageInformationToKeep(publishingPageTransformationInformation.SourcePage);
                }

                LogInfo($"{publishingPageTransformationInformation.SourcePage[Constants.FileRefField].ToString()}", LogStrings.Heading_Summary, LogEntrySignificance.SourcePage);

                var spVersion = publishingPageTransformationInformation.SourceVersion;
                var exactSpVersion = publishingPageTransformationInformation.SourceVersionNumber;
                LogInfo($"{spVersion.DisplaySharePointVersion()} ({exactSpVersion})", LogStrings.Heading_Summary, LogEntrySignificance.SharePointVersion);
                LogInfo(LogStrings.TransformationModePublishing, LogStrings.Heading_Summary, LogEntrySignificance.TransformMode);

                //Load User Mapping File
                InitializeUserMapping(publishingPageTransformationInformation);

#if DEBUG && MEASURE
            Stop("Telemetry");
#endif
                #endregion

                #region Page creation
                // Detect if the page is living inside a folder
                LogDebug(LogStrings.DetectIfPageIsInFolder, LogStrings.Heading_PageCreation);
                string pageFolder = "";

                // Get the publishing pages library name
                this.publishingPagesLibraryName = CacheManager.Instance.GetPublishingPagesLibraryName(this.sourceClientContext);

                if (publishingPageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileDirRefField))
                {
                    var fileRefFieldValue = publishingPageTransformationInformation.SourcePage[Constants.FileDirRefField].ToString();

                    if (fileRefFieldValue.ContainsIgnoringCasing($"/{this.publishingPagesLibraryName}"))
                    {
                        string pagesLibraryRelativeUrl = $"{sourceClientContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/{this.publishingPagesLibraryName}";
                        pageFolder = fileRefFieldValue.Replace(pagesLibraryRelativeUrl, "", StringComparison.InvariantCultureIgnoreCase).Trim();
                    }
                    else
                    {
                        // Page was living in another list, leave the list name as that will be the folder hosting the modern file in SitePages.
                        // This convention is used to avoid naming conflicts
                        pageFolder = fileRefFieldValue.Replace($"{sourceClientContext.Web.ServerRelativeUrl}", "").Trim();
                    }

                    if (pageFolder.Length > 0 || !string.IsNullOrEmpty(publishingPageTransformationInformation.TargetPageFolder))
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

                        if (!string.IsNullOrEmpty(publishingPageTransformationInformation.TargetPageFolder))
                        {
                            // Handle special case <root> to indicate page should be created in the root folder
                            if (publishingPageTransformationInformation.TargetPageFolder.Equals("<root>", StringComparison.InvariantCultureIgnoreCase))
                            {
                                publishingPageTransformationInformation.TargetPageFolder = "";
                            }

                            if (publishingPageTransformationInformation.TargetPageFolderOverridesDefaultFolder)
                            {
                                pageFolder = publishingPageTransformationInformation.TargetPageFolder;
                            }
                            else
                            {
                                pageFolder = Path.Combine(pageFolder, publishingPageTransformationInformation.TargetPageFolder);
                            }

                            if (!pageFolder.EndsWith("/"))
                            {
                                // Add a trailing slash
                                pageFolder = pageFolder + "/";
                            }
                        }

                        LogInfo(LogStrings.PageIsLocatedInFolder, LogStrings.Heading_PageCreation);
                    }
                }
                publishingPageTransformationInformation.Folder = pageFolder;

                // If no targetname specified then we'll come up with one
                if (string.IsNullOrEmpty(publishingPageTransformationInformation.TargetPageName))
                {
                    LogInfo(LogStrings.CrossSiteInUseUsingOriginalFileName, LogStrings.Heading_PageCreation);
                    publishingPageTransformationInformation.TargetPageName = $"{publishingPageTransformationInformation.SourcePage[Constants.FileLeafRefField].ToString()}";
                }

                // Canonicalize the target page name once, before it is used to probe for a collision, to save the
                // page and to read the saved page back. Saving appends the .aspx extension when it is missing, so a
                // caller supplied name without the extension would otherwise probe and read back a different file
                // than the one that is written, and an existing target page could be overwritten unnoticed.
                publishingPageTransformationInformation.TargetPageName = EnsureAspxExtension(publishingPageTransformationInformation.TargetPageName);

                // Check if page name is free to use
#if DEBUG && MEASURE
            Start();
#endif
                PnPCore.IPage targetPage = null;
                List pagesLibrary = null;
                Microsoft.SharePoint.Client.File existingFile = null;

                //The determines of the target client context has been specified and use that to generate the target page
                var context = targetClientContext;

                targetPage = CreateTargetPageAfterCollisionProbe(
                    transformationTarget,
                    publishingPageTransformationInformation,
                    () => Load(sourceClientContext, targetClientContext, publishingPageTransformationInformation, out pagesLibrary),
                    pageName => context.Web.AddClientSidePage(pageName),
                    out existingFile);

#if DEBUG && MEASURE
            Stop("Load Page");
#endif

                LogInfo($"{LogStrings.ModernPageCreated} ", LogStrings.Heading_PageCreation);
                #endregion

                LogInfo(LogStrings.TransformSourcePageAsArticlePage, LogStrings.Heading_ArticlePageHandling);

                #region Analysis of the source page
#if DEBUG && MEASURE
                Start();
#endif
                // Analyze the source page
                Tuple<Pages.PageLayout, List<WebPartEntity>> pageData = null;

                LogInfo($"{LogStrings.TransformSourcePageIsPublishingPage} - {LogStrings.TransformSourcePageAnalysing}", LogStrings.Heading_ArticlePageHandling);

                // Grab the pagelayout mapping to use:
                var publishingModel = GetPublishingModel(publishingPageTransformationInformation);
                var pageLayoutMappingModel = new PageLayoutManager(this.RegisteredLogObservers).GetPageLayoutMappingModel(publishingModel, publishingPageTransformationInformation.SourcePage);
                var enterpriseWikiRatings = PrepareEnterpriseWikiRatings(publishingPageTransformationInformation, pageLayoutMappingModel, sameSiteCollection);
               
                if (spVersion == SPVersion.SP2010 || spVersion == SPVersion.SP2013Legacy || spVersion == SPVersion.SP2016Legacy)
                {
                    pageData = new PublishingPageOnPremises(publishingPageTransformationInformation.SourcePage, pageTransformation, publishingModel, publishingPageTransformationInformation as BaseTransformationInformation, targetContext: targetClientContext, logObservers: base.RegisteredLogObservers).Analyze(pageLayoutMappingModel);
                }
                else
                {
                    pageData = new PublishingPage(publishingPageTransformationInformation.SourcePage, pageTransformation, publishingModel, publishingPageTransformationInformation as BaseTransformationInformation, targetContext: targetClientContext, logObservers: base.RegisteredLogObservers).Analyze(pageLayoutMappingModel);
                }


                // Images stay inline; embedded iframes are handled as separate video web parts.
                LogInfo(LogStrings.WikiTextContainsImagesVideosReferences, LogStrings.Heading_ArticlePageHandling);
                pageData = new Tuple<Pages.PageLayout, List<WebPartEntity>>(pageData.Item1, new WikiHtmlTransformator(this.sourceClientContext, this.targetClientContext, targetPage, publishingPageTransformationInformation as BaseTransformationInformation, base.RegisteredLogObservers).TransformPlusSplit(pageData.Item2, publishingPageTransformationInformation.HandleWikiImagesAndVideos, publishingPageTransformationInformation.AddTableListImageAsImageWebPart));

#if DEBUG && MEASURE
                Stop("Analyze page");
#endif
                #endregion

                #region Page title configuration
#if DEBUG && MEASURE
                Start();
#endif
                // Set page title
                SetPageTitle(publishingPageTransformationInformation, targetPage);

                if (publishingPageTransformationInformation.PageTitleOverride != null)
                {
                    var title = publishingPageTransformationInformation.PageTitleOverride(targetPage.PageTitle);
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
                bool useCustomLayoutTransformator = false;
                if (publishingPageTransformationInformation.LayoutTransformatorOverride != null)
                {
                    LogInfo(LogStrings.TransformLayoutTransformatorOverride, LogStrings.Heading_ArticlePageHandling);
                    layoutTransformator = publishingPageTransformationInformation.LayoutTransformatorOverride(targetPage);
                    useCustomLayoutTransformator = true;
                }

                // Apply the layout to the page
                layoutTransformator.Transform(pageData);

                // If needed call the specific publishing page layout transformator
                if ((pageData.Item1 == Pages.PageLayout.PublishingPage_AutoDetect || pageData.Item1 == Pages.PageLayout.PublishingPage_AutoDetectWithVerticalColumn || pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnRightVerticalSection || pageData.Item1 == Pages.PageLayout.PublishingPage_TwoColumnLeftVerticalSection) && !useCustomLayoutTransformator)
                {
                    // Call out the specific publishing layout transformator implementation
                    PublishingLayoutTransformator publishingLayoutTransformator = new PublishingLayoutTransformator(targetPage, pageLayoutMappingModel, base.RegisteredLogObservers);
                    publishingLayoutTransformator.Transform(pageData);
                }

#if DEBUG && MEASURE
                Stop("Page layout");
#endif
                #endregion

                #region Content transformation

                LogDebug(LogStrings.PreparingContentTransformation, LogStrings.Heading_ArticlePageHandling);

#if DEBUG && MEASURE
                Start();
#endif
                // Use the default content transformator
                IContentTransformator contentTransformator = new ContentTransformator(sourceClientContext, targetClientContext, targetPage, pageTransformation, publishingPageTransformationInformation as BaseTransformationInformation, base.RegisteredLogObservers);

                // Do we have an override?
                if (publishingPageTransformationInformation.ContentTransformatorOverride != null)
                {
                    LogInfo(LogStrings.TransformUsingContentTransformerOverride, LogStrings.Heading_ArticlePageHandling);

                    contentTransformator = publishingPageTransformationInformation.ContentTransformatorOverride(targetPage, pageTransformation);
                }

                LogInfo(LogStrings.TransformingContentStart, LogStrings.Heading_ArticlePageHandling);

                // Run the content transformator
                contentTransformator.Transform(pageData.Item2.Where(c => !c.IsClosed).ToList());

                LogInfo(LogStrings.TransformingContentEnd, LogStrings.Heading_ArticlePageHandling);
#if DEBUG && MEASURE
                Stop("Content transformation");
#endif
                #endregion

                #region Configure header for target page
#if DEBUG && MEASURE
                Start();
#endif
                PublishingPageHeaderTransformator headerTransformator = new PublishingPageHeaderTransformator(publishingPageTransformationInformation, sourceClientContext, targetClientContext, publishingModel, base.RegisteredLogObservers);
                headerTransformator.TransformHeader(ref targetPage);

#if DEBUG && MEASURE
                Stop("Target page header");
#endif
                #endregion

                #region Text/Section/Column cleanup
                // Drop "empty" text parts. Wiki pages tend to have a lot of text parts just containing div's and BR's...no point in keep those as they generate to much whitespace
                RemoveEmptyTextParts(targetPage);

                // Remove empty sections and columns to optimize screen real estate
                if (publishingPageTransformationInformation.RemoveEmptySectionsAndColumns)
                {
                    RemoveEmptySectionsAndColumns(targetPage);
                }
                #endregion

                #region Page persisting + permissions

                if (publishingPageTransformationInformation.ShouldIncludeEnterpriseWikiSidebar)
                {
                    EnterpriseWikiSidebar.Ensure(targetPage);
                }

                #region Save the page
#if DEBUG && MEASURE
            Start();
#endif
                // Persist the client side page
                var pageName = $"{publishingPageTransformationInformation.Folder}{publishingPageTransformationInformation.TargetPageName}";
                SavePage(targetPage, pageName, publishingPageTransformationInformation, context);
                LogInfo($"{LogStrings.TransformSavedPageInCrossSiteCollection}: {pageName}", LogStrings.Heading_ArticlePageHandling);

                // Load the page list item
                string pageNameToLoad = pageName;
                if (pageNameToLoad.StartsWith("/"))
                {
                    pageNameToLoad = pageNameToLoad.Substring(1);
                }
                var savedTargetPage = targetClientContext.Web.GetFileByServerRelativePath(ResourcePath.FromDecodedUrl($"{targetPage.PagesLibrary.RootFolder.ServerRelativeUrl}/{pageNameToLoad}"));
                targetClientContext.Web.Context.Load(savedTargetPage, p => p.ListItemAllFields);
                targetClientContext.Web.Context.ExecuteQueryRetry();

#if DEBUG && MEASURE
            Stop("Persist page");
#endif
                #endregion

                #region Page metadata handling
                PublishingMetadataTransformator publishingMetadataTransformator = 
                    new PublishingMetadataTransformator(publishingPageTransformationInformation, sourceClientContext, targetClientContext, savedTargetPage, targetPage, 
                        pageLayoutMappingModel, publishingModel, this.userTransformator, base.RegisteredLogObservers);

                publishingMetadataTransformator.Transform();
                enterpriseWikiRatings?.CopyTo(savedTargetPage.ListItemAllFields);
                #endregion

                #region Permission handling
                ListItemPermission listItemPermissionsToKeep = null;
                if (publishingPageTransformationInformation.KeepPageSpecificPermissions)
                {
#if DEBUG && MEASURE
                Start();
#endif
                    // Check if we do have item level permissions we want to take over
                    var usesCrossSitePermissionSemantics = PublishingPageTransformationValidator.UsesCrossSitePermissionSemantics(transformationTarget);
                    listItemPermissionsToKeep = GetItemLevelPermissions(usesCrossSitePermissionSemantics, pagesLibrary, publishingPageTransformationInformation.SourcePage, savedTargetPage.ListItemAllFields);

                    ApplyItemLevelPermissions(usesCrossSitePermissionSemantics, savedTargetPage.ListItemAllFields, listItemPermissionsToKeep);
#if DEBUG && MEASURE
                Stop("Permission handling");
#endif
                }
                #endregion

                #region Page Publishing
                // Tag the file with a page modernization version stamp
                string serverRelativePathForModernPage = savedTargetPage.ListItemAllFields[Constants.FileRefField].ToString();
                bool pageListItemWasReloaded = false;
                try
                {
                    var targetPageFile = context.Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
                    context.Load(targetPageFile, p => p.Properties);
                    targetPageFile.Properties["sharepointpnp_pagemodernization"] = this.version;
                    targetPageFile.Update();

                    // In case of KeepPageCreationModificationInformation and PostAsNews the publishing is handled at the very end of the flow, so skip it right here
                    if (!publishingPageTransformationInformation.KeepPageCreationModificationInformation && 
                        !publishingPageTransformationInformation.PostAsNews && 
                        publishingPageTransformationInformation.PublishCreatedPage)
                    {
                        // Try to publish, if publish is not needed/possible (e.g. when no minor/major versioning set) then this will return an error that we'll be ignoring
                        targetPageFile.Publish(LogStrings.PublishMessage);
                    }

                    // Ensure we've the most recent page list item loaded, must be last statement before calling ExecuteQuery
                    context.Load(savedTargetPage.ListItemAllFields);
                    // Send both the property update and publish as a single operation to SharePoint
                    context.ExecuteQueryRetry();
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
                        context.Load(savedTargetPage.ListItemAllFields);
                        context.ExecuteQueryRetry();
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
                        context.Load(savedTargetPage.ListItemAllFields);
                        context.ExecuteQueryRetry();
                    }
                }
                catch (Exception)
                {
                    // Eat any exception
                }

                // Disable page comments on the create page, if needed
                if (publishingPageTransformationInformation.DisablePageComments)
                {
                    targetPage.DisableComments();
                    LogInfo(LogStrings.TransformDisablePageComments, LogStrings.Heading_ArticlePageHandling);
                }
                #endregion

                #region Restore page author/editor/created/modified
                if ((publishingPageTransformationInformation.KeepPageCreationModificationInformation && this.SourcePageAuthor != null && this.SourcePageEditor != null) || 
                    publishingPageTransformationInformation.PostAsNews)
                {
                    UpdateTargetPageWithSourcePageInformation(savedTargetPage.ListItemAllFields, publishingPageTransformationInformation, serverRelativePathForModernPage, true);
                }
                #endregion

                // NO page updates are allowed anymore past this point as otherwise the set page usage information and published/posted state will be impacted!

                #region Telemetry
                if (!publishingPageTransformationInformation.SkipTelemetry && this.pageTelemetry != null)
                {
                    TimeSpan duration = DateTime.Now.Subtract(transformationStartDateTime);
                    this.pageTelemetry.LogTransformationDone(duration, pageType, publishingPageTransformationInformation);
                    this.pageTelemetry.Flush();
                }

                LogInfo(LogStrings.TransformComplete, LogStrings.Heading_PageCreation);
                #endregion

                #region Closing
                CacheManager.Instance.SetLastUsedTransformator(this);
                LogInfo($"{serverRelativePathForModernPage}", LogStrings.Heading_Summary, LogEntrySignificance.TargetPage);
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

        /// <summary>
        /// Loads the default page layout mapping model file
        /// </summary>
        /// <returns></returns>
        public static string LoadDefaultPageLayoutMappingFile()
        {
            return LoadFile("PnP.Core.Modernization.Publishing.pagelayoutmapping.xml");
        }

        #region Helper methods
        internal PublishingPageTransformation GetPublishingModel(PublishingPageTransformationInformation information)
        {
            return information.EnableEnterpriseWikiCapabilities ? enterpriseWikiPublishingPageTransformation : publishingPageTransformation;
        }

        private void EnsureEnterpriseWikiCapabilities(PublishingPageTransformationInformation information)
        {
            information.IsEnterpriseWikiPage = false;
            if (!information.EnableEnterpriseWikiCapabilities)
            {
                return;
            }
            string contentTypeId;
            if (information.SourcePage.FieldValues.TryGetValue(Constants.ContentTypeIdField, out var contentTypeValue) &&
                contentTypeValue != null)
            {
                contentTypeId = contentTypeValue.ToString();
            }
            else
            {
                var contentType = information.SourcePage.EnsureProperty(p => p.ContentType);
                contentTypeId = contentType.EnsureProperty(c => c.Id).StringValue;
            }
            var isEnterpriseWikiPage = EnterpriseWikiPagePreparation.IsEnterpriseWikiPage(
                contentTypeId, information.SourcePage.PageLayoutFile());
            information.IsEnterpriseWikiPage = isEnterpriseWikiPage;

            try
            {
                enterpriseWikiPreparation.EnsureReady(
                    targetClientContext.Site.Id, targetClientContext.Web.Id, targetClientContext.Web.WebTemplate,
                    isEnterpriseWikiPage, information.EnableEnterpriseWikiCapabilities, () =>
                    {
                        LogInfo($"Preparing destination web '{targetClientContext.Url}'.", LogStrings.Heading_EnterpriseWikiPreparation);
                        EnterpriseWikiPagePreparation.Prepare(targetClientContext);
                        LogInfo("Enterprise Wiki SitePages prerequisites verified.", LogStrings.Heading_EnterpriseWikiPreparation);
                    });
            }
            catch (Exception ex)
            {
                LogError($"Enterprise Wiki preparation failed for '{targetClientContext.Url}'.",
                    LogStrings.Heading_EnterpriseWikiPreparation, ex);
                throw;
            }
        }

        private EnterpriseWikiRatings PrepareEnterpriseWikiRatings(PublishingPageTransformationInformation information, PageLayout layout, bool sameSiteCollection)
        {
            if (!information.EnableEnterpriseWikiCapabilities || !information.IsEnterpriseWikiPage || !EnterpriseWikiRatings.UsesSameNameMapping(layout))
            {
                return null;
            }
            var ratings = EnterpriseWikiRatings.Read(information.SourcePage);
            if (sameSiteCollection || ratings.Count == 0)
            {
                return ratings;
            }
            using (var sourceContext = sourceClientContext.Clone(sourceClientContext.Url))
            {
                var users = ratings.Voters.ToDictionary(v => v.LookupId, v => sourceContext.Web.SiteUsers.GetById(v.LookupId));
                foreach (var user in users.Values)
                {
                    sourceContext.Load(user, u => u.LoginName);
                }
                sourceContext.ExecuteQueryRetry();
                return ratings.MapVoters(voter =>
                {
                    var principal = userTransformator.RemapPrincipal(users[voter.LookupId].LoginName);
                    if (string.IsNullOrEmpty(principal))
                    {
                        throw new InvalidOperationException("An Enterprise Wiki voter could not be mapped to the destination.");
                    }
                    var user = CacheManager.Instance.GetEnsuredUser(targetClientContext, principal);
                    if (user == null)
                    {
                        throw new InvalidOperationException("An Enterprise Wiki voter could not be resolved on the destination.");
                    }
                    return new FieldUserValue { LookupId = user.Id };
                });
            }
        }

        /// <summary>
        /// Production seam that keeps the collision probe and the first target-page operation in one fail-closed flow.
        /// </summary>
        internal PnPCore.IPage CreateTargetPageAfterCollisionProbe(
            PublishingPageTransformationTarget transformationTarget,
            PublishingPageTransformationInformation publishingPageTransformationInformation,
            Func<Microsoft.SharePoint.Client.File> targetPageProbe,
            Func<string, PnPCore.IPage> addClientSidePage,
            out Microsoft.SharePoint.Client.File existingFile)
        {
            // Keep the first target operation fail-closed even if a future caller bypasses the earlier routing guard.
            EnsureTransformationTargetAllowed(transformationTarget);

            var pageExists = false;
            existingFile = null;

            try
            {
                LogDebug(LogStrings.LoadingExistingPageIfExists, LogStrings.Heading_PageCreation);

                // A successful probe means the target name is already occupied, even if the caller does not need the file object.
                existingFile = targetPageProbe();
                pageExists = true;
            }
            catch (Exception ex)
            {
                // Match the "page not found" signal structurally. LogStrings are English-only constants that are
                // intended to be localized, so matching on the message text would silently break this routing.
                var targetPageDoesNotExist = ex is PageDoesNotExistInWebException;

                if (targetPageDoesNotExist)
                {
                    LogInfo(LogStrings.CheckPageExistsError, LogStrings.Heading_PageCreation);
                }
                else if (transformationTarget == PublishingPageTransformationTarget.SameWeb)
                {
                    // Stop in-place transformation when the target collision probe is inconclusive.
                    throw;
                }
                else
                {
                    // Preserve the existing default cross-site behavior.
                    LogError(LogStrings.CheckPageExistsError, LogStrings.Heading_PageCreation, ex);
                }
            }

            if (pageExists)
            {
                LogInfo(LogStrings.PageAlreadyExistsInTargetLocation, LogStrings.Heading_PageCreation);

                if (!PublishingPageTransformationValidator.CanOverwriteTarget(transformationTarget, publishingPageTransformationInformation.Overwrite))
                {
                    var message = transformationTarget == PublishingPageTransformationTarget.SameWeb
                        ? $"{LogStrings.Error_InPlacePublishingPageTargetExists} {publishingPageTransformationInformation.TargetPageName}."
                        : $"{LogStrings.PageNotOverwriteIfExists}  {publishingPageTransformationInformation.TargetPageName}.";
                    LogError(message, LogStrings.Heading_PageCreation);
                    throw new ArgumentException(message);
                }
            }

            return addClientSidePage($"{publishingPageTransformationInformation.Folder}{publishingPageTransformationInformation.TargetPageName}");
        }

        private void EnsureTransformationTargetAllowed(PublishingPageTransformationTarget transformationTarget)
        {
            switch (transformationTarget)
            {
                case PublishingPageTransformationTarget.SameSiteCollectionDifferentWeb:
                    LogError(LogStrings.Error_SameSiteTransferDifferentWeb, LogStrings.Heading_SharePointConnection);
                    throw new ArgumentException(LogStrings.Error_SameSiteTransferDifferentWeb);
                case PublishingPageTransformationTarget.SameWebRequiresWritableSitePages:
                    LogError(LogStrings.Error_InPlacePublishingPageRequiresWritableSitePages, LogStrings.Heading_SharePointConnection);
                    throw new ArgumentException(LogStrings.Error_InPlacePublishingPageRequiresWritableSitePages);
            }
        }

        internal void EnsureSitePagesFeatureEnabled(PublishingPageTransformationTarget transformationTarget)
        {
            if (transformationTarget != PublishingPageTransformationTarget.SameWeb)
            {
                return;
            }

            // Reload the collection: a reused context must not retain a feature that was deactivated.
            var features = sourceClientContext.Web.Features;
            features.ClearObjectData();
            sourceClientContext.Load(features, collection => collection.Include(feature => feature.DefinitionId));
            sourceClientContext.ExecuteQueryRetry();

            if (!features.Any(feature => feature.DefinitionId == Constants.FeatureId_Web_ModernPage))
            {
                var message = $"{LogStrings.Error_InPlacePublishingPageSitePagesFeatureNotEnabled} Web: {sourceClientContext.Url}; Feature ID: {Constants.FeatureId_Web_ModernPage}.";
                LogError(message, LogStrings.Heading_SharePointConnection);
                throw new InvalidOperationException(message);
            }
        }

        private bool HasWritableSitePagesLibrary(ClientContext context)
        {
            var sitePagesServerRelativeUrl = UrlUtility.Combine(context.Web.ServerRelativeUrl, "SitePages");

            try
            {
                var sitePagesLibrary = context.Web.GetList(sitePagesServerRelativeUrl);
                context.Load(sitePagesLibrary,
                    p => p.Id,
                    p => p.BaseTemplate,
                    p => p.EffectiveBasePermissions);
                context.ExecuteQueryRetry();

                if (sitePagesLibrary.Id == Guid.Empty)
                {
                    LogError($"{LogStrings.Error_InPlacePublishingPageSitePagesNotFound} Path: {sitePagesServerRelativeUrl}.", LogStrings.Heading_SharePointConnection);
                    return false;
                }

                if (sitePagesLibrary.BaseTemplate != (int)ListTemplateType.WebPageLibrary)
                {
                    LogError($"{LogStrings.Error_InPlacePublishingPageSitePagesInvalidTemplate} Path: {sitePagesServerRelativeUrl}; BaseTemplate: {sitePagesLibrary.BaseTemplate}.", LogStrings.Heading_SharePointConnection);
                    return false;
                }

                var canAdd = sitePagesLibrary.EffectiveBasePermissions.Has(PermissionKind.AddListItems);
                var canEdit = sitePagesLibrary.EffectiveBasePermissions.Has(PermissionKind.EditListItems);
                if (!canAdd || !canEdit)
                {
                    LogError($"{LogStrings.Error_InPlacePublishingPageSitePagesInsufficientPermissions} Path: {sitePagesServerRelativeUrl}; AddListItems: {canAdd}; EditListItems: {canEdit}.", LogStrings.Heading_SharePointConnection);
                    return false;
                }

                return true;
            }
            catch (ServerUnauthorizedAccessException ex)
            {
                LogError($"{LogStrings.Error_InPlacePublishingPageSitePagesProbeUnauthorized} Path: {sitePagesServerRelativeUrl}; {ex.Message}", LogStrings.Heading_SharePointConnection);
                return false;
            }
            catch (ServerException ex) when (string.Equals(ex.ServerErrorTypeName, "System.IO.FileNotFoundException", StringComparison.Ordinal))
            {
                LogError($"{LogStrings.Error_InPlacePublishingPageSitePagesNotFound} Path: {sitePagesServerRelativeUrl}; {ex.Message}", LogStrings.Heading_SharePointConnection);
                return false;
            }
            catch (Exception ex)
            {
                var message = $"{LogStrings.Error_InPlacePublishingPageSitePagesProbeFailed} Path: {sitePagesServerRelativeUrl}; {ex.Message}";
                LogError(message, LogStrings.Heading_SharePointConnection);
                throw new InvalidOperationException(message, ex);
            }
        }

        private void SetPageTitle(PublishingPageTransformationInformation publishingPageTransformationInformation, PnPCore.IPage targetPage)
        {
            string titleValue = "";
            if (publishingPageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.TitleField))
            {
                titleValue = publishingPageTransformationInformation.SourcePage[Constants.TitleField].ToString();
            }
            else if (publishingPageTransformationInformation.SourcePage.FieldExistsAndUsed(Constants.FileLeafRefField))
            {
                titleValue = Path.GetFileNameWithoutExtension((publishingPageTransformationInformation.SourcePage[Constants.FileLeafRefField].ToString()));
            }

            if (!string.IsNullOrEmpty(titleValue))
            {
                titleValue = titleValue.First().ToString().ToUpper() + titleValue.Substring(1);
                targetPage.PageTitle = titleValue;
                LogInfo($"{LogStrings.TransformPageModernTitle} {titleValue}", LogStrings.Heading_SetPageTitle);
            }
        }

        private Microsoft.SharePoint.Client.File Load(ClientContext sourceContext, ClientContext targetContext, PublishingPageTransformationInformation publishingPageTransformationInformation, out List pagesLibrary)
        {
            sourceContext.Web.EnsureProperty(w => w.ServerRelativeUrl);
            sourceContext.Site.EnsureProperty(w => w.Url); 
            targetContext.Web.EnsureProperty(w => w.ServerRelativeUrl);

            // Load the pages library and page file (if exists) in one go 
            pagesLibrary = sourceContext.Web.GetListById(Guid.Parse(sourceContext.Web.GetPagesLibraryId())); 

            sourceContext.Web.Context.Load(pagesLibrary, l => l.DefaultViewUrl, l => l.Id, l => l.BaseTemplate, l => l.OnQuickLaunch, l => l.DefaultViewUrl, l => l.Title,
                                              l => l.Hidden, l => l.EffectiveBasePermissions, l => l.RootFolder, l => l.RootFolder.ServerRelativeUrl);

            if (publishingPageTransformationInformation.KeepPageSpecificPermissions)
            {
                sourceContext.Load(publishingPageTransformationInformation.SourcePage, p => p.HasUniqueRoleAssignments);
            }
            try
            {
                sourceClientContext.ExecuteQueryRetry();
            }
            catch (ServerException se)
            {
                if (se.ServerErrorTypeName == "System.IO.FileNotFoundException")
                {
                    pagesLibrary = null;
                }
                else
                {
                    throw;
                }
            }

            var sitePagesServerRelativeUrl = UrlUtility.Combine(targetClientContext.Web.ServerRelativeUrl, "sitepages");
            var file = targetClientContext.Web.GetFileByServerRelativeUrl($"{sitePagesServerRelativeUrl}/{publishingPageTransformationInformation.Folder}{publishingPageTransformationInformation.TargetPageName}");
            targetClientContext.Web.Context.Load(file, f => f.Exists, f => f.ListItemAllFields);
            targetClientContext.ExecuteQueryRetry();

            if (pagesLibrary == null)
            {
                LogError(LogStrings.Error_MissingSitePagesLibrary, LogStrings.Heading_Load);
                throw new ArgumentNullException(LogStrings.Error_MissingSitePagesLibrary);
            }

            if (!file.Exists)
            {
                LogInfo(LogStrings.TransformPageDoesNotExistInWeb, LogStrings.Heading_Load); //Not an error this is a check
                throw new PageDoesNotExistInWebException($"{publishingPageTransformationInformation.TargetPageName} - {LogStrings.TransformPageDoesNotExistInWeb}");
            }

            return file;
        }

        /// <summary>
        /// Use reflection to read the object properties and detail the values
        /// </summary>
        /// <param name="pti">PageTransformationInformation object</param>
        /// <returns></returns>
        private List<LogEntry> DetailSettingsAsLogEntries(PublishingPageTransformationInformation pti)
        {
            List<LogEntry> logs = new List<LogEntry>();

            try
            {
                // Add version 
                logs.Add(new LogEntry()
                {
                    Heading = LogStrings.Heading_PageTransformationInfomation,
                    Message = $"Engine version {LogStrings.KeyValueSeperatorToken} {this.version ?? "Not Specified"}"
                });

                var properties = pti.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(String) ||
                        property.PropertyType == typeof(bool))
                    {
                        var propVal = property.GetValue(pti);

                        logs.Add(new LogEntry()
                        {
                            Heading = LogStrings.Heading_PageTransformationInfomation,
                            Message = $"{property.Name.FormatAsFriendlyTitle()} {LogStrings.KeyValueSeperatorToken} {propVal ?? "Not Specified"}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logs.Add(new LogEntry() { Message = "Failed to convert object properties for reporting", Exception = ex, Heading = LogStrings.Heading_PageTransformationInfomation });
            }

            return logs;

        }
        #endregion

    }
}
