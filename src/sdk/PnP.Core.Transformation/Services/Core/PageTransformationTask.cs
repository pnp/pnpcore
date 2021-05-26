using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Defines a page transformation task
    /// </summary>
    public class PageTransformationTask
    {
        /// <summary>
        /// Creates an instance of the task
        /// </summary>
        /// <param name="sourceItem"></param>
        /// <param name="targetContext"></param>
        /// <param name="targetPageUri"></param>
        public PageTransformationTask(ISourceItem sourceItem, PnPContext targetContext, Uri targetPageUri)
        {
            SourceItem = sourceItem ?? throw new ArgumentNullException(nameof(sourceItem));
            TargetContext = targetContext ?? throw new ArgumentNullException(nameof(targetContext));
            TargetPageUri = targetPageUri ?? throw new ArgumentNullException(nameof(targetPageUri));
        }

        /// <summary>
        /// The Unique ID of a Transformation Task
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the source item to process
        /// </summary>
        public ISourceItem SourceItem { get; }

        /// <summary>
        /// The target PnPContext
        /// </summary>
        public PnPContext TargetContext { get; }

        /// <summary>
        /// Relative URL of the source page to transform
        /// </summary>
        public Uri TargetPageUri { get; }

    }
}
