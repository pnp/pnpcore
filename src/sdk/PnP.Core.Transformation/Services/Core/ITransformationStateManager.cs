using System;
using System.Collections.Generic;
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
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        Task WriteProcessStatusAsync(TransformationProcessStatus status, CancellationToken token = default);

        /// <summary>
        /// Allows to write the task status
        /// </summary>
        /// <param name="status">The status to write</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        Task WriteTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token = default);

        /// <summary>
        /// Returns the list of tasks using the query specified
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="query">A custom query to apply</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>A list of task statuses</returns>
        IAsyncEnumerable<TransformationProcessTaskStatus> GetProcessTasksStatus(Guid processId, TasksStatusQuery query, CancellationToken token = default);

        /// <summary>
        /// Allows to read the process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The value of the state variable</returns>
        Task<TransformationProcessStatus> ReadProcessStatusAsync(Guid processId, CancellationToken token = default);

        /// <summary>
        /// Allows to read the task status by process and task ids
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The value of the status variable for the specified task</returns>
        Task<TransformationProcessTaskStatus> ReadTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token, if any</param>
        Task<bool> RemoveProcessStatusAsync(Guid processId, CancellationToken token = default);

        /// <summary>
        /// Allows to remove a task status by process and task ids
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token, if any</param>
        Task<bool> RemoveTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default);
    }
}
