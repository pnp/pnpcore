using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Represents a Transformation Process
    /// </summary>
    public interface ITransformationProcess
    {
        /// <summary>
        /// The unique ID of a Transformation Process
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <param name="token">The cancellation token</param>
        Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default);

        /// <summary>
        /// Stops the Transformation Process
        /// </summary>
        Task StopProcessAsync(CancellationToken token = default);

        /// <summary>
        /// Allows monitoring the progress of the Transformation Process
        /// </summary>
        Func<TransformationProcessStatus, Task> Progress { get; set; }

        /// <summary>
        /// Allows monitoring the progress of each tasks
        /// </summary>
        Func<TransformationProcessTaskStatus, Task> TasksProgress { get; set; }

        /// <summary>
        /// Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default);

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default);

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, CancellationToken token = default);
    }
}
