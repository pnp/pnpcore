using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
        /// <param name="sourceContext">The PnPContext of the source site</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <returns>The transformation process</returns>
        Task<TransformationProcess> CreateTransformationProcessAsync(PnPContext sourceContext, PnPContext targetContext);

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <returns>The transformation process</returns>
        Task<TransformationProcess> LoadTransformationProcessAsync(Guid processId);
    }
}
