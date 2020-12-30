using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Teams;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Test.Teams
{
    [TestClass]
    public class TeamSettingsTest
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetFunSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.FunSettings);
                Assert.IsNotNull(team.FunSettings);
            }
        }

        [TestMethod]
        public async Task GetClassSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.ClassSettings);
                Assert.IsNotNull(team.ClassSettings);
            }
        }

        [TestMethod]
        public async Task GetClassificationSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Classification);
                Assert.IsTrue(team.IsPropertyAvailable(t => t.Classification));
                Assert.IsNull(team.Classification); // Does not return value even when set
            }
        }

        [TestMethod]
        public async Task GetDescriptionSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Description);
                Assert.IsNotNull(team.Description);
            }
        }

        [TestMethod]
        public async Task GetInternalIdSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.InternalId);
                Assert.IsNotNull(team.InternalId);
            }
        }

        [TestMethod]
        public async Task GetSpecializationSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Specialization);
                Assert.IsNotNull(team.Specialization);
            }
        }

        [TestMethod]
        public async Task GetVisibilitySettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.Visibility);
                Assert.IsNotNull(team.Visibility);
            }
        }

        [TestMethod]
        public async Task GetWebUrlSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.WebUrl);
                Assert.IsNotNull(team.WebUrl);
            }
        }

        [TestMethod]
        public async Task GetGuestSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.GuestSettings);
                Assert.IsNotNull(team.GuestSettings);
            }
        }

        [TestMethod]
        public async Task GetDiscoverySettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.DiscoverySettings);
                Assert.IsNotNull(team.DiscoverySettings);
            }
        }

        [TestMethod]
        public async Task GetMessagingSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.MessagingSettings);
                Assert.IsNotNull(team.MessagingSettings);
            }
        }

        [TestMethod]
        public async Task GetMemberSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.MemberSettings);
                Assert.IsNotNull(team.MemberSettings);
            }
        }

        [TestMethod]
        public async Task UpdateFunSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.FunSettings);

                team.FunSettings.GiphyContentRating = TeamGiphyContentRating.Moderate;
                team.FunSettings.AllowStickersAndMemes = true;
                team.FunSettings.AllowCustomMemes = true;
                team.FunSettings.AllowGiphy = true;

                await team.UpdateAsync();

                team = await context.Team.GetAsync(x => x.FunSettings);

                Assert.AreEqual(team.FunSettings.GiphyContentRating, TeamGiphyContentRating.Moderate);
                Assert.IsTrue(team.FunSettings.AllowStickersAndMemes);
                Assert.IsTrue(team.FunSettings.AllowCustomMemes);
                Assert.IsTrue(team.FunSettings.AllowGiphy);
            }
        }

        [TestMethod]
        public async Task UpdateGuestSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.GuestSettings);

                team.GuestSettings.AllowDeleteChannels = true;
                team.GuestSettings.AllowCreateUpdateChannels = true;

                await team.UpdateAsync();

                team = await context.Team.GetAsync(x => x.GuestSettings);

                Assert.IsTrue(team.GuestSettings.AllowDeleteChannels);
                Assert.IsTrue(team.GuestSettings.AllowCreateUpdateChannels);
            }
        }

        /* TEMP IN COMMENTS
         * This also does not work anymore via Graph Explorer...
        [TestMethod]
        public async Task UpdateDiscoverySettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.DiscoverySettings);

                team.DiscoverySettings.ShowInTeamsSearchAndSuggestions = true;

                await team.UpdateAsync();

                team = await context.Team.GetAsync(x => x.DiscoverySettings);

                Assert.IsTrue(team.DiscoverySettings.ShowInTeamsSearchAndSuggestions);
            }
        }
        */

        [TestMethod]
        public async Task UpdateMessagingSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.MessagingSettings);

                team.MessagingSettings.AllowChannelMentions = true;
                team.MessagingSettings.AllowOwnerDeleteMessages = true;
                team.MessagingSettings.AllowTeamMentions = true;
                team.MessagingSettings.AllowUserDeleteMessages = true;
                team.MessagingSettings.AllowUserEditMessages = true;

                await team.UpdateAsync();

                team = await context.Team.GetAsync(x => x.MessagingSettings);

                Assert.IsTrue(team.MessagingSettings.AllowChannelMentions);
                Assert.IsTrue(team.MessagingSettings.AllowOwnerDeleteMessages);
                Assert.IsTrue(team.MessagingSettings.AllowTeamMentions);
                Assert.IsTrue(team.MessagingSettings.AllowUserDeleteMessages);
                Assert.IsTrue(team.MessagingSettings.AllowUserEditMessages);
            }
        }

        [TestMethod]
        public async Task UpdateMemberSettings()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(x => x.MemberSettings);

                team.MemberSettings.AllowAddRemoveApps = true;
                team.MemberSettings.AllowCreatePrivateChannels = true;
                team.MemberSettings.AllowCreateUpdateChannels = true;
                team.MemberSettings.AllowCreateUpdateRemoveConnectors = true;
                team.MemberSettings.AllowCreateUpdateRemoveTabs = true;
                team.MemberSettings.AllowDeleteChannels = true;

                await team.UpdateAsync();

                team = await context.Team.GetAsync(x => x.MemberSettings);

                Assert.IsTrue(team.MemberSettings.AllowAddRemoveApps);
                Assert.IsTrue(team.MemberSettings.AllowCreatePrivateChannels);
                Assert.IsTrue(team.MemberSettings.AllowCreateUpdateChannels);
                Assert.IsTrue(team.MemberSettings.AllowCreateUpdateRemoveConnectors);
                Assert.IsTrue(team.MemberSettings.AllowCreateUpdateRemoveTabs);
                Assert.IsTrue(team.MemberSettings.AllowDeleteChannels);
            }
        }
    }
}
