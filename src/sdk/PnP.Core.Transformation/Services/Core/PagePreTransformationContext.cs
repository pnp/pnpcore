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
        /// Creates an instance
        /// </summary>
        /// <param name="task"></param>
        public PagePreTransformationContext(PageTransformationTask task) : base(task)
        {
        }

        // TODO: add source properties
    }
}
