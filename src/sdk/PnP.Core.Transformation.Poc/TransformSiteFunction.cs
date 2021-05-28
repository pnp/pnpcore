using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PnP.Core.Services;
using PnP.Core.Transformation.Poc.Implementations;
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint;

namespace PnP.Core.Transformation.Poc
{
    public class TransformSiteFunction
    {
        private readonly IPnPContextFactory pnpContextFactory;
        private readonly ITransformationExecutor transformationExecutor;
        private readonly ITransformationStateManager transformationStateManager;

        public TransformSiteFunction(IPnPContextFactory pnpContextFactory, ITransformationExecutor transformationExecutor, ITransformationStateManager transformationStateManager)
        {
            this.pnpContextFactory = pnpContextFactory;
            this.transformationExecutor = transformationExecutor;
            this.transformationStateManager = transformationStateManager;
        }

        [FunctionName("TransformSite")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            CancellationToken token)
        {
            ITransformationProcess process;
            string id;
            Guid processId;
            switch (req.Method)
            {
                case "POST":
                    string source = req.Query["source"];
                    string target = req.Query["target"];

                    // Create the process
                    process = await transformationExecutor.CreateTransformationProcessAsync(token);
                    // Keep additional info for the process
                    await transformationStateManager.WriteStateAsync(process.Id.ToString(), new SharePointConfig { Source = source, Target = target }, token);
                    // Start to enqueue items
                    await process.StartProcessAsync(
                        pnpContextFactory,
                        source,
                        target, token);

                    return new OkObjectResult(new { process.Id });
                case "GET":
                    id = req.Query["id"];
                    if (!Guid.TryParse(id, out processId)) return new BadRequestResult();

                    process = await transformationExecutor.LoadTransformationProcessAsync(processId, token);
                    var status = await process.GetStatusAsync(token);

                    return new OkObjectResult(status);
                case "DELETE":
                    id = req.Query["id"];
                    if (!Guid.TryParse(id, out processId)) return new BadRequestResult();

                    process = await transformationExecutor.LoadTransformationProcessAsync(processId, token);
                    await process.StopProcessAsync(token);

                    return new OkResult();
            }

            return new BadRequestResult();
        }
    }
}
