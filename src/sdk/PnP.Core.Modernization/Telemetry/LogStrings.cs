namespace PnP.Core.Modernization.Telemetry
{
    /// <summary>
    /// Class holding strings used in log messages
    /// </summary>
    public static class LogStrings
    {
        // Ensure the string variables are meaningful and readable from a developer point of view.
        // Eventually this needs to localised to create multi-lingual outputs.
        // Prefixes
        //      Heading_ for headings
        //      Error_ for error messages
        //      No Prefix - output messages to user

        public const string KeyValueSeperatorToken = ";#;";

        #region Report Text

        public const string Report_ModernisationReport = "Modernization Report";
        public const string Report_ModernisationSummaryReport = "Modernization Summary Report";
        public const string Report_ModernisationPageDetails = "Individual Page details";
        public const string Report_TransformationDetails = "Transformation Details";
        public const string Report_ReportDate = "Report date";
        public const string Report_TransformDuration = "Transform duration";
        public const string Report_TransformationSettings = "Page Transformation Settings";
        public const string Report_Property = "Property";
        public const string Report_Settings = "Setting";
        public const string Report_TransformDetails = "Transformation Operation Details";
        public const string Report_TransformDetailsTableHeader = "Date {0} Operation {0} Actions Performed";

        public const string Report_TransformIssuesTableHeader = "Date {0} Source Page {0} Operation {0} Message";
        public const string Report_ValueNotSet = "<Not Set>";

        public const string Report_ErrorsOccurred = "Errors during transformation";
        public const string Report_ErrorsCriticalOccurred = "Critical Errors during transformation";
        public const string Report_WarningsOccurred = "Warnings during transformation";

        public const string Report_TransformStatus = "Transform Status";
        public const string Report_TransformSuccess = "Successful";
        public const string Report_TransformSuccessWithIssues = "Successful with {0} warnings and {1} non-critical errors";
        public const string Report_TransformFail = "A issue prevented successful transformation";

        #endregion

        #region Page Transformation

        #region Headings

        public const string Heading_PageTransformationInfomation = "Page Transformation Information";
        public const string Heading_Summary = "Summary";
        public const string Heading_InputValidation = "Input Validation";
        public const string Heading_SharePointConnection = "SharePoint Connection";
        public const string Heading_PageCreation = "Page Creation";
        public const string Heading_HomePageHandling = "Home page handling";
        public const string Heading_ArticlePageHandling = "Article page handling";
        public const string Heading_SetPageTitle = "Set Page Title";
        public const string Heading_GetVersion = "Get Version";
        public const string Heading_Load = "Load";
        public const string Heading_RemoveEmptyTextParts = "Remove Empty Text Parts";
        public const string Heading_CopyingPageMetadata = "Copying page metadata";
        public const string Heading_ApplyItemLevelPermissions = "Item level permissions";
        public const string Heading_SwappingPages = "Swapping Pages";
        public const string Heading_GetPrincipal = "Get Principal";
        public const string Heading_TermMapping = "Term Mapping";
        public const string Heading_EnterpriseWikiPreparation = "Preparing Enterprise Wiki capabilities";

        #endregion

        #region Error Messages

        public const string Error_SourcePageNotFound = "Source page cannot be null";
        public const string Error_SourcePageIsModern = "Source page is already a modern page";
        public const string Error_PageNotValidMissingFileRef = "Page is not valid due to missing FileRef or FileLeafRef value";
        public const string Error_BasicASPXPageCannotTransform = "Page is an basic aspx page...can't currently transform that one, sorry!";
        public const string Error_PublishingPagesNotYetSupported = "Page transformation for publishing pages can only be done using the PublishingPageTransformator class";
        public const string Error_DelveBlogPagesNotSupported = "Page transformation for Delve blog pages can only be done using the DelvePageTransformator class";
        public const string Error_PageIsNotAPublishingPage = "Page is not a publishing page, please use the wiki and web part page API's";
        public const string Error_CannotUsePageAcceptBannerCrossSite = "Page transformation towards a different site collection cannot use the page accept banner.";
        public const string Error_OverridingTagePageTakesSourcePageName = "Overriding 'TargetPageTakesSourcePageName' to ensure that the newly created page in the other site collection gets the same name as the source page";
        public const string Error_FallBackToSameSiteTransfer = "Oops, seems source and target point to the same web...switch back the 'source only' mode";
        public const string Error_SameSiteTransferDifferentWeb = "Page transformation between different webs in the same site collection is not supported. Use the source web or a target in a different site collection.";
        public const string Error_SameSiteTransferNoAllowedForPublishingPages = "Oops, seems source and target point to the same site collection...that's a no go for publishing portal page transformation!";
        public const string Error_InPlacePublishingPageDifferentWeb = Error_SameSiteTransferDifferentWeb;
        public const string Error_InPlacePublishingPageSitePagesFeatureNotEnabled = "In-place publishing page transformation cannot continue because the Site Pages feature is not enabled. Enable Site Pages under Site Settings > Manage site features on this web, then retry.";
        public const string Error_InPlacePublishingPageRequiresWritableSitePages = "In-place publishing page transformation requires an existing Site Pages library with Add Items and Edit Items permissions.";
        public const string Error_InPlacePublishingPageSitePagesNotFound = "The Site Pages library required for in-place publishing page transformation was not found.";
        public const string Error_InPlacePublishingPageSitePagesInvalidTemplate = "The Site Pages path does not resolve to a Web Page Library.";
        public const string Error_InPlacePublishingPageSitePagesInsufficientPermissions = "The current user cannot add and edit items in the Site Pages library.";
        public const string Error_InPlacePublishingPageSitePagesProbeUnauthorized = "The Site Pages capability probe was denied.";
        public const string Error_InPlacePublishingPageSitePagesProbeFailed = "The Site Pages capability probe could not determine whether the library is writable; transformation was stopped without creating a target page.";
        public const string Error_InPlacePublishingPageTargetExists = "In-place publishing page transformation cannot overwrite an existing target page.";
        public const string Error_CrossSiteTransferTargetsNonModernSite = "Page transformation for targeting non-modern sites is currently not supported.";
        public const string Error_GetVersionError = "Setting version stamp error";
        public const string Error_MissingSitePagesLibrary = "Site does not have a sitepages library and therefore this page can't be a client side page.";
        public const string Error_BlogPageTransformationHasToBeCrossSite = "Blog pages cannot be transformed in-place, their target site collection must be a different one.";
        public const string Error_SameSiteTransferNoAllowedForBlogPages = "Oops, seems source and target point to the same site collection...that's a no go for blog page transformation!";

        public const string Error_SettingVersionStampError = "Setting version stamp on page error";
        public const string Error_GetPrincipalFailedEnsureUser = "Failed to ensure user exists";
        public const string Error_WebPartMappingSchemaValidation = "Provided custom web part mapping file is invalid: {0}";
        public const string Error_ExtractWebPartPropertiesViaWebServicesFromPage = "Extract Web Part Properties via Web Services from Page failed";
        public const string Error_ExtractWebPartPageViaWebServicesFromPage = "Extract Web Part page via Web Services from Page failed";
        public const string Error_CallingWebServicesToExtractWebPartsFromPage = "Calling Web Services to Extract Web Parts from Page";
        public const string Error_ExportWebPartXmlWorkaround = "Export Web Part Xml from Web Call failed";
        public const string Error_AnalyserCouldNotFindLayouts = "GetAllPageLayouts - Could not search for page layouts";
        public const string Error_AnalyserErrorOccurredExtractMetadata = "An issue occurred with extracting metadata from page layout";
        public const string Error_AnalyserErrorOccurredExtractNamespaces = "An error occurred extracting web part prefixes from namespaces";
        public const string Error_AnalyserErrorOccurredExtractHtmlBlocks = "An error occurred extracting html blocks from page layout html";
        public const string Error_AnalyserCleaningExtractedWebPartFields = "An error occurred cleaning extracted web part fields";
        public const string Error_AnalyserExtractPageHeaderFromPageLayout = "An error occurred extract page header from page layout associated content type";
        public const string Error_ApplyPermissionFailedToApplyPermissions = "Error occurred applying permissions to item {0}";
        public const string CriticalError_ErrorOccurred = "A critical error occurred - transformation did not complete";

        public const string Error_TransformingTaxonomyField = "Error occurred transforming taxonomy field: {0}";
        public const string Error_CommittingTaxonomyField = "Error occurred committing taxonomy field data";

        #endregion

        #region Warning messages

        public const string Warning_NonCriticalErrorDuringVersionStampAndPublish = "Page could not be published as versioning is not enabled or version stamp could not be set.";
        public const string Warning_NonCriticalErrorDuringPublish = "There was a non critical issue during page update/publish. Returned error message: {0}";
        public const string Warning_PostingAPageAsNewsRequiresPagePublishing = "Posting a page as news requires the page to be published. Turning on the PublishCreatedPage flag.";
        public const string Warning_ContextValidationFailWithKeepPermissionsEnabled = "Keep Specific Permissions was set, however this is not currently supported when contexts are cross-farm/tenant - this feature has been disabled.";
        public const string Warning_FieldNotFoundInSourcePage = "You specified field {0} in your page layout mapping, but that field is not available in the source page.";
        public const string Warning_FieldNotFoundInTargetPage = "You specified field {0} in your page layout mapping, but that field is not available in the target page.";
        public const string Warning_PageLayoutsCannotBeDetermined = "Page layout could not be determined by the publishing page.";
        public const string Warning_PageHeaderAuthorNotSet = "The page author header could not be set. Error = {0}.";
        public const string Warning_UserIsNotResolving = "User {0} can't be found, skipped assigning this user. Error: {1}";
        public const string Warning_UserIsNotMappedOrResolving = "User {0} can't be found, skipped assigning this user to field {1}";
        public const string Warning_TransformGetItemPermissionsAccessDenied = "You don't have needed the needed permission level (ManagePermissions) to read/set item level permissions on the target site.";
        #endregion

        #region Status Messages

        public const string ValidationChecksComplete = "Validation checks complete";
        public const string LoadingTargetClientContext = "Loading target client context object";
        public const string LoadingClientContextObjects = "Loading client context objects";
        public const string TransformingSite = "Transforming from site:";
        public const string TransformingPage = "Transforming page:";
        public const string CrossSiteTransferToSite = "Cross-Site transfer mode to site:";
        public const string PageIsLocatedInFolder = "The transform page is located in a folder";
        public const string DetectIfPageIsInFolder = "Detect if the page is living inside a folder";
        public const string NoTargetNameUsingDefaultPrefix = "No target name specified - using a default prefix";
        public const string CrossSiteInUseUsingOriginalFileName = "In Cross-Site transform mode the original source file name is used";
        public const string UsingSuppliedPrefix = "Using the supplied prefix";
        public const string LoadingExistingPageIfExists = "Just try to load the page in the fastest possible manner, we only want to see if the page exists or not";
        public const string CheckPageExistsError = "Checking Page Exists";
        public const string PageAlreadyExistsInTargetLocation = "The page already exists in target location";
        public const string PageNotOverwriteIfExists = "Not overwriting - there already exists a page with name ";
        public const string ModernPageCreated = "Modern page created";
        public const string WelcomePageSettingsIsPresent = "Welcome page setting does exist, checking if the transform page is a home page";
        public const string TransformSourcePageIsHomePage = "The current page is used as a home page - settings modern page to 'Home' layout";
        public const string TransformSourcePageHomePageUsingStock = "Using a stock homepage layout as the new homepage - not transforming page.";
        public const string TransformSourcePageIsNotHomePage = "The current page is not used as the site home page";
        public const string PreparingContentTransformation = "Preparing content transformation";
        public const string TransformSourcePageAsArticlePage = "Transforming source page as Article page";
        public const string TransformArticleSetHeaderToNone = "Page Header Set to None. Removing the page header";
        public const string TransformArticleSetHeaderToNoneWithAuthor = "Setting page author in page header";
        public const string TransformArticleSetHeaderToDefault = "Page Header Set to Default. Using page header default settings.";
        public const string TransformArticleSetHeaderToCustom = "Page Header Set to Custom. Using page header settings:";
        public const string TransformArticleHeaderImageUrl = "Image Url: ";
        public const string TransformSourcePageIsWikiPage = "Recognized source page as a Wiki Page.";
        public const string TransformSourcePageIsPublishingPage = "Recognized source page as a Publishing Page";
        public const string TransformSourcePageIsDelvePage = "Recognized source page as a Delve Blog Page";
        public const string TransformSourcePageAnalysing = "Analyzing web parts and page layouts";
        public const string WikiTextContainsImagesVideosReferences = "Keeping images inline in rich text and handling embedded iframes as separate video web parts";
        public const string TransformSourcePageIsWebPartPage = "Recognized source page as a Web Part Page.";
        public const string TransformPageModernTitle = "Setting the modern page title:";
        public const string TransformPageTitleOverride = "Using specified page title override";
        public const string TransformLayoutTransformatorOverride = "Using layout override for target page";
        public const string TransformAddedPageAcceptBanner = "Added Page Accept Banner web part to be added to the target page";
        public const string TransformUsingContentTransformerOverride = "Using content transformator override";
        public const string TransformingContentStart = "Transforming content";
        public const string TransformingContentEnd = "Transforming content complete";
        public const string TransformRemovingEmptyWebPart = "Removing empty text web part";
        public const string TransformSavedPageInCrossSiteCollection = "Saved page in cross-site collection";
        public const string TransformSavedPage = "Saved page";
        public const string TransformCopyingMetaDataField = "Copying field: ";
        public const string TransformCopyingMetaDataTaxFieldEmpty = "Copying taxonomy field '{0}' skipped because it's empty";
        public const string TransformCopyingMetaDataTaxFieldValue = "Copying taxonomy field value '{0}' skipped because it's not part of the target termset";
        public const string TransformCopyingMetaDataFieldSkipped = "Skipped copying field: ";
        public const string TransformCopyingMetaDataFieldMismatch = "Source or target multi-value setting mis-match";
        public const string TransformCopyingUserMetaDataFieldSkipped = "Skipped copying user field due to a cross farm modernization. Skipped field: ";
        public const string TransformGetItemPermissions = "Item level permissions read";
        public const string TransformCopiedItemPermissions = "Item level permissions copied";
        public const string TransformComplete = "Transformation Complete";
        public const string TransformSwappingPageStep1 = "Step 1 - First rename (move) the source page to a new name";
        public const string TransformSwappingPageUpdateNavigation = "Navigation references found, these have been updated";
        public const string TransformSwappingPageStep2 = "Step 2 - Now rename (move) the created modern page to the original source page, at this point the new page has the same name as the original page had before transformation";
        public const string TransformSwappingPageStep2Path = "Renaming page to";
        public const string TransformSwappingPageStep3 = "Step 3 - Finish with restoring the page navigation: update the navigation links to point back the original page name";
        public const string TransformedPage = "Transformed Page:";
        public const string TransformCheckIfPageIsHomePage = "Check if the transformed page is the web's home page";
        public const string TransformDisablePageComments = "Page commenting is disabled this this page";
        public const string PageLivesOutsideOfALibrary = "Page is loaded from outside a library";
        public const string TransformPageDoesNotExistInWeb = "Page does not exist in current web";
        public const string CallingWebServicesToExtractWebPartPropertiesFromPage = "Calling Web Services to Extract Web Part Properties from Page";
        public const string CallingWebServicesToExtractWebPartPageFromPage = "Calling Web Services to Extract Web Part page from Page";
        public const string CallingWebServicesToExtractWebPartsFromPage = "Calling Web Services to Extract Web Parts from Page";
        public const string RetreivingExportWebPartXmlWorkaround = "Retrieving Web Part using Workaround from Page for Transform";
        public const string PublishMessage = "Published by the page modernization engine";

        public const string AnalyserFoundItems = "Found {0} page layouts";
        public const string AnalyserNoLayoutsFound = "There are no page layouts found to analyse";
        public const string AnalyserMappingLayout = "Generating mapping for `{0}` layout";

        public const string SourceSharePointVersion = "Source SharePoint version: ";
        public const string TransformMode = "Mode: ";
        public const string TransformUsesWebServicesFallback = "Transform using Web Services";
        public const string TransformFallback = "Fallback:";
        public const string TransformationModePublishing = "Publishing Page Transformation Mode";
        public const string TransformationMode = "{0} Transformation";
        public const string AnalysingNoWebPartsFound = "No web parts were found on page";
        public const string Warning_WikiWebPartHtmlPreserved = "Wiki web-part placeholder '{0}' at row {1}, column {2} could not be resolved to a registered web part. Its rendered HTML was preserved as editable text.";
        public const string Warning_WikiWebPartHtmlMissing = "Wiki web-part placeholder '{0}' at row {1}, column {2} could not be resolved to a registered web part and has no rendered HTML to preserve.";
        public const string ContentTransformFoundSourceWebParts = "Source page contains web part `{0}` of type `{1}`";
        public const string WebPartXmlNotExported = "Xml definition for web part {0} was not exportable. Error {1}.";

        #endregion

        #endregion

        #region Content Transformator

        public const string Heading_ContentTransform = "Content Transform";
        public const string Heading_MappingWebParts = "Web Part Mapping";
        public const string Heading_AddingWebPartsToPage = "Adding Web Parts to Target Page";

        public const string ContentUsingAddinWebPart = "Using add-in web part";
        public const string ContentUsing = "Using";
        public const string ContentAdded = "Added";
        public const string ContentModernWebPart = "modern web part";
        public const string ContentWarnModernNotFound = "Modern web part not found";
        public const string ContentTransformationComplete = "Transforming web parts complete";
        public const string ContentClientToTargetPage = "Client Side Web Part to target page";
        public const string ContentTransformingWebParts = "Transforming web parts";
        public const string NothingToTransform = "There is nothing to transform - no web parts found";
        public const string NotTransformingTitleBar = "Not transforming Title Bar - this is not used in modern pages";
        public const string CrossSiteNotSupported = "Skipping this web part's transformation - cross site not supported";
        public const string ContentWebPartBeingTransformed = "Web Part:'{0}' of type '{1}' is being transformed";
        public const string ProcessingSelectorFunctions = "Processing selector functions";
        public const string ProcessingMappingFunctions = "Processing mapping functions";
        public const string ContentWebPartMappingNotFound = "Web Part Mapping not found";
        public const string AddedClientSideTextWebPart = "Added 'Client Side Text Web Part' to target page";
        public const string UsingCustomModernWebPart = "Using 'custom' modern web part ";

        public const string Error_NotValidForTargetSiteCollection = "NotAvailableAtTargetException is used to \"skip\" a web part since it's not valid for the target site collection (only applies to cross site collection transfers)";
        public const string Error_MediaWebpartConfiguration = "The media web part configuration could not be read, skipping this web part from the transformation";
        public const string Error_NoDefaultMappingFound = "No default mapping was found int the provided mapping file";
        public const string Error_AnErrorOccurredFunctions = "An error occurred processing functions";

        public const string Warning_NotSupportedWebPart = "Not Supported, Non Exportable or Unidentified Web Part";

        #endregion

        #region Asset Transfer

        public const string Heading_AssetTransfer = "Asset Transfer";
        public const string Error_AssetTransferClientContextNull = "One or more client context is null";

        public const string AssetTransferredToUrl = "An referenced asset was found and copied to:";
        public const string AssetTransferFailedFallback = "Asset was not transferred. Asset: ";

        public const string Error_AssetTransferCheckingIfAssetExists = "An error occurred checking if a referenced asset exists";
        public const string AssetTransferUploading = "Uploading {0}";
        public const string AssetTransferUploadComplete = "Upload {0} complete";

        #endregion

        #region Function Processor

        public const string Heading_FunctionProcessor = "Function Processor";
        public const string Error_FailedToInitiateCustomFunctionClasses = "Failed to instantiate custom function classes";

        #endregion

        #region Built In Functions

        public const string Heading_BuiltInFunctions = "Built-in Function";

        public const string OverridingQuickLinksDefaults = "Overriding QuickLinks properties via this JSON: {0}";

        public const string Warning_OverridingQuickLinksDefaultsFailed = "Overriding QuickLinks properties failed: {0}";
        public const string Warning_SummaryLinkMissingUrlSkipped = "Summary link '{0}' has a missing, empty or whitespace-only URL and was skipped when creating Quick Links. Correct the source link or use the text mapping to retain its title.";
        public const string Warning_SummaryLinkMissingUrlAsText = "Summary link '{0}' has a missing, empty or whitespace-only URL. Its title is retained as plain text without a hyperlink.";

        public const string Error_ReturnCrossSiteRelativePath = "An error occurred in ReturnCrossSiteRelativePath function";
        public const string Error_DocumentEmbedLookup = "An error occurred in DocumentEmbedLookup function";
        public const string Error_DocumentEmbedLookupFileNotRetrievable = "An error occurred in DocumentEmbedLookup function - file not retrievable";
        public const string Warning_DocumentEmbedLookupFileNotFound = "DocumentEmbedLookup received File Not Found for '{0}' (context: '{1}'). Retaining the document reference with empty metadata so page transformation can continue. Verify the URL, document existence, and the migration identity's access; this response does not establish why the file is unavailable.";
        public const string Error_LoadContentFromFile = "An error occurred in LoadContentFromFile function";

        public const string Error_LoadContentFromFileContentLink = "An error occurred in getting the referenced file in content link";

        public const string Warning_TableCouldNotBeNormalized = "Table could not be normalized and will be taken over as is. Error = {0}";
        public const string Warning_OverlappingTableSpans = "Overlapping table spans were clipped to preserve cell content.";
        #endregion

        #region Publishing Page Transformation

        #region PageLayoutAnalyser

        public const string Heading_PageLayoutAnalyser = "Page Layout Analyser";

        public const string Error_CannotWriteToXmlFile = "Error writing to mapping file: {0} {1}";
        public const string Error_CannotGetSiteCollContext = "Cannot get site collection context";
        public const string Error_CannotMapMetadataFields = "Cannot map the metadata fields from the content types";
        public const string Error_CannotCastToEnum = "An error occurred casting value to enum";
        public const string Error_CannotProcessPageLayoutAnalyseAll = "Error mapping page layout - Analyse All";
        public const string Error_CannotProcessPageLayoutAnalyse = "Error mapping page layout - Analyse";

        public const string OOBPageLayoutSkipped = "Skipped page layout {0} because it's an OOB page layout";

        public const string XmlMappingSavedAs = "Xml Mapping saved as";


        #endregion

        #region PageLayoutManager

        public const string Heading_PageLayoutManager = "Page Layout Manager";

        public const string Error_MappingFileSchemaValidation = "Provided custom pagelayout mapping file is invalid: {0}";
        public const string Error_PageLayoutMappingFileDoesNotExist = "File {0} does not exist";

        public const string CustomPageLayoutMappingFileProvided = "Custom pagelayout mapping file: {0}";
        public const string PageLayoutMappingBeingUsed = "Page uses {1} as page layout, mapping that will be used is {0}";
        public const string PageLayoutMappingGeneration = "Page uses {0} as page layout, no mapping was provided so auto generating a mapping";

        #endregion

        #region PublishingPage

        public const string Heading_PublishingPage = "Publishing Page analyzer";

        public const string Error_NoPageLayoutTransformationModel = "No valid pagelayout transformation model could be retrieved for publishing page layout {0}";
        public const string Warning_CannotRetrieveFieldValue = "Could not retrieve field value from mapping, the contents were empty";
        public const string Warning_SkippedWebPartDueToEmptyInSourcee = "Target web part {0} is not added for field {1} because the field value was empty and the RemoveEmptySectionsAndColumns flag was set";

        #endregion

        #region PublishingPageHeaderTransformator

        public const string Heading_PublishingPageHeader = "Publishing Page header transformation";

        public const string Error_HeaderImageAssetTransferFailed = "Header image {0} could not be transferred to target site";

        public const string SettingHeaderImage = "Header image set to {0}";

        #endregion

        #region PublishingLayoutTransformator

        public const string Heading_PublishingLayoutTransformator = "Publishing Page layout transformation";

        public const string Error_Maximum3ColumnsAllowed = "Publishing transformation layout mapping can maximum use 3 columns";

        #endregion

        #endregion

        #region Url rewriting
        public const string Heading_UrlRewriter = "URL rewriter";

        public const string Error_UrlMappingFileNotFound = "URL mapping file {0} was not found";

        public const string LoadingUrlMappingFile = "Loading URL mapping file {0}";
        public const string UrlMappingLoaded = "Mapping from {0} to {1} loaded";
        public const string UrlRewritten = "ULR rewritten from: {0} to: {1}";
        #endregion

        #region Term Mapping
        
        public const string Term_LoadingMappingFile = "Loading term mapping file {0}";
        public const string Term_MappingLoaded = "Mapping Term from {0} to {1} loaded";
        public const string Warning_TermMappingFailedResolveTarget = "Mapping failed to resolve the target term mapping {0}";
        public const string Warning_TermMappingFailedMapping = "Mapping failed to match the page metadata term with the mapping ID:{0}|{1}";
        #endregion

        #region User Mapping

        public const string Heading_UserMapping = "User Mapping";
        public const string Heading_UserTransform = "User Transform";

        public const string Error_UserMappingFileNotFound = "User mapping file {0} was not found";
        public const string Error_UserMappingValidateExists = "Error occurred ensuring valid user exists: {0}";
        public const string Error_ErrorSearchingDomain = "Error Searching Source Domain For UPN";
        public const string Error_UserTransfomrmCannotResolveDomain = "Error Resolving Friendly Domain To Ldap Domain";

        public const string Warning_MultipleMatchFound = "More than one mapping found, only the first match is used: {0}";
        public const string Warning_UserTransformUserNotOnDomain = "Failed to detect if user is part of the domain, please use mapping instead.";
        public const string Warning_UserTransformCannotValidateUserExists = "Cannot validate user {0} exists";
        public const string Warning_UserTransformCannotGetDomain = "Cannot get current domain";
        public const string Warning_UserTransformCannotUseLDAPConnection = "Cannot use the LDAP Query to connect to domain";

        public const string LoadingUserMappingFile = "Loading User mapping file {0}";
        public const string UserMappingLoaded = "User Mapping Loaded Source: {0} Target: {1}";
        public const string UserTransformSuccess = "Mapping and user replaced: {0} to {1}";
        public const string UserTransformMappingNotFound = "Mapping for user cannot be found: {0}";

        public const string UserTransformPrincipalInput = "Principal Input:{0}";
        public const string UserTransformDefaultMapping = "Default remapping mechanism for user {0}";
        public const string UserTransformRemappedUser = "Remapped user {0} with {1}";
        public const string UserTransformNotRemappedUser = "Not remapping user {0}";

        #endregion

    }
}
