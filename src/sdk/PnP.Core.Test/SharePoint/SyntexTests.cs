using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SyntexTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task IsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsFalse(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task IsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;
            
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                Assert.IsTrue(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsTrue(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsTrue(await context.Web.AsSyntexContentCenterAsync() == null);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() == null);
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                Assert.IsTrue(cc != null);
                Assert.IsTrue(cc.Web.Id == context.Web.Id);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() != null);
            }
        }

        [TestMethod]
        public async Task GetSyntexModels()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                Assert.IsTrue(models.Any());
                Assert.IsTrue(!string.IsNullOrEmpty(models.First().Name));
                Assert.IsTrue(models.First().ModelLastTrained != DateTime.MinValue);
            }
        }

        [TestMethod]
        public async Task GetSyntexModelsViaFilter()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = context.Web.AsSyntexContentCenter();
                var models = cc.GetSyntexModels("XYZ");
                Assert.IsFalse(models.Any());
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelToList()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelToList");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync(testLibrary);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(testLibrary);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelToList2()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelToList2");
                var testLibrary = await context.Web.Lists.AddAsync(libraryName, ListTemplateType.DocumentLibrary);

                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                // publish model to library
                var result = await modelToRegister.PublishModelAsync($"{context.Web.ServerRelativeUrl}/{libraryName}", context.Uri.ToString(), context.Web.ServerRelativeUrl);
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync($"{context.Web.ServerRelativeUrl}/{libraryName}", context.Uri.ToString(), context.Web.ServerRelativeUrl);
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

    }
}
