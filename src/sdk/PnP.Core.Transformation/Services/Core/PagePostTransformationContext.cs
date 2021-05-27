using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Object containing information about a page that has been transformed
    /// </summary>
    public class PagePostTransformationContext : PageTransformationContext
    {
        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="task"></param>
        /// <param name="sourceItem"></param>
        /// <param name="targetPageUri"></param>
        public PagePostTransformationContext(PageTransformationTask task, ISourceItem sourceItem, Uri targetPageUri) : base(task, sourceItem, targetPageUri)
        {
        }

        // TODO: add transformation result properties
    }
}
