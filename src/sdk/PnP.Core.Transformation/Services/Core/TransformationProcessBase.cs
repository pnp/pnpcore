using System;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Base abstract implementation of <see cref="ITransformationProcess"/>
    /// </summary>
    public abstract class TransformationProcessBase : ITransformationProcess
    {
        /// <summary>
        ///     The unique ID of a Transformation Process
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        ///     Starts the Transformation Process
        /// </summary>
        public abstract Task StartProcessAsync(CancellationToken token = default);

        /// <summary>
        ///     Stops the Transformation Process
        /// </summary>
        public abstract Task StopProcessAsync(CancellationToken token = default);

        /// <summary>
        ///     Allows monitoring the progress of the Transformation Process
        /// </summary>
        public virtual Func<TransformationProcessStatus, Task> Progress { get; set; }

        /// <summary>
        ///     Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public abstract Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default);

        /// <summary>
        ///     Raises the progress if property is set
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected virtual Task RaiseProgressAsync(TransformationProcessStatus status)
        {
            if (Progress != null)
            {
                return Progress(status);
            }

            return Task.CompletedTask;
        }
    }
}