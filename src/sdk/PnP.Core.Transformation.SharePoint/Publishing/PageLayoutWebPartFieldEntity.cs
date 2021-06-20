using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.SharePoint.Publishing
{
    /// <summary>
    /// Class for holding data properties for field to web part mapping
    /// </summary>
    internal class PageLayoutWebPartFieldEntity
    {
        internal string TargetWebPart { get; set; }
        internal string FieldType { get; set; }
        internal string Name { get; set; }
        internal string Type { get; set; }
        internal string Functions { get; set; }
    }
}
