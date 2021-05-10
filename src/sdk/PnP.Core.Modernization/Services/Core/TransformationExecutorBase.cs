using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PnP.Core.Services;

namespace PnP.Core.Modernization.Services.Core
{
    public abstract class TransformationExecutorBase : ITransformationExecutor
    {
        public Action<TransformationExecutionStatus> Progress { get; set; }
        public Task<TransformationExecutionStatus> GetStatusAsync(Guid processId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> StartTransformAsync(PnPContext SourceContext, PnPContext TargetContext)
        {
            throw new NotImplementedException();
        }

        public Task StopTransformAsync(Guid processId)
        {
            throw new NotImplementedException();
        }
    }
}
