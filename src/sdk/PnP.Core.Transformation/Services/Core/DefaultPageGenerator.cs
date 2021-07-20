using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.MappingProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Model.SharePoint;
using System.Text.Json;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implements the concrete PageGenerator (this is the core of PnP Transformation Framework)
    /// </summary>
    public class DefaultPageGenerator : IPageGenerator
    {
        private readonly ILogger<DefaultPageGenerator> logger;
        private readonly PageTransformationOptions defaultPageTransformationOptions;
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructor with DI support
        /// </summary>
        /// <param name="logger">The logger interface</param>
        /// <param name="pageTransformationOptions">The options</param>
        /// <param name="serviceProvider">Service provider</param>
        public DefaultPageGenerator(
            ILogger<DefaultPageGenerator> logger,
            IOptions<PageTransformationOptions> pageTransformationOptions,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.defaultPageTransformationOptions = pageTransformationOptions?.Value ?? throw new ArgumentNullException(nameof(pageTransformationOptions));
            this.serviceProvider = serviceProvider;
        }

        public async Task<Uri> GenerateAsync(PageTransformationContext context, MappingProviderOutput mappingOutput, Uri targetPageUri, CancellationToken token = default)
        {
            #region Validate input arguments

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (mappingOutput == null)
            {
                throw new ArgumentNullException(nameof(mappingOutput));
            }

            if (targetPageUri == null)
            {
                throw new ArgumentNullException(nameof(targetPageUri));
            }

            #endregion

            #region Validate target site

            // Ensure to have the needed target web properties
            var targetWeb = context.Task.TargetContext.Web;
            await targetWeb.EnsurePropertiesAsync(w => w.WebTemplate).ConfigureAwait(false);

            // Check target site
            if (targetWeb.WebTemplate != "SITEPAGEPUBLISHING" &&
                targetWeb.WebTemplate != "STS" &&
                targetWeb.WebTemplate != "GROUP" &&
                targetWeb.WebTemplate != "BDR" &&
                targetWeb.WebTemplate != "DEV")
            {
                logger.LogError(TransformationResources.Error_CrossSiteTransferTargetsNonModernSite);
                throw new ArgumentException(TransformationResources.Error_CrossSiteTransferTargetsNonModernSite);
            }

            #endregion

            #region Validate and create target page

            // Ensure PostAsNews is used together with PagePublishing
            if (this.defaultPageTransformationOptions.PostAsNews && !this.defaultPageTransformationOptions.PublishPage)
            {
                this.defaultPageTransformationOptions.PublishPage = true;
                logger.LogWarning(TransformationResources.Warning_PostingAPageAsNewsRequiresPagePublishing);
            }

            // Check if the target page already exists
            string targetPageUriString = targetPageUri.IsAbsoluteUri ? targetPageUri.AbsolutePath : targetPageUri.ToString();
            var targetFile = await targetWeb.GetFileByServerRelativeUrlAsync(targetPageUriString).ConfigureAwait(false);
            if (targetFile.Exists)
            {
                logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    TransformationResources.Info_PageAlreadyExistsInTargetLocation, targetPageUri.ToString()));

                if (!this.defaultPageTransformationOptions.Overwrite)
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        TransformationResources.Error_PageNotOverwriteIfExists, targetPageUri.ToString());

                    logger.LogError(errorMessage);
                    throw new ApplicationException(errorMessage);
                }
            }

            // Create the client side page using PnP Core SDK
            var targetPage = await targetWeb.NewPageAsync().ConfigureAwait(false);

            logger.LogDebug(TransformationResources.Debug_TransformCheckIfPageIsHomePage);

            #endregion

            #region Check if the page is the home page

            // Check if the page is the home page
            bool replacedByOOBHomePage = false;

            // Check if the transformed page is the web's home page
            if (mappingOutput.IsHomePage)
            {
                targetPage.LayoutType = PnP.Core.Model.SharePoint.PageLayoutType.Home;
                if (this.defaultPageTransformationOptions.ReplaceHomePageWithDefaultHomePage)
                {
                    targetPage.KeepDefaultWebParts = true;
                    replacedByOOBHomePage = true;

                    logger.LogInformation(TransformationResources.Info_TransformSourcePageHomePageUsingStock);
                }
            }

            // If it is not the home, let's define the actual page structure
            if (!replacedByOOBHomePage)
            {
                logger.LogInformation(TransformationResources.Info_TransformSourcePageAsArticlePage);

                #region Configure header from target page
                if (mappingOutput.PageHeader == null || (mappingOutput.PageHeader as PageHeader).Type == PageHeaderType.None)
                {
                    logger.LogInformation(TransformationResources.Info_TransformArticleSetHeaderToNone);

                    if (mappingOutput.SetAuthorInPageHeader)
                    {
                        targetPage.SetDefaultPageHeader();
                        targetPage.PageHeader.LayoutType = PageHeaderLayoutType.NoImage;

                        logger.LogInformation(TransformationResources.Info_TransformArticleSetHeaderToNoneWithAuthor);
                        await SetAuthorInPageHeaderAsync(context, mappingOutput, targetPage, token).ConfigureAwait(false);
                    }
                    else
                    {
                        targetPage.RemovePageHeader();
                    }
                }
                else if ((mappingOutput.PageHeader as PageHeader).Type == PageHeaderType.Default)
                {
                    logger.LogInformation(TransformationResources.Info_TransformArticleSetHeaderToDefault);
                    targetPage.SetDefaultPageHeader();
                }
                else if ((mappingOutput.PageHeader as PageHeader).Type == PageHeaderType.Custom)
                {
                    var infoMessage = $"{TransformationResources.Info_TransformArticleSetHeaderToCustom} {TransformationResources.Info_TransformArticleHeaderImageUrl} {mappingOutput.PageHeader.ImageServerRelativeUrl}";
                    logger.LogInformation(infoMessage);

                    targetPage.SetCustomPageHeader(mappingOutput.PageHeader.ImageServerRelativeUrl,
                        mappingOutput.PageHeader.TranslateX,
                        mappingOutput.PageHeader.TranslateY);
                }
                #endregion
            }

            #endregion

            // Set page title
            targetPage.PageTitle = mappingOutput.PageTitle;

            // Create the web parts and the transformed content

            // Start with the content sections
            int order = 1;
            foreach (var section in mappingOutput.Sections)
            {
                targetPage.AddSection(section, order);
                order++;
            }

            // Now generate web parts
            //if (transformationInformation.SkipHiddenWebParts)
            //{
            //    webParts = webParts.Where(c => !c.Hidden).ToList();
            //}

            // Persist the new client side page

            // Set metadata fields for list item of the page

            // Configure page item permissions
            // if (KeepPageSpecificPermissions) { }

            // Other page settings
            //#region Page Publishing
            //// Tag the file with a page modernization version stamp
            //string serverRelativePathForModernPage = savedTargetPage.ListItemAllFields[Constants.FileRefField].ToString();
            //bool pageListItemWasReloaded = false;
            //try
            //{
            //    var targetPageFile = context.Web.GetFileByServerRelativeUrl(serverRelativePathForModernPage);
            //    context.Load(targetPageFile, p => p.Properties);
            //    targetPageFile.Properties["sharepointpnp_pagemodernization"] = this.version;
            //    targetPageFile.Update();

            //    if (!pageTransformationInformation.KeepPageCreationModificationInformation &&
            //        !pageTransformationInformation.PostAsNews &&
            //        pageTransformationInformation.PublishCreatedPage)
            //    {
            //        // Try to publish, if publish is not needed/possible (e.g. when no minor/major versioning set) then this will return an error that we'll be ignoring
            //        targetPageFile.Publish(LogStrings.PublishMessage);
            //    }

            //    // Ensure we've the most recent page list item loaded, must be last statement before calling ExecuteQuery
            //    context.Load(savedTargetPage.ListItemAllFields);
            //    // Send both the property update and publish as a single operation to SharePoint
            //    context.ExecuteQueryRetry();
            //    pageListItemWasReloaded = true;
            //}
            //catch (Exception)
            //{
            //    // Eat exceptions as this is not critical for the generated page
            //    LogWarning(LogStrings.Warning_NonCriticalErrorDuringVersionStampAndPublish, LogStrings.Heading_ArticlePageHandling);
            //}

            //// Update flags field to indicate this is a "migrated" page
            //try
            //{
            //    // If for some reason the reload batched with the previous request did not finish then do it again
            //    if (!pageListItemWasReloaded)
            //    {
            //        context.Load(savedTargetPage.ListItemAllFields);
            //        context.ExecuteQueryRetry();
            //    }

            //    // Only perform the update when the field was not yet set
            //    bool skipSettingMigratedFromServerRendered = false;
            //    if (savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] != null)
            //    {
            //        skipSettingMigratedFromServerRendered = (savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] as string[]).Contains("MigratedFromServerRendered");
            //    }

            //    if (!skipSettingMigratedFromServerRendered)
            //    {
            //        savedTargetPage.ListItemAllFields[Constants.SPSitePageFlagsField] = ";#MigratedFromServerRendered;#";
            //        // Don't use UpdateOverWriteVersion as the listitem already exists 
            //        // resulting in an "Additions to this Web site have been blocked" error
            //        savedTargetPage.ListItemAllFields.SystemUpdate();
            //        context.Load(savedTargetPage.ListItemAllFields);
            //        context.ExecuteQueryRetry();
            //    }
            //}
            //catch (Exception)
            //{
            //    // Eat any exception
            //}

            //// Disable page comments on the create page, if needed
            //if (pageTransformationInformation.DisablePageComments)
            //{
            //    targetPage.DisableComments();
            //    LogInfo(LogStrings.TransformDisablePageComments, LogStrings.Heading_ArticlePageHandling);
            //}

            //#endregion

            // Swap pages?

            //#region Restore page author/editor/created/modified
            //if ((pageTransformationInformation.SourcePage != null && pageTransformationInformation.KeepPageCreationModificationInformation && this.SourcePageAuthor != null && this.SourcePageEditor != null) ||
            //    pageTransformationInformation.PostAsNews)
            //{
            //    UpdateTargetPageWithSourcePageInformation(finalListItemToUpdate, pageTransformationInformation, finalListItemToUpdate[Constants.FileRefField].ToString(), hasTargetContext);
            //}
            //#endregion

            // Log generation completed

            return null;
        }

        internal async Task SetAuthorInPageHeaderAsync(PageTransformationContext context, MappingProviderOutput mappingOutput, IPage targetClientSidePage, CancellationToken token = default)
        {
            // Try to get th
            var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();

            if (userMappingProvider == null)
            {
                throw new ApplicationException(TransformationResources.Error_MissingUserMappingProvider);
            }

            try
            {
                var mappedAuthorOutput = await userMappingProvider.MapUserAsync(new UserMappingProviderInput(context, mappingOutput.Author), token).ConfigureAwait(false);
                var ensuredPageAuthorUser = await targetClientSidePage.PnPContext.Web.EnsureUserAsync(mappedAuthorOutput.UserPrincipalName).ConfigureAwait(false);

                if (ensuredPageAuthorUser != null)
                {
                    var author = ensuredPageAuthorUser.ToUserEntity();

                    if (author != null)
                    {
                        if (!author.IsGroup)
                        {
                            // Don't serialize null values
                            var jsonSerializerOptions = new JsonSerializerOptions()
                            {
                                IgnoreNullValues = true,
                            };

                            var json = JsonSerializer.Serialize(author, jsonSerializerOptions);

                            if (!string.IsNullOrEmpty(json))
                            {
                                targetClientSidePage.PageHeader.Authors = json;
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                            TransformationResources.Warning_PageHeaderAuthorNotSet,
                            mappingOutput.Author));
                    }
                }
                else
                {
                    logger.LogWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        TransformationResources.Warning_PageHeaderAuthorNotSet,
                        mappingOutput.Author));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    TransformationResources.Warning_PageHeaderAuthorNotSetGenericError,
                    ex.Message));
            }
        }
    }
}
