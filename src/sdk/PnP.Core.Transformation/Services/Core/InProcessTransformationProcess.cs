using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// In process implementation of <see cref="ITransformationProcess"/>
    /// </summary>
    /// <remarks>This class is thread safe</remarks>
    public class InProcessTransformationProcess : TransformationProcessBase, IDisposable
    {
        private readonly ILogger<InProcessTransformationProcess> logger;
        private readonly ITransformationDistiller transformationDistiller;
        private readonly IPageTransformator pageTransformator;

        private CancellationTokenSource cancellationTokenSource;
        private TransformationProcessStatus status;
        private readonly ConcurrentDictionary<Guid, TransformationProcessTaskStatus> tasksStatuses;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="id">The id of the process</param>
        /// <param name="logger">A custom logger</param>
        /// <param name="transformationDistiller">The Page Transformation Distiller to collect items to process</param>
        /// <param name="pageTransformator">The Page Transformator instance to use</param>
        public InProcessTransformationProcess(
            Guid id,
            ILogger<InProcessTransformationProcess> logger,
            ITransformationDistiller transformationDistiller,
            IPageTransformator pageTransformator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.transformationDistiller = transformationDistiller ?? throw new ArgumentNullException(nameof(transformationDistiller));
            this.pageTransformator = pageTransformator ?? throw new ArgumentNullException(nameof(pageTransformator));

            Id = id;

            status = new TransformationProcessStatus(Id, TransformationExecutionState.Pending);
            tasksStatuses = new ConcurrentDictionary<Guid, TransformationProcessTaskStatus>();
        }

        /// <summary>
        /// The unique ID of a Transformation Process
        /// </summary>
        public override Guid Id { get; }

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <param name="token">The cancellation token, if any</param>
        public override async Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default)
        {
            if (cancellationTokenSource != null)
            {
                throw new InvalidOperationException("Process cannot start twice");
            }

            cancellationTokenSource = new CancellationTokenSource();

            await ChangeProcessStateAsync(TransformationExecutionState.Running).ConfigureAwait(false);

            var runToken = cancellationTokenSource.Token;
            _ = Task.Run(() => Run(sourceProvider, targetContext, runToken), token);
        }

        private async Task Run(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token)
        {
            try
            {
                // Iterate through each task
                await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token).ConfigureAwait(false))
                {

                    var taskStatus = TransformationProcessTaskStatus.CreateNormal(Id, task.Id, DateTimeOffset.Now, DateTimeOffset.Now, null, TransformationTaskExecutionState.Running);
                    try
                    {
                        // Save the task status to pending
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);

                        // Execute the actual transformation task
                        await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                        // Save the task status to completed
                        taskStatus = TransformationProcessTaskStatus.CreateNormal(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, TransformationTaskExecutionState.Completed);
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while transforming task {id}", task.Id);

                        // Save the task status to faulted
                        taskStatus = TransformationProcessTaskStatus.CreateFaulted(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, ex);
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);
                    }

                }

                // Mark the process as completed
                await ChangeProcessStateAsync(TransformationExecutionState.Completed).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Mark the process as aborted and ignore this kind of error
                await ChangeProcessStateAsync(TransformationExecutionState.Aborted).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The entire process has failed");
                // Mark the process as faulted
                await ChangeProcessStateAsync(TransformationExecutionState.Faulted).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Stops the Transformation Process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        public override Task StopProcessAsync(CancellationToken token = default)
        {
            cancellationTokenSource?.Cancel();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The status of the Transformation Process</returns>
        public override Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default)
        {
            return Task.FromResult(status);
        }

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The status of the single Task</returns>
        public override Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default)
        {
            TransformationProcessTaskStatus status;
            if (!tasksStatuses.TryGetValue(id, out status))
            {
                throw new ArgumentException($"Cannot find task with id {id}", nameof(id));
            }

            return Task.FromResult(status);
        }

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, [EnumeratorCancellation] CancellationToken token = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            foreach (var pair in tasksStatuses)
            {
                // Filter task
                if (!query.State.HasValue || pair.Value.State == query.State)
                {
                    yield return pair.Value;
                }
            }
        }

        /// <summary>
        /// Overridable dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellationTokenSource?.Dispose();
            }
        }

        private Task ChangeProcessStateAsync(TransformationExecutionState state)
        {
            status = new TransformationProcessStatus(Id, state);
            return RaiseProgressAsync(status);
        }

        private Task ChangeTaskStatusAsync(TransformationProcessTaskStatus status)
        {
            tasksStatuses[status.Id] = status;

            return RaiseTasksProgressAsync(status);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}