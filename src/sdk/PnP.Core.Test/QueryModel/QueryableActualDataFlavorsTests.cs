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
        public async Task TestQueryPropertiesMultipleBehaviors()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                // QueryProperties with LoadAsync
                await context.Web.LoadAsync(w => w.Title, 
                    w => w.Description, 
                    w => w.Lists.QueryProperties(l => l.Id, l => l.Title));

                Assert.IsNotNull(context.Web.Lists);
                Assert.IsTrue(context.Web.Lists.Length > 0);

                // QueryProperties with GetAsync
                var web1 = await context.Web.GetAsync(w => w.Title,
                    w => w.Description,
                    w => w.Lists.QueryProperties(l => l.Id, l => l.Title));

                Assert.IsNotNull(web1.Lists);
                Assert.IsTrue(web1.Lists.Length > 0);

                // Nested QueryProperties
                var lists1 = context.Web.Lists.QueryProperties(l => l.Id,
                        l => l.Title,
                        l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.Description)
                    );

                foreach (var l in lists1)
                {
                    Assert.IsNotNull(l);
                }

                // Nested QueryProperties with Where
                var lists2 = context.Web.Lists.Where(l => l.Title == "Documents")
                    .QueryProperties(l => l.Id,
                        l => l.Title,
                        l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.Description));

                foreach (var l in lists2)
                {
                    Assert.IsNotNull(l);
                }

                // Nested QueryProperties with FirstOrDefault
                var list = context.Web.Lists.QueryProperties(l => l.Id,
                    l => l.Title,
                    l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.Description)
                    ).FirstOrDefault(l => l.Title == "Documents");

                Assert.IsNotNull(list);

                // Nested QueryProperties with Take
                var lists = context.Web.Lists.QueryProperties(l => l.Id,
                    l => l.Title,
                    l => l.ContentTypes.QueryProperties(ct => ct.Id, ct => ct.Description)
                    ).Take(2).ToArray();

                Assert.IsNotNull(lists);

                // AsBatchAsync
                var queryBatchAsync = await context.Web.Lists
                    .Where(l => l.Title == "Documents")
                    .QueryProperties(l => l.Title, l => l.TemplateType)
                    .AsBatchAsync();
                Assert.IsFalse(queryBatchAsync.IsAvailable);

                await context.ExecuteAsync();
                Assert.IsTrue(queryBatchAsync.IsAvailable);
                foreach (var l in queryBatchAsync)
                {
                    Assert.IsNotNull(l);
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.Title));
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.TemplateType));
                }

                // AsBatch
                var queryBatch = context.Web.Lists
                    .Where(l => l.Title == "Documents")
                    .QueryProperties(l => l.Title, l => l.TemplateType)
                    .AsBatch();
                Assert.IsFalse(queryBatch.IsAvailable);

                context.Execute();
                Assert.IsTrue(queryBatch.IsAvailable);
                foreach (var l in queryBatch)
                {
                    Assert.IsNotNull(l);
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.Title));
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.TemplateType));
                }

                // With a specified batch
                var batch = context.NewBatch();

                // AsBatchAsync
                queryBatchAsync = await context.Web.Lists
                    .Where(l => l.Title == "Documents")
                    .QueryProperties(l => l.Title, l => l.TemplateType)
                    .AsBatchAsync(batch);
                Assert.IsFalse(queryBatchAsync.IsAvailable);

                await context.ExecuteAsync(batch);
                Assert.IsTrue(queryBatchAsync.IsAvailable);
                foreach (var l in queryBatchAsync)
                {
                    Assert.IsNotNull(l);
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.Title));
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.TemplateType));
                }

                // AsBatch
                queryBatch = context.Web.Lists
                    .Where(l => l.Title == "Documents")
                    .QueryProperties(l => l.Title, l => l.TemplateType)
                    .AsBatch(batch);
                Assert.IsFalse(queryBatch.IsAvailable);

                context.Execute(batch);
                Assert.IsTrue(queryBatch.IsAvailable);
                foreach (var l in queryBatch)
                {
                    Assert.IsNotNull(l);
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.Title));
                    Assert.IsTrue(l.IsPropertyAvailable(l => l.TemplateType));
                }

                //var item = context.Web.Lists.GetByTitle("Site Pages").Items.GetById(1);

                //var pnpTab = context.Team.PrimaryChannel.Tabs.FirstOrDefault(p => p.DisplayName == "PnPTab");
                //if (pnpTab != null)
                //{
                //    var t = pnpTab.DisplayName;
                //}
            }
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
            // TestCommon.Instance.Mocking = false;
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

        [TestMethod]
        public async Task TestLINQQueryPropertiesExceptionViaSPORest()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.ThrowsException<InvalidOperationException>(() =>
                {
                    var query = context.Web.AssociatedOwnerGroup.QueryProperties(p => p.Title);
                });                               
            }
        }
    }
}