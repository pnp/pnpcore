using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
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

        //[TestMethod]
        //public async Task GetFunSettings()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var team = await context.Team.GetAsync(x => x.FunSettings);
        //        Assert.IsNotNull(team.FunSettings);
        //    }
        //}

        //[TestMethod]
        //public async Task UpdateMessagingSettings()
        //{
        //    //TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        var team = await context.Team.GetAsync(x => x.MessagingSettings);

        //        team.MessagingSettings.AllowChannelMentions = true;
        //        team.MessagingSettings.AllowOwnerDeleteMessages = true;
        //        team.MessagingSettings.AllowTeamMentions = true;
        //        team.MessagingSettings.AllowUserDeleteMessages = true;
        //        team.MessagingSettings.AllowUserEditMessages = true;

        //        await team.UpdateAsync();

        //        team = await context.Team.GetAsync(x => x.MessagingSettings);

        //        Assert.IsTrue(team.MessagingSettings.AllowChannelMentions);
        //        Assert.IsTrue(team.MessagingSettings.AllowOwnerDeleteMessages);
        //        Assert.IsTrue(team.MessagingSettings.AllowTeamMentions);
        //        Assert.IsTrue(team.MessagingSettings.AllowUserDeleteMessages);
        //        Assert.IsTrue(team.MessagingSettings.AllowUserEditMessages);
        //    }
        //}

        [TestMethod]
        public async Task GetChannelsAsyncTest()
        {
            TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(o => o.Channels);
                Assert.IsTrue(team.Channels.Length > 0);
            }
        }
    }
}
