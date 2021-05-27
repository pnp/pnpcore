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
        /// <param name="sourceItem"></param>
        /// <param name="targetPageUri"></param>
        public PageTransformationContext(PageTransformationTask task, ISourceItem sourceItem, Uri targetPageUri)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            SourceItem = sourceItem ?? throw new ArgumentNullException(nameof(sourceItem));
            TargetPageUri = targetPageUri ?? throw new ArgumentNullException(nameof(targetPageUri));
        }

        /// <summary>
        /// Gets the task of the current transformation
        /// </summary>
        public PageTransformationTask Task { get; }

        /// <summary>
        /// Gets the source item for the current transformation
        /// </summary>
        public ISourceItem SourceItem { get; }

        /// <summary>
        /// Gets the target page uri for current transformation
        /// </summary>
        public Uri TargetPageUri { get; }
    }
}
