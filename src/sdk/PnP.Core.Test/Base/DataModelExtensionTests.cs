using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.Model;
using System.Linq;

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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Is the property populated
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));

                var web = await context.Web.GetAsync(p => p.Lists.Include(p => p.Title));

                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
            }
        }

        [TestMethod]
        public async Task EnsureProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public async Task EnsurePropertiesMultiple()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollection()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists, p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Count() > 0);

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithInclude()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists.Include(p => p.Title, p => p.TemplateType), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Count() > 0);
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.TemplateType));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
                Assert.IsFalse(web.Lists.First().IsPropertyAvailable(p => p.TemplateFeatureId));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithIncludePartiallyLoaded()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                await context.Web.GetAsync(p => p.Lists.Include(p => p.Title));

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsFalse(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                await context.Web.EnsurePropertiesAsync(p => p.Lists.Include(p => p.Title, p => p.TemplateType), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
                Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(web.Lists.Count() > 0);
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.TemplateType));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
                Assert.IsFalse(web.Lists.First().IsPropertyAvailable(p => p.TemplateFeatureId));
            }
        }

        [TestMethod]
        public async Task EnsurePropertiesCollectionWithIncludePartiallyLoadedRecursive()
        {
            //TestCommon.Instance.Mocking = false;            
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                await context.Web.GetAsync(p => p.WelcomePage, p => p.Lists.Include(p => p.Title, p => p.Description, p => p.Fields.Include(p => p.Title)));

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // The StaticName field property was not loaded, the query will be executed again
                await context.Web.EnsurePropertiesAsync(p => p.Lists.Include(p => p.Title, p => p.Description, p => p.Fields.Include(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(web.Lists.First().Fields.Requested);
                Assert.IsTrue(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
                Assert.IsFalse(web.Lists.First().IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.SchemaXml));
            }

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite, 2))
            {
                await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Description, p => p.Fields.Include(p => p.StaticName)), p => p.WelcomePage);

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // All properties were loaded, so we're not going back to server
                await context.Web.EnsurePropertiesAsync(p => p.Lists.Include(p => p.Title, p => p.Description, p => p.Fields.Include(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(web.Lists.First().Fields.Requested);
                Assert.IsTrue(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
                Assert.IsFalse(web.Lists.First().IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.SchemaXml));
            }        

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite, 3))
            {
                await context.Web.GetAsync(p => p.Lists.Include(p => p.Title, p => p.Description), p => p.WelcomePage);

                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.Lists));
                Assert.IsTrue(context.Web.IsPropertyAvailable(p => p.WelcomePage));

                // The fields collection was not previously requested, so we're going back to server
                await context.Web.EnsurePropertiesAsync(p => p.Lists.Include(p => p.Title, p => p.Description, p => p.Fields.Include(p => p.StaticName)), p => p.WelcomePage);
                var web = context.Web;

                // Are the property populated
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Title));
                Assert.IsTrue(web.Lists.First().IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(web.Lists.First().Fields.Requested);
                Assert.IsTrue(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.StaticName));

                // Are other properties still not available
                Assert.IsFalse(web.IsPropertyAvailable(p => p.AlternateCSS));
                Assert.IsFalse(web.Lists.First().IsPropertyAvailable(p => p.TemplateFeatureId));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.Title));
                Assert.IsFalse(web.Lists.First().Fields.First().IsPropertyAvailable(p => p.SchemaXml));
            }
        }

    }
}
