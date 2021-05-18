using System;
using System.Collections.Generic;
using System.Text;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Object containing information about transformation of a page
    /// </summary>
    public class PageTransformationContext
    {
        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="task"></param>
        /// <param name="options"></param>
        public PageTransformationContext(PageTransformationTask task, PageTransformationOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>
        /// Gets the options used for the transformation
        /// </summary>
        public PageTransformationOptions Options { get; }

        /// <summary>
        /// Gets the task of the current transformation
        /// </summary>
        public PageTransformationTask Task { get; }
    }
}
