using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Defines a page tranformation task
    /// </summary>
    public class PageTransformationTask
    {
        /// <summary>
        /// The Unique ID of a Transformation Task
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        #region Context properties

        /// <summary>
        /// The source PnPContext
        /// </summary>
        public PnPContext SourceContext { get; set; }

        /// <summary>
        /// The target PnPContext
        /// </summary>
        public PnPContext TargetContext { get; set; }

        #endregion

        #region Page properties

        /// <summary>
        /// Relative URL of the source page to transform
        /// </summary>
        public Uri SourcePageUri { get; set; }

        /// <summary>
        /// Relative URL of the source page to transform
        /// </summary>
        public Uri TargetPageUri { get; set; }

        #endregion
    }
}
