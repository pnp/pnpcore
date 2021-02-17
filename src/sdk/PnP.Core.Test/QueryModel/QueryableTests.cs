using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestQueryTake()
        {
            var expected = "$top=10";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Take(10);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQuerySkip()
        {
            var expected = "$skip=10";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Skip(10);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryOrderBy()
        {
            var expected = "$orderby=displayName";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .OrderBy(w => w.Title);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryOrderByMultiple()
        {
            var expected = "$orderby=displayName,sharepointIds";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .OrderBy(w => w.Title)
                    .OrderBy(w => w.Id);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryOrderByMultipleDirections()
        {
            var expected = "$orderby=displayName,sharepointIds desc";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .OrderBy(w => w.Title)
                    .OrderByDescending(w => w.Id);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryOrderByMultipleDirectionsLINQ()
        {
            var expected = "$orderby=displayName,sharepointIds desc";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = from w in context.Site.AllWebs
                            orderby w.Title, w.Id descending
                            select w;

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryWhereLINQ()
        {
            var expected = "$filter=displayName eq 'Test'";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test"
                             select w);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryWhereLINQWithMultipleFilters()
        {
            var expected = "$filter=(displayName eq 'Test' and sharepointIds eq (guid'69e8b219-d7af-4ac9-bc23-d382b7de985e'))";
            var filteredId = new Guid("69e8b219-d7af-4ac9-bc23-d382b7de985e");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test" && w.Id == filteredId
                             select w);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryComplexGraph()
        {
            var expected = "$filter=displayName eq 'Test'";

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test"
                             select w);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public async Task TestQueryComplexMultiWhere()
        {
            var expected = "$filter=((displayName eq 'Test' and description eq 'Description') and sharepointIds eq (guid'69e8b219-d7af-4ac9-bc23-d382b7de985e'))";
            var filteredId = new Guid("69e8b219-d7af-4ac9-bc23-d382b7de985e");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test" &&
                                w.Description == "Description" &&
                                w.Id == filteredId
                             select w);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}