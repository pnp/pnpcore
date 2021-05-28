using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using PnP.Core.Services;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint;

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
        private readonly CloudQueue queueReference;

        public AzureQueueTransformationProcessBase(Guid id, IServiceProvider serviceProvider) : base(id, serviceProvider)
        {
            var tasksQueueName = Environment.GetEnvironmentVariable("TasksQueueName");
            queueReference = serviceProvider.GetRequiredService<CloudQueueClient>().GetQueueReference(tasksQueueName);
        }

        public override async Task StartProcessAsync(ISourceProvider sourceProvider, PnPContext targetContext, CancellationToken token = default)
        {
            await queueReference.CreateIfNotExistsAsync();

            await base.StartProcessAsync(sourceProvider, targetContext, token).ConfigureAwait(false);
        }

        protected override async Task EnqueueTaskAsync(PageTransformationTask task, CancellationToken token)
        {
            var spItemId = (SharePointSourceItemId) task.SourceItemId;

            var message = new TaskQueueItem
            {
                ProcessId = Id,
                SourcePageUri = spItemId.Uri,
                TaskId = task.Id
            };
            string json = JsonConvert.SerializeObject(message);
            await queueReference.AddMessageAsync(new CloudQueueMessage(json),
                    null, null, null, null, token)
                .ConfigureAwait(false);
        }


    }
}
