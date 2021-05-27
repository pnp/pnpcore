using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// In process implementation of <see cref="ITransformationProcess"/>
    /// </summary>
    public class InProcessTransformationProcess : TransformationProcessBase, IDisposable
    {
        private readonly ILogger<InProcessTransformationProcess> logger;
        private readonly ITransformationDistiller transformationDistiller;
        private readonly IPageTransformator pageTransformator;

        private CancellationTokenSource cancellationTokenSource;
        private TransformationProcessStatus status;
        private readonly ConcurrentDictionary<Guid, TransformationProcessTaskStatus> tasksStatus;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logger"></param>
        /// <param name="transformationDistiller"></param>
        /// <param name="pageTransformator"></param>
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
            tasksStatus = new ConcurrentDictionary<Guid, TransformationProcessTaskStatus>();
        }

        /// <summary>
        ///     The unique ID of a Transformation Process
        /// </summary>
        public override Guid Id { get; }

        /// <summary>
        /// Starts the Transformation Process
        /// </summary>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <param name="token">The cancellation token</param>
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
                await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token))
                {

                    var taskStatus = new TransformationProcessTaskStatus(Id, task.Id, DateTimeOffset.Now, DateTimeOffset.Now, null, TransformationTaskExecutionState.Running);
                    try
                    {
                        // Save the task status to pending
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);

                        await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                        // Save the task status to completed
                        taskStatus = new TransformationProcessTaskStatus(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, TransformationTaskExecutionState.Completed);
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while transforming task {id}", task.Id);

                        // Save the task status to faulted
                        taskStatus = new TransformationProcessTaskStatus(Id, task.Id, taskStatus.CreationDate, taskStatus.StartDate, DateTimeOffset.Now, ex);
                        await ChangeTaskStatusAsync(taskStatus).ConfigureAwait(false);
                    }

                }

                // Re-align total
                await ChangeProcessStateAsync(TransformationExecutionState.Completed).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore this kind of error
                await ChangeProcessStateAsync(TransformationExecutionState.Aborted).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The entire process has failed");
                await ChangeProcessStateAsync(TransformationExecutionState.Faulted).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Stops the Transformation Process
        /// </summary>
        public override Task StopProcessAsync(CancellationToken token = default)
        {
            cancellationTokenSource?.Cancel();

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public override Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default)
        {
            return Task.FromResult(status);
        }

        /// <summary>
        /// Gets the status of a single task
        /// </summary>
        /// <param name="id">The id of the task</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public override Task<TransformationProcessTaskStatus> GetTaskStatusAsync(Guid id, CancellationToken token = default)
        {
            TransformationProcessTaskStatus status;
            if (!tasksStatus.TryGetValue(id, out status))
            {
                throw new ArgumentException($"Cannot find task with id {id}", nameof(id));
            }

            return Task.FromResult(status);
        }

        /// <summary>
        /// Gets the list of the tasks using the criteria specified into the query
        /// </summary>
        /// <param name="query">Query to use for filtering</param>
        /// <param name="token">Cancellation token to use</param>
        /// <returns></returns>
        public override async IAsyncEnumerable<TransformationProcessTaskStatus> GetTasksStatusAsync(TasksStatusQuery query, [EnumeratorCancellation] CancellationToken token = default)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            foreach (var pair in tasksStatus)
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
            tasksStatus[status.Id] = status;

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