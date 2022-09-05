using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    [TestClass]
    public class DataModelExtensionTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task IsPropertyAvailable()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                var web = await context.Web.GetAsync(p => p.WelcomePage);

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        public async Task IsPropertyAvailableCollection()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Is the property populated
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));

                var web = await context.Web.GetAsync(p => p.Lists.QueryProperties(p => p.Title));

                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
            }
        }

        [TestMethod]
        public async Task EnsureProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.WelcomePage);
                var web = context.Web;

                // Is the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClientException))]
        public async Task EnsurePropertiesBadLamda()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.EnsurePropertiesAsync(p => p.RootFolder.ServerRelativeUrl);
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesMultiple()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.WelcomePage, p => p.Title);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(!string.IsNullOrEmpty(web.Title));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollection()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists, p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithInclude()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.TemplateType), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                Assert.IsTrue(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.TemplateType));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.TemplateFeatureId));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesModelWithInclude()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.Title);

                Assert.IsFalse(list.IsPropertyAvailable(p => p.RootFolder));

                // Load the RootFolder property with an include
                await list.EnsurePropertiesAsync(p => p.RootFolder.QueryProperties(p => p.ServerRelativeUrl));

                Assert.IsTrue(list.IsPropertyAvailable(p => p.RootFolder));
                Assert.IsTrue(list.RootFolder.IsPropertyAvailable(p => p.ServerRelativeUrl));

                // Are other properties still not available
                Assert.IsFalse(list.IsPropertyAvailable(p => p.TemplateType));
                Assert.IsFalse(list.RootFolder.IsPropertyAvailable(p => p.IsWOPIEnabled));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithIncludePartiallyLoaded()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Title));

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.TemplateType), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Length > 0);
                Assert.IsTrue(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.TemplateType));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(web.Lists.AsRequested().First().IsPropertyAvailable(p => p.TemplateFeatureId));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesModelWithIncludePartiallyLoaded()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var list = await context.Web.Lists.GetByTitleAsync("Site Pages", p => p.Title, p => p.RootFolder);

                Assert.IsTrue(list.RootFolder.IsPropertyAvailable(p => p.ServerRelativeUrl));
                Assert.IsFalse(list.RootFolder.IsPropertyAvailable(p => p.Properties));

                // Load the RootFolder property with an include
                await list.EnsurePropertiesAsync(p => p.RootFolder.QueryProperties(p => p.Properties));

                Assert.IsTrue(list.IsPropertyAvailable(p => p.RootFolder));
                Assert.IsTrue(list.RootFolder.IsPropertyAvailable(p => p.ServerRelativeUrl));
                Assert.IsTrue(list.RootFolder.IsPropertyAvailable(p => p.Properties));

                // Are other properties still not available
                Assert.IsFalse(list.IsPropertyAvailable(p => p.TemplateType));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithIncludePartiallyLoadedRecursive()
        {
            //TestCommon.Instance.Mocking = false;            
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.WelcomePage, p => p.Lists.QueryProperties(p => p.Title, p => p.Description, p => p.Fields.QueryProperties(p => p.Title)));

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // The StaticName field property was not loaded, the query will be executed again
                await context.Web.EnsurePropertiesAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Description, p => p.Fields.QueryProperties(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(firstList.Fields.Requested);

                var firstField = firstList.Fields.AsRequested().First();
                Assert.IsTrue(firstField.IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.SchemaXml));
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 2))
            {
                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Description, p => p.Fields.QueryProperties(p => p.StaticName)), p => p.WelcomePage);

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // All properties were loaded, so we're not going back to server
                await context.Web.EnsurePropertiesAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Description, p => p.Fields.QueryProperties(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(firstList.Fields.Requested);

                var firstField = firstList.Fields.AsRequested().First();
                Assert.IsTrue(firstField.IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.SchemaXml));
            }

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 3))
            {
                await context.Web.LoadAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Description), p => p.WelcomePage);

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // The fields collection was not previously requested, so we're going back to server
                await context.Web.EnsurePropertiesAsync(p => p.Lists.QueryProperties(p => p.Title, p => p.Description, p => p.Fields.QueryProperties(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                var firstList = web.Lists.AsRequested().First();
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(firstList.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(firstList.Fields.Requested);

                var firstField = firstList.Fields.AsRequested().First();
                Assert.IsTrue(firstField.IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCssUrl));
                Assert.IsFalse(firstList.IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(firstField.IsPropertyAvailable(p => p.SchemaXml));
            }
        }
    }
}
