using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using PnP.Core.QueryModel.Model;
using PnP.Core.Model.SharePoint;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class FunctionsTests
    {
        [TestMethod]
        public void TestWhereMethodNotSupported()
        {
            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.IndexOf("Value") == 0)
                as QueryableDataModelCollection<IListItem>;

            Assert.ThrowsException<NotSupportedException>(() => query.ToString());
        }

        [TestMethod]
        public void TestWhereMemberNotSupported()
        {
            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Ticks == 0)
                as QueryableDataModelCollection<IListItem>;

            Assert.ThrowsException<NotSupportedException>(() => query.ToString());
        }

        [TestMethod]
        public void TestWhereContains()
        {
            var expected = "$filter=substringof('Value',Title) eq true";

            var query = new ListItemCollection(null, null)
                .Where(i => i.Title.Contains("Value"));

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereContainsEquals()
        {
            var expected = "$filter=substringof('Value',Title) eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.Contains("Value") == true);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereStartsWith()
        {
            var expected = "$filter=startswith(Title,'Value') eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.StartsWith("Value"));

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereDay()
        {
            var expected = "$filter=day(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Day == 1);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereMonth()
        {
            var expected = "$filter=month(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Month == 1);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereYear()
        {
            var expected = "$filter=year(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Year == 1);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereMinute()
        {
            var expected = "$filter=minute(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Minute == 1);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }

        [TestMethod]
        public void TestWhereSecond()
        {
            var expected = "$filter=second(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Second == 1);

            var result = query.ToString();
            Assert.AreEqual(result, expected);
        }
    }
}