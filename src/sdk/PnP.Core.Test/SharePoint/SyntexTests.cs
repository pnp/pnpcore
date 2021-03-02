using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SyntexTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task IsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsFalse(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task IsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;
            
            if (!TestCommon.Instance.Mocking && string.IsNullOrEmpty(TestCommon.SyntexContentCenterTestSite)) Assert.Inconclusive("No Syntex Content Center setup for live testing");

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                Assert.IsTrue(await context.Web.IsSyntexContentCenterAsync());
                Assert.IsTrue(context.Web.IsSyntexContentCenter());
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterNegative()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsTrue(await context.Web.AsSyntexContentCenterAsync() == null);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() == null);
            }
        }

        [TestMethod]
        public async Task AsSyntexContentCenterPositive()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.SyntexContentCenterTestSite))
            {
                var cc = await context.Web.AsSyntexContentCenterAsync();
                Assert.IsTrue(cc != null);
                Assert.IsTrue(cc.Id == context.Web.Id);
                Assert.IsTrue(context.Web.AsSyntexContentCenter() != null);
            }
        }
    }
}
