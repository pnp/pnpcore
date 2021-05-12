using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;
using System;
using System.Threading.Tasks;
using PnP.Core.Model;

namespace PnP.Core.Test.Base
{
    /// <summary>
    /// Tests that focus on PnPContext specifics
    /// </summary>
    [TestClass]
    public class PnPContextTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext testContext)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task PropertiesInitialization()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsNotNull(context.Web);
                Assert.IsNotNull(context.Site);
                Assert.IsNotNull(context.Team);
                Assert.IsNotNull(context.Uri);
                Assert.IsNotNull(context.RestClient);
                Assert.IsNotNull(context.GraphClient);
                Assert.IsNotNull(context.Logger);
                Assert.IsNotNull(context.BatchClient);
                Assert.IsNotNull(context.CurrentBatch);
            }
        }

        [TestMethod]
        public async Task PendingRequests()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                Assert.IsFalse(context.HasPendingRequests);

                await context.Web.GetBatchAsync(p => p.Title);

                Assert.IsTrue(context.HasPendingRequests);

                await context.ExecuteAsync();

                Assert.IsFalse(context.HasPendingRequests);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForRootContextViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.GraphFirst = false;

                await context.Web.GetAsync();

                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id == context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForRootContextViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.GetAsync();

                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id == context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForSubSiteContextViaRest()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                context.GraphFirst = false;

                await context.Web.LoadAsync();

                // Since we've not loaded the actual web that's the rootweb, this means that the site rootweb property is not yet loaded
                Assert.IsFalse(context.Site.IsPropertyAvailable(p => p.RootWeb));

                // Load site rootweb and check again
                await context.Site.LoadAsync(p => p.RootWeb);
                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id != context.Web.Id);
            }
        }

        [TestMethod]
        public async Task RootWebPopulatedForSubSiteContextViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSubSite))
            {
                await context.Web.LoadAsync();

                // Since we've not loaded the actual web that's the rootweb, this means that the site rootweb property is not yet loaded
                Assert.IsFalse(context.Site.IsPropertyAvailable(p => p.RootWeb));

                // Load site rootweb and check again
                await context.Site.LoadAsync(p => p.RootWeb);
                Assert.IsTrue(context.Site.IsPropertyAvailable(p => p.RootWeb));
                Assert.IsTrue(context.Site.RootWeb.Id != context.Web.Id);
            }
        }

        [TestMethod]
        public async Task SkipLoadingTeamForNoGroupSitesViaGraph()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.NoGroupTestSite))
            {
                var team = await context.Team.GetAsync();

                // Requested stays false as there's no group connected to this site, so also no team
                Assert.IsFalse(context.Team.Requested);
            }
        }

        [TestMethod]
        public async Task CreateContextFromGroupId()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.EnsurePropertiesAsync(p => p.GroupId);
                // Group id should be loaded
                Assert.IsTrue(context.Site.GroupId != Guid.Empty);

                // Create a new context using this group id
                using (var context2 = await TestCommon.Instance.GetContextAsync(context.Site.GroupId, 1))
                {
                    Assert.IsTrue(context2.Group.Requested == true);
                    Assert.IsTrue(context2.Group.IsPropertyAvailable(p => p.WebUrl) == true);
                    Assert.IsTrue(context2.Uri != null);

                    // Try to get SharePoint and Teams information from a context created via a group id
                    var web = await context2.Web.GetAsync(p => p.Title);

                    Assert.IsTrue(web.Requested);
                    Assert.IsTrue(web.IsPropertyAvailable(p => p.Title));

                    var site = await context2.Site.GetAsync(p => p.GroupId);

                    Assert.IsTrue(site.Requested);
                    Assert.IsTrue(site.IsPropertyAvailable(p => p.GroupId));
                    Assert.IsTrue(site.GroupId == context.Site.GroupId);

                    var team = await context2.Team.GetAsync(p => p.DisplayName);

                    Assert.IsTrue(team.Requested);
                    Assert.IsTrue(team.IsPropertyAvailable(p => p.DisplayName));
                }
            }
        }

        [TestMethod]
        public async Task UseGroupMethodsFromRegularContext()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                // Ensure the metadata on the group is populated
                Assert.IsTrue((context.Group as IMetadataExtensible).Metadata.ContainsKey(PnPConstants.MetaDataGraphId));

                // Perform a Graph call via the group
                await context.Group.EnsurePropertiesAsync(g => g.Visibility);
                Assert.IsTrue(context.Group.IsPropertyAvailable(p => p.Visibility));
            }
        }

        [TestMethod]
        public async Task ContextCloningForSameSite()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.Title);

                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, 2))
                {
                    Assert.AreEqual(context.Uri, clonedContext.Uri);
                    Assert.AreEqual(context.AuthenticationProvider, clonedContext.AuthenticationProvider);

                    Assert.AreEqual(context.GraphAlwaysUseBeta, clonedContext.GraphAlwaysUseBeta);
                    Assert.AreEqual(context.GraphCanUseBeta, clonedContext.GraphCanUseBeta);
                    Assert.AreEqual(context.GraphFirst, clonedContext.GraphFirst);

                    Assert.AreEqual(context.RestClient, clonedContext.RestClient);
                    Assert.AreEqual(context.GraphClient, clonedContext.GraphClient);
                    Assert.AreEqual(context.Logger, clonedContext.Logger);

                    // Cloning for the same site will not run context initialization to save on performance but will copy 
                    // the earlier loaded data into the new model. Validate that everything needed was copied over
                    Assert.AreEqual(context.Web.Id, clonedContext.Web.Id);
                    Assert.AreEqual(context.Web.Url, clonedContext.Web.Url);

                    Assert.AreEqual(context.Web.RegionalSettings.AM, clonedContext.Web.RegionalSettings.AM);
                    Assert.AreEqual(context.Web.RegionalSettings.CollationLCID, clonedContext.Web.RegionalSettings.CollationLCID);
                    Assert.AreEqual(context.Web.RegionalSettings.DateFormat, clonedContext.Web.RegionalSettings.DateFormat);
                    Assert.AreEqual(context.Web.RegionalSettings.DateSeparator, clonedContext.Web.RegionalSettings.DateSeparator);
                    Assert.AreEqual(context.Web.RegionalSettings.DecimalSeparator, clonedContext.Web.RegionalSettings.DecimalSeparator);
                    Assert.AreEqual(context.Web.RegionalSettings.DigitGrouping, clonedContext.Web.RegionalSettings.DigitGrouping);
                    Assert.AreEqual(context.Web.RegionalSettings.FirstDayOfWeek, clonedContext.Web.RegionalSettings.FirstDayOfWeek);
                    Assert.AreEqual(context.Web.RegionalSettings.IsEastAsia, clonedContext.Web.RegionalSettings.IsEastAsia);
                    Assert.AreEqual(context.Web.RegionalSettings.IsRightToLeft, clonedContext.Web.RegionalSettings.IsRightToLeft);
                    Assert.AreEqual(context.Web.RegionalSettings.IsUIRightToLeft, clonedContext.Web.RegionalSettings.IsUIRightToLeft);
                    Assert.AreEqual(context.Web.RegionalSettings.ListSeparator, clonedContext.Web.RegionalSettings.ListSeparator);
                    Assert.AreEqual(context.Web.RegionalSettings.LocaleId, clonedContext.Web.RegionalSettings.LocaleId);
                    Assert.AreEqual(context.Web.RegionalSettings.NegativeSign, clonedContext.Web.RegionalSettings.NegativeSign);
                    Assert.AreEqual(context.Web.RegionalSettings.NegNumberMode, clonedContext.Web.RegionalSettings.NegNumberMode);
                    Assert.AreEqual(context.Web.RegionalSettings.PM, clonedContext.Web.RegionalSettings.PM);
                    Assert.AreEqual(context.Web.RegionalSettings.PositiveSign, clonedContext.Web.RegionalSettings.PositiveSign);
                    Assert.AreEqual(context.Web.RegionalSettings.ShowWeeks, clonedContext.Web.RegionalSettings.ShowWeeks);
                    Assert.AreEqual(context.Web.RegionalSettings.ThousandSeparator, clonedContext.Web.RegionalSettings.ThousandSeparator);
                    Assert.AreEqual(context.Web.RegionalSettings.Time24, clonedContext.Web.RegionalSettings.Time24);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeMarkerPosition, clonedContext.Web.RegionalSettings.TimeMarkerPosition);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeSeparator, clonedContext.Web.RegionalSettings.TimeSeparator);

                    Assert.AreEqual(context.Web.RegionalSettings.TimeZone.Description, clonedContext.Web.RegionalSettings.TimeZone.Description);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeZone.Bias, clonedContext.Web.RegionalSettings.TimeZone.Bias);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeZone.DaylightBias, clonedContext.Web.RegionalSettings.TimeZone.DaylightBias);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeZone.Id, clonedContext.Web.RegionalSettings.TimeZone.Id);
                    Assert.AreEqual(context.Web.RegionalSettings.TimeZone.StandardBias, clonedContext.Web.RegionalSettings.TimeZone.StandardBias);

                    Assert.AreEqual(context.Site.Id, clonedContext.Site.Id);
                    Assert.AreEqual(context.Site.GroupId, clonedContext.Site.GroupId);

                    // This is a new context, so id's have to be different
                    Assert.AreNotEqual(context.Id, clonedContext.Id);
                }

                // Since test cases work with mocking data we need to use a custom Clone method, this one will use
                // the PnPContext.Clone method and additionally will copy of the "test" settings
                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, 1))
                {
                    await clonedContext.Web.LoadAsync(p => p.Title);

                    Assert.AreEqual(context.Web.Title, clonedContext.Web.Title);
                }
            }
        }

        [TestMethod]
        public async Task ContextCloningForOtherSite()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.Title);

                var otherSite = TestCommon.Instance.TestUris[TestCommon.TestSubSite];

                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, otherSite, 2))
                {
                    Assert.AreNotEqual(context.Uri, clonedContext.Uri);
                    Assert.AreEqual(context.AuthenticationProvider, clonedContext.AuthenticationProvider);

                    Assert.AreEqual(context.GraphAlwaysUseBeta, clonedContext.GraphAlwaysUseBeta);
                    Assert.AreEqual(context.GraphCanUseBeta, clonedContext.GraphCanUseBeta);
                    Assert.AreEqual(context.GraphFirst, clonedContext.GraphFirst);

                    Assert.AreEqual(context.RestClient, clonedContext.RestClient);
                    Assert.AreEqual(context.GraphClient, clonedContext.GraphClient);
                    Assert.AreEqual(context.Logger, clonedContext.Logger);

                    Assert.AreNotEqual(context.Id, clonedContext.Id);
                }

                // Since test cases work with mocking data we need to use a custom Clone method, this one will use
                // the PnPContext.Clone method and additionally will copy of the "test" settings
                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, otherSite, 1))
                {
                    await clonedContext.Web.LoadAsync(p => p.Title);

                    Assert.AreNotEqual(context.Web.Title, clonedContext.Web.Title);
                }
            }
        }

        [TestMethod]
        public async Task ContextCloningForOtherConfiguration()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Web.LoadAsync(p => p.Title);

                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, TestCommon.TestSubSite, 2))
                {
                    Assert.AreNotEqual(context.Uri, clonedContext.Uri);
                    Assert.AreEqual(context.AuthenticationProvider, clonedContext.AuthenticationProvider);

                    Assert.AreEqual(context.GraphAlwaysUseBeta, clonedContext.GraphAlwaysUseBeta);
                    Assert.AreEqual(context.GraphCanUseBeta, clonedContext.GraphCanUseBeta);
                    Assert.AreEqual(context.GraphFirst, clonedContext.GraphFirst);

                    Assert.AreEqual(context.RestClient, clonedContext.RestClient);
                    Assert.AreEqual(context.GraphClient, clonedContext.GraphClient);
                    Assert.AreEqual(context.Logger, clonedContext.Logger);

                    Assert.AreNotEqual(context.Id, clonedContext.Id);
                }

                // Since test cases work with mocking data we need to use a custom Clone method, this one will use
                // the PnPContext.Clone method and additionally will copy of the "test" settings
                using (var clonedContext = await TestCommon.Instance.CloneAsync(context, TestCommon.TestSubSite, 1))
                {
                    await clonedContext.Web.LoadAsync(p => p.Title);

                    Assert.AreNotEqual(context.Web.Title, clonedContext.Web.Title);
                }
            }
        }

        [TestMethod]
        public async Task SetAADTenant()
        {
            PnPContextFactoryOptions options = new PnPContextFactoryOptions();
            PnPGlobalSettingsOptions globalOptions = new PnPGlobalSettingsOptions();
            using (var context = new PnPContext(logger: null, authenticationProvider: null, sharePointRestClient: null, microsoftGraphClient: null, contextOptions: options, globalOptions: globalOptions, telemetryManager: null))
            {
                context.Uri = new Uri("https://officedevpnp.sharepoint.com/sites/PnPCoreSDKDoNotDelete");

                await context.SetAADTenantId();

                Assert.AreEqual(globalOptions.AADTenantId, Guid.Parse("73da091f-a58d-405f-9015-9bd386425255"));
            }
        }

    }
}
