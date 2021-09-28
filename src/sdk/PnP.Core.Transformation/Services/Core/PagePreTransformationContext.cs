using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Object containing information about a page to be transformed
    /// </summary>
    public class PagePreTransformationContext : PageTransformationContext
    {
        /// <summary>
        /// Creates an instance of PagePreTransformationContext
        /// </summary>
        /// <param name="task">The page transformation task</param>
        /// <param name="sourceItem">The source item of the transformation</param>
        /// <param name="targetPageUri">The target URI of the transformed page</param>
        public PagePreTransformationContext(PageTransformationTask task, ISourceItem sourceItem, Uri targetPageUri) : base(task, sourceItem, targetPageUri)
        {
        }

        // NOTE: Still under construction
        // TODO: add source properties
    }
}
