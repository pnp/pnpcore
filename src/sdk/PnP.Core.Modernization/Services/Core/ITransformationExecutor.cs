using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Abstract interface for a service that manages the transformation of one or more pages
    /// </summary>
    public interface ITransformationExecutor
    {
        /// <summary>
        /// Allows monitoring (and cancel) the progress of a transformation process
        /// </summary>
        public Func<TransformationExecutionStatus, Boolean> Progress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of a transformation process
        /// </summary>
        /// <returns>The status of a transformation process</returns>
        public TransformationExecutionStatus GetStatus();

        /// <summary>
        /// Defines a list of Page Transformation Tasks
        /// </summary>
        /// <param name="SourceContext">The PnPContext of the source site</param>
        /// <param name="TargetContext">The PnPContext of the target site</param>
        /// <returns>A dictionary of URLs of the transformed pages mapped to the URLs of the source pages</returns>
        Task<Dictionary<Uri, Uri>> Transform(PnPContext SourceContext, PnPContext TargetContext);
    }
}
