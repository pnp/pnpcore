using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.WebParts;
using PnP.Core.Modernization.Cache;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Extensions;
using PnP.Core.Modernization.Publishing;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Pages
{
    /// <summary>
    /// Analyzes a publishing page
    /// </summary>
    public class PublishingPageOnPremises : PublishingPage
    {

        #region Construction
        /// <summary>
        /// Instantiates a publishing page object
        /// </summary>
        /// <param name="page">ListItem holding the page to analyze</param>
        /// <param name="pageTransformation">Page transformation information</param>
        /// <param name="baseTransformationInformation">Page transformation information</param>
        /// <param name="logObservers"></param>
        public PublishingPageOnPremises(ListItem page, PageTransformation pageTransformation, BaseTransformationInformation baseTransformationInformation, IList<ILogObserver> logObservers = null) : base(page, pageTransformation, baseTransformationInformation, logObservers)
        {
           
        }

        /// <summary>
        /// Instantiates a publishing page object
        /// </summary>
        /// <param name="page">ListItem holding the page to analyze</param>
        /// <param name="pageTransformation">Page transformation information</param>
        /// <param name="publishingPageTransformation"></param>
        /// <param name="baseTransformationInformation">Page transformation information</param>
        /// <param name="targetContext"></param>
        /// <param name="logObservers"></param>
        public PublishingPageOnPremises(ListItem page, PageTransformation pageTransformation, PublishingPageTransformation publishingPageTransformation, BaseTransformationInformation baseTransformationInformation, ClientContext targetContext = null, IList<ILogObserver> logObservers = null) : base(page, pageTransformation, publishingPageTransformation,  baseTransformationInformation, targetContext, logObservers)
        {

        }
        #endregion

        /// <summary>
        /// Analyses a publishing page
        /// </summary>
        /// <returns>Information about the analyzed publishing page</returns>
        public override Tuple<PageLayout, List<WebPartEntity>> Analyze(Publishing.PageLayout publishingPageTransformationModel)
        {
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            //Load the page
            var publishingPageUrl = page[Constants.FileRefField].ToString();
            var publishingPage = cc.Web.GetFileByServerRelativeUrl(publishingPageUrl);

            // Load relevant model data for the used page layout in case not already provided - safetynet for calls from modernization scanner
            string usedPageLayout = System.IO.Path.GetFileNameWithoutExtension(page.PageLayoutFile());
            if (publishingPageTransformationModel == null)
            {
                publishingPageTransformationModel = new PageLayoutManager(this.RegisteredLogObservers).GetPageLayoutMappingModel(this.publishingPageTransformation, page);
            }

            // Still no layout...can't continue...
            if (publishingPageTransformationModel == null)
            {
                LogError(string.Format(LogStrings.Error_NoPageLayoutTransformationModel, usedPageLayout), LogStrings.Heading_PublishingPage);
                throw new Exception(string.Format(LogStrings.Error_NoPageLayoutTransformationModel, usedPageLayout));
            }

            // Map layout
            bool includeVerticalColumn = false;
            if (publishingPageTransformationModel.IncludeVerticalColumnSpecified)
            {
                includeVerticalColumn = publishingPageTransformationModel.IncludeVerticalColumn;
            }

            PageLayout layout = MapToLayout(publishingPageTransformationModel.PageLayoutTemplate, includeVerticalColumn);

            #region Process fields that become web parts 
            if (publishingPageTransformationModel.WebParts != null)
            {
                #region Publishing Html column processing
                // Converting to WikiTextPart is a special case as we'll need to process the html
                var wikiTextWebParts = publishingPageTransformationModel.WebParts.Where(p => p.TargetWebPart.Equals(WebParts.WikiText, StringComparison.InvariantCultureIgnoreCase));
                List<WebPartPlaceHolder> webPartsToRetrieve = new List<WebPartPlaceHolder>();
                foreach (var wikiTextPart in wikiTextWebParts)
                {
                    string pageContents = page.GetFieldValueAs<string>(wikiTextPart.Name);

                    if (wikiTextPart.Property.Length > 0)
                    {
                        foreach (var fieldWebPartProperty in wikiTextPart.Property)
                        {
                            if (fieldWebPartProperty.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(fieldWebPartProperty.Functions))
                            {
                                // execute function
                                var evaluatedField = this.functionProcessor.Process(fieldWebPartProperty.Functions, fieldWebPartProperty.Name, MapToFunctionProcessorFieldType(fieldWebPartProperty.Type));
                                if (!string.IsNullOrEmpty(evaluatedField.Item1))
                                {
                                    pageContents = evaluatedField.Item2;
                                }
                            }
                        }
                    }

                    if (pageContents != null && !string.IsNullOrEmpty(pageContents))
                    {
                        var htmlDoc = parser.ParseDocument(pageContents);

                        // Analyze the html block (which is a wiki block)
                        var content = htmlDoc.FirstElementChild.LastElementChild;
                        AnalyzeWikiContentBlock(webparts, htmlDoc, webPartsToRetrieve, wikiTextPart.Row, wikiTextPart.Column, GetNextOrder(wikiTextPart.Row, wikiTextPart.Column, wikiTextPart.Order, webparts), content);
                    }
                    else
                    {
                        LogWarning(LogStrings.Warning_CannotRetrieveFieldValue, LogStrings.Heading_PublishingPage);
                    }
                }

                // Bulk load the needed web part information
                if (webPartsToRetrieve.Count > 0)
                {
                    LoadWebPartsInWikiContentFromOnPremisesServer(webparts, publishingPage, webPartsToRetrieve);
                }
                #endregion

                #region Generic processing of the other 'webpart' fields
                var fieldWebParts = publishingPageTransformationModel.WebParts.Where(p => !p.TargetWebPart.Equals(WebParts.WikiText, StringComparison.InvariantCultureIgnoreCase));
                foreach (var fieldWebPart in fieldWebParts.OrderBy(p => p.Row).OrderBy(p => p.Column))
                {
                    // In publishing scenarios it's common to not have all fields defined in the page layout mapping filled. By default we'll not map empty fields as that will result in empty web parts
                    // which impact the page look and feel. Using the RemoveEmptySectionsAndColumns flag this behaviour can be turned off.
                    if (this.baseTransformationInformation.RemoveEmptySectionsAndColumns)
                    {
                        var fieldContents = page.GetFieldValueAs<string>(fieldWebPart.Name);

                        if (string.IsNullOrEmpty(fieldContents))
                        {
                            LogWarning(String.Format(LogStrings.Warning_SkippedWebPartDueToEmptyInSourcee, fieldWebPart.TargetWebPart, fieldWebPart.Name), LogStrings.Heading_PublishingPage);
                            continue;
                        }
                    }

                    Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                    foreach (var fieldWebPartProperty in fieldWebPart.Property)
                    {
                        if (!string.IsNullOrEmpty(fieldWebPartProperty.Functions))
                        {
                            // execute function
                            var evaluatedField = this.functionProcessor.Process(fieldWebPartProperty.Functions, fieldWebPartProperty.Name, MapToFunctionProcessorFieldType(fieldWebPartProperty.Type));
                            if (!string.IsNullOrEmpty(evaluatedField.Item1) && !properties.ContainsKey(evaluatedField.Item1))
                            {
                                properties.Add(evaluatedField.Item1, evaluatedField.Item2);
                            }
                        }
                        else
                        {
                            var webPartName = page.FieldValues[fieldWebPart.Name]?.ToString().Trim();
                            if (webPartName != null)
                            {
                                properties.Add(fieldWebPartProperty.Name, page.FieldValues[fieldWebPart.Name].ToString().Trim());
                            }
                        }
                    }

                    var wpEntity = new WebPartEntity()
                    {
                        Title = fieldWebPart.Name,
                        Type = fieldWebPart.TargetWebPart,
                        Id = Guid.Empty,
                        Row = fieldWebPart.Row,
                        Column = fieldWebPart.Column,
                        Order = GetNextOrder(fieldWebPart.Row, fieldWebPart.Column, fieldWebPart.Order, webparts),
                        Properties = properties,
                    };

                    webparts.Add(wpEntity);
                }
            }
            #endregion
            #endregion

            #region Process fields that become metadata as they might result in the creation of page properties web part
            if (publishingPageTransformationModel.MetaData != null && publishingPageTransformationModel.MetaData.ShowPageProperties)
            {
                List<string> pagePropertiesFields = new List<string>();

                var fieldsToProcess = publishingPageTransformationModel.MetaData.Field.Where(p => p.ShowInPageProperties == true && !string.IsNullOrEmpty(p.TargetFieldName));

                if (fieldsToProcess.Any())
                {
                    // Loop over the fields that are defined to be shown in the page properties and that have a target field name set
                    foreach (var fieldToProcess in fieldsToProcess)
                    {
                        var targetFieldInstance = targetContext.Web.GetFieldByInternalName(fieldToProcess.TargetFieldName, true) ??
                            targetContext.Web.GetListByUrl("SitePages").Fields.GetFieldByInternalName(fieldToProcess.TargetFieldName);

                        if (targetFieldInstance != null)
                        {
                            if (!pagePropertiesFields.Contains(targetFieldInstance.Id.ToString()))
                            {
                                pagePropertiesFields.Add(targetFieldInstance.Id.ToString());
                            }
                        }
                    }

                    if (pagePropertiesFields.Count > 0)
                    {
                        string propertyString = "";
                        foreach (var propertyField in pagePropertiesFields)
                        {
                            if (!string.IsNullOrEmpty(propertyField))
                            {
                                propertyString = $"{propertyString},\"{propertyField.ToString()}\"";
                            }
                        }

                        if (!string.IsNullOrEmpty(propertyString))
                        {
                            propertyString = propertyString.TrimStart(new char[] { ',' });
                        }

                        if (!string.IsNullOrEmpty(propertyString))
                        {
                            Dictionary<string, string> properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "SelectedFields", propertyString }
                        };

                            var wpEntity = new WebPartEntity()
                            {
                                Type = WebParts.PageProperties,
                                Id = Guid.Empty,
                                Row = publishingPageTransformationModel.MetaData.PagePropertiesRow,
                                Column = publishingPageTransformationModel.MetaData.PagePropertiesColumn,
                                Order = GetNextOrder(publishingPageTransformationModel.MetaData.PagePropertiesRow, publishingPageTransformationModel.MetaData.PagePropertiesColumn, publishingPageTransformationModel.MetaData.PagePropertiesOrder, webparts),
                                Properties = properties,
                            };

                            webparts.Add(wpEntity);
                        }
                    }
                }
            }
            #endregion

            #region Web Parts in webpart zone handling
            // Load web parts put in web part zones on the publishing page
            // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. 
            var limitedWPManager = publishingPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
            cc.Load(limitedWPManager);

            IEnumerable<WebPartDefinition> webPartsViaManager = null;
            List<WebServiceWebPartEntity> webServiceWebPartEntities = null;
            
            //Properties, ExportMode and ZoneId - not Supported in 2010, Web Services are used to compensate for the missing properties
            webPartsViaManager = cc.LoadQuery(limitedWPManager.WebParts.IncludeWithDefaultProperties(wp => wp.Id, wp => wp.WebPart.Title, wp => wp.WebPart.ZoneIndex, wp => wp.WebPart.IsClosed, wp => wp.WebPart.Hidden));
            cc.ExecuteQueryRetry();

            webServiceWebPartEntities = LoadPublishingPageFromWebServices(publishingPage.EnsureProperty(p=>p.ServerRelativeUrl));
           

            if (webPartsViaManager.Any())
            {
                List<WebPartPlaceHolder> webPartsToRetrieve = new List<WebPartPlaceHolder>();

                foreach (var foundWebPart in webPartsViaManager)
                {
                    // Remove the web parts which we've already picked up by analyzing the wiki content block
                    if (webparts.Where(p => p.Id.Equals(foundWebPart.Id)).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    webPartsToRetrieve.Add(new WebPartPlaceHolder()
                    {
                        WebPartDefinition = foundWebPart,
                        WebPartXml = null,
                        WebPartType = "",
                    });
                }
                               
                foreach (var foundWebPart in webPartsToRetrieve)
                {
                    // If the web service call includes the export mode value then set the export options
                    var wsWp = webServiceWebPartEntities.FirstOrDefault(o => o.Id == foundWebPart.WebPartDefinition.Id);
                    var wsExportMode = wsWp.Properties.FirstOrDefault(o => o.Key.Equals("exportmode", StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(wsExportMode.Value) && wsExportMode.Value.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var webPartXml = base.ExportWebPartXmlWorkaround(publishingPageUrl, foundWebPart.WebPartDefinition.Id.ToString());
                        foundWebPart.WebPartXmlOnPremises = webPartXml;
                    }
                }
               
                List<WebPartZoneLayoutMap> webPartZoneLayoutMap = new List<WebPartZoneLayoutMap>();
                foreach (var foundWebPart in webPartsToRetrieve.OrderBy(p=> p.WebPartDefinition.WebPart.ZoneIndex))
                {
                    bool isExportable = false;

                    Dictionary<string, object> webPartProperties = null;

                    // If the web service call includes the export mode value then set the export options
                    var wsWp = webServiceWebPartEntities.FirstOrDefault(o => o.Id == foundWebPart.WebPartDefinition.Id);
                    webPartProperties = wsWp.PropertiesAsStringObjectDictionary();
                                               
                    var wsExportMode = wsWp.Properties.FirstOrDefault(o => o.Key.Equals("exportmode", StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(wsExportMode.Value) && wsExportMode.Value.ToString().Equals("all", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isExportable = true;
                    }
                    
                    if (!isExportable)
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        foundWebPart.WebPartType = GetTypeFromProperties(webPartProperties, true);
                    }
                    else
                    {
                        foundWebPart.WebPartType = GetType(foundWebPart.WebPartXmlOnPremises);
                    }

                    string zoneId = string.Empty;
                    var wsZoneId = wsWp.Properties.FirstOrDefault(o => o.Key.Equals("zoneid", StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(wsZoneId.Value))
                    {
                        zoneId = wsZoneId.Value;
                    }
                    
                    int wpInZoneRow = 1;
                    int wpInZoneCol = 1;
                    int wpStartOrder = 0;
                    // Determine location based upon the location given to the web part zone in the mapping
                    if (publishingPageTransformationModel.WebPartZones != null)
                    {
                        var wpZoneFromTemplate = publishingPageTransformationModel.WebPartZones.Where(p => p.ZoneId.Equals(zoneId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (wpZoneFromTemplate != null)
                        {
                            // Was there a webpart zone layout specified? If so then use that information to correctly position the webparts on the target page
                            if (wpZoneFromTemplate.WebPartZoneLayout != null && wpZoneFromTemplate.WebPartZoneLayout.Length > 0)
                            {
                                // Did we already map a web part of this type?
                                var webPartZoneLayoutMapEntry = webPartZoneLayoutMap.Where(p => p.ZoneId.Equals(wpZoneFromTemplate.ZoneId, StringComparison.InvariantCultureIgnoreCase) &&
                                                                                                p.Type.Equals(foundWebPart.WebPartType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                                // What's the expected occurance for this web part in the mapping?
                                int webPartOccuranceInZoneLayout = 1;
                                if (webPartZoneLayoutMapEntry != null)
                                {
                                    webPartOccuranceInZoneLayout += webPartZoneLayoutMapEntry.Occurances;
                                }

                                // Get the webpart from the webpart zone layout mapping
                                int occuranceCounter = 0;
                                bool occuranceFound = false;
                                foreach(var wpInWebPartZoneLayout in wpZoneFromTemplate.WebPartZoneLayout.Where(p=>p.Type.Equals(foundWebPart.WebPartType, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    occuranceCounter++;

                                    if (occuranceCounter == webPartOccuranceInZoneLayout)
                                    {
                                        occuranceFound = true;
                                        wpInZoneRow = wpInWebPartZoneLayout.Row;
                                        wpInZoneCol = wpInWebPartZoneLayout.Column;
                                        wpStartOrder = wpInWebPartZoneLayout.Order;
                                        break;
                                    }
                                }

                                if (occuranceFound)
                                {
                                    // Update the WebPartZoneLayoutMap
                                    if (webPartZoneLayoutMapEntry != null)
                                    {
                                        webPartZoneLayoutMapEntry.Occurances = webPartOccuranceInZoneLayout;
                                    }
                                    else
                                    {
                                        webPartZoneLayoutMap.Add(new WebPartZoneLayoutMap() { ZoneId = wpZoneFromTemplate.ZoneId, Type = foundWebPart.WebPartType, Occurances = webPartOccuranceInZoneLayout });
                                    }
                                }
                                else
                                {
                                    // fall back to the defaults from the zone definition
                                    wpInZoneRow = wpZoneFromTemplate.Row;
                                    wpInZoneCol = wpZoneFromTemplate.Column;
                                    wpStartOrder = wpZoneFromTemplate.Order;
                                }
                            }
                            else
                            {
                                wpInZoneRow = wpZoneFromTemplate.Row;
                                wpInZoneCol = wpZoneFromTemplate.Column;
                                wpStartOrder = wpZoneFromTemplate.Order;
                            }
                        }
                    }

                    // Determine order already taken
                    int wpInZoneOrderUsed = GetNextOrder(wpInZoneRow, wpInZoneCol, wpStartOrder, webparts);

                    string webPartXmlForPropertiesMethod = null;
                    webPartXmlForPropertiesMethod = foundWebPart.WebPartXmlOnPremises;

                    LogInfo(string.Format(LogStrings.ContentTransformFoundSourceWebParts, 
                        foundWebPart.WebPartDefinition.WebPart.Title, foundWebPart.WebPartType.GetTypeShort()), LogStrings.Heading_ContentTransform);

                    webparts.Add(new WebPartEntity()
                    {
                        Title = foundWebPart.WebPartDefinition.WebPart.Title,
                        Type = foundWebPart.WebPartType,
                        Id = foundWebPart.WebPartDefinition.Id,
                        ServerControlId = foundWebPart.WebPartDefinition.Id.ToString(),
                        Row = wpInZoneRow,
                        Column = wpInZoneCol,
                        Order = wpInZoneOrderUsed + foundWebPart.WebPartDefinition.WebPart.ZoneIndex,
                        ZoneId = zoneId,
                        ZoneIndex = (uint)foundWebPart.WebPartDefinition.WebPart.ZoneIndex,
                        IsClosed = foundWebPart.WebPartDefinition.WebPart.IsClosed,
                        Hidden = foundWebPart.WebPartDefinition.WebPart.Hidden,
                        Properties = Properties(webPartProperties, foundWebPart.WebPartType, webPartXmlForPropertiesMethod),
                    });
                }
            }
            else
            {
                LogInfo(LogStrings.AnalysingNoWebPartsFound, LogStrings.Heading_ArticlePageHandling);
            }
            #endregion

            #region Fixed webparts mapping
            if (publishingPageTransformationModel.FixedWebParts != null)
            {
                foreach(var fixedWebpart in publishingPageTransformationModel.FixedWebParts)
                {
                    int wpFixedOrderUsed = GetNextOrder(fixedWebpart.Row, fixedWebpart.Column, fixedWebpart.Order, webparts);

                    webparts.Add(new WebPartEntity()
                    {
                        Title = GetFixedWebPartProperty<string>(fixedWebpart, "Title", ""),
                        Type = fixedWebpart.Type,
                        Id = Guid.NewGuid(),
                        Row = fixedWebpart.Row,
                        Column = fixedWebpart.Column,
                        Order = wpFixedOrderUsed,
                        ZoneId = "",
                        ZoneIndex = 0,
                        IsClosed = GetFixedWebPartProperty<bool>(fixedWebpart, "__designer:IsClosed", false),
                        Hidden = false,
                        Properties = CastAsPropertiesDictionary(fixedWebpart),
                    });

                }
            }
            #endregion

            return new Tuple<PageLayout, List<WebPartEntity>>(layout, webparts);
        }

  
    }
}