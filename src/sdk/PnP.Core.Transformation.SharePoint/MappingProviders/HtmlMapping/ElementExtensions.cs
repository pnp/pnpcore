using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace PnP.Core.Transformation.SharePoint.MappingProviders.HtmlMapping
{
    /// <summary>
    /// Extensions methods for <see cref="IElement"/>
    /// </summary>
    public static class ElementExtensions
    {
        /// <summary>
        /// Gets if element is a block
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsBlockElement(this IElement element)
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

        /// <summary>
        /// Gets if the element has strike through style
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsStrikeThrough(this IElement element)
        {
            if (element.GetStyle() != null && (!string.IsNullOrEmpty(element.GetStyle().GetTextDecoration()) && element.GetStyle().GetTextDecoration().Equals("line-through", StringComparison.InvariantCultureIgnoreCase) ||
                                               !string.IsNullOrEmpty(element.GetStyle().GetPropertyValue("text-decoration-line")) && element.GetStyle().GetPropertyValue("text-decoration-line").Equals("line-through", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets if element is underline
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsUnderline(this IElement element)
        {
            if (element.GetStyle() != null && (!string.IsNullOrEmpty(element.GetStyle().GetTextDecoration()) && element.GetStyle().GetTextDecoration().Equals("underline", StringComparison.InvariantCultureIgnoreCase) ||
                                               !string.IsNullOrEmpty(element.GetStyle().GetPropertyValue("text-decoration-line")) && element.GetStyle().GetPropertyValue("text-decoration-line").Equals("underline", StringComparison.InvariantCultureIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the table as a normalized table object. Includes replacing row and col spans by actual empty cells
        /// </summary>
        /// <param name="table">Table to normalize</param>
        /// <returns>Normalized table</returns>
        internal static Table NormalizeTable(this IElement table)
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
    }
}
