using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
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
        private static readonly string WebTitleCsom = "<Request AddExpandoFieldTypeSuffix=\"true\" SchemaVersion=\"15.0.0.0\" LibraryVersion=\"16.0.0.0\" ApplicationName=\".NET Library\" xmlns=\"http://schemas.microsoft.com/sharepoint/clientquery/2009\"><Actions><ObjectPath Id=\"2\" ObjectPathId=\"1\" /><ObjectPath Id=\"4\" ObjectPathId=\"3\" /><Query Id=\"5\" ObjectPathId=\"3\"><Query SelectAllProperties=\"false\"><Properties><Property Name=\"Title\" ScalarProperty=\"true\" /></Properties></Query></Query></Actions><ObjectPaths><StaticProperty Id=\"1\" TypeId=\"{3747adcd-a3c3-41b9-bfab-4a64dd2f1e0a}\" Name=\"Current\" /><Property Id=\"3\" ParentId=\"1\" Name=\"Web\" /></ObjectPaths></Request>";

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
                    new string[] { "Group.ReadWrite.All" }).ConfigureAwait(true);

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
                    context.Uri, new string[] { "AllSites.FullControl" }).ConfigureAwait(true);

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
                    context.Uri, new string[] { "AllSites.FullControl" }).ConfigureAwait(true);

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
                    new string[] { "Group.ReadWrite.All" }).ConfigureAwait(true);

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

                var apiCall = new ApiCall(WebTitleCsom);

                var response = await (web as Web).RawRequestAsync(apiCall, HttpMethod.Post);

                Assert.IsTrue(response.CsomResponseJson.Count > 0);
                Assert.IsTrue(response.CsomResponseJson[5].GetProperty("Title").GetString() == web.Title);
            }
        }
    }
}
