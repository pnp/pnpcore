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
    /// <summary>
    /// Executor that relies on an Azure Storage Queue to process the transformations
    /// </summary>
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

    /// <summary>
    /// Transformation Process that relies on an Azure Storage Queue
    /// </summary>
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

        /// <summary>
        /// Protected method that simply enqueues a task into an Azure Storage Queue
        /// </summary>
        /// <param name="task">The task to enqueue</param>
        /// <param name="token">The cancellation token, if any</param>
        /// <returns></returns>
        protected override async Task EnqueueTaskAsync(PageTransformationTask task, CancellationToken token = default)
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
