using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Modernization.Model.Modern
{
    /// <summary>
    /// Defines the page layout of a modern page
    /// </summary>
    public class PageLayout
    {
        /// <summary>
        /// Defines the layout fo the page
        /// </summary>
        public PageLayoutType Layout { get; set; }

        /// <summary>
        /// Defines the format of the Canvas Section
        /// </summary>
        public CanvasSectionTemplate CanvasSection { get; set; }
    }
}
