using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class TenantAppManagerTests
    {
        private const string packageName = "pnp-alm-app.sppkg";
        private const string appTitle = "pnp-alm-app-client-side-solution";
        private string packagePath = $"TestAssets/{packageName}";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTenantAppCatalogUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetTenantAppManager().GetTenantAppCatalogUri();
                Assert.IsTrue(url != null);
            }
        }

        //[TestMethod]
        //public async Task GetTenantAppCatalogUrlReturnsNullTest()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var url = context.GetTenantAppManager().GetTenantAppCatalogUri();
        //        Assert.IsTrue(url == null);
        //    }
        //}

        [TestMethod]
        public async Task EnsureTenantAppCatalogUrlTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppCatalogCreated = context.GetTenantAppManager().EnsureTenantAppCatalog();
                Assert.IsFalse(tenantAppCatalogCreated);
            }
        }


        [TestMethod]
        public async Task AddTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                Assert.IsNotNull(app);
            }
        }

        [TestMethod]
        public async Task AddUsingTenantAppCatalogContextTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var appCatalogUrl = appManager.GetTenantAppCatalogUri();
                using (var tenantCatalogContext = context.Clone(appCatalogUrl))
                {
                    appManager = tenantCatalogContext.GetTenantAppManager();
                    var app = appManager.Add(packagePath, true);
                    Assert.IsNotNull(app);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddThrowsArgumentExceptionTitleNullTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(null, "filename", true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public async Task AddThrowsExceptionNoneExistingPathTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add("none-existing.sppkg", true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddThrowsArgumentExceptionPathNullTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(null, true);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task AddThrowsArgumentExceptionFilenameNullTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var bytes = System.IO.File.ReadAllBytes(packagePath);
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(bytes, null, true);
            }
        }

        [TestMethod]
        public async Task GetAppByTitleTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                appManager.Add(packagePath, true);
                var app = appManager.GetAvailable(appTitle);

                Assert.IsNotNull(app);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetAppByTitleThrowsArgumentExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.GetAvailable(null);
            }
        }

        [TestMethod]
        public async Task GetAppByTitleReturnsNullTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                appManager.Add(packagePath, true);
                var app = appManager.GetAvailable("none-existing");

                Assert.IsNull(app);
            }
        }

        [TestMethod]
        public async Task GetAppByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                appManager.Add(packagePath, true);
                var app = appManager.GetAvailable(appTitle);
                app = appManager.GetAvailable(app.Id);

                Assert.IsNotNull(app);
            }
        }

        [TestMethod]
        public async Task DeployTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                var result = app.Deploy(false);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task DeployByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                var result = appManager.Deploy(app.Id, false);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task GetAvailableAppsTests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                appManager.Add(packagePath, true);
                var apps = appManager.GetAvailable();

                Assert.IsTrue(apps.Count > 0);
            }
        }

        [TestMethod]
        public async Task InstallTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);
                if (app.InstalledVersion != null)
                {
                    app.Uninstall();
                    if (!TestCommon.Instance.Mocking)
                    {
                        // uninstall takes some time
                        await Task.Delay(10 * 1000);
                    }
                }
                var result = app.Install();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task InstallByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);
                if (app.InstalledVersion != null)
                {
                    app.Uninstall();
                    if (!TestCommon.Instance.Mocking)
                    {
                        // uninstall takes some time
                        await Task.Delay(10 * 1000);
                    }
                }

                var result = appManager.Install(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task UpgradeTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);

                if (app.InstalledVersion == null)
                {
                    app.Install();
                }

                var result = app.Upgrade();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task UpgradeByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);

                if (app.InstalledVersion == null)
                {
                    app.Install();
                }

                var result = appManager.Upgrade(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task UninstallTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);

                if (app.InstalledVersion == null)
                {
                    app.Install();
                }

                var result = app.Uninstall();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task UninstallByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);

                if (app.InstalledVersion == null)
                {
                    app.Install();
                }

                var result = appManager.Uninstall(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task RetractTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);
                var result = app.Retract();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task RetractByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);
                var result = appManager.Retract(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task RemoveTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                var result = app.Remove();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task RemoveByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                var result = appManager.Remove(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SolutionContainsTeamsComponentTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var app = tenantAppManager.Add(packagePath, true);
                app.Deploy(false);

                var result = tenantAppManager.SolutionContainsTeamsComponent(app.Id);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task GetSiteCollectionAppCatalogsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var result = tenantAppManager.GetSiteCollectionAppCatalogs();

                Assert.IsTrue(result != null && result.Count > 0);
            }
        }

        [TestMethod]
        public async Task AddRemoveSiteCollectionAppCatalogTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();

                tenantAppManager.AddSiteCollectionAppCatalog(context.Uri);

                var result = tenantAppManager.GetSiteCollectionAppCatalogs();

                Assert.IsTrue(result != null && result.Count > 0);
                Assert.IsTrue(result.FirstOrDefault(p => p.AbsoluteUrl == context.Uri.ToString()) != null);

                tenantAppManager.RemoveSiteCollectionAppCatalog(context.Uri);

                var result2 = tenantAppManager.GetSiteCollectionAppCatalogs();

                Assert.IsTrue(result2 != null && result2.Count > 0);
                Assert.IsTrue(result2.FirstOrDefault(p => p.AbsoluteUrl == context.Uri.ToString()) == null);
            }
        }

        [TestMethod]
        public async Task EnsureSiteCollectionAppCatalogTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                try
                {
                    // We start from the assumption there's no app catalog for this site
                    var result2 = tenantAppManager.GetSiteCollectionAppCatalogs();

                    Assert.IsTrue(result2 != null && result2.Count > 0);
                    Assert.IsTrue(result2.FirstOrDefault(p => p.AbsoluteUrl == context.Uri.ToString()) == null);

                    // Ensure the app catalog    
                    tenantAppManager.EnsureSiteCollectionAppCatalog(context.Uri);

                    // Verify there now is
                    var result = tenantAppManager.GetSiteCollectionAppCatalogs();

                    Assert.IsTrue(result != null && result.Count > 0);
                    Assert.IsTrue(result.FirstOrDefault(p => p.AbsoluteUrl == context.Uri.ToString()) != null);
                }
                finally
                {
                    // Drop the created 
                    tenantAppManager.RemoveSiteCollectionAppCatalog(context.Uri);
                }
            }
        }

        [TestMethod]
        public async Task GetStoreAppsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var result = tenantAppManager.GetStoreApps();

                Assert.IsTrue(result != null);
            }
        }

        [TestMethod]
        public async Task AddAndDeployStoreAppByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var app = tenantAppManager.AddAndDeployStoreApp("WA200003254", CultureInfo.GetCultureInfo(1033).Name, false, true);

                Assert.IsTrue(app != null);
            }
        }

        [TestMethod]
        public async Task IsAppUpgradeAvailableTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var app = tenantAppManager.Add(packagePath, true);
                var result = tenantAppManager.IsAppUpgradeAvailable(app.Id);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task DownloadTeamsSolutionAsStreamTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantAppManager = context.GetTenantAppManager();
                var app = tenantAppManager.Add(packagePath, true);
                app.Deploy(false);

                using (var stream = tenantAppManager.DownloadTeamsSolution(app.Id))
                using (var outputFileStream = new FileStream("teams app.zip", FileMode.Create))
                {
                    stream.CopyTo(outputFileStream);

                    Assert.IsTrue(stream != null);
                }
            }
        }

        [TestMethod]
        public async Task DownloadTeamsSolutionAsStreamByIdTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var appManager = context.GetTenantAppManager();
                var app = appManager.Add(packagePath, true);
                app.Deploy(false);
                var appCatalogUrl = await appManager.GetTenantAppCatalogUriAsync();
                using (var appCatalogContext = await context.CloneAsync(appCatalogUrl))
                {
                    var list = await appCatalogContext.Web.Lists.GetByServerRelativeUrlAsync($"{appCatalogContext.Uri.LocalPath}/appcatalog").ConfigureAwait(false);
                    var query = new CamlQueryOptions
                    {
                        ViewXml = $"<View><Query><Where><Contains><FieldRef Name='UniqueId'/><Value Type='Text'>{app.Id}</Value></Contains></Where></Query></View>"
                    };
                    await list.LoadItemsByCamlQueryAsync(query).ConfigureAwait(false);
                    var item = list.Items.AsRequested().Single();

                    var stream = appManager.DownloadTeamsSolution(item.Id);

                    Assert.IsTrue(stream != null);
                }
            }
        }
    }
}
