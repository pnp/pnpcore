using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Communication sites

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

                    int rewrites = 0;
                    context.BatchClient.MockingFileRewriteHandler = (string input) =>
                    {
                        if (rewrites < 2 && input.Contains(",\"SiteStatus\":2,"))
                        {
                            input = input.Replace(",\"SiteStatus\":2,", ",\"SiteStatus\":1,");
                            rewrites++;
                        }
                        return input;
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);
                }

            }
        }

        [TestMethod]
        public async Task CreateCommunicationSiteAdvancedUsingDelegatedPermissions()
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
                        SensitivityLabelId = Guid.Parse("fc60ee7c-2723-44c1-a0ef-296c3c36babc"),
                        ShareByEmailEnabled = true,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var site = await newSiteContext.Site.GetAsync(p => p.SensitivityLabelId, p => p.SensitivityLabel);
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);

                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                        Assert.IsTrue(site.SensitivityLabelId == communicationSiteToCreate.SensitivityLabelId);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);
                }

            }
        }

        [TestMethod]
        public async Task CreateCommunicationSiteUsingDelegatedPermissionsWithAsyncProvisioningWait()
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
                        UsingApplicationPermissions = false,
                        WaitForAsyncProvisioning = true
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);
                }

            }
        }

        [TestMethod]
        public async Task CreateCommunicationSiteUsingApplicationPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
                {
                    // Determine the user to set as owner
                    await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Users));
                    var user = context.Web.AssociatedOwnerGroup.Users.AsRequested().FirstOrDefault();

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    string owner = user.LoginName;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "SiteOwner", owner }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        owner = TestManager.GetProperties(context)["SiteOwner"];
                    }                    

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        Owner = owner
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = true
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                        Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                        Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);
                }
                TestCommon.Instance.UseApplicationPermissions = false;
            }
        }

        #endregion

        #region Team site without group

        [TestMethod]
        public async Task CreateTeamSiteWithoutGroupUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            TeamSiteWithoutGroupOptions teamSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestteamsite{Guid.NewGuid().ToString().Replace("-", "")}");
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

                    teamSiteToCreate = new TeamSiteWithoutGroupOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(teamSiteToCreate, siteCreationOptions))
                    {
                        var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Url == teamSiteToCreate.Url);
                        Assert.IsTrue(web.Title == teamSiteToCreate.Title);
                        Assert.IsTrue(web.Description == teamSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)teamSiteToCreate.Language);
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(teamSiteToCreate.Url, teamSiteToCreate.WebTemplate);
                }

            }
        }

        #endregion

        #region Team site

        [TestMethod]
        public async Task CreateTeamSiteUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            TeamSiteOptions teamSiteToCreate = null;

            // Create the site collection
            Uri createdSiteCollection = null;
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {

                    // Persist the used site url as we need to have the same url when we run an offline test
                    string alias;
                    if (!TestCommon.Instance.Mocking)
                    {
                        alias = $"pnpcoresdktestteamsite{Guid.NewGuid().ToString().Replace("-", "")}";
                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "Alias", alias }
                        };
                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        alias = TestManager.GetProperties(context)["Alias"];
                    }

                    teamSiteToCreate = new TeamSiteOptions(alias, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        IsPublic= true,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    int rewrites = 0;
                    context.BatchClient.MockingFileRewriteHandler = (string input) =>
                    {
                        if (!siteCreationOptions.UsingApplicationPermissions.Value && rewrites < 2 && input.Contains(",\"SiteStatus\":2,"))
                        {
                            input = input.Replace(",\"SiteStatus\":2,", ",\"SiteStatus\":1,");
                            rewrites++;
                        }
                        return input;
                    };

                    using (var newSiteContext = context.GetSharePointAdmin().CreateSiteCollection(teamSiteToCreate, siteCreationOptions))
                    {
                        createdSiteCollection = newSiteContext.Uri;

                        Assert.IsTrue(newSiteContext.Site.GroupId != Guid.Empty);

                        var web = await newSiteContext.Web.GetAsync(p => p.Title, p => p.Description, p => p.Language);
                        Assert.IsTrue(web.Description == teamSiteToCreate.Description);
                        Assert.IsTrue(web.Language == (int)teamSiteToCreate.Language);
                    }

                    if (context.Mode == TestMode.Record)
                    {
                        // Add a little delay between creation and deletion
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(createdSiteCollection, teamSiteToCreate.WebTemplate);
                }

            }
        }

        #endregion

        #region Classic site
        [TestMethod]
        public async Task CreateClassicSiteUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            ClassicSiteOptions classicSite = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync())
                    {
                        // Persist the used site url as we need to have the same url when we run an offline test
                        Uri siteUrl;
                        string owner;
                        if (!TestCommon.Instance.Mocking)
                        {
                            siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestclassicsite{Guid.NewGuid().ToString().Replace("-", "")}");
                            owner = context.Web.GetCurrentUser().LoginName;
                            Dictionary<string, string> properties = new Dictionary<string, string>
                            {
                                { "SiteUrl", siteUrl.ToString() },
                                { "Owner",  owner}
                            };
                            TestManager.SaveProperties(context, properties);
                        }
                        else
                        {
                            siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                            owner = TestManager.GetProperties(context)["Owner"];
                        }

                        classicSite = new ClassicSiteOptions(siteUrl, "PnP Core SDK Test", "STS#0", owner, Language.English,
                            Model.SharePoint.TimeZone.UTCPLUS0100_BRUSSELS_COPENHAGEN_MADRID_PARIS);

                        SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                        {
                            UsingApplicationPermissions = false
                        };

                        using (var newSiteContext = tenantAdminContext.GetSharePointAdmin().CreateSiteCollection(classicSite, siteCreationOptions))
                        {
                            var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                            Assert.IsTrue(web.Url == classicSite.Url);
                            Assert.IsTrue(web.Title == classicSite.Title);
                            Assert.IsTrue(web.Language == (int)classicSite.Language);
                        }
                    }
                }
            }
            finally
            {
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(classicSite.Url, classicSite.WebTemplate);
                }

            }
        }
        #endregion

        #region Recycle, Restore, Delete 

        [TestMethod]
        public async Task CreateRestoreDeleteUsingDelegatedPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = false;

            CommunicationSiteOptions communicationSiteToCreate = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
                {
                    using (var adminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync())
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

                        using (var newSiteContext = adminContext.GetSharePointAdmin().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
                        {
                            var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                            Assert.IsTrue(web.Url == communicationSiteToCreate.Url);
                            Assert.IsTrue(web.Title == communicationSiteToCreate.Title);
                            Assert.IsTrue(web.Description == communicationSiteToCreate.Description);
                            Assert.IsTrue(web.Language == (int)communicationSiteToCreate.Language);
                        }

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay between creation and deletion
                            await Task.Delay(TimeSpan.FromSeconds(15));
                        }

                        // Recycle the site collection
                        adminContext.GetSharePointAdmin().RecycleSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);

                        // Verify the site collection is returned as recycled site
                        var recycledSites = adminContext.GetSharePointAdmin().GetRecycledSiteCollections();
                        var recycledCommunicationSite = recycledSites.FirstOrDefault(c => c.Url == communicationSiteToCreate.Url);
                        Assert.IsNotNull(recycledCommunicationSite);
                        Assert.IsTrue(!string.IsNullOrEmpty(recycledCommunicationSite.Name));
                        Assert.IsTrue(!string.IsNullOrEmpty(recycledCommunicationSite.CreatedBy));
                        Assert.IsTrue(!string.IsNullOrEmpty(recycledCommunicationSite.DeletedBy));
                        Assert.IsTrue(recycledCommunicationSite.TimeCreated > DateTime.MinValue);
                        Assert.IsTrue(recycledCommunicationSite.TimeDeleted > DateTime.MinValue);
                        Assert.IsTrue(recycledCommunicationSite.StorageQuota > 0);
                        Assert.IsTrue(recycledCommunicationSite.StorageUsed > 0);
                        Assert.IsTrue(!string.IsNullOrEmpty(recycledCommunicationSite.TemplateName));


                        // Restore the recycled site collection again
                        adminContext.GetSharePointAdmin().RestoreSiteCollection(communicationSiteToCreate.Url);
                        // Verify the site collection is not returned as recycled site
                        recycledSites = adminContext.GetSharePointAdmin().GetRecycledSiteCollections();
                        recycledCommunicationSite = recycledSites.FirstOrDefault(c => c.Url == communicationSiteToCreate.Url);
                        Assert.IsNull(recycledCommunicationSite);

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay between creation and deletion
                            await Task.Delay(TimeSpan.FromSeconds(15));
                        }
                    }
                }
            }
            finally
            {
                // Clean up the created site collection
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSharePointAdmin().DeleteSiteCollection(communicationSiteToCreate.Url, communicationSiteToCreate.WebTemplate);
                }

            }
        }

        #endregion

    }
}
