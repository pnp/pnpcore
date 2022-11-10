using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PnP.Core.Services;
using PnP.Core.Model.SharePoint;
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

            return new OkObjectResult(result);
        }

        public async Task<string> TestAccessWithMSI(ILogger log)
        {
            try
            {
                log.LogInformation("Connecting to the SPO site. READ permissions required");

                using (var pnpContext = await pnpContextFactory.CreateAsync("Default"))
                {
                    log.LogInformation("Connection to SPO established.");

                    var today = DateTime.Now.ToString("yyyy-MM-dd");

                    #region Get List; requires READ
                    var listName = "Site Assets";
                    log.LogInformation($"Retrieving list '{listName}'. READ permissions required");
                    var demoList = pnpContext.Web.Lists.GetByTitle(listName, l => l.Id, l => l.Title, l => l.Description);
                    log.LogInformation("   Succeeded");
                    #endregion

                    #region Duplicate file; requires WRITE
                    log.LogInformation($"Duplicating '__siteIcon__.png' in 'SiteAssets' . WRITE permissions required");
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
                    log.LogInformation("   Succeeded");
                    #endregion

                    #region create list; requires FULL_CONTROL
                    var newListName = $"TEST_{today}";
                    log.LogInformation($"Creating list '{newListName}' . FULL CONTROL permissions required");

                    // Check if lists exists and delete first if needed
                    var newList = pnpContext.Web.Lists.GetByTitle(newListName, l => l.Id, l => l.Title, l => l.Description);
                    // Delete if exists
                    if (newList != null)
                    {
                        await newList.DeleteAsync();
                    }
                    // Create list
                    newList = await pnpContext.Web.Lists.AddAsync(newListName, ListTemplateType.GenericList);
                    log.LogInformation("   Succeeded");
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
                return error.ToString();
            }
            catch (Exception exc)
            {
                log.LogError(exc.Message);
                return exc.Message;
            }
            return "Success";
        }
    }
}