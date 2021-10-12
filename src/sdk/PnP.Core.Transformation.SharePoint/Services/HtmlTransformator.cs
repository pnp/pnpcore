using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using AngleSharp.Io;
using AngleSharp.Html.Dom;
using System.Linq;
using AngleSharp.Css.Dom;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.SharePoint.Services
{
    public class HtmlTransformator
    {
        private ILogger<HtmlTransformator> logger;

        #region Internal table classes
        internal class Table
        {
            internal string ClassName { get; set; }
            internal IElement InsertionPoint { get; set; }
            internal IElementCell[] Header { get; set; }
            internal IElementCell[,] Cells { get; set; }

            internal bool HasHeader
            {
                get
                {
                    if (this.Header == null)
                    {
                        return false;
                    }

                    for (int colPos = 0; colPos < this.Header.Length; colPos += 1)
                    {
                        if (this.Header[colPos] != null)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        internal class IElementCell
        {
            internal bool HasValue
            {
                get
                {
                    return Element != null;
                }
            }
            internal IElement Element { get; set; }

            internal IElementCell(IElement element)
            {
                this.Element = element;
            }

            internal string TextAlign { get; set; }
        }
        #endregion

        private const int DefaultTableWidth = 800;
        private HtmlParser parser;

        #region Construction

        /// <summary>
        /// HtmlTransformator class constructor
        /// </summary>
        public HtmlTransformator(ILogger<HtmlTransformator> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        public string Transform(string text, bool usePlaceHolder)
        {
            // Strip out the "zero width space characters"
            text = text.Replace("\u200B", string.Empty);
            text = text.Replace("\u200b", string.Empty);

            using (var document = this.parser.ParseDocument(text))
            {
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
            foreach (var element in document.All.Where(p => p.HasAttribute("style")))
            {

                if (string.IsNullOrEmpty(element.GetAttribute("style")))
                {
                    // If the style attribute was empty then drop it
                    element.RemoveAttribute("style");
                }
                else
                {
                    if (IsBlockElement(element))
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
                }
            }
        }

        protected virtual void TransformTables(IHtmlCollection<IElement> tables, IHtmlDocument document)
        {
            List<Tuple<IElement, IElement, IElement>> tableReplaceList = new List<Tuple<IElement, IElement, IElement>>();

            foreach (var table in tables)
            {
                Table normalizedTable = null;

                try
                {
                    // Normalize table by removing the col and row spans and returning a Table object containing matrix of cells, header cells and table information
                    normalizedTable = NormalizeTable(table);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        SharePointTransformationResources.Warning_TableCouldNotBeNormalized,
                        ex.Message);
                }

                // If we could not normalize this table then let's skip it
                if (normalizedTable == null)
                {
                    continue;
                }

                // Skip empty tables
                if (normalizedTable.Cells.GetLength(0) == 0 && normalizedTable.Cells.GetLength(1) == 0)
                {
                    continue;
                }

                // <div class="canvasRteResponsiveTable">
                var newTableElement = document.CreateElement($"div");
                newTableElement.ClassName = "canvasRteResponsiveTable";

                // <div class="tableCenterAlign tableWrapper">
                var innerDiv = document.CreateElement("div");
                // Possible alignments: tableLeftAlign, tableCenterAlign and tableRightAlign, since wiki does not have this option default to left align
                innerDiv.ClassList.Add(new string[] { "tableLeftAlign", "tableWrapper" });
                newTableElement.AppendChild(innerDiv);

                // <table class="bandedRowTableStyleNeutral" title="Table">
                var tableElement = document.CreateElement("table");
                //ms-rteTable-default: basic grid lines
                string tableClassName = "borderHeaderTableStyleNeutral";
                if (!string.IsNullOrEmpty(normalizedTable.ClassName))
                {
                    if (normalizedTable.ClassName.Equals("ms-rteTable-default", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tableClassName = "borderHeaderTableStyleNeutral";
                    }
                    else
                    {
                        if (int.TryParse(normalizedTable.ClassName.ToLower().Replace("ms-rtetable-", ""), out int tableStyleCode))
                        {
                            tableClassName = TableStyleCodeToName(tableStyleCode);
                        }
                    }
                }

                tableElement.ClassName = tableClassName;
                tableElement.SetAttribute("title", "Table");
                innerDiv.AppendChild(tableElement);

                // <tbody>
                var tableBody = document.CreateElement("tbody");
                tableElement.AppendChild(tableBody);

                // Table cell width
                var cellWidth = GetDefaultCellTableCellWidths(normalizedTable.Cells.GetLength(1));

                // Table headers
                if (normalizedTable.HasHeader)
                {
                    // Header is transformed into  a row with bold formatted headers
                    var newRow = document.CreateElement("tr");
                    int headerCounter = 0;

                    for (int colPos = 0; colPos < normalizedTable.Header.Length; colPos += 1)
                    {
                        var tableHeaderValue = document.CreateElement("strong");
                        if (normalizedTable.Header[colPos].HasValue)
                        {
                            tableHeaderValue.TextContent = normalizedTable.Header[colPos].Element.TextContent;
                        }

                        var tableHeaderCell = document.CreateElement("td");
                        tableHeaderCell.GetStyle().SetWidth($"{cellWidth[headerCounter]}px");

                        // Restore alignment if needed
                        if (normalizedTable.Header[colPos].TextAlign != null)
                        {
                            tableHeaderCell.GetStyle().SetTextAlign(normalizedTable.Header[colPos].TextAlign);
                        }

                        headerCounter++;

                        tableHeaderCell.AppendChild(tableHeaderValue);
                        newRow.AppendChild(tableHeaderCell);
                    }

                    // Append the new header as table row
                    tableBody.AppendChild(newRow);
                }

                // Iterate the table rows
                for (int rowPos = 0; rowPos < normalizedTable.Cells.GetLength(0); rowPos += 1)
                {
                    var newRow = document.CreateElement("tr");
                    int cellCounter = 0;

                    for (int colPos = 0; colPos < normalizedTable.Cells.GetLength(1); colPos += 1)
                    {
                        var newTableCell = document.CreateElement("td");
                        newTableCell.GetStyle().SetWidth($"{cellWidth[cellCounter]}px");
                        cellCounter++;

                        // Did we put a "real" cell?
                        if (normalizedTable.Cells[rowPos, colPos].HasValue)
                        {
                            // Copy over the content, take over html content as cell can have formatting inside
                            // Formatting of the table cell content was already done in previous steps, so we simply copy what we have
                            newTableCell.InnerHtml = normalizedTable.Cells[rowPos, colPos].Element.InnerHtml;
                        }

                        // Restore alignment if needed
                        if (normalizedTable.Cells[rowPos, colPos].TextAlign != null)
                        {
                            newTableCell.GetStyle().SetTextAlign(normalizedTable.Cells[rowPos, colPos].TextAlign);
                        }

                        newRow.AppendChild(newTableCell);
                    }

                    // Append the new row
                    tableBody.AppendChild(newRow);
                }

                // Add table to list for doing actual replacements once we're done analyzing all tables
                tableReplaceList.Add(new Tuple<IElement, IElement, IElement>(table, newTableElement, normalizedTable.InsertionPoint));
            }

            foreach (var tableToReplace in tableReplaceList.Where(p => p.Item3 != null))
            {
                // Insert the new table at the insertion point. Add a br to work around an RTE bug that you can't separate tables
                tableToReplace.Item3.AppendChild(document.CreateElement("br"));
                tableToReplace.Item3.AppendChild(tableToReplace.Item2);

                // Remove the old table
                tableToReplace.Item1.Parent.RemoveChild(tableToReplace.Item1);
            }

            foreach (var tableToReplace in tableReplaceList.Where(p => p.Item3 == null))
            {
                // Swap old table with new table
                tableToReplace.Item1.Parent.ReplaceChild(tableToReplace.Item2, tableToReplace.Item1);
            }

            // Drop any nested table
            foreach (var tableToCheck in tableReplaceList)
            {
                var nestedTables = tableToCheck.Item2.QuerySelectorAll("table");
                if (nestedTables.Any())
                {
                    foreach (var nestedTable in nestedTables.ToList())
                    {
                        // the new Table element has a DIV as top node, an inner DIV and then the table as child
                        if (tableToCheck.Item2.FirstElementChild != null && tableToCheck.Item2.FirstElementChild.FirstElementChild != null)
                        {
                            if (nestedTable != tableToCheck.Item2.FirstElementChild.FirstElementChild)
                            {
                                nestedTable.ParentElement.RemoveChild(nestedTable);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void ImageIFramePlaceHolders(IHtmlDocument document)
        {
            var images = document.QuerySelectorAll("img");
            var iframes = document.QuerySelectorAll("iframe");
            var elements = images.Union(iframes);

            foreach (var element in elements)
            {
                // Add a text content in place of the element
                string webPartType = string.Empty;
                string sourceValue = string.Empty;
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
                if (blockQuote.OuterHtml.ToLower().Contains("margin:0px 0px 0px 40px"))
                {
                    if ((blockQuote.ChildElementCount > 0 && blockQuote.Children[0].TagName.ToLower() == "blockquote"))
                    {
                        // do nothing if we still see a blockquote as child element
                    }
                    else
                    {
                        // We're at the bottom level, the children of this blockquote node do contain regular content. This is always wrapped inside either P or Div tags
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

                        // Important to do a ToList() to get a copy as the Children list might be affected by the code in the loop!
                        foreach (var nodeToProcess in blockQuote.Children.ToList())
                        {
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

            foreach (var brNode in brNodes)
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
            foreach (var fromNode in fromNodes)
            {
                var parent = fromNode.Parent;

                if (to == 0)
                {
                    // wrap the content inside a div so that further formatting processing will pick it up
                    var newElement = document.CreateElement("div");
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
            foreach (var element in elementsToTransform)
            {
                var parent = element.Parent;

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
                        rtestyle.Equals("IntenseReferences", StringComparison.InvariantCultureIgnoreCase))
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
                        rtestyle.Equals("IntenseReferences", StringComparison.InvariantCultureIgnoreCase))
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
                    string newClass = null;

                    // Modern Theme colors
                    // Darker, Dark, Dark Alternate, Primary, Secondary
                    // Neutral Tertiary, Neutral Secondary, Primary alternate, Neutral primary, Neutral Dark
                    if (int.TryParse(themeForeColor.ToLower()[themeForeColor.ToLower().Length - 1].ToString(), out int themeCode))
                    {
                        string colorName = ThemeCodeToForegroundColorName(themeCode);
                        if (!string.IsNullOrEmpty(colorName))
                        {
                            newClass = $"fontColor{colorName}";
                        }
                    }

                    element.ClassList.Remove(themeForeColor);
                    if (!string.IsNullOrEmpty(newClass))
                    {
                        // We mapped a color
                        element.ClassList.Add(newClass);
                    }
                }

                // <span class="ms-rteThemeBackColor-5-0">red</span>
                var rtethemebackcolor = element.ClassList.PartialMatch("ms-rtethemebackcolor-");
                if (!string.IsNullOrEmpty(rtethemebackcolor))
                {
                    // There are no themed back colors in modern, so for now drop the color span and the background color
                    element.ClassList.Remove(rtethemebackcolor);
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
                        }
                    }

                    element.ClassList.Remove(rteforecolor);
                    if (!string.IsNullOrEmpty(newClass))
                    {
                        // We mapped a color
                        element.ClassList.Add(newClass);
                    }
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
                string elementToKeep = string.Empty;
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
                        newElement.ClassList.Add(element.ClassList.ToArray());
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
                        // if we still did not replace the span element, the span has no classes set anymore and the span does not have child elements then we can replace it by text
                        if (!replacementDone && element.ClassList.Length == 0 && element.FirstElementChild == null)
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
                        emElement.InnerHtml = element.InnerHtml;
                        strikeThroughElement.AppendChild(emElement);
                    }
                    else if (isUnderlineOnElementToKeep)
                    {
                        var underlineElement = document.CreateElement("u");
                        newElement.AppendChild(underlineElement);
                        // Create the element that held the underline style
                        var emElement = document.CreateElement(elementToKeep.ToLower());
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

        /// <summary>
        /// Map wiki table style to a RTE compatible style
        /// </summary>
        /// <param name="tableStyleCode">Code used for the wiki table style</param>
        /// <returns>RTE compatible table style</returns>
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

            // Transform the piece of html we add in this span, needed to handle spans nested in here as these otherwise would not be found anymore
            string innerHtml = TransformInnerHtml(element.InnerHtml);

            // If the element has a class defined then wrap inside a span
            if (element.ClassList.Length > 0)
            {
                var newSpan = document.CreateElement("span");
                newSpan.ClassList.Add(element.ClassList.ToArray());
                newSpan.InnerHtml = innerHtml;
                newElement.AppendChild(newSpan);
            }
            else
            {
                newElement.InnerHtml = innerHtml;
            }

            parent.ReplaceChild(newElement, element);
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
                tag == "blockquote")
            {
                return true;
            }

            return false;
        }

        private static List<int> GetDefaultCellTableCellWidths(int columns)
        {
            List<int> widths = new List<int>(columns);

            int width = DefaultTableWidth / columns;
            int lastWidth = DefaultTableWidth - ((columns - 1) * width);

            for (int i = 0; i < columns; i++)
            {
                if (i < columns - 1)
                {
                    widths.Add(width);
                }
                else
                {
                    widths.Add(lastWidth);
                }
            }

            return widths;
        }

        private static bool IsStrikeThrough(IElement element)
        {
            if (element.GetStyle() != null && (!string.IsNullOrEmpty(element.GetStyle().GetTextDecoration()) && element.GetStyle().GetTextDecoration().Equals("line-through", StringComparison.InvariantCultureIgnoreCase) ||
                                          !string.IsNullOrEmpty(element.GetStyle().GetPropertyValue("text-decoration-line")) && element.GetStyle().GetPropertyValue("text-decoration-line").Equals("line-through", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsUnderline(IElement element)
        {
            if (element.GetStyle() != null && (!string.IsNullOrEmpty(element.GetStyle().GetTextDecoration()) && element.GetStyle().GetTextDecoration().Equals("underline", StringComparison.InvariantCultureIgnoreCase) ||
                                          !string.IsNullOrEmpty(element.GetStyle().GetPropertyValue("text-decoration-line")) && element.GetStyle().GetPropertyValue("text-decoration-line").Equals("underline", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Returns the table as a normalized table object. Includes replacing row and col spans by actual empty cells
        /// </summary>
        /// <param name="table">Table to normalize</param>
        /// <returns>Normalized table</returns>
        private Table NormalizeTable(IElement table)
        {
            // Determine the table dimension, item1 = max cols / item 2 = max rows
            var tableDimension = GetTableDimensions(table);

            // Create table object
            Table normalizedTable = new Table()
            {
                ClassName = table.ClassName,
            };

            // A 2 dimensional array to hold the normalized table cells
            IElementCell[,] cells = new IElementCell[tableDimension.Item2, tableDimension.Item1];
            // An array to hold the header cells (if any)
            IElementCell[] headerCells = new IElementCell[tableDimension.Item1];

            var tableBodyElement = (table as IHtmlTableElement).Bodies[0];
            var rows = tableBodyElement.Children.Where(p => p.TagName.Equals("tr", StringComparison.InvariantCultureIgnoreCase));
            if (rows != null && rows.Any())
            {
                int rowPos = 0;
                foreach (var row in rows)
                {
                    // We assume a table header only has TH cells
                    var tableHeaders = row.Children.Where(p => p.TagName.Equals("th", StringComparison.InvariantCultureIgnoreCase));
                    var cellsInTableHeader = row.Children.Where(p => p.TagName.Equals("td", StringComparison.InvariantCultureIgnoreCase));
                    if (tableHeaders != null && tableHeaders.Any() && !cellsInTableHeader.Any())
                    {
                        int colPos = 0;
                        foreach (var tableHeader in tableHeaders)
                        {
                            headerCells[colPos] = new IElementCell(tableHeader);

                            //Check header styles
                            if (tableHeader.GetStyle() != null && !string.IsNullOrEmpty(tableHeader.GetStyle().GetTextAlign()))
                            {
                                headerCells[colPos].TextAlign = tableHeader.GetStyle().GetTextAlign();
                            }

                            // Prep for next cell
                            colPos++;

                            var colSpan = tableHeader.GetAttribute("colspan");
                            if (!string.IsNullOrEmpty(colSpan) && colSpan != "1" && int.TryParse(colSpan, out int columnCellsToAdd))
                            {
                                for (int i = 0; i < columnCellsToAdd - 1; i++)
                                {
                                    headerCells[colPos] = new IElementCell(null);
                                    colPos++;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Support model of TD cells mixed with TH
                        var tableCells = row.Children.Where(p => p.TagName.Equals("td", StringComparison.InvariantCultureIgnoreCase) || p.TagName.Equals("th", StringComparison.InvariantCultureIgnoreCase));
                        if (tableCells != null && tableCells.Any())
                        {
                            int colPos = 0;
                            foreach (var tableCell in tableCells)
                            {

                                if (cells[rowPos, colPos] != null)
                                {
                                    // cells was already filled due to previous row span expansion. Find next free cell on this row
                                    while ((cells[rowPos, colPos] != null) && colPos < tableDimension.Item1)
                                    {
                                        colPos++;
                                    }
                                }

                                cells[rowPos, colPos] = new IElementCell(tableCell);

                                //Check header styles
                                if (tableCell.GetStyle() != null && !string.IsNullOrEmpty(tableCell.GetStyle().GetTextAlign()))
                                {
                                    cells[rowPos, colPos].TextAlign = tableCell.GetStyle().GetTextAlign();
                                }

                                // Prep for next cell
                                colPos++;

                                var colSpan = tableCell.GetAttribute("colspan");
                                var rowSpan = tableCell.GetAttribute("rowspan");

                                // We do have both col and row span set
                                if (!string.IsNullOrEmpty(colSpan) && colSpan != "1" && int.TryParse(colSpan, out int columnCellsToAdd) &&
                                    !string.IsNullOrEmpty(rowSpan) && rowSpan != "1" && int.TryParse(rowSpan, out int rowCellsToAdd))
                                {
                                    int tempRowPos = rowPos;
                                    for (int j = 0; j < rowCellsToAdd; j++)
                                    {
                                        int tempColPos = colPos - 1;
                                        for (int i = 0; i < columnCellsToAdd; i++)
                                        {
                                            if (cells[tempRowPos, tempColPos] == null)
                                            {
                                                cells[tempRowPos, tempColPos] = new IElementCell(null);
                                            }
                                            tempColPos++;
                                        }
                                        tempRowPos++;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(colSpan) && colSpan != "1" && int.TryParse(colSpan, out int columnCellsToAdd2))
                                    {
                                        for (int i = 0; i < columnCellsToAdd2 - 1; i++)
                                        {
                                            cells[rowPos, colPos] = new IElementCell(null);
                                            colPos++;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(rowSpan) && rowSpan != "1" && int.TryParse(rowSpan, out int rowCellsToAdd2))
                                    {
                                        int tempRowPos = rowPos + 1;
                                        for (int i = 0; i < rowCellsToAdd2 - 1; i++)
                                        {
                                            cells[tempRowPos, colPos - 1] = new IElementCell(null);
                                            tempRowPos++;
                                        }
                                    }
                                }
                            }

                            // Only increase row pos for actual rows as header is handled differently
                            rowPos++;
                        }
                        else
                        {
                            // Deals with rows formatted without TD or TH tags (e.g.  <tr class="ms-rteTableOddRow-default"></tr>)
                            rowPos++;
                        }
                    }
                }
            }

            normalizedTable.Header = headerCells;
            normalizedTable.Cells = cells;

            // Let's assume there's badly shaped tables as well...fill the empty cells with an empty cell value so they're correctly replaced
            for (int rowPos = 0; rowPos < normalizedTable.Cells.GetLength(0); rowPos += 1)
            {
                for (int colPos = 0; colPos < normalizedTable.Cells.GetLength(1); colPos += 1)
                {
                    if (normalizedTable.Cells[rowPos, colPos] == null)
                    {
                        normalizedTable.Cells[rowPos, colPos] = new IElementCell(null);
                    }
                }
            }

            if (normalizedTable.HasHeader)
            {
                for (int colPos = 0; colPos < normalizedTable.Header.GetLength(0); colPos += 1)
                {
                    if (normalizedTable.Header[colPos] == null)
                    {
                        normalizedTable.Header[colPos] = new IElementCell(null);
                    }
                }
            }

            // Determine insertion point
            if (table.ParentElement != null)
            {
                var startNode = table.ParentElement;
                IElement lastTableElement = null;

                // walk up the parent tree
                while (startNode != null)
                {
                    if (startNode.TagName.Equals("table", StringComparison.InvariantCultureIgnoreCase))
                    {
                        lastTableElement = startNode;
                    }
                    startNode = startNode.ParentElement;
                }

                if (lastTableElement != null)
                {
                    // This table was nested, let's take the parent element of the 'highest' table as insertion point
                    normalizedTable.InsertionPoint = lastTableElement.ParentElement;
                }

            }

            return normalizedTable;
        }

        /// <summary>
        /// Gets the dimensions of a table, excluding the header
        /// </summary>
        /// <param name="table">Table to investigate</param>
        /// <returns>Tuple containing the columns and rows</returns>
        private static (int columns, int rows) GetTableDimensions(IElement table)
        {
            int maxCols = 0;
            int maxRows = 0;
            bool hasHeader = false;

            var tableBodyElement = (table as IHtmlTableElement).Bodies[0];
            var rows = tableBodyElement.Children.Where(p => p.TagName.Equals("tr", StringComparison.InvariantCultureIgnoreCase));
            if (rows != null && rows.Any())
            {
                maxRows = rows.Count();
                foreach (var row in rows)
                {
                    // Do we have a header
                    var tableHeaders = row.Children.Where(p => p.TagName.Equals("th", StringComparison.InvariantCultureIgnoreCase));

                    var tdInHeader = row.Children.Where(p => p.TagName.Equals("td", StringComparison.InvariantCultureIgnoreCase));
                    int tdCellsInHeader = 0;
                    if (tdInHeader != null)
                    {
                        tdCellsInHeader = tdInHeader.Count();
                    }

                    if (tableHeaders != null && tableHeaders.Any() && tdCellsInHeader == 0)
                    {
                        hasHeader = true;
                    }

                    // It's allowed to mix TH and TD in a table row
                    var tableCells = row.Children.Where(p => p.TagName.Equals("td", StringComparison.InvariantCultureIgnoreCase) || p.TagName.Equals("th", StringComparison.InvariantCultureIgnoreCase));
                    if (tableCells != null && tableCells.Any())
                    {
                        int colCount = 0;
                        foreach (var tableCell in tableCells)
                        {
                            var colSpan = tableCell.GetAttribute("colspan");
                            if (!string.IsNullOrEmpty(colSpan) && colSpan != "1")
                            {
                                if (int.TryParse(colSpan, out int columnCellsToAdd))
                                {
                                    colCount = colCount + columnCellsToAdd;
                                }
                                else
                                {
                                    colCount++;
                                }
                            }
                            else
                            {
                                colCount++;
                            }
                        }

                        if (colCount > maxCols)
                        {
                            maxCols = colCount;
                        }
                    }
                }
            }

            if (hasHeader)
            {
                // remove one row as header is handled separately
                maxRows--;
            }

            // If there's a TR then there must be one cell
            if (maxRows > 0 && maxCols == 0)
            {
                maxCols = 1;
            }

            return (maxCols, maxRows);
        }
        #endregion
    }
}
