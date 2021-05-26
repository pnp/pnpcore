using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Abstract interface for a service that manages the transformation of one or more pages
    /// </summary>
    public interface ITransformationExecutor
    {
        /// <summary>
        /// Creates a Page Transformation process
        /// </summary>
        /// <param name="sourceProvider">The source provider to use</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The transformation process</returns>
        Task<ITransformationProcess> CreateTransformationProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default);

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The transformation process</returns>
        Task<ITransformationProcess> LoadTransformationProcessAsync(Guid processId, CancellationToken token = default);
    }
}
