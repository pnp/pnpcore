using System;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implementation of <see cref="ITransformationExecutor"/> that persists state for a long running transformation
    /// </summary>
    public abstract class LongRunningTransformationExecutorBase : ITransformationExecutor
    {
        /// <summary>
        /// The provider to use to resolve dependencies
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Constructor with Dependency Injection
        /// </summary>
        protected LongRunningTransformationExecutorBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Creates a Page Transformation process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        public virtual async Task<ITransformationProcess> CreateTransformationProcessAsync(
            CancellationToken token = default)
        {
            var process = CreateProcess(Guid.NewGuid());
            await process.InitAsync(token).ConfigureAwait(false);

            return process;
        }

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        public virtual Task<ITransformationProcess> LoadTransformationProcessAsync(Guid processId,
            CancellationToken token = default)
        {
            // When a long running process is restored, source provider and target context are not available
            ITransformationProcess process = CreateProcess(processId);

            return Task.FromResult(process);
        }

        /// <summary>
        /// Creates a new instance of a inherited of type <see cref="LongRunningTransformationProcessBase"/>
        /// </summary>
        /// <param name="id">The process id</param>
        /// <returns>A new instance of a long running transformation process</returns>
        protected abstract LongRunningTransformationProcessBase CreateProcess(Guid id);
    }
}
