using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Test.Services.Core.CSOM.Requests;
using PnP.Core.Test.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on Authentication specifics
    /// </summary>
    [TestClass]
    public class AuthenticationTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            // TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task TestGraphAccessToken()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    PnPConstants.MicrosoftGraphBaseUri,
                    new string[] { "Group.ReadWrite.All" }).ConfigureAwait(false);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestSPOAccessToken()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    context.Uri, new string[] { "AllSites.FullControl" }).ConfigureAwait(false);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestSPOAccessTokenLive()
        {
            TestCommon.PnPCoreSDKTestUserSetup();

            using (var context = await TestCommon.Instance.GetLiveContextAsync())
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    context.Uri, new string[] { "AllSites.FullControl" }).ConfigureAwait(false);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestGraphAccessTokenLive()
        {
            TestCommon.PnPCoreSDKTestUserSetup();

            using (var context = await TestCommon.Instance.GetLiveContextAsync())
            {
                var accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(
                    PnPConstants.MicrosoftGraphBaseUri,
                    new string[] { "Group.ReadWrite.All" }).ConfigureAwait(false);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestGraphCallLive()
        {
            TestCommon.PnPCoreSDKTestUserSetup();

            using (var context = await TestCommon.Instance.GetLiveContextAsync())
            {
                context.GraphFirst = true;

                await context.Web.GetAsync(p => p.Title);
            }
        }

        [TestMethod]
        public async Task TestCSOMPlusRestCallLive()
        {
            TestCommon.PnPCoreSDKTestUserSetup();

            using (var context = await TestCommon.Instance.GetLiveContextAsync())
            {
                context.GraphFirst = false;

                var web = context.Web;

                // Get the title value via non CSOM
                await web.EnsurePropertiesAsync(p => p.Title);

                var apiCall = new ApiCall(new List<IRequest<object>>() { new GetTitleRequest() });

                var response = await (web as Web).RawRequestAsync(apiCall, HttpMethod.Post);

                Assert.IsTrue((response.ApiCall.CSOMRequests[0].Result as IWeb).Title == web.Title);
            }
        }

    }
}
