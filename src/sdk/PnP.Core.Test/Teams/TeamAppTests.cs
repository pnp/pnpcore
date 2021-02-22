using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamAppTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetTeamInstalledAppsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.InstalledApps);
                Assert.IsNotNull(team.InstalledApps);

                var app = team.InstalledApps.AsEnumerable().First();
                Assert.IsNotNull(app.DisplayName);
                Assert.IsNotNull(app.DistributionMethod);
                Assert.IsNotNull(app.ExternalId);
                Assert.IsNotNull(app.Id);
            }
        }

        
    }
}
