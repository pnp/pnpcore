using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.MappingProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implements the concrete PageGenerator (this is the core of PnP Transformation Framework)
    /// </summary>
    public class DefaultPageGenerator : IPageGenerator
    {
        private readonly ILogger<DefaultPageGenerator> logger;
        private readonly PageTransformationOptions defaultPageTransformationOptions;
        private readonly IMemoryCache memoryCache;
        private readonly IServiceProvider serviceProvider;
        private readonly TokenParser tokenParser;

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
            this.memoryCache = this.serviceProvider.GetService<IMemoryCache>();
            this.tokenParser = this.serviceProvider.GetService<TokenParser>();
        }

        /// <summary>
        /// Translates the provided input into a page
        /// </summary>
        /// <param name="context">Page transformation options</param>
        /// <param name="mappingOutput">The mapping provider output</param>
        /// <param name="targetPageUri">The url for the target page</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The result of the page generation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ApplicationException"></exception>
        public async Task<PageGeneratorOutput> GenerateAsync(PageTransformationContext context, MappingProviderOutput mappingOutput, Uri targetPageUri, CancellationToken token = default)
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

            var result = new PageGeneratorOutput();

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
                logger.LogError(
                    TransformationResources.Error_CrossSiteTransferTargetsNonModernSite
                    .CorrelateString(context.Task.Id));
                throw new ArgumentException(TransformationResources.Error_CrossSiteTransferTargetsNonModernSite);
            }

            #endregion

            #region Validate and create the target page

            // Ensure PostAsNews is used together with PagePublishing
            if (this.defaultPageTransformationOptions.PostAsNews && !this.defaultPageTransformationOptions.PublishPage)
            {
                this.defaultPageTransformationOptions.PublishPage = true;
                logger.LogWarning(
                    TransformationResources.Warning_PostingAPageAsNewsRequiresPagePublishing
                    .CorrelateString(context.Task.Id));
            }

            // Check if the target page already exists
            string targetPageUriString = targetPageUri.IsAbsoluteUri ? targetPageUri.AbsolutePath : targetPageUri.ToString();
            IFile targetFile = null;
            var targetFileExists = false;
            try
            {
                targetFile = await targetWeb.GetFileByServerRelativeUrlAsync(targetPageUriString).ConfigureAwait(false);
                targetFileExists = true;
            }
            catch (SharePointRestServiceException)
            {
                // Simply ignore this exception and assume that the page does not exist
                targetFileExists = false;
            }
            if (targetFileExists)
            {
                logger.LogInformation(
                    TransformationResources.Info_PageAlreadyExistsInTargetLocation
                    .CorrelateString(context.Task.Id),
                    targetPageUri);

                if (!this.defaultPageTransformationOptions.Overwrite)
                {
                    // Raise an exception and stop the process if Overwrite is not allowed
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        TransformationResources.Error_PageNotOverwriteIfExists, targetPageUri.ToString());

                    logger.LogError(errorMessage
                        .CorrelateString(context.Task.Id));
                    throw new ApplicationException(errorMessage);
                }
            }

            // Create the client side page using PnP Core SDK and save it
            var targetPage = await targetWeb.NewPageAsync().ConfigureAwait(false);

            var targetPageFilePath = $"{mappingOutput.TargetPage.Folder}{mappingOutput.TargetPage.PageName}";
            await targetPage.SaveAsync(targetPageFilePath).ConfigureAwait(false);

            // Reload the generated file
            targetFile = await targetWeb.GetFileByServerRelativeUrlAsync(targetPageUriString).ConfigureAwait(false);

            #endregion

            #region Check if the page is the home page

            logger.LogDebug(
                TransformationResources.Debug_TransformCheckIfPageIsHomePage
                .CorrelateString(context.Task.Id));

            // Check if the page is the home page
            bool replacedByOOBHomePage = false;

            // Check if the transformed page is the web's home page
            if (mappingOutput.TargetPage.IsHomePage)
            {
                targetPage.LayoutType = PnP.Core.Model.SharePoint.PageLayoutType.Home;
                if (this.defaultPageTransformationOptions.ReplaceHomePageWithDefaultHomePage)
                {
                    targetPage.KeepDefaultWebParts = true;
                    replacedByOOBHomePage = true;

                    logger.LogInformation(
                        TransformationResources.Info_TransformSourcePageHomePageUsingStock
                        .CorrelateString(context.Task.Id));
                }
            }

            // If it is not the home, let's define the actual page structure
            if (!replacedByOOBHomePage)
            {
                logger.LogInformation(
                    TransformationResources.Info_TransformSourcePageAsArticlePage
                    .CorrelateString(context.Task.Id));

                #region Configure header from target page
                if (mappingOutput.TargetPage.PageHeader == null || (mappingOutput.TargetPage.PageHeader as PageHeader).Type == PageHeaderType.None)
                {
                    logger.LogInformation(
                        TransformationResources.Info_TransformArticleSetHeaderToNone
                        .CorrelateString(context.Task.Id));

                    if (mappingOutput.TargetPage.SetAuthorInPageHeader)
                    {
                        targetPage.SetDefaultPageHeader();
                        targetPage.PageHeader.LayoutType = PageHeaderLayoutType.NoImage;

                        logger.LogInformation(
                            TransformationResources.Info_TransformArticleSetHeaderToNoneWithAuthor
                            .CorrelateString(context.Task.Id));
                        await SetAuthorInPageHeaderAsync(context, mappingOutput, targetPage, context.Task.Id, token).ConfigureAwait(false);
                    }
                    else
                    {
                        targetPage.RemovePageHeader();
                    }
                }
                else if ((mappingOutput.TargetPage.PageHeader as PageHeader).Type == PageHeaderType.Default)
                {
                    logger.LogInformation(
                        TransformationResources.Info_TransformArticleSetHeaderToDefault
                        .CorrelateString(context.Task.Id));
                    targetPage.SetDefaultPageHeader();
                }
                else if ((mappingOutput.TargetPage.PageHeader as PageHeader).Type == PageHeaderType.Custom)
                {
                    var infoMessage = $"{TransformationResources.Info_TransformArticleSetHeaderToCustom} {TransformationResources.Info_TransformArticleHeaderImageUrl} {mappingOutput.TargetPage.PageHeader.ImageServerRelativeUrl}";
                    logger.LogInformation(infoMessage
                        .CorrelateString(context.Task.Id));

                    targetPage.SetCustomPageHeader(mappingOutput.TargetPage.PageHeader.ImageServerRelativeUrl,
                        mappingOutput.TargetPage.PageHeader.TranslateX,
                        mappingOutput.TargetPage.PageHeader.TranslateY);
                }
                #endregion
            }

            #endregion

            // Set page title
            targetPage.PageTitle = mappingOutput.TargetPage.PageTitle;

            // Create the web parts and the transformed content

            // Process all the sections, columns, and controls
            await GenerateTargetCanvasControlsAsync(targetWeb, targetPage, mappingOutput, context.Task.Id).ConfigureAwait(false);

            // Process metadata
            await CopyPageMetadataAsync(targetFile, mappingOutput, context.Task.Id).ConfigureAwait(false);

            // TODO: Process permissions

            #region Leftover code, most likely to be removed

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

            #endregion

            // Save the generated page
            await targetPage.SaveAsync(targetPageFilePath).ConfigureAwait(false);

            // Restore page author/editor/created/modified
            if ((this.defaultPageTransformationOptions.KeepPageCreationModificationInformation
                && mappingOutput.TargetPage.Author != null
                && mappingOutput.TargetPage.Editor != null) ||
                this.defaultPageTransformationOptions.PostAsNews)
            {
                await UpdateTargetPageWithSourcePageInformationAsync(targetFile, mappingOutput, context.Task.Id).ConfigureAwait(false);
            }

            // Return the generated page URL
            var generatedPageFile = await targetPage.GetPageFileAsync(f => f.ServerRelativeUrl).ConfigureAwait(false);
            var generatedPageUri = new Uri($"{targetWeb.Url.Scheme}://{targetWeb.Url.Host}{generatedPageFile.ServerRelativeUrl}");

            // Validate the URI of the output page
            if (!generatedPageUri.AbsoluteUri.Equals(targetPageUri.AbsoluteUri, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ApplicationException(TransformationResources.Error_InvalidTargetPageUri);
            }

            result.GeneratedPageUrl = targetPageUri;

            return result;
        }

        private async Task UpdateTargetPageWithSourcePageInformationAsync(IFile targetFile, MappingProviderOutput mappingOutput, Guid taskId)
        {
            try
            {
                // Ensure the properties to define the cache key
                var targetWeb = targetFile.PnPContext.Web;
                await targetWeb.EnsurePropertiesAsync(w => w.Id).ConfigureAwait(false);

                // Load the list item corresponding to the file
                await targetFile.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);
                var targetItem = targetFile.ListItemAllFields;

                // Update the target page information properties
                var pageAuthorUser = await targetWeb.EnsureUserAsync(mappingOutput.TargetPage.Author).ConfigureAwait(false);
                var pageEditorUser = await targetWeb.EnsureUserAsync(mappingOutput.TargetPage.Editor).ConfigureAwait(false);

                // Prep a new FieldUserValue object instance and update the list item
                var pageAuthor = new FieldUserValue()
                {
                    LookupValue = pageAuthorUser.Title,
                    LookupId = pageAuthorUser.Id
                };

                var pageEditor = new FieldUserValue()
                {
                    LookupValue = pageEditorUser.Title,
                    LookupId = pageEditorUser.Id
                };

                if (this.defaultPageTransformationOptions.KeepPageCreationModificationInformation ||
                    this.defaultPageTransformationOptions.PostAsNews)
                {
                    if (this.defaultPageTransformationOptions.KeepPageCreationModificationInformation)
                    {
                        // All 4 fields have to be set!
                        targetItem[SharePointConstants.CreatedByField] = pageAuthor;
                        targetItem[SharePointConstants.ModifiedByField] = pageEditor;
                        targetItem[SharePointConstants.CreatedField] = mappingOutput.TargetPage.Created;
                        targetItem[SharePointConstants.ModifiedField] = mappingOutput.TargetPage.Modified;
                    }

                    if (this.defaultPageTransformationOptions.PostAsNews)
                    {
                        targetItem[SharePointConstants.PromotedStateField] = "2";

                        // Determine what will be the publishing date that will show up in the news rollup
                        if (this.defaultPageTransformationOptions.KeepPageCreationModificationInformation)
                        {
                            targetItem[SharePointConstants.FirstPublishedDateField] = mappingOutput.TargetPage.Modified;
                        }
                        else
                        {
                            targetItem[SharePointConstants.FirstPublishedDateField] = targetItem[SharePointConstants.ModifiedField];
                        }
                    }

                    await targetItem.UpdateOverwriteVersionAsync().ConfigureAwait(false);

                    if (this.defaultPageTransformationOptions.PublishPage)
                    {
                        await targetFile.PublishAsync(TransformationResources.Info_PublishMessage).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                // Eat exceptions as this is not critical for the generated page
                logger.LogWarning(
                    TransformationResources.Warning_NonCriticalErrorDuringPublish
                    .CorrelateString(taskId), ex.Message);
            }
        }

        private async Task CopyPageMetadataAsync(IFile targetFile, MappingProviderOutput mappingOutput, Guid taskId)
        {
            // Ensure the properties to define the cache key
            var targetWeb = targetFile.PnPContext.Web;
            await targetWeb.EnsurePropertiesAsync(w => w.Id).ConfigureAwait(false);
            var targetSite = targetFile.PnPContext.Site;
            await targetSite.EnsurePropertiesAsync(s => s.Id).ConfigureAwait(false);
            await targetFile.EnsurePropertiesAsync(f => f.ListId).ConfigureAwait(false);

            // Retrieve the list of fields from cache
            var fieldsToCopy = await GetFieldsFromCache(targetFile, targetWeb, targetSite).ConfigureAwait(false);

            //bool listItemWasReloaded = false;
            if (fieldsToCopy.Count > 0)
            {
                // Load the list item corresponding to the file
                await targetFile.LoadAsync(f => f.ListItemAllFields).ConfigureAwait(false);
                var targetItem = targetFile.ListItemAllFields;

                // Initially set the content type Id - Are we sure about this excerpt?
                //targetItem[PageConstants.ContentTypeId] = mappingOutput.Metadata[PageConstants.ContentTypeId]?.Value;
                //await targetItem.UpdateOverwriteVersionAsync().ConfigureAwait(false);

                // TODO: Complete metadata fields handling (taxonomy with mapping provider and other fields)
                foreach (var fieldToCopy in fieldsToCopy.Where(f => f.Type == "TaxonomyFieldTypeMulti" || f.Type == "TaxonomyFieldType"))
                {
                    logger.LogInformation(
                        TransformationResources.Info_MappingTaxonomyField
                        .CorrelateString(taskId), fieldToCopy.Name);

                    // https://pnp.github.io/pnpcore/using-the-sdk/listitems-fields.html#taxonomy-fields
                }

                foreach (var fieldToCopy in fieldsToCopy.Where(f => f.Type != "TaxonomyFieldTypeMulti" && f.Type != "TaxonomyFieldType"))
                {
                    logger.LogInformation(
                        TransformationResources.Info_MappingRegularField
                        .CorrelateString(taskId), fieldToCopy.Name);

                    // https://pnp.github.io/pnpcore/using-the-sdk/listitems-fields.html
                }
            }
        }

        private async Task<List<FieldData>> GetFieldsFromCache(IFile targetFile, IWeb targetWeb, ISite targetSite)
        {
            // Define the cache key
            var fieldsCacheKey = $"Fields|{targetSite.Id}|{targetWeb.Id}|{targetFile.ListId}";

            List<FieldData> result = null;
            if (!this.memoryCache.TryGetValue(fieldsCacheKey, out result))
            {
                // Get the fields of the current list
                var targetList = await targetFile.PnPContext.Web.Lists.GetByIdAsync(targetFile.ListId,
                    l => l.Id,
                    l => l.Fields.QueryProperties(f => f.Id, f => f.StaticName, f => f.TypeAsString, f => f.Hidden)
                    ).ConfigureAwait(false);

                // Exclude hidden and built-in fields
                result = targetList.Fields.AsRequested()
                    .Where(f => !f.Hidden && !BuiltInFields.Contains(f.StaticName)).Select(f => new FieldData
                {
                    Id = f.Id,
                    Name = f.StaticName,
                    Type = f.TypeAsString
                }).ToList();

                // Store the list of fields in cache for future use
                this.memoryCache.Set(fieldsCacheKey, result);
            }

            return result;
        }

        private async Task GenerateTargetCanvasControlsAsync(IWeb targetWeb, IPage targetPage, MappingProviderOutput mappingOutput, Guid taskId)
        {
            // Prepare global tokens
            var globalTokens = await PrepareGlobalTokensAsync(targetWeb).ConfigureAwait(false);

            // Get the list of components available in the current site
            var componentsToAdd = await GetClientSideComponentsAsync(targetPage).ConfigureAwait(false);

            int sectionOrder = 0;
            foreach (var section in mappingOutput.TargetPage.Sections)
            {
                section.Order = sectionOrder;
                targetPage.AddSection(section.CanvasTemplate, sectionOrder);
                var targetSection = targetPage.Sections[sectionOrder];
                sectionOrder++;

                int columnOrder = 0;
                int controlOrder = 0;
                foreach (var column in section.Columns)
                {
                    var targetColumn = targetSection.Columns[columnOrder];
                    columnOrder++;
                    controlOrder++;

                    foreach (var control in column.Controls)
                    {
                        GenerateTargetCanvasControl(targetPage, componentsToAdd, controlOrder, targetColumn, control, globalTokens, taskId);
                    }
                }
            }
        }

        private void GenerateTargetCanvasControl(IPage targetPage, List<PageComponent> componentsToAdd, int controlOrder, ICanvasColumn targetColumn, Model.CanvasControl control, Dictionary<string, string> globalTokens, Guid taskId)
        {
            // Prepare a web part control container
            IPageComponent baseControl = null;

            switch (control.ControlType)
            {
                case Model.CanvasControlType.ClientSideText:
                    // Here we add a text control
                    var text = targetPage.NewTextPart();
                    text.Text = control["Text"] as string;

                    // Add the actual text control to the page
                    targetPage.AddControl(text, targetColumn, controlOrder);

                    // Log the just executed action
                    logger.LogInformation(
                        TransformationResources.Info_CreatedTextControl
                        .CorrelateString(taskId));

                    break;
                case Model.CanvasControlType.CustomClientSideWebPart:
                    // Parse the control ID to support generic web part placement scenarios
                    var ControlId = control["ControlId"] as string;
                    // Check if this web part belongs to the list of "usable" web parts for this site
                    baseControl = componentsToAdd.FirstOrDefault(p => p.Id.Equals($"{{{ControlId}}}", StringComparison.InvariantCultureIgnoreCase));

                    logger.LogInformation(
                        TransformationResources.Info_UsingCustomModernWebPart
                        .CorrelateString(taskId));

                    break;
                case Model.CanvasControlType.DefaultWebPart:
                    // Determine the actual default web part
                    var webPartType = (DefaultWebPart)Enum.Parse(typeof(DefaultWebPart), control["WebPartType"] as string);
                    var webPartName = targetPage.DefaultWebPartToWebPartId(webPartType);
                    var webPartTitle = control["Title"] as string;
                    var webPartProperties = control["Properties"] as Dictionary<string, string>;
                    var jsonControlData = control["JsonControlData"] as string;

                    if (webPartType == DefaultWebPart.ClientWebPart)
                    {
                        var addinComponents = componentsToAdd.Where(p => p.Name.Equals(webPartName, StringComparison.InvariantCultureIgnoreCase));
                        foreach (var addin in addinComponents)
                        {
                            // Find the right add-in web part via title matching...maybe not bullet proof but did find anything better for now
                            JObject wpJObject = JObject.Parse(addin.Manifest);

                            // As there can be multiple classic web parts (via provider hosted add ins or SharePoint hosted add ins) we're looping to find the first one with a matching title
                            foreach (var addinEntry in wpJObject["preconfiguredEntries"])
                            {
                                if (addinEntry["title"]["default"].Value<string>() == webPartTitle)
                                {
                                    baseControl = addin;

                                    var jsonProperties = addinEntry;

                                    // Fill custom web part properties in this json. Custom properties are listed as child elements under clientWebPartProperties, 
                                    // replace their "default" value with the value we got from the web part's properties
                                    jsonProperties = PopulateAddInProperties(jsonProperties, webPartProperties);

                                    // Override the JSON data we read from the model as this is fully dynamic due to the nature of the add-in client part
                                    jsonControlData = jsonProperties.ToString(Newtonsoft.Json.Formatting.None);

                                    logger.LogInformation(
                                        TransformationResources.Info_ContentUsingAddinWebPart
                                        .CorrelateString(taskId), baseControl.Name);

                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        baseControl = componentsToAdd.FirstOrDefault(p => p.Name.Equals(webPartName, StringComparison.InvariantCultureIgnoreCase));

                        logger.LogInformation(
                            TransformationResources.Info_ContentUsingModernWebPart
                            .CorrelateString(taskId), webPartType);
                    }

                    // If we found the web part as a possible candidate to use then add it
                    if (baseControl != null)
                    {                        
                        var jsonDecoded = WebUtility.HtmlDecode(this.tokenParser.ReplaceTargetTokens(targetPage.PnPContext, jsonControlData, webPartProperties, globalTokens));

                        var myWebPart = targetPage.NewWebPart(baseControl);
                        myWebPart.Order = controlOrder;
                        myWebPart.PropertiesJson = jsonDecoded;

                        // Add the actual text control to the page
                        targetPage.AddControl(myWebPart, targetColumn, controlOrder);

                        logger.LogInformation(
                            TransformationResources.Info_AddedClientSideWebPartToPage
                            .CorrelateString(taskId), webPartTitle);
                    }
                    else
                    {
                        logger.LogWarning(
                            TransformationResources.Warning_ContentWarnModernNotFound
                            .CorrelateString(taskId));
                    }

                    break;
                default:
                    break;
            }
        }

        private async Task<List<PageComponent>> GetClientSideComponentsAsync(IPage targetPage)
        {
            var siteToComponentMapping = new Dictionary<Guid, string>();

            var componentsToAdd = (await targetPage.AvailablePageComponentsAsync().ConfigureAwait(false))
                .Cast<PageComponent>().ToList();

            // TODO: Consider adding back caching

            return componentsToAdd;
        }

        private JToken PopulateAddInProperties(JToken jsonProperties, Dictionary<string, string> webPartProperties)
        {
            foreach (JToken property in jsonProperties["properties"]["clientWebPartProperties"])
            {
                var wpProp = property["name"].Value<string>();
                if (!string.IsNullOrEmpty(wpProp))
                {
                    if (webPartProperties.ContainsKey(wpProp))
                    {
                        if (jsonProperties["properties"]["userDefinedProperties"][wpProp] != null)
                        {
                            jsonProperties["properties"]["userDefinedProperties"][wpProp] = webPartProperties[wpProp].ToString();
                        }
                        else
                        {
                            JToken newProp = JObject.Parse($"{{\"{wpProp}\": \"{webPartProperties[wpProp].ToString()}\"}}");
                            (jsonProperties["properties"]["userDefinedProperties"] as JObject).Merge(newProp);
                        }
                    }
                }
            }

            return jsonProperties;
        }


        internal async Task SetAuthorInPageHeaderAsync(PageTransformationContext context, MappingProviderOutput mappingOutput, IPage targetClientSidePage, Guid taskId, CancellationToken token = default)
        {
            // Try to get th
            var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();

            if (userMappingProvider == null)
            {
                throw new ApplicationException(TransformationResources.Error_MissingUserMappingProvider);
            }

            try
            {
                var mappedAuthorOutput = await userMappingProvider.MapUserAsync(new UserMappingProviderInput(context, mappingOutput.TargetPage.Author), token).ConfigureAwait(false);
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
#if NET5_0_OR_GREATER
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#else
                                IgnoreNullValues = true,
#endif
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
                        logger.LogWarning(
                            TransformationResources.Warning_PageHeaderAuthorNotSet
                            .CorrelateString(taskId),
                            mappingOutput.TargetPage.Author);
                    }
                }
                else
                {
                    logger.LogWarning(
                        TransformationResources.Warning_PageHeaderAuthorNotSet
                        .CorrelateString(taskId),
                        mappingOutput.TargetPage.Author);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    TransformationResources.Warning_PageHeaderAuthorNotSetGenericError
                    .CorrelateString(taskId), ex.Message);
            }
        }

        /// <summary>
        /// Prepares global tokens for target environment
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> PrepareGlobalTokensAsync(IWeb web)
        {
            Dictionary<string, string> globalTokens = new Dictionary<string, string>(5);

            await web.EnsurePropertiesAsync(w => w.Id, w => w.Url, w => w.ServerRelativeUrl).ConfigureAwait(false);
            var site = web.PnPContext.Site;
            await site.EnsurePropertiesAsync(s => s.Id, s => s.RootWeb.QueryProperties(rw => rw.ServerRelativeUrl)).ConfigureAwait(false);

            // Add the fixed properties
            globalTokens.Add("Host", $"{web.Url.Scheme}://{web.Url.DnsSafeHost}");
            globalTokens.Add("Web", web.ServerRelativeUrl.TrimEnd('/'));
            globalTokens.Add("SiteCollection", site.RootWeb.ServerRelativeUrl.TrimEnd('/'));
            globalTokens.Add("WebId", web.Id.ToString());
            globalTokens.Add("SiteId", site.Id.ToString());

            return globalTokens;
        }
    }
}
