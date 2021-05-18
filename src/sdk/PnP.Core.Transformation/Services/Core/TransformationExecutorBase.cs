using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Transformation.Services.Core
{
    /// <summary>
    /// Base implementation of <see cref="ITransformationExecutor"/>
    /// </summary>
    public abstract class TransformationExecutorBase : ITransformationExecutor
    {
        /// <summary>
        /// Creates a Page Transformation process
        /// </summary>
        /// <param name="sourceContext">The PnPContext of the source site</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <returns>The transformation process</returns>
        public virtual Task<TransformationProcess> CreateTransformationProcessAsync(PnPContext sourceContext, PnPContext targetContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <returns>The transformation process</returns>
        public virtual Task<TransformationProcess> LoadTransformationProcessAsync(Guid processId)
        {
            throw new NotImplementedException();
        }
    }
}
