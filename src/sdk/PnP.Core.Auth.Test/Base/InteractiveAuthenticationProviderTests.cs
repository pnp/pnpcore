using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the InteractiveAuthenticationProvider
    /// </summary>
    [TestClass]
    public class InteractiveAuthenticationProviderTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestInteractiveWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteInteractive))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestInteractiveWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteInteractive))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }
    }
}
