using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using System.Threading.Tasks;
using PnP.Core.Model;

namespace PnP.Core.Test.SharePoint
{
    [TestClass]
    public class SiteTests
    {
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_A_G_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.AllowCreateDeclarativeWorkflow,
                    p => p.AllowDesigner,
                    p => p.AllowExternalEmbeddingWrapper,
                    p => p.AllowMasterPageEditing,
                    p => p.AllowRevertFromTemplate,
                    p => p.AllowSaveDeclarativeWorkflowAsTemplate,
                    p => p.AllowSavePublishDeclarativeWorkflow,
                    p => p.AuditLogTrimmingRetention,
                    p => p.CanSyncHubSitePermissions,
                    p => p.ChannelGroupId,
                    p => p.Classification,
                    p => p.CommentsOnSitePagesDisabled,
                    p => p.DisableAppViews,
                    p => p.DisableCompanyWideSharingLinks,
                    p => p.DisableFlows,
                    p => p.ExternalSharingTipsEnabled,
                    p => p.ExternalUserExpirationInDays,
                    p => p.GeoLocation,
                    p => p.GroupId
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.IsTrue(site.AllowCreateDeclarativeWorkflow);
                Assert.IsTrue(site.AllowDesigner);
                Assert.AreNotEqual(0, site.AllowExternalEmbeddingWrapper);
                Assert.IsFalse(site.AllowMasterPageEditing);
                Assert.IsFalse(site.AllowRevertFromTemplate);
                Assert.IsTrue(site.AllowSaveDeclarativeWorkflowAsTemplate);
                Assert.IsTrue(site.AllowSavePublishDeclarativeWorkflow);
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
        public async Task GetSiteSimpleProperties_H_R_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite,
                    p => p.LockIssue,
                    p => p.MaxItemsPerThrottledOperation,
                    p => p.ReadOnly,
                    p => p.RelatedGroupId
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);
                Assert.IsNull(site.LockIssue);
                Assert.AreNotEqual(0, site.MaxItemsPerThrottledOperation);
                Assert.IsFalse(site.ReadOnly);
                Assert.AreNotEqual(default, site.RelatedGroupId);
            }
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_S_Z_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                await context.Site.LoadAsync(
                    p => p.SearchBoxInNavBar,
                    p => p.SearchBoxPlaceholderText,
                    p => p.SensitivityLabelId,
                    p => p.SensitivityLabel,
                    p => p.ServerRelativeUrl,
                    p => p.ShareByEmailEnabled,
                    p => p.ShareByLinkEnabled,
                    p => p.ShowPeoplePickerSuggestionsForGuestUsers,
                    p => p.SocialBarOnSitePagesDisabled,
                    p => p.StatusBarLink,
                    p => p.StatusBarText,
                    p => p.ThicketSupportDisabled,
                    p => p.TrimAuditLog
                    );

                var site = context.Site;

                Assert.IsNotNull(site);
                Assert.AreEqual(SearchBoxInNavBar.Inherit, site.SearchBoxInNavBar);
                Assert.IsNull(site.SearchBoxPlaceholderText);
                Assert.IsNull(site.SensitivityLabelId);
                Assert.AreEqual(default, site.SensitivityLabel);
                Assert.AreNotEqual("", site.ServerRelativeUrl);
                Assert.IsFalse(site.ShareByEmailEnabled);
                Assert.IsFalse(site.ShareByLinkEnabled);
                Assert.IsFalse(site.ShowPeoplePickerSuggestionsForGuestUsers);
                Assert.IsFalse(site.SocialBarOnSitePagesDisabled);
                Assert.IsNull(site.StatusBarLink);
                Assert.IsNull(site.StatusBarText);
                Assert.IsTrue(site.ThicketSupportDisabled);
                Assert.IsTrue(site.TrimAuditLog);
            }
        }

        [TestMethod]
        public async Task GetSiteChangesAsyncTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var changes = await context.Site.GetChangesAsync(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }

        [TestMethod]
        public void GetSiteChangesTest()
        {
            //TestCommon.Instance.Mocking = false;

            using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
            {
                var changes = context.Site.GetChanges(new ChangeQueryOptions(true, true)
                {
                    FetchLimit = 5,
                });

                Assert.IsNotNull(changes);
                Assert.IsTrue(changes.Count > 0);
            }
        }
    }
}
