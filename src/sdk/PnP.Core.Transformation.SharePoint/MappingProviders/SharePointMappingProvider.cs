using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using Newtonsoft.Json.Linq;
using PnPCoreModelSP = PnP.Core.Model.SharePoint;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Extensions;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using PnP.Core.Transformation.SharePoint.Model;
using PnP.Core.Transformation.SharePoint.Services;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.Services.MappingProviders;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMappingProvider"/>
    /// </summary>
    public class SharePointMappingProvider : IMappingProvider
    {
        class CombinedMapping
        {
            public int Order { get; set; }
            public ClientSideText ClientSideText { get; set; }
            public ClientSideWebPart ClientSideWebPart { get; set; }
        }

        private ILogger<SharePointMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;

        private const string webPartMarkerString = "[[WebPartMarker]]";

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointMappingProvider(ILogger<SharePointMappingProvider> logger,
            IOptions<SharePointTransformationOptions> options,
            IServiceProvider serviceProvider)
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
            MappingProviderOutput result = new MappingProviderOutput();

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

            // And set target properties accordingly
            result.TargetPage.Author = sourcePage.Author.Email;
            result.TargetPage.Editor = sourcePage.Editor.Email;
            result.TargetPage.Created = sourcePage.Created.Date;
            result.TargetPage.Modified = sourcePage.Modified.Date;

            // Determine if the page is a Root Page
            sourcePage.IsRootPage = pageFile != null;
            
            // and configure the resulting object
            result.TargetPage.IsHomePage = sourcePage.IsRootPage;

            // Evaluate the source page type
            sourcePage.PageType = EvaluatePageType(pageFile, pageItem, crossSiteTransformation, crossTenantTransformation);

            // Retrieve Version and ExactVersion of source and target
            (sourcePage.SourceVersion, sourcePage.SourceVersionNumber) = sourceContext.GetVersions();

            // Log that we've finished the validation stage
            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_PageValidationChecksComplete, pageFile.ServerRelativeUrl));

            // Extract the list of Web Parts to process and the reference page layout
            var (layout, webPartsToProcess) = await AnalyzePageAsync(input.Context, pageFile, pageItem, sourcePage).ConfigureAwait(false);

            // Determine the target page title
            result.TargetPage.PageTitle = DeterminePageTitle(input.Context, pageFile, pageItem, sourcePage, webPartsToProcess);

            // Start the actual transformation
            var transformationStartDateTime = DateTime.Now;

            // Load information from source context
            LoadClientObjects(sourceContext);

            // TODO: Let's see if we really need to load tenant global properties or not
            // see line 299 of PageTransformator.cs in PnP Framework

            // Retrieve the parent Folder of the page, if any and the target page name
            string targetPageName = null;
            (sourcePage.Folder, targetPageName) = DeterminePageFolder(sourceContext, sourcePage, pageItem, input.Context.TargetPageUri);

            // Log that we've finished the analysis stage
            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                SharePointTransformationResources.Info_PageAnalysisComplete, pageFile.ServerRelativeUrl));
            result.TargetPage.Folder = sourcePage.Folder;

            // Map the page layout into a modern page structure
            result.TargetPage.Sections.AddRange(MapPageWireframe(layout));

            // Transform the web parts
            var webPartMappingProvider = serviceProvider.GetService<IWebPartMappingProvider>();
            if (webPartMappingProvider != null)
            {
                var sourceWebParts = webPartsToProcess.Where(w => !w.IsClosed).ToList();

                // Double-check if we have web parts to process
                if (sourceWebParts == null || sourceWebParts.Count == 0)
                {
                    // nothing to transform
                    logger.LogWarning(SharePointTransformationResources.Warning_NothingToTransform);
                }
                else
                {
                    // Normalize row numbers as there can be gaps if the analyzed page contained wiki tables
                    int newRowOrder = 0;
                    int lastExistingRowOrder = -1;
                    foreach (var webPart in sourceWebParts.OrderBy(p => p.Row))
                    {
                        if (lastExistingRowOrder < webPart.Row)
                        {
                            newRowOrder++;
                            lastExistingRowOrder = webPart.Row;
                        }

                        webPart.Row = newRowOrder;
                    }

                    // For every web part that is not closed
                    foreach (var webPart in sourceWebParts.OrderBy(p => p.Row).ThenBy(p => p.Column).ThenBy(p => p.Order))
                    {
                        // Process the actual mapping
                        var webPartInput = new SharePointWebPartMappingProviderInput(input.Context, sourceContext)
                        {
                            WebPart = webPart,
                            IsCrossSiteTransformation = crossSiteTransformation
                        };
                        var output = await webPartMappingProvider
                            .MapWebPartAsync(webPartInput, token)
                            .ConfigureAwait(false) as SharePointWebPartMappingProviderOutput;

                        ManageCombinedMapping(result.TargetPage, webPart, output.Mapping);
                    }
                }
            }

            #region Text/Section/Column cleanup

            // Drop "empty" text parts. Wiki pages tend to have a lot of text parts just containing div's and BR's...no point in keep those as they generate to much whitespace
            RemoveEmptyTextParts(result.TargetPage);

            // Remove empty sections and columns to optimize screen real estate
            if (options.Value.RemoveEmptySectionsAndColumns)
            {
                RemoveEmptySectionsAndColumns(result.TargetPage);
            }

            #endregion

            #region Handle page metadata

            if (options.Value.CopyPageMetadata)
            {
                // Copy the source page metadata 
                var metadata = await LoadSourcePageMetadataAsync(pageItem).ConfigureAwait(false);
                foreach (var m in metadata)
                {
                    result.Metadata.Add(m.Key, m.Value);
                }
            }

            #endregion

            #region Handle page permissions

            if (options.Value.KeepPageSpecificPermissions)
            {
                // Copy the source page item level permissions                 
                GetItemLevelPermissions(sourceContext, pageItem);
            }

            #endregion

            #region temporary code, most likely to be removed

            //var htmlMappingProvider = serviceProvider.GetService<IHtmlMappingProvider>();
            //if (htmlMappingProvider != null)
            //{
            //    // TODO: get the html content
            //    var htmlInput = new HtmlMappingProviderInput(input.Context, "TODO");
            //    var output = await htmlMappingProvider
            //        .MapHtmlAsync(htmlInput, token)
            //        .ConfigureAwait(false);
            //}

            //var metadataMappingProvider = serviceProvider.GetService<IMetadataMappingProvider>();
            //if (metadataMappingProvider != null)
            //{
            //    // TODO: prepare input
            //    var metadataInput = new MetadataMappingProviderInput(input.Context);
            //    var output = await metadataMappingProvider
            //        .MapMetadataFieldAsync(metadataInput, token)
            //        .ConfigureAwait(false);
            //}

            //var urlMappingProvider = serviceProvider.GetService<IUrlMappingProvider>();
            //if (urlMappingProvider != null)
            //{
            //    // TODO: prepare uri
            //    var metadataInput = new UrlMappingProviderInput(input.Context, string.Empty);
            //    var output = await urlMappingProvider
            //        .MapUrlAsync(metadataInput, token)
            //        .ConfigureAwait(false);
            //}

            ////var pageLayoutMappingProvider = serviceProvider.GetService<IPageLayoutMappingProvider>();
            ////if (pageLayoutMappingProvider != null)
            ////{
            ////    // TODO: prepare page layout
            ////    var pageLayoutInput = new PageLayoutMappingProviderInput(input.Context);
            ////    var output = await pageLayoutMappingProvider
            ////        .MapPageLayoutAsync(pageLayoutInput, token)
            ////        .ConfigureAwait(false);
            ////}

            //var taxonomyMappingProvider = serviceProvider.GetService<ITaxonomyMappingProvider>();
            //if (taxonomyMappingProvider != null)
            //{
            //    // TODO: prepare term id
            //    var taxonomyInput = new TaxonomyMappingProviderInput(input.Context, "");
            //    var output = await taxonomyMappingProvider
            //        .MapTermAsync(taxonomyInput, token)
            //        .ConfigureAwait(false);
            //}

            //var userMappingProvider = serviceProvider.GetService<IUserMappingProvider>();
            //if (userMappingProvider != null)
            //{
            //    // TODO: prepare user
            //    var userInput = new UserMappingProviderInput(input.Context, "");
            //    var output = await userMappingProvider
            //        .MapUserAsync(userInput, token)
            //        .ConfigureAwait(false);
            //}

            #endregion

            return result;
        }

        private ListItemPermission GetItemLevelPermissions(ClientContext sourceContext, ListItem pageItem)
        {
            // Prepare result variable
            ListItemPermission lip = new ListItemPermission();

            // Prepare supporting data
            var pagesLibrary = pageItem.ParentList;
            pagesLibrary.EnsureProperty(l => l.EffectiveBasePermissions);
            pageItem.EnsureProperty(p => p.HasUniqueRoleAssignments);

            if (pageItem.IsPropertyAvailable("HasUniqueRoleAssignments") && pageItem.HasUniqueRoleAssignments)
            {
                // You need to have the ManagePermissions permission before item level permissions can be copied
                if (pagesLibrary.EffectiveBasePermissions.Has(PermissionKind.ManagePermissions))
                {
                    // Copy the unique permissions from source to target
                    // Get the unique permissions
                    sourceContext.Load(pageItem, a => a.EffectiveBasePermissions, a => a.RoleAssignments.Include(roleAsg => roleAsg.Member.LoginName,
                        roleAsg => roleAsg.RoleDefinitionBindings.Include(roleDef => roleDef.Id, roleDef => roleDef.Name, roleDef => roleDef.Description)));
                    sourceContext.ExecuteQueryRetry();

                    if (pageItem.EffectiveBasePermissions.Has(PermissionKind.ManagePermissions))
                    {
                        // Load the site groups
                        sourceContext.Load(sourceContext.Web.SiteGroups, p => p.Include(g => g.LoginName));
                        sourceContext.ExecuteQueryRetry();
                        
                        // TODO: We need to manage this information
                        // lip.RoleAssignments = pageItem.RoleAssignments;

                        // Apply new permissions
                        foreach (var roleAssignment in pageItem.RoleAssignments)
                        {
                            var principal = GetPrincipal(sourceContext.Web, roleAssignment.Member.LoginName, true);
                            if (principal != null)
                            {
                                if (!lip.Principals.ContainsKey(roleAssignment.Member.LoginName))
                                {
                                    lip.Principals.Add(roleAssignment.Member.LoginName, principal);
                                }
                            }
                        }
                    }
                }
                else
                {
                    logger.LogWarning(SharePointTransformationResources.Warning_TransformGetItemPermissionsAccessDenied);
                    return lip;
                }
            }

            logger.LogInformation(SharePointTransformationResources.Info_GetItemLevelPermissions);

            return lip;
        }

        private Principal GetPrincipal(Web web, string principalInput, bool reading = false)
        {
            Principal principal = web.SiteGroups.FirstOrDefault(g => g.LoginName.Equals(principalInput, StringComparison.OrdinalIgnoreCase));

            if (principal == null)
            {
                if (principalInput.Contains("#ext#"))
                {
                    principal = web.SiteUsers.FirstOrDefault(u => u.LoginName.Equals(principalInput));

                    if (principal == null)
                    {
                        //Skipping external user...
                    }
                }
                else
                {
                    try
                    {
                        principal = web.EnsureUser(principalInput);
                        web.Context.ExecuteQueryRetry();
                    }
                    catch (Exception ex)
                    {
                        //Failed to EnsureUser, we're not failing for this, only log as error when doing an in site transformation as it's not expected to fail here
                        logger.LogError(SharePointTransformationResources.Error_GetPrincipalFailedEnsureUser, ex);

                        principal = null;
                    }
                }
            }

            return principal;
        }

        private static async Task<Dictionary<string, FieldData>> LoadSourcePageMetadataAsync(ListItem pageItem)
        {
            var result = new Dictionary<string, FieldData>();

            var listFields = pageItem.ParentList.Fields;
            pageItem.Context.Load(listFields);
            await pageItem.Context.ExecuteQueryRetryAsync().ConfigureAwait(false);

            foreach (var field in pageItem.FieldValues)
            {
                // Search for the corresponding field in the list
                var listField = listFields.FirstOrDefault(f => f.InternalName == field.Key);

                result.Add(field.Key, new FieldData
                {
                    Id = listField != null ? listField.Id : Guid.Empty,
                    Name = listField?.InternalName,
                    Type = listField?.TypeAsString,
                    Value = field.Value
                });
            }

            return result;
        }

        internal void RemoveEmptyTextParts(Page targetPage)
        {
            foreach (var section in targetPage.Sections)
            {
                foreach (var column in section.Columns)
                {
                    foreach (var cc in column.Controls.Where(i => i.ControlType == CanvasControlType.ClientSideText).ToList())
                    {
                        HtmlParser parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });

                        using (var document = parser.ParseDocument(cc["Text"] as string))
                        {
                            if (document.FirstChild != null && string.IsNullOrEmpty(document.FirstChild.TextContent))
                            {
                                logger.LogInformation(SharePointTransformationResources.Info_TransformRemovingEmptyWebPart);
                                // Drop text part
                                column.Controls.Remove(cc);
                            }
                        }
                    }
                }
            }
        }

        internal void RemoveEmptySectionsAndColumns(Page targetPage)
        {
            foreach (var section in targetPage.Sections.ToList())
            {
                // First remove all empty sections
                if (section.Columns.Count == 0)
                {
                    targetPage.Sections.Remove(section);
                }
            }

            // Remove empty columns
            foreach (var section in targetPage.Sections)
            {
                if (section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumn ||
                    section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnLeft ||
                    section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnRight ||
                    section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnVerticalSection ||
                    section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                    section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnRightVerticalSection)
                {
                    // NOTE: We never set IsVerticalSectionColumn, how is it possible?

                    var emptyColumn = section.Columns.FirstOrDefault(p => p.Controls.Count == 0); // && !p.IsVerticalSectionColumn);
                    if (emptyColumn != null)
                    {
                        // drop the empty column and change to single column section
                        section.Columns.Remove(emptyColumn);

                        if (section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnVerticalSection ||
                            section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnLeftVerticalSection ||
                            section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.TwoColumnRightVerticalSection)
                        {
                            section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.OneColumnVerticalSection;
                        }
                        else
                        {
                            section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.OneColumn;
                        }

                        // NOTE: This should be done by the generator (on the target side) so we don't need it here, I guess
                        // (section.Columns.First() as PnPCore.CanvasColumn).ResetColumn(0, 12);
                    }
                }
                else if (section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.ThreeColumn ||
                         section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.ThreeColumnVerticalSection)
                {

                    // NOTE: Same note as before about IsVerticalSectionColumn

                    var emptyColumns = section.Columns.Where(p => p.Controls.Count == 0); // && !p.IsVerticalSectionColumn);
                    if (emptyColumns != null)
                    {
                        if (emptyColumns.Count() == 2)
                        {
                            // drop the two empty columns and change to single column section
                            foreach (var emptyColumn in emptyColumns)
                            {
                                section.Columns.Remove(emptyColumn);
                            }

                            if (section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.OneColumnVerticalSection;
                            }
                            else
                            {
                                section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.OneColumn;
                            }

                            // NOTE: This should be done by the generator (on the target side) so we don't need it here, I guess
                            // (section.Columns.First() as PnPCoreModelSP.CanvasColumn).ResetColumn(0, 12);
                        }
                        if (emptyColumns.Count() == 1)
                        {
                            // Remove the empty column and change to two column section
                            section.Columns.Remove(emptyColumns.First());

                            if (section.CanvasTemplate == PnPCoreModelSP.CanvasSectionTemplate.ThreeColumnVerticalSection)
                            {
                                section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.TwoColumnVerticalSection;
                            }
                            else
                            {
                                section.CanvasTemplate = PnPCoreModelSP.CanvasSectionTemplate.TwoColumn;
                            }

                            // NOTE: This should be done by the generator (on the target side) so we don't need it here, I guess
                            //int i = 0;
                            //foreach (var column in section.Columns.Where(p => !p.IsVerticalSectionColumn))
                            //{
                            //    (column as PnPCore.CanvasColumn).ResetColumn(i, 6);
                            //    i++;
                            //}
                        }
                    }
                }
            }
        }

        private void ManageCombinedMapping(Page targetPage, WebPartEntity webPart, Mapping mapping)
        {
            // Use the mapping data => make one list of Text and WebParts to allow for correct ordering
            logger.LogDebug(SharePointTransformationResources.Debug_CombiningMappingData);
            var combinedMappinglist = new List<CombinedMapping>();
            if (mapping.ClientSideText != null)
            {
                foreach (var map in mapping.ClientSideText.OrderBy(p => p.Order))
                {
                    if (!Int32.TryParse(map.Order, out Int32 mapOrder))
                    {
                        mapOrder = 0;
                    }

                    combinedMappinglist.Add(new CombinedMapping { ClientSideText = map, ClientSideWebPart = null, Order = mapOrder });
                }
            }
            if (mapping.ClientSideWebPart != null)
            {
                foreach (var map in mapping.ClientSideWebPart.OrderBy(p => p.Order))
                {
                    if (!Int32.TryParse(map.Order, out Int32 mapOrder))
                    {
                        mapOrder = 0;
                    }

                    combinedMappinglist.Add(new CombinedMapping { ClientSideText = null, ClientSideWebPart = map, Order = mapOrder });
                }
            }

            // Get the order of the last inserted control in this column
            int order = LastColumnOrder(targetPage, webPart.Row - 1, webPart.Column - 1);
            // Interate the controls for this mapping using their order
            foreach (var map in combinedMappinglist.OrderBy(p => p.Order))
            {
                order++;

                var control = new CanvasControl();
                control.Order = order;

                if (map.ClientSideText != null)
                {
                    control.ControlType = CanvasControlType.ClientSideText;

                    // Parse the Text to support custom tokens
                    control["Text"] = TokenParser.ReplaceTokens(map.ClientSideText.Text, webPart);

                    logger.LogInformation(SharePointTransformationResources.Info_AddedClientSideTextWebPart);
                }
                else if (map.ClientSideWebPart != null)
                {
                    if (map.ClientSideWebPart.Type == ClientSideWebPartType.Custom)
                    {
                        control.ControlType = CanvasControlType.CustomClientSideWebPart;

                        // Parse the control ID to support generic web part placement scenarios
                        control["ControlId"] = TokenParser.ReplaceTokens(map.ClientSideWebPart.ControlId, webPart);

                        logger.LogInformation(SharePointTransformationResources.Info_UsingCustomModernWebPart);
                    }
                    else
                    {
                        control.ControlType = CanvasControlType.DefaultWebPart;

                        string webPartName = map.ClientSideWebPart.Type.ToString();

                        logger.LogInformation(string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            SharePointTransformationResources.Info_ContentUsingModernWebPart,
                            map.ClientSideWebPart.Type.ToString()));
                    }
                }

                // We add the component to the temporary abstract page
                targetPage.Sections[webPart.Row - 1].Columns[webPart.Column - 1].Controls.Add(control);
            }
        }

        private Int32 LastColumnOrder(Page targetPage, int row, int col)
        {
            var lastControl = targetPage.Sections[row].Columns[col].Controls.OrderBy(p => p.Order).LastOrDefault();
            if (lastControl != null)
            {
                return lastControl.Order;
            }
            else
            {
                return -1;
            }
        }

        private static List<Section> MapPageWireframe(PageLayout pageLayout)
        {
            var result = new List<Section>();

            switch (pageLayout)
            {
                // In case of a custom layout let's stick with one column as model
                case PageLayout.Wiki_OneColumn:
                case PageLayout.WebPart_FullPageVertical:
                case PageLayout.Wiki_Custom:
                case PageLayout.WebPart_Custom:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    break;
                case PageLayout.Wiki_TwoColumns:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                            Columns = GetColumns(2)
                        });
                    break;
                case PageLayout.Wiki_ThreeColumns:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumn,
                            Columns = GetColumns(3)
                        });
                    break;
                case PageLayout.Wiki_TwoColumnsWithSidebar:
                case PageLayout.WebPart_2010_TwoColumnsLeft:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeft,
                            Columns = GetColumns(2)
                        });
                    break;
                case PageLayout.WebPart_HeaderRightColumnBody:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnLeft,
                            Columns = GetColumns(2)
                        });
                    break;
                case PageLayout.WebPart_HeaderLeftColumnBody:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumnRight,
                            Columns = GetColumns(2)
                        });
                    break;
                case PageLayout.Wiki_TwoColumnsWithHeader:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                            Columns = GetColumns(2)
                        });
                    break;
                case PageLayout.Wiki_TwoColumnsWithHeaderAndFooter:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.TwoColumn,
                            Columns = GetColumns(2)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    break;
                case PageLayout.Wiki_ThreeColumnsWithHeader:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumn,
                            Columns = GetColumns(3)
                        });
                    break;
                case PageLayout.Wiki_ThreeColumnsWithHeaderAndFooter:
                case PageLayout.WebPart_HeaderFooterThreeColumns:
                case PageLayout.WebPart_HeaderFooter4ColumnsTopRow:
                case PageLayout.WebPart_HeaderFooter2Columns4Rows:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumn,
                            Columns = GetColumns(3)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    break;
                case PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns:
                case PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.ThreeColumn,
                            Columns = GetColumns(3)
                        });
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    break;
                default:
                    result.Add(
                        new Section
                        {
                            CanvasTemplate = Core.Model.SharePoint.CanvasSectionTemplate.OneColumn,
                            Columns = GetColumns(1)
                        });
                    break;
            }

            return result;

            List<Column> GetColumns(int size)
            {
                return (from n in Enumerable.Range(0, size) select new Column()).ToList();
            }
        }

        private string DeterminePageTitle(PageTransformationContext context, Microsoft.SharePoint.Client.File pageFile, ListItem pageItem, SourcePageInformation sourcePage, List<WebPartEntity> webPartsToProcess)
        {
            string pageTitle = string.Empty;

            switch (sourcePage.PageType)
            {
                case SourcePageType.WikiPage:
                    pageTitle = ExtractPageTitleFromFileName(pageItem);
                    break;
                case SourcePageType.BlogPage:
                    string titleValue = null;
                    if (pageItem.TryGetFieldValue<string>(SharePointConstants.TitleField, out titleValue))
                    {
                        pageTitle = titleValue ?? string.Empty;
                    }
                    break;
                case SourcePageType.WebPartPage:
                    var titleWebPart = webPartsToProcess.FirstOrDefault(w => w.Type == WebParts.TitleBar);
                    if (titleWebPart != null &&
                        titleWebPart.Properties.ContainsKey(SharePointConstants.HeaderTitle) &&
                        !string.IsNullOrEmpty(titleWebPart.Properties[SharePointConstants.HeaderTitle]))
                    {
                        pageTitle = titleWebPart.Properties[SharePointConstants.HeaderTitle];
                    }
                    else
                    {
                        pageTitle = ExtractPageTitleFromFileName(pageItem);
                    }
                    break;
                case SourcePageType.PublishingPage:
                    string publishingTitleValue = null;
                    if (pageItem.TryGetFieldValue<string>(SharePointConstants.TitleField, out publishingTitleValue))
                    {
                        pageTitle = publishingTitleValue ?? string.Empty;
                    }
                    else
                    {
                        pageTitle = ExtractPageTitleFromFileName(pageItem);
                    }
                    break;
                default:
                    break;
            }

            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                SharePointTransformationResources.Info_TransformPageModernTitle, pageFile.ServerRelativeUrl, pageTitle));

            return pageTitle;
        }

        private static string ExtractPageTitleFromFileName(ListItem pageItem)
        {
            string pageTitle = null;

            string fileLeafRefValue = null;
            if (pageItem.TryGetFieldValue<string>(SharePointConstants.FileLeafRefField, out fileLeafRefValue))
            {
                pageTitle = Path.GetFileNameWithoutExtension(fileLeafRefValue);
                if (!string.IsNullOrEmpty(pageTitle))
                {
                    pageTitle = pageTitle.First().ToString().ToUpper() + pageTitle.Substring(1);
                }
            }

            return pageTitle;
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
                else if (sourcePage.PageType == SourcePageType.PublishingPage)
                {
                    var pagesLibraryName = sourceContext.Web.GetLocalizedListName(ListType.PublishingPages, "pages");
                    if (fileRefFieldValue.ContainsIgnoringCasing($"/{pagesLibraryName}"))
                    {
                        pageFolder = fileRefFieldValue.Replace($"{sourceContext.Web.ServerRelativeUrl.TrimEnd(new[] { '/' })}/{pagesLibraryName}", "", StringComparison.InvariantCultureIgnoreCase).Trim();
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

        private SourcePageType EvaluatePageType(Microsoft.SharePoint.Client.File pageFile, ListItem pageItem, bool crossSiteTransformation, bool crossTenantTransformation)
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

        private void ValidatePageType(Microsoft.SharePoint.Client.File pageFile, SourcePageType pageType, bool crossSiteTransformation, bool crossTenantTransformation)
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

        /// <summary>
        /// Analyzes the current page to provide information about web parts and page layout
        /// </summary>
        /// <returns>The page layout and the list of web parts</returns>
        internal async Task<Tuple<Model.PageLayout, List<WebPartEntity>>> AnalyzePageAsync(
            PageTransformationContext context,
            Microsoft.SharePoint.Client.File pageFile,
            ListItem pageItem,
            SourcePageInformation page)
        {
            // Prepare the result variables
            List<WebPartEntity> webparts = null;
            Model.PageLayout layout = Model.PageLayout.Unknown;

            switch (page.PageType)
            {
                case SourcePageType.WebPartPage when page.SourceVersion != SPVersion.SPO:
                    (layout, webparts) = await AnalyzeWebPartPageOnPremisesAsync(context, pageFile, page).ConfigureAwait(false);
                    break;
                case SourcePageType.WebPartPage:
                    (layout, webparts) = await AnalyzeWebPartPageAsync(context, pageFile, page).ConfigureAwait(false);
                    break;
                case SourcePageType.WikiPage:
                    (layout, webparts) = await AnalyzeWikiPageAsync(context, pageFile, pageItem, page).ConfigureAwait(false);
                    break;
                case SourcePageType.BlogPage:
                    (layout, webparts) = await AnalyzeWikiPageAsync(context, pageFile, pageItem, page).ConfigureAwait(false);
                    break;
                case SourcePageType.PublishingPage when page.SourceVersion != SPVersion.SPO:
                    (layout, webparts) = await AnalyzePublishingPageOnPremisesAsync(context, pageFile, pageItem, page).ConfigureAwait(false);
                    break;
                case SourcePageType.PublishingPage:
                    (layout, webparts) = await AnalyzePublishingPageAsync(context, pageFile, pageItem, page).ConfigureAwait(false);
                    break;
            }

            return new Tuple<Model.PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        private async Task<Tuple<Model.PageLayout, List<WebPartEntity>>> AnalyzeWebPartPageAsync(PageTransformationContext context, Microsoft.SharePoint.Client.File pageFile, SourcePageInformation page)
        {
            // Prepare the result variable
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            // Get a reference to the current source context
            var clientContext = pageFile.Context;

            // Load web parts on web part page
            // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. There's no API that will return those,
            //       only possible option to add parsing of the raw page aspx file.
            var limitedWPManager = pageFile.GetLimitedWebPartManager(PersonalizationScope.Shared);
            clientContext.Load(limitedWPManager);

            // Load page properties
            var pageProperties = pageFile.Properties;
            clientContext.Load(pageProperties);

            // Load the web parts properties
            IEnumerable<WebPartDefinition> webParts;

            webParts = clientContext.LoadQuery(
            limitedWPManager.WebParts.IncludeWithDefaultProperties(
                wp => wp.Id,
                wp => wp.ZoneId,
                wp => wp.WebPart.ExportMode,
                wp => wp.WebPart.Title,
                wp => wp.WebPart.ZoneIndex,
                wp => wp.WebPart.IsClosed,
                wp => wp.WebPart.Hidden,
                wp => wp.WebPart.Properties));

            await clientContext.ExecuteQueryAsync().ConfigureAwait(false);

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
                    await clientContext.ExecuteQueryAsync().ConfigureAwait(false);
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

            return new Tuple<Model.PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        private async Task<Tuple<Model.PageLayout, List<WebPartEntity>>> AnalyzeWebPartPageOnPremisesAsync(PageTransformationContext context, Microsoft.SharePoint.Client.File pageFile, SourcePageInformation page)
        {
            // Prepare the result variable
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            // Get a reference to the current source context
            var clientContext = pageFile.Context;

            // Load web parts on web part page
            // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. There's no API that will return those,
            //       only possible option to add parsing of the raw page aspx file.
            var limitedWPManager = pageFile.GetLimitedWebPartManager(PersonalizationScope.Shared);
            clientContext.Load(limitedWPManager);

            // Load page properties
            var pageProperties = pageFile.Properties;
            clientContext.Load(pageProperties);

            // Load the web parts properties
            IEnumerable<WebPartDefinition> webParts;

            webParts = clientContext.LoadQuery(
            limitedWPManager.WebParts.IncludeWithDefaultProperties(
                wp => wp.Id,
                wp => wp.ZoneId,
                wp => wp.WebPart.ExportMode,
                wp => wp.WebPart.Title,
                wp => wp.WebPart.ZoneIndex,
                wp => wp.WebPart.IsClosed,
                wp => wp.WebPart.Hidden,
                wp => wp.WebPart.Properties));

            await clientContext.ExecuteQueryAsync().ConfigureAwait(false);

            // Check page type
            var layout = GetLayoutFromWebServices(pageFile);

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

                    var webPartXml = ExportWebPartXmlWorkaround(pageFile, foundWebPart.WebPartDefinition.Id.ToString());
                    foundWebPart.WebPartXmlOnPremises = webPartXml;
                }

                foreach (var foundWebPart in webPartsToRetrieve)
                {
                    Dictionary<string, object> webPartProperties = null;

                    string zoneId = foundWebPart.WebPartDefinition.ZoneId;
                    webPartProperties = foundWebPart.WebPartDefinition.WebPart.Properties.FieldValues;

                    // TODO: Ask Bert -> isn't this a repetition?
                    if (foundWebPart.WebPartDefinition.ZoneId.Equals("TitleBar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!options.Value.IncludeTitleBarWebPart)
                        {
                            continue;
                        }
                    }

                    if (string.IsNullOrEmpty(foundWebPart.WebPartXmlOnPremises))
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        foundWebPart.WebPartType = GetTypeFromProperties(webPartProperties, true);
                    }
                    else
                    {
                        foundWebPart.WebPartType = GetType(foundWebPart.WebPartXmlOnPremises);
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

            return new Tuple<Model.PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        private async Task<Tuple<Model.PageLayout, List<WebPartEntity>>> AnalyzeWikiPageAsync(PageTransformationContext context, Microsoft.SharePoint.Client.File wikiPage, ListItem pageItem, SourcePageInformation page)
        {
            List<WebPartEntity> webparts = new List<WebPartEntity>();
            string pageContents = null;

            if (page.PageType == SourcePageType.BlogPage)
            {
                pageContents = pageItem.FieldValues[SharePointConstants.BodyField].ToString();
                if (string.IsNullOrEmpty(pageContents))
                {
                    throw new ApplicationException(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_InvalidOrMissingBlogContent,
                        wikiPage.ServerRelativeUrl));
                }
            }
            else if (page.PageType == SourcePageType.WikiPage)
            {
                pageContents = pageItem.FieldValues[SharePointConstants.WikiField].ToString();
                if (string.IsNullOrEmpty(pageContents))
                {
                    throw new ApplicationException(string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_InvalidOrMissingWikiContent,
                        wikiPage.ServerRelativeUrl));
                }
            }

            HtmlParser parser = new HtmlParser();

            var layout = Model.PageLayout.Wiki_OneColumn;

            if (!string.IsNullOrEmpty(pageContents))
            {
                var htmlDoc = parser.ParseDocument(pageContents);
                layout = GetWikiLayout(htmlDoc);

                List<WebPartPlaceHolder> webPartsToRetrieve = new List<WebPartPlaceHolder>();

                var rows = htmlDoc.All.Where(p => p.LocalName == "tr");
                int rowCount = 0;

                foreach (var row in rows)
                {
                    rowCount++;
                    var columns = row.Children.Where(p => p.LocalName == "td" && p.Parent == row);

                    int colCount = 0;
                    foreach (var column in columns)
                    {
                        colCount++;
                        var contentHost = column.Children.Where(p => p.LocalName == "div" && (p.ClassName != null && p.ClassName.Equals("ms-rte-layoutszone-outer", StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();

                        // Check if this element is nested in another already processed element...this needs to be skipped to avoid content duplication and possible processing errors
                        if (contentHost != null && contentHost.FirstElementChild != null && !IsNestedLayoutsZoneOuter(contentHost))
                        {
                            var content = contentHost.FirstElementChild;
                            AnalyzeWikiContentBlock(parser, webparts, htmlDoc, webPartsToRetrieve, rowCount, colCount, 0, content);
                        }
                    }
                }

                // Bulk load the needed web part information
                if (webPartsToRetrieve.Count > 0)
                {
                    if (page.SourceVersion == SPVersion.SP2010 ||
                        page.SourceVersion == SPVersion.SP2013Legacy ||
                        page.SourceVersion == SPVersion.SP2016Legacy)
                    {
                        LoadWebPartsInWikiContentFromOnPremisesServer(webparts, wikiPage, webPartsToRetrieve);
                    }
                    else
                    {
                        LoadWebPartsInWikiContentFromServer(webparts, wikiPage, webPartsToRetrieve);
                    }
                }
                else
                {
                    logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_AnalysingNoWebPartsFound,
                        wikiPage.ServerRelativeUrl));
                }

                // Somehow the wiki was not standard formatted, so lets wrap its contents in a text block
                if (webparts.Count == 0 && !String.IsNullOrEmpty(htmlDoc.Source.Text))
                {
                    webparts.Add(CreateWikiTextPart(htmlDoc.Source.Text, 1, 1, 1));
                }
            }

            return new Tuple<Model.PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        private async Task<Tuple<Model.PageLayout, List<WebPartEntity>>> AnalyzePublishingPageAsync(PageTransformationContext context, Microsoft.SharePoint.Client.File pageFile, ListItem pageItem, SourcePageInformation page)
        {
            // Prepare the result variable
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            // Get a reference to the current source context
            var sourceContext = pageFile.Context;

            // Determine the source page layout
            string pageLayoutUrl;
            if (pageItem.TryGetFieldValue(SharePointConstants.PublishingPageLayoutField, out pageLayoutUrl))
            {
                page.PageLayout = System.IO.Path.GetFileNameWithoutExtension(pageLayoutUrl);
            }

            // Here we need to invoke the Page Layout Mapping Provider
            // to determine the target page layout
            // => Where do we store the result?
            // => Should we pass the cancellation token?
            var pageLayoutMappingProvider = serviceProvider.GetService<IPageLayoutMappingProvider>();
            if (pageLayoutMappingProvider != null)
            {
                var pageLayoutInput = new PageLayoutMappingProviderInput(context);
                pageLayoutInput.PageLayout = page.PageLayout;
                var output = await pageLayoutMappingProvider
                    .MapPageLayoutAsync(pageLayoutInput)
                    .ConfigureAwait(false);
            }


            //#region Process fields that become web parts 
            //if (publishingPageTransformationModel.WebParts != null)
            //{
            //    #region Publishing Html column processing
            //    // Converting to WikiTextPart is a special case as we'll need to process the html
            //    var wikiTextWebParts = publishingPageTransformationModel.WebParts.Where(p => p.TargetWebPart.Equals(WebParts.WikiText, StringComparison.InvariantCultureIgnoreCase));
            //    List<WebPartPlaceHolder> webPartsToRetrieve = new List<WebPartPlaceHolder>();
            //    foreach (var wikiTextPart in wikiTextWebParts)
            //    {
            //        string pageContents = page.GetFieldValueAs<string>(wikiTextPart.Name);

            //        if (wikiTextPart.Property.Length > 0)
            //        {
            //            foreach (var fieldWebPartProperty in wikiTextPart.Property)
            //            {
            //                if (fieldWebPartProperty.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(fieldWebPartProperty.Functions))
            //                {
            //                    // execute function
            //                    var evaluatedField = this.functionProcessor.Process(fieldWebPartProperty.Functions, fieldWebPartProperty.Name, MapToFunctionProcessorFieldType(fieldWebPartProperty.Type));
            //                    if (!string.IsNullOrEmpty(evaluatedField.Item1))
            //                    {
            //                        pageContents = evaluatedField.Item2;
            //                    }
            //                }
            //            }
            //        }

            //        if (pageContents != null && !string.IsNullOrEmpty(pageContents))
            //        {
            //            var htmlDoc = parser.ParseDocument(pageContents);

            //            // Analyze the html block (which is a wiki block)
            //            var content = htmlDoc.FirstElementChild.LastElementChild;
            //            AnalyzeWikiContentBlock(webparts, htmlDoc, webPartsToRetrieve, wikiTextPart.Row, wikiTextPart.Column, GetNextOrder(wikiTextPart.Row, wikiTextPart.Column, wikiTextPart.Order, webparts), content);
            //        }
            //        else
            //        {
            //            LogWarning(LogStrings.Warning_CannotRetrieveFieldValue, LogStrings.Heading_PublishingPage);
            //        }
            //    }

            //    // Bulk load the needed web part information
            //    if (webPartsToRetrieve.Count > 0)
            //    {
            //        LoadWebPartsInWikiContentFromServer(webparts, publishingPage, webPartsToRetrieve);
            //    }
            //    #endregion

            //    #region Generic processing of the other 'webpart' fields
            //    var fieldWebParts = publishingPageTransformationModel.WebParts.Where(p => !p.TargetWebPart.Equals(WebParts.WikiText, StringComparison.InvariantCultureIgnoreCase));
            //    foreach (var fieldWebPart in fieldWebParts.OrderBy(p => p.Row).OrderBy(p => p.Column))
            //    {
            //        // In publishing scenarios it's common to not have all fields defined in the page layout mapping filled. By default we'll not map empty fields as that will result in empty web parts
            //        // which impact the page look and feel. Using the RemoveEmptySectionsAndColumns flag this behaviour can be turned off.
            //        if (this.baseTransformationInformation.RemoveEmptySectionsAndColumns)
            //        {
            //            var fieldContents = page.GetFieldValueAs<string>(fieldWebPart.Name);

            //            if (string.IsNullOrEmpty(fieldContents))
            //            {
            //                LogWarning(String.Format(LogStrings.Warning_SkippedWebPartDueToEmptyInSourcee, fieldWebPart.TargetWebPart, fieldWebPart.Name), LogStrings.Heading_PublishingPage);
            //                continue;
            //            }
            //        }

            //        Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            //        foreach (var fieldWebPartProperty in fieldWebPart.Property)
            //        {
            //            if (!string.IsNullOrEmpty(fieldWebPartProperty.Functions))
            //            {
            //                // execute function
            //                var evaluatedField = this.functionProcessor.Process(fieldWebPartProperty.Functions, fieldWebPartProperty.Name, MapToFunctionProcessorFieldType(fieldWebPartProperty.Type));
            //                if (!string.IsNullOrEmpty(evaluatedField.Item1) && !properties.ContainsKey(evaluatedField.Item1))
            //                {
            //                    properties.Add(evaluatedField.Item1, evaluatedField.Item2);
            //                }
            //            }
            //            else
            //            {
            //                var webPartName = page.FieldValues[fieldWebPart.Name]?.ToString().Trim();
            //                if (webPartName != null)
            //                {
            //                    properties.Add(fieldWebPartProperty.Name, page.FieldValues[fieldWebPart.Name].ToString().Trim());
            //                }
            //            }
            //        }

            //        var wpEntity = new WebPartEntity()
            //        {
            //            Title = fieldWebPart.Name,
            //            Type = fieldWebPart.TargetWebPart,
            //            Id = Guid.Empty,
            //            Row = fieldWebPart.Row,
            //            Column = fieldWebPart.Column,
            //            Order = GetNextOrder(fieldWebPart.Row, fieldWebPart.Column, fieldWebPart.Order, webparts),
            //            Properties = properties,
            //        };

            //        webparts.Add(wpEntity);
            //    }
            //    #endregion
            //}
            //#endregion


            return null;
        }

        private async Task<Tuple<PageLayout, List<WebPartEntity>>> AnalyzePublishingPageOnPremisesAsync(PageTransformationContext context, Microsoft.SharePoint.Client.File pageFile, ListItem pageItem, SourcePageInformation page)
        {
            return null;
        }

        /// <summary>
        /// Check if this element is nested in another already processed element...this needs to be skipped to avoid content duplication and possible processing errors
        /// </summary>
        /// <param name="contentHost">element to check</param>
        /// <returns>true if embedded in a already processed element</returns>
        private static bool IsNestedLayoutsZoneOuter(IElement contentHost)
        {
            if (contentHost == null)
            {
                return false;
            }

            var elementToInspect = contentHost.ParentElement;
            if (elementToInspect == null)
            {
                return false;
            }

            while (elementToInspect != null)
            {
                if (elementToInspect.LocalName == "div" && (elementToInspect.ClassName != null && elementToInspect.ClassName.Equals("ms-rte-layoutszone-outer", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return true;
                }
                else
                {
                    elementToInspect = elementToInspect.ParentElement;
                }
            }

            return false;
        }

        private static void AnalyzeWikiContentBlock(HtmlParser parser, List<WebPartEntity> webparts, IHtmlDocument htmlDoc, List<WebPartPlaceHolder> webPartsToRetrieve, int rowCount, int colCount, int startOrder, IElement content)
        {
            // Drop elements which we anyhow can't transform and/or which are stripped out from RTE
            CleanHtml(content, htmlDoc);

            StringBuilder textContent = new StringBuilder();
            int order = startOrder;
            foreach (var node in content.ChildNodes)
            {
                // Do we find a web part inside...
                if (((node as IHtmlElement) != null) && ContainsWebPart(parser, node as IHtmlElement))
                {
                    var extraText = StripWebPart(parser, node as IHtmlElement);
                    string extraTextAfterWebPart = null;
                    string extraTextBeforeWebPart = null;
                    if (!string.IsNullOrEmpty(extraText))
                    {
                        // Should be, but checking anyhow
                        int webPartMarker = extraText.IndexOf(webPartMarkerString);
                        if (webPartMarker > -1)
                        {
                            extraTextBeforeWebPart = extraText.Substring(0, webPartMarker);
                            extraTextAfterWebPart = extraText.Substring(webPartMarker + webPartMarkerString.Length);

                            // there could have been multiple web parts in a row (we don't support text inbetween them for now)...strip the remaining markers
                            extraTextBeforeWebPart = extraTextBeforeWebPart.Replace(webPartMarkerString, "");
                            extraTextAfterWebPart = extraTextAfterWebPart.Replace(webPartMarkerString, "");
                        }
                    }

                    if (!string.IsNullOrEmpty(extraTextBeforeWebPart))
                    {
                        textContent.AppendLine(extraTextBeforeWebPart);
                    }

                    // first insert text part (if it was available)
                    if (!string.IsNullOrEmpty(textContent.ToString()))
                    {
                        order++;
                        webparts.Add(CreateWikiTextPart(textContent.ToString(), rowCount, colCount, order));
                        textContent.Clear();
                    }

                    // then process the web part
                    order++;
                    Regex regexClientIds = new Regex(@"id=\""div_(?<ControlId>(\w|\-)+)");
                    if (regexClientIds.IsMatch((node as IHtmlElement).OuterHtml))
                    {
                        foreach (Match webPartMatch in regexClientIds.Matches((node as IHtmlElement).OuterHtml))
                        {
                            // Store the web part we need, will be retrieved afterwards to optimize performance
                            string serverSideControlId = webPartMatch.Groups["ControlId"].Value;
                            var serverSideControlIdToSearchFor = $"g_{serverSideControlId.Replace("-", "_")}";
                            webPartsToRetrieve.Add(new WebPartPlaceHolder() { ControlId = serverSideControlIdToSearchFor, Id = serverSideControlId, Row = rowCount, Column = colCount, Order = order });
                        }
                    }

                    // Process the extra text that was positioned after the web part (if any)
                    if (!string.IsNullOrEmpty(extraTextAfterWebPart))
                    {
                        textContent.AppendLine(extraTextAfterWebPart);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(node.TextContent.Trim()) && node.TextContent.Trim() == "\n")
                    {
                        // ignore, this one is typically added after a web part
                    }
                    else
                    {
                        if (node.HasChildNodes)
                        {
                            textContent.AppendLine((node as IHtmlElement).OuterHtml);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(node.TextContent.Trim()))
                            {
                                textContent.AppendLine(node.TextContent);
                            }
                            else
                            {
                                if (node.NodeName.Equals("br", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    textContent.AppendLine("<BR>");
                                }
                                // given that wiki html can contain embedded images and videos while not having child nodes we need include these.
                                // case: img/iframe tag as "only" element to evaluate (e.g. first element in the contenthost)
                                else if (node.NodeName.Equals("img", StringComparison.InvariantCultureIgnoreCase) ||
                                         node.NodeName.Equals("iframe", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    textContent.AppendLine((node as IHtmlElement).OuterHtml);
                                }
                            }
                        }
                    }
                }
            }

            // there was only one text part
            if (!string.IsNullOrEmpty(textContent.ToString()))
            {
                // insert text part to the web part collection
                order++;
                webparts.Add(CreateWikiTextPart(textContent.ToString(), rowCount, colCount, order));
            }
        }

        private static void CleanHtml(IElement element, IHtmlDocument document)
        {
            foreach (var node in element.QuerySelectorAll("*").ToList())
            {
                if (node.ParentElement != null && IsUntransformableBlockElement(node))
                {
                    // create new div node and add all current children to it
                    var div = document.CreateElement("div");
                    foreach (var child in node.ChildNodes.ToList())
                    {
                        div.AppendChild(child);
                    }
                    // replace the unsupported node with the new div
                    node.ParentElement.ReplaceChild(div, node);
                }
            }
        }

        private static bool IsUntransformableBlockElement(IElement element)
        {
            var tag = element.TagName.ToLower();
            if (tag == "article" ||
                tag == "address" ||
                tag == "aside" ||
                tag == "canvas" ||
                tag == "dd" ||
                tag == "dl" ||
                tag == "dt" ||
                tag == "fieldset" ||
                tag == "figcaption" ||
                tag == "figure" ||
                tag == "footer" ||
                tag == "form" ||
                tag == "header" ||
                //tag == "hr" || // will be replaced at in the html transformator
                tag == "main" ||
                tag == "nav" ||
                tag == "noscript" ||
                tag == "output" ||
                tag == "pre" ||
                tag == "section" ||
                tag == "tfoot" ||
                tag == "video" ||
                tag == "aside")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Does the tree of nodes somewhere contain a web part?
        /// </summary>
        /// <param name="parser">Html parser</param>
        /// <param name="element">Html content to analyze</param>
        /// <returns>True if it contains a web part</returns>
        private static bool ContainsWebPart(HtmlParser parser, IHtmlElement element)
        {
            var doc = parser.ParseDocument(element.OuterHtml);
            var nodes = doc.All.Where(p => p.LocalName == "div");
            foreach (var node in nodes)
            {
                if (((node as IHtmlElement) != null) && (node as IHtmlElement).ClassList.Contains("ms-rte-wpbox"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Strips the div holding the web part from the html
        /// </summary>
        /// <param name="parser">Html parser</param>
        /// <param name="element">Html element holding one or more web part divs</param>
        /// <returns>Cleaned html with a placeholder for the web part div</returns>
        private static string StripWebPart(HtmlParser parser, IHtmlElement element)
        {
            IElement copy = element.Clone(true) as IElement;
            var doc = parser.ParseDocument(copy.OuterHtml);
            var nodes = doc.All.Where(p => p.LocalName == "div");
            if (nodes.Any())
            {
                foreach (var node in nodes.ToList())
                {
                    if (((node as IHtmlElement) != null) && (node as IHtmlElement).ClassList.Contains("ms-rte-wpbox"))
                    {
                        var newElement = doc.CreateTextNode(webPartMarkerString);
                        node.Parent.ReplaceChild(newElement, node);
                    }
                }

                if (doc.DocumentElement.Children[1].FirstElementChild != null &&
                    doc.DocumentElement.Children[1].FirstElementChild is IHtmlDivElement)
                {
                    return doc.DocumentElement.Children[1].FirstElementChild.InnerHtml;
                }
                else
                {
                    return doc.DocumentElement.Children[1].InnerHtml;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Stores text content as a fake web part
        /// </summary>
        /// <param name="wikiTextPartContent">Text to store</param>
        /// <param name="row">Row of the fake web part</param>
        /// <param name="col">Column of the fake web part</param>
        /// <param name="order">Order inside the row/column</param>
        /// <returns>A web part entity to add to the collection</returns>
        private static WebPartEntity CreateWikiTextPart(string wikiTextPartContent, int row, int col, int order)
        {
            var result = new WebPartEntity()
            {
                Title = "WikiText",
                Type = "SharePointPnP.Modernization.WikiTextPart",
                Id = Guid.Empty,
                Row = row,
                Column = col,
                Order = order
            };

            result.Properties.Add("Text", wikiTextPartContent.Trim().Replace("\r\n", string.Empty));

            return result;
        }

        /// <summary>
        /// Exports Web Part XML via an older workround
        /// </summary>
        /// <param name="pageFile">The file of the page</param>
        /// <param name="webPartGuid">The ID of the web part</param>
        /// <returns>The XML of the web part</returns>
        internal string ExportWebPartXmlWorkaround(Microsoft.SharePoint.Client.File pageFile, string webPartGuid)
        {
            // Issue hints and Credit: 
            //      https://blog.mastykarz.nl/export-web-parts-csom/ 
            //      https://sharepoint.stackexchange.com/questions/30865/missing-export-option-for-sharepoint-2010-webparts
            //      https://github.com/SharePoint/PnP-Sites-Core/pull/908/files

            var context = pageFile.Context as ClientContext;
            if (context == null)
            {
                throw new ApplicationException(SharePointTransformationResources.Error_InvalidSourceContext);
            }

            try
            {
                logger.LogInformation(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_RetreivingExportWebPartXmlWorkaround,
                    webPartGuid, pageFile.ServerRelativeUrl));

                string webPartXml = string.Empty;
                string serverRelativeUrl = context.Web.EnsureProperty(w => w.ServerRelativeUrl);
                var uri = new Uri(context.Site.EnsureProperty(s => s.Url));

                var fullWebUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}{serverRelativeUrl}";
                var fullPageUrl = $"{uri.Scheme}://{uri.Host}:{uri.Port}{pageFile.ServerRelativeUrl}";

                if (!fullWebUrl.EndsWith("/"))
                {
                    fullWebUrl = fullWebUrl + "/";
                }

                string webServiceUrl = $"{fullWebUrl}_vti_bin/exportwp.aspx?pageurl={fullPageUrl}&guidstring={webPartGuid}";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webServiceUrl); //hack to force webpart zones to render
                //request.Credentials = context.Credentials;
                request.AddAuthenticationData(context);
                request.Method = "GET";

                var response = request.GetResponse();
                using (var dataStream = response.GetResponseStream())
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(dataStream);

                    if (xDoc.DocumentElement != null && xDoc.DocumentElement.OuterXml.Length > 0)
                    {
                        webPartXml = xDoc.DocumentElement.OuterXml;

                        // Not sure what causes the web parts to switch from singular to multiple
                        if (xDoc.DocumentElement.LocalName == "webParts")
                        {
                            webPartXml = xDoc.DocumentElement.InnerXml;
                        }

                        return webPartXml;
                    }
                }
            }
            catch (WebException ex)
            {
                logger.LogInformation(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_WebPartXmlNotExported,
                    webPartGuid, pageFile.ServerRelativeUrl, ex.Message));
            }

            return string.Empty;
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
        /// Analyzes the wiki page to determine which layout was used
        /// </summary>
        /// <param name="doc">html object</param>
        /// <returns>Layout of the wiki page</returns>
        private static PageLayout GetWikiLayout(IHtmlDocument doc)
        {
            string spanValue = "";
            var spanTags = doc.All.Where(p => p.LocalName == "span" && p.HasAttribute("id"));
            if (spanTags.Any())
            {
                foreach (var span in spanTags)
                {
                    if (span.GetAttribute("id").Equals("layoutsdata", StringComparison.InvariantCultureIgnoreCase))
                    {
                        spanValue = span.InnerHtml.ToLower();

                        if (spanValue == "false,false,1")
                        {
                            return PageLayout.Wiki_OneColumn;
                        }
                        else if (spanValue == "false,false,2")
                        {
                            var tdTag = doc.All.Where(p => p.LocalName == "td" && p.HasAttribute("style")).FirstOrDefault();
                            if (tdTag != null)
                            {
                                if (tdTag.GetAttribute("style").IndexOf("width:49.95%;", StringComparison.InvariantCultureIgnoreCase) > -1)
                                {
                                    return PageLayout.Wiki_TwoColumns;
                                }
                                else if (tdTag.GetAttribute("style").IndexOf("width:66.6%;", StringComparison.InvariantCultureIgnoreCase) > -1)
                                {
                                    return PageLayout.Wiki_TwoColumnsWithSidebar;
                                }
                                else
                                {
                                    return PageLayout.Wiki_TwoColumns;
                                }
                            }
                        }
                        else if (spanValue == "true,false,2")
                        {
                            return PageLayout.Wiki_TwoColumnsWithHeader;
                        }
                        else if (spanValue == "true,true,2")
                        {
                            return PageLayout.Wiki_TwoColumnsWithHeaderAndFooter;
                        }
                        else if (spanValue == "false,false,3")
                        {
                            return PageLayout.Wiki_ThreeColumns;
                        }
                        else if (spanValue == "true,false,3")
                        {
                            return PageLayout.Wiki_ThreeColumnsWithHeader;
                        }
                        else if (spanValue == "true,true,3")
                        {
                            return PageLayout.Wiki_ThreeColumnsWithHeaderAndFooter;
                        }
                    }
                }
            }

            // Oops, we're still here...let's try to deduct a layout as some pages (e.g. from community template) do not add the proper span value
            if (spanValue.StartsWith("false,false,") || spanValue.StartsWith("true,true,") || spanValue.StartsWith("true,false,"))
            {
                // false,false,&#123;0&#125; case..let's try to count the columns via the TD tag data
                var tdTags = doc.All.Where(p => p.LocalName == "td" && p.HasAttribute("style"));
                if (spanValue.StartsWith("false,false,"))
                {
                    if (tdTags.Count() == 1)
                    {
                        return PageLayout.Wiki_OneColumn;
                    }
                    else if (tdTags.Count() == 2)
                    {
                        if (tdTags.First().GetAttribute("style").IndexOf("width:49.95%;", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            return PageLayout.Wiki_TwoColumns;
                        }
                        else if (tdTags.First().GetAttribute("style").IndexOf("width:66.6%;", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            return PageLayout.Wiki_TwoColumnsWithSidebar;
                        }
                        else
                        {
                            return PageLayout.Wiki_TwoColumns;
                        }
                    }
                    else if (tdTags.Count() == 3)
                    {
                        return PageLayout.Wiki_ThreeColumns;
                    }
                }
                else if (spanValue.StartsWith("true,true,"))
                {
                    if (tdTags.Count() == 2)
                    {
                        return PageLayout.Wiki_TwoColumnsWithHeaderAndFooter;
                    }
                    else if (tdTags.Count() == 3)
                    {
                        return PageLayout.Wiki_ThreeColumnsWithHeaderAndFooter;
                    }
                }
                else if (spanValue.StartsWith("true,false,"))
                {
                    if (tdTags.Count() == 2)
                    {
                        return PageLayout.Wiki_TwoColumnsWithHeader;
                    }
                    else if (tdTags.Count() == 3)
                    {
                        return PageLayout.Wiki_ThreeColumnsWithHeader;
                    }
                }
            }

            return PageLayout.Wiki_Custom;
        }

        /// <summary>
        /// Gets and parses the layout from the web services URL
        /// </summary>
        /// <param name="pageFile">The page file</param>
        /// <returns></returns>
        private PageLayout GetLayoutFromWebServices(Microsoft.SharePoint.Client.File pageFile)
        {
            var wsPageDocument = ExtractWebPartDocumentViaWebServicesFromPage(pageFile);

            if (!string.IsNullOrEmpty(wsPageDocument.Item1))
            {
                //Example fragment from WS
                //<li>vti_setuppath
                //<li>SR|1033&#92;STS&#92;doctemp&#92;smartpgs&#92;spstd2.aspx
                //<li>vti_generator

                var fullDocument = wsPageDocument.Item1;

                if (!string.IsNullOrEmpty(fullDocument))
                {
                    if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd1.aspx"))
                    {
                        return PageLayout.WebPart_FullPageVertical;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd2.aspx"))
                    {
                        return PageLayout.WebPart_HeaderFooterThreeColumns;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd3.aspx"))
                    {
                        return PageLayout.WebPart_HeaderLeftColumnBody;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd4.aspx"))
                    {
                        return PageLayout.WebPart_HeaderRightColumnBody;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd5.aspx"))
                    {
                        return PageLayout.WebPart_HeaderFooter2Columns4Rows;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd6.aspx"))
                    {
                        return PageLayout.WebPart_HeaderFooter4ColumnsTopRow;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd7.aspx"))
                    {
                        return PageLayout.WebPart_LeftColumnHeaderFooterTopRow3Columns;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"STS&#92;doctemp&#92;smartpgs&#92;spstd8.aspx"))
                    {
                        return PageLayout.WebPart_RightColumnHeaderFooterTopRow3Columns;
                    }
                    else if (fullDocument.ContainsIgnoringCasing(@"SiteTemplates&#92;STS&#92;default.aspx"))
                    {
                        return PageLayout.WebPart_2010_TwoColumnsLeft;
                    }
                }

            }

            return PageLayout.WebPart_Custom;
        }

        /// <summary>
        /// Call SharePoint Web Services to extract web part properties not exposed by CSOM
        /// </summary>
        /// <returns></returns>
        private Tuple<string, string> ExtractWebPartDocumentViaWebServicesFromPage(Microsoft.SharePoint.Client.File pageFile)
        {
            var pageUrl = pageFile.EnsureProperty(p => p.ServerRelativeUrl);
            var context = pageFile.Context as ClientContext;

            try
            {
                logger.LogInformation(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Info_CallingWebServicesToExtractWebPartsFromPage,
                    pageFile.ServerRelativeUrl));

                string webUrl = context.Web.EnsureProperty(p => p.Url);
                string webServiceUrl = webUrl + "/_vti_bin/WebPartPages.asmx";

                StringBuilder soapEnvelope = new StringBuilder();

                soapEnvelope.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                soapEnvelope.Append("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");

                soapEnvelope.Append(String.Format(
                 "<soap:Body>" +
                     "<GetWebPartPage xmlns=\"http://microsoft.com/sharepoint/webpartpages\">" +
                         "<documentName>{0}</documentName>" +
                         "<behavior>Version3</behavior>" +
                     "</GetWebPartPage>" +
                 "</soap:Body>"
                 , pageUrl));

                soapEnvelope.Append("</soap:Envelope>");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(webServiceUrl);
                //request.Credentials = cc.Credentials;
                request.AddAuthenticationData(context);
                request.Method = "POST";
                request.ContentType = "text/xml; charset=\"utf-8\"";
                request.Accept = "text/xml";
                request.Headers.Add("SOAPAction", "\"http://microsoft.com/sharepoint/webpartpages/GetWebPartPage\"");

                using (System.IO.Stream stream = request.GetRequestStream())
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(stream))
                    {
                        writer.Write(soapEnvelope.ToString());
                    }
                }

                var response = request.GetResponse();
                using (var dataStream = response.GetResponseStream())
                {
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.Load(dataStream);

                    if (xDoc.DocumentElement != null && xDoc.DocumentElement.InnerText.Length > 0)
                    {
                        var webPartPageContents = xDoc.DocumentElement.InnerText;
                        //Remove the junk from the result
                        var tag = "<HasByteOrderMark/>";
                        var marker = webPartPageContents.IndexOf(tag);
                        var partDocument = string.Empty;
                        if (marker > -1)
                        {
                            partDocument = webPartPageContents.Substring(marker).Replace(tag, "");
                        }

                        return new Tuple<string, string>(webPartPageContents, partDocument);
                    }
                }
            }
            catch (WebException ex)
            {
                logger.LogError(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    SharePointTransformationResources.Error_CallingWebServicesToExtractWebPartsFromPage,
                    pageFile.ServerRelativeUrl));
            }

            return new Tuple<string, string>(string.Empty, string.Empty);
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

        private SourcePageInformation ReadSourcePageInformation(ListItem sourcePage)
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

            // Determine if the page is the home page in the source site
            // Check if the transformed page is the web's home page
            var sourceClientContext = sourcePage.Context as ClientContext;

            if (sourceClientContext.Web.RootFolder.IsPropertyAvailable("WelcomePage") && !string.IsNullOrEmpty(sourceClientContext.Web.RootFolder.WelcomePage))
            {
                logger.LogInformation(SharePointTransformationResources.Info_WelcomePageSettingsIsPresent);

                var homePageUrl = sourceClientContext.Web.RootFolder.WelcomePage;
                var homepageName = Path.GetFileName(sourceClientContext.Web.RootFolder.WelcomePage);
                string fileLeafRef = null;
                if (sourcePage.TryGetFieldValue<string>(SharePointConstants.FileLeafRefField, out fileLeafRef))
                {
                    if (homepageName.Equals(fileLeafRef, StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.LogInformation(SharePointTransformationResources.Info_TransformSourcePageIsHomePage);
                        result.IsHomePage = true;
                    }
                    else
                    {
                        logger.LogInformation(SharePointTransformationResources.Info_TransformSourcePageIsNotHomePage);
                        result.IsHomePage = false;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Load Web Parts from Wiki Content page on On-Premises Server
        /// </summary>
        /// <param name="webparts"></param>
        /// <param name="wikiPage"></param>
        /// <param name="webPartsToRetrieve"></param>
        private void LoadWebPartsInWikiContentFromOnPremisesServer(List<WebPartEntity> webparts, Microsoft.SharePoint.Client.File wikiPage, List<WebPartPlaceHolder> webPartsToRetrieve)
        {
            var context = wikiPage.Context as ClientContext;

            // Load web part manager and use it to load each web part
            LimitedWebPartManager limitedWPManager = wikiPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            context.Load(limitedWPManager);

            // Don't load export mode as it's not available in on-premises, we'll try to complement this data afterwards with data retrieved via the web service call
            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                // Check if the web part was loaded when we loaded the web parts collection via the web part manager
                if (!Guid.TryParse(webPartToRetrieve.Id, out Guid webPartToRetrieveGuid))
                {
                    // Skip since guid is not valid
                    continue;
                }

                // Sometimes the returned wiki html contains web parts which are not anymore on the page...using the ExceptionHandlingScope 
                // we can handle these errors server side while just doing a single roundtrip
                var scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    using (scope.StartTry())
                    {
                        webPartToRetrieve.WebPartDefinition = limitedWPManager.WebParts.GetByControlId(webPartToRetrieve.ControlId);
                        context.Load(webPartToRetrieve.WebPartDefinition, wp => wp.Id, wp => wp.WebPart.Title, wp => wp.WebPart.ZoneIndex, wp => wp.WebPart.IsClosed, wp => wp.WebPart.Hidden, wp => wp.WebPart.Properties);
                    }
                    using (scope.StartCatch())
                    {

                    }
                }
            }

            context.ExecuteQueryRetry();

            // Load the web part XML for the web parts that do allow it
            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                if (webPartToRetrieve.WebPartDefinition != null)
                {
                    // Important to only process the web parts that did not return an error in the previous server call
                    if (webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.HasValue && webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.Value == false)
                    {
                        // Let's try to retrieve the web part XML...this will fail for web parts that are not exportable
                        var webPartXml = ExportWebPartXmlWorkaround(wikiPage, webPartToRetrieve.WebPartDefinition.Id.ToString());
                        webPartToRetrieve.WebPartXmlOnPremises = webPartXml;
                    }
                }
            }

            // Determine the web part type and store it in the web parts array
            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                if (webPartToRetrieve.WebPartDefinition != null && (webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.HasValue && webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.Value == false))
                {
                    Dictionary<string, object> webPartProperties = null;
                    webPartProperties = webPartToRetrieve.WebPartDefinition.WebPart.Properties.FieldValues;

                    if (string.IsNullOrEmpty(webPartToRetrieve.WebPartXmlOnPremises))
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        webPartToRetrieve.WebPartType = GetTypeFromProperties(webPartProperties, true);
                    }
                    else
                    {
                        webPartToRetrieve.WebPartType = GetType(webPartToRetrieve.WebPartXmlOnPremises);
                    }

                    logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Info_ContentTransformFoundSourceWebParts,
                        wikiPage.ServerRelativeUrl,
                        webPartToRetrieve.WebPartDefinition.WebPart.Title,
                        webPartToRetrieve.WebPartType.GetTypeShort()));

                    webparts.Add(new WebPartEntity()
                    {
                        Title = webPartToRetrieve.WebPartDefinition.WebPart.Title,
                        Type = webPartToRetrieve.WebPartType,
                        Id = webPartToRetrieve.WebPartDefinition.Id,
                        ServerControlId = webPartToRetrieve.Id,
                        Row = webPartToRetrieve.Row,
                        Column = webPartToRetrieve.Column,
                        Order = webPartToRetrieve.Order,
                        ZoneId = "",
                        ZoneIndex = (uint)webPartToRetrieve.WebPartDefinition.WebPart.ZoneIndex,
                        IsClosed = webPartToRetrieve.WebPartDefinition.WebPart.IsClosed,
                        Hidden = webPartToRetrieve.WebPartDefinition.WebPart.Hidden,
                        // Properties = Properties(webPartProperties, webPartToRetrieve.WebPartType, 
                        // webPartToRetrieve.WebPartXmlOnPremises),
                    });
                }
            }
        }

        /// <summary>
        /// Load Web Parts from Wiki Content page on Online Server
        /// </summary>
        /// <param name="webparts"></param>
        /// <param name="wikiPage"></param>
        /// <param name="webPartsToRetrieve"></param>
        internal void LoadWebPartsInWikiContentFromServer(List<WebPartEntity> webparts, Microsoft.SharePoint.Client.File wikiPage, List<WebPartPlaceHolder> webPartsToRetrieve)
        {
            var context = wikiPage.Context as ClientContext;

            // Load web part manager and use it to load each web part
            LimitedWebPartManager limitedWPManager = wikiPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            context.Load(limitedWPManager);

            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                // Check if the web part was loaded when we loaded the web parts collection via the web part manager
                if (!Guid.TryParse(webPartToRetrieve.Id, out Guid webPartToRetrieveGuid))
                {
                    // Skip since guid is not valid
                    continue;
                }

                // Sometimes the returned wiki html contains web parts which are not anymore on the page...using the ExceptionHandlingScope 
                // we can handle these errors server side while just doing a single roundtrip
                var scope = new ExceptionHandlingScope(context);
                using (scope.StartScope())
                {
                    using (scope.StartTry())
                    {
                        webPartToRetrieve.WebPartDefinition = limitedWPManager.WebParts.GetByControlId(webPartToRetrieve.ControlId);
                        context.Load(webPartToRetrieve.WebPartDefinition, wp => wp.Id, wp => wp.WebPart.ExportMode, wp => wp.WebPart.Title, wp => wp.WebPart.ZoneIndex, wp => wp.WebPart.IsClosed, wp => wp.WebPart.Hidden, wp => wp.WebPart.Properties);
                    }
                    using (scope.StartCatch())
                    {

                    }
                }
            }
            context.ExecuteQueryRetry();


            // Load the web part XML for the web parts that do allow it
            bool isDirty = false;
            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                // Important to only process the web parts that did not return an error in the previous server call
                if (webPartToRetrieve.WebPartDefinition != null && (webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.HasValue && webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.Value == false))
                {
                    // Retry to load the properties, sometimes they're not retrieved
                    webPartToRetrieve.WebPartDefinition.EnsureProperty(wp => wp.Id);
                    webPartToRetrieve.WebPartDefinition.WebPart.EnsureProperties(wp => wp.ExportMode, wp => wp.Title, wp => wp.ZoneIndex, wp => wp.IsClosed, wp => wp.Hidden, wp => wp.Properties);

                    if (webPartToRetrieve.WebPartDefinition.WebPart.ExportMode == WebPartExportMode.All)
                    {
                        webPartToRetrieve.WebPartXml = limitedWPManager.ExportWebPart(webPartToRetrieve.WebPartDefinition.Id);
                        isDirty = true;
                    }
                }
            }
            if (isDirty)
            {
                context.ExecuteQueryRetry();
            }

            // Determine the web part type and store it in the web parts array
            foreach (var webPartToRetrieve in webPartsToRetrieve)
            {
                if (webPartToRetrieve.WebPartDefinition != null && (webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.HasValue && webPartToRetrieve.WebPartDefinition.ServerObjectIsNull.Value == false))
                {
                    // Important to only process the web parts that did not return an error in the previous server call
                    if (webPartToRetrieve.WebPartDefinition.WebPart.ExportMode != WebPartExportMode.All)
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        webPartToRetrieve.WebPartType = GetTypeFromProperties(webPartToRetrieve.WebPartDefinition.WebPart.Properties.FieldValues);
                    }
                    else
                    {
                        webPartToRetrieve.WebPartType = GetType(webPartToRetrieve.WebPartXml.Value);
                    }

                    logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Info_ContentTransformFoundSourceWebParts,
                        wikiPage.ServerRelativeUrl,
                        webPartToRetrieve.WebPartDefinition.WebPart.Title,
                        webPartToRetrieve.WebPartType.GetTypeShort()));

                    webparts.Add(new WebPartEntity()
                    {
                        Title = webPartToRetrieve.WebPartDefinition.WebPart.Title,
                        Type = webPartToRetrieve.WebPartType,
                        Id = webPartToRetrieve.WebPartDefinition.Id,
                        ServerControlId = webPartToRetrieve.Id,
                        Row = webPartToRetrieve.Row,
                        Column = webPartToRetrieve.Column,
                        Order = webPartToRetrieve.Order,
                        ZoneId = "",
                        ZoneIndex = (uint)webPartToRetrieve.WebPartDefinition.WebPart.ZoneIndex,
                        IsClosed = webPartToRetrieve.WebPartDefinition.WebPart.IsClosed,
                        Hidden = webPartToRetrieve.WebPartDefinition.WebPart.Hidden,
                        // Properties = Properties(webPartToRetrieve.WebPartDefinition.WebPart.Properties.FieldValues, 
                        // webPartToRetrieve.WebPartType, webPartToRetrieve.WebPartXml == null ? "" : 
                        // webPartToRetrieve.WebPartXml.Value),
                    });
                }
            }
        }
    }
}
