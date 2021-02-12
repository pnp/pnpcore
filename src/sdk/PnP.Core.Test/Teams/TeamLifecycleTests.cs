using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.Model;

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
        public async Task ArchiveUnarchiveTeamAsync()
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

        [TestMethod]
        public void ArchiveUnarchiveTeam()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get();

                var archiveOperation = team.Archive();
                // We already updated the model
                Assert.IsTrue(team.IsArchived);

                // lets wait for the operation to complete
                archiveOperation.WaitForCompletion();

                // reload from the server
                context.Team.Get();
                // Server side should be archived as well now
                Assert.IsTrue(team.IsArchived);

                // unarchive again
                var unarchiveOperation = team.Unarchive();
                // We already updated the model
                Assert.IsFalse(team.IsArchived);

                unarchiveOperation.WaitForCompletion();

                // reload from the server
                context.Team.Get();
                // Server side should be archived as well now
                Assert.IsFalse(team.IsArchived);
            }

        }

        [TestMethod]
        public void ArchiveUnarchiveWithNotSettingReadOnlySPTeam()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get();

                var archiveOperation = team.Archive(false);
                // We already updated the model
                Assert.IsTrue(team.IsArchived);

                // lets wait for the operation to complete
                archiveOperation.WaitForCompletion();

                // reload from the server
                context.Team.Get();
                // Server side should be archived as well now
                Assert.IsTrue(team.IsArchived);

                // unarchive again
                var unarchiveOperation = team.Unarchive();
                // We already updated the model
                Assert.IsFalse(team.IsArchived);

                unarchiveOperation.WaitForCompletion();

                // reload from the server
                context.Team.Get();
                // Server side should be archived as well now
                Assert.IsFalse(team.IsArchived);
            }

        }


    }
}
