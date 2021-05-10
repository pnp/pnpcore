using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Base implementation of <see cref="ITransformationExecutor"/>
    /// </summary>
    public abstract class TransformationExecutorBase : ITransformationExecutor
    {
        /// <summary>
        /// Allows monitoring (and cancel) the progress of a transformation process
        /// </summary>
        public Func<TransformationExecutionStatus, Task> Progress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of a transformation process
        /// </summary>
        /// <param name="processId">The ID of the transformation process</param>
        /// <returns>The status of a transformation process</returns>
        public abstract Task<TransformationExecutionStatus> GetStatusAsync(Guid processId);

        /// <summary>
        /// Starts a Page Transformation process
        /// </summary>
        /// <param name="sourceContext">The PnPContext of the source site</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <returns>The ID of the transformation process</returns>
        public abstract Task<Guid> StartTransformAsync(PnPContext sourceContext, PnPContext targetContext);

        /// <summary>
        /// Stops a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the transformation process</param>
        /// <returns></returns>
        public abstract Task StopTransformAsync(Guid processId);

        /// <summary>
        /// Raises the progress if property is set
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected virtual Task RaiseProgressAsync(TransformationExecutionStatus status)
        {
            if (Progress != null)
            {
                return Progress(status);
            }
            return Task.CompletedTask;
        }
    }
}
