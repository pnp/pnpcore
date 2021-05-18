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
        /// <param name="options"></param>
        /// <param name="task"></param>
        public PagePostTransformationContext(PageTransformationOptions options, PageTransformationTask task) : base(options, task)
        {
        }
    }
}
