using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Utilities
{
    /// <summary>
    /// This class is used to rewrite the html coming out of the SummaryLinks web part
    /// </summary>
    public class SummaryLinksHtmlTransformator
    {
        private HtmlParser parser;
        private string webPartTitle;

        #region Construction
        /// <summary>
        /// Default constructor
        /// </summary>
        public SummaryLinksHtmlTransformator()
        {
            // Instantiate the AngleSharp Html parser
            parser = new HtmlParser(new HtmlParserOptions() { IsEmbedded = true });
        }
        #endregion

        #region Properties
        public string WebPartTitle
        {
            get
            {
                return this.webPartTitle;
            }
            set
            {
                this.webPartTitle = value;
            }
        }
        #endregion

        /// <summary>
        /// Transforms the passed summarylinks html to be usable by the client side text part
        /// </summary>
        /// <param name="text">Summarylinks html to be transformed</param>
        /// <param name="usePlaceHolder"></param>
        /// <returns>Html that can be used and edited via the client side text part</returns>
        public string Transform(string text, bool usePlaceHolder)
        {
            using (var document = this.parser.ParseDocument(text))
            {
                using (var newDocument = this.parser.ParseDocument(""))
                {
                    // Iterate over the divs
                    var divs = document.QuerySelectorAll("div").Where(p => p.GetAttribute("title") == "_link");
                    IElement header = null;
                    IElement list = null;
                    foreach (var div in divs)
                    {
                        var summaryLinkHeader = div.Children.Where(p => p.GetAttribute("title") == "_groupstyle").FirstOrDefault();
                        if (summaryLinkHeader != null)
                        {
                            // Header
                            header = newDocument.CreateElement("div");
                            var strong = newDocument.CreateElement("strong");
                            var title = div.Children.Where(p => p.GetAttribute("title") == "_title").FirstOrDefault();
                            if (title != null)
                            {
                                strong.TextContent = title.TextContent;
                            }
                            header.AppendChild(strong);
                            newDocument.DocumentElement.Children[1].AppendChild(header);

                            // reset list
                            list = null;
                        }
                        else
                        {
                            // Link
                            if (list == null)
                            {
                                list = newDocument.CreateElement("ul");

                                if (header == null)
                                {
                                    header = newDocument.CreateElement("div");
                                    newDocument.DocumentElement.Children[1].AppendChild(header);
                                }

                                header.AppendChild(list);
                            }


                            // Link
                            var title = div.Children.Where(p => p.GetAttribute("title") == "_title").FirstOrDefault();
                            var linkUrl = div.Children.Where(p => p.GetAttribute("title") == "_linkurl").FirstOrDefault();
                            var openInNewWindow = div.Children.Where(p => p.GetAttribute("title") == "_openinnewwindow").FirstOrDefault();

                            if (linkUrl != null && title != null)
                            {
                                // ListItem
                                var item = newDocument.CreateElement("li");

                                var link = newDocument.CreateElement("a");
                                var href = linkUrl.Children.Where(p => p.HasAttribute("href")).FirstOrDefault();
                                link.SetAttribute("href", href != null ? href.GetAttribute("href") : "");
                                link.TextContent = title.TextContent;

                                if (openInNewWindow != null)
                                {
                                    if (bool.TryParse(openInNewWindow.TextContent, out bool openInNewWindowValue))
                                    {
                                        if (openInNewWindowValue)
                                        {
                                            link.SetAttribute("target", "_blank");
                                            link.SetAttribute("data-interception", "off");
                                        }
                                    }
                                }

                                item.AppendChild(link);
                                list.AppendChild(item);
                            }
                        }
                    }

                    // Return the transformed html
                    if (newDocument.DocumentElement.Children.Length > 1)
                    {
                        string htmlContent = newDocument.DocumentElement.Children[1].InnerHtml;

                        // Add the web part title is that's required
                        if (!string.IsNullOrEmpty(this.webPartTitle))
                        {
                            htmlContent = $"<H3>{this.webPartTitle}</H3>{htmlContent}";
                        }

                        return htmlContent;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.webPartTitle))
                        {
                            text = $"<H3>{this.webPartTitle}</H3><div>{text}</div>";
                        }

                        return text;
                    }
                }
            }
        }


        /// <summary>
        /// Transforms the passed summarylinks html a list of links
        /// </summary>
        /// <param name="text">Summarylinks html to be transformed</param>
        /// <returns>List of links</returns>
        public List<SummaryLink> GetLinks(string text)
        {
            List<SummaryLink> links = new List<SummaryLink>();

            using (var document = this.parser.ParseDocument(text))
            {
                // Iterate over the divs
                var divs = document.QuerySelectorAll("div").Where(p => p.GetAttribute("title") == "_link");
                foreach (var div in divs)
                {
                    var summaryLinkHeader = div.Children.Where(p => p.GetAttribute("title") == "_groupstyle").FirstOrDefault();
                    if (summaryLinkHeader != null)
                    {
                        // no support for headers
                    }
                    else
                    {
                        // Link
                        var linkTitle = div.Children.Where(p => p.GetAttribute("title") == "_title").FirstOrDefault();
                        var linkDescription = div.Children.Where(p => p.GetAttribute("title") == "_description").FirstOrDefault();
                        var linkUrl = div.Children.Where(p => p.GetAttribute("title") == "_linkurl").FirstOrDefault();
                        var linkImageUrl = div.Children.Where(p => p.GetAttribute("title") == "_imageurl").FirstOrDefault();
                        var linkImageAltText = div.Children.Where(p => p.GetAttribute("title") == "_imageurlalttext").FirstOrDefault();
                        var linkToolTip = div.Children.Where(p => p.GetAttribute("title") == "_linktooltip").FirstOrDefault();
                        var linkStyle = div.Children.Where(p => p.GetAttribute("title") == "_style").FirstOrDefault();
                        var openInNewWindow = div.Children.Where(p => p.GetAttribute("title") == "_openinnewwindow").FirstOrDefault();

                        SummaryLink l = new SummaryLink()
                        {
                            Title = linkTitle?.TextContent,
                            Description = linkDescription?.TextContent,
                            ImageAltText = linkImageAltText?.TextContent,
                            ToolTip = linkToolTip?.TextContent,
                            Style = linkStyle?.TextContent
                        };

                        // Handle Url fields
                        if (linkUrl != null)
                        {
                            var href = linkUrl.Children.Where(p => p.HasAttribute("href")).FirstOrDefault();
                            if (href != null)
                            {
                                l.Url = href.GetAttribute("href");
                            }
                        }
                        if (linkImageUrl != null)
                        {
                            var href = linkImageUrl.Children.Where(p => p.HasAttribute("href")).FirstOrDefault();
                            if (href != null)
                            {
                                l.ImageUrl = href.GetAttribute("href");
                            }
                        }

                        // Handle bool fields
                        if (openInNewWindow != null)
                        {
                            if (bool.TryParse(openInNewWindow.TextContent, out bool openInNewWindowValue))
                            {
                                l.OpenInNewWindow = openInNewWindowValue;
                            }
                        }

                        // Add to collection
                        links.Add(l);
                    }
                }
            }

            return links;
        }
    }
}
