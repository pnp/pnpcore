using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
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
                    p => p.EffectiveBasePermissions,
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

                // EffectiveBasePermissions returns a BasePermissions model
                Assert.IsTrue(web.EffectiveBasePermissions.Requested);
                Assert.IsTrue(web.EffectiveBasePermissions.High > 0);
                Assert.IsTrue(web.EffectiveBasePermissions.Low > 0);

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
                //This is not consistent, not good for use in tests
                //Assert.IsTrue(web.NextStepsFirstRunEnabled);
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
                // Not validating this property ~ this could have been manipulated on test sites causing false positives
                //Assert.IsNull(web.ThemedCssFolderUrl);
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
                Assert.IsTrue((string)webWithAllProperties.AllProperties["GroupType"] == "Private" || (string)webWithAllProperties.AllProperties["GroupType"] == "Public");
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

        [TestMethod]
        public async Task GetWebBasePermissionsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                IWeb web = await context.Web.GetAsync(p => p.EffectiveBasePermissions);

                Assert.IsNotNull(web);
                Assert.IsTrue(web.EffectiveBasePermissions.Requested);
                Assert.IsTrue(web.EffectiveBasePermissions.Low > 0);
                Assert.IsTrue(web.EffectiveBasePermissions.High > 0);
                Assert.IsTrue(web.EffectiveBasePermissions.Has(PermissionKind.AddListItems));
                Assert.IsTrue(web.EffectiveBasePermissions.HasPermissions(2147483647, 4294705151));
                Assert.IsTrue(web.EffectiveBasePermissions.Has(PermissionKind.EmptyMask));
                Assert.IsFalse(web.EffectiveBasePermissions.Has(PermissionKind.FullMask));
            }
        }

        [TestMethod]
        public async Task IsNoScriptTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                bool isNoScript = await context.Web.IsNoScriptSiteAsync();
                Assert.IsTrue(isNoScript);
            }
        }

        [TestMethod]
        public async Task SetWebPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                // Test safety - sites typically are noscript sites
                bool isNoScript = await context.Web.IsNoScriptSiteAsync();

                if (!isNoScript)
                {
                    var web = await context.Web.GetAsync(p => p.AllProperties);

                    var propertyKey = "SetWebPropertiesTest";
                    var myProperty = web.AllProperties.GetInteger(propertyKey, 0);
                    if (myProperty == 0)
                    {
                        web.AllProperties[propertyKey] = 55;
                        await web.AllProperties.UpdateAsync();
                    }

                    using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 2))
                    {
                        web = await context2.Web.GetAsync(p => p.AllProperties);
                        myProperty = web.AllProperties.GetInteger(propertyKey, 0);
                        Assert.IsTrue(myProperty == 55);

                        web.AllProperties[propertyKey] = null;
                        await web.AllProperties.UpdateAsync();
                    }

                    using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite, 3))
                    {
                        web = await context3.Web.GetAsync(p => p.AllProperties);
                        myProperty = web.AllProperties.GetInteger(propertyKey, 0);
                        Assert.IsTrue(myProperty == 0);
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetSiteLanguagesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var web = await context.Web.GetAsync(p => p.SupportedUILanguageIds);

                Assert.IsTrue(web.SupportedUILanguageIds != null);
                Assert.IsTrue(web.SupportedUILanguageIds.Any());
            }
        }

        [TestMethod]
        public async Task AddRemoveSiteLanguagesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var web = await context.Web.GetAsync(p => p.SupportedUILanguageIds);

                var lastLanguage = web.SupportedUILanguageIds.Last();

                // remove a language
                web.SupportedUILanguageIds.Remove(lastLanguage);
                await web.UpdateAsync();

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite, 2))
                {
                    // Verify the language was actually removed
                    var web2 = await context2.Web.GetAsync(p => p.SupportedUILanguageIds);
                    Assert.IsFalse(web2.SupportedUILanguageIds.Contains(lastLanguage));

                    // Add the language again
                    web2.SupportedUILanguageIds.Add(lastLanguage);
                    await web2.UpdateAsync();
                }

                using (var context3 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite, 3))
                {
                    // Verify the language was added removed
                    var web3 = await context3.Web.GetAsync(p => p.SupportedUILanguageIds);
                    Assert.IsTrue(web3.SupportedUILanguageIds.Contains(lastLanguage));
                }
            }
        }

        [TestMethod]
        public async Task GetRegionalSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var web = await context.Web.GetAsync(p => p.RegionalSettings);

                Assert.IsTrue(web.RegionalSettings.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(web.RegionalSettings.DateSeparator));
                Assert.IsTrue(!string.IsNullOrEmpty(web.RegionalSettings.DecimalSeparator));
            }
        }

        [TestMethod]
        public async Task GetTimeZoneSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var web = await context.Web.GetAsync(p => p.RegionalSettings);
                var timeZone = await web.RegionalSettings.TimeZone.GetAsync();

                Assert.IsTrue(timeZone.Requested);
                Assert.IsTrue(!string.IsNullOrEmpty(timeZone.Description));
                Assert.IsTrue(timeZone.Id >= 0);
                Assert.IsTrue(timeZone.IsPropertyAvailable(p => p.Bias));
                Assert.IsTrue(timeZone.IsPropertyAvailable(p => p.DaylightBias));
                Assert.IsTrue(timeZone.IsPropertyAvailable(p => p.StandardBias));
            }
        }

        [TestMethod]
        public async Task GetTimeZonesSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                var web = await context.Web.GetAsync(p => p.RegionalSettings);
                var timeZones = (await web.RegionalSettings.GetAsync(p=>p.TimeZones)).TimeZones;

                Assert.IsTrue(timeZones.Requested);
                Assert.IsTrue(timeZones.Any());
                Assert.IsTrue(!string.IsNullOrEmpty(timeZones.First().Description));
                Assert.IsTrue(timeZones.First().IsPropertyAvailable(p => p.Bias));
                Assert.IsTrue(timeZones.First().IsPropertyAvailable(p => p.DaylightBias));
                Assert.IsTrue(timeZones.First().IsPropertyAvailable(p => p.StandardBias));
                Assert.IsTrue(!string.IsNullOrEmpty(timeZones.Last().Description));
                Assert.IsTrue(timeZones.Last().IsPropertyAvailable(p => p.Bias));
                Assert.IsTrue(timeZones.Last().IsPropertyAvailable(p => p.DaylightBias));
                Assert.IsTrue(timeZones.Last().IsPropertyAvailable(p => p.StandardBias));
            }
        }

        [TestMethod]
        public async Task GetRegionalSettingsPlusTimeZoneTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.RegionalSettings.GetAsync(p=>p.DecimalSeparator, p => p.TimeZone);

                Assert.IsTrue(context.Web.RegionalSettings.Requested);
                Assert.IsTrue(context.Web.RegionalSettings.IsPropertyAvailable(p => p.DecimalSeparator));
                Assert.IsTrue(context.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeZone));
                Assert.IsTrue(context.Web.RegionalSettings.TimeZone.Requested);
                Assert.IsTrue(context.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p=>p.Bias));
            }
        }

        [TestMethod]
        public async Task GetWebCurrentUserTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser = await context.Web.GetCurrentUserAsync();
                Assert.IsTrue(currentUser.Requested);
                Assert.IsTrue(currentUser is Model.Security.ISharePointUser);
            }
        }

        [TestMethod]
        public async Task GetWebCurrentUserBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser1 = await context.Web.GetCurrentUserBatchAsync();
                await context.ExecuteAsync();

                Assert.IsTrue(currentUser1.Requested);
                Assert.IsTrue(currentUser1 is Model.Security.ISharePointUser);
            }
        }

        [TestMethod]
        public async Task EnsureUserTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser = await context.Web.GetCurrentUserAsync();
                var ensuredUser = await context.Web.EnsureUserAsync(currentUser.UserPrincipalName);

                Assert.IsTrue(ensuredUser.Requested);
                Assert.IsTrue(ensuredUser is Model.Security.ISharePointUser);
                Assert.IsTrue(ensuredUser.UserPrincipalName == currentUser.UserPrincipalName);
            }
        }

        [TestMethod]
        public async Task EnsureUserBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser = await context.Web.GetCurrentUserAsync();
                var ensuredUser1 = await context.Web.EnsureUserBatchAsync(currentUser.UserPrincipalName);
                var ensuredUser2 = await context.Web.EnsureUserBatchAsync("Everyone except external users");
                await context.ExecuteAsync();

                Assert.IsTrue(ensuredUser1.Requested);
                Assert.IsTrue(ensuredUser1 is Model.Security.ISharePointUser);
                Assert.IsTrue(ensuredUser2.Requested);
                Assert.IsTrue(ensuredUser2 is Model.Security.ISharePointUser);
                Assert.IsTrue(ensuredUser1.UserPrincipalName == currentUser.UserPrincipalName);
                Assert.IsTrue(ensuredUser1.Id != ensuredUser2.Id);
            }
        }

        [TestMethod]
        public async Task GetUserTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser = await context.Web.GetCurrentUserAsync();
                var foundUser = await context.Web.GetUserByIdAsync(currentUser.Id);

                Assert.IsTrue(foundUser.Requested);
                Assert.IsTrue(foundUser is Model.Security.ISharePointUser);
                Assert.IsTrue(foundUser.UserPrincipalName == currentUser.UserPrincipalName);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SharePointRestServiceException))]
        public async Task GetUserMissingTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var foundUser = await context.Web.GetUserByIdAsync(890879070);
            }
        }

        [TestMethod]
        public async Task GetUserBatchTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // First ensure we have the needed users
                var currentUser = await context.Web.GetCurrentUserAsync();
                var ensuredUser1 = await context.Web.EnsureUserBatchAsync(currentUser.UserPrincipalName);
                var ensuredUser2 = await context.Web.EnsureUserBatchAsync("Everyone except external users");
                await context.ExecuteAsync();

                // Now retrieve them again in batch
                var foundUser1 = await context.Web.GetUserByIdBatchAsync(ensuredUser1.Id);
                var foundUser2 = await context.Web.GetUserByIdBatchAsync(ensuredUser2.Id);
                await context.ExecuteAsync();

                Assert.IsTrue(foundUser1.Requested);
                Assert.IsTrue(foundUser1 is Model.Security.ISharePointUser);
                Assert.IsTrue(foundUser2.Requested);
                Assert.IsTrue(foundUser2 is Model.Security.ISharePointUser);
                Assert.IsTrue(foundUser1.UserPrincipalName == currentUser.UserPrincipalName);
                Assert.IsTrue(foundUser1.Id != foundUser2.Id);
            }
        }

        [TestMethod]
        public async Task GetAssociatedGroups()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var web = await context.Web.GetAsync(p => p.AssociatedMemberGroup, p => p.AssociatedOwnerGroup, p => p.AssociatedVisitorGroup);
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedMemberGroup));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedOwnerGroup));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedVisitorGroup));

                Assert.IsTrue(web.AssociatedMemberGroup.Id != 0);
                Assert.IsTrue(web.AssociatedOwnerGroup.Id != 0);
                Assert.IsTrue(web.AssociatedVisitorGroup.Id != 0);
            }
        }

    }
}
