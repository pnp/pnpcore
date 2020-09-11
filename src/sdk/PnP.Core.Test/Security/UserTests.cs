using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.Test.Utilities;
using PnP.Core.QueryModel;
using System.Threading.Tasks;

namespace PnP.Core.Test.Security
{
    [TestClass]
    public class UserTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task QuerySiteUsers()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Load the current user from current web
                IUser foundUser = await context.Web.SiteUsers.FirstOrDefaultAsync(u => u.Title == "Yannick Plenevaux");

                Assert.IsNotNull(foundUser);
                Assert.AreEqual("Yannick Plenevaux", foundUser.Title);
                Assert.AreNotEqual(0, foundUser.SharePointId);
            }
        }

        [TestMethod]
        public async Task GetCurrentUser()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Load the current user from current web
                await context.Web.GetAsync(w => w.CurrentUser);

                // TODO Must be loaded from the web first to be able to call GetAsync() (to Graph)
                // TODO Couldn't it be ensured automatically or at least checked ?

                // Get the Graph properties
                IUser user = await context.Web.CurrentUser.GetAsync();

                Assert.IsNotNull(user);
                Assert.AreNotEqual(default, user.GraphId);
                Assert.AreNotEqual(0, user.SharePointId);
            }
        }

        [TestMethod]
        public async Task GetUserByPrincipalNameAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // TODO Do we need to handle the UPN in tests to be tenant-agnostic ?
                IUser user = await context.Web.SiteUsers.GetByUserPrincipalNameAsync("dev01@pvxdev.onmicrosoft.com");

                Assert.IsNotNull(user);
                Assert.AreNotEqual(default, user.GraphId);
                Assert.IsNotNull(user.Title);
                Assert.AreNotEqual("", user.Title);
            }
        }

        [TestMethod]
        public async Task GetAlreadyEnsuredUserSharePointPropertiesWithEnsure()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // TODO Do we need to handle the UPN in tests to be tenant-agnostic ?
                IUser user = await context.Web.SiteUsers.GetByUserPrincipalNameAsync("dev01@pvxdev.onmicrosoft.com");
                await user.LoadSharePointPropertiesAsync();

                Assert.IsNotNull(user);
                Assert.AreNotEqual(default, user.GraphId);
                Assert.AreNotEqual(0, user.SharePointId);
            }
        }

        [TestMethod]
        public async Task GetNotEnsuredUserSharePointPropertiesWithEnsure()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // TODO Do we need to handle the UPN in tests to be tenant-agnostic ?
                IUser user = await context.Web.SiteUsers.GetByUserPrincipalNameAsync("dev03@pvxdev.onmicrosoft.com");
                await user.LoadSharePointPropertiesAsync();

                Assert.IsNotNull(user);
                Assert.AreNotEqual(default, user.GraphId);
                Assert.AreNotEqual(0, user.SharePointId);
            }
        }

        [TestMethod]
        public async Task GetAlreadyEnsuredUserWithoutEnsureTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUser user = await context.Web.SiteUsers.GetByUserPrincipalNameAsync("dev01@pvxdev.onmicrosoft.com");
                await user.LoadSharePointPropertiesAsync(false);

                Assert.IsNotNull(user);
                Assert.AreNotEqual(default, user.GraphId);
                Assert.AreNotEqual(0, user.SharePointId);
            }
        }

        [TestMethod]
        public async Task GetNotEnsuredUserWithoutEnsureTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IUser user = await context.Web.SiteUsers.GetByUserPrincipalNameAsync("dev02@pvxdev.onmicrosoft.com");
                await Assert.ThrowsExceptionAsync<SharePointRestServiceException>(async () =>
                {
                    await user.LoadSharePointPropertiesAsync(false);
                });
            }
        }
    }
}
