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
        private readonly ITransformationStateManager transformationStateManager;

        public TransformTaskFunction(ITransformationExecutor executor, IPnPContextFactory pnpContextFactory, ITransformationStateManager transformationStateManager)
        {
            this.executor = executor;
            this.pnpContextFactory = pnpContextFactory;
            this.transformationStateManager = transformationStateManager;
        }

        [FunctionName("TransformTaskFunction")]
        public async Task Run([QueueTrigger("tasks", Connection = "%AzureWebJobsStorage%")] TaskQueueItem item,
            ILogger log,
            CancellationToken token)
        {
            log.LogInformation($"Processing: {item.TaskId}");

            var process = await executor.LoadTransformationProcessAsync(item.ProcessId, token);
            if (!(process is LongRunningTransformationProcessBase p)) throw new NotSupportedException();

            var sharePointConfig = await transformationStateManager.ReadStateAsync<SharePointConfig>(process.Id, token);

            PnPContext targetContext = await pnpContextFactory.CreateAsync(sharePointConfig.Target);
            PnPContext sourceContext = await pnpContextFactory.CreateAsync(sharePointConfig.Source);

            var sourceItemId = new SharePointSourceItemId(item.SourcePageUri);
            var sourceProvider = new SharePointSourceProvider(sourceContext);

            var task = new PageTransformationTask(sourceProvider, sourceItemId, targetContext);
            await p.ProcessTaskAsync(task, token);
        }
    }
}
