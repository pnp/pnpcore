using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Implementation of <see cref="ITransformationExecutor"/> that works sequentially and in process
    /// </summary>
    /// <remarks>This class is thread safe</remarks>
    public class InProcessTransformationExecutor : ITransformationExecutor, IDisposable
    {
        private readonly IServiceProvider serviceProvider;

        private readonly ConcurrentDictionary<Guid, ITransformationProcess> processes = new ConcurrentDictionary<Guid, ITransformationProcess>();

        /// <summary>
        /// Constructor with Dependency Injection
        /// </summary>
        public InProcessTransformationExecutor(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a Page Transformation process
        /// </summary>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        public Task<ITransformationProcess> CreateTransformationProcessAsync(CancellationToken token = default)
        {
            var transformationDistiller = serviceProvider.GetRequiredService<ITransformationDistiller>();
            var pageTransformator = serviceProvider.GetRequiredService<IPageTransformator>();
            var logger = serviceProvider.GetRequiredService<ILogger<InProcessTransformationProcess>>();

            ITransformationProcess result = new InProcessTransformationProcess(
                Guid.NewGuid(),
                logger,
                transformationDistiller,
                pageTransformator);

            processes.TryAdd(result.Id, result);

            return Task.FromResult(result);
        }

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        public Task<ITransformationProcess> LoadTransformationProcessAsync(Guid processId,
            CancellationToken token = default)
        {
            if (processes.TryGetValue(processId, out ITransformationProcess tp))
            {
                return Task.FromResult(tp);
            }

            throw new ArgumentException($"Cannot find process with id {processId}");
        }

        /// <summary>
        /// Overridable dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var pair in processes)
                {
                    (pair.Value as IDisposable)?.Dispose();
                }
                processes.Clear();
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
