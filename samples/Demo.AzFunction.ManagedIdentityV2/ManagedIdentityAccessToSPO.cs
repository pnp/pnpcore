using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using PnP.Core.Services;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core;

namespace Demo.AzFunction.ManagedIdentity
{
    public class ManagedIdentityAccessToSPO(IPnPContextFactory pnpContextFactory, FunctionContext executionContext)
    {
        #region private properties
        private readonly ILogger logger = executionContext.GetLogger(nameof(ManagedIdentityAccessToSPO));
        private readonly IPnPContextFactory pnpContextFactory = pnpContextFactory;
        #endregion


        [Function("ManagedIdentityAccessToSPO")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req
        )
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            var result = await TestAccessWithMSI();

            return new OkObjectResult(result);
        }

        public async Task<string> TestAccessWithMSI()
        {
            try
            {
                logger.LogInformation("Connecting to the SPO site. READ permissions required");

                using (var pnpContext = await pnpContextFactory.CreateAsync("Default"))
                {
                    logger.LogInformation("Connection to SPO established.");

                    var today = DateTime.Now.ToString("yyyy-MM-dd");

                    #region Get List; requires READ
                    var listName = "Site Assets";
                    logger.LogInformation("Retrieving list '{listName}'. READ permissions required", listName);

                    var demoList = pnpContext.Web.Lists.GetByTitle(listName, l => l.Id, l => l.Title, l => l.Description);
                    logger.LogInformation("   Succeeded");
                    #endregion

                    #region Duplicate file; requires WRITE
                    logger.LogInformation("Duplicating '__siteIcon__.png' in 'SiteAssets' . WRITE permissions required");
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
                    logger.LogInformation("   Succeeded");
                    #endregion

                    #region create list; requires FULL_CONTROL
                    var newListName = $"TEST_{today}";
                    logger.LogInformation("Creating list '{newListName}' . FULL CONTROL permissions required", newListName);

                    // Check if lists exists and delete first if needed
                    var newList = pnpContext.Web.Lists.GetByTitle(newListName, l => l.Id, l => l.Title, l => l.Description);
                    // Delete if exists
                    if (newList != null)
                    {
                        await newList.DeleteAsync();
                    }
                    // Create list
                    newList = await pnpContext.Web.Lists.AddAsync(newListName, ListTemplateType.GenericList);
                    logger.LogInformation("   Succeeded");
                    #endregion
                }
            }
            catch (SharePointRestServiceException exc)
            {
                SharePointRestError error = (exc.Error as SharePointRestError)!;
                if (error.HttpResponseCode == 403)
                {
                    logger.LogError("Access is denied. Make sure you granted REQUIRED API permissions to your app.");
                }
                logger.LogError(error.ToString());
                return error.ToString();
            }
            catch (Exception exc)
            {
                logger.LogError(exc.Message);
                return exc.Message;
            }
            return "Success";
        }
    }
}