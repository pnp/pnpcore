using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface to handle the state of a transformation process managed by an implementation of ITransformationExecutor
    /// </summary>
    public interface ITransformationStateManager
    {
        /// <summary>
        /// Allows to write the process status
        /// </summary>
        /// <param name="status">The status to write</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task WriteProcessStatusAsync(TransformationProcessStatus status, CancellationToken token = default);

        /// <summary>
        /// Allows to write the task status
        /// </summary>
        /// <param name="status">The status to write</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        Task WriteTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token = default);

        /// <summary>
        /// Returns the list of tasks using the query specified
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="query"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IAsyncEnumerable<TransformationProcessTaskStatus> GetProcessTasksStatus(Guid processId, TasksStatusQuery query, CancellationToken token = default);

        /// <summary>
        /// Allows to read the process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        Task<TransformationProcessStatus> ReadProcessStatusAsync(Guid processId, CancellationToken token = default);

        /// <summary>
        /// Allows to read the task status by process and task ids
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        Task<TransformationProcessTaskStatus> ReadTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token</param>
        Task<bool> RemoveProcessStatusAsync(Guid processId, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a task status by process and task ids
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token</param>
        Task<bool> RemoveTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default);

    }
}
