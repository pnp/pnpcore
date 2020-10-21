using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.QueryModel
{
    [TestClass]
    public class QueryableActualDataTeams
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestQueryTeamChannels()
        {
            var expectedDisplayName = "General";

            // TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
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
        [ExpectedException(typeof(MicrosoftGraphServiceException))]
        public async Task TestQueryTeamChannelMessages()
        {
            var expectedDisplayName = "General";

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var query = context.Team.Channels.GetByDisplayName(expectedDisplayName).Messages;

                var queryResult = query.ToList();

                Assert.IsNotNull(queryResult);
                Assert.IsTrue(queryResult.Count >= 1);
            }
        }
    }
}