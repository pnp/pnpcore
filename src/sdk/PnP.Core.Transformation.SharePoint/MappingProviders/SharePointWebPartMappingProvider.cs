using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using PnP.Core.Transformation.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Services.Builder.Configuration;
using PnP.Core.Transformation.SharePoint.MappingFiles;
using System.Xml.XPath;
using Microsoft.Extensions.Caching.Memory;
using PnP.Core.Transformation.Model;
using PnP.Core.Transformation.SharePoint.Services.MappingProviders;
using PnP.Core.Transformation.SharePoint.Functions;
using Newtonsoft.Json.Linq;

namespace PnP.Core.Transformation.SharePoint.MappingProviders
{
    /// <summary>
    /// SharePoint implementation of <see cref="IWebPartMappingProvider"/>
    /// </summary>
    public class SharePointWebPartMappingProvider : IWebPartMappingProvider
    {
        private ILogger<SharePointWebPartMappingProvider> logger;
        private readonly IOptions<SharePointTransformationOptions> options;
        private readonly IServiceProvider serviceProvider;
        private readonly IMemoryCache memoryCache;
        private readonly FunctionProcessor functionProcessor;

        class CombinedMapping
        {
            public int Order { get; set; }
            public ClientSideText ClientSideText { get; set; }
            public ClientSideWebPart ClientSideWebPart { get; set; }
        }

        /// <summary>
        /// Main constructor for the mapping provider
        /// </summary>
        /// <param name="logger">Logger for tracing activities</param>
        /// <param name="options">Configuration options</param>
        /// <param name="functionProcessor">The SharePoint Function processor</param>
        /// <param name="serviceProvider">Service provider</param>
        public SharePointWebPartMappingProvider(ILogger<SharePointWebPartMappingProvider> logger, 
            IOptions<SharePointTransformationOptions> options,
            FunctionProcessor functionProcessor,
            IServiceProvider serviceProvider)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.functionProcessor = functionProcessor ?? throw new ArgumentNullException(nameof(functionProcessor));
            this.serviceProvider = serviceProvider;
            this.memoryCache = this.serviceProvider.GetService<IMemoryCache>();
        }

        /// <summary>
        /// Maps a classic Web Part into a modern Web Part
        /// </summary>
        /// <param name="input">The input for the mapping activity</param>
        /// <param name="token">The cancellation token to use, if any</param>
        /// <returns>The output of the mapping activity</returns>
        public async Task<WebPartMappingProviderOutput> MapWebPartAsync(WebPartMappingProviderInput input, CancellationToken token = default)
        {
            // Check that we have input data
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Try to convert the mapping provider input into a typed version
            var specializedInput = input as SharePointWebPartMappingProviderInput;
            if (specializedInput == null) throw new ArgumentException(SharePointTransformationResources.Error_InvalidWebPartMappingProviderInput);

            // Configure the SharePoint Function service instance
            this.functionProcessor.Init(specializedInput.Context, specializedInput.SourceContext);

            // Define the variable holding the input web part
            var webPart = input.WebPart;

            // Prepare the output object
            var result = new WebPartMappingProviderOutput();

            logger.LogInformation($"Invoked: {this.GetType().Namespace}.{this.GetType().Name}.MapWebPartAsync");
            logger.LogInformation(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                SharePointTransformationResources.Info_ContentWebPartBeingTransformed,
                webPart.Title, webPart.TypeShort()));

            // Title bar will never be migrated
            if (input.WebPart.Type == WebParts.TitleBar)
            {
                logger.LogInformation(SharePointTransformationResources.Info_NotTransformingTitleBar);
                return result;
            }

            // Load the mapping configuration
            WebPartMapping mappingFile = LoadMappingFile(this.options.Value.WebPartMappingFile);

            var sourceItem = input.Context.SourceItem as SharePointSourceItem;

            if (sourceItem == null)
            {
                throw new ApplicationException(SharePointTransformationResources.Error_MissiningSharePointInputItem);
            }

            // Find the default mapping, will be used for webparts for which the model does not contain a mapping
            var defaultMapping = mappingFile.BaseWebPart.Mappings.Mapping.FirstOrDefault(p => p.Default == true);
            if (defaultMapping == null)
            {
                logger.LogError(SharePointTransformationResources.Error_NoDefaultMappingFound);
                throw new Exception(SharePointTransformationResources.Error_NoDefaultMappingFound);
            }

            // Assign the default mapping, if we've a more specific mapping than that will overwrite this mapping
            Mapping mapping = defaultMapping;

            // Does the web part have a mapping defined?
            var webPartData = mappingFile.WebParts.FirstOrDefault(p => p.Type.GetTypeShort() == webPart.Type.GetTypeShort());

            // Check for cross site transfer support
            if (webPartData != null && input.IsCrossSiteTransformation)
            {
                if (!webPartData.CrossSiteTransformationSupported)
                {
                    logger.LogWarning(SharePointTransformationResources.Warning_CrossSiteNotSupported);
                    return result;
                }
            }

            if (webPartData != null && webPartData.Mappings != null)
            {
                // Add site level (e.g. site) tokens to the web part properties and model so they can be used in the same manner as a web part property
                UpdateWebPartDataProperties(webPartData, mappingFile);

                string selectorResult = null;
                try
                {
                    // The mapping can have a selector function defined, if so it will be executed.
                    // If a selector was executed the selectorResult will contain the name of the mapping to use
                    logger.LogDebug(SharePointTransformationResources.Debug_ProcessingSelectorFunctions);
                    selectorResult = this.functionProcessor.Process(ref webPartData, webPart);
                }
                catch (Exception ex)
                {
                    // NotAvailableAtTargetException is used to "skip" a web part since it's not valid for the target site collection (only applies to cross site collection transfers)
                    if (ex.InnerException is NotAvailableAtTargetException)
                    {
                        logger.LogError(SharePointTransformationResources.Error_NotValidForTargetSiteCollection);
                    }

                    if (ex.InnerException is MediaWebpartConfigurationException)
                    {
                        logger.LogError(SharePointTransformationResources.Error_MediaWebpartConfiguration);
                    }

                    logger.LogError($"{SharePointTransformationResources.Error_AnErrorOccurredFunctions} - {ex.Message}");
                    throw;
                }

                Mapping webPartMapping = null;
                // Get the needed mapping:
                // - use the mapping returned by the selector
                // - if no selector then take the default mapping
                // - if no mapping found we'll fall back to the default web part mapping
                if (!string.IsNullOrEmpty(selectorResult))
                {
                    webPartMapping = webPartData.Mappings.Mapping.Where(p => p.Name.Equals(selectorResult, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }
                else
                {
                    // If there's only one mapping let's take that one, even if not specified as default
                    if (webPartData.Mappings.Mapping.Length == 1)
                    {
                        webPartMapping = webPartData.Mappings.Mapping[0];
                    }
                    else
                    {
                        webPartMapping = webPartData.Mappings.Mapping.FirstOrDefault(p => p.Default == true);
                    }
                }

                if (webPartMapping != null)
                {
                    mapping = webPartMapping;
                }
                else
                {
                    logger.LogWarning($"{SharePointTransformationResources.Warning_ContentWebPartMappingNotFound}");
                }

                // Process mapping specific functions (if any)
                if (!String.IsNullOrEmpty(mapping.Functions))
                {
                    try
                    {
                        logger.LogInformation(SharePointTransformationResources.Info_ProcessingMappingFunctions);
                        functionProcessor.ProcessMappingFunctions(ref webPartData, webPart, mapping);
                    }
                    catch (Exception ex)
                    {
                        // NotAvailableAtTargetException is used to "skip" a web part since it's not valid for the target site collection (only applies to cross site collection transfers)
                        if (ex.InnerException is NotAvailableAtTargetException)
                        {
                            logger.LogError(SharePointTransformationResources.Error_NotValidForTargetSiteCollection);
                        }

                        logger.LogError($"{SharePointTransformationResources.Error_AnErrorOccurredFunctions} - {ex.Message}");
                        throw;
                    }
                }
            }

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

            //// Get the order of the last inserted control in this column
            //int order = LastColumnOrder(webPart.Row - 1, webPart.Column - 1);
            //// Interate the controls for this mapping using their order
            //foreach (var map in combinedMappinglist.OrderBy(p => p.Order))
            //{
            //    order++;

            //    if (map.ClientSideText != null)
            //    {
            //        // Insert a Text control
            //        //PnP.Framework.Pages.ClientSideText text = new PnP.Framework.Pages.ClientSideText()
            //        //{
            //        //    Text = TokenParser.ReplaceTokens(map.ClientSideText.Text, webPart)
            //        //};

            //        var text = page.NewTextPart();
            //        text.Text = TokenParser.ReplaceTokens(map.ClientSideText.Text, webPart);

            //        page.AddControl(text, page.Sections[webPart.Row - 1].Columns[webPart.Column - 1], order);
            //        LogInfo(LogStrings.AddedClientSideTextWebPart, LogStrings.Heading_AddingWebPartsToPage);

            //    }
            //    else if (map.ClientSideWebPart != null)
            //    {
            //        // Insert a web part
            //        //ClientSideComponent baseControl = null;
            //        PnPCore.IPageComponent baseControl = null;

            //        if (map.ClientSideWebPart.Type == ClientSideWebPartType.Custom)
            //        {
            //            // Parse the control ID to support generic web part placement scenarios
            //            map.ClientSideWebPart.ControlId = TokenParser.ReplaceTokens(map.ClientSideWebPart.ControlId, webPart);
            //            // Check if this web part belongs to the list of "usable" web parts for this site
            //            baseControl = componentsToAdd.FirstOrDefault(p => p.Id.Equals($"{{{map.ClientSideWebPart.ControlId}}}", StringComparison.InvariantCultureIgnoreCase));
            //            LogInfo(LogStrings.UsingCustomModernWebPart, LogStrings.Heading_AddingWebPartsToPage);
            //        }
            //        else
            //        {
            //            string webPartName = "";
            //            switch (map.ClientSideWebPart.Type)
            //            {
            //                case ClientSideWebPartType.List:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.List);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.List);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Image:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Image);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Image);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.ContentRollup:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.ContentRollup);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.ContentRollup);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.BingMap:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.BingMap);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.BingMap);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.ContentEmbed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.ContentEmbed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.ContentEmbed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.DocumentEmbed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.DocumentEmbed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.DocumentEmbed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.ImageGallery:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.ImageGallery);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.ImageGallery);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.LinkPreview:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.LinkPreview);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.LinkPreview);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.NewsFeed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.NewsFeed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.NewsFeed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.NewsReel:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.NewsReel);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.NewsReel);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.News:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.News);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.News);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.PowerBIReportEmbed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.PowerBIReportEmbed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.PowerBIReportEmbed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.QuickChart:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.QuickChart);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.QuickChart);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.SiteActivity:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.SiteActivity);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.SiteActivity);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.VideoEmbed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.VideoEmbed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.VideoEmbed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.YammerEmbed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.YammerEmbed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.YammerEmbed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Events:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Events);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Events);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.GroupCalendar:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.GroupCalendar);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.GroupCalendar);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Hero:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Hero);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Hero);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.PageTitle:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.PageTitle);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.PageTitle);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.People:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.People);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.People);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.QuickLinks:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.QuickLinks);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.QuickLinks);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Divider:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Divider);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Divider);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.MicrosoftForms:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.MicrosoftForms);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.MicrosoftForms);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Spacer:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Spacer);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Spacer);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.ClientWebPart:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.ClientWebPart);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.ClientWebPart);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.PowerApps:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.PowerApps);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.PowerApps);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.CodeSnippet:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.CodeSnippet);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.CodeSnippet);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.PageFields:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.PageFields);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.PageFields);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Weather:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Weather);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Weather);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.YouTube:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.YouTube);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.YouTube);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.MyDocuments:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.MyDocuments);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.MyDocuments);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.YammerFullFeed:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.YammerFullFeed);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.YammerFullFeed);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.CountDown:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.CountDown);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.CountDown);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.ListProperties:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.ListProperties);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.ListProperties);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.MarkDown:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.MarkDown);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.MarkDown);
            //                        break;
            //                    }
            //                case ClientSideWebPartType.Planner:
            //                    {
            //                        //webPartName = ClientSidePage.ClientSideWebPartEnumToName(DefaultClientSideWebParts.Planner);
            //                        webPartName = page.DefaultWebPartToWebPartId(PnPCore.DefaultWebPart.Planner);
            //                        break;
            //                    }
            //                default:
            //                    {
            //                        break;
            //                    }
            //            }

            //            // SharePoint add-ins can be added on client side pages...all add-ins are added via the client web part, so we need additional logic to find the one we need
            //            if (map.ClientSideWebPart.Type == ClientSideWebPartType.ClientWebPart)
            //            {
            //                var addinComponents = componentsToAdd.Where(p => p.Name.Equals(webPartName, StringComparison.InvariantCultureIgnoreCase));
            //                foreach (var addin in addinComponents)
            //                {
            //                    // Find the right add-in web part via title matching...maybe not bullet proof but did find anything better for now
            //                    JObject wpJObject = JObject.Parse(addin.Manifest);

            //                    // As there can be multiple classic web parts (via provider hosted add ins or SharePoint hosted add ins) we're looping to find the first one with a matching title
            //                    foreach (var addinEntry in wpJObject["preconfiguredEntries"])
            //                    {
            //                        if (addinEntry["title"]["default"].Value<string>() == webPart.Title)
            //                        {
            //                            baseControl = addin;

            //                            var jsonProperties = addinEntry;

            //                            // Fill custom web part properties in this json. Custom properties are listed as child elements under clientWebPartProperties, 
            //                            // replace their "default" value with the value we got from the web part's properties
            //                            jsonProperties = PopulateAddInProperties(jsonProperties, webPart);

            //                            // Override the JSON data we read from the model as this is fully dynamic due to the nature of the add-in client part
            //                            map.ClientSideWebPart.JsonControlData = jsonProperties.ToString(Newtonsoft.Json.Formatting.None);

            //                            LogInfo($"{LogStrings.ContentUsingAddinWebPart} '{baseControl.Name}' ", LogStrings.Heading_AddingWebPartsToPage);
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                baseControl = componentsToAdd.FirstOrDefault(p => p.Name.Equals(webPartName, StringComparison.InvariantCultureIgnoreCase));
            //                LogInfo($"{LogStrings.ContentUsing} '{ map.ClientSideWebPart.Type.ToString() }' {LogStrings.ContentModernWebPart}", LogStrings.Heading_AddingWebPartsToPage);
            //            }
            //        }

            //        // If we found the web part as a possible candidate to use then add it
            //        if (baseControl != null)
            //        {
            //            var jsonDecoded = WebUtility.HtmlDecode(TokenParser.ReplaceTokens(map.ClientSideWebPart.JsonControlData, webPart));
            //            //PnP.Framework.Pages.ClientSideWebPart myWebPart = new PnP.Framework.Pages.ClientSideWebPart(baseControl)
            //            //{
            //            //    Order = map.Order,
            //            //    PropertiesJson = jsonDecoded
            //            //};

            //            var myWebPart = page.NewWebPart(baseControl);
            //            myWebPart.Order = map.Order;
            //            myWebPart.PropertiesJson = jsonDecoded;

            //            page.AddControl(myWebPart, page.Sections[webPart.Row - 1].Columns[webPart.Column - 1], order);
            //            LogInfo($"{LogStrings.ContentAdded} '{ myWebPart.Title }' {LogStrings.ContentClientToTargetPage}", LogStrings.Heading_AddingWebPartsToPage);
            //        }
            //        else
            //        {
            //            LogWarning(LogStrings.ContentWarnModernNotFound, LogStrings.Heading_AddingWebPartsToPage);
            //        }

            //    }
            //}

            // Transform the source Web Part into the the target Web Part
            result.WebPart = new PnP.Core.Transformation.Model.WebPartEntity();
            //{
            //    Title = webPartToRetrieve.WebPartDefinition.WebPart.Title,
            //    Type = webPartToRetrieve.WebPartType,
            //    Id = webPartToRetrieve.WebPartDefinition.Id,
            //    ServerControlId = webPartToRetrieve.Id,
            //    Row = webPartToRetrieve.Row,
            //    Column = webPartToRetrieve.Column,
            //    Order = webPartToRetrieve.Order,
            //    ZoneId = "",
            //    ZoneIndex = (uint)webPartToRetrieve.WebPartDefinition.WebPart.ZoneIndex,
            //    IsClosed = webPartToRetrieve.WebPartDefinition.WebPart.IsClosed,
            //    Hidden = webPartToRetrieve.WebPartDefinition.WebPart.Hidden,
            //    Properties = ExtractProperties(input, mapping);
            //};

            return result;
        }

        private WebPartMapping LoadMappingFile(string mappingFilePath = null)
        {
            // Prepare the result variable
            WebPartMapping result = null;

            if (string.IsNullOrEmpty(mappingFilePath))
            {
                mappingFilePath = "stream.webpartmapping.xml";
            }

            // Check if we already have the mapping file in the in-memory cache
            if (this.memoryCache.TryGetValue(mappingFilePath, out result))
            {
                return result;
            }

            // Create the xml mapping serializer
            XmlSerializer xmlMapping = new XmlSerializer(typeof(WebPartMapping));

            // If we don't have the mapping file as an input
            if (string.IsNullOrEmpty(mappingFilePath) ||
                !System.IO.File.Exists(mappingFilePath))
            {
                // We use the default one, without validation
                using (var mappingStream = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.webpartmapping.xml"))
                {
                    using (var reader = XmlReader.Create(mappingStream))
                    {
                        result = (WebPartMapping)xmlMapping.Deserialize(reader);
                    }
                }
            }
            else
            {
                using (Stream schema = this.GetType().Assembly.GetManifestResourceStream("PnP.Core.Transformation.SharePoint.MappingFiles.webpartmapping.xsd"))
                {
                    using (var mappingStream = new FileStream(mappingFilePath, FileMode.Open))
                    {
                        // Ensure the provided file complies with the current schema
                        ValidateSchema(schema, mappingStream);

                        using (var reader = XmlReader.Create(mappingStream))
                        {
                            result = (WebPartMapping)xmlMapping.Deserialize(reader);
                        }
                    }
                }
            }

            // Cache the mapping file into the in-memory cache
            this.memoryCache.Set(mappingFilePath, result);
 
            return result;
        }

        private void ValidateSchema(Stream schema, FileStream stream)
        {
            // Load the template into an XDocument
            XDocument xml = XDocument.Load(stream);

            using (var schemaReader = new XmlTextReader(schema))
            {
                // Prepare the XML Schema Set
                XmlSchemaSet schemas = new XmlSchemaSet();
                schema.Seek(0, SeekOrigin.Begin);
                schemas.Add(SharePointConstants.PageTransformationSchema, schemaReader);

                // Set stream back to start
                stream.Seek(0, SeekOrigin.Begin);

                xml.Validate(schemas, (o, e) =>
                {
                    var errorMessage = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        SharePointTransformationResources.Error_WebPartMappingSchemaValidation, e.Message);
                    this.logger.LogError(e.Exception, errorMessage);
                    throw new ApplicationException(errorMessage);
                });
            }
        }

        private static Dictionary<string, string> ExtractProperties(WebPartMappingProviderInput input, WebPartMapping mapping)
        {
            // Storage for properties to keep
            Dictionary<string, string> propertiesToKeep = new Dictionary<string, string>();

            // List of properties to retrieve
            List<Property> propertiesToRetrieve = mapping.BaseWebPart.Properties.ToList<Property>();

            // For older versions of SharePoint the type in the mapping would not match. Use the TypeShort Comparison. 
            var webPartProperties = mapping.WebParts.FirstOrDefault(p => p.Type.GetTypeShort().Equals(input.SourceComponentType.GetTypeShort(), StringComparison.InvariantCultureIgnoreCase));
            if (webPartProperties != null && webPartProperties.Properties != null)
            {
                foreach (var p in webPartProperties.Properties.ToList<Property>())
                {
                    if (!propertiesToRetrieve.Contains(p))
                    {
                        propertiesToRetrieve.Add(p);
                    }
                }
            }

            // If we don't have the raw content
            if (string.IsNullOrEmpty(input.SourceComponentRawContent))
            {
                if (input.SourceComponentType.GetTypeShort() == WebParts.Client.GetTypeShort())
                {
                    // Special case since we don't know upfront which properties are relevant here...so let's take them all
                    foreach (var p in input.SourceProperties)
                    {
                        if (!propertiesToKeep.ContainsKey(p.Key))
                        {
                            propertiesToKeep.Add(p.Key, p.Value != null ? p.Value.ToString() : string.Empty);
                        }
                    }
                }
                else
                {
                    // Special case where we did not have export rights for the web part XML, assume this is a V3 web part
                    foreach (var p in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(p.Name) &&
                            input.SourceProperties.ContainsKey(p.Name) &&
                            !propertiesToKeep.ContainsKey(p.Name))
                        {
                            propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                input.SourceProperties[p.Name].ToString() : string.Empty);
                        }
                    }
                }
            }
            else
            {
                var xml = XElement.Parse(input.SourceComponentRawContent);
                var xmlns = xml.XPathSelectElement("*").GetDefaultNamespace();
                if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v3",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    if (input.SourceComponentType.GetTypeShort() == WebParts.Client.GetTypeShort())
                    {
                        // Special case since we don't know upfront which properties are relevant here...so let's take them all
                        foreach (var p in input.SourceProperties)
                        {
                            if (!propertiesToKeep.ContainsKey(p.Key))
                            {
                                propertiesToKeep.Add(p.Key, p.Value != null ? p.Value.ToString() : string.Empty);
                            }
                        }
                    }
                    else
                    {
                        // the retrieved properties are sufficient
                        foreach (var p in propertiesToRetrieve)
                        {
                            if (!string.IsNullOrEmpty(p.Name) &&
                                input.SourceProperties.ContainsKey(p.Name) &&
                                !propertiesToKeep.ContainsKey(p.Name))
                            {
                                propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                    input.SourceProperties[p.Name].ToString() : string.Empty);
                            }
                        }
                    }
                }
                else if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v2",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var p in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(p.Name))
                        {
                            if (input.SourceProperties.ContainsKey(p.Name))
                            {
                                if (!propertiesToKeep.ContainsKey(p.Name))
                                {
                                    propertiesToKeep.Add(p.Name, input.SourceProperties[p.Name] != null ?
                                        input.SourceProperties[p.Name].ToString() : "");
                                }
                            }
                            else
                            {
                                // check XMl for property
                                var v2Element = xml.Descendants(xmlns + p.Name).FirstOrDefault();
                                if (v2Element != null)
                                {
                                    if (!propertiesToKeep.ContainsKey(p.Name))
                                    {
                                        propertiesToKeep.Add(p.Name, v2Element.Value);
                                    }
                                }

                                // Some properties do have their own namespace defined
                                if (input.SourceComponentType.GetTypeShort() == WebParts.SimpleForm.GetTypeShort() &&
                                    p.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Load using the http://schemas.microsoft.com/WebPart/v2/SimpleForm namespace
                                    XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/SimpleForm";
                                    v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                    if (v2Element != null)
                                    {
                                        if (!propertiesToKeep.ContainsKey(p.Name))
                                        {
                                            propertiesToKeep.Add(p.Name, v2Element.Value);
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.ContentEditor.GetTypeShort())
                                {
                                    if (p.Name.Equals("ContentLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/ContentEditor";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.Xml.GetTypeShort())
                                {
                                    if (p.Name.Equals("XMLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XML", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XSLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("XSL", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/Xml";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (input.SourceComponentType.GetTypeShort() == WebParts.SiteDocuments.GetTypeShort())
                                {
                                    if (p.Name.Equals("UserControlledNavigation", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("ShowMemberships", StringComparison.InvariantCultureIgnoreCase) ||
                                        p.Name.Equals("UserTabs", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "urn:schemas-microsoft-com:sharepoint:portal:sitedocumentswebpart";
                                        v2Element = xml.Descendants(xmlcontentns + p.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(p.Name))
                                            {
                                                propertiesToKeep.Add(p.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return propertiesToKeep;
        }

        private void UpdateWebPartDataProperties(WebPart webPartData, WebPartMapping mapping)
        {
            List<Property> tempList = new List<Property>();
            if (webPartData.Properties != null)
            {
                tempList.AddRange(webPartData.Properties);
            }

            // Add properties listed on the Base web part
            var baseProperties = mapping.BaseWebPart.Properties;
            foreach (var baseProperty in baseProperties)
            {
                // Only add the base property once as the webPartData.Properties collection is reused across web parts and pages
                if (!tempList.Any(p => p.Name.Equals(baseProperty.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Add parameter to model
                    tempList.Add(new Property()
                    {
                        Functions = baseProperty.Functions,
                        Name = baseProperty.Name,
                        Type = PropertyType.@string
                    });
                }
            }

            webPartData.Properties = tempList.ToArray();
        }
    }
}
