using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Extensions for <see cref="ITransformationProcess"/>
    /// </summary>
    public static class TransformationExecutorExtensions
    {
        /// <summary>
        /// Starts a process and wait for its completion
        /// </summary>
        /// <param name="process">The process to start</param>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnP target context</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static async Task<TransformationProcessStatus> StartAndWaitProcessAsync(
            this ITransformationProcess process,
            ISourceProvider sourceProvider,
            PnPContext targetContext,
            CancellationToken token = default)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));
            if (sourceProvider == null) throw new ArgumentNullException(nameof(sourceProvider));
            if (targetContext == null) throw new ArgumentNullException(nameof(targetContext));

            // Start the process
            await process.StartProcessAsync(sourceProvider, targetContext, token).ConfigureAwait(false);

            return await process.WaitProcessAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Wait for the completion of a process
        /// </summary>
        /// <param name="process">The process to start</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static async Task<TransformationProcessStatus> WaitProcessAsync(
            this ITransformationProcess process,
            CancellationToken token = default)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            // Intercept progress
            var completionTask = new TaskCompletionSource<TransformationProcessStatus>();
            process.Progress = LocalProgress;

            // When token is cancelled, stop the process
            token.Register(() => _ = process.StopProcessAsync(token));

            // Since process could be already completed, we check immediately the status
            await LocalProgress(await process.GetStatusAsync(token).ConfigureAwait(false)).ConfigureAwait(false);

            // Wait for the completion of the task
            return await completionTask.Task.ConfigureAwait(false);

            Task LocalProgress(TransformationProcessStatus status)
            {
                switch (status.State)
                {
                    case TransformationExecutionState.Completed:
                        completionTask.TrySetResult(status);
                        break;
                    case TransformationExecutionState.Aborted:
                        completionTask.TrySetCanceled(token);
                        break;
                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Creates a new transformation process and wait for its completion
        /// </summary>
        /// <param name="transformationExecutor">The executor to use</param>
        /// <param name="sourceProvider">The source provider</param>
        /// <param name="targetContext">The target context</param>
        /// <param name="token">The cancellation token</param>
        /// <returns></returns>
        public static async Task<TransformationProcessStatus> TransformAsync(
            this ITransformationExecutor transformationExecutor,
            ISourceProvider sourceProvider,
            PnPContext targetContext,
            CancellationToken token = default)
        {
            if (transformationExecutor == null) throw new ArgumentNullException(nameof(transformationExecutor));

            var process = await transformationExecutor.CreateTransformationProcessAsync(token).ConfigureAwait(false);
            using (process as IDisposable)
            {
                return await process.StartAndWaitProcessAsync(sourceProvider, targetContext, token).ConfigureAwait(false);
            }
        }
    }
}
