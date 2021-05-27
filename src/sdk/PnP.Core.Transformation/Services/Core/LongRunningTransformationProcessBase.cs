using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;

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
        /// <param name="id"></param>
        /// <param name="serviceProvider"></param>
        protected LongRunningTransformationProcessBase(
            Guid id,
            IServiceProvider serviceProvider)
        {
            Id = id;
            ServiceProvider = serviceProvider;

            TransformationStateManager = serviceProvider.GetRequiredService<ITransformationStateManager>();
        }


        /// <summary>
        ///     The unique ID of a Transformation Process
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
        ///     Starts the Transformation Process
        /// </summary>
        public override async Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default)
        {
            // Check if process is not started yet
            var status = await GetStatusAsync(token).ConfigureAwait(false);
            if (status.State != TransformationExecutionState.Pending)
            {
                throw new InvalidOperationException("Process cannot run twice");
            }

            var transformationDistiller = ServiceProvider.GetRequiredService<ITransformationDistiller>();

            // Change state to running
            await ChangeProcessStateAsync(TransformationExecutionState.Running, token).ConfigureAwait(false);

            try
            {
                await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token).WithCancellation(token))
                {
                    // Set task state to pending
                    await ChangeTaskStatusAsync(new TransformationProcessTaskStatus(Id, task.Id, DateTimeOffset.Now), token)
                        .ConfigureAwait(false);

                    // Queue item
                    await QueueTaskAsync(task, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Change state to aborted
                await ChangeProcessStateAsync(TransformationExecutionState.Aborted, token).ConfigureAwait(false);
                throw;
            }
            catch (Exception)
            {
                // Change state to faulted
                await ChangeProcessStateAsync(TransformationExecutionState.Faulted, token).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Adds the task to a queue in order to process it asynchronously
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected abstract Task QueueTaskAsync(PageTransformationTask task, CancellationToken token);

        /// <summary>
        ///     Stops the Transformation Process
        /// </summary>
        public override async Task StopProcessAsync(CancellationToken token = default)
        {
            // Change state to aborted
            await ChangeProcessStateAsync(TransformationExecutionState.Aborted, token).ConfigureAwait(false);
        }

        /// <summary>
        ///     Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public override async Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default)
        {
            var status = await TransformationStateManager.ReadStateAsync<TransformationProcessStatus>(Id, token).ConfigureAwait(false);
            return status ?? new TransformationProcessStatus(Id, TransformationExecutionState.Pending);
        }

        /// <summary>
        /// Initialize the process
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual Task InitAsync(CancellationToken token = default)
        {
            // Save the initial state
            return ChangeProcessStateAsync(TransformationExecutionState.Pending, token);
        }

        /// <summary>
        /// Process a task and update the status
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ProcessTaskAsync(PageTransformationTask task, CancellationToken token = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            var logger = ServiceProvider.GetRequiredService<ILogger<LongRunningTransformationProcessBase>>();
            var pageTransformator = ServiceProvider.GetRequiredService<IPageTransformator>();

            // Retrieve current status
            var taskStatus = await GetTaskStatusAsync(task.Id, token).ConfigureAwait(false);

            try
            {
                // Set task to running
                taskStatus = new TransformationProcessTaskStatus(Id, task.Id, taskStatus.CreationDate, DateTimeOffset.Now, null, TransformationTaskExecutionState.Running);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);

                await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                // Set task to completed
                taskStatus = new TransformationProcessTaskStatus(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, TransformationTaskExecutionState.Completed);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while transforming task {id}", task.Id);

                // Set task to faulted
                taskStatus = new TransformationProcessTaskStatus(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, ex);
                await ChangeTaskStatusAsync(taskStatus, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public override async Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default)
        {
            var taskStatus = await TransformationStateManager.ReadStateAsync<TransformationProcessTaskStatus>(Id, token).ConfigureAwait(false);
            if (taskStatus == null)
            {
                throw new ArgumentException($"Cannot find task with id {id}", nameof(id));
            }

            return taskStatus;
        }

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public override IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the state of the process
        /// </summary>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task ChangeProcessStateAsync(TransformationExecutionState state, CancellationToken token)
        {
            var status = new TransformationProcessStatus(Id, state);
            await TransformationStateManager
                .WriteStateAsync(Id, status, token)
                .ConfigureAwait(false);

            await RaiseProgressAsync(status).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the status of a task
        /// </summary>
        /// <param name="status"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual async Task ChangeTaskStatusAsync(TransformationProcessTaskStatus status, CancellationToken token)
        {
            await TransformationStateManager
                .WriteStateAsync(Id, status, token)
                .ConfigureAwait(false);

            await RaiseTasksProgressAsync(status).ConfigureAwait(false);
        }
    }

}