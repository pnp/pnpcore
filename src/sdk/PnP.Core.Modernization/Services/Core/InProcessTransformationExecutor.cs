using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class InProcessTransformationExecutor : LongRunningTransformationExecutorBase
    {
        private readonly IPageTransformator _pageTransformator;

        /// <summary>
        /// Constructor with Dependency Injection
        /// </summary>
        /// <param name="transformationDistiller">The Transformation Distiller</param>
        /// <param name="transformationStateManager">The State Manager</param>
        /// <param name="transformator">The Page Transformator</param>
        public InProcessTransformationExecutor(
            ITransformationDistiller transformationDistiller,
            ITransformationStateManager transformationStateManager,
            IPageTransformator transformator
            ) : base(transformationDistiller, transformationStateManager)
        {
            this._pageTransformator = transformator ?? throw new ArgumentNullException(nameof(transformator));
        }
    }
}
