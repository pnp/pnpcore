using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
