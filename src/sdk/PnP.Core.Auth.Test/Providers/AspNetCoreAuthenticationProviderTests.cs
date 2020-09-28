using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Providers
{
    /// <summary>
    /// Tests that focus on validating the AspNetCoreAuthenticationProvider
    /// </summary>
    [TestClass]
    public class AspNetCoreAuthenticationProviderTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        //[TestMethod]
        //public async Task TestAspNetCoreWithGraph()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await TestCommon.CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestAspNetCoreWithSPO()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await TestCommon.CheckAccessToTargetResource(context, false);
        //    }
        //}
    }
}
