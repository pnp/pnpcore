using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PnP.Core.Transformation.Services.Core;

namespace PnP.Core.Transformation.Poc.Implementations
{
    public class AzureQueueTransformationExecutor : LongRunningTransformationExecutorBase
    {
        public AzureQueueTransformationExecutor(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        
        
        protected override LongRunningTransformationProcessBase CreateProcess(Guid id)
        {
            return new AzureQueueTransformationProcessBase(id, ServiceProvider);
        }
    }

    public class AzureQueueTransformationProcessBase : LongRunningTransformationProcessBase
    {
        public AzureQueueTransformationProcessBase(Guid id, IServiceProvider serviceProvider) : base(id, serviceProvider)
        {
        }


        protected override Task QueueTaskAsync(PageTransformationTask task, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override Task StopProcessAsync(CancellationToken token = default)
        {
            return base.StopProcessAsync(token);
        }
    }
}
