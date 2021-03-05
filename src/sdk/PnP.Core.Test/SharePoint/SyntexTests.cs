using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Collections.Generic;
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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

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
                var result = await modelToRegister.PublishModelAsync(new SyntexModelPublicationOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsTrue(result != null);
                Assert.IsTrue(result.ErrorMessage == null);
                Assert.IsTrue(result.StatusCode == 201);
                Assert.IsTrue(result.Succeeded);

                // unpublish model from library
                var unpublishResult = await modelToRegister.UnPublishModelAsync(new SyntexModelUnPublicationOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                Assert.IsTrue(unpublishResult != null);
                Assert.IsTrue(unpublishResult.ErrorMessage == null);
                Assert.IsTrue(unpublishResult.StatusCode == 200);
                Assert.IsTrue(unpublishResult.Succeeded);

                // cleanup the library
                await testLibrary.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelsToList()
        {
            //TestCommon.Instance.Mocking = false;
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                // Add library to site
                var libraryName1 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList1");
                var libraryName2 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList2");
                var library1 = await context.Web.Lists.AddAsync(libraryName1, ListTemplateType.DocumentLibrary);
                var library2 = await context.Web.Lists.AddAsync(libraryName2, ListTemplateType.DocumentLibrary);

                List<IList> libraries = new();
                libraries.Add(library1);
                libraries.Add(library2);

                // publish model to library
                var results = await modelToRegister.PublishModelAsync(libraries);
                Assert.IsTrue(results != null);
                foreach (var result in results)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library
                var unpublishResults = await modelToRegister.UnPublishModelAsync(libraries);
                Assert.IsTrue(unpublishResults != null);
                foreach (var unpublishResult in unpublishResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // cleanup the libraries
                await library1.DeleteAsync();
                await library2.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task PublishUnPublishModelsToList2()
        {
            //TestCommon.Instance.Mocking = false;
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                var models = await cc.GetSyntexModelsAsync();
                var modelToRegister = models.First();

                await context.Web.EnsurePropertiesAsync(p => p.ServerRelativeUrl).ConfigureAwait(false);

                // Add library to site
                var libraryName1 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList21");
                var libraryName2 = TestCommon.GetPnPSdkTestAssetName("PublishUnPublishModelsToList22");
                var library1 = await context.Web.Lists.AddAsync(libraryName1, ListTemplateType.DocumentLibrary);
                var library2 = await context.Web.Lists.AddAsync(libraryName2, ListTemplateType.DocumentLibrary);

                List<SyntexModelPublicationOptions> publications = new();
                publications.Add(new SyntexModelPublicationOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName1}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });
                publications.Add(new SyntexModelPublicationOptions()
                {
                    TargetLibraryServerRelativeUrl = $"{context.Web.ServerRelativeUrl}/{libraryName2}",
                    TargetSiteUrl = context.Uri.ToString(),
                    TargetWebServerRelativeUrl = context.Web.ServerRelativeUrl,
                });

                // publish model to library
                var results = await modelToRegister.PublishModelAsync(publications);
                Assert.IsTrue(results != null);
                foreach (var result in results)
                {
                    Assert.IsTrue(result.ErrorMessage == null);
                    Assert.IsTrue(result.StatusCode == 201);
                    Assert.IsTrue(result.Succeeded);
                }

                // unpublish model from library
                var unpublishResults = await modelToRegister.UnPublishModelAsync(publications.Cast<SyntexModelUnPublicationOptions>().ToList());
                Assert.IsTrue(unpublishResults != null);
                foreach (var unpublishResult in unpublishResults)
                {
                    Assert.IsTrue(unpublishResult.ErrorMessage == null);
                    Assert.IsTrue(unpublishResult.StatusCode == 200);
                    Assert.IsTrue(unpublishResult.Succeeded);
                }

                // cleanup the libraries
                await library1.DeleteAsync();
                await library2.DeleteAsync();
            }
        }

    }
}
