using System;
using System.Collections.Generic;
using System.Text;
using AngleSharp.Dom;

namespace PnP.Core.Transformation.SharePoint.MappingProviders.HtmlMapping
{
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
}
