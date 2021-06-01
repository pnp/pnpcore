using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;

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
        /// Starts the Transformation Process
        /// </summary>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <param name="token">The cancellation token</param>
        public abstract Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default);

        /// <summary>
        ///     Stops the Transformation Process
        /// </summary>
        public abstract Task StopProcessAsync(CancellationToken token = default);

        /// <summary>
        ///     Allows monitoring the progress of the Transformation Process
        /// </summary>
        public virtual Func<TransformationProcessStatus, Task> Progress { get; set; }

        /// <summary>
        /// Allows monitoring the progress of each tasks
        /// </summary>
        public Func<TransformationProcessTaskStatus, Task> TasksProgress { get; set; }

        /// <summary>
        ///     Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public abstract Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default);

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public abstract Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default);

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public abstract IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, CancellationToken token = default);

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

        /// <summary>
        ///     Raises the tasks progress if property is set
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected virtual Task RaiseTasksProgressAsync(TransformationProcessTaskStatus status)
        {
            if (TasksProgress != null)
            {
                return TasksProgress(status);
            }

            return Task.CompletedTask;
        }
    }
}