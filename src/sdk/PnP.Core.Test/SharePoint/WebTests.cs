﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using PnP.Core.QueryModel;
using System.IO;

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
                await context.Web.LoadAsync(
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
                    p => p.ContainsConfidentialInfo,
                    p => p.Created,
                    p => p.CustomMasterUrl,
                    p => p.DesignPackageId,
                    p => p.DisableRecommendedItems,
                    p => p.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled,
                    p => p.EffectiveBasePermissions,
                    p => p.EnableMinimalDownload,
                    p => p.FooterEmphasis,
                    p => p.FooterEnabled,
                    p => p.FooterLayout
                    );

                var web = context.Web;

                Assert.IsNotNull(web);
                Assert.IsNull(web.AccessRequestListUrl);
                Assert.AreEqual("", web.AccessRequestSiteDescription);
                Assert.IsFalse(web.AllowCreateDeclarativeWorkflowForCurrentUser);
                Assert.IsTrue(web.AllowDesignerForCurrentUser);
                Assert.IsFalse(web.AllowMasterPageEditingForCurrentUser);
                Assert.IsTrue(web.AllowRevertFromTemplateForCurrentUser);
                Assert.IsTrue(web.AllowRssFeeds);
                Assert.IsFalse(web.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser);
                Assert.IsFalse(web.AllowSavePublishDeclarativeWorkflowForCurrentUser);
                Assert.AreEqual("", web.AlternateCssUrl);
                // TODO: This one should be tested with an addin web to be relevant
                Assert.AreEqual(default, web.AppInstanceId);
                Assert.IsFalse(web.ContainsConfidentialInfo);
                Assert.IsTrue(web.CustomMasterUrl.EndsWith("/_catalogs/masterpage/seattle.master"));
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
                await context.Web.LoadAsync(
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
                
                var web = context.Web;

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
                await context.Web.LoadAsync(
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
                    p => p.SiteLogoDescription,
                    p => p.SiteLogoUrl,
                    p => p.SyndicationEnabled
                    );

                var web = context.Web;

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
                await context.Web.LoadAsync(
                    p => p.TenantAdminMembersCanShare,
                    // TODO Review this one, it causes SP REST to return an error
                    //p => p.ThemeData,
                    p => p.ThirdPartyMdmEnabled,
                    p => p.TreeViewEnabled,
                    p => p.UseAccessRequestDefault,
                    p => p.WebTemplate,
                    p => p.WebTemplateConfiguration
                    );

                var web = context.Web;

                Assert.IsNotNull(web);
                Assert.AreEqual(SharingState.Unspecified, web.TenantAdminMembersCanShare);
                // TODO Review this one, it causes SP REST to return an error
                //Assert.AreNotEqual("", web.ThemeData);
                // Not validating this property ~ this could have been manipulated on test sites causing false positives
                //Assert.IsNull(web.ThemedCssFolderUrl);
                Assert.IsFalse(web.ThirdPartyMdmEnabled);
                Assert.IsFalse(web.TreeViewEnabled);
                Assert.IsTrue(web.UseAccessRequestDefault);
                Assert.AreEqual("GROUP", web.WebTemplate);
                Assert.AreEqual("GROUP#0", web.WebTemplateConfiguration);
            }
        }


        [TestMethod]
        public async Task GetWebAllPropertiesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.AllProperties);

                var webWithAllProperties = context.Web;
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
                await context.Web.LoadAsync(p => p.RootFolder);

                IWeb webWithRootFolder = context.Web;

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
                await context.Web.LoadAsync(p => p.SiteUserInfoList);

                IWeb webWithSiteInfoUserList = context.Web;

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
                await context.Web.LoadAsync(p => p.AvailableContentTypes);

                IWeb web = context.Web;
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
                await context.Web.LoadAsync(p => p.AvailableFields);

                IWeb web = context.Web;

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
                await context.Web.LoadAsync(p => p.EffectiveBasePermissions);

                IWeb web = context.Web;

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
            TestCommon.ClassicSTS0TestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.ClassicSTS0TestSite))
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

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetInteger(propertyKey, 0);
                    Assert.IsTrue(myProperty == 55);

                    web.AllProperties[propertyKey] = null;
                    await web.AllProperties.UpdateAsync();

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetInteger(propertyKey, 0);
                    Assert.IsTrue(myProperty == 0);
                }
            }
        }

        [TestMethod]
        public async Task SetWebPropertiesBooleanTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.ClassicSTS0TestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.ClassicSTS0TestSite))
            {
                // Test safety - sites typically are noscript sites
                bool isNoScript = await context.Web.IsNoScriptSiteAsync();

                if (!isNoScript)
                {
                    var web = await context.Web.GetAsync(p => p.AllProperties);

                    var propertyKey = "SetWebPropertiesBooleanTest";
                    var myProperty = web.AllProperties.GetBoolean(propertyKey, false);
                    if (myProperty == false)
                    {
                        web.AllProperties[propertyKey] = true;
                        await web.AllProperties.UpdateAsync();
                    }

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetBoolean(propertyKey, false);
                    Assert.IsTrue(myProperty == true);

                    web.AllProperties[propertyKey] = null;
                    await web.AllProperties.UpdateAsync();

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetBoolean(propertyKey, false);
                    Assert.IsTrue(myProperty == false);
                }
            }
        }

        [TestMethod]
        public async Task SetWebPropertiesSpecialCharsTest()
        {
            //TestCommon.Instance.Mocking = false;
            TestCommon.ClassicSTS0TestSetup();

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.ClassicSTS0TestSite))
            {
                // Test safety - sites typically are noscript sites
                bool isNoScript = await context.Web.IsNoScriptSiteAsync();

                if (!isNoScript)
                {
                    var web = await context.Web.GetAsync(p => p.AllProperties);

                    string specialChars = "<this & is a \n \t > a special \" ' string";

                    var propertyKey = "SetWebPropertiesSpecialCharsTest";
                    var myProperty = web.AllProperties.GetString(propertyKey, null);
                    if (myProperty == null)
                    {
                        web.AllProperties[propertyKey] = specialChars;
                        await web.AllProperties.UpdateAsync();
                    }

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetString(propertyKey, null);
                    Assert.IsTrue(myProperty == specialChars);

                    web.AllProperties[propertyKey] = null;
                    await web.AllProperties.UpdateAsync();

                    web = await context.Web.GetAsync(p => p.AllProperties);
                    myProperty = web.AllProperties.GetString(propertyKey, null);
                    Assert.IsTrue(myProperty == null);
                }
            }
        }

        [TestMethod]
        public async Task GetSiteLanguagesTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.LoadAsync(p => p.SupportedUILanguageIds);

                var web = context.Web;

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

                // Verify the language was actually removed
                var web2 = await context.Web.GetAsync(p => p.SupportedUILanguageIds);
                Assert.IsFalse(web2.SupportedUILanguageIds.Contains(lastLanguage));

                // Add the language again
                web2.SupportedUILanguageIds.Add(lastLanguage);
                await web2.UpdateAsync();
                // Verify the language was added removed
                var web3 = await context.Web.GetAsync(p => p.SupportedUILanguageIds);
                Assert.IsTrue(web3.SupportedUILanguageIds.Contains(lastLanguage));
            }
        }

        [TestMethod]
        public async Task GetRegionalSettingsTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.LoadAsync(p => p.RegionalSettings);

                var web = context.Web;

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
                await context.Web.LoadAsync(p => p.RegionalSettings);
                await context.Web.RegionalSettings.TimeZone.LoadAsync();

                var timeZone = context.Web.RegionalSettings.TimeZone;

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
                await context.Web.LoadAsync(p => p.RegionalSettings);
                await context.Web.RegionalSettings.LoadAsync(p=>p.TimeZones);

                var timeZones = context.Web.RegionalSettings.TimeZones;

                Assert.IsTrue(timeZones.Requested);
                Assert.IsTrue(timeZones.Length > 0);
                var firstTimeZone = timeZones.AsRequested().First();
                var listTimeZone = timeZones.AsRequested().Last();
                Assert.IsTrue(!string.IsNullOrEmpty(firstTimeZone.Description));
                Assert.IsTrue(firstTimeZone.IsPropertyAvailable(p => p.Bias));
                Assert.IsTrue(firstTimeZone.IsPropertyAvailable(p => p.DaylightBias));
                Assert.IsTrue(firstTimeZone.IsPropertyAvailable(p => p.StandardBias));
                Assert.IsTrue(!string.IsNullOrEmpty(listTimeZone.Description));
                Assert.IsTrue(listTimeZone.IsPropertyAvailable(p => p.Bias));
                Assert.IsTrue(listTimeZone.IsPropertyAvailable(p => p.DaylightBias));
                Assert.IsTrue(listTimeZone.IsPropertyAvailable(p => p.StandardBias));
            }
        }

        [TestMethod]
        public async Task GetRegionalSettingsPlusTimeZoneTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.RegionalSettings.LoadAsync(p=>p.DecimalSeparator, p => p.TimeZone);

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
                await context.Web.LoadAsync(p => p.AssociatedMemberGroup, p => p.AssociatedOwnerGroup, p => p.AssociatedVisitorGroup);
                var web = context.Web;
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedMemberGroup));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedOwnerGroup));
                Assert.IsTrue(web.IsPropertyAvailable(p => p.AssociatedVisitorGroup));

                Assert.IsTrue(web.AssociatedMemberGroup.Id != 0);
                Assert.IsTrue(web.AssociatedOwnerGroup.Id != 0);
                Assert.IsTrue(web.AssociatedVisitorGroup.Id != 0);
            }
        }

        [TestMethod]
        public async Task AddWebWithDefaults()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "newsubweb";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });
                Assert.IsTrue(addedWeb != null);
                Assert.AreEqual(addedWeb.Title, webTitle);
                Assert.AreEqual(addedWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.LoadAsync(p => p.Webs);
                    var createdWeb = context2.Web.Webs.AsRequested().FirstOrDefault(p => p.Title == webTitle);
                    Assert.IsTrue(createdWeb != null);
                    Assert.AreEqual(createdWeb.Title, webTitle);
                    Assert.AreEqual(createdWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                    // Delete the created web again
                    await createdWeb.DeleteAsync();
                    Assert.IsTrue((createdWeb as Web).Deleted);
                }
            }
        }

        [TestMethod]
        public async Task AddWebInBatchWithDefaults()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "AddWebInBatchWithDefaults";
                var addedWeb = await context.Web.Webs.AddBatchAsync(new WebOptions { Title = webTitle, Url = webTitle });
                await context.ExecuteAsync();

                Assert.IsTrue(addedWeb != null);
                Assert.AreEqual(addedWeb.Title, webTitle);
                Assert.AreEqual(addedWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.LoadAsync(p => p.Webs);
                    var createdWeb = context2.Web.Webs.AsRequested().FirstOrDefault(p => p.Title == webTitle);
                    Assert.IsTrue(createdWeb != null);
                    Assert.AreEqual(createdWeb.Title, webTitle);
                    Assert.AreEqual(createdWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                    // Delete the created web again
                    await createdWeb.DeleteAsync();
                    Assert.IsTrue((createdWeb as Web).Deleted);
                }
            }
        }

        [TestMethod]
        public async Task AddWebWithCustomOptions()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "AddWebWithCustomOptions";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions 
                { 
                    Title = webTitle, 
                    Url = webTitle,
                    Description = "Description of the sub web",
                    Template = "STS#0",
                    Language = 1043,
                    InheritPermissions = false
                });
                Assert.IsTrue(addedWeb != null);
                Assert.AreEqual(addedWeb.Title, webTitle);
                Assert.AreEqual(addedWeb.Description, "Description of the sub web");
                Assert.AreEqual(addedWeb.WebTemplate, "STS");
                Assert.AreEqual(addedWeb.Language, 1043);
                Assert.AreEqual(addedWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.LoadAsync(p => p.Webs);
                    var createdWeb = context2.Web.Webs.AsRequested().FirstOrDefault(p => p.Title == webTitle);
                    Assert.IsTrue(createdWeb != null);

                    Assert.AreEqual(createdWeb.Title, webTitle);
                    Assert.AreEqual(createdWeb.Description, "Description of the sub web");
                    Assert.AreEqual(createdWeb.WebTemplate, "STS");
                    Assert.AreEqual(createdWeb.Language, 1043);
                    Assert.AreEqual(createdWeb.Url, new Uri($"{context.Uri}/{webTitle}"));

                    // Delete the created web again
                    await createdWeb.DeleteAsync();
                    Assert.IsTrue((createdWeb as Web).Deleted);
                }
            }
        }


        [TestMethod]
        public async Task BatchDelete()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "BatchDelete";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                using (var context2 = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite, 1))
                {
                    await context2.Web.LoadAsync(p => p.Webs);
                    var createdWeb = context2.Web.Webs.AsRequested().FirstOrDefault(p => p.Title == webTitle);
                    Assert.IsTrue(createdWeb != null);

                    await Assert.ThrowsExceptionAsync<ClientException>(async () =>
                    {
                        // Delete the created web again...should result in an exception
                        await createdWeb.DeleteBatchAsync();
                    });

                    // Use supported delete method
                    await createdWeb.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public async Task DeleteCurrentWeb()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                string webTitle = "DeleteCurrentWeb";
                var addedWeb = await context.Web.Webs.AddAsync(new WebOptions { Title = webTitle, Url = webTitle });

                using (var context2 = await TestCommon.Instance.CloneAsync(context, addedWeb.Url, 1))
                {
                    // This will not delete the web, the delete request will be cancelled
                    await context2.Web.DeleteAsync();
                }

                // Use supported delete method
                await addedWeb.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task GetByServerRelativeSpecialChars()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Get the default document library root folder
                string sharedDocumentsFolderUrl = $"{context.Uri.PathAndQuery}/Shared Documents";
                IFolder sharedDocumentsFolder = await context.Web.GetFolderByServerRelativeUrlAsync(sharedDocumentsFolderUrl);
                Assert.IsNotNull(sharedDocumentsFolder);

                string lib2Name = TestCommon.GetPnPSdkTestAssetName("GetByServerRelativeSpecialChars");
                var lib2 = await context.Web.Lists.AddAsync(lib2Name, ListTemplateType.DocumentLibrary);

                await lib2.EnsurePropertiesAsync(p => p.RootFolder);
                await lib2.RootFolder.EnsureFolderAsync("Hi'there# is &ok");
                IFolder specialNameFolder2 = await context.Web.GetFolderByServerRelativeUrlAsync(lib2.RootFolder.ServerRelativeUrl + "/" + "Hi'there# is &ok");
                Assert.IsNotNull(specialNameFolder2);

                // Upload a file with a special name
                var file2 = await specialNameFolder2.Files.AddAsync("Hi'there# is &ok.docx", System.IO.File.OpenRead($".{Path.DirectorySeparatorChar}TestAssets{Path.DirectorySeparatorChar}test.docx"));
                IFile specialFile2 = await context.Web.GetFileByServerRelativeUrlAsync(lib2.RootFolder.ServerRelativeUrl + "/Hi'there# is &ok/Hi'there# is &ok.docx");

                await lib2.DeleteAsync();
            }
        }

    }
}
