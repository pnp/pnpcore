using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using System;
using System.Linq;

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
        public void TestWhereMethodCallNotSupported()
        {
            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.Equals("Value"))
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
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereContainsEquals()
        {
            var expected = "$filter=substringof('Value',Title) eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.Contains("Value") == true);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereContainsVarEquals()
        {
            var expected = "$filter=substringof('Value',Title) eq true";

            var trueVar = true;

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.Contains("Value") == trueVar);

            var result = query.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWhereContainsFalseEquals()
        {
            var expected = "$filter=substringof('Value',Title) eq false";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.Contains("Value") == false);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereContainsUnaryEquals()
        {
            var expected = "$filter=substringof('Value',Title) eq false";

            var query = new ListItemCollection(null, null)
                    .Where(i => !i.Title.Contains("Value"));

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereBoolConditionEquals()
        {
            var expected = "$filter=HasUniqueRoleAssignments eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.HasUniqueRoleAssignments == true);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereBoolNotConditionEquals()
        {
            var expected = "$filter=HasUniqueRoleAssignments eq false";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.HasUniqueRoleAssignments == false);

            var result = query.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWhereBoolShortConditionEquals()
        {
            var expected = "$filter=HasUniqueRoleAssignments eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.HasUniqueRoleAssignments);

            var result = query.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWhereBoolShortUnaryConditionEquals()
        {
            var expected = "$filter=HasUniqueRoleAssignments eq false";

            var query = new ListItemCollection(null, null)
                    .Where(i => !i.HasUniqueRoleAssignments);

            var result = query.ToString();
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestWhereStartsWith()
        {
            var expected = "$filter=startswith(Title,'Value') eq true";

            var query = new ListItemCollection(null, null)
                    .Where(i => i.Title.StartsWith("Value"));

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereDay()
        {
            var expected = "$filter=day(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Day == 1);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereMonth()
        {
            var expected = "$filter=month(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Month == 1);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereYear()
        {
            var expected = "$filter=year(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Year == 1);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereMinute()
        {
            var expected = "$filter=minute(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Minute == 1);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }

        [TestMethod]
        public void TestWhereSecond()
        {
            var expected = "$filter=second(Date) eq 1";

            var query = new ListItemCollection(null, null)
                    .Where(i => ((DateTime)i.Values["Date"]).Second == 1);

            var result = query.ToString();
            Assert.AreEqual(expected,result);
        }
    }
}