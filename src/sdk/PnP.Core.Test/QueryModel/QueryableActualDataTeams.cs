using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using PnP.Core.Test.Utilities;
using PnP.Core.Model.SharePoint;
using PnP.Core.Model.SharePoint.Core;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTeams
    {
        [TestMethod]
        public void TestQueryTeamChannels()
        {
            var expectedDisplayName = "General";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = from c in context.Team.Channels
                            where c.DisplayName == expectedDisplayName
                            select c;

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count == 1);
            }
        }

        [TestMethod]
        public void TestQueryTeamChannelMessages()
        {
            var expectedDisplayName = "General";

            // TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var query = context.Team.Channels.GetByDisplayName(expectedDisplayName).Messages;

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 1);
            }
        }
    }
}