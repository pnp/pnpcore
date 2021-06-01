using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// In memory implementation of <see cref="ITransformationStateManager"/>
    /// <remarks>Class is thread safety</remarks>
    /// </summary>
    public class InMemoryTransformationStateManager : ITransformationStateManager
    {
        private readonly ConcurrentDictionary<Guid, TransformationProcessTaskStatus> taskStatuses =
            new ConcurrentDictionary<Guid, TransformationProcessTaskStatus>();

        private readonly ConcurrentDictionary<Guid, TransformationProcessStatus> processStatuses =
            new ConcurrentDictionary<Guid, TransformationProcessStatus>();


        /// <summary>
        /// Allows to write the process status
        /// </summary>
        /// <param name="status">The status to write</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public Task WriteProcessStatusAsync(TransformationProcessStatus status, CancellationToken token = default)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            processStatuses.AddOrUpdate(status.ProcessId, status, (k, o) => status);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows to write the task status
        /// </summary>
        /// <param name="status">The status to write</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public Task WriteTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token = default)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            taskStatuses.AddOrUpdate(status.Id, status, (k, o) => status);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the list of tasks using the query specified
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="query"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<TransformationProcessTaskStatus> GetProcessTasksStatus(Guid processId, TasksStatusQuery query, CancellationToken token = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            foreach (var pair in taskStatuses)
            {
                if (!query.State.HasValue || pair.Value.State == query.State.Value)
                {
                    yield return pair.Value;
                }
            }
        }

        /// <summary>
        /// Allows to read the process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        public Task<TransformationProcessStatus> ReadProcessStatusAsync(Guid processId, CancellationToken token = default)
        {
            processStatuses.TryGetValue(processId, out var result);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Allows to read the task status by process and task ids
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The value of the state variable</returns>
        public Task<TransformationProcessTaskStatus> ReadTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default)
        {
            taskStatuses.TryGetValue(taskId, out var result);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Allows to remove a process status by id
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="token">The cancellation token</param>
        public Task<bool> RemoveProcessStatusAsync(Guid processId, CancellationToken token = default)
        {
            bool result = taskStatuses.TryRemove(processId, out _);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Allows to remove a task status by process and task ids
        /// </summary>
        /// <param name="processId">The process id</param>
        /// <param name="taskId">The task id</param>
        /// <param name="token">The cancellation token</param>
        public Task<bool> RemoveTaskStatusAsync(Guid processId, Guid taskId, CancellationToken token = default)
        {
            bool result = processStatuses.TryRemove(taskId, out _);

            return Task.FromResult(result);
        }
    }
}
