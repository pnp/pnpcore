using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class FollowingTests
    {
        private const string testAccountName = "i:0#.f|membership|admin@m365x790252.onmicrosoft.com";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetMyInfoTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var info = context.Social.Following.GetFollowingInfo();

            Assert.IsNotNull(info);
        }

        [TestMethod]
        public async Task GetFollowersForTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followers = context.Social.Following.GetFollowersFor(testAccountName);

            Assert.IsNotNull(followers.Count > 0);
        }

        [TestMethod]
        public async Task GetFollowedByTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followers = context.Social.Following.GetPeopleFollowedBy(testAccountName);

            Assert.IsNotNull(followers.Count > 0);
        }

        [TestMethod]
        public async Task GetFollowedBySelectTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followers = context.Social.Following.GetPeopleFollowedBy(testAccountName, p => p.AccountName);

            Assert.IsNotNull(followers.Count > 0);
            Assert.ThrowsException<ClientException>(() => followers[0].PersonalUrl);
        }

        [TestMethod]
        public async Task AmIFollowedByTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.AmIFollowedBy(testAccountName);

            Assert.IsFalse(followed);
        }

        [TestMethod]
        public async Task AmIFollowingTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.AmIFollowing(testAccountName);

            Assert.IsFalse(followed);
        }

        [TestMethod]
        public async Task FollowSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.Follow(new FollowSiteData
            {
                ContentUri = context.Uri.AbsoluteUri
            });

            Assert.IsTrue(followed == SocialFollowResult.Ok || followed == SocialFollowResult.AlreadyFollowing);
        }

        [TestMethod]
        public async Task FollowPersonTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.Follow(new FollowPersonData
            {
                AccountName = "i:0%23.f|membership|alexw@M365x790252.onmicrosoft.com"
            });

            Assert.IsTrue(followed == SocialFollowResult.Ok || followed == SocialFollowResult.AlreadyFollowing);
        }

        [TestMethod]
        public async Task FollowDocTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.Follow(new FollowDocumentData
            {
                ContentUri = $"{context.Uri.AbsoluteUri}/Shared Documents/test.docx"
            });

            Assert.IsTrue(followed == SocialFollowResult.Ok || followed == SocialFollowResult.AlreadyFollowing);
        }

        [TestMethod]
        public async Task FollowTagTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.Follow(new FollowTagData
            {
                TagGuid = new Guid("4fd0d107-8df7-4ace-bffc-72aa0f9a736a")
            });

            Assert.IsTrue(followed == SocialFollowResult.Ok || followed == SocialFollowResult.AlreadyFollowing);
        }

        [TestMethod]
        public async Task StopFollowingSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            try
            {
                context.Social.Following.StopFollowing(new FollowSiteData
                {
                    ContentUri = context.Uri.AbsoluteUri
                });
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public async Task IsFollowedSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);
            var request = new FollowSiteData
            {
                ContentUri = context.Uri.AbsoluteUri
            };

            var followed = context.Social.Following.Follow(request);


            var result = context.Social.Following.IsFollowed(request);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task FollowedByMeTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followed = context.Social.Following.FollowedByMe(SocialActorTypes.Users | SocialActorTypes.Sites);

            Assert.IsTrue(followed.Count > 0);
        }

        [TestMethod]
        public async Task FollowedByMeCountTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var count = context.Social.Following.FollowedByMeCount(SocialActorTypes.Users | SocialActorTypes.Sites);

            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public async Task MyFollowersTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var followers = context.Social.Following.MyFollowers();

            Assert.IsTrue(followers.Count > 0);
        }

        [TestMethod]
        public async Task MySuggestionsTest()
        {
            //TestCommon.Instance.Mocking = false;

            using var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite);

            var suggestions = context.Social.Following.MySuggestions();

            Assert.IsTrue(suggestions != null);
        }
    }
}
