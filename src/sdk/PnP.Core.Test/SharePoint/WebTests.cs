using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using PnP.Core.Utilities;
using System.Threading.Tasks;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class WebTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetWebAllPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb webWithAllProperties = await context.Web.GetAsync(p => p.AllProperties);

                Assert.IsNotNull(webWithAllProperties);
                Assert.IsTrue(webWithAllProperties.AllProperties.Count > 0);
                Assert.AreEqual("Public", (string)webWithAllProperties.AllProperties["GroupType"]);
                Assert.AreEqual("Shared Documents", webWithAllProperties.AllProperties.AsDynamic().GroupDocumentsUrl);
            }
        }

        [TestMethod]
        public async Task GetWebRootFolderTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb webWithRootFolder = await context.Web.GetAsync(p => p.RootFolder);

                Assert.IsNotNull(webWithRootFolder);
                Assert.IsNotNull(webWithRootFolder.RootFolder);
                Assert.AreEqual("", webWithRootFolder.RootFolder.Name);
                Assert.IsTrue(context.Uri.EnsureTrailingSlash().ToString().ToLower().EndsWith(webWithRootFolder.RootFolder.ServerRelativeUrl));
                Assert.AreEqual("SitePages/Home.aspx", webWithRootFolder.RootFolder.WelcomePage);
            }
        }

        [TestMethod]
        public async Task GetWebSiteUserInfoListTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb webWithSiteInfoUserList = await context.Web.GetAsync(p => p.SiteUserInfoList);

                Assert.IsNotNull(webWithSiteInfoUserList);
                Assert.IsNotNull(webWithSiteInfoUserList.SiteUserInfoList);
                Assert.AreEqual("User Information List", webWithSiteInfoUserList.SiteUserInfoList.Title);
            }
        }
    }
}
