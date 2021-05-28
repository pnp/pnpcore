using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PnP.Core.Transformation.Poc
{
    public class DashboardFunction
    {
        private readonly IConfiguration configuration;

        public DashboardFunction(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [FunctionName("DashboardFunction")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dashboard")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {

            return new PhysicalFileResult(Path.Combine(context.FunctionAppDirectory, "default.html"), "text/html");
        }
    }
}
