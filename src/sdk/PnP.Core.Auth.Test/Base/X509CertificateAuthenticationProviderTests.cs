using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Auth.Test.Utilities;
using PnP.Core.Model;
using PnP.Core.Services;
using System.Threading.Tasks;

namespace PnP.Core.Auth.Test.Base
{
    /// <summary>
    /// Tests that focus on validating the X509CertificateAuthenticationProvider
    /// </summary>
    [TestClass]
    public class X509CertificateAuthenticationProviderTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // NOOP so far
        }

        [TestMethod]
        public async Task TestX509CertificateWithGraph()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context);
            }
        }

        [TestMethod]
        public async Task TestX509CertificateWithSPO()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSiteX509Certificate))
            {
                await TestCommon.CheckAccessToTargetResource(context, false);
            }
        }
    }
}
