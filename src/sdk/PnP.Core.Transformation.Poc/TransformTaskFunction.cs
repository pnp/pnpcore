using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Transformation.Poc.Implementations;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint;

namespace PnP.Core.Transformation.Poc
{
    public class TransformTaskFunction
    {
        private readonly ITransformationExecutor executor;
        private readonly IPnPContextFactory pnpContextFactory;

        public TransformTaskFunction(ITransformationExecutor executor, IPnPContextFactory pnpContextFactory)
        {
            this.executor = executor;
            this.pnpContextFactory = pnpContextFactory;
        }

        [FunctionName("TransformTaskFunction")]
        public async Task Run([QueueTrigger("tasks", Connection = "%AzureWebJobsStorage%")] TaskQueueItem item,
            ILogger log,
            CancellationToken token)
        {
            log.LogInformation($"Processing: {item.TaskId}");

            var process = await executor.LoadTransformationProcessAsync(item.ProcessId, token);
            if (!(process is LongRunningTransformationProcessBase p)) throw new NotSupportedException();

            PnPContext targetContext = null;
            PnPContext sourceContext = null;
            
            var sourceItemId = new SharePointSourceItemId(item.SourcePageUri);
            var sourceProvider = new SharePointSourceProvider(sourceContext);
            var sourceItem = await sourceProvider.GetItemAsync(sourceItemId, token);

            var task = new PageTransformationTask(sourceItem, targetContext, item.TargetPageUri);
            await p.ProcessTaskAsync(task, token);
        }
    }
}
