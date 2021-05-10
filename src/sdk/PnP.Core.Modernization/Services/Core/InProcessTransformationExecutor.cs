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

        public InProcessTransformationExecutor(
            ITransformationDistiller transformationDistiller,
            ITransformationStateManager transformationStateManager,
            IPageTransformator transformator
            ) : base(transformationDistiller, transformationStateManager)
        {
            this.PageTransformator = transformator ?? throw new ArgumentNullException(nameof(transformator));
        }

        public IPageTransformator PageTransformator { get; }

        public override async Task<Guid> StartTransformAsync(PnPContext sourceContext, PnPContext targetContext)
        {
            var processId = await base.StartTransformAsync(sourceContext, targetContext);

            // Start into another thread
            _ = Task.Run(ProcessTasksAsync);

            return processId;
        }

        protected virtual async Task ProcessTasksAsync()
        {
            // TODO: iterate through state keys
            var tasks = new PageTransformationTask[0];
            foreach (var pageTransformationTask in tasks)
            {
                await PageTransformator.TransformAsync(pageTransformationTask);
            }
        }
    }
}
