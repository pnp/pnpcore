using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.Microsoft365;
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

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
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

                    // Get a list of available sensitivity labels
                    var labels = await SensitivityLabelManager.GetLabelsUsingDelegatedPermissionsAsync(context);
                    var siteLabel = labels.FirstOrDefault();
                    Guid sensitivityLabelId = Guid.Empty;

                    // Persist the used site url as we need to have the same url when we run an offline test
                    Uri siteUrl;
                    if (!TestCommon.Instance.Mocking)
                    {
                        siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestcommsite{Guid.NewGuid().ToString().Replace("-", "")}");

                        if (siteLabel != null)
                        {
                            sensitivityLabelId = siteLabel.Id;
                        }

                        Dictionary<string, string> properties = new Dictionary<string, string>
                        {
                            { "SiteUrl", siteUrl.ToString() },
                            { "SensitivityLabelId", sensitivityLabelId.ToString() }
                        };

                        TestManager.SaveProperties(context, properties);
                    }
                    else
                    {
                        siteUrl = new Uri(TestManager.GetProperties(context)["SiteUrl"]);
                        sensitivityLabelId = Guid.Parse(TestManager.GetProperties(context)["SensitivityLabelId"]);
                    }

                    communicationSiteToCreate = new CommunicationSiteOptions(siteUrl, "PnP Core SDK Test")
                    {
                        Description = "This is a test site collection",
                        Language = Language.English,
                        SensitivityLabelId = sensitivityLabelId,
                        ShareByEmailEnabled = true,
                    };


                    SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                    {
                        UsingApplicationPermissions = false
                    };

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
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

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
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

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
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

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(teamSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(teamSiteToCreate.Url);
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
                        IsPublic = true,
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

                    using (var newSiteContext = context.GetSiteCollectionManager().CreateSiteCollection(teamSiteToCreate, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(createdSiteCollection);
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

                        using (var newSiteContext = tenantAdminContext.GetSiteCollectionManager().CreateSiteCollection(classicSite, siteCreationOptions))
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(classicSite.Url);
                }

            }
        }

        [TestMethod]
        public async Task CreateClassicSiteUsingApplicationPermissions()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.Instance.UseApplicationPermissions = true;

            ClassicSiteOptions classicSite = null;

            // Create the site collection
            try
            {
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
                {
                    using (var tenantAdminContext = await context.GetSharePointAdmin().GetTenantAdminCenterContextAsync())
                    {
                        // Determine the user to set as owner
                        await context.Web.LoadAsync(p => p.AssociatedOwnerGroup.QueryProperties(p => p.Users));
                        var user = context.Web.AssociatedOwnerGroup.Users.AsRequested().FirstOrDefault();

                        // Persist the used site url as we need to have the same url when we run an offline test
                        Uri siteUrl;
                        string owner = user.LoginName;
                        if (!TestCommon.Instance.Mocking)
                        {
                            siteUrl = new Uri($"https://{context.Uri.DnsSafeHost}/sites/pnpcoresdktestclassicsite{Guid.NewGuid().ToString().Replace("-", "")}");
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

                        classicSite = new ClassicSiteOptions(siteUrl, "PnP Core SDK Test", "STS#0", owner, Language.English,
                            Model.SharePoint.TimeZone.UTCPLUS0100_BRUSSELS_COPENHAGEN_MADRID_PARIS);

                        SiteCreationOptions siteCreationOptions = new SiteCreationOptions()
                        {
                            UsingApplicationPermissions = false
                        };

                        using (var newSiteContext = tenantAdminContext.GetSiteCollectionManager().CreateSiteCollection(classicSite, siteCreationOptions))
                        {
                            var web = await newSiteContext.Web.GetAsync(p => p.Url, p => p.Title, p => p.Description, p => p.Language);
                            Assert.IsTrue(web.Url == classicSite.Url);
                            Assert.IsTrue(web.Title == classicSite.Title);
                            Assert.IsTrue(web.Language == (int)classicSite.Language);
                        }

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
                TestCommon.Instance.UseApplicationPermissions = false;
                using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    context.GetSiteCollectionManager().DeleteSiteCollection(classicSite.Url);
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

                        using (var newSiteContext = adminContext.GetSiteCollectionManager().CreateSiteCollection(communicationSiteToCreate, siteCreationOptions))
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
                            await Task.Delay(TimeSpan.FromSeconds(30));
                        }

                        // Recycle the site collection
                        adminContext.GetSiteCollectionManager().RecycleSiteCollection(communicationSiteToCreate.Url);

                        // Verify the site collection is returned as recycled site
                        var recycledSites = adminContext.GetSiteCollectionManager().GetRecycledSiteCollections();
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
                        adminContext.GetSiteCollectionManager().RestoreSiteCollection(communicationSiteToCreate.Url);

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay between creation and deletion
                            await Task.Delay(TimeSpan.FromSeconds(30));
                        }

                        // Verify the site collection is not returned as recycled site
                        recycledSites = adminContext.GetSiteCollectionManager().GetRecycledSiteCollections();
                        recycledCommunicationSite = recycledSites.FirstOrDefault(c => c.Url == communicationSiteToCreate.Url);
                        Assert.IsNull(recycledCommunicationSite);

                        if (context.Mode == TestMode.Record)
                        {
                            // Add a little delay between creation and deletion
                            await Task.Delay(TimeSpan.FromSeconds(30));
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
                    context.GetSiteCollectionManager().DeleteSiteCollection(communicationSiteToCreate.Url);
                }

            }
        }

        #endregion

    }
}
