using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Model.SharePoint;
using PnP.Core.Test.Utilities;
using PnP.Core.Utilities;
using System.Linq;
using System.Threading.Tasks;

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
                ISite site = await context.Site.GetAsync(
                    p => p.AllowCreateDeclarativeWorkflow,
                    p => p.AllowDesigner,
                    p => p.AllowExternalEmbeddingWrapper,
                    p => p.AllowMasterPageEditing,
                    p => p.AllowRevertFromTemplate,
                    p => p.AllowSaveDeclarativeWorkflowAsTemplate,
                    p => p.AllowSavePublishDeclarativeWorkflow,
                    p => p.AllowSelfServiceUpgrade,
                    p => p.AllowSelfServiceUpgradeEvaluation,
                    p => p.AuditLogTrimmingRetention,
                    p => p.CanSyncHubSitePermissions,
                    p => p.CanUpgrade,
                    p => p.ChannelGroupId,
                    p => p.Classification,
                    p => p.CommentsOnSitePagesDisabled,
                    p => p.CompatibilityLevel,
                    p => p.ComplianceAttribute,
                    p => p.DisableAppViews,
                    p => p.DisableCompanyWideSharingLinks,
                    p => p.DisableFlows,
                    p => p.ExternalSharingTipsEnabled,
                    p => p.ExternalUserExpirationInDays,
                    p => p.GeoLocation,
                    p => p.GroupId
                    );

                Assert.IsNotNull(site);
                Assert.IsTrue(site.AllowCreateDeclarativeWorkflow);
                Assert.IsTrue(site.AllowDesigner);
                Assert.AreNotEqual(0, site.AllowExternalEmbeddingWrapper);
                Assert.IsFalse(site.AllowMasterPageEditing);
                Assert.IsFalse(site.AllowRevertFromTemplate);
                Assert.IsTrue(site.AllowSaveDeclarativeWorkflowAsTemplate);
                Assert.IsTrue(site.AllowSavePublishDeclarativeWorkflow);
                Assert.IsTrue(site.AllowSelfServiceUpgrade);
                Assert.IsTrue(site.AllowSelfServiceUpgradeEvaluation);
                Assert.AreNotEqual(0, site.AuditLogTrimmingRetention);
                Assert.IsFalse(site.CanSyncHubSitePermissions);
                Assert.IsTrue(site.CanUpgrade);
                Assert.AreEqual(default, site.ChannelGroupId);
                Assert.AreEqual("", site.Classification);
                Assert.IsFalse(site.CommentsOnSitePagesDisabled);
                Assert.AreNotEqual(0, site.CompatibilityLevel);
                Assert.AreEqual("", site.ComplianceAttribute);
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
                ISite site = await context.Site.GetAsync(
                    p => p.HubSiteId,
                    p => p.IsHubSite,
                    p => p.LockIssue,
                    p => p.MaxItemsPerThrottledOperation,
                    p => p.NeedsB2BUpgrade,
                    p => p.PrimaryUri,
                    p => p.ReadOnly,
                    p => p.RelatedGroupId,
                    p => p.RequiredDesignerVersion
                    );

                Assert.IsNotNull(site);
                Assert.AreEqual(default, site.HubSiteId);
                Assert.IsFalse(site.IsHubSite);
                Assert.IsNull(site.LockIssue);
                Assert.AreNotEqual(0, site.MaxItemsPerThrottledOperation);
                Assert.IsFalse(site.NeedsB2BUpgrade);
                Assert.IsNotNull(site.PrimaryUri);
                Assert.AreNotEqual("", site.PrimaryUri?.ToString());
                Assert.IsFalse(site.ReadOnly);
                Assert.AreNotEqual(default, site.RelatedGroupId);
                Assert.AreNotEqual("", site.RequiredDesignerVersion);
            }
        }

        [TestMethod]
        public async Task GetSiteSimpleProperties_S_Z_Test()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                ISite site = await context.Site.GetAsync(
                    p => p.SearchBoxInNavBar,
                    p => p.SearchBoxPlaceholderText,
                    p => p.SearchCenterUrl,
                    p => p.SensitivityLabelId,
                    p => p.SensitivityLabel,
                    p => p.ServerRelativeUrl,
                    p => p.ShareByEmailEnabled,
                    p => p.ShareByLinkEnabled,
                    p => p.ShowPeoplePickerSuggestionsForGuestUsers,
                    p => p.ShowUrlStructure,
                    p => p.SocialBarOnSitePagesDisabled,
                    p => p.StatusBarLink,
                    p => p.StatusBarText,
                    p => p.ThicketSupportDisabled,
                    p => p.TrimAuditLog,
                    p => p.UIVersionConfigurationEnabled,
                    p => p.UpgradeReminderDate,
                    p => p.UpgradeScheduled,
                    p => p.UpgradeScheduledDate,
                    p => p.Upgrading
                    );

                Assert.IsNotNull(site);
                Assert.AreEqual(SearchBoxInNavBar.Inherit, site.SearchBoxInNavBar);
                Assert.IsNull(site.SearchBoxPlaceholderText);
                // TODO This one is not loaded => Review why Graph is not called
                //Assert.IsNull(site.SearchCenterUrl);
                Assert.IsNull(site.SensitivityLabelId);
                Assert.AreEqual(default, site.SensitivityLabel);
                Assert.AreNotEqual("", site.ServerRelativeUrl);
                Assert.IsFalse(site.ShareByEmailEnabled);
                Assert.IsFalse(site.ShareByLinkEnabled);
                Assert.IsFalse(site.ShowPeoplePickerSuggestionsForGuestUsers);
                Assert.IsFalse(site.ShowUrlStructure);
                Assert.IsFalse(site.SocialBarOnSitePagesDisabled);
                Assert.IsNull(site.StatusBarLink);
                Assert.IsNull(site.StatusBarText);
                Assert.IsTrue(site.ThicketSupportDisabled);
                Assert.IsTrue(site.TrimAuditLog);
                Assert.IsFalse(site.UIVersionConfigurationEnabled);
                Assert.AreEqual(1899, site.UpgradeReminderDate.Year);
                Assert.IsFalse(site.UpgradeScheduled);
                Assert.AreEqual(1753, site.UpgradeScheduledDate.Year);
                Assert.IsFalse(site.Upgrading);
                // TODO This one is not loaded => Review why Graph is not called
                //Assert.IsNotNull(site.Url);
                //Assert.AreNotEqual("", site.Url.ToString());
            }
        }

    }
}
