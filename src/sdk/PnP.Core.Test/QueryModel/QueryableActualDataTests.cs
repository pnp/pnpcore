using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using PnP.Core.QueryModel.Query;
using System;
using PnP.Core.Test.Utilities;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTests
    {
        [TestMethod]
        public void TestQueryLists()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from l in context.Web.Lists
                            select l)
                            // .Where(l => l.Title == "Shared Documents")
                            .Load(l => l.Id, l => l.Title, l => l.Description);

                var queryResult = query.ToList();

                foreach (var l in query)
                {
                    Assert.IsNotNull(l);
                }

                //Assert.IsTrue(true);
                //Assert.IsNotNull(queryResult);
            }
        }

        [TestMethod]
        public void TestQueryItems()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = (from i in context.Web.Lists.GetByTitle("Documents").Items
                             where i.Title == "Sample Document 01"
                             select i)
                            .Load(l => l.Id, l => l.Title);

                var queryResult = query.ToList();

                foreach (var i in query)
                {
                    Assert.IsNotNull(i);
                }

                //Assert.IsTrue(true);
                //Assert.IsNotNull(queryResult);
            }
        }
    }
}