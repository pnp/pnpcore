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
        public void TestQueryLoadExtensionMethod()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Web.Lists
                    // .Where(l => l.Title == "Shared Documents")
                    .Load(l => l.Id, l => l.Title, l => l.Description);

                foreach (var l in query)
                {
                    Console.WriteLine(l.Title);
                }

                Assert.IsTrue(true);
                // Assert.IsNotNull(queryResult);
                // Assert.AreEqual(10, queryResult.Count);
            }
        }
   }
}