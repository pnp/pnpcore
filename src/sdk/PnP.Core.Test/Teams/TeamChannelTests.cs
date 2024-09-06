using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.Teams;
using PnP.Core.QueryModel;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamChannelTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetChannelsAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }

        [TestMethod]
        public void GetChannelsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }


        [TestMethod]
        public async Task GetGeneralChannelAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = await team.Channels.GetByDisplayNameAsync("General");

                Assert.AreEqual(channel.MembershipType, TeamChannelMembershipType.Standard);
                Assert.IsNotNull(channel.WebUrl);
            }
        }

        [TestMethod]
        public void GetGeneralChannelTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.GetByDisplayName("General");

                Assert.AreEqual(channel.MembershipType, TeamChannelMembershipType.Standard);
                Assert.IsNotNull(channel.WebUrl);
            }
        }

        [TestMethod]
        public async Task GetGeneralChannelAlteratveAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.AsRequested().Any());

                var channel = team.Channels.FirstOrDefault(i => i.DisplayName == "General"); ;

                Assert.IsNotNull(channel);
                Assert.AreEqual(channel.MembershipType, TeamChannelMembershipType.Standard);
                Assert.IsNotNull(channel.WebUrl);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetChannelNameExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.GetByDisplayName(null);

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetChannelNameExceptionAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = await team.Channels.GetByDisplayNameAsync("");

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddChannelNameExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.Add("");

            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddChannelNameBatchExceptionTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var team = context.Team.Get(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);

                var channel = team.Channels.AddBatch("");

            }
        }

        [TestMethod]
        public async Task GetFilesFolderFromChannel()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);

                var folder = team.Channels.AsRequested().First().GetFilesFolder(p => p.Files);

                Assert.IsNotNull(folder);
                Assert.IsTrue(folder.Requested);
            }
        }

        [TestMethod]
        public async Task GetFilesFolderFromPrivateChannel()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);

                var folder = team.Channels.AsRequested().First(p=>p.MembershipType == TeamChannelMembershipType.Private).GetFilesFolder(p => p.Files);

                Assert.IsNotNull(folder);
                Assert.IsTrue(folder.Requested);
            }
        }

        [TestMethod]
        public async Task GetFilesFolderFromSharedChannel()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);

                // TODO: update once shared channels APIs come out of beta, currently shared channels are not returned
                var folder = team.Channels.AsRequested().First(p => p.MembershipType == TeamChannelMembershipType.Private).GetFilesFolder(p => p.Files);

                Assert.IsNotNull(folder);
                Assert.IsTrue(folder.Requested);
            }
        }

        [TestMethod]
        public async Task GetFilesFolderWebUrlChannelLoad()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);

                var batch = context.NewBatch();
                foreach(var channel in team.Channels.AsRequested())
                {
                    await channel.LoadBatchAsync(batch, p => p.FilesFolderWebUrl);
                }
                var errors = await context.ExecuteAsync(batch, false);

                foreach(var channel in team.Channels.AsRequested())
                {
                    if (channel.IsPropertyAvailable(p=>p.FilesFolderWebUrl))
                    {
                        Assert.IsTrue(!string.IsNullOrEmpty(channel.FilesFolderWebUrl.ToString()));
                    }
                }

            }
        }
    }
}
