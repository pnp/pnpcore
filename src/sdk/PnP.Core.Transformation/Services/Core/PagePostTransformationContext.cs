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
        /// Creates an instance of PagePostTransformationContext
        /// </summary>
        /// <param name="task">The page transformation task</param>
        /// <param name="sourceItem">The source item of the transformation</param>
        /// <param name="targetPageUri">The target URI of the transformed page</param>
        public PagePostTransformationContext(PageTransformationTask task, ISourceItem sourceItem, Uri targetPageUri) : base(task, sourceItem, targetPageUri)
        {
        }

        // TODO: add transformation result properties
    }
}
