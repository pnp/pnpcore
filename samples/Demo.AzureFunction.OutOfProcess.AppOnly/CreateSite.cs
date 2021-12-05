using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ProvisioningDemo
{
    public class CreateSite
    {
        private readonly ILogger logger;
        private readonly IPnPContextFactory contextFactory;
        private readonly AzureFunctionSettings azureFunctionSettings;

        public CreateSite(IPnPContextFactory pnpContextFactory, ILoggerFactory loggerFactory, AzureFunctionSettings settings)
        {
            logger = loggerFactory.CreateLogger<CreateSite>();
            contextFactory = pnpContextFactory;
            azureFunctionSettings = settings;
        }

        /// <summary>
        /// Demo function that creates a site collection, uploads an image to site assets and creates a page with an image web part
        /// GET/POST url: http://localhost:7071/api/CreateSite?owner=bert.jansen@bertonline.onmicrosoft.com&sitename=deleteme1844
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("CreateSite")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("CreateSite function starting...");

            // Parse the url parameters
            NameValueCollection parameters = HttpUtility.ParseQueryString(req.Url.Query);
            var siteName = parameters["siteName"];
            var owner = parameters["owner"];

            HttpResponseData response = null;

            try
            {
                using (var pnpContext = await contextFactory.CreateAsync("Default"))
                {
                    response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json");

                    var communicationSiteToCreate = new CommunicationSiteOptions(new Uri($"https://{pnpContext.Uri.DnsSafeHost}/sites/{siteName}"), "Demo site")
                    {
                        Description = "PnP Core SDK demo site",
                        Language = Language.English,
                        Owner = $"i:0#.f|membership|{owner}"
                    };

                    logger.LogInformation($"Creating site: {communicationSiteToCreate.Url}");

                    // Create the new site collection
                    using (var newSiteContext = await pnpContext.GetSiteCollectionManager().CreateSiteCollectionAsync(communicationSiteToCreate))
                    {
                        logger.LogInformation($"Site created: {communicationSiteToCreate.Url}");

                        // Step 1: Upload image to site assets library
                        var siteAssetsLibrary = await newSiteContext.Web.Lists.EnsureSiteAssetsLibraryAsync(p => p.RootFolder);
                        var uploadFolder = await siteAssetsLibrary.RootFolder.EnsureFolderAsync("SitePages/PnP");
                        var addedFile = await uploadFolder.Files.AddAsync("parker.png", File.OpenRead($".{Path.DirectorySeparatorChar}parker.png"), true);

                        // Step 2: Create the page
                        var page = await newSiteContext.Web.NewPageAsync();
                        page.AddSection(CanvasSectionTemplate.OneColumn, 1);

                        // Add text with inline image
                        var text = page.NewTextPart();
                        var parker = await page.GetInlineImageAsync(text, addedFile.ServerRelativeUrl, new PageImageOptions { Alignment = PageImageAlignment.Left });
                        text.Text = $"<H2>Hello everyone!</H2>{parker}<P>Community rocks, sharing is caring!</P>";
                        page.AddControl(text, page.Sections[0].Columns[0]);

                        // Save the page
                        await page.SaveAsync("PnP.aspx");

                        // Return the URL of the created site
                        await response.WriteStringAsync(JsonSerializer.Serialize(new { siteUrl = newSiteContext.Uri.AbsoluteUri }));
                    }

                    return response;
                }
            }
            catch (Exception ex)
            {
                response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
                return response;
            }
        }
    }
}
