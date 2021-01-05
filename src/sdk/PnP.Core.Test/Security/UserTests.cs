using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.Security;
using PnP.Core.Test.Utilities;
using System.Linq;
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
        public async Task GetSharePointUsers()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.SiteUsers);

                Assert.IsTrue(web.SiteUsers.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetCurrentUser()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Load the current user from current web
                var web = await context.Web.GetAsync(w => w.CurrentUser);

                Assert.IsNotNull(web.CurrentUser);
                Assert.IsTrue(web.CurrentUser.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(web.CurrentUser.UserPrincipalName));
                Assert.IsTrue(web.CurrentUser.Id > 0);
            }
        }


        [TestMethod]
        public async Task GetSharePointUserAsGraphUser()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.SiteUsers);

                Assert.IsTrue(web.SiteUsers.Length > 0);

                // get the first real user
                var testUser = web.SiteUsers.FirstOrDefault(p => p.PrincipalType == PrincipalType.User);
                Assert.IsTrue(testUser != null);

                // Get that user as a Graph user
                var graphUser = await testUser.AsGraphUserAsync();

                Assert.IsTrue(graphUser != null);
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.Id));
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.UserPrincipalName));
                Assert.IsTrue((graphUser as GraphUser).Metadata.ContainsKey(PnPConstants.MetaDataGraphId));
                Assert.IsTrue((graphUser as GraphUser).Metadata[PnPConstants.MetaDataGraphId] == testUser.AadObjectId);
            }
        }

        [TestMethod]
        public async Task GetGraphUsersViaGroupMembership()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var team = await context.Team.GetAsync(p => p.Description, p => p.Owners, p => p.Members);

                Assert.IsTrue(team.IsPropertyAvailable(p => p.Description));
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Owners));
                Assert.IsTrue(team.Owners.Length > 0);
                Assert.IsTrue(team.IsPropertyAvailable(p => p.Members));
                Assert.IsTrue(team.Members.Length > 0);

                // Get the first owner
                var graphUser = team.Owners.FirstOrDefault();

                Assert.IsTrue(graphUser != null);
                Assert.IsTrue(!string.IsNullOrEmpty(graphUser.UserPrincipalName));

                // get sharepoint user for graph user
                var sharePointUser = await graphUser.AsSharePointUserAsync();
                Assert.IsTrue(sharePointUser != null);
                Assert.IsTrue(sharePointUser.Id > 0);
                Assert.IsTrue(sharePointUser.UserPrincipalName == graphUser.UserPrincipalName);
            }
        }
    }
}
