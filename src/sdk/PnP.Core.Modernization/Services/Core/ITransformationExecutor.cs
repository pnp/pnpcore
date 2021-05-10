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
        public Action<TransformationExecutionStatus> Progress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of a transformation process
        /// </summary>
        /// <param name="processId">The ID of the transformation process</param>
        /// <returns>The status of a transformation process</returns>
        public Task<TransformationExecutionStatus> GetStatusAsync(Guid processId);

        /// <summary>
        /// Starts a Page Transformation process
        /// </summary>
        /// <param name="SourceContext">The PnPContext of the source site</param>
        /// <param name="TargetContext">The PnPContext of the target site</param>
        /// <returns>The ID of the transformation process</returns>
        Task<Guid> StartTransformAsync(PnPContext SourceContext, PnPContext TargetContext);

        /// <summary>
        /// Stops a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the transformation process</param>
        /// <returns></returns>
        Task StopTransformAsync(Guid processId);
    }
}
