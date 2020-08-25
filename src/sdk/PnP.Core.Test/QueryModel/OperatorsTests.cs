using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using System.Linq;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class OperatorsTests
    {

        [TestMethod]
        public void TestEqual()
        {
            var expected = "$filter=Title eq 'title'";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title == "title");

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNotEqual()
        {
            var expected = "$filter=Title ne 'title'";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Title != "title");

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLessThen()
        {
            var expected = "$filter=Number lt 0";

            var query = new ListItemCollection(null, null)
                .Where(i => (int)i.Values["Number"] < 0);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGreaterThen()
        {
            var expected = "$filter=Number gt 0";

            var query = new ListItemCollection(null, null)
                .Where(i => (int)i.Values["Number"] > 0);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestGreaterEqual()
        {
            var expected = "$filter=Number ge 0";

            var query = new ListItemCollection(null, null)
                .Where(i => (int)i.Values["Number"] >= 0);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestLessEqual()
        {
            var expected = "$filter=Number le 0";

            var query = new ListItemCollection(null, null)
                .Where(i => (int)i.Values["Number"] <= 0);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDoubleOr()
        {
            var expected = "$filter=(id eq 1 or Title eq 'title')";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Id == 1 || i.Title == "title");

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDoubleAnd()
        {
            var expected = "$filter=(id eq 1 and Title eq 'title')";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Id == 1 && i.Title == "title");

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void TestTripleOr()
        {
            var expected = "$filter=((id eq 1 or Title eq 'title') or Number eq 0)";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Id == 1 || i.Title == "title" || (int)i.Values["Number"] == 0);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestQuadrupleOr()
        {
            var expected = "$filter=(((id eq 1 or Title eq 'title') or Number eq 0) or Number2 eq 1)";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Id == 1 || i.Title == "title" || (int)i.Values["Number"] == 0 || (int)i.Values["Number2"] == 1);

            var actual = query.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}