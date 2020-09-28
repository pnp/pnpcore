using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Model.SharePoint;
using System.Linq;

namespace Demo.AzureFunction
{
    public class GetSiteDocuments
    {
        private readonly IPnPContextFactory pnpContextFactory;
        public GetSiteDocuments(IPnPContextFactory pnpContextFactory)
        {
            this.pnpContextFactory = pnpContextFactory;
        }

        [FunctionName("GetSiteDocuments")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            log.LogInformation("GetSiteDocuments()");
            using (var pnpContext = await pnpContextFactory.CreateAsync("Default"))
            {
                log.LogInformation("Getting all the shared documents on site.");

                IList sharedDocuments = await pnpContext.Web.Lists.GetByTitleAsync("Documents", l => l.RootFolder);
                var sharedDocumentsFolder = await sharedDocuments.RootFolder.GetAsync(f => f.Files);

                var documents = (from d in sharedDocumentsFolder.Files
                                 select new
                                 {
                                     d.Name,
                                     d.TimeLastModified,
                                     d.UniqueId
                                 }).ToList();

                return new OkObjectResult(new { documents });
            }
        }
    }
}
