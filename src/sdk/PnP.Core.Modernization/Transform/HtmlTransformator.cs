using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using PnP.Core.Modernization.Telemetry;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using AngleSharp.Io;

namespace PnP.Core.Modernization.Transform
{
    /// <summary>
    /// Transforms the received Html in html that can be displayed and maintained in the modern client side text part
    /// </summary>
    public class HtmlTransformator : BaseTransform, IHtmlTransformator
    {
        #region Internal table classes
        internal class Table
        {
            internal string ClassName { get; set; }
            internal List<List<TableCell>> Rows { get; } = new List<List<TableCell>>();
            internal int Columns { get; set; }
        }

        internal class TableCell
        {
            internal IElement Element { get; set; }
            internal int Column { get; set; }
            internal int ColumnSpan { get; set; }
            internal int RowSpan { get; set; }
        }
        #endregion

        private HtmlParser parser;
        private readonly Func<string, string> themeColorResolver;

        #region Construction
        /// <summary>
        /// Default constructor
        /// </summary>
        public HtmlTransformator(IList<ILogObserver> logObservers = null): this(logObservers, null)
        {
        }

        internal HtmlTransformator(IList<ILogObserver> logObservers, Func<string, string> themeColorResolver): base()
        {
            this.themeColorResolver = themeColorResolver;
            if (logObservers != null)
            {
                foreach (var observer in logObservers)
                {
                    base.RegisterObserver(observer);
                }
            }
            // Instantiate the AngleSharp Html parser, configure to load the Style property as well
            var config = Configuration.Default.WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = true }).WithCss();
            var context = BrowsingContext.New(config);
            parser = new HtmlParser(new HtmlParserOptions { IsEmbedded = true }, context);
            
            
        }
        #endregion

        /// <summary>
        /// Transforms the passed html to be usable by the client side text part
        /// </summary>
        /// <param name="text">Html to be transformed</param>
        /// <param name="usePlaceHolder">Insert placeholders for images and iframe tags</param>
        /// <returns>Html that can be used and edited via the client side text part</returns>
        /// <remarks>
        /// Source fragment destinations are retained, with heading IDs placed on content spans because modern rendering
        /// generates its own heading IDs. Those generated IDs can collide with source IDs. The modern text editor can
        /// discard custom IDs on a subsequent edit/save; preserving them here does not prevent that editor behavior.
        /// Ordered lists retain their inline or HTML type marker style (decimal by default) at every nesting level,
        /// using native RTE list markup instead of the modern renderer's implicit alphabetic nested markers.
        /// </remarks>
        public string Transform(string text, bool usePlaceHolder)
        {
            // Strip out the "zero width space characters"
            text = text.Replace("\u200B", string.Empty);
            text = text.Replace("\u200b", string.Empty);

            using (var document = this.parser.ParseDocument(text))
            {
                PreserveStrikethrough(document.Body, document);

                // Process headings: RTE does h2, h3, h4 while wiki does h1, h2, h3, h4. Wiki h4 to h6 will become (formatted) text
                TransformHeadings(document, 6, 0);
                TransformHeadings(document, 5, 0);
                TransformHeadings(document, 4, 0);
                TransformHeadings(document, 3, 4);
                TransformHeadings(document, 2, 3);
                TransformHeadings(document, 1, 2);

                // Process blockquotes
                TransformBlockQuotes(document.QuerySelectorAll("blockquote"), document);

                // Process elements that can hold forecolor, backcolor, fontsize, underline and strikethrough information
                TransformElements(document.QuerySelectorAll("font"), document);
                TransformElements(document.QuerySelectorAll("span"), document);
                TransformElements(document.QuerySelectorAll("sup"), document);
                TransformElements(document.QuerySelectorAll("sub"), document);
                TransformElements(document.QuerySelectorAll("strong"), document);
                TransformElements(document.QuerySelectorAll("em"), document);
                TransformElements(document.QuerySelectorAll("p"), document);


                // Process image and iframes ==> put a place holder text as these will be dropped by RTE on edit mode
                if (usePlaceHolder)
                {
                    ImageIFramePlaceHolders(document);
                }

                // Process tables
                TransformTables(document.QuerySelectorAll("table"), document);

                TransformLists(document);

                // Finalize the transformation by cleaning the html
                // - styling information: RTE does only support a limited set of styles
                // - html nodes that are not supported in RTE (clean/replace nodes but avoid dropping relevant content)
                CleanStyles(document);
                CleanHtmlNodes(document);

                // Return the transformed html
                if (document.DocumentElement.Children.Length > 1)
                {
                    string updatedText = document.DocumentElement.Children[1].InnerHtml;
                    return updatedText;
                }
                else
                {
                    return text;
                }
            }
        }

        /// <summary>
        /// Returns true is the passed html is "empty"
        /// </summary>
        /// <param name="text">Html to verify</param>
        /// <returns>True if considered empty, false otherwise</returns>
        public bool IsEmptyParagraph(string text)
        {
            // Remove the "Zero width space" chars
            text = text.Replace("\u200B", string.Empty);
            text = text.Replace("\u200b", string.Empty);

            // Check for empty text or "Zero width space"
            if (string.IsNullOrEmpty(text) || (text.Length == 1 && (text[0] == '\u200B' || text[0] == '\u200b')))
            {
                return true;
            }

            using (var document = this.parser.ParseDocument(text))
            {
                if (document.Body.InnerHtml.ToLower() == "<p></p>" ||
                    document.Body.InnerHtml.ToLower() == "<span><p></p></span>")
                {
                    return true;
                }
            }

            return false;
        }
        protected virtual void CleanHtmlNodes(IHtmlDocument document)
        {
            // HR tag --> replace by BR
            foreach (var element in document.All.Where(p => p.TagName.Equals("hr", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (element.ParentElement != null)
                {
                    var container = document.CreateElement("span");
                    container.AppendChild(document.CreateElement("br"));
                    container.AppendChild(document.CreateElement("br"));
                    element.ParentElement.ReplaceChild(container, element);
                }
            }
        }

        protected virtual void CleanStyles(IHtmlDocument document)
        {
            foreach(var element in document.All.Where(p => p.HasAttribute("style")))
            {
                string foregroundColor = element.GetStyle().GetColor();
                string backgroundColor = element.GetStyle().GetBackgroundColor();
                string listStyleType = element.TagName == "OL" || element.TagName == "UL" || element.TagName == "LI"
                    ? element.GetStyle().GetPropertyValue("list-style-type") : null;

                if (string.IsNullOrEmpty(element.GetAttribute("style")))
                {
                    // If the style attribute was empty then drop it
                    element.RemoveAttribute("style");
                }
                else
                {
                    if (IsBlockElement(element) &&
                        !(element.TagName == "FIGURE" && element.ClassList.Contains("canvasRteResponsiveTable")))
                    {
                        // Save the styles we want to maintain
                        string marginLeft = element.GetStyle().GetMarginLeft();
                        string textAlign = element.GetStyle().GetTextAlign();

                        // Delete all styling information from the element
                        element.RemoveAttribute("style");

                        // Add the "styles to keep" again
                        if (!string.IsNullOrEmpty(marginLeft))
                        {
                            element.GetStyle().SetMarginLeft(marginLeft);
                        }
                        if (!string.IsNullOrEmpty(textAlign))
                        {
                            element.GetStyle().SetTextAlign(textAlign);
                        }
                    }
                    else
                    {
                        // Save the styles we want to maintain
                        string width = element.GetStyle().GetWidth();
                        string textAlign = element.GetStyle().GetTextAlign();

                        // Delete all styling information from the element
                        element.RemoveAttribute("style");

                        // Add the "styles to keep" again
                        if (!string.IsNullOrEmpty(width))
                        {
                            element.GetStyle().SetWidth(width);
                        }
                        if (!string.IsNullOrEmpty(textAlign))
                        {
                            element.GetStyle().SetTextAlign(textAlign);
                        }
                    }

                    if (!string.IsNullOrEmpty(foregroundColor))
                    {
                        element.GetStyle().SetColor(foregroundColor);
                    }
                    if (!string.IsNullOrEmpty(backgroundColor))
                    {
                        element.GetStyle().SetBackgroundColor(backgroundColor);
                    }
                    if (!string.IsNullOrEmpty(listStyleType))
                    {
                        element.GetStyle().SetProperty("list-style-type", listStyleType);
                    }
                }
            }
        }

        private static void TransformLists(IHtmlDocument document)
        {
            foreach (var list in document.QuerySelectorAll("ol, ul"))
            {
                if (list.TagName == "UL" && list.QuerySelector("ol") == null)
                {
                    continue;
                }

                // RTE uses this class on list containers to disable alphabetic LI and ::before overrides.
                // Include UL ancestors so ordered lists nested inside bulleted lists inherit their own marker style.
                list.ClassList.Add("customListStyle");
                if (list.TagName == "OL" && string.IsNullOrEmpty(list.GetStyle().GetPropertyValue("list-style-type")))
                {
                    list.GetStyle().SetProperty("list-style-type", GetOrderedListStyle(list.GetAttribute("type")) ?? "decimal");
                }
            }

            foreach (var item in document.QuerySelectorAll("ol > li[type]"))
            {
                var marker = GetOrderedListStyle(item.GetAttribute("type"));
                if (marker != null && string.IsNullOrEmpty(item.GetStyle().GetPropertyValue("list-style-type")))
                {
                    item.GetStyle().SetProperty("list-style-type", marker);
                }
            }
        }

        private static string GetOrderedListStyle(string type)
        {
            return type switch
            {
                "1" => "decimal",
                "a" => "lower-alpha",
                "A" => "upper-alpha",
                "i" => "lower-roman",
                "I" => "upper-roman",
                _ => null
            };
        }

        /// <summary>
        /// Preserves table spans and relative column sizing in native canvas RTE markup that fits the canvas.
        /// Absolute table widths become responsive widths because RTE does not retain overflow constraints.
        /// Tables without explicit sizing, spans, or cell backgrounds retain the legacy wrapper.
        /// Nested tables remain in their owning cells for canvas RTE to render and edit.
        /// </summary>
        protected virtual void TransformTables(IHtmlCollection<IElement> tables, IHtmlDocument document)
        {
            // Transform inside-out so copying a cell includes its already transformed nested tables.
            foreach (var table in tables.Reverse())
            {
                Table normalizedTable = null;

                try
                {
                    // Resolve the logical grid without turning covered positions into empty cells.
                    normalizedTable = NormalizeTable(table);
                }
                catch(Exception ex)
                {
                    LogWarning(string.Format(LogStrings.Warning_TableCouldNotBeNormalized, ex.Message), LogStrings.Heading_BuiltInFunctions);
                }

                // If we could not normalize this table then let's skip it
                if (normalizedTable == null)
                {
                    continue;
                }

                // Skip empty tables
                if (normalizedTable.Columns == 0)
                {
                    continue;
                }

                var bandedStyle = normalizedTable.ClassName == "simpleTableStyleNeutral" ? GetTableClassName(table, true) : null;
                var bandingColors = GetHeaderlessTableBandingColors(bandedStyle);
                bool nativeTable = bandingColors != null || normalizedTable.Rows.SelectMany(row => row).Any(cell =>
                    cell.ColumnSpan > 1 || cell.RowSpan > 1 || HasExplicitWidth(cell.Element) ||
                    GetTableCellBackgroundColor(cell.Element) != null) ||
                    HasExplicitWidth(table) || table.Children.Any(child => child.TagName == "COLGROUP");
                var newTableElement = document.CreateElement(nativeTable ? "figure" : "div");
                newTableElement.ClassName = "canvasRteResponsiveTable";
                IElement tableContainer = newTableElement;
                if (nativeTable)
                {
                    // The legacy div wrapper regenerates column widths from physical, not logical, cells.
                    newTableElement.ClassList.Add("table", "tableLeftAlign");
                    newTableElement.SetAttribute("title", "Table");
                    var width = GetTableWidth(table);
                    newTableElement.GetStyle().SetWidth(TryGetColumnWidth(width, out double value, out string unit) && unit == "%"
                        ? FormatTablePercentage(Math.Min(value, 100)) : "100%");
                }
                else
                {
                    tableContainer = document.CreateElement("div");
                    tableContainer.ClassList.Add("tableLeftAlign", "tableWrapper");
                    newTableElement.AppendChild(tableContainer);
                }

                // <table class="bandedRowTableStyleNeutral" title="Table">
                var tableElement = document.CreateElement("table");
                tableElement.ClassName = normalizedTable.ClassName;
                tableElement.SetAttribute("title", "Table");
                tableContainer.AppendChild(tableElement);
                var sourceCaption = ((IHtmlTableElement)table).Caption;
                if (sourceCaption != null)
                {
                    var caption = document.CreateElement("caption");
                    caption.InnerHtml = sourceCaption.InnerHtml;
                    tableElement.AppendChild(caption);
                }
                double[] columnWidths = null;
                if (nativeTable)
                {
                    columnWidths = AddTableColumns(table, normalizedTable, tableElement, document);
                }

                // <tbody>
                var tableBody = document.CreateElement("tbody");
                tableElement.AppendChild(tableBody);

                int rowIndex = 0;
                foreach (var row in normalizedTable.Rows)
                {
                    var newRow = document.CreateElement("tr");
                    string rowBackgroundColor = null;
                    if (bandingColors != null && row.Count > 0)
                    {
                        int bandIndex = rowIndex % 2;
                        var sourceRow = row[0].Element.ParentElement;
                        string styleCode = bandedStyle == "bandedRowTableStyleTheme" ? "6" : "1";
                        if (sourceRow.ClassList.Any(name => name.Equals($"ms-rteTableEvenRow-{styleCode}", StringComparison.OrdinalIgnoreCase)))
                        {
                            bandIndex = 0;
                        }
                        else if (sourceRow.ClassList.Any(name => name.Equals($"ms-rteTableOddRow-{styleCode}", StringComparison.OrdinalIgnoreCase)))
                        {
                            bandIndex = 1;
                        }
                        rowBackgroundColor = bandingColors[bandIndex];
                    }
                    foreach (var cell in row)
                    {
                        var newTableCell = document.CreateElement("td");
                        newTableCell.GetStyle().SetWidth(columnWidths == null ? "auto" :
                            FormatTablePercentage(columnWidths.Skip(cell.Column).Take(cell.ColumnSpan).Sum()));
                        var cellBackgroundColor = GetTableCellBackgroundColor(cell.Element) ?? rowBackgroundColor;
                        if (cellBackgroundColor != null)
                        {
                            newTableCell.GetStyle().SetBackgroundColor(cellBackgroundColor);
                        }
                        var foregroundColor = cell.Element.GetStyle().GetColor();
                        if (!string.IsNullOrEmpty(foregroundColor))
                        {
                            newTableCell.GetStyle().SetColor(foregroundColor);
                        }
                        if (cell.ColumnSpan > 1)
                        {
                            newTableCell.SetAttribute("colspan", cell.ColumnSpan.ToString(CultureInfo.InvariantCulture));
                        }
                        if (cell.RowSpan > 1)
                        {
                            newTableCell.SetAttribute("rowspan", cell.RowSpan.ToString(CultureInfo.InvariantCulture));
                        }
                        if (cell.Element.TagName == "TH")
                        {
                            var strong = document.CreateElement("strong");
                            strong.InnerHtml = cell.Element.InnerHtml;
                            newTableCell.AppendChild(strong);
                        }
                        else
                        {
                            newTableCell.InnerHtml = cell.Element.InnerHtml;
                        }
                        var textAlign = cell.Element.GetStyle().GetTextAlign();
                        if (!string.IsNullOrEmpty(textAlign))
                        {
                            newTableCell.GetStyle().SetTextAlign(textAlign);
                        }

                        newRow.AppendChild(newTableCell);
                    }

                    // Append the new row
                    tableBody.AppendChild(newRow);
                    rowIndex++;
                }

                table.Parent.ReplaceChild(newTableElement, table);
            }
        }

        protected virtual void ImageIFramePlaceHolders(IHtmlDocument document)
        {
            var images = document.QuerySelectorAll("img");
            var iframes = document.QuerySelectorAll("iframe");
            var elements = images.Union(iframes);

            foreach(var element in elements)
            {
                // Add a text content in place of the element
                string webPartType = "";
                string sourceValue = "";
                var source = element.Attributes.Where(p => p.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (source != null)
                {
                    sourceValue = source.Value;
                }
                if (element is IHtmlImageElement)
                {
                    webPartType = "Image";
                }
                else if (element is IHtmlInlineFrameElement)
                {
                    webPartType = "IFrame";
                }

                string placeHolder = $"***{webPartType} placeholder for source {sourceValue}***";

                // Create P element and insert it just before our current image or iframe element
                var newElement = document.CreateElement($"P");
                newElement.TextContent = placeHolder;

                if (element.Parent != null)
                {
                    element.Parent.InsertBefore(newElement, element);
                }
            }
        }

        protected virtual void TransformBlockQuotes(IHtmlCollection<IElement> blockQuotes, IHtmlDocument document)
        {
            int level = 1;
            // Dictionary that holds the nodes that will be replaced. Replacement node is a span that groups the elements at the given level.
            // We can't immediately replace the nodes as that will break the model where we walk the tree to find the level and top node
            Dictionary<IElement, IElement> replacementList = new Dictionary<IElement, IElement>();

            // Iterate over all blockquote nodes in the html
            foreach (var blockQuote in blockQuotes)
            {
                // Only process block quotes that are used for indentation
                var style = blockQuote.GetStyle();
                if (style.GetMarginLeft() == "40px" &&
                    new[] { style.GetMarginTop(), style.GetMarginRight(), style.GetMarginBottom() }
                        .All(margin => margin == "0px" || margin == "0"))
                {
                    if ((blockQuote.ChildElementCount > 0 && blockQuote.Children[0].TagName.ToLower() == "blockquote"))
                    {
                        // do nothing if we still see a blockquote as child element
                    }
                    else
                    {
                        // We're at the bottom level, where blockquotes can contain both elements and unwrapped text.
                        bool replacementDone = false;
                        IElement insertionContainer = null;
                        IElement topLevelBlockQuote = null;

                        // Calculate the level this blockquote is at, it's top level blockquote node, the insertion container for that level (if it exists), whether strike-through was used and whether underline was used
                        bool strikeThroughWasUsed = false;
                        bool underLineWasUsed = false;
                        DetectBlockQuoteLevelParentContainer(replacementList, blockQuote, ref level, ref topLevelBlockQuote, ref insertionContainer, ref strikeThroughWasUsed, ref underLineWasUsed);

                        // For the first level we get null as top level blockquote, so assign the current blockquote 
                        if (topLevelBlockQuote == null)
                        {
                            topLevelBlockQuote = blockQuote;
                        }

                        // If we found an insertion container then we'll append converted nodes to that container
                        if (insertionContainer != null)
                        {
                            replacementDone = true;
                        }

                        // Snapshot all nodes: iterating only elements would discard unwrapped answers.
                        foreach (var childNode in blockQuote.ChildNodes.ToList())
                        {
                            var nodeToProcess = childNode as IElement;
                            if (childNode is IText)
                            {
                                nodeToProcess = document.CreateElement("p");
                                nodeToProcess.TextContent = childNode.TextContent;
                            }
                            if (nodeToProcess == null)
                            {
                                continue;
                            }

                            // Indention in realized via P element with the needed pixels as margin-left
                            IElement newElement;

                            // Block elements get their indentation style on the element, others will we wrapped inside a P
                            bool isBlockElement = IsBlockElement(nodeToProcess);
                            string innerHtml = "";
                            if (isBlockElement)
                            {
                                newElement = nodeToProcess;
                                newElement.GetStyle().SetMarginLeft($"{level * 40}px");
                                // Store the block element html for later adding
                                innerHtml = nodeToProcess.InnerHtml;
                                // since we're injecting the block element content again we first remove all nodes from it
                                foreach (var child in newElement.ChildNodes.ToList())
                                {
                                    newElement.RemoveChild(child);
                                }
                            }
                            else
                            {
                                newElement = document.CreateElement($"p");
                                newElement.GetStyle().SetMarginLeft($"{level * 40}px");
                            }

                            if (strikeThroughWasUsed)
                            {
                                // Since strikethrough was used wrap the contents in an s element
                                var newLineThroughElement = document.CreateElement("s");
                                if (isBlockElement)
                                {
                                    newLineThroughElement.InnerHtml = innerHtml; 
                                }
                                else
                                {
                                    newLineThroughElement.InnerHtml = nodeToProcess.OuterHtml;
                                }
                                newElement.AppendChild(newLineThroughElement);
                            }
                            else if (underLineWasUsed)
                            {
                                // Since underline was used wrap the contents in an u element
                                var newUnderlineElement = document.CreateElement("u");
                                if (isBlockElement)
                                {
                                    newUnderlineElement.InnerHtml = innerHtml;
                                }
                                else
                                {
                                    newUnderlineElement.InnerHtml = nodeToProcess.OuterHtml;
                                }
                                newElement.AppendChild(newUnderlineElement);
                            }
                            else
                            {
                                if (isBlockElement)
                                {
                                    newElement.InnerHtml = innerHtml;
                                }
                                else
                                {
                                    newElement.InnerHtml = nodeToProcess.OuterHtml;
                                }
                            }

                            // if no insertion container was created then create it as span and add the new element
                            if (insertionContainer == null)
                            {
                                insertionContainer = document.CreateElement("span");
                                insertionContainer.AppendChild(newElement);
                            }

                            if (!replacementDone)
                            {
                                // Since we can't simply replace the node (as that would break future blockquote tree traversals) we're storing the "new" node in a list. 
                                // Replacement will happen at the very end of this flow
                                if (!replacementList.ContainsKey(topLevelBlockQuote))
                                {
                                    replacementList.Add(topLevelBlockQuote, insertionContainer);
                                }
                            }
                            else
                            {
                                // There's already an insertion container, so we can simply append our element
                                insertionContainer.AppendChild(newElement);
                            }

                            replacementDone = true;
                        }

                        // reset variables since we're starting a new blockquote
                        level = 1;
                        insertionContainer = null;
                    }
                }
            }

            // Perform the actual replacements 
            foreach (var node in replacementList)
            {
                var parent = node.Key.Parent;
                if (parent.Contains(node.Key))
                {
                    parent.ReplaceChild(node.Value, node.Key);
                }
            }
        }

        protected virtual void DropBRs(IHtmlDocument document)
        {
            var brNodes = document.QuerySelectorAll($"BR");

            foreach(var brNode in brNodes)
            {
                var parent = brNode.Parent;
                parent.RemoveChild(brNode);
            }
        }

#pragma warning disable CA1716
        protected virtual void TransformHeadings(IHtmlDocument document, int from, int to)
#pragma warning restore CA1716
        {
            var fromNodes = document.QuerySelectorAll($"h{from}");
            foreach(var fromNode in fromNodes)
            {
                var parent = fromNode.Parent;
                var colorClasses = fromNode.ClassList.Where(IsTextColorClass).ToArray();
                string foregroundColor = fromNode.GetStyle().GetColor();
                string backgroundColor = fromNode.GetStyle().GetBackgroundColor();
                bool preserveHeadingAnchor = to != 0 && !string.IsNullOrEmpty(fromNode.Id);
                if (preserveHeadingAnchor || colorClasses.Length > 0 ||
                    !string.IsNullOrEmpty(foregroundColor) || !string.IsNullOrEmpty(backgroundColor))
                {
                    // Modern rendering generates heading IDs. Keep legacy destinations on the content instead.
                    var headingContent = document.CreateElement("span");
                    if (preserveHeadingAnchor)
                    {
                        CopyId(fromNode, headingContent);
                    }
                    headingContent.ClassList.Add(colorClasses);
                    if (!string.IsNullOrEmpty(foregroundColor))
                    {
                        headingContent.GetStyle().SetColor(foregroundColor);
                    }
                    if (!string.IsNullOrEmpty(backgroundColor))
                    {
                        headingContent.GetStyle().SetBackgroundColor(backgroundColor);
                    }
                    while (fromNode.FirstChild != null)
                    {
                        headingContent.AppendChild(fromNode.FirstChild);
                    }
                    fromNode.AppendChild(headingContent);
                }

                if (to == 0)
                {
                    // wrap the content inside a div so that further formatting processing will pick it up
                    var newElement = document.CreateElement("div");
                    CopyId(fromNode, newElement);
                    newElement.InnerHtml = fromNode.InnerHtml;
                    parent.ReplaceChild(newElement, fromNode);
                }
                else
                {
                    var newElement = document.CreateElement($"h{to}");
                    newElement.InnerHtml = fromNode.InnerHtml;

                    // Copy the text alignment style
                    if (fromNode.GetStyle() != null && !string.IsNullOrEmpty(fromNode.GetStyle().GetTextAlign()))
                    {
                        newElement.GetStyle().SetTextAlign(fromNode.GetStyle().GetTextAlign());
                    }
                    parent.ReplaceChild(newElement, fromNode);
                }
            }
        }

        protected virtual void TransformElements(IHtmlCollection<IElement> elementsToTransform, IHtmlDocument document)
        {
            // Replacements copy inner HTML; transform descendants before their original nodes are detached.
            foreach (var element in elementsToTransform.Reverse())
            {
                var parent = element.Parent;
                string foregroundColor = element.GetStyle().GetColor();
                string backgroundColor = element.GetStyle().GetBackgroundColor();

                // ================================
                // Rewrite font styles outside of the header styles
                // ================================
                // <span class="ms-rteStyle-Normal">Norm</span>
                //ms-rteStyle-Normal
                //ms-rteStyle-Quote = Italic
                //ms-rteStyle-IntenseQuote = Italic + Underline
                //ms-rteStyle-Emphasis = italic + light blue
                //ms-rteStyle-IntenseEmphasis = italic + light blue + underline
                //ms-rteStyle-References = light gray
                //ms-rteStyle-IntenseReferences = light gray + underline
                //ms-rteStyle-Accent1 = light blue
                //ms-rteStyle-Accent2 = dark blue
                var rtestylenormal = element.ClassList.PartialMatch("ms-rtestyle-normal");
                if (!string.IsNullOrEmpty(rtestylenormal))
                {
                    element.ClassList.Remove(rtestylenormal);
                }

                // Replace "rte styles" with their wiki style alternatives which then can be picked up by the default processing
                var rtestyle = element.ClassList.PartialMatch("ms-rtestyle-");
                bool addItalic = false;
                if (!string.IsNullOrEmpty(rtestyle))
                {
                    // Remove the wiki style
                    element.ClassList.Remove(rtestyle);

                    rtestyle = rtestyle.ToLower().Replace("ms-rtestyle-", "");
                    // Underline style
                    if (rtestyle.Equals("IntenseQuote", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseEmphasis", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseReferences", StringComparison.InvariantCultureIgnoreCase) )
                    {
                        // Update current element Style which will be picked up later on 
                        element.GetStyle().SetTextDecoration("underline");
                    }

                    // Light blue
                    if (rtestyle.Equals("Emphasis", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseEmphasis", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("Accent1", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Update current element ClassList which will be picked up later on 
                        element.ClassList.Add("ms-rteforecolor-8");
                    }

                    // Light gray
                    if (rtestyle.Equals("References", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseReferences", StringComparison.InvariantCultureIgnoreCase) )
                    {
                        // no color to add as default is light gray
                    }

                    // Dark blue
                    if (rtestyle.Equals("Accent2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Update current element ClassList which will be picked up later on 
                        element.ClassList.Add("ms-rteforecolor-9");
                    }

                    // Italic style
                    if (rtestyle.Equals("Quote", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseQuote", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("Emphasis", StringComparison.InvariantCultureIgnoreCase) ||
                        rtestyle.Equals("IntenseEmphasis", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // As we need to insert an EM element we can't do that right now. Let's start with setting an indicator 
                        addItalic = true;
                    }
                }

                // ================================
                // rewrite colors, back and fore color + size can be defined as class on a single span element
                // ================================
                // <span class="ms-rteThemeForeColor-5-0">red</span>
                var themeForeColor = element.ClassList.PartialMatch("ms-rtethemeforecolor-");
                if (!string.IsNullOrEmpty(themeForeColor))
                {
                    if (!string.IsNullOrEmpty(foregroundColor))
                    {
                        element.ClassList.Remove(themeForeColor);
                    }
                    else
                    {
                        var shadeName = GetThemeShadeName(themeForeColor);
                        if (shadeName != null && themeColorResolver != null)
                        {
                            element.GetStyle().SetColor(themeColorResolver(shadeName));
                            foregroundColor = element.GetStyle().GetColor();
                            if (string.IsNullOrEmpty(foregroundColor))
                            {
                                throw new InvalidOperationException($"The source theme returned an invalid color for '{shadeName}'.");
                            }
                            element.ClassList.Remove(themeForeColor);
                        }
                        else
                        {
                            LogWarning($"Cannot resolve classic foreground color '{themeForeColor}' without a valid source theme shade. Retaining the class.",
                                LogStrings.Heading_BuiltInFunctions);
                        }
                    }
                }

                // <span class="ms-rteThemeBackColor-5-0">red</span>
                var rtethemebackcolor = element.ClassList.PartialMatch("ms-rtethemebackcolor-");
                if (!string.IsNullOrEmpty(rtethemebackcolor))
                {
                    if (!string.IsNullOrEmpty(backgroundColor))
                    {
                        element.ClassList.Remove(rtethemebackcolor);
                    }
                    else
                    {
                        var shadeName = GetThemeShadeName(rtethemebackcolor);
                        if (shadeName != null && themeColorResolver != null)
                        {
                            element.GetStyle().SetBackgroundColor(themeColorResolver(shadeName));
                            backgroundColor = element.GetStyle().GetBackgroundColor();
                            if (string.IsNullOrEmpty(backgroundColor))
                            {
                                throw new InvalidOperationException($"The source theme returned an invalid color for '{shadeName}'.");
                            }
                            element.ClassList.Remove(rtethemebackcolor);
                        }
                        else
                        {
                            LogWarning($"Cannot resolve classic background color '{rtethemebackcolor}' without a valid source theme shade. Retaining the class.",
                                LogStrings.Heading_BuiltInFunctions);
                        }
                    }
                }

                //<span class="ms-rteForeColor-2" style="">Red,&#160;</span>
                //<sup class="ms-rteForeColor-10" style=""><strong style="">superscript</strong></sup>
                var rteforecolor = element.ClassList.PartialMatch("ms-rteforecolor-");
                if (!string.IsNullOrEmpty(rteforecolor))
                {
                    // Modern Theme colors
                    // Dark Red, Red, Orange, Yellow, Light green
                    // Green, Light Blue, Blue, Dark Blue, Purple

                    string newClass = null;
                    if (int.TryParse(rteforecolor.ToLower().Replace("ms-rteforecolor-", ""), out int colorCode))
                    {
                        string colorName = ColorCodeToForegroundColorName(colorCode);
                        if (!string.IsNullOrEmpty(colorName))
                        {
                            newClass = $"fontColor{colorName}";
                            if (string.IsNullOrEmpty(foregroundColor))
                            {
                                foregroundColor = new[] { "darkred", "red", "orange", "yellow", "lightgreen",
                                    "green", "lightblue", "blue", "darkblue", "purple" }[colorCode - 1];
                            }
                        }
                    }

                    element.ClassList.Remove(rteforecolor);
                    if (!string.IsNullOrEmpty(newClass))
                    {
                        // We mapped a color
                        element.ClassList.Add(newClass);
                    }
                }

                // CSS colors take precedence over the legacy font color attribute.
                if (element.TagName.Equals("font", StringComparison.OrdinalIgnoreCase) && element.HasAttribute("color"))
                {
                    if (string.IsNullOrEmpty(foregroundColor))
                    {
                        element.GetStyle().SetColor(element.GetAttribute("color"));
                        foregroundColor = element.GetStyle().GetColor();
                    }
                    if (!string.IsNullOrEmpty(foregroundColor))
                    {
                        element.RemoveAttribute("color");
                    }
                }
                if (!string.IsNullOrEmpty(foregroundColor))
                {
                    element.GetStyle().SetColor(foregroundColor);
                }

                // <sub class="ms-rteBackColor-2">lowerscript</sub>
                var rtebackcolor = element.ClassList.PartialMatch("ms-rtebackcolor-");
                if (!string.IsNullOrEmpty(rtebackcolor))
                {
                    // Modern Theme colors
                    // Dark Red, Red, Orange, Yellow, Light green
                    // Green, Light Blue, Blue, Dark Blue, Purple

                    string newClass = null;
                    if (int.TryParse(rtebackcolor.ToLower().Replace("ms-rtebackcolor-", ""), out int colorCode))
                    {
                        string colorName = ColorCodeToBackgroundColorName(colorCode);
                        if (!string.IsNullOrEmpty(colorName))
                        {
                            newClass = $"highlightColor{colorName}";
                        }
                    }

                    element.ClassList.Remove(rtebackcolor);
                    if (!string.IsNullOrEmpty(newClass))
                    {
                        // We mapped a color
                        element.ClassList.Add(newClass);
                    }
                }

                // ================================
                // rewrite font size
                // ================================
                var rtefontsize = element.ClassList.PartialMatch("ms-rtefontsize-");
                if (!string.IsNullOrEmpty(rtefontsize))
                {
                    string newClass = null;
                    if (int.TryParse(rtefontsize.ToLower().Replace("ms-rtefontsize-", ""), out int fontsizeCode))
                    {
                        string fontSize = FontCodeToName(fontsizeCode);
                        if (!string.IsNullOrEmpty(fontSize))
                        {
                            newClass = $"fontSize{fontSize}";
                        }
                    }

                    element.ClassList.Remove(rtefontsize);
                    if (!string.IsNullOrEmpty(newClass))
                    {
                        // We mapped a color
                        element.ClassList.Add(newClass);
                    }
                }

                // ================================
                // rewrite fonts
                // ================================
                // <span class="ms-rteFontFace-7">custom font</span>
                var rtefontface = element.ClassList.PartialMatch("ms-rtefontface-");
                if (!string.IsNullOrEmpty(rtefontface))
                {
                    // There are no themed back colors in modern, so for now drop the color span and the background color
                    element.ClassList.Remove(rtefontface);
                }

                // rewrite striked and underline
                // <span style="text-decoration&#58;line-through;">striked</span>
                // <span style="text-decoration&#58;underline;">underline</span>
                bool replacementDone = false;
                bool isStrikeThroughOnElementToKeep = false;
                bool isUnderlineOnElementToKeep = false;
                string elementToKeep = "";
                if (IsStrikeThrough(element))
                {
                    // strike through can be on an element that we're replacing as well (like em)...if so don't
                    // replace em with strike through now, but wait until at the very end 
                    if (element.TagName.Equals("em", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("strong", StringComparison.InvariantCultureIgnoreCase) ||
                        //element.TagName.Equals("blockquote", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("sup", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("sub", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elementToKeep = element.TagName;
                        isStrikeThroughOnElementToKeep = true;
                    }
                    else
                    {
                        ReplaceByRelevantTag(document, element, parent, "s");
                        replacementDone = true;
                    }
                }
                else if (IsUnderline(element))
                {
                    if (element.TagName.Equals("em", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("strong", StringComparison.InvariantCultureIgnoreCase) ||
                        //element.TagName.Equals("blockquote", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("sup", StringComparison.InvariantCultureIgnoreCase) ||
                        element.TagName.Equals("sub", StringComparison.InvariantCultureIgnoreCase))
                    {
                        elementToKeep = element.TagName;
                        isUnderlineOnElementToKeep = true;
                    }
                    else
                    {
                        // Don't apply the default logic when we need to insert a EM tag...a node can only be replaced once, so postpone the 
                        // replacement by an U element when we also need to insert an EM tag
                        if (!addItalic)
                        {
                            ReplaceByRelevantTag(document, element, parent, "u");
                            replacementDone = true;
                        }
                    }
                }

                // No need to wrap a span into a new span
                if (element is IHtmlSpanElement)
                {
                    if (addItalic)
                    {
                        // We needed to insert an EM tag as the provided style uses italic
                        var newElement = document.CreateElement("span");
                        CopyId(element, newElement);
                        newElement.ClassList.Add(element.ClassList.ToArray());
                        if (!string.IsNullOrEmpty(foregroundColor))
                        {
                            newElement.GetStyle().SetColor(foregroundColor);
                        }
                        if (!string.IsNullOrEmpty(backgroundColor))
                        {
                            newElement.GetStyle().SetBackgroundColor(backgroundColor);
                        }
                        var newItalic = document.CreateElement("em");

                        // Transform the piece of html we add in this span, needed to handle spans nested in here as these otherwise would not be found anymore
                        string innerHtml = TransformInnerHtml(element.InnerHtml);

                        // If the implemented style also used underline then add the U node as well
                        if (IsUnderline(element))
                        {
                            var newUnderline = document.CreateElement("u");
                            newUnderline.InnerHtml = innerHtml;
                            newItalic.AppendChild(newUnderline);
                        }
                        else
                        {
                            newItalic.InnerHtml = innerHtml;                         
                        }

                        newElement.AppendChild(newItalic);
                        parent.ReplaceChild(newElement, element);
                    }
                    else
                    {
                        // Plain spans can be unwrapped unless they are fragment destinations.
                        if (!replacementDone && element.ClassList.Length == 0 && element.FirstElementChild == null &&
                            string.IsNullOrEmpty(foregroundColor) && string.IsNullOrEmpty(backgroundColor) &&
                            string.IsNullOrEmpty(element.Id))
                        {
                            ReplaceChildElementByText(parent, element, document);
                        }
                        else
                        {
                            // drop style attribute if still present
                            if (element.HasAttribute("style"))
                            {
                                element.RemoveAttribute("style");
                            }
                            if (!string.IsNullOrEmpty(foregroundColor))
                            {
                                element.GetStyle().SetColor(foregroundColor);
                            }
                            if (!string.IsNullOrEmpty(backgroundColor))
                            {
                                element.GetStyle().SetBackgroundColor(backgroundColor);
                            }
                            // if the element has no classes anymore then let's drop the class attribute
                            if (element.ClassList.Length == 0 && element.HasAttribute("class"))
                            {
                                element.RemoveAttribute("class");
                            }
                        }
                    }
                }
                else
                {
                    // Non span element with styling that was transformed will be wrapped in a span containing the styling which wraps a "clean" element
                    var newElement = document.CreateElement("span");
                    newElement.ClassList.Add(element.ClassList.ToArray());
                    if (!string.IsNullOrEmpty(foregroundColor))
                    {
                        newElement.GetStyle().SetColor(foregroundColor);
                    }
                    if (!string.IsNullOrEmpty(backgroundColor))
                    {
                        newElement.GetStyle().SetBackgroundColor(backgroundColor);
                    }
                    // no point in having an empty class attribute
                    if (element.HasAttribute("class"))
                    {
                        element.RemoveAttribute("class");
                    }

                    if (isStrikeThroughOnElementToKeep) 
                    {
                        var strikeThroughElement = document.CreateElement("s");
                        newElement.AppendChild(strikeThroughElement);                        
                        // Create the element that held the strikethrough style
                        var emElement = document.CreateElement(elementToKeep.ToLower());
                        CopyId(element, emElement);
                        emElement.InnerHtml = element.InnerHtml;
                        strikeThroughElement.AppendChild(emElement);
                    }
                    else if (isUnderlineOnElementToKeep)
                    {
                        var underlineElement = document.CreateElement("u");
                        newElement.AppendChild(underlineElement);
                        // Create the element that held the underline style
                        var emElement = document.CreateElement(elementToKeep.ToLower());
                        CopyId(element, emElement);
                        emElement.InnerHtml = element.InnerHtml;
                        underlineElement.AppendChild(emElement);
                    }
                    else
                    {
                        newElement.InnerHtml = element.OuterHtml;
                    }

                    // Only replace the element if it's still available...it could have been replaced earlier on
                    if (parent.Contains(element))
                    {
                        parent.ReplaceChild(newElement, element);
                    }
                }
            }
        }

        private static bool IsTextColorClass(string className)
        {
            return className.StartsWith("ms-rteForeColor-", StringComparison.OrdinalIgnoreCase) ||
                className.StartsWith("ms-rteThemeForeColor-", StringComparison.OrdinalIgnoreCase) ||
                className.StartsWith("ms-rteBackColor-", StringComparison.OrdinalIgnoreCase) ||
                className.StartsWith("ms-rteThemeBackColor-", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetThemeShadeName(string className)
        {
            var match = Regex.Match(className, @"^ms-rteTheme(?:Fore|Back)Color-([1-9]|10)-([0-5])$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                return null;
            }

            var slots = new[] { "BackgroundOverlay", "BodyText", "SubtleEmphasisText", "SubtleEmphasisBackground",
                "ContentAccent1", "ContentAccent2", "ContentAccent3", "ContentAccent4", "ContentAccent5", "ContentAccent6" };
            var shades = new[] { "", "-Lightest", "-Lighter", "-Medium", "-Darker", "-Darkest" };
            return slots[int.Parse(match.Groups[1].Value) - 1] + shades[int.Parse(match.Groups[2].Value)];
        }

        private static string GetTableClassName(IElement table, bool hasHeader)
        {
            string borderedStyle = hasHeader ? "borderHeaderTableStyleNeutral" : "simpleTableStyleNeutral";
            const string borderlessStyle = "noBorderTableStyleNeutral";
            const string classicStylePrefix = "ms-rteTable-";
            string mappedStyle = null;
            foreach (var className in table.ClassList)
            {
                if (className.Equals("ms-rteTable-default", StringComparison.OrdinalIgnoreCase))
                {
                    mappedStyle = borderedStyle;
                    break;
                }

                if (className.StartsWith(classicStylePrefix, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(className.Substring(classicStylePrefix.Length), out int styleCode) &&
                    styleCode >= 0 && styleCode <= 10)
                {
                    mappedStyle = TableStyleCodeToName(styleCode);
                    break;
                }
            }

            if (mappedStyle != null && mappedStyle != borderlessStyle)
            {
                // Banded styles also fill and bold the first row, even when it contains only body cells.
                if (!hasHeader && (mappedStyle == "filledHeaderTableStyleTheme" || mappedStyle == "filledHeaderTableStyleNeutral" ||
                    mappedStyle == "bandedRowTableStyleTheme" || mappedStyle == "bandedRowTableStyleNeutral"))
                {
                    return "simpleTableStyleNeutral";
                }

                return mappedStyle;
            }

            bool hasBorderAttribute = int.TryParse(table.GetAttribute("border"), out int borderWidth);
            // A nested table's borders must not change its containing table's style.
            bool hasVisibleBorder = (hasBorderAttribute && borderWidth > 0) || HasVisibleBorder(table) ||
                table.QuerySelectorAll("tr,td,th")
                    .Any(element => element.Closest("table") == table && HasVisibleBorder(element));
            if (hasVisibleBorder)
            {
                return borderedStyle;
            }

            var style = table.GetStyle();
            bool explicitlyBorderless = (hasBorderAttribute && borderWidth == 0) ||
                (style != null && new[] { "top", "right", "bottom", "left" }
                    .All(side => IsBorderlessSide(style, side)));
            if (mappedStyle == borderlessStyle || table.ClassList.Length == 0 || explicitlyBorderless)
            {
                return borderlessStyle;
            }

            return borderedStyle;
        }

        private static string GetTableCellBackgroundColor(IElement cell)
        {
            return cell.GetStyle().GetProperty("background-color")?.RawValue is CssColorValue color && color.A > 0
                ? color.CssText : null;
        }

        private string[] GetHeaderlessTableBandingColors(string style)
        {
            bool themed = style == "bandedRowTableStyleTheme";
            if (!themed && style != "bandedRowTableStyleNeutral")
            {
                return null;
            }

            if (themeColorResolver == null)
            {
                LogWarning("Cannot resolve source table banding without a source theme. Using the classic default palette.",
                    LogStrings.Heading_BuiltInFunctions);
            }

            CssColorValue ResolveColor(string shade, string defaultColor)
            {
                string color = themeColorResolver == null ? defaultColor : themeColorResolver(shade);
                if (string.IsNullOrEmpty(color) || !CssColorValue.TryFromHex(color.TrimStart('#'), out var parsed))
                {
                    throw new InvalidOperationException($"The source theme returned an invalid color for '{shade}'.");
                }
                return parsed;
            }

            var background = ResolveColor("BackgroundOverlay", "#ffffff");
            var band = ResolveColor(themed ? "ContentAccent1-Lightest" : "BackgroundOverlay-Lighter", themed ? "#c0e4ff" : "#d8d8d8");
            if (themed)
            {
                // Classic style 6 uses 70% opacity. The modern cell editor drops translucent fills, so flatten against the source background.
                band = CssColorValue.FromRgb(
                    (byte)Math.Round(band.R * 0.7 + background.R * 0.3, MidpointRounding.AwayFromZero),
                    (byte)Math.Round(band.G * 0.7 + background.G * 0.3, MidpointRounding.AwayFromZero),
                    (byte)Math.Round(band.B * 0.7 + background.B * 0.3, MidpointRounding.AwayFromZero));
            }
            return new[] { background.CssText, band.CssText };
        }

        private static bool HasVisibleBorder(IElement element)
        {
            var style = element.GetStyle();
            return style != null && new[] { "top", "right", "bottom", "left" }
                .Any(side => !string.IsNullOrEmpty(style.GetPropertyValue($"border-{side}-style")) &&
                    !IsBorderlessSide(style, side));
        }

        private static bool IsBorderlessSide(ICssStyleDeclaration style, string side)
        {
            string borderStyle = style.GetPropertyValue($"border-{side}-style");
            string borderWidth = style.GetPropertyValue($"border-{side}-width");
            return borderStyle.Equals("none", StringComparison.OrdinalIgnoreCase) ||
                borderStyle.Equals("hidden", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(borderWidth, @"^0(?:\.0+)?(?:[a-z]+)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Map wiki table style to a RTE compatible style
        /// </summary>
        /// <param name="tableStyleCode">Code used for the wiki table style</param>
        /// <returns>RTE compatible table style</returns>
        /// <remarks>
        /// Filled-header, banded, and bordered-header styles emphasize the first row.
        /// Transformation uses a neutral grid for headerless tables, preserving banded fills as native cell backgrounds.
        /// </remarks>
        public static string TableStyleCodeToName(int tableStyleCode)
        {
            //ms-rteTable-default: basic grid lines
            //ms-rteTable-0: no grid
            //ms-rteTable-1: table style 2, light banded: no header, alternating light gray rows, no grid
            //ms-rteTable-2: table style 4, light lines: header, no alternating rows, no grid
            //ms-rteTable-3: table style 5, grid: no header, no alternating rows, grid
            //ms-rteTable-4: table style 6, Accent 1: header, no alternating rows, grid, blue colors
            //ms-rteTable-5: table style 7, Accent 2: header, no alternating rows, grid, light blue colors
            //ms-rteTable-6: table style 3, medium two tones: header with alternating blue colors, no grid
            //ms-rteTable-7: table style 8, Accent 3: header, no alternating rows, grid, green colors
            //ms-rteTable-8: table style 9, Accent 4: header, no alternating rows, grid, brownish colors
            //ms-rteTable-9: table style 10, Accent 5: header, no alternating rows, grid, red colors
            //ms-rteTable-10: table style 11, Accent 6: header, no alternating rows, grid, purple colors

            switch (tableStyleCode)
            {
                case 0:
                    {
                        return "noBorderTableStyleNeutral";
                    }
                case 3:
                    {
                        return "simpleTableStyleNeutral";
                    }
                case 1:
                    {
                        return "bandedRowTableStyleNeutral";
                    }
                case 2:
                    {
                        return "filledHeaderTableStyleNeutral";
                    }
                case 4:
                case 5:
                case 7:
                case 8:
                case 9:
                case 10:
                    {
                        return "filledHeaderTableStyleTheme";
                    }
                case 6:
                    {
                        return "bandedRowTableStyleTheme";
                    }
            }

            return "borderHeaderTableStyleNeutral";
        }

        /// <summary>
        /// Translates SharePoint wiki font size (e.g. ms-rtefontsize-3 means font size 3) to RTE font size name
        /// </summary>
        /// <param name="fontCode">Wiki font size code</param>
        /// <returns>RTE font size name</returns>
        public static string FontCodeToName(int fontCode)
        {
            switch (fontCode)
            {
                case 1: //8pt
                    {
                        return "Small";
                    }
                case 2: //10t
                    {
                        return "Medium";
                    }
                case 3: //12pt
                    {
                        return "MediumPlus";
                    }
                case 4: //18pt 
                    {
                        // Return empty as this will be mapped to default size
                        return "";
                    }
                case 5: //24pt
                    {
                        return "XxLarge";
                    }
                case 6: //36pt
                    {
                        return "XxxLarge";
                    }
                case 7: //48pt
                    {
                        return "XxLargePlus";
                    }
                case 8: //72pt
                    {
                        return "Super";
                    }
            }

            return null;
        }

        /// <summary>
        /// Translated SharePoint Wiki foreground color number (ms-rteforecolor-2 means number 2 is used) to RTE compatible color name
        /// </summary>
        /// <param name="colorCode">Used color number</param>
        /// <returns>RTE color string</returns>
        public static string ColorCodeToForegroundColorName(int colorCode)
        {
            switch (colorCode)
            {
                case 1:
                    {
                        return "RedDark";
                    }
                case 2:
                    {
                        return "Red";
                    }
                case 3:
                    {
                        return "Yellow";
                    }
                case 4:
                    {
                        return "YellowLight";
                    }
                case 5:
                    {
                        return "GreenLight";
                    }
                case 6:
                    {
                        return "Green";
                    }
                case 7:
                    {
                        return "BlueLight";
                    }
                case 8:
                    {
                        return "Blue";
                    }
                case 9:
                    {
                        return "BlueDark";
                    }
                case 10:
                    {
                        return "Purple";
                    }
            }

            return null;
        }

        /// <summary>
        /// Translated SharePoint Wiki foreground theme color number (e.g. ms-rteThemeForeColor-6-1) to RTE compatible color name
        /// </summary>
        /// <param name="themeCode">Theme color code</param>
        /// <returns>RTE color string</returns>
        public static string ThemeCodeToForegroundColorName(int themeCode)
        {
            switch (themeCode)
            {
                case 0:
                    {
                        // 0 (light) will go to NeutralPrimary which is the default, hence returning null
                        return null;
                    }
                case 1:
                    {
                        return "ThemeSecondary";
                    }
                case 2:
                    {
                        return "ThemePrimary";
                    }
                case 3:
                    {
                        return "ThemeDarkAlt";
                    }
                case 4:
                    {
                        return "ThemeDark";
                    }
                case 5:
                    {
                        return "ThemeDarker";
                    }
            }

            return null;
        }

        /// <summary>
        /// Translated SharePoint Wiki background color number (ms-rtebackcolor-5 means number 5 is used) to RTE compatible color name
        /// </summary>
        /// <param name="colorCode">Used color number</param>
        /// <returns>RTE color string</returns>
        public static string ColorCodeToBackgroundColorName(int colorCode)
        {
            switch (colorCode)
            {
                case 1:
                    {
                        return "Maroon";
                    }
                case 2:
                    {
                        return "Red";
                    }
                case 3:
                    {
                        return "Yellow";
                    }
                case 4:
                    {
                        return "Yellow";
                    }
                case 5:
                    {
                        return "Green";
                    }
                case 6:
                    {
                        return "Green";
                    }
                case 7:
                    {
                        return "Aqua";
                    }
                case 8:
                    {
                        return "Blue";
                    }
                case 9:
                    {
                        return "DarkBlue";
                    }
                case 10:
                    {
                        return "Purple";
                    }
            }

            return null;
        }

        #region Helper methods
        private static void PreserveStrikethrough(IElement element, IHtmlDocument document, bool strikeThrough = false, bool underline = false)
        {
            if (IsStrikeThrough(element))
            {
                strikeThrough = true;
                underline |= IsUnderline(element);
                element.GetStyle().RemoveProperty("text-decoration");
                element.GetStyle().RemoveProperty("text-decoration-line");
            }

            // Materialize decoration on text before headings/containers lose their styles.
            // Do not wrap blocks in <s>: that would break heading and list structure in the editor.
            foreach (var child in element.ChildNodes.ToArray())
            {
                if (child is IElement childElement)
                {
                    if (childElement.TagName != "SCRIPT" && childElement.TagName != "STYLE" &&
                        childElement.TagName != "TEXTAREA")
                    {
                        PreserveStrikethrough(childElement, document, strikeThrough, underline);
                    }
                }
                else if (strikeThrough && child is IText && !string.IsNullOrWhiteSpace(child.TextContent))
                {
                    INode content = child;
                    if (element.Closest("s, strike, del") == null)
                    {
                        var strike = document.CreateElement("s");
                        element.ReplaceChild(strike, content);
                        strike.AppendChild(content);
                        content = strike;
                    }
                    if (underline && element.Closest("u") == null)
                    {
                        var underlined = document.CreateElement("u");
                        element.ReplaceChild(underlined, content);
                        underlined.AppendChild(content);
                    }
                }
            }
        }

        /// <summary>
        /// Recursively loop the blockquote elements until we're at the top level, returns needed information to process:
        /// - Level: how many indents where used
        /// - TopLevelBlockQuote: what is the top level blockquote that we'll be using as "replacement node"
        /// - If there already was a container created to store content at this level then let's return that one
        /// - If by walking the blockquote tree we see strike through being used then indicate that
        /// - If by walking the blockquote tree we see underline being used then indicate that
        /// </summary>
        /// <param name="replacementList"></param>
        /// <param name="blockQuote"></param>
        /// <param name="level"></param>
        /// <param name="topLevelBlockQuote"></param>
        /// <param name="insertionContainer"></param>
        /// <param name="strikeThroughWasUsed"></param>
        /// <param name="underLineWasUsed"></param>
        private void DetectBlockQuoteLevelParentContainer(Dictionary<IElement, IElement> replacementList, IElement blockQuote, ref int level, ref IElement topLevelBlockQuote, ref IElement insertionContainer, ref bool strikeThroughWasUsed, ref bool underLineWasUsed)
        {
            if (IsStrikeThrough(blockQuote))
            {
                strikeThroughWasUsed = true;
            }
            if (IsUnderline(blockQuote))
            {
                underLineWasUsed = true;
            }

            if (blockQuote.Parent is IElement)
            {
                if (blockQuote.ParentElement.TagName.ToLower() == "blockquote")
                {
                    topLevelBlockQuote = blockQuote.ParentElement;
                    level++;

                    if (IsStrikeThrough(blockQuote.ParentElement))
                    {
                        strikeThroughWasUsed = true;
                    }
                    if (IsUnderline(blockQuote.ParentElement))
                    {
                        underLineWasUsed = true;
                    }

                    DetectBlockQuoteLevelParentContainer(replacementList, blockQuote.ParentElement, ref level, ref topLevelBlockQuote, ref insertionContainer, ref strikeThroughWasUsed, ref underLineWasUsed);
                }
                else
                {
                    if (insertionContainer == null && topLevelBlockQuote != null && replacementList.ContainsKey(topLevelBlockQuote))
                    {
                        insertionContainer = replacementList[topLevelBlockQuote];
                    }
                }
            }
        }

        private string TransformInnerHtml(string innerHtml)
        {
            // Let's inspect if there's still span's in the html we take over...these spans are not in our current list of spans 
            // and as such will be ignored if we don't handle them.
            using (var documentTemp = this.parser.ParseDocument(innerHtml))
            {
                TransformElements(documentTemp.QuerySelectorAll("span"), documentTemp);
                //TransformElements(documentTemp.QuerySelectorAll("sup"), documentTemp);
                //TransformElements(documentTemp.QuerySelectorAll("sub"), documentTemp);
                //TransformElements(documentTemp.QuerySelectorAll("strong"), documentTemp);
                //TransformElements(documentTemp.QuerySelectorAll("em"), documentTemp);

                if (documentTemp.DocumentElement.Children.Length > 1)
                {
                    innerHtml = documentTemp.DocumentElement.Children[1].InnerHtml;
                }
            }

            return innerHtml;
        }

        private void ReplaceByRelevantTag(IHtmlDocument document, IElement element, INode parent, string tag)
        {
            var newElement = document.CreateElement(tag);
            CopyId(element, newElement);

            // Transform the piece of html we add in this span, needed to handle spans nested in here as these otherwise would not be found anymore
            string innerHtml = TransformInnerHtml(element.InnerHtml);

            // If the element has a class defined then wrap inside a span
            string foregroundColor = element.GetStyle().GetColor();
            string backgroundColor = element.GetStyle().GetBackgroundColor();
            if (element.ClassList.Length > 0 || !string.IsNullOrEmpty(foregroundColor) || !string.IsNullOrEmpty(backgroundColor))
            {
                var newSpan = document.CreateElement("span");
                newSpan.ClassList.Add(element.ClassList.ToArray());
                if (!string.IsNullOrEmpty(foregroundColor))
                {
                    newSpan.GetStyle().SetColor(foregroundColor);
                }
                if (!string.IsNullOrEmpty(backgroundColor))
                {
                    newSpan.GetStyle().SetBackgroundColor(backgroundColor);
                }
                newSpan.InnerHtml = innerHtml;
                newElement.AppendChild(newSpan);
            }
            else
            {
                newElement.InnerHtml = innerHtml;
            }

            parent.ReplaceChild(newElement, element);
        }

        private static void CopyId(IElement source, IElement target)
        {
            if (!string.IsNullOrEmpty(source.Id))
            {
                target.Id = source.Id;
            }
        }

        private static void ReplaceChildElementByText(INode parent, IElement child, IHtmlDocument document)
        {
            if (!string.IsNullOrEmpty(child.TextContent))
            {
                // Add a text content in place of the element
                var newElement = document.CreateTextNode(child.TextContent);
                parent.ReplaceChild(newElement, child);
            }
            else
            {
                // If no content then drop the element
                parent.RemoveChild(child);
            }
        }

        private static bool IsBlockElement(IElement element)
        {
            var tag = element.TagName.ToLower();
            if (tag == "article" ||
                tag == "div" ||
                tag == "p" ||
                tag == "h1" ||
                tag == "h2" ||
                tag == "h3" ||
                tag == "h3" ||
                tag == "h4" ||
                tag == "h5" ||
                tag == "h6" ||
                tag == "li" ||
                tag == "ol" ||
                tag == "ul" ||
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
                tag == "hr" ||
                tag == "main" ||
                tag == "nav" ||
                tag == "noscript" ||
                tag == "output" ||
                tag == "pre" ||
                tag == "section" ||
                tag == "table" ||
                tag == "tfoot" ||
                tag == "video" ||
                tag == "aside" ||
                tag == "blockquote" )
            {
                return true;
            }

            return false;
        }

        private static string GetTableWidth(IElement element)
        {
            var width = element.GetStyle().GetWidth();
            if (!string.IsNullOrEmpty(width))
            {
                return width;
            }

            var attribute = element.GetAttribute("width")?.Trim();
            if (string.IsNullOrEmpty(attribute))
            {
                return null;
            }

            // Validate presentational attributes with the CSS parser before retaining them.
            var probe = element.Owner.CreateElement("span");
            probe.GetStyle().SetWidth(Regex.IsMatch(attribute, @"^\d+(?:\.\d+)?$") ? attribute + "px" : attribute);
            return probe.GetStyle().GetWidth();
        }

        private static string FormatTablePercentage(double value) =>
            value.ToString("0.######", CultureInfo.InvariantCulture) + "%";

        private static bool TryGetColumnWidth(string width, out double value, out string unit)
        {
            var match = Regex.Match(width ?? "", @"^(\d+(?:\.\d+)?|\.\d+)(px|pt|pc|in|cm|mm|%)$", RegexOptions.IgnoreCase);
            unit = match.Groups[2].Value.ToLowerInvariant();
            if (!double.TryParse(match.Groups[1].Value, NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out value) || double.IsInfinity(value))
            {
                return false;
            }

            switch (unit)
            {
                case "pt": value *= 96.0 / 72; break;
                case "pc": value *= 16; break;
                case "in": value *= 96; break;
                case "cm": value *= 96 / 2.54; break;
                case "mm": value *= 96 / 25.4; break;
            }
            if (unit != "%")
            {
                unit = "px";
            }
            return !double.IsInfinity(value);
        }

        private static double[] AddTableColumns(IElement source, Table table, IElement target, IHtmlDocument document)
        {
            var widths = new string[table.Columns];
            int column = 0;
            foreach (var group in source.Children.Where(element => element.TagName == "COLGROUP"))
            {
                var columns = group.Children.Where(element => element.TagName == "COL").ToArray();
                foreach (var definition in columns.Length == 0 ? new[] { group } : columns)
                {
                    var width = GetTableWidth(definition);
                    if (string.IsNullOrEmpty(width))
                    {
                        width = GetTableWidth(group);
                    }
                    int span = ReadTableSpan(definition.GetAttribute("span"), 1000, false);
                    for (int i = 0; i < span && column < widths.Length; i++, column++)
                    {
                        widths[column] = width;
                    }
                }
            }

            // Resolve individual columns before distributing a merged cell's width over unresolved columns.
            foreach (var cell in table.Rows.SelectMany(row => row).OrderBy(cell => cell.ColumnSpan))
            {
                var unresolved = Enumerable.Range(cell.Column, cell.ColumnSpan)
                    .Where(index => !IsConstrainedWidth(widths[index])).ToArray();
                if (unresolved.Length == 0 ||
                    !TryGetColumnWidth(GetTableWidth(cell.Element), out double remaining, out string unit))
                {
                    continue;
                }
                bool compatible = true;
                foreach (var index in Enumerable.Range(cell.Column, cell.ColumnSpan).Except(unresolved))
                {
                    if (!TryGetColumnWidth(widths[index], out double known, out string knownUnit) || unit != knownUnit)
                    {
                        compatible = false;
                        break;
                    }
                    remaining -= known;
                }
                if (compatible && remaining > 0)
                {
                    foreach (var index in unresolved)
                    {
                        widths[index] = (remaining / unresolved.Length).ToString("0.######", CultureInfo.InvariantCulture) + unit;
                    }
                }
            }

            var values = new double?[widths.Length];
            var units = new string[widths.Length];
            for (int i = 0; i < widths.Length; i++)
            {
                if (TryGetColumnWidth(widths[i], out double value, out string unit))
                {
                    values[i] = value;
                    units[i] = unit;
                }
            }
            if (!values.Any(value => value.HasValue))
            {
                return null;
            }

            int unspecified = values.Count(value => !value.HasValue);
            double absoluteTotal = values.Where((value, index) => units[index] == "px").Sum(value => value ?? 0);
            double percentageTotal = values.Where((value, index) => units[index] == "%").Sum(value => value ?? 0);
            if (!TryGetColumnWidth(GetTableWidth(source), out double total, out string totalUnit) || totalUnit != "px" || total <= 0)
            {
                int absoluteCount = units.Count(unit => unit == "px");
                double inferredAbsoluteTotal = absoluteTotal +
                    (absoluteCount > 0 ? absoluteTotal / absoluteCount * unspecified : 0);
                total = inferredAbsoluteTotal > 0
                    ? inferredAbsoluteTotal / (percentageTotal < 100 ? 1 - percentageTotal / 100 : 1)
                    : 100;
            }
            for (int i = 0; i < values.Length; i++)
            {
                if (units[i] == "%")
                {
                    values[i] = values[i] * total / 100;
                }
            }
            double knownTotal = values.Sum(value => value ?? 0);
            double availableWidth = unspecified > 0 ? Math.Max(0, total - knownTotal) / unspecified : 0;
            var weights = values.Select(value => value ?? (availableWidth > 0 ? availableWidth : total / values.Length)).ToArray();
            double weightTotal = weights.Sum();
            if (weightTotal <= 0 || double.IsInfinity(weightTotal))
            {
                return null;
            }
            var percentages = weights.Select(value => value / weightTotal * 100).ToArray();
            var colgroup = document.CreateElement("colgroup");
            foreach (var percentage in percentages)
            {
                var col = document.CreateElement("col");
                col.GetStyle().SetWidth(FormatTablePercentage(percentage));
                colgroup.AppendChild(col);
            }
            target.AppendChild(colgroup);
            return percentages;
        }

        private static int ReadTableSpan(string text, int maximum, bool zeroMeansRemaining)
        {
            var match = Regex.Match(text ?? "", @"^[ \t\r\n\f]*\+?([0-9]+)");
            if (!match.Success)
            {
                return 1;
            }

            int value = 0;
            foreach (char digit in match.Groups[1].Value)
            {
                value = Math.Min(maximum, value * 10 + digit - '0');
                if (value == maximum)
                {
                    return maximum;
                }
            }
            return value == 0 ? (zeroMeansRemaining ? maximum : 1) : value;
        }

        private static bool HasExplicitWidth(IElement element)
        {
            string widthAttribute = element.GetAttribute("width");
            var style = element.GetStyle();
            return IsConstrainedWidth(widthAttribute) || style != null &&
                new[] { "width", "min-width", "max-width" }
                    .Any(property => IsConstrainedWidth(style.GetPropertyValue(property)));
        }

        private static bool IsConstrainedWidth(string width) =>
            !string.IsNullOrWhiteSpace(width) && !width.Trim().Equals("auto", StringComparison.OrdinalIgnoreCase);

        private static bool IsStrikeThrough(IElement element)
        {
            return HasTextDecoration(element, "line-through");
        }

        private static bool IsUnderline(IElement element)
        {
            return HasTextDecoration(element, "underline");
        }

        private static bool HasTextDecoration(IElement element, string decoration)
        {
            var lines = element.GetStyle()?.GetPropertyValue("text-decoration-line");
            return !string.IsNullOrEmpty(lines) &&
                lines.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Contains(decoration, StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Resolves cell positions and spans within each source row group, without allocating a rectangular matrix.
        /// </summary>
        /// <param name="table">Table to normalize</param>
        /// <returns>Normalized table</returns>
        private Table NormalizeTable(IElement table)
        {
            var normalizedTable = new Table();
            var rows = ((IHtmlTableElement)table).Rows;
            foreach (var group in rows.GroupBy(row => row.ParentElement))
            {
                var groupRows = group.ToArray();
                var occupiedUntil = new Dictionary<int, int>();
                for (int rowIndex = 0; rowIndex < groupRows.Length; rowIndex++)
                {
                    var row = new List<TableCell>();
                    int column = 0;
                    foreach (var element in groupRows[rowIndex].Cells)
                    {
                        while (occupiedUntil.TryGetValue(column, out int endRow) && endRow > rowIndex)
                        {
                            column++;
                        }
                        int columnSpan = ReadTableSpan(element.GetAttribute("colspan"), 1000, false);
                        int rowSpan = ReadTableSpan(element.GetAttribute("rowspan"), Math.Min(65534, groupRows.Length - rowIndex), true);
                        for (int offset = 1; offset < columnSpan; offset++)
                        {
                            if (occupiedUntil.TryGetValue(column + offset, out int endRow) && endRow > rowIndex)
                            {
                                LogWarning(LogStrings.Warning_OverlappingTableSpans, LogStrings.Heading_BuiltInFunctions);
                                columnSpan = offset;
                                break;
                            }
                        }
                        row.Add(new TableCell { Element = element, Column = column, ColumnSpan = columnSpan, RowSpan = rowSpan });
                        for (int offset = 0; offset < columnSpan; offset++)
                        {
                            occupiedUntil[column + offset] = rowIndex + rowSpan;
                        }
                        column += columnSpan;
                        normalizedTable.Columns = Math.Max(normalizedTable.Columns, column);
                    }
                    normalizedTable.Rows.Add(row);
                }
            }
            var firstRow = normalizedTable.Rows.FirstOrDefault();
            normalizedTable.ClassName = GetTableClassName(table,
                firstRow != null && firstRow.Count > 0 && firstRow.All(cell => cell.Element.TagName == "TH"));

            return normalizedTable;
        }

        #endregion
    }
}
