using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using PnP.Core.Modernization.Entities;
using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PnP.Core.Modernization.Pages
{

    /// <summary>
    /// Analyzes a wiki page
    /// </summary>
    public class WikiPage: BasePage
    {
        #region construction
        /// <summary>
        /// Instantiates a wiki page object
        /// </summary>
        /// <param name="page">ListItem holding the page to analyze</param>
        /// <param name="pageTransformation">Page transformation information</param>
        /// <param name="logObservers"></param>
        public WikiPage(IListItem page, PageTransformation pageTransformation, IList<ILogObserver> logObservers = null)
            : base(page, null, pageTransformation, logObservers)
        {
        }
        #endregion

        /// <summary>
        /// Analyses a wiki page
        /// </summary>
        /// <returns>Information about the analyzed wiki page</returns>
        public Tuple<PageLayout,List<WebPartEntity>> Analyze(bool isBlogPage = false)
        {
            List<WebPartEntity> webparts = new List<WebPartEntity>();

            //Load the page
            var wikiPageUrl = page[Constants.FileRefField].ToString();
            IFile wikiPage = cc.Web.GetFileByServerRelativeUrl(wikiPageUrl);
            wikiPage.LoadBatch();
            cc.Execute();

            string pageContents = null;
                
            if (!isBlogPage)
            {
                // Load wiki content in HTML parser
                if (page.Values[Constants.WikiField] == null)
                {
                    throw new Exception("WikiField contents was set to null, this is an invalid and empty wiki page.");
                }

                pageContents = page.Values[Constants.WikiField].ToString();
            }
            else
            {
                // Load wiki content in HTML parser
                if (page.Values[Constants.BodyField] == null)
                {
                    throw new Exception("Body contents was set to null, this is an invalid and empty blog page.");
                }

                pageContents = page.Values[Constants.BodyField].ToString();
            }

            var htmlDoc = parser.ParseDocument(pageContents);
            var layout = GetLayout(htmlDoc);
            if (string.IsNullOrEmpty(pageContents))
            {
                layout = PageLayout.Wiki_OneColumn;
            }

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
                        AnalyzeWikiContentBlock(webparts, htmlDoc, webPartsToRetrieve, rowCount, colCount, 0, content);
                    }
                }
            }

            // Bulk load the needed web part information
            if (webPartsToRetrieve.Count > 0)
            {
                LoadWebPartsInWikiContentFromServer(webparts, wikiPage, webPartsToRetrieve);
            }
            else
            {
                LogInfo(LogStrings.AnalysingNoWebPartsFound, LogStrings.Heading_ArticlePageHandling);
            }

            // Somehow the wiki was not standard formatted, so lets wrap its contents in a text block
            if (webparts.Count == 0 && !String.IsNullOrEmpty(htmlDoc.Source.Text))
            {
                webparts.Add(CreateWikiTextPart(htmlDoc.Source.Text, 1, 1, 1));
            }

            return new Tuple<PageLayout, List<WebPartEntity>>(layout, webparts);
        }

        /// <summary>
        /// Check if this element is nested in another already processed element...this needs to be skipped to avoid content duplication and possible processing errors
        /// </summary>
        /// <param name="contentHost">element to check</param>
        /// <returns>true if embedded in a already processed element</returns>
        private bool IsNestedLayoutsZoneOuter(IElement contentHost)
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

        /// <summary>
        /// Analyzes the wiki page to determine which layout was used
        /// </summary>
        /// <param name="doc">html object</param>
        /// <returns>Layout of the wiki page</returns>
        private PageLayout GetLayout(IHtmlDocument doc)
        {
            string spanValue = "";
            var spanTags = doc.All.Where(p => p.LocalName == "span" && p.HasAttribute("id"));
            if (spanTags.Any())
            {
                foreach(var span in spanTags)
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

    }
}
