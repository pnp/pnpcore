using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PnP.Core.Modernization.Pages
{
    /// <summary>
    /// Analyzes a web part page
    /// </summary>
    public class WebPartPage: BasePage
    {

        #region construction
        /// <summary>
        /// Instantiates a web part page object
        /// </summary>
        /// <param name="page">ListItem holding the page to analyze</param>
        /// <param name="pageFile">File holding the page (for pages living outside of a library)</param>
        /// <param name="pageTransformation">Page transformation information</param>
        /// <param name="logObservers"></param>
        public WebPartPage(IListItem page, IFile pageFile, PageTransformation pageTransformation, IList<ILogObserver> logObservers = null) : base(page, pageFile, pageTransformation, logObservers)
        {
        }
        #endregion

        /// <summary>
        /// Analyses a webpart page
        /// </summary>
        /// <param name="includeTitleBarWebPart">Include the TitleBar web part</param>
        /// <returns>Information about the analyzed webpart page</returns>
        public virtual Tuple<PageLayout, List<WebPartEntity>> Analyze(bool includeTitleBarWebPart = false)
        {
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            //Load the page
            string webPartPageUrl = null;
            IFile webPartPage = null;

            if (this.page != null)
            {
                webPartPageUrl = page[Constants.FileRefField].ToString();
                webPartPage = cc.Web.GetFileByServerRelativeUrl(webPartPageUrl);
            }
            else
            {
                webPartPageUrl = this.pageFile.EnsureProperty(p => p.ServerRelativeUrl);
                webPartPage = this.pageFile;
            }

            // Load page properties
            IPropertyValues pageProperties = webPartPage.Properties;
            webPartPage.LoadBatch(f => f.Properties);

            // Load web parts on web part page
            // Note: Web parts placed outside of a web part zone using SPD are not picked up by the web part manager. There's no API that will return those,
            //       only possible option to add parsing of the raw page aspx file.
            var limitedWPManager = new ClassicWebPartReader(webPartPage);

            var webParts = limitedWPManager.Read();
            cc.Execute();

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
                foreach(var foundWebPart in webPartsToRetrieve)
                {
                    // Skip Microsoft.SharePoint.WebPartPages.TitleBarWebPart webpart in TitleBar zone
                    if (foundWebPart.WebPartDefinition.ZoneId.Equals("TitleBar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!includeTitleBarWebPart)
                        {
                            continue;
                        }
                    }

                    if (foundWebPart.WebPartDefinition.WebPart.ExportMode == ClassicWebPartExportMode.All)
                    {
                        foundWebPart.WebPartXml = limitedWPManager.Export(foundWebPart.WebPartDefinition.Id);
                    }
                }

                foreach (var foundWebPart in webPartsToRetrieve)
                {
                    // Skip Microsoft.SharePoint.WebPartPages.TitleBarWebPart webpart in TitleBar zone
                    if (foundWebPart.WebPartDefinition.ZoneId.Equals("TitleBar", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!includeTitleBarWebPart)
                        {
                            continue;
                        }
                    }

                    if (foundWebPart.WebPartDefinition.WebPart.ExportMode != ClassicWebPartExportMode.All)
                    {
                        // Use different approach to determine type as we can't export the web part XML without indroducing a change
                        foundWebPart.WebPartType = GetTypeFromProperties(foundWebPart.WebPartDefinition.WebPart.Properties);
                    }
                    else
                    {
                        foundWebPart.WebPartType = GetType(foundWebPart.WebPartXml);
                    }

                    LogInfo(string.Format(LogStrings.ContentTransformFoundSourceWebParts,
                       foundWebPart.WebPartDefinition.WebPart.Title, foundWebPart.WebPartType.GetTypeShort()), LogStrings.Heading_ContentTransform);

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
                        Properties = Properties(foundWebPart.WebPartDefinition.WebPart.Properties, foundWebPart.WebPartType, foundWebPart.WebPartXml ==null ? "" : foundWebPart.WebPartXml),
                    });
                }
            }
            else
            {
                LogInfo(LogStrings.AnalysingNoWebPartsFound, LogStrings.Heading_ArticlePageHandling);
            }

            AddTemplateContent(layout, webparts);
            return new Tuple<PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        internal void AddTemplateContent(PageLayout layout, List<WebPartEntity> webparts)
        {
            if (layout != PageLayout.WebPart_DocumentWorkspace && layout != PageLayout.WebPart_ProjectSite)
            {
                return;
            }

            var description = cc.Web.EnsureProperty(w => w.Description);
            if (!string.IsNullOrWhiteSpace(description))
            {
                // These templates render the web description before the Top zone.
                foreach (var webpart in webparts.Where(w => w.Row == 1 && w.Column == 1))
                {
                    webpart.Order++;
                }
                webparts.Insert(0, CreateWikiTextPart($"<p>{WebUtility.HtmlEncode(description)}</p>", 1, 1, 0));
            }
        }

        /// <summary>
        /// Translates the given zone value and page layout to a column number
        /// </summary>
        /// <param name="zoneId">Web part zone id</param>
        /// <param name="layout">Layout of the web part page</param>
        /// <returns>Column value</returns>
        internal int GetColumn(string zoneId, PageLayout layout)
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
                            zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) )
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
                case PageLayout.WebPart_DocumentWorkspace:
                case PageLayout.WebPart_ProjectSite:
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
                case PageLayout.WebPart_DocumentCenter:
                    {
                        return zoneId.Equals("Top_Right", StringComparison.InvariantCultureIgnoreCase) ||
                               zoneId.Equals("Right", StringComparison.InvariantCultureIgnoreCase) ? 2 : 1;
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
        internal int GetRow(string zoneId, PageLayout layout)
        {
            switch (layout)
            {
                case PageLayout.WebPart_DocumentWorkspace:
                case PageLayout.WebPart_ProjectSite:
                case PageLayout.WebPart_DocumentCenter:
                    {
                        return zoneId.Equals("Left", StringComparison.InvariantCultureIgnoreCase) ||
                               zoneId.Equals("Right", StringComparison.InvariantCultureIgnoreCase) ? 2 : 1;
                    }
                case PageLayout.WebPart_HeaderFooterThreeColumns:
                    {
                        if (zoneId.Equals("Header", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return 1;
                        }
                        else if (zoneId.Equals("LeftColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("MiddleColumn", StringComparison.InvariantCultureIgnoreCase) ||
                                 zoneId.Equals("RightColumn", StringComparison.InvariantCultureIgnoreCase) )
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
        /// Determines the used web part page layout
        /// </summary>
        /// <param name="pageProperties">Properties of the web part page file</param>
        /// <returns>Used layout</returns>
        internal PageLayout GetLayout(IPropertyValues pageProperties)
        {
            pageProperties.Values.TryGetValue("vti_setuppath", out var setupPath);
            return GetLayoutFromSetupPath(setupPath?.ToString());
        }

        internal static PageLayout GetLayoutFromSetupPath(string setupPath)
        {
            if (!string.IsNullOrEmpty(setupPath))
            {
                setupPath = "\\" + setupPath.Replace('/', '\\').TrimStart('\\');
                if (setupPath.EndsWith(@"\SiteTemplates\STS\defaultdws.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PageLayout.WebPart_DocumentWorkspace;
                }
                else if (setupPath.EndsWith(@"\SiteTemplates\ProjectSite\default.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PageLayout.WebPart_ProjectSite;
                }
                else if (setupPath.EndsWith(@"\Features\BDR\default.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PageLayout.WebPart_DocumentCenter;
                }
                else if (setupPath.EndsWith(@"\bpstd.aspx", StringComparison.InvariantCultureIgnoreCase))
                {
                    return PageLayout.WebPart_FullPageVertical;
                }
                else
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
                    else if (setupPath.EndsWith(@"\SiteTemplates\STS\default.aspx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return PageLayout.WebPart_2010_TwoColumnsLeft;
                    }
                }
            }

            return PageLayout.WebPart_Custom;
        }

    }
}
