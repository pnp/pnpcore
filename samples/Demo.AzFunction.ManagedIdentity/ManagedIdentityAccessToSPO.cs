using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PnP.Core.Services;
using PnP.Core.Model.SharePoint;
using System.Collections.Generic;

namespace Demo.AzFunction.ManagedIdentity
{
    public class ManagedIdentityAccessToSPO
    {
        private readonly IPnPContextFactory pnpContextFactory;
        public ManagedIdentityAccessToSPO(IPnPContextFactory pnpContextFactory)
        {
            this.pnpContextFactory = pnpContextFactory;
        }

        [FunctionName("ManagedIdentityAccessToSPO")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            using (var pnpContext = await pnpContextFactory.CreateAsync("Default"))
            {
                log.LogInformation("Connection to SPO established.");

            }

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        // public async Task<System.Collections.Generic.List<string>> TestAccessWithMSI(ILogger log)
        // {
        //     return new System.Collections.Generic.List<string>();
        // }
    }
}