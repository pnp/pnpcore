using AngleSharp.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using PnP.Core.Modernization.Entities;

using PnP.Core.Modernization.Telemetry;
using PnP.Core.Modernization.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PnP.Core.Modernization.Pages
{

    /// <summary>
    /// Base class for the page analyzers
    /// </summary>
    public abstract class BasePage : BaseTransform
    {
        internal class WebPartPlaceHolder
        {
            public string Id { get; set; }
            public string ControlId { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public int Order { get; set; }
            public ClassicWebPartDefinition WebPartDefinition { get; set; }
            public string WebPartXml { get; set; }

            public string WebPartType { get; set; }
            public string RenderedHtml { get; set; }
        }

        internal HtmlParser parser;

        public IListItem page;
        public IFile pageFile;
        public PnPContext cc;
        public PageTransformation pageTransformation;

        #region construction
        /// <summary>
        /// Constructs the base page class instance
        /// </summary>
        /// <param name="page">page ListItem</param>
        /// <param name="pageFile">page File</param>
        /// <param name="pageTransformation">page transformation model to use for extraction or transformation</param>
        /// <param name="logObservers"></param>
        public BasePage(IListItem page, IFile pageFile, PageTransformation pageTransformation, IList<ILogObserver> logObservers = null)
        {
            // Register observers
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }

            this.page = page;
            this.pageFile = pageFile;

            if (page != null)
            {
                this.cc = (page.PnPContext as PnPContext);
            }
            else if (pageFile != null)
            {
                this.cc = (pageFile.PnPContext as PnPContext);
            }

            if (this.cc != null)
            {
                SharePointOnlineSupport.Validate(this.cc);
            }

            this.pageTransformation = pageTransformation;
            this.parser = new HtmlParser();
        }
        #endregion

        /// <summary>
        /// Gets the type of the web part
        /// </summary>
        /// <param name="webPartXml">Web part xml to analyze</param>
        /// <returns>Type of the web part as fully qualified name</returns>
        public string GetType(string webPartXml)
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

        internal void AnalyzeWikiContentBlock(List<WebPartEntity> webparts, IHtmlDocument htmlDoc, List<WebPartPlaceHolder> webPartsToRetrieve, int rowCount, int colCount, int startOrder, IElement content)
        {
            // Drop elements which we anyhow can't transform and/or which are stripped out from RTE
            CleanHtml(content, htmlDoc);

            StringBuilder textContent = new StringBuilder();
            int order = startOrder;
            foreach (var node in content.ChildNodes.SelectMany(SplitWikiContentNode))
            {
                if (IsWikiWebPartBox(node))
                {
                    // first insert text part (if it was available)
                    if (!string.IsNullOrEmpty(textContent.ToString()))
                    {
                        order++;
                        webparts.Add(CreateWikiTextPart(textContent.ToString(), rowCount, colCount, order));
                        textContent.Clear();
                    }

                    // then process the web part
                    order++;
                    var box = (IElement)node;
                    var control = box.QuerySelectorAll("div[id]").Prepend(box).FirstOrDefault(element =>
                        element.Id != null && element.Id.StartsWith("div_", StringComparison.OrdinalIgnoreCase) &&
                        Guid.TryParse(element.Id.Substring(4), out _));
                    var controlId = control?.Id.Substring(4);
                    var placeholder = new WebPartPlaceHolder
                    {
                        Id = controlId,
                        ControlId = controlId == null ? null : $"g_{controlId.Replace("-", "_")}",
                        Row = rowCount,
                        Column = colCount,
                        Order = order,
                        RenderedHtml = box.InnerHtml
                    };
                    if (controlId != null)
                    {
                        webPartsToRetrieve.Add(placeholder);
                    }
                    else
                    {
                        PreserveWikiWebPartHtml(webparts, placeholder);
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

        /// <summary>
        /// Load Web Parts from Wiki Content page on Online Server
        /// </summary>
        /// <param name="webparts"></param>
        /// <param name="wikiPage"></param>
        /// <param name="webPartsToRetrieve"></param>
        internal void LoadWebPartsInWikiContentFromServer(List<WebPartEntity> webparts, IFile wikiPage, List<WebPartPlaceHolder> webPartsToRetrieve)
        {
            var reader = new ClassicWebPartReader(wikiPage);
            var definitions = reader.Read().ToDictionary(part => part.Id);
            foreach (var placeholder in webPartsToRetrieve)
            {
                if (!Guid.TryParse(placeholder.Id, out var id))
                {
                    continue;
                }
                if (!definitions.TryGetValue(id, out var definition))
                {
                    PreserveWikiWebPartHtml(webparts, placeholder);
                    continue;
                }
                string xml = definition.WebPart.ExportMode == ClassicWebPartExportMode.All ? reader.Export(id) : null;
                string type = xml == null ? GetTypeFromProperties(definition.WebPart.Properties) : GetType(xml);
                LogInfo(string.Format(LogStrings.ContentTransformFoundSourceWebParts,
                    definition.WebPart.Title, type.GetTypeShort()), LogStrings.Heading_ContentTransform);
                webparts.Add(new WebPartEntity
                {
                    Title = definition.WebPart.Title,
                    Type = type,
                    Id = definition.Id,
                    ServerControlId = placeholder.Id,
                    Row = placeholder.Row,
                    Column = placeholder.Column,
                    Order = placeholder.Order,
                    ZoneId = "",
                    ZoneIndex = (uint)definition.WebPart.ZoneIndex,
                    IsClosed = definition.WebPart.IsClosed,
                    Hidden = definition.WebPart.Hidden,
                    Properties = Properties(definition.WebPart.Properties, type, xml ?? "")
                });
            }
        }
        private void PreserveWikiWebPartHtml(List<WebPartEntity> webparts, WebPartPlaceHolder placeholder)
        {
            var hasHtml = !string.IsNullOrWhiteSpace(placeholder.RenderedHtml);
            LogWarning(string.Format(hasHtml ? LogStrings.Warning_WikiWebPartHtmlPreserved : LogStrings.Warning_WikiWebPartHtmlMissing,
                placeholder.Id ?? "(missing or invalid control ID)", placeholder.Row, placeholder.Column),
                LogStrings.Heading_ContentTransform);
            if (hasHtml)
            {
                webparts.Add(CreateWikiTextPart(placeholder.RenderedHtml, placeholder.Row, placeholder.Column, placeholder.Order));
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
        internal WebPartEntity CreateWikiTextPart(string wikiTextPartContent, int row, int col, int order)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("Text", wikiTextPartContent.Trim().Replace("\r\n", string.Empty));

            return new WebPartEntity()
            {
                Title = "WikiText",
                Type = "SharePointPnP.Modernization.WikiTextPart",
                Id = Guid.Empty,
                Row = row,
                Column = col,
                Order = order,
                Properties = properties,
            };
        }

        private void CleanHtml(IElement element, IHtmlDocument document)
        {
            foreach (var node in element.QuerySelectorAll("*").ToList())
            {
                if (node.ParentElement != null && IsUntransformableBlockElement(node))
                {
                    // create new div node and add all current children to it
                    var div = document.CreateElement("div");
                    if (node.HasAttribute("style"))
                    {
                        // Keep decoration until HtmlTransformator can materialize it on the text.
                        var decoration = new CssParser().ParseDeclaration(node.GetAttribute("style"))
                            .GetPropertyValue("text-decoration-line");
                        if (!string.IsNullOrEmpty(decoration))
                        {
                            div.SetAttribute("style", $"text-decoration-line:{decoration}");
                        }
                    }
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


        private static bool IsWikiWebPartBox(INode node)
        {
            return node is IElement element && element.LocalName == "div" && element.ClassList.Contains("ms-rte-wpbox");
        }

        private static IEnumerable<INode> SplitWikiContentNode(INode node)
        {
            if (!(node is IElement element) || IsWikiWebPartBox(node) || element.QuerySelector("div.ms-rte-wpbox") == null)
            {
                yield return node;
                yield break;
            }

            // Keep ancestor formatting around each text fragment, and reserve a distinct position for every box.
            var fragment = element.Clone(false);
            foreach (var child in element.ChildNodes.SelectMany(SplitWikiContentNode))
            {
                if (IsWikiWebPartBox(child))
                {
                    if (fragment.HasChildNodes)
                    {
                        yield return fragment;
                    }
                    yield return child;
                    fragment = element.Clone(false);
                }
                else
                {
                    fragment.AppendChild(child.Clone(true));
                }
            }
            if (fragment.HasChildNodes)
            {
                yield return fragment;
            }
        }


        /// <summary>
        /// Gets the type of the web part by detecting it from the available properties
        /// </summary>
        /// <param name="properties">Web part properties to analyze</param>
        /// <param name="isLegacy">If true tries additional webpart types used in legacy versions</param>
        /// <returns>Type of the web part as fully qualified name</returns>
        public string GetTypeFromProperties(Dictionary<string, object> properties, bool isLegacy = false)
        {
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

            // check for Media web part
            string[] mediaWebPart = new string[] { "AutoPlay", "MediaSource", "Loop", "IsPreviewImageSourceOverridenForVideoSet", "PreviewImageSource" };
            if (CheckWebPartProperties(mediaWebPart, properties))
            {
                return WebParts.Media;
            }

            // check for SlideShow web part
            string[] slideShowWebPart = new string[] { "LibraryGuid", "Layout", "Speed", "ShowToolbar", "ViewGuid" };
            if (CheckWebPartProperties(slideShowWebPart, properties))
            {
                return WebParts.PictureLibrarySlideshow;
            }

            // check for Chart web part
            string[] chartWebPart = new string[] { "ConnectionPointEnabled", "ChartXml", "DataBindingsString", "DesignerChartTheme" };
            if (CheckWebPartProperties(chartWebPart, properties))
            {
                return WebParts.Chart;
            }

            // check for Site Members web part
            string[] membersWebPart = new string[] { "NumberLimit", "DisplayType", "MembershipGroupId", "Toolbar" };
            if (CheckWebPartProperties(membersWebPart, properties))
            {
                return WebParts.Members;
            }

            // check for Silverlight web part
            string[] silverlightWebPart = new string[] { "MinRuntimeVersion", "WindowlessMode", "CustomInitParameters", "Url", "ApplicationXml" };
            if (CheckWebPartProperties(silverlightWebPart, properties))
            {
                return WebParts.Silverlight;
            }

            // check for Add-in Part web part
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
                if(properties.ContainsKey("TypeName") && properties["TypeName"].ToString() == "Microsoft.SharePoint.WebPartPages.TitleBarWebPart")
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
                    foreach(var iconPath in iconsToCheck)
                    {
                        if (iconToRepresent.ToString().ContainsIgnoringCasing(iconPath))
                        {
                            return WebParts.XsltListView;
                        }
                    }
                }
            }

            // check for Script Editor web part
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

            LogWarning(LogStrings.Warning_NotSupportedWebPart, LogStrings.Heading_ContentTransform);
            return "Unsupported Web Part Type";
        }

        private static bool CheckWebPartProperties(string[] propertiesToCheck, Dictionary<string, object> properties)
        {
            bool isWebPart = true;
            foreach (var wpProp in propertiesToCheck)
            {
                if (!properties.ContainsKey(wpProp))
                {
                    isWebPart = false;
                    break;
                }
            }

            return isWebPart;
        }

        /// <summary>
        /// Checks the PageTransformation XML data to know which properties need to be kept for the given web part and collects their values
        /// </summary>
        /// <param name="properties">Properties collection retrieved when we loaded the web part</param>
        /// <param name="webPartType">Type of the web part</param>
        /// <param name="webPartXml">Web part XML</param>
        /// <returns>Collection of the requested property/value pairs</returns>
        public Dictionary<string, string> Properties(Dictionary<string, object> properties, string webPartType, string webPartXml)
        {
            Dictionary<string, string> propertiesToKeep = new Dictionary<string, string>();

            List<Property> propertiesToRetrieve = this.pageTransformation.BaseWebPart.Properties.ToList<Property>();

            //For older versions of SharePoint the type in the mapping would not match. Use the TypeShort Comparison. 
            var webPartProperties = this.pageTransformation.WebParts.Where(p => p.Type.GetTypeShort().Equals(webPartType.GetTypeShort(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
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

            if (string.IsNullOrEmpty(webPartXml))
            {
                if (webPartType.GetTypeShort() == WebParts.Client.GetTypeShort())
                {
                    // Special case since we don't know upfront which properties are relevant here...so let's take them all
                    foreach (var prop in properties)
                    {
                        if (!propertiesToKeep.ContainsKey(prop.Key))
                        {
                            propertiesToKeep.Add(prop.Key, prop.Value != null ? prop.Value.ToString() : "");
                        }
                    }
                }
                else
                {
                    // Special case where we did not have export rights for the web part XML, assume this is a V3 web part
                    foreach (var property in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(property.Name) && properties.ContainsKey(property.Name))
                        {
                            if (!propertiesToKeep.ContainsKey(property.Name))
                            {
                                propertiesToKeep.Add(property.Name, properties[property.Name] != null ? properties[property.Name].ToString() : "");
                            }
                        }
                    }
                }
            }
            else
            {
                var xml = XElement.Parse(webPartXml);
                var xmlns = xml.XPathSelectElement("*").GetDefaultNamespace();
                if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v3", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (webPartType.GetTypeShort() == WebParts.Client.GetTypeShort())
                    {
                        // Special case since we don't know upfront which properties are relevant here...so let's take them all
                        foreach (var prop in properties)
                        {
                            if (!propertiesToKeep.ContainsKey(prop.Key))
                            {
                                propertiesToKeep.Add(prop.Key, prop.Value != null ? prop.Value.ToString() : "");
                            }
                        }
                    }
                    else
                    {
                        // the retrieved properties are sufficient
                        foreach (var property in propertiesToRetrieve)
                        {
                            if (!string.IsNullOrEmpty(property.Name) && properties.ContainsKey(property.Name))
                            {
                                if (!propertiesToKeep.ContainsKey(property.Name))
                                {
                                    propertiesToKeep.Add(property.Name, properties[property.Name] != null ? properties[property.Name].ToString() : "");
                                }
                            }
                        }
                    }
                }
                else if (xmlns.NamespaceName.Equals("http://schemas.microsoft.com/WebPart/v2", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var property in propertiesToRetrieve)
                    {
                        if (!string.IsNullOrEmpty(property.Name))
                        {
                            if (properties.ContainsKey(property.Name))
                            {
                                if (!propertiesToKeep.ContainsKey(property.Name))
                                {
                                    propertiesToKeep.Add(property.Name, properties[property.Name] != null ? properties[property.Name].ToString() : "");
                                }
                            }
                            else
                            {
                                // check XMl for property
                                var v2Element = xml.Descendants(xmlns + property.Name).FirstOrDefault();
                                if (v2Element != null)
                                {
                                    if (!propertiesToKeep.ContainsKey(property.Name))
                                    {
                                        propertiesToKeep.Add(property.Name, v2Element.Value);
                                    }
                                }

                                // Some properties do have their own namespace defined
                                if (webPartType.GetTypeShort() == WebParts.SimpleForm.GetTypeShort() && property.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Load using the http://schemas.microsoft.com/WebPart/v2/SimpleForm namespace
                                    XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/SimpleForm";
                                    v2Element = xml.Descendants(xmlcontentns + property.Name).FirstOrDefault();
                                    if (v2Element != null)
                                    {
                                        if (!propertiesToKeep.ContainsKey(property.Name))
                                        {
                                            propertiesToKeep.Add(property.Name, v2Element.Value);
                                        }
                                    }
                                }
                                else if (webPartType.GetTypeShort() == WebParts.ContentEditor.GetTypeShort())
                                {
                                    if (property.Name.Equals("ContentLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("Content", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/ContentEditor";
                                        v2Element = xml.Descendants(xmlcontentns + property.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(property.Name))
                                            {
                                                propertiesToKeep.Add(property.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (webPartType.GetTypeShort() == WebParts.Xml.GetTypeShort())
                                {
                                    if (property.Name.Equals("XMLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("XML", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("XSLLink", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("XSL", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("PartStorage", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "http://schemas.microsoft.com/WebPart/v2/Xml";
                                        v2Element = xml.Descendants(xmlcontentns + property.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(property.Name))
                                            {
                                                propertiesToKeep.Add(property.Name, v2Element.Value);
                                            }
                                        }
                                    }
                                }
                                else if (webPartType.GetTypeShort() == WebParts.SiteDocuments.GetTypeShort())
                                {
                                    if (property.Name.Equals("UserControlledNavigation", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("ShowMemberships", StringComparison.InvariantCultureIgnoreCase) ||
                                        property.Name.Equals("UserTabs", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        XNamespace xmlcontentns = "urn:schemas-microsoft-com:sharepoint:portal:sitedocumentswebpart";
                                        v2Element = xml.Descendants(xmlcontentns + property.Name).FirstOrDefault();
                                        if (v2Element != null)
                                        {
                                            if (!propertiesToKeep.ContainsKey(property.Name))
                                            {
                                                propertiesToKeep.Add(property.Name, v2Element.Value);
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

        /// <summary>
        /// Gets the tag prefixes from the document
        /// </summary>
        /// <param name="webPartPage"></param>
        /// <returns></returns>
        internal List<Tuple<string, string>> ExtractWebPartPrefixesFromNamespaces(IHtmlDocument webPartPage)
        {
            var tagPrefixes = new List<Tuple<string, string>>();

            Regex regex = new Regex("&lt;%@(.*?)%&gt;", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var aspxHeader = webPartPage.All.Where(o => o.TagName == "HTML").FirstOrDefault();
            var results = regex.Matches(aspxHeader?.InnerHtml);

            StringBuilder blockHtml = new StringBuilder();
            foreach (var match in results)
            {
                var matchString = match.ToString().Replace("&lt;%@ ", "<").Replace("%&gt;", " />");
                blockHtml.AppendLine(matchString);
            }

            var fullBlock = blockHtml.ToString();
            using (var subDocument = this.parser.ParseDocument(fullBlock))
            {
                var registers = subDocument.All.Where(o => o.TagName == "REGISTER");

                foreach (var register in registers)
                {
                    var prefix = register.GetAttribute("Tagprefix");
                    var nameSpace = register.GetAttribute("Namespace");
                    var className = nameSpace.InferClassNameFromNameSpace();
                    tagPrefixes.Add(new Tuple<string, string>(prefix, nameSpace));
                    tagPrefixes.Add(new Tuple<string, string>(className, nameSpace));
                }

            }

            return tagPrefixes;
        }

    }
}
