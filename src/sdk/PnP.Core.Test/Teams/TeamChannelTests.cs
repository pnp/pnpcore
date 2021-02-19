using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.Model;

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
                Assert.IsTrue(team.Channels.Length > 0);

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
    }
}
