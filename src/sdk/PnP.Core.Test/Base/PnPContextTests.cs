using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Test.Utilities;

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
        public async Task DefaultPropertiesOnInit()
        {
            //TestCommon.Instance.Mocking = false;

            var options = new PnPContextFactoryOptions()
            {
                DefaultSitePropertiesOnCreate = new Expression<Func<ISite, object>>[]
                {
                    s => s.AllowCreateDeclarativeWorkflow,
                    s => s.AllowDesigner,
                    s => s.AllowExternalEmbeddingWrapper,
                    s => s.AllowMasterPageEditing,
                    s => s.AllowRevertFromTemplate,
                    s => s.AllowSaveDeclarativeWorkflowAsTemplate,
                    s => s.AllowSavePublishDeclarativeWorkflow,
                    s => s.AuditLogTrimmingRetention,
                    s => s.CanSyncHubSitePermissions,
                    s => s.ChannelGroupId,
                    s => s.Classification,
                    s => s.CommentsOnSitePagesDisabled,
                    s => s.DisableAppViews,
                    s => s.DisableCompanyWideSharingLinks,
                    s => s.DisableFlows,
                    s => s.ExternalSharingTipsEnabled,
                    s => s.ExternalUserExpirationInDays,
                    s => s.GeoLocation,
                    s => s.GroupId
                },
                DefaultWebPropertiesOnCreate = new Expression<Func<IWeb, object>>[]
                {
                    w => w.AccessRequestListUrl,
                    w => w.AccessRequestSiteDescription,
                    w => w.AllowCreateDeclarativeWorkflowForCurrentUser,
                    w => w.AllowDesignerForCurrentUser,
                    w => w.AllowMasterPageEditingForCurrentUser,
                    w => w.AllowRevertFromTemplateForCurrentUser,
                    w => w.AllowRssFeeds,
                    w => w.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser,
                    w => w.AllowSavePublishDeclarativeWorkflowForCurrentUser,
                    w => w.AllProperties,
                    w => w.AlternateCssUrl,
                    w => w.AppInstanceId,
                    w => w.ContainsConfidentialInfo,
                    w => w.Created,
                    w => w.CustomMasterUrl,
                    w => w.DesignPackageId,
                    w => w.DisableRecommendedItems,
                    w => w.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled,
                    w => w.EffectiveBasePermissions,
                    w => w.EnableMinimalDownload,
                    w => w.FooterEmphasis,
                    w => w.FooterEnabled,
                    w => w.FooterLayout
                }
            };

            using (var context = await TestCommon.Instance.GetContextWithOptionsAsync(TestCommon.TestSite, factoryOptions: options))
            {
                Assert.IsNotNull(context.Web);
                Assert.IsNotNull(context.Site);

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
                Assert.IsTrue(web.AllProperties.Count > 0);
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

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.IsFalse(site.AllowCreateDeclarativeWorkflow);
                Assert.IsTrue(site.AllowDesigner);
                Assert.AreNotEqual(0, site.AllowExternalEmbeddingWrapper);
                Assert.IsFalse(site.AllowMasterPageEditing);
                Assert.IsFalse(site.AllowRevertFromTemplate);
                Assert.IsFalse(site.AllowSaveDeclarativeWorkflowAsTemplate);
                Assert.IsFalse(site.AllowSavePublishDeclarativeWorkflow);
                Assert.AreNotEqual(0, site.AuditLogTrimmingRetention);
                Assert.IsFalse(site.CanSyncHubSitePermissions);
                Assert.AreEqual(default, site.ChannelGroupId);
                Assert.IsNotNull(site.Classification);
                Assert.IsFalse(site.CommentsOnSitePagesDisabled);
                Assert.IsFalse(site.DisableAppViews);
                Assert.IsFalse(site.DisableCompanyWideSharingLinks);
                Assert.IsFalse(site.DisableFlows);
                Assert.IsFalse(site.ExternalSharingTipsEnabled);
                Assert.AreEqual(0, site.ExternalUserExpirationInDays);
                Assert.AreNotEqual("", site.GeoLocation);
                Assert.AreNotEqual(default, site.GroupId);
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
        public async Task ContextCloningForSameSiteDefaultPropertiesOnInit()
        {
            //TestCommon.Instance.Mocking = false;
            var options = new PnPContextFactoryOptions()
            {
                DefaultSitePropertiesOnCreate = new Expression<Func<ISite, object>>[]
                {
                    s => s.AllowCreateDeclarativeWorkflow,
                    s => s.AllowDesigner,
                    s => s.AllowExternalEmbeddingWrapper,
                    s => s.AllowMasterPageEditing,
                    s => s.AllowRevertFromTemplate,
                    s => s.AllowSaveDeclarativeWorkflowAsTemplate,
                    s => s.AllowSavePublishDeclarativeWorkflow,
                    s => s.AuditLogTrimmingRetention,
                    s => s.CanSyncHubSitePermissions,
                    s => s.ChannelGroupId,
                    s => s.Classification,
                    s => s.CommentsOnSitePagesDisabled,
                    s => s.DisableAppViews,
                    s => s.DisableCompanyWideSharingLinks,
                    s => s.DisableFlows,
                    s => s.ExternalSharingTipsEnabled,
                    s => s.ExternalUserExpirationInDays,
                    s => s.GeoLocation,
                    s => s.GroupId
                },
                DefaultWebPropertiesOnCreate = new Expression<Func<IWeb, object>>[]
                {
                    w => w.AccessRequestListUrl,
                    w => w.AccessRequestSiteDescription,
                    w => w.AllowCreateDeclarativeWorkflowForCurrentUser,
                    w => w.AllowDesignerForCurrentUser,
                    w => w.AllowMasterPageEditingForCurrentUser,
                    w => w.AllowRevertFromTemplateForCurrentUser,
                    w => w.AllowRssFeeds,
                    w => w.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser,
                    w => w.AllowSavePublishDeclarativeWorkflowForCurrentUser,
                    // TODO: w => w.AllProperties,
                    w => w.AlternateCssUrl,
                    w => w.AppInstanceId,
                    w => w.ContainsConfidentialInfo,
                    w => w.Created,
                    w => w.CustomMasterUrl,
                    w => w.DesignPackageId,
                    w => w.DisableRecommendedItems,
                    w => w.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled,
                    w => w.EffectiveBasePermissions,
                    w => w.EnableMinimalDownload,
                    w => w.FooterEmphasis,
                    w => w.FooterEnabled,
                    w => w.FooterLayout
                }
            };

            using (var context = await TestCommon.Instance.GetContextWithOptionsAsync(TestCommon.TestSite, factoryOptions: options))
            {
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

                    Assert.AreEqual(context.ContextOptions.DefaultSitePropertiesOnCreate, clonedContext.ContextOptions.DefaultSitePropertiesOnCreate);
                    Assert.AreEqual(context.ContextOptions.DefaultWebPropertiesOnCreate, clonedContext.ContextOptions.DefaultWebPropertiesOnCreate);

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

                    Assert.AreEqual(context.Site.AllowCreateDeclarativeWorkflow, clonedContext.Site.AllowCreateDeclarativeWorkflow);
                    Assert.AreEqual(context.Site.AllowDesigner, clonedContext.Site.AllowDesigner);
                    Assert.AreEqual(context.Site.AllowExternalEmbeddingWrapper, clonedContext.Site.AllowExternalEmbeddingWrapper);
                    Assert.AreEqual(context.Site.AllowMasterPageEditing, clonedContext.Site.AllowMasterPageEditing);
                    Assert.AreEqual(context.Site.AllowRevertFromTemplate, clonedContext.Site.AllowRevertFromTemplate);
                    Assert.AreEqual(context.Site.AllowSaveDeclarativeWorkflowAsTemplate, clonedContext.Site.AllowSaveDeclarativeWorkflowAsTemplate);
                    Assert.AreEqual(context.Site.AllowSavePublishDeclarativeWorkflow, clonedContext.Site.AllowSavePublishDeclarativeWorkflow);
                    Assert.AreEqual(context.Site.AuditLogTrimmingRetention, clonedContext.Site.AuditLogTrimmingRetention);
                    Assert.AreEqual(context.Site.CanSyncHubSitePermissions, clonedContext.Site.CanSyncHubSitePermissions);
                    Assert.AreEqual(context.Site.ChannelGroupId, clonedContext.Site.ChannelGroupId);
                    Assert.AreEqual(context.Site.Classification, clonedContext.Site.Classification);
                    Assert.AreEqual(context.Site.CommentsOnSitePagesDisabled, clonedContext.Site.CommentsOnSitePagesDisabled);
                    Assert.AreEqual(context.Site.DisableAppViews, clonedContext.Site.DisableAppViews);
                    Assert.AreEqual(context.Site.DisableCompanyWideSharingLinks, clonedContext.Site.DisableCompanyWideSharingLinks);
                    Assert.AreEqual(context.Site.DisableFlows, clonedContext.Site.DisableFlows);
                    Assert.AreEqual(context.Site.ExternalSharingTipsEnabled, clonedContext.Site.ExternalSharingTipsEnabled);
                    Assert.AreEqual(context.Site.ExternalUserExpirationInDays, clonedContext.Site.ExternalUserExpirationInDays);
                    Assert.AreEqual(context.Site.GeoLocation, clonedContext.Site.GeoLocation);
                    Assert.AreEqual(context.Site.GroupId, clonedContext.Site.GroupId);

                    Assert.AreEqual(context.Web.AccessRequestListUrl, clonedContext.Web.AccessRequestListUrl);
                    Assert.AreEqual(context.Web.AccessRequestSiteDescription, clonedContext.Web.AccessRequestSiteDescription);
                    Assert.AreEqual(context.Web.AllowCreateDeclarativeWorkflowForCurrentUser, clonedContext.Web.AllowCreateDeclarativeWorkflowForCurrentUser);
                    Assert.AreEqual(context.Web.AllowDesignerForCurrentUser, clonedContext.Web.AllowDesignerForCurrentUser);
                    Assert.AreEqual(context.Web.AllowMasterPageEditingForCurrentUser, clonedContext.Web.AllowMasterPageEditingForCurrentUser);
                    Assert.AreEqual(context.Web.AllowRevertFromTemplateForCurrentUser, clonedContext.Web.AllowRevertFromTemplateForCurrentUser);
                    Assert.AreEqual(context.Web.AllowRssFeeds, clonedContext.Web.AllowRssFeeds);
                    Assert.AreEqual(context.Web.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser, clonedContext.Web.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser);
                    Assert.AreEqual(context.Web.AllowSavePublishDeclarativeWorkflowForCurrentUser, clonedContext.Web.AllowSavePublishDeclarativeWorkflowForCurrentUser);
                    //Assert.AreEqual(context.Web.AllProperties.Count, clonedContext.Web.AllProperties.Count);
                    Assert.AreEqual(context.Web.AlternateCssUrl, clonedContext.Web.AlternateCssUrl);
                    Assert.AreEqual(context.Web.AppInstanceId, clonedContext.Web.AppInstanceId);
                    Assert.AreEqual(context.Web.ContainsConfidentialInfo, clonedContext.Web.ContainsConfidentialInfo);
                    Assert.AreEqual(context.Web.Created, clonedContext.Web.Created);
                    Assert.AreEqual(context.Web.CustomMasterUrl, clonedContext.Web.CustomMasterUrl);
                    Assert.AreEqual(context.Web.DesignPackageId, clonedContext.Web.DesignPackageId);
                    Assert.AreEqual(context.Web.DisableRecommendedItems, clonedContext.Web.DisableRecommendedItems);
                    Assert.AreEqual(context.Web.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled, clonedContext.Web.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled);
                    Assert.AreEqual(context.Web.EffectiveBasePermissions, clonedContext.Web.EffectiveBasePermissions);
                    Assert.AreEqual(context.Web.EnableMinimalDownload, clonedContext.Web.EnableMinimalDownload);
                    Assert.AreEqual(context.Web.FooterEmphasis, clonedContext.Web.FooterEmphasis);
                    Assert.AreEqual(context.Web.FooterEnabled, clonedContext.Web.FooterEnabled);
                    Assert.AreEqual(context.Web.FooterLayout, clonedContext.Web.FooterLayout);
                }
            }
        }

        [TestMethod]
        public async Task ContextCloningForOtherSiteDefaultPropertiesOnInit()
        {
            //TestCommon.Instance.Mocking = false;
            var options = new PnPContextFactoryOptions()
            {
                DefaultSitePropertiesOnCreate = new Expression<Func<ISite, object>>[]
                {
                    s => s.AllowCreateDeclarativeWorkflow,
                    s => s.AllowDesigner,
                    s => s.AllowExternalEmbeddingWrapper,
                    s => s.AllowMasterPageEditing,
                    s => s.AllowRevertFromTemplate,
                    s => s.AllowSaveDeclarativeWorkflowAsTemplate,
                    s => s.AllowSavePublishDeclarativeWorkflow,
                    s => s.AuditLogTrimmingRetention,
                    s => s.CanSyncHubSitePermissions,
                    s => s.ChannelGroupId,
                    s => s.Classification,
                    s => s.CommentsOnSitePagesDisabled,
                    s => s.DisableAppViews,
                    s => s.DisableCompanyWideSharingLinks,
                    s => s.DisableFlows,
                    s => s.ExternalSharingTipsEnabled,
                    s => s.ExternalUserExpirationInDays,
                    s => s.GeoLocation,
                    s => s.GroupId
                },
                DefaultWebPropertiesOnCreate = new Expression<Func<IWeb, object>>[]
                {
                    w => w.AccessRequestListUrl,
                    w => w.AccessRequestSiteDescription,
                    w => w.AllowCreateDeclarativeWorkflowForCurrentUser,
                    w => w.AllowDesignerForCurrentUser,
                    w => w.AllowMasterPageEditingForCurrentUser,
                    w => w.AllowRevertFromTemplateForCurrentUser,
                    w => w.AllowRssFeeds,
                    w => w.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser,
                    w => w.AllowSavePublishDeclarativeWorkflowForCurrentUser,
                    w => w.AllProperties,
                    w => w.AlternateCssUrl,
                    w => w.AppInstanceId,
                    w => w.ContainsConfidentialInfo,
                    w => w.Created,
                    w => w.CustomMasterUrl,
                    w => w.DesignPackageId,
                    w => w.DisableRecommendedItems,
                    w => w.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled,
                    w => w.EffectiveBasePermissions,
                    w => w.EnableMinimalDownload,
                    w => w.FooterEmphasis,
                    w => w.FooterEnabled,
                    w => w.FooterLayout,
                    w => w.ServerRelativeUrl,
                    w => w.Title
                }
            };

            using (var context = await TestCommon.Instance.GetContextWithOptionsAsync(TestCommon.TestSite, factoryOptions: options))
            {
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

                    Assert.AreEqual(context.ContextOptions.DefaultSitePropertiesOnCreate, clonedContext.ContextOptions.DefaultSitePropertiesOnCreate);
                    Assert.AreEqual(context.ContextOptions.DefaultWebPropertiesOnCreate, clonedContext.ContextOptions.DefaultWebPropertiesOnCreate);

                    Assert.AreNotEqual(context.Id, clonedContext.Id);
                    Assert.AreNotEqual(context.Web.Id, clonedContext.Web.Id);

                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowCreateDeclarativeWorkflow));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowDesigner));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowExternalEmbeddingWrapper));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowMasterPageEditing));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowRevertFromTemplate));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowSaveDeclarativeWorkflowAsTemplate));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AllowSavePublishDeclarativeWorkflow));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.AuditLogTrimmingRetention));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.CanSyncHubSitePermissions));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.ChannelGroupId));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.Classification));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.CommentsOnSitePagesDisabled));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.DisableAppViews));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.DisableCompanyWideSharingLinks));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.DisableFlows));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.ExternalSharingTipsEnabled));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.ExternalUserExpirationInDays));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.GeoLocation));
                    Assert.IsTrue(clonedContext.Site.IsPropertyAvailable(s => s.GroupId));

                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AccessRequestListUrl));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AccessRequestSiteDescription));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowCreateDeclarativeWorkflowForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowDesignerForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowMasterPageEditingForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowRevertFromTemplateForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowRssFeeds));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowSaveDeclarativeWorkflowAsTemplateForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllowSavePublishDeclarativeWorkflowForCurrentUser));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AllProperties));
                    Assert.IsTrue(clonedContext.Web.AllProperties.Count > 0);
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AlternateCssUrl));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.AppInstanceId));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.ContainsConfidentialInfo));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.Created));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.CustomMasterUrl));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.DesignPackageId));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.DisableRecommendedItems));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.DocumentLibraryCalloutOfficeWebAppPreviewersDisabled));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.EffectiveBasePermissions));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.EnableMinimalDownload));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.FooterEmphasis));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.FooterEnabled));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.FooterLayout));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.ServerRelativeUrl));
                    Assert.IsTrue(clonedContext.Web.IsPropertyAvailable(w => w.Title));

                    Assert.AreNotEqual(context.Web.ServerRelativeUrl, clonedContext.Web.ServerRelativeUrl);
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
