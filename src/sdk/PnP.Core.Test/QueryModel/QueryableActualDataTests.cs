using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using PnP.Core.QueryModel.Query;
using System;
using PnP.Core.Test.Utilities;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Core;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTests
    {
        [TestMethod]
        public void TestQueryLists()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from l in context.Web.Lists
                             select l)
                            .Load(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 5);
            }
        }

        [TestMethod]
        public void TestQueryItems()
        {
            var expectedListItemTitle = "Home";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from i in context.Web.Lists.GetByTitle("Site Pages").Items
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
        public void TestQueryFirstOrDefaultNoPredicateLINQ()
        {
            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var actual = context.Web.Lists.FirstOrDefault();

                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        public void TestQueryFirstOrDefaultWithPredicateLINQ()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var actual = (from l in context.Web.Lists
                              select l)
                             .Load(l => l.Id, l => l.Title)
                             .FirstOrDefault(l => l.Title == expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryFirstOrDefaultNoPredicateOnQueryLINQ()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var actual = (from l in context.Web.Lists
                              where l.Title == expected
                              select l).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryGetByTitleLINQ()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var actual = context.Web.Lists.GetByTitle(expected);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual.Title);
            }
        }

        [TestMethod]
        public void TestQueryGetByTitleWithFieldsLINQ()
        {
            var expected = "Documents";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
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
        public void TestQueryGetByIdLINQ()
        {
            var targetListTitle = "Site Pages";
            var expectedTitle = "Home";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var sitePages = context.Web.Lists.GetByTitle(targetListTitle);
                var firstItem = sitePages.Items.GetById(1, 
                    i => i.Id, 
                    i => i.Title);

                Assert.IsNotNull(firstItem);
                Assert.AreEqual(1, firstItem.Id);
                Assert.AreEqual(firstItem.Title, expectedTitle);
            }
        }
    }
}