using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the dynamic configuration for the various Authentication Providers
    /// </summary>
    [TestClass]
    public class AuthenticationConfigurationTests
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
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestUsernamePasswordWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteUsernamePassword))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestCredentialManagerWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteCredentialManager))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        //[TestMethod]
        //public async Task TestOnBehalfOfWithGraph()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestOnBehalfOfWithSPO()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteOnBehalfOf))
        //    {
        //        await CheckAccessToTargetResource(context, false);
        //    }
        //}

        //[TestMethod]
        //public async Task TestAspNetCoreWithGraph()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await CheckAccessToTargetResource(context);
        //    }
        //}

        //[TestMethod]
        //public async Task TestAspNetCoreWithSPO()
        //{
        //    if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
        //
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteAspNetCore))
        //    {
        //        await CheckAccessToTargetResource(context, false);
        //    }
        //}

        [TestMethod]
        public async Task TestX509CertificateWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await CheckAccessToTargetResource(context, false);
            }
        }

        private static async Task CheckAccessToTargetResource(PnPContext context, bool graphFirst = true)
        {
            context.GraphFirst = graphFirst;
            var web = await context.Web.GetAsync(w => w.Title);

            Assert.IsNotNull(web.Title);
            Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));
            
            // Can't assume title values as not everyone will use the same sites in their live test environment
            //Assert.AreEqual("pnpcoresdkdemo", web.Title);
        }
    }
}
