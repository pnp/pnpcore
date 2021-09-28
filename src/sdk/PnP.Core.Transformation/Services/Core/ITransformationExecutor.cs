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
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        Task<ITransformationProcess> CreateTransformationProcessAsync(CancellationToken token = default);

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns>The transformation process</returns>
        Task<ITransformationProcess> LoadTransformationProcessAsync(Guid processId, CancellationToken token = default);
    }
}
