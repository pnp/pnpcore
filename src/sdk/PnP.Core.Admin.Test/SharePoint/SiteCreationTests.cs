using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class SiteCreationTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod]
        public async Task CreateCommunicationSiteUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl; 
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,                        
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url);
                }

            }
        }
    }
}
