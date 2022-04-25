using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.QueryModel;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
                    p => p.MegaMenuEnabled,
                    p => p.HasUniqueRoleAssignments
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
                Assert.IsTrue(web.HasUniqueRoleAssignments);
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
                Assert.IsTrue(string.IsNullOrEmpty(web.SiteLogoDescription));
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
        public async Task SetWebPropertiesUnderScoreTest()
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

                    var propertyKey = "With SpaceAnd_Underscore";
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
                await context.Web.RegionalSettings.LoadAsync(p => p.TimeZones);

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
                await context.Web.RegionalSettings.LoadAsync(p => p.DecimalSeparator, p => p.TimeZone);

                Assert.IsTrue(context.Web.RegionalSettings.Requested);
                Assert.IsTrue(context.Web.RegionalSettings.IsPropertyAvailable(p => p.DecimalSeparator));
                Assert.IsTrue(context.Web.RegionalSettings.IsPropertyAvailable(p => p.TimeZone));
                Assert.IsTrue(context.Web.RegionalSettings.TimeZone.Requested);
                Assert.IsTrue(context.Web.RegionalSettings.TimeZone.IsPropertyAvailable(p => p.Bias));
            }
        }

        [TestMethod]
        [DataRow("(UTC-12:00) International Date Line West")]
        [DataRow("(UTC-11:00) Coordinated Universal Time-11")]
        [DataRow("(UTC-10:00) Hawaii")]
        [DataRow("(UTC-09:00) Alaska")]
        [DataRow("(UTC-08:00) Baja California")]
        [DataRow("(UTC-08:00) Pacific Time (US and Canada)")]
        [DataRow("(UTC-07:00) Arizona")]
        [DataRow("(UTC-07:00) Chihuahua, La Paz, Mazatlan")]
        [DataRow("(UTC-07:00) Mountain Time (US and Canada)")]
        [DataRow("(UTC-06:00) Central America")]
        [DataRow("(UTC-06:00) Central Time (US and Canada)")]
        [DataRow("(UTC-06:00) Guadalajara, Mexico City, Monterrey")]
        [DataRow("(UTC-06:00) Saskatchewan")]
        [DataRow("(UTC-05:00) Bogota, Lima, Quito")]
        [DataRow("(UTC-05:00) Eastern Time (US and Canada)")]
        [DataRow("(UTC-05:00) Indiana (East)")]
        [DataRow("(UTC-04:30) Caracas")]
        [DataRow("(UTC-04:00) Asuncion")]
        [DataRow("(UTC-04:00) Atlantic Time (Canada)")]
        [DataRow("(UTC-04:00) Cuiaba")]
        [DataRow("(UTC-04:00) Georgetown, La Paz, Manaus, San Juan")]
        [DataRow("(UTC-04:00) Santiago")]
        [DataRow("(UTC-03:30) Newfoundland")]
        [DataRow("(UTC-03:00) Brasilia")]
        [DataRow("(UTC-03:00) Buenos Aires")]
        [DataRow("(UTC-03:00) Cayenne, Fortaleza")]
        [DataRow("(UTC-03:00) Greenland")]
        [DataRow("(UTC-03:00) Montevideo")]
        [DataRow("(UTC-03:00) Salvador")]
        [DataRow("(UTC-02:00) Coordinated Universal Time-02")]
        [DataRow("(UTC-02:00) Mid-Atlantic")]
        [DataRow("(UTC-01:00) Azores")]
        [DataRow("(UTC-01:00) Cabo Verde")]
        [DataRow("(UTC) Casablanca")]
        [DataRow("(UTC) Coordinated Universal Time")]
        [DataRow("(UTC) Dublin, Edinburgh, Lisbon, London")]
        [DataRow("(UTC) Monrovia, Reykjavik")]
        [DataRow("(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna")]
        [DataRow("(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague")]
        [DataRow("(UTC+01:00) Brussels, Copenhagen, Madrid, Paris")]
        [DataRow("(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb")]
        [DataRow("(UTC+01:00) West Central Africa")]
        [DataRow("(UTC+01:00) Windhoek")]
        [DataRow("(UTC+02:00) Amman")]
        [DataRow("(UTC+02:00) Athens, Bucharest")]
        [DataRow("(UTC+02:00) Beirut")]
        [DataRow("(UTC+02:00) Cairo")]
        [DataRow("(UTC+02:00) Damascus")]
        [DataRow("(UTC+02:00) Harare, Pretoria")]
        [DataRow("(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius")]
        [DataRow("(UTC+02:00) Jerusalem")]
        [DataRow("(UTC+02:00) Minsk (old)")]
        [DataRow("(UTC+02:00) E. Europe")]
        [DataRow("(UTC+02:00) Kaliningrad")]
        [DataRow("(UTC+03:00) Baghdad")]
        [DataRow("(UTC+03:00) Istanbul")]
        [DataRow("(UTC+03:00) Kuwait, Riyadh")]
        [DataRow("(UTC+03:00) Minsk")]
        [DataRow("(UTC+03:00) Moscow, St. Petersburg, Volgograd")]
        [DataRow("(UTC+03:00) Nairobi")]
        [DataRow("(UTC+03:30) Tehran")]
        [DataRow("(UTC+04:00) Abu Dhabi, Muscat")]
        [DataRow("(UTC+04:00) Astrakhan, Ulyanovsk")]
        [DataRow("(UTC+04:00) Baku")]
        [DataRow("(UTC+04:00) Izhevsk, Samara")]
        [DataRow("(UTC+04:00) Port Louis")]
        [DataRow("(UTC+04:00) Tbilisi")]
        [DataRow("(UTC+04:00) Yerevan")]
        [DataRow("(UTC+04:30) Kabul")]
        [DataRow("(UTC+05:00) Ekaterinburg")]
        [DataRow("(UTC+05:00) Islamabad, Karachi")]
        [DataRow("(UTC+05:00) Tashkent")]
        [DataRow("(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi")]
        [DataRow("(UTC+05:30) Sri Jayawardenepura")]
        [DataRow("(UTC+05:45) Kathmandu")]
        [DataRow("(UTC+06:00) Astana")]
        [DataRow("(UTC+06:00) Dhaka")]
        [DataRow("(UTC+06:00) Omsk")]
        [DataRow("(UTC+06:30) Yangon (Rangoon)")]
        [DataRow("(UTC+07:00) Bangkok, Hanoi, Jakarta")]
        [DataRow("(UTC+07:00) Barnaul, Gorno-Altaysk")]
        [DataRow("(UTC+07:00) Krasnoyarsk")]
        [DataRow("(UTC+07:00) Novosibirsk")]
        [DataRow("(UTC+07:00) Tomsk")]
        [DataRow("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi")]
        [DataRow("(UTC+08:00) Irkutsk")]
        [DataRow("(UTC+08:00) Kuala Lumpur, Singapore")]
        [DataRow("(UTC+08:00) Perth")]
        [DataRow("(UTC+08:00) Taipei")]
        [DataRow("(UTC+08:00) Ulaanbaatar")]
        [DataRow("(UTC+09:00) Osaka, Sapporo, Tokyo")]
        [DataRow("(UTC+09:00) Seoul")]
        [DataRow("(UTC+09:00) Yakutsk")]
        [DataRow("(UTC+09:30) Adelaide")]
        [DataRow("(UTC+09:30) Darwin")]
        [DataRow("(UTC+10:00) Brisbane")]
        [DataRow("(UTC+10:00) Canberra, Melbourne, Sydney")]
        [DataRow("(UTC+10:00) Guam, Port Moresby")]
        [DataRow("(UTC+10:00) Hobart")]
        [DataRow("(UTC+10:00) Magadan")]
        [DataRow("(UTC+10:00) Vladivostok")]
        [DataRow("(UTC+11:00) Chokurdakh")]
        [DataRow("(UTC+11:00) Sakhalin")]
        [DataRow("(UTC+11:00) Solomon Is., New Caledonia")]
        [DataRow("(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky")]
        [DataRow("(UTC+12:00) Auckland, Wellington")]
        [DataRow("(UTC+12:00) Coordinated Universal Time+12")]
        [DataRow("(UTC+12:00) Fiji")]
        [DataRow("(UTC+12:00) Petropavlovsk-Kamchatsky - Old")]
        [DataRow("(UTC+13:00) Nuku'alofa")]
        [DataRow("(UTC+13:00) Samoa")]
        [DataRow("(UTC-11:00) whatever")]
        [DataRow("(UTC-12:00) whatever")]
        [DataRow("(UTC-10:00) whatever")]
        [DataRow("(UTC-09:00) whatever")]
        [DataRow("(UTC-08:00) whatever")]
        [DataRow("(UTC-07:00) whatever")]
        [DataRow("(UTC-06:00) whatever")]
        [DataRow("(UTC-05:00) whatever")]
        [DataRow("(UTC-04:30) whatever")]
        [DataRow("(UTC-04:00) whatever")]
        [DataRow("(UTC-03:30) whatever")]
        [DataRow("(UTC-03:00) whatever")]
        [DataRow("(UTC-02:00) whatever")]
        [DataRow("(UTC-01:00) whatever")]
        [DataRow("(UTC) whatever")]
        [DataRow("(UTC+01:00) whatever")]
        [DataRow("(UTC+02:00) whatever")]
        [DataRow("(UTC+03:00) whatever")]
        [DataRow("(UTC+04:00) whatever")]
        [DataRow("(UTC+05:00) whatever")]
        [DataRow("(UTC+05:30) whatever")]
        [DataRow("(UTC+05:45) whatever")]
        [DataRow("(UTC+06:00) whatever")]
        [DataRow("(UTC+06:30) whatever")]
        [DataRow("(UTC+07:00) whatever")]
        [DataRow("(UTC+08:00) whatever")]
        [DataRow("(UTC+09:00) whatever")]
        [DataRow("(UTC+09:30) whatever")]
        [DataRow("(UTC+10:00) whatever")]
        [DataRow("(UTC+11:00) whatever")]
        [DataRow("(UTC+12:00) whatever")]
        [DataRow("(UTC+13:00) whatever")]
        public void TimeZoneMappingTest(string sharePointTimeZone)
        {
            Assert.IsNotNull(Model.SharePoint.TimeZone.GetTimeZoneInfoFromSharePoint(sharePointTimeZone));
        }

        [TestMethod]
        public void TimeZoneConversionTests()
        {
            if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping running this one on GitHub");

            // Assume timezone UTC-8 (PST), description = (UTC-08:00) Pacific Time (US and Canada)
            // Bias = 480, DaylightBias = -60, StandardBias = 0

            // Winter time in Belgium: Jan 15, 15:15 = 06:15 PST
            var localDate = new DateTime(2021, 1, 15, 15, 15, 15);
            var utcDate = localDate.ToUniversalTime();
            var localWebTime = utcDate - UtcDelta(utcDate, 480, -60, 0, "(UTC-08:00) Pacific Time (US and Canada)");
            Assert.IsTrue(localWebTime.Hour == 6);

            // Summer time in Belgium: Jul 15, 15:15 = 06:15 PST
            localDate = new DateTime(2021, 6, 15, 15, 15, 15);
            utcDate = localDate.ToUniversalTime();
            localWebTime = utcDate - UtcDelta(utcDate, 480, -60, 0, "(UTC-08:00) Pacific Time (US and Canada)");
            Assert.IsTrue(localWebTime.Hour == 6);

            // Period when US swithed to summer time while Belgium is still on winter time: Mar 15, 15:15 = 07:15 PST
            localDate = new DateTime(2021, 3, 15, 15, 15, 15);
            utcDate = localDate.ToUniversalTime();
            localWebTime = utcDate - UtcDelta(utcDate, 480, -60, 0, "(UTC-08:00) Pacific Time (US and Canada)");
            Assert.IsTrue(localWebTime.Hour == 7);

            // Assume timezone UTC-1 (Brussels), description = (UTC+01:00) Brussels, Copenhagen, Madrid, Paris
            // Bias = -60, DaylightBias = -60, StandardBias = 0

            // Winter time in Belgium: Jan 15, 15:15 stays the same
            localDate = new DateTime(2021, 1, 15, 15, 15, 15);
            utcDate = localDate.ToUniversalTime();
            localWebTime = utcDate - UtcDelta(utcDate, -60, -60, 0, "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris");
            Assert.IsTrue(localWebTime.Hour == 15);

            // Summer time in Belgium: Jul 15, 15:15 stays the same
            localDate = new DateTime(2021, 6, 15, 15, 15, 15);
            utcDate = localDate.ToUniversalTime();
            localWebTime = utcDate - UtcDelta(utcDate, -60, -60, 0, "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris");
            Assert.IsTrue(localWebTime.Hour == 15);

            // Period when US swithed to summer time while Belgium is still on winter time: Mar 15, 15:15 stays the same
            localDate = new DateTime(2021, 3, 15, 15, 15, 15);
            utcDate = localDate.ToUniversalTime();
            localWebTime = utcDate - UtcDelta(utcDate, -60, -60, 0, "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris");
            Assert.IsTrue(localWebTime.Hour == 15);

        }

        private TimeSpan UtcDelta(DateTime dateTime, int bias, int daylightBias, int standardBias, string description)
        {
            return new TimeSpan(0, bias + (Model.SharePoint.TimeZone.GetTimeZoneInfoFromSharePoint(description).IsDaylightSavingTime(dateTime) ? daylightBias : standardBias), 0);
        }

        [TestMethod]
        public async Task TimeZoneUtcToLocalTimeTest()
        {
            //TestCommon.Instance.Mocking = false;
            // Test site is configured with UTC + 1 timezone
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Datetime in timezone of the code running PnP Core SDK, use time that falls outside of daylight saving time
                var localDate = new DateTime(2021, 1, 15, 15, 15, 15, DateTimeKind.Local);

                // Convert to UTC time
                var utcDate = localDate.ToUniversalTime();

                // Convert to Web's timezone
                var localSiteTime = context.Web.RegionalSettings.TimeZone.UtcToLocalTime(utcDate);

                // Use server call to do the same
                var response = context.Web.RegionalSettings.TimeZone.ExecuteRequest(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, $"_api/web/regionalsettings/timezone/utctolocaltime('{utcDate.ToString("o", CultureInfo.InvariantCulture)}')", null));

                if (!string.IsNullOrEmpty(response.Response))
                {
                    var json = JsonDocument.Parse(response.Response).RootElement.GetProperty("d");

                    if (json.TryGetProperty("UTCToLocalTime", out JsonElement utcToLocalTimeViaServerCall))
                    {
                        if (utcToLocalTimeViaServerCall.TryGetDateTime(out DateTime utcToLocalTimeViaServerCallDateTime))
                        {
                            if (!TestCommon.RunningInGitHubWorkflow())
                            {
                                Assert.AreEqual(utcToLocalTimeViaServerCallDateTime, localSiteTime);
                            }
                        }
                    }
                }

                // Convert from Web's timezone back to UTC time
                var localSiteTimeBackToUtc = context.Web.RegionalSettings.TimeZone.LocalTimeToUtc(localSiteTime);

                // Use server call to do the same
                var response2 = context.Web.RegionalSettings.TimeZone.ExecuteRequest(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, $"_api/web/regionalsettings/timezone/localtimetoutc('{localSiteTime.ToString("o", CultureInfo.InvariantCulture)}')", null));

                if (!string.IsNullOrEmpty(response2.Response))
                {
                    var json = JsonDocument.Parse(response2.Response).RootElement.GetProperty("d");

                    if (json.TryGetProperty("LocalTimeToUTC", out JsonElement LocalTimeToUtcViaServerCall))
                    {
                        if (LocalTimeToUtcViaServerCall.TryGetDateTime(out DateTime LocalTimeToUtcViaServerCallDateTime))
                        {
                            if (!TestCommon.RunningInGitHubWorkflow())
                            {
                                Assert.AreEqual(LocalTimeToUtcViaServerCallDateTime, localSiteTimeBackToUtc);
                            }
                        }
                    }
                }

                Assert.AreEqual(utcDate, localSiteTimeBackToUtc);

                // Convert back to timezone of the running code
                var utcDateBackToLocal = localSiteTimeBackToUtc.ToLocalTime();

                Assert.AreEqual(localDate, utcDateBackToLocal);
            }
        }

        [TestMethod]
        public async Task TimeZoneUtcToLocalTime2Test()
        {
            //TestCommon.Instance.Mocking = false;
            // Test sub site is configured with UTC-8 timezone 
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                // Datetime in timezone of the code running PnP Core SDK, use time that falls inside daylight saving time
                var localDate = new DateTime(2021, 7, 15, 15, 15, 15, DateTimeKind.Local);

                // Convert to UTC time
                var utcDate = localDate.ToUniversalTime();

                // Convert to Web's timezone
                var localSiteTime = context.Web.RegionalSettings.TimeZone.UtcToLocalTime(utcDate);

                // Use server call to do the same
                var response = context.Web.RegionalSettings.TimeZone.ExecuteRequest(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, $"_api/web/regionalsettings/timezone/utctolocaltime('{utcDate.ToString("o", CultureInfo.InvariantCulture)}')", null));
                // {"d":{"UTCToLocalTime":"2021-07-15T06:15:15"}}

                if (!string.IsNullOrEmpty(response.Response))
                {
                    var json = JsonDocument.Parse(response.Response).RootElement;

                    if (json.TryGetProperty("value", out JsonElement utcToLocalTimeViaServerCall))
                    {
                        if (utcToLocalTimeViaServerCall.TryGetDateTime(out DateTime utcToLocalTimeViaServerCallDateTime))
                        {
                            if (!TestCommon.RunningInGitHubWorkflow())
                            {
                                Assert.AreEqual(utcToLocalTimeViaServerCallDateTime, localSiteTime);
                            }
                        }
                    }
                }

                // Convert from Web's timezone back to UTC time
                var localSiteTimeBackToUtc = context.Web.RegionalSettings.TimeZone.LocalTimeToUtc(localSiteTime);

                // Use server call to do the same
                var response2 = context.Web.RegionalSettings.TimeZone.ExecuteRequest(new ApiRequest(HttpMethod.Post, ApiRequestType.SPORest, $"_api/web/regionalsettings/timezone/localtimetoutc('{localSiteTime.ToString("o", CultureInfo.InvariantCulture)}')", null));
                // {"d":{"LocalTimeToUTC":"2021-07-15T13:15:15Z"}}

                if (!string.IsNullOrEmpty(response2.Response))
                {
                    var json = JsonDocument.Parse(response2.Response).RootElement;

                    if (json.TryGetProperty("value", out JsonElement LocalTimeToUtcViaServerCall))
                    {
                        if (LocalTimeToUtcViaServerCall.TryGetDateTime(out DateTime LocalTimeToUtcViaServerCallDateTime))
                        {
                            if (!TestCommon.RunningInGitHubWorkflow())
                            {
                                Assert.AreEqual(LocalTimeToUtcViaServerCallDateTime, localSiteTimeBackToUtc);
                            }
                        }
                    }
                }

                Assert.AreEqual(utcDate, localSiteTimeBackToUtc);

                // Convert back to timezone of the running code
                var utcDateBackToLocal = localSiteTimeBackToUtc.ToLocalTime();

                Assert.AreEqual(localDate, utcDateBackToLocal);
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
        public async Task AddUserToWebTest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var currentUser = await context.Web.GetCurrentUserAsync();

                // Is equivalent to EnsureUser
                var addedUser = context.Web.SiteUsers.Add(currentUser.LoginName);

                addedUser.AddRoleDefinitions("Full Control");

                var currentRoleDefinitions = addedUser.GetRoleDefinitions();

                Assert.IsNotNull(currentRoleDefinitions.AsRequested().FirstOrDefault(p => p.Name == "Full Control"));

                addedUser.RemoveRoleDefinitions(new string[] { "Full Control" });

                currentRoleDefinitions = addedUser.GetRoleDefinitions();

                Assert.IsTrue(currentRoleDefinitions == null);
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

        [TestMethod]
        public async Task GetWebChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var changes = await context.Web.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var changesBatch = context.Web.GetChangesBatch(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5
                });

                Assert.IsFalse(changesBatch.IsAvailable);

                await context.ExecuteAsync();

                Assert.IsTrue(changesBatch.IsAvailable);

                Assert.IsTrue(changesBatch.Count > 0);
            }
        }

        [TestMethod]
        public void GetWebChangesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var changes = context.Web.GetChanges(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }

        [TestMethod]
        public async Task GetWebChangesViaChangeTokenAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var changes = await context.Web.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 10
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);

                var firstChangeToken = changes.First().ChangeToken;

                // Alternative way to pass in a change token if you've stored the last change token string value
                var lastChangetoken = new ChangeTokenOptions(changes.Last().ChangeToken.StringValue);

                var changes2 = await context.Web.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    ChangeTokenStart = firstChangeToken,
                    ChangeTokenEnd = lastChangetoken
                });

                Assert.IsTrue(changes2.Count == 9);
            }
        }

        [TestMethod]
        public async Task GetWebChangesContentTypeAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Add a new content type
                IContentType newContentType = await context.Web.ContentTypes.AddAsync("0x0100554FB756A84E4A4899FB819522D2BF50", "ChangeTest", "TESTING", "TESTING");

                try
                {
                    var changes = await context.Web.GetChangesAsync(new ChangeQueryOptions(false, true)
                    {
                        ContentType = true,
                        FetchLimit = 1000
                    });

                    Assert.IsNotNull(changes);
                    Assert.IsTrue(changes.Count > 0);
                    Assert.IsTrue((changes.Last() as IChangeContentType).ContentTypeId != null);
                    Assert.IsTrue((changes.Last() as IChangeContentType).WebId != Guid.Empty);

                    Assert.IsTrue(changes.Last().IsPropertyAvailable<IChangeContentType>(p => p.ContentTypeId));
                    Assert.ThrowsException<ClientException>(() =>
                    {
                        changes.Last().IsPropertyAvailable<IChangeContentType>(p => p.ContentTypeId.Name);
                    });
                    Assert.ThrowsException<ArgumentNullException>(() =>
                    {
                        changes.Last().IsPropertyAvailable<IChangeContentType>(null);
                    });

                    // Load additional properties based upon the returned content type
                    IChangeContentType contentTypeToValidate = null;
                    foreach (var change in changes)
                    {
                        if (change is IChangeContentType changeContentType)
                        {
                            if (changeContentType.ContentTypeId.StringId == "0x0100554FB756A84E4A4899FB819522D2BF50")
                            {
                                contentTypeToValidate = changeContentType;
                                break;
                            }
                        }
                    }

                    if (contentTypeToValidate != null)
                    {
                        await contentTypeToValidate.ContentTypeId.LoadAsync(p => p.Group);
                        Assert.IsTrue(contentTypeToValidate.ContentTypeId.IsPropertyAvailable(p => p.Group));
                    }
                }
                finally
                {
                    // Delete the content type again
                    await newContentType.DeleteAsync();
                }
            }
        }

        [TestMethod]
        public void IsSubSitePositiveTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSubSite))
            {
                Assert.IsTrue(context.Web.IsSubSite());
            }
        }

        [TestMethod]
        public void IsSubSiteNegativeTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.IsSubSite());
            }
        }

        [TestMethod]
        public void EnsurePageSchedulingTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.NoGroupTestSite))
            {
                Guid pageSchedulingFeature = new("e87ca965-5e07-4a23-b007-ddd4b5afb9c7");

                try
                {
                    context.Web.EnsurePageScheduling();

                    Assert.IsTrue(context.Web.Features.AsRequested().Any(p => p.DefinitionId == pageSchedulingFeature));
                }
                finally
                {
                    context.Web.Features.Disable(pageSchedulingFeature);
                }

            }
        }

        [TestMethod]
        public void EnsurePageSchedulingOnSubSiteTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSubSite))
            {
                Assert.ThrowsException<ClientException>(() =>
                {
                    context.Web.EnsurePageScheduling();
                });
            }
        }

        [TestMethod]
        public async Task GetSubWebs()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.Web.Load(w => w.Webs);
                Assert.IsTrue(context.Web.Webs.Length > 0);

                var webs = context.Web.Webs.ToList();
                Assert.IsTrue(webs.Count == context.Web.Webs.Length);
                Guid id = webs.First().Id;

                var webs2 = context.Web.Webs.Where(p => p.Id == id).ToList();
                Assert.IsTrue(webs2.Count == 1);
                Assert.IsTrue(webs2.First().Id == webs.First().Id);
            }
        }

        [TestMethod]
        public void HasCommunicationSiteFeatures()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                Assert.IsFalse(context.Web.HasCommunicationSiteFeatures());
            }

            using (var context = TestCommon.Instance.GetContext(TestCommon.NoGroupTestSite, 1))
            {
                Assert.IsTrue(context.Web.HasCommunicationSiteFeatures());
            }

            using (var context = TestCommon.Instance.GetContext(TestCommon.SyntexContentCenterTestSite, 2))
            {
                Assert.IsTrue(context.Web.HasCommunicationSiteFeatures());
            }

            using (var context = TestCommon.Instance.GetContext(TestCommon.VivaTopicCenterTestSite, 3))
            {
                Assert.IsTrue(context.Web.HasCommunicationSiteFeatures());
            }
        }

        [TestMethod]
        public void SearchBasicTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {

                SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
                {
                    RowLimit = 10,
                    TrimDuplicates = false,
                    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" }
                };

                var searchResult = context.Web.Search(searchOptions);

                Assert.IsTrue(searchResult != null);
                Assert.IsTrue(searchResult.ElapsedTime > 0);
                Assert.IsTrue(searchResult.TotalRows > 0);
                Assert.IsTrue(searchResult.TotalRowsIncludingDuplicates > 0);
                Assert.IsTrue(searchResult.Rows.Count == 10);
                foreach (var row in searchResult.Rows)
                {
                    Assert.IsTrue(row.ContainsKey("Path"));
                }
            }
        }

        [TestMethod]
        public async Task SearchBasicBatchTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {

                SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
                {
                    RowLimit = 10,
                    TrimDuplicates = false,
                    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" }
                };

                var searchResult = context.Web.SearchBatch(searchOptions);
                Assert.IsFalse(searchResult.IsAvailable);

                await context.ExecuteAsync();

                Assert.IsTrue(searchResult.IsAvailable);

                Assert.IsTrue(searchResult != null);
                Assert.IsTrue(searchResult.Result.ElapsedTime > 0);
                Assert.IsTrue(searchResult.Result.TotalRows > 0);
                Assert.IsTrue(searchResult.Result.TotalRowsIncludingDuplicates > 0);
                Assert.IsTrue(searchResult.Result.Rows.Count == 10);

            }
        }

        [TestMethod]
        public void SearchSortingPagingTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                SearchOptions searchOptions = new SearchOptions("contenttypeid:\"0x010100*\"")
                {
                    RowsPerPage = 10,
                    TrimDuplicates = false,
                    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
                    SortProperties = new System.Collections.Generic.List<SortOption>() { new SortOption("DocId"), new SortOption("ModifiedBy", SortDirection.Ascending) },
                };

                var searchResult = context.Web.Search(searchOptions);

                Assert.IsTrue(searchResult != null);
                Assert.IsTrue(searchResult.ElapsedTime > 0);
                Assert.IsTrue(searchResult.TotalRows > 0);
                Assert.IsTrue(searchResult.TotalRowsIncludingDuplicates > 0);
                Assert.IsTrue(searchResult.Rows.Count == 10);

                // Load next page
                searchOptions.StartRow = 10;
                searchResult = context.Web.Search(searchOptions);

                Assert.IsTrue(searchResult != null);
                Assert.IsTrue(searchResult.ElapsedTime > 0);
                Assert.IsTrue(searchResult.TotalRows > 0);
                Assert.IsTrue(searchResult.TotalRowsIncludingDuplicates > 0);
                Assert.IsTrue(searchResult.Rows.Count == 10);
            }
        }

        [TestMethod]
        public void SearchRefinerTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {

                SearchOptions searchOptions = new SearchOptions("contentclass:STS_ListItem_DocumentLibrary")
                {
                    RowsPerPage = 10,
                    TrimDuplicates = false,
                    SelectProperties = new System.Collections.Generic.List<string>() { "Path", "Url", "Title", "ListId" },
                    SortProperties = new System.Collections.Generic.List<SortOption>() { new SortOption("DocId") },
                    RefineProperties = new System.Collections.Generic.List<string>() { "ContentTypeId" }
                };

                var searchResult = context.Web.Search(searchOptions);

                Assert.IsTrue(searchResult != null);
                Assert.IsTrue(searchResult.ElapsedTime > 0);
                Assert.IsTrue(searchResult.TotalRows > 0);
                Assert.IsTrue(searchResult.TotalRowsIncludingDuplicates > 0);
                Assert.IsTrue(searchResult.Rows.Count == 10);
                Assert.IsTrue(searchResult.Refinements.Count > 0);
                foreach (var refiner in searchResult.Refinements)
                {
                    Assert.IsTrue(!string.IsNullOrEmpty(refiner.Key));
                    foreach (var refinementResult in refiner.Value)
                    {
                        Assert.IsTrue(refinementResult.Count > 0);
                        Assert.IsTrue(!string.IsNullOrEmpty(refinementResult.Value));
                        Assert.IsTrue(!string.IsNullOrEmpty(refinementResult.Token));
                        Assert.IsTrue(!string.IsNullOrEmpty(refinementResult.Name));
                    }
                }

            }
        }

        [TestMethod]
        public async Task FindFilesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Create new lists
                string list1Title = TestCommon.GetPnPSdkTestAssetName("FindFilesAsyncTest1_WEB");
                var myList1 = context.Web.Lists.GetByTitle(list1Title);

                if (TestCommon.Instance.Mocking && myList1 != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList1 == null)
                {
                    myList1 = await context.Web.Lists.AddAsync(list1Title, ListTemplateType.DocumentLibrary);
                }

                string list2Title = TestCommon.GetPnPSdkTestAssetName("FindFilesAsyncTest2_WEB");
                var myList2 = context.Web.Lists.GetByTitle(list2Title);

                if (TestCommon.Instance.Mocking && myList2 != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList2 == null)
                {
                    myList2 = await context.Web.Lists.AddAsync(list2Title, ListTemplateType.DocumentLibrary);
                }

                // Add files to the lists
                using (MemoryStream ms = new())
                {
                    var sw = new StreamWriter(ms, System.Text.Encoding.Unicode);
                    try
                    {
                        sw.Write("[Your name here]");
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        for (int i = 0; i < 5; i++)
                        {
                            myList1.RootFolder.Files.Add($"367664-472-E-T0{i} - Artichoke.txt", ms);
                        }

                        var subfolder = myList1.RootFolder.AddFolder("subfolder");
                        var subsubfolder = subfolder.AddFolder("subsubfolder");
                        for (int i = 0; i < 2; i++)
                        {
                            subsubfolder.Files.Add($"872374-522-F-T0{i} - Cucumber.txt", ms);
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            myList2.RootFolder.Files.Add($"98455-332-F-T0{i} - Artichoke.txt", ms);
                        }
                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }

                var result = await context.Web.FindFilesAsync("*ArTicHoke*");
                Assert.IsTrue(result.Count == 7);

                var result2 = await context.Web.FindFilesAsync("*F-T01*");
                Assert.IsTrue(result2.Count == 2);

                await myList1.DeleteAsync();
                await myList2.DeleteAsync();
            }
        }

        [TestMethod]
        public async Task FindFilesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                // Create new lists
                string listTitle = TestCommon.GetPnPSdkTestAssetName("FindFilesTest_WEB");
                var myList = context.Web.Lists.GetByTitle(listTitle);

                if (TestCommon.Instance.Mocking && myList != null)
                {
                    Assert.Inconclusive("Test data set should be setup to not have the list available.");
                }

                if (myList == null)
                {
                    myList = await context.Web.Lists.AddAsync(listTitle, ListTemplateType.DocumentLibrary);
                }

                // Add file to the list
                using (MemoryStream ms = new())
                {
                    var sw = new StreamWriter(ms, System.Text.Encoding.Unicode);
                    try
                    {
                        sw.Write("[Your name here]");
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);

                        myList.RootFolder.Files.Add($"367664-472-E-X01 - Artichoke.txt", ms);

                    }
                    finally
                    {
                        sw.Dispose();
                    }
                }

                var result = context.Web.FindFiles("*E-x01*");
                Assert.IsTrue(result.Count == 1);

                await myList.DeleteAsync();
            }
        }
    }
}
