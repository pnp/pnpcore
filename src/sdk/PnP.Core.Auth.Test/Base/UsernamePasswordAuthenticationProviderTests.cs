using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the UsernamePasswordAuthenticationProvider
    /// </summary>
    [TestClass]
    public class UsernamePasswordAuthenticationProviderTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }
    }
}
