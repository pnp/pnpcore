using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class HubSiteTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task RegisterHubAndJoinLeaveUnregisterTest()
        {
            //TestCommon.Instance.Mocking = false;

            CommunicationSiteOptions hubSiteToCreate = null;
            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site urls as we need to have the same url when we run an offline test
                    Uri hubSiteUrl;
                    Uri joinSiteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        hubSiteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsitehub{Guid.NewGuid().ToString().Replace("-", "")}");
                        joinSiteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsitehubjoin{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "HubSiteUrl", hubSiteUrl.ToString() },
                            { "JoinSiteUrl", joinSiteUrl.ToString() }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        hubSiteUrl = new Uri(TestManager.GetProperties(context)["HubSiteUrl"]);
                        joinSiteUrl = new Uri(TestManager.GetProperties(context)["JoinSiteUrl"]);
                    }

                    hubSiteToCreate = new CommunicationSiteOptions(hubSiteUrl, "PnP Core SDK Test")
                    {
                        Language = Language.English,
                    };

                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    Guid hubSiteId;
                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(hubSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == hubSiteToCreate.Url);
                        Assert.IsTrue(web.Title == hubSiteToCreate.Title);
                        Assert.IsTrue(web.Language == (int)hubSiteToCreate.Language);

                        // Let's mark as hub site
                        await newSiteContext.Site.RegisterHubSiteAsync();

                        // Get hub site information
                        var data = newSiteContext.Site.GetHubSiteData(newSiteContext.Site.Id);

                        Assert.IsTrue(data.SiteId == newSiteContext.Site.Id);

                        hubSiteId = data.SiteId;
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(joinSiteUrl, "PnP Core SDK Test")
                    {
                        Language = Language.English,
                    };
                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay 
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }

                        // Let's join the created site to the created hub site
                        newSiteContext.GetSharePointAdmin().ConnectToHubSite(joinSiteUrl, hubSiteUrl);

                        // Let's check if the site is connected to the hub site
                        var getSite = await newSiteContext.Site.GetAsync(p => p.HubSiteId);
                        Assert.IsTrue(getSite.HubSiteId == hubSiteId);

                        // Let's disconnect the site from the hub site
                        await newSiteContext.Site.UnJoinHubSiteAsync();

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay 
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);

                    // Unregister as hub to allow deletion
                    using (var hubContext = await context.CloneForTestingAsync(context, hubSiteToCreate.Url, "RegisterHubAndJoinLeaveUnregisterTest", 2))
                    {
                        await hubContext.Site.UnregisterHubSiteAsync();
                        
                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay 
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }
                    }

                    context.GetSiteCollectionManager().DeleteSiteCollection(hubSiteToCreate.Url);
                }

            }

        }
    }
}
