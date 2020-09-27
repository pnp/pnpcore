using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the OnBehalfOfAuthenticationProvider
    /// </summary>
    [TestClass]
    public class OnBehalfOfAuthenticationProviderTests
    {
        private static string onBehalfOfConfigurationPath = "onBehalfOf";

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        //[TestMethod]
        //public async Task TestOnBehalfOfWithGraph()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await TestCommon.CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestOnBehalfOfWithSPO()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await TestCommon.CheckAccessToTargetResource(context, false);
        //    }
        //}
    }
}
