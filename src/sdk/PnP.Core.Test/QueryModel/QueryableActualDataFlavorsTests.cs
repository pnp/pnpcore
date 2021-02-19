using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataFlavorsTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestLoadViaGraph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;
                await TestLoadImplementation(context);
            }
        }

        [TestMethod]
        public async Task TestLoadViaSPORest()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                await TestLoadImplementation(context);
            }
        }

        private static async Task TestLoadImplementation(Core.Services.PnPContext context)
        {
            await context.Web.LoadAsync(w => w.Title, w => w.Description, w => w.Lists);

            Assert.IsNotNull(context.Web);
            Assert.IsNotNull(context.Web.Lists);
            Assert.IsTrue(context.Web.IsPropertyAvailable(w => w.Title));
            Assert.IsTrue(context.Web.IsPropertyAvailable(w => w.Description));
            Assert.IsTrue(context.Web.Lists.Length > 0);
        }

        [TestMethod]
        public async Task TestGetViaGraph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;
                await TestGetImplementation(context);
            }
        }

        [TestMethod]
        public async Task TestGetViaSPORest()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;
                await TestGetImplementation(context);
            }
        }

        private static async Task TestGetImplementation(Core.Services.PnPContext context)
        {
            var web = await context.Web.GetAsync(w => w.Title, w => w.Description, w => w.Lists);

            // The objects connected to the context should be empty
            Assert.IsFalse(context.Web.IsPropertyAvailable(w => w.Title));
            Assert.IsFalse(context.Web.IsPropertyAvailable(w => w.Description));
            Assert.IsTrue(context.Web.Lists.Length == 0);

            // The results of the Get method should be valid
            Assert.IsNotNull(web);
            Assert.IsTrue(web.IsPropertyAvailable(w => w.Title));
            Assert.IsTrue(web.IsPropertyAvailable(w => w.Description));
            Assert.IsNotNull(web.Lists);
            Assert.IsTrue(web.Lists.Length > 0);

            // The resulting objects should be different from the objects in the context hierarchy
            Assert.AreNotEqual(context.Web.GetHashCode(), web.GetHashCode());
            Assert.AreNotEqual(context.Web.Lists.GetHashCode(), web.Lists.GetHashCode());
        }

        [TestMethod]
        public async Task TestLINQViaGraph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var list = await context.Web.Lists.FirstOrDefaultAsync();

                // The objects connected to the context should be empty
                Assert.IsTrue(context.Web.Lists.Length == 0);

                // The results of the Get method should be valid
                Assert.IsNotNull(list);
            }
        }

        [TestMethod]
        public async Task TestLINQViaSPORest()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list = await context.Web.Lists.FirstOrDefaultAsync();

                // The objects connected to the context should be empty
                Assert.IsTrue(context.Web.Lists.Length == 0);

                // The results of the Get method should be valid
                Assert.IsNotNull(list);
            }
        }
    }
}