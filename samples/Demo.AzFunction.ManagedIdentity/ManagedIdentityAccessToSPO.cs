using System;
using System.Linq;
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
using PnP.Core.QueryModel;
using PnP.Core;

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
            var result = TestAccessWithMSI(log).Result;

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public async Task<System.Collections.Generic.List<string>> TestAccessWithMSI(ILogger log)
        {
            try
            {
                using (var pnpContext = await pnpContextFactory.CreateAsync("Default"))
                {
                    var today = DateTime.Now.ToString("yyyy-MM-dd");
                    log.LogInformation("Connection to SPO established.");

                    #region Get List; requires READ
                    var listName = "Site Assets";
                    var demoList = pnpContext.Web.Lists.GetByTitle(listName, l => l.Id, l => l.Title, l => l.Description);
                    log.LogInformation($"List {listName} found.");
                    #endregion

                    #region Duplicate file; requires WRITE
                    // Get logo image file
                    string logoPath = $"{pnpContext.Uri.PathAndQuery}/SiteAssets/__siteIcon__.png";
                    // Get a reference to the file
                    IFile testDocument = await pnpContext.Web.GetFileByServerRelativeUrlAsync(logoPath);
                    // Download the file as stream
                    Stream downloadedContentStream = await testDocument.GetContentAsync();
                    // Get Site Assets library
                    IFolder siteAssetsFolder = await pnpContext.Web.Folders.Where(f => f.Name == "SiteAssets").FirstOrDefaultAsync();
                    // Upload the '__siteIcon__.png' again, using __siteIcon__{today}.png file name
                    IFile addedFile = await siteAssetsFolder.Files.AddAsync($"__siteIcon__{today}.png", downloadedContentStream, true);
                    #endregion

                    #region create list; requires FULL_CONTROL
                    var newListName = $"TEST_{today}";
                    // Check if lists exists and delete first if needed
                    var newList = pnpContext.Web.Lists.GetByTitle(newListName, l => l.Id, l => l.Title, l => l.Description);
                    // Delete if exists
                    if (newList != null)
                    {
                        await newList.DeleteAsync();
                    }
                    // Create list
                    newList = await pnpContext.Web.Lists.AddAsync(newListName, ListTemplateType.GenericList);
                    #endregion
                }
            }
            catch (SharePointRestServiceException exc)
            {
                SharePointRestError error = exc.Error as SharePointRestError;
                if (error.HttpResponseCode == 403)
                {
                    log.LogError($"Access is denied. Make sure you granted REQUIRED API permissions to your app.");
                }
                log.LogError(error.ToString());
            }
            catch (Exception exc)
            {
                log.LogError(exc.Message);
            }
            return new System.Collections.Generic.List<string>();
        }
    }
}