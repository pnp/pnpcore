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
using PnP.Core.Transformation.Services.Core;
using PnP.Core.Transformation.SharePoint;

namespace PnP.Core.Transformation.Poc
{
    public class TransformSiteFunction
    {
        private readonly IPnPContextFactory pnpContextFactory;
        private readonly ITransformationExecutor transformationExecutor;

        public TransformSiteFunction(IPnPContextFactory pnpContextFactory, ITransformationExecutor transformationExecutor)
        {
            this.pnpContextFactory = pnpContextFactory;
            this.transformationExecutor = transformationExecutor;
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

                    process = await transformationExecutor.CreateSharePointTransformationProcessAsync(
                        pnpContextFactory,
                        source,
                        target, 
                        token);
                    await process.StartProcessAsync(token);

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
