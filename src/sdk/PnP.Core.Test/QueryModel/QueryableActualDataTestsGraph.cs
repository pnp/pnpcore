using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using PnP.Core.Test.Utilities;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTestsGraph
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public void TestQueryWebs_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var query = context.Site.AllWebs
                            .Load(w => w.Id, w => w.Title, w => w.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count > 0);
            }
        }

        [TestMethod]
        public void TestQueryLists_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var query = (from l in context.Web.Lists
                             select l)
                            .Load(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 1);
            }
        }

        [TestMethod]
        public void TestQueryItems_Graph()
        {
            var expectedListItemTitle = "Sample Document 01";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var query = (from i in context.Web.Lists.GetByTitle("Documents").Items
                             where i.Title == expectedListItemTitle
                             select i)
                             .Load(l => l.Id, l => l.Title);

                var queryResult = query.ToList();

                // Ensure that we have 1 list in the collection of lists
                Assert.AreEqual(1, context.Web.Lists.Length);

                // Ensure that we have 1 item in the result and that its title is the expected one
                Assert.IsNotNull(queryResult);
                Assert.AreEqual(1, queryResult.Count);
                Assert.AreEqual(expectedListItemTitle, queryResult[0].Title);
            }
        }

        [TestMethod]
        public void TestQueryFirstOrDefaultNoPredicateLINQ_Graph()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.FirstOrDefault();

                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public void TestQueryFirstOrDefaultWithPredicateLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = (from l in context.Web.Lists
                              select l)
                             .Load(l => l.Id, l => l.Title)
                             .FirstOrDefault(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryFirstOrDefaultNoPredicateOnQueryLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = (from l in context.Web.Lists
                              where l.Title == expected
                              select l).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryGetByTitleLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.GetByTitle(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryGetByTitleWithFieldsLINQ_Graph()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var actual = context.Web.Lists.GetByTitle(expected,
                    l => l.Id,
                    l => l.Title
                    );

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
                Assert.AreNotEqual(Guid.Empty, actual.Id);
            }
        }

        [TestMethod]
        public void TestQueryGetByIdLINQ_Graph()
        {
            var targetListTitle = "Documents";
            var expectedTitle = "General";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                context.GraphFirst = true;

                var library = context.Web.Lists.GetByTitle(targetListTitle);
                var firstItem = library.Items.GetById(1, 
                    i => i.Id, 
                    i => i.Title);

                Assert.IsNotNull(firstItem);
                Assert.AreEqual(1, firstItem.Id);
                Assert.AreEqual(firstItem.Title, expectedTitle);
            }
        }
    }
}