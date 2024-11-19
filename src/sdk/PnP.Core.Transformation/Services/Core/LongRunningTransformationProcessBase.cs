using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Base implementation of <see cref="ITransformationProcess"/> for long running processes
    /// </summary>
    public abstract class LongRunningTransformationProcessBase : TransformationProcessBase
    {
        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="id">The process id</param>
        /// <param name="serviceProvider">The service provider to use to resolve dependencies</param>
        protected LongRunningTransformationProcessBase(
            Guid id,
            IServiceProvider serviceProvider)
        {
            Id = id;
            ServiceProvider = serviceProvider;

            TransformationStateManager = serviceProvider.GetRequiredService<ITransformationStateManager>();
        }


        /// <summary>
        /// The unique ID of a Transformation Process
        /// </summary>
        public override Guid Id { get; }

        /// <summary>
        /// The service provider to use to resolve dependencies
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The state manager used
        /// </summary>
        protected ITransformationStateManager TransformationStateManager { get; }

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        public override async Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default)
        {
            // Check if process is already running
            var status = await GetStatusAsync(token).ConfigureAwait(false);
            if (status.State != TransformationExecutionState.Pending)
            {
                throw new InvalidOperationException("Process cannot be started twice!");
            }

            var transformationDistiller = ServiceProvider.GetRequiredService<ITransformationDistiller>();

            // Change state to running
            await ChangeProcessStatusAsync(TransformationExecutionState.Running, token).ConfigureAwait(false);

            try
            {
                await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token).WithCancellation(token))
                {
                    // Mark task as pending
                    await ChangeTaskStatusAsync(TransformationProcessTaskStatus.CreatePending(Id, task.Id, DateTimeOffset.Now), token)
                        .ConfigureAwait(false);

                    // Queue item
                    await EnqueueTaskAsync(task, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Change state to aborted
                await ChangeProcessStatusAsync(TransformationExecutionState.Aborted, token).ConfigureAwait(false);
                throw;
            }
            catch (Exception)
            {
                // Change state to faulted
                await ChangeProcessStatusAsync(TransformationExecutionState.Faulted, token).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Adds the task to a queue in order to process it asynchronously
        /// </summary>
        /// <param name="task">The task item to enqueue</param>
        /// <param name="token">The cancellation token, if any</param>
        protected abstract Task EnqueueTaskAsync(PageTransformationTask task, CancellationToken token);

        /// <summary>
        /// Stops the Transformation Process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        public override async Task StopProcessAsync(CancellationToken token = default)
        {
            // Change state to aborted
            await ChangeProcessStatusAsync(TransformationExecutionState.Aborted, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The status of the Transformation Process</returns>
        public override async Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default)
        {
            var status = await TransformationStateManager.ReadProcessStatusAsync(Id, token).ConfigureAwait(false);
            status ??= new TransformationProcessStatus(Id, TransformationExecutionState.Pending);

            // If status is running we need to check if there is any item in pending state
            if (status.State == TransformationExecutionState.Running)
            {
                bool hasPending = false;
                await foreach (var task in GetTasksStatusAsync(new TasksStatusQuery(TransformationTaskExecutionState.Pending), token).ConfigureAwait(false))
                {
                    hasPending = true;
                    break;
                }

                // No more tasks in pending state
                if (!hasPending)
                {
                    // Automatically set the state to completed
                    status = await ChangeProcessStatusAsync(TransformationExecutionState.Completed, token).ConfigureAwait(false);
                }
            }

            return status;
        }

        /// <summary>
        /// Initialize the process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        public virtual Task InitAsync(CancellationToken token = default)
        {
            // Save the initial state
            return ChangeProcessStatusAsync(TransformationExecutionState.Pending, token);
        }

        /// <summary>
        /// Process a task and update the status
        /// </summary>
        /// <param name="task">The task item to process</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        public virtual async Task<TransformationProcessTaskStatus> ProcessTaskAsync(PageTransformationTask task,
            CancellationToken token = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            var logger = ServiceProvider.GetRequiredService<ILogger<LongRunningTransformationProcessBase>>();
            var pageTransformator = ServiceProvider.GetRequiredService<IPageTransformator>();

            // Retrieve the status for the task
            var taskStatus = await GetTaskStatusAsync(task.Id, token).ConfigureAwait(false);

            // Check if task has been already processed
            if (taskStatus.State != TransformationTaskExecutionState.Pending)
            {
                // Skip
                return taskStatus;
            }

            // Retrieve current process status
            var status = await GetStatusAsync(token).ConfigureAwait(false);

            // Process is not running, skip the task
            if (status.State != TransformationExecutionState.Running)
            {
                // Mark task as aborted
                taskStatus = TransformationProcessTaskStatus.CreateNormal(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, TransformationTaskExecutionState.Aborted);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);
                return taskStatus;
            }

            try
            {
                // Mark task as running
                taskStatus = TransformationProcessTaskStatus.CreateNormal(Id, task.Id, taskStatus.CreationDate, DateTimeOffset.Now, null, TransformationTaskExecutionState.Running);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);

                // Run the actual transformation task
                await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                // Mark task as completed
                taskStatus = TransformationProcessTaskStatus.CreateNormal(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, TransformationTaskExecutionState.Completed);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);

                return taskStatus;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while transforming task {id}", task.Id);

                // Mark task as faulted
                taskStatus = TransformationProcessTaskStatus.CreateFaulted(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, ex);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);

                return taskStatus;
            }
        }

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">Cancellation token to use, if any</param>
        /// <returns>The process task status</returns>
        public override async Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default)
        {
            var taskStatus = await TransformationStateManager.ReadTaskStatusAsync(Id, id, token).ConfigureAwait(false);
            if (taskStatus != null)
            {
                return taskStatus;
            }

            throw new ArgumentException($"Cannot find task with id {id}", nameof(id));
        }

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">Cancellation token to use, if any</param>
        /// <returns>The process task status</returns>
        public override IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, CancellationToken token = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            return TransformationStateManager.GetProcessTasksStatus(Id, query, token);
        }

        /// <summary>
        /// Sets the state of the process
        /// </summary>
        /// <param name="status">The process status</param>
        /// <param name="token">Cancellation token to use, if any</param>
        /// <returns>The updated process status</returns>
        protected virtual async Task<TransformationProcessStatus> ChangeProcessStatusAsync(TransformationExecutionState status, CancellationToken token)
        {
            // NOTE: This one returns the TransformationProcessStatus, while the next one doesn't (for the task)
            // Evaluate a different and more consistent approach

            var newStatus = new TransformationProcessStatus(Id, status);
            await TransformationStateManager
                .WriteProcessStatusAsync(newStatus, token)
                .ConfigureAwait(false);

            await RaiseProgressAsync(newStatus).ConfigureAwait(false);

            return newStatus;
        }

        /// <summary>
        /// Sets the status of a task
        /// </summary>
        /// <param name="status">The process task status</param>
        /// <param name="token">Cancellation token to use, if any</param>
        protected virtual async Task ChangeTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token)
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            // Save the new state
            await TransformationStateManager
                .WriteTaskStatusAsync( status, token)
                .ConfigureAwait(false);

            await RaiseTasksProgressAsync(status).ConfigureAwait(false);
        }
    }
}