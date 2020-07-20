using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using PnP.Core.Model;
using System.Threading.Tasks;
using System.Linq.Expressions;
using PnP.Core.Services;
using System.Collections.Generic;

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
                    context.Uri, null).ConfigureAwait(true);

                Assert.IsNotNull(accessToken);
            }
        }

        [TestMethod]
        public async Task TestAccessTokenAuthenticationProvider()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string accessToken;
                // Persist the fetched token to be used in the offline test
                if (!TestCommon.Instance.Mocking)
                {
                    // First obtain a valid access token
                    accessToken = await context.AuthenticationProvider.GetAccessTokenAsync(PnPConstants.MicrosoftGraphBaseUri).ConfigureAwait(true);
                    Assert.IsNotNull(accessToken);

                    Dictionary<string, string> properties = new Dictionary<string, string>
                    {
                        { "AccessToken", accessToken }
                    };
                    TestManager.SaveProperties(context, properties);
                }
                else
                {
                    var properties = TestManager.GetProperties(context);
                    accessToken = properties["AccessToken"];
                }

                // Use this access token in a new context
                using (var context1 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAccessToken, 1))
                {
                    // Set the access token on the context
                    context1.SetAccessToken(accessToken);

                    var site = await context1.Site.GetAsync();

                    Assert.IsTrue(site.IsPropertyAvailable(p => p.Id));
                }
            }
        }
    }
}
