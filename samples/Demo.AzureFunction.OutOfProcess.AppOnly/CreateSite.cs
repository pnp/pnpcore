using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

        public CreateSite(IPnPContextFactory pnpContextFactory, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<CreateSite>();
            contextFactory = pnpContextFactory;
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
                        var addedFile = await uploadFolder.Files.AddAsync("parker.png",File.OpenRead($".{Path.DirectorySeparatorChar}parker.png"), true);

                        // Step 2: Create the page
                        var page = await newSiteContext.Web.NewPageAsync();
                        page.AddSection(CanvasSectionTemplate.TwoColumnRight, 1);

                        // Add text
                        page.AddControl(page.NewTextPart("<H1>Hello everyone!</H1>"), page.Sections[0].Columns[0]);

                        // Get the image web part 'blueprint'
                        var availableComponents = await page.AvailablePageComponentsAsync();
                        var imageWebPartComponent = availableComponents.FirstOrDefault(p => p.Id == page.DefaultWebPartToWebPartId(DefaultWebPart.Image));

                        // Configure and add the image web part
                        var imagePart = page.NewWebPart(imageWebPartComponent);
                        imagePart.PropertiesJson = PrepareImageWebPartConfiguration(newSiteContext, addedFile.ServerRelativeUrl,
                                                                                    siteAssetsLibrary.Id, addedFile.UniqueId);
                        page.AddControl(imagePart, page.Sections[0].Columns[1]);

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

        private static string PrepareImageWebPartConfiguration(PnPContext context, string imageUrl, Guid siteAssetsId, Guid imageId)
        {
            string baseImageWebPart = "{\"webPartData\":{\"id\":\"d1d91016-032f-456d-98a4-721247c305e8\",\"instanceId\":\"{InstanceId}\",\"title\":\"Image\",\"description\":\"Add an image, picture or photo to your page including text overlays and ability to crop and resize images.\",\"audiences\":[],\"serverProcessedContent\":{\"htmlStrings\":{},\"searchablePlainTexts\":{},\"imageSources\":{\"imageSource\":\"{FullyQualifiedImageUrl}\"},\"links\":{},\"customMetadata\":{\"imageSource\":{\"siteId\":\"{SiteId}\",\"webId\":\"{WebId}\",\"listId\":\"{{ListId}}\",\"uniqueId\":\"{UniqueId}\",\"imgWidth\":-1,\"imgHeight\":-1}}},\"dataVersion\":\"1.9\",\"properties\":{\"imageSourceType\":2,\"captionText\":\"\",\"altText\":\"\",\"linkUrl\":\"\",\"overlayText\":\"\",\"fileName\":\"\",\"siteId\":\"{SiteId}\",\"webId\":\"{WebId}\",\"listId\":\"{{ListId}}\",\"uniqueId\":\"{UniqueId}\",\"imgWidth\":-1,\"imgHeight\":-1,\"alignment\":\"Center\",\"fixAspectRatio\":false}}}";

            return baseImageWebPart.Replace("{InstanceId}", Guid.NewGuid().ToString())
                                   .Replace("{FullyQualifiedImageUrl}", $"https://{context.Uri.DnsSafeHost}{imageUrl}")
                                   .Replace("{SiteId}", context.Site.Id.ToString())
                                   .Replace("{WebId}", context.Web.Id.ToString())
                                   .Replace("{ListId}", siteAssetsId.ToString())
                                   .Replace("{UniqueId}", imageId.ToString());
        }
    }
}
