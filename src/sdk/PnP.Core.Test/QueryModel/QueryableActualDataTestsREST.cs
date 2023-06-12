using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTestsREST
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestQueryWebs_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var query = context.Site.AllWebs
                            .QueryProperties(w => w.Id, w => w.Title, w => w.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count > 0);
            }
        }

        [TestMethod]
        public async Task TestQueryWebsAsync_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var query = context.Site.AllWebs
                    .QueryProperties(w => w.Id, w => w.Title, w => w.Description);

                var queryResult = await query.ToListAsync();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count > 0);
            }
        }

        [TestMethod]
        public async Task TestQueryLists_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var query = (from l in context.Web.Lists
                             select l)
                            .QueryProperties(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 5);
            }
        }

        [TestMethod]
        public async Task TestQueryListsAsync_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var query = (from l in context.Web.Lists
                             select l)
                            .QueryProperties(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = await query.ToListAsync();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 5);
            }
        }

        [TestMethod]
        public async Task TestQueryItems_REST()
        {
            // TestCommon.Instance.Mocking = false;
            var expectedListItemTitle = "Home";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var query = (from i in context.Web.Lists.GetByTitle("Site Pages").Items
                             where i.Title == expectedListItemTitle
                             select i)
                             .QueryProperties(l => l.Id, l => l.Title);

                var queryResult = query.ToList();

                // Ensure that we have 1 item in the result and that its title is the expected one
                Assert.IsNotNull(queryResult);
                Assert.AreEqual(1, queryResult.Count);
                Assert.AreEqual(expectedListItemTitle, queryResult[0].Title);
            }
        }

        [TestMethod]
        public async Task TestQueryItemsAsync_REST()
        {
            // TestCommon.Instance.Mocking = false;

            var expectedListItemTitle = "Home";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                IList list = await context.Web.Lists.GetByTitleAsync("Site Pages");
                var query = (from i in list.Items
                             where i.Title == expectedListItemTitle
                             select i)
                    .QueryProperties(l => l.Id, l => l.Title);

                var queryResult = await query.ToListAsync();

                // Ensure that we have 1 item in the result and that its title is the expected one
                Assert.IsNotNull(queryResult);
                Assert.AreEqual(1, queryResult.Count);
                Assert.AreEqual(expectedListItemTitle, queryResult[0].Title);
            }
        }

        [TestMethod]
        public async Task TestQueryItemsFilterOnDateAsync_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var dt = DateTime.UtcNow;
                var lists = await context.Web.Lists.Where(p => p.Title == "Site Pages" && p.LastItemModifiedDate <= dt).ToListAsync();                
                Assert.IsTrue(lists.Count == 1);

                var dto = DateTimeOffset.UtcNow;
                lists = await context.Web.Lists.Where(p => p.Title == "Site Pages" && p.LastItemModifiedDate <= dto).ToListAsync();
                Assert.IsTrue(lists.Count == 1);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateLINQ_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = context.Web.Lists.FirstOrDefault();

                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateLINQAsync_REST()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = await context.Web.Lists.FirstOrDefaultAsync();

                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultWithPredicateLINQ_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = (from l in context.Web.Lists
                              select l)
                             .QueryProperties(l => l.Id, l => l.Title)
                             .FirstOrDefault(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultWithPredicateLINQAsync_REST()
        {
            var expected = "Documents";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = await context.Web.Lists.QueryProperties(l => l.Id, l => l.Title).FirstOrDefaultAsync(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateOnQueryLINQ_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = (from l in context.Web.Lists
                              where l.Title == expected
                              select l).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryFirstOrDefaultNoPredicateOnQueryLINQAsync_REST()
        {
            var expected = "Documents";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = await context.Web.Lists.FirstOrDefaultAsync(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleLINQ_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = context.Web.Lists.GetByTitle(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleLINQAsync_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = await context.Web.Lists.GetByTitleAsync(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleWithFieldsLINQ_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = context.Web.Lists.GetByTitle(expected,
                    l => l.Id,
                    l => l.Title,
                    l => l.TemplateType
                    );

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
                Assert.AreNotEqual(Guid.Empty, actual.Id);
                Assert.AreEqual(ListTemplateType.DocumentLibrary, actual.TemplateType);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByTitleWithFieldsLINQAsync_REST()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var actual = await context.Web.Lists.GetByTitleAsync(expected,
                    l => l.Id,
                    l => l.Title,
                    l => l.TemplateType
                    );

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
                Assert.AreNotEqual(Guid.Empty, actual.Id);
                Assert.AreEqual(ListTemplateType.DocumentLibrary, actual.TemplateType);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByIdLINQ_REST()
        {
            var targetListTitle = "Site Pages";
            var expectedTitle = "Home";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.LoadAsync(w => w.Title);
                var newWeb = await context.Web.GetAsync(w => w.Title);

                var list = await context.Web.Lists.FirstOrDefaultAsync(l => l.Title == "Something");

                context.GraphFirst = false;

                var library = context.Web.Lists.GetByTitle(targetListTitle);
                var firstItem = library.Items.GetById(1);

                Assert.IsNotNull(firstItem);
                Assert.AreEqual(1, firstItem.Id);
                Assert.AreEqual(firstItem.Title, expectedTitle);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByIdLINQAsync_REST()
        {
            var targetListTitle = "Site Pages";
            var expectedTitle = "Home";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var library = await context.Web.Lists.GetByTitleAsync(targetListTitle);
                var firstItem = await library.Items.GetByIdAsync(1);

                Assert.IsNotNull(firstItem);
                Assert.AreEqual(1, firstItem.Id);
                Assert.AreEqual(firstItem.Title, expectedTitle);

                var firstItemAgain = await library.Items.GetByIdAsync(1);

                Assert.IsNotNull(firstItem);
            }
        }

        [TestMethod]
        public async Task TestQueryGetByIdLINQExceptionsAsync()
        {
            var targetListTitle = "Site Pages";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var library = await context.Web.Lists.GetByTitleAsync(targetListTitle);
                var firstItem = await library.Items.GetByIdAsync(1);

                // Not relevant now that GetById is a regular method instead of an extension method
                //Assert.ThrowsException<ArgumentNullException>(() => {
                //    IListItemCollection fakeLibraryColl = null;
                //    fakeLibraryColl.GetById(1,
                //        i => i.Id,
                //        i => i.Title);
                //});

                //await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => {
                //    IListItemCollection fakeLibraryColl = null;
                //    await fakeLibraryColl.GetByIdAsync(1);
                //});
            }
        }

        [TestMethod]
        public async Task TestQueryContentTypes_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var cts = context.Web.ContentTypes.QueryProperties(p => p.Id, p => p.Name);
                var contentTypes = cts.ToList();

                // Ensure that we a result
                Assert.IsNotNull(contentTypes);

                // Ensure that we have 1 content type in the lists of content types
                Assert.IsTrue(contentTypes.Count > 1);
            }
        }

        [TestMethod]
        public async Task TestQueryContentTypesAsync_REST()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var cts = context.Web.ContentTypes.QueryProperties(p => p.Id, p => p.Name);
                var contentTypes = await cts.ToListAsync();

                // Ensure that we a result
                Assert.IsNotNull(contentTypes);

                // Ensure that we have 1 content type in the lists of content types
                Assert.IsTrue(contentTypes.Count > 1);
            }
        }

        [TestMethod]
        public async Task TestComplexFirstOrDefault_REST()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list = context.Web.Lists.FirstOrDefault(l => !l.Hidden && l.Title.StartsWith("Site"));
                // Ensure that we have 1 content type in the lists of content types
                Assert.IsTrue(list != null);
            }
        }

        [TestMethod]
        public async Task TestHiddenLibraries_REST()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                var list = context.Web.Lists.FirstOrDefault(p => p.TemplateType == ListTemplateType.DocumentLibrary && !p.Hidden);
                Assert.IsTrue(list != null);
            }
        }
    }
}