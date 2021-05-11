using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// Implementation of <see cref="ITransformationExecutor"/> that persists state for a long running operation
    /// </summary>
    public abstract class LongRunningTransformationExecutorBase : TransformationExecutorBase
    {
        /// <summary>
        /// Instance of the Transformation Distiller
        /// </summary>
        protected ITransformationDistiller TransformationDistiller { get; }

        /// <summary>
        /// Instance of the Transformation State Manager
        /// </summary>
        protected ITransformationStateManager TransformationStateManager { get; }

        /// <summary>
        /// Constructor with Dependency Injection
        /// </summary>
        /// <param name="transformationDistiller">The Transformation Distiller</param>
        /// <param name="transformationStateManager">The State Manager</param>
        protected LongRunningTransformationExecutorBase(
            ITransformationDistiller transformationDistiller,
            ITransformationStateManager transformationStateManager)
        {
            TransformationDistiller = transformationDistiller ?? throw new ArgumentNullException(nameof(transformationDistiller));
            TransformationStateManager = transformationStateManager ?? throw new ArgumentNullException(nameof(transformationStateManager));
        }

        /// <summary>
        /// Creates a Page Transformation process
        /// </summary>
        /// <param name="sourceContext">The PnPContext of the source site</param>
        /// <param name="targetContext">The PnPContext of the target site</param>
        /// <returns>The transformation process</returns>
        public override Task<TransformationProcess> CreateTransformationProcessAsync(PnPContext sourceContext, PnPContext targetContext)
        {
            return base.CreateTransformationProcessAsync(sourceContext, targetContext);
        }

        /// <summary>
        /// Loads a Page Transformation process
        /// </summary>
        /// <param name="processId">The ID of the process to load</param>
        /// <returns>The transformation process</returns>
        public override Task<TransformationProcess> LoadTransformationProcessAsync(Guid processId)
        {
            return base.LoadTransformationProcessAsync(processId);
        }
    }
}
