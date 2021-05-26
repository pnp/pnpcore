using System;
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
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// The state manager used
        /// </summary>
        protected ITransformationStateManager TransformationStateManager { get; }

        /// <summary>
        ///     Starts the Transformation Process
        /// </summary>
        public override async Task StartProcessAsync(CancellationToken token = default)
        {
            var transformationDistiller = ServiceProvider.GetRequiredService<ITransformationDistiller>();

            // 
            await TransformationStateManager
                .WriteStateAsync(Id, TransformationProcessStatus.GetRunning(Id, 0, 0, null), token)
                .ConfigureAwait(false);
            //await foreach (var task in transformationDistiller.GetPageTransformationTasksAsync(sourceProvider, targetContext, token))
            //{
                
            //}
        }

        /// <summary>
        ///     Stops the Transformation Process
        /// </summary>
        public override Task StopProcessAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Allows to retrieve the status of the Transformation Process
        /// </summary>
        /// <returns>The status of the Transformation Process</returns>
        public override Task<TransformationProcessStatus> GetStatusAsync(CancellationToken token = default)
        {
            var status = TransformationStateManager.ReadStateAsync<TransformationProcessStatus>(Id, token);
            if (status == null)
            {
                throw new InvalidOperationException($"Cannot find status for process {Id}");
            }
            return status;
        }

        /// <summary>
        /// Initialize the process
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task InitAsync(CancellationToken token = default)
        {
            // Save the initial state
            await TransformationStateManager
                .WriteStateAsync(Id, TransformationProcessStatus.GetPending(Id), token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Process a task and update the status
        /// </summary>
        /// <param name="task"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ProcessTaskAsync(PageTransformationTask task, CancellationToken token = default)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<LongRunningTransformationProcessBase>>();
            var pageTransformator = ServiceProvider.GetRequiredService<IPageTransformator>();

            TransformationProcessStatus status = null;
            try
            {
                await pageTransformator.TransformAsync(task, token).ConfigureAwait(false);

                var prevStatus = await GetStatusAsync(token).ConfigureAwait(false);
                // TODO: update and concurrency
                status = prevStatus;
                await TransformationStateManager.WriteStateAsync(Id, status, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while transforming task {id}", task.Id);
            }

            if (status != null)
            {
                await RaiseProgressAsync(status).ConfigureAwait(false);
            }
        }
    }

}