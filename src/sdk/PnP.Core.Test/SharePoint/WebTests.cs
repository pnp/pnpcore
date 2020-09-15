using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using PnP.Core.Utilities;
using System.Linq;
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
        public async Task GetWebSimpleProperties_A_G_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(
                    p => p.AccessRequestListUrl,
                    p => p.AccessRequestSiteDescription,
                    p => p.AllowCreateDeclarativeWorkflowForCurrentUser,
                    p => p.AllowDesignerForCurrentUser,
                    p => p.AllowMasterPageEditingForCurrentUser,
                    p => p.AllowRevertFromTemplateForCurrentUser,
                    p => p.AllowRssFeeds,
                    p => p.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser,
                    p => p.AllowSavePublishDeclarativeWorkflowForCurrentUser,
                    p => p.AlternateCssUrl,
                    p => p.AppInstanceId,
                    p => p.ClassicWelcomePage,
                    p => p.ContainsConfidentialInfo,
                    p => p.Created,
                    p => p.CustomMasterUrl,
                    p => p.CustomSiteActionsDisabled,
                    // TODO Test this in Targeted Release
                    //p => p.DefaultNewPageTemplateId,
                    p => p.DesignerDownloadUrlForCurrentUser,
                    p => p.DesignPackageId,
                    p => p.DisableRecommendedItems,
                    p => p.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled,
                    p => p.EnableMinimalDownload,
                    p => p.FooterEmphasis,
                    p => p.FooterEnabled,
                    p => p.FooterLayout
                    );

                Assert.IsNotNull(web);
                Assert.IsNull(web.AccessRequestListUrl);
                Assert.AreEqual("", web.AccessRequestSiteDescription);
                Assert.IsTrue(web.AllowCreateDeclarativeWorkflowForCurrentUser);
                Assert.IsTrue(web.AllowDesignerForCurrentUser);
                Assert.IsFalse(web.AllowMasterPageEditingForCurrentUser);
                Assert.IsTrue(web.AllowRevertFromTemplateForCurrentUser);
                Assert.IsTrue(web.AllowRssFeeds);
                Assert.IsTrue(web.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser);
                Assert.IsTrue(web.AllowSavePublishDeclarativeWorkflowForCurrentUser);
                Assert.AreEqual("", web.AlternateCssUrl);
                // TODO: This one should be tested with an addin web to be relevant
                Assert.AreEqual(default, web.AppInstanceId);
                Assert.IsNull(web.ClassicWelcomePage);
                Assert.IsFalse(web.ContainsConfidentialInfo);
                Assert.IsTrue(web.CustomMasterUrl.EndsWith("/_catalogs/masterpage/seattle.master"));
                Assert.IsFalse(web.CustomSiteActionsDisabled);
                // TODO Test this in Targeted Release
                //Assert.AreNotEqual(default, web.DefaultNewPageTemplateId);
                Assert.AreEqual("https://go.microsoft.com/fwlink/?LinkId=328584&clcid=0x409", web.DesignerDownloadUrlForCurrentUser);
                Assert.AreEqual(default, web.DesignPackageId);
                Assert.IsFalse(web.DisableRecommendedItems);
                Assert.IsFalse(web.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled);
                Assert.IsFalse(web.EnableMinimalDownload);
                Assert.AreEqual(FooterVariantThemeType.Strong, web.FooterEmphasis);
                Assert.IsFalse(web.FooterEnabled);
                Assert.AreEqual(FooterLayoutType.Simple, web.FooterLayout);
            }
        }

        // See discussion https://github.com/pnp/pnpcore/discussions/111#discussioncomment-76156
        //[TestMethod]
        //public async Task GetWebAuthorTest()
        //{
        //    TestCommon.Instance.Mocking = false;
        //    using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
        //    {
        //        IWeb webWithAuthor = await context.Web.GetAsync(p => p.Author);

        //        Assert.IsNotNull(webWithAuthor);
        //        Assert.IsNotNull(webWithAuthor.Author);
        //        Assert.AreNotEqual(0, webWithAuthor.Author.SharePointId);
        //    }
        //}


        [TestMethod]
        public async Task GetWebSimpleProperties_H_M_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(
                    p => p.HeaderEmphasis,
                    p => p.HeaderLayout,
                    p => p.HideTitleInHeader,
                    p => p.IsHomepageModernized,
                    p => p.IsProvisioningComplete,
                    p => p.IsRevertHomepageLinkHidden,
                    p => p.Language,
                    p => p.LastItemModifiedDate,
                    p => p.LastItemUserModifiedDate,
                    p => p.LogoAlignment,
                    p => p.MasterUrl,
                    p => p.MegaMenuEnabled
                    );

                Assert.IsNotNull(web);
                Assert.IsFalse(web.IsHomepageModernized);
                Assert.IsTrue(web.IsProvisioningComplete);
                Assert.IsFalse(web.IsRevertHomepageLinkHidden);
                Assert.AreEqual(1033, web.Language);
                Assert.AreNotEqual(default, web.LastItemModifiedDate);
                Assert.AreNotEqual(default, web.LastItemUserModifiedDate);
                Assert.AreEqual(LogoAlignment.Left, web.LogoAlignment);
                Assert.AreNotEqual("", web.MasterUrl);
                Assert.IsFalse(web.MegaMenuEnabled);
            }
        }

        [TestMethod]
        public async Task GetWebSimpleProperties_N_S_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(
                    p => p.NavAudienceTargetingEnabled,
                    p => p.NextStepsFirstRunEnabled,
                    p => p.NotificationsInOneDriveForBusinessEnabled,
                    p => p.NotificationsInSharePointEnabled,
                    p => p.ObjectCacheEnabled,
                    p => p.PreviewFeaturesEnabled,
                    p => p.PrimaryColor,
                    p => p.RecycleBinEnabled,
                    p => p.SaveSiteAsTemplateEnabled,
                    p => p.SearchBoxPlaceholderText,
                    p => p.ServerRelativeUrl,
                    p => p.ShowUrlStructureForCurrentUser,
                    p => p.SiteLogoDescription,
                    p => p.SiteLogoUrl,
                    p => p.SyndicationEnabled
                    );

                Assert.IsNotNull(web);
                Assert.IsFalse(web.NavAudienceTargetingEnabled);
                Assert.IsTrue(web.NextStepsFirstRunEnabled);
                Assert.IsTrue(web.NotificationsInOneDriveForBusinessEnabled);
                Assert.IsTrue(web.NotificationsInSharePointEnabled);
                Assert.IsFalse(web.ObjectCacheEnabled);
                Assert.IsTrue(web.PreviewFeaturesEnabled);
                Assert.AreNotEqual("", web.PrimaryColor);
                Assert.IsTrue(web.RecycleBinEnabled);
                Assert.IsTrue(web.SaveSiteAsTemplateEnabled);
                Assert.IsNull(web.SearchBoxPlaceholderText);
                Assert.AreNotEqual("", web.ServerRelativeUrl);
                Assert.IsTrue(web.ShowUrlStructureForCurrentUser);
                Assert.AreEqual("", web.SiteLogoDescription);
                Assert.AreNotEqual("", web.SiteLogoUrl);
            }
        }

        [TestMethod]
        public async Task GetWebSimpleProperties_T_Z_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(
                    p => p.TenantAdminMembersCanShare,
                    p => p.TenantTagPolicyEnabled,
                    // TODO Review this one, it causes SP REST to return an error
                    //p => p.ThemeData,
                    p => p.ThemedCssFolderUrl,
                    p => p.ThirdPartyMdmEnabled,
                    p => p.TreeViewEnabled,
                    p => p.UIVersion,
                    p => p.UIVersionConfigurationEnabled,
                    p => p.UseAccessRequestDefault,
                    p => p.WebTemplate,
                    p => p.WebTemplateConfiguration,
                    p => p.WebTemplatesGalleryFirstRunEnabled
                    );

                Assert.IsNotNull(web);
                Assert.AreEqual(0, web.TenantAdminMembersCanShare);
                // TODO Review this one, it causes SP REST to return an error
                //Assert.AreNotEqual("", web.ThemeData);
                Assert.IsNull(web.ThemedCssFolderUrl);
                Assert.IsFalse(web.ThirdPartyMdmEnabled);
                Assert.IsFalse(web.TreeViewEnabled);
                Assert.AreNotEqual(0, web.UIVersion);
                Assert.IsFalse(web.UIVersionConfigurationEnabled);
                Assert.IsTrue(web.UseAccessRequestDefault);
                Assert.AreNotEqual(0, web.UIVersion);
                Assert.AreEqual("GROUP", web.WebTemplate);
                Assert.AreEqual("GROUP#0", web.WebTemplateConfiguration);
                Assert.IsFalse(web.WebTemplatesGalleryFirstRunEnabled);
            }
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
                // Don't do this check as context.Uri maps to the site configured in the developers setup while webWithRootFolder.RootFolder.ServerRelativeUrl maps to the site name used
                // while generating the off line file
                //Assert.IsTrue(context.Uri.EnsureTrailingSlash().ToString().ToLower().EndsWith(webWithRootFolder.RootFolder.ServerRelativeUrl));
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

        [TestMethod]
        public async Task GetWebAvailableContentTypesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.AvailableContentTypes);

                Assert.IsNotNull(web);
                Assert.IsTrue(web.AvailableContentTypes.Requested);
                Assert.IsTrue(web.AvailableContentTypes.Length > 0);
            }
        }

        [TestMethod]
        public async Task GetWebAvailableFieldsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.AvailableFields);

                Assert.IsNotNull(web);
                Assert.IsTrue(web.AvailableFields.Requested);
                Assert.IsTrue(web.AvailableFields.Length > 0);
            }
        }

    }
}
