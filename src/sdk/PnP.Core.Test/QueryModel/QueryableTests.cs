using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using PnP.Core.Test.Utilities;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableTests
    {
        [TestMethod]
        public void TestQueryLoadExtensionMethod()
        {
            var expected = "$select=sharepointIds,name,description";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Load(w => w.Id, w => w.Title, w => w.Description);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQueryIncludeExtensionMethod()
        {
            var expected = "$expand=lists,lists";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Include(w => w.Lists, w => w.Lists);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQueryTake()
        {
            var expected = "$top=10";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Take(10);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQuerySkip()
        {
            var expected = "$skip=10";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .Skip(10);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQueryOrderBy()
        {
            var expected = "$orderby=name";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Site.AllWebs
                    .OrderBy(w => w.Title);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQueryOrderByMultiple()
        {
            var expected = "$orderby=name,sharepointIds";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public void TestQueryOrderByMultipleDirections()
        {
            var expected = "$orderby=name,sharepointIds desc";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public void TestQueryOrderByMultipleDirectionsLINQ()
        {
            var expected = "$orderby=name,sharepointIds desc";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public void TestQueryWhereLINQ()
        {
            var expected = "$filter=name eq 'Test'";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public void TestQueryWhereLINQWithMultipleFilters()
        {
            var expected = "$filter=(name eq 'Test' and sharepointIds eq guid('69e8b219-d7af-4ac9-bc23-d382b7de985e'))";
            var filteredId = new Guid("69e8b219-d7af-4ac9-bc23-d382b7de985e");

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
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
        public void TestQueryComplex()
        {
            var expected = "$select=sharepointIds,name,description&$filter=name eq 'Test'&$top=10&$skip=5&$expand=lists";

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test"
                             select w)
                             .Load(w => w.Id, w => w.Title, w => w.Description)
                             .Include(w => w.Lists)
                             .Take(10)
                             .Skip(5);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestQueryComplexMultiWhere()
        {
            var expected = "$select=sharepointIds,name,description&$filter=((name eq 'Test' and description eq 'Description') and sharepointIds eq guid('69e8b219-d7af-4ac9-bc23-d382b7de985e'))&$top=10&$skip=5&$expand=lists";
            var filteredId = new Guid("69e8b219-d7af-4ac9-bc23-d382b7de985e");

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from w in context.Site.AllWebs
                             where w.Title == "Test" &&
                                w.Description == "Description" &&
                                w.Id == filteredId
                             select w)
                             .Load(w => w.Id, w => w.Title, w => w.Description)
                             .Include(w => w.Lists)
                             .Take(10)
                             .Skip(5);

                var actual = query.ToString();
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}