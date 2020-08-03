using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamLifecycleTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task ArchiveUnarchiveTeam()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync();

                var archiveOperation = await team.ArchiveAsync();
                // We already updated the model
                Assert.IsTrue(team.IsArchived);

                // lets wait for the operation to complete
                await archiveOperation.WaitForCompletionAsync();

                // reload from the server
                await context.Team.GetAsync();
                // Server side should be archived as well now
                Assert.IsTrue(team.IsArchived);

                // unarchive again
                var unarchiveOperation = await team.UnarchiveAsync();
                // We already updated the model
                Assert.IsFalse(team.IsArchived);

                await unarchiveOperation.WaitForCompletionAsync();

                // reload from the server
                await context.Team.GetAsync();
                // Server side should be archived as well now
                Assert.IsFalse(team.IsArchived);
            }

        }        
    }
}
