using System;
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
        private readonly ISourceProvider sourceProvider;
        private readonly PnPContext targetContext;
        private readonly ITransformationDistiller transformationDistiller;
        private readonly IPageTransformator pageTransformator;
        private CancellationTokenSource cancellationTokenSource;

        private int done;
        private int errors;
        private int? total;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="logger"></param>
        /// <param name="sourceProvider"></param>
        /// <param name="targetContext"></param>
        /// <param name="transformationDistiller"></param>
        /// <param name="pageTransformator"></param>
        public InProcessTransformationProcess(
            Guid id,
            ILogger<InProcessTransformationProcess> logger,
            ISourceProvider sourceProvider,
            PnPContext targetContext,
            ITransformationDistiller transformationDistiller,
            IPageTransformator pageTransformator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sourceProvider = sourceProvider ?? throw new ArgumentNullException(nameof(sourceProvider));
            this.targetContext = targetContext ?? throw new ArgumentNullException(nameof(targetContext));
            this.transformationDistiller = transformationDistiller ?? throw new ArgumentNullException(nameof(transformationDistiller));
            this.pageTransformator = pageTransformator ?? throw new ArgumentNullException(nameof(pageTransformator));
            Id = id;
        }

        /// <summary>
        ///     The unique ID of a Transformation Process
        /// </summary>
        public override Guid Id { get; }

        /// <summary>
        ///     Starts the Transformation Process
        /// </summary>
        public override Task StartProcessAsync(CancellationToken token = default)
        {
            if (cancellationTokenSource is { IsCancellationRequested: false })
            {
                throw new InvalidOperationException("Cannot start process since is already running");
            }

            cancellationTokenSource = new CancellationTokenSource();
            var runToken = cancellationTokenSource.Token;
            _ = Task.Run(() => Run(runToken), token);
            return Task.CompletedTask;
        }

        private async Task Run(CancellationToken token)
        {
            try
            {
                // TODO: can compute?
                total = null;
                done = 0;
                errors = 0;

                await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token))
                {
                    try
                    {
                        await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                        done++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while transforming task {id}", task.Id);
                        errors++;
                    }
                    await RaiseProgressAsync(GetStatus()).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore this kind of error
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
            return Task.FromResult(GetStatus());
        }

        private TransformationProcessStatus GetStatus()
        {
            if (cancellationTokenSource == null)
            {
                return TransformationProcessStatus.GetPending(Id);
            }
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return TransformationProcessStatus.GetAborted(Id, done, errors, total);
            }
            if (done + errors >= total)
            {
                return TransformationProcessStatus.GetCompleted(Id, done, errors, total);
            }
            return TransformationProcessStatus.GetRunning(Id, done, errors, total);
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

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}