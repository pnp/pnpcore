using Microsoft.VisualStudio.TestTools.UnitTesting;
using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Admin.Test.Utilities;
using PnP.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Test.SharePoint
{
    [TestClass]
    public class TenantTests
    {

        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Configure mocking default for all tests in this class, unless override by a specific test
            //TestCommon.Instance.Mocking = false;

            // Configure the test cases to use application permissions instead of delegated permissions
            //TestCommon.Instance.UseApplicationPermissions = true;
        }

        [TestMethod]
        public void GetTenantPortalUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline.sharepoint.com"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline.sharepoint.us"), SharePointAdmin.GetTenantPortalUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
        }

        [TestMethod]
        public async Task GetTenantPortalUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantPortalUri();
                Assert.IsFalse(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsFalse(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantPortalUri();
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void GetTenantMySiteHostUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-my.sharepoint.com"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-my.sharepoint.us"), SharePointAdmin.GetTenantMySiteHostUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
        }

        [TestMethod]
        public async Task GetTenantMySiteHostUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantMySiteHostUri();
                Assert.IsFalse(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsTrue(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantMySiteHostUri();
                    Assert.IsFalse(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                    Assert.IsTrue(url.DnsSafeHost.Contains("-my", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public void GetTenantAdminCenterUrlForRegularTenants()
        {
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline-my.sharepoint.com")));
            Assert.AreEqual(new Uri("https://BertOnline-admin.sharepoint.com"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://BertOnline-admin.sharepoint.com")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-my.sharepoint.us")));
            Assert.AreEqual(new Uri("https://bertonline-admin.sharepoint.us"), SharePointAdmin.GetTenantAdminCenterUriForStandardTenants(new Uri("https://bertonline-admin.sharepoint.us")));
        }

        [TestMethod]
        public async Task GetTenantAdminCenterUrl()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                Assert.IsTrue(url.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));

                using (var tenantContext = await TestCommon.Instance.CloneAsync(context, url, 2))
                {
                    var url2 = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                    Assert.IsTrue(url2.DnsSafeHost.Contains("-admin", StringComparison.InvariantCultureIgnoreCase));
                }
            }
        }

        [TestMethod]
        public async Task GetTenantAdminCenterContext()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                using (var tenantContext = context.GetSharePointAdmin().GetTenantAdminCenterContext())
                {
                    var url = context.GetSharePointAdmin().GetTenantAdminCenterUri();
                    Assert.IsTrue(tenantContext != null);
                    Assert.IsTrue(tenantContext.Web.Requested);
                    Assert.IsTrue(tenantContext.Web.IsPropertyAvailable(p => p.Id));
                    Assert.IsTrue(tenantContext.Uri == url);
                }
            }
        }

        [TestMethod]
        public async Task GetTenantAdminCenterContextVanity()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var vanityUrls = new VanityUrlOptions
                {
                    AdminCenterUri = context.Uri
                };

                using (var tenantContext = context.GetSharePointAdmin().GetTenantAdminCenterContext(vanityUrls))
                {
                    var url = context.GetSharePointAdmin().GetTenantAdminCenterUri(vanityUrls);
                    Assert.IsTrue(tenantContext != null);
                    Assert.IsTrue(tenantContext.Web.Requested);
                    Assert.IsTrue(tenantContext.Web.IsPropertyAvailable(p => p.Id));
                    Assert.IsTrue(tenantContext.Uri == url);
                }
            }
        }

        [TestMethod]
        public async Task GetTenantAdmins()
        {
            //TestCommon.Instance.Mocking = false;
            //TestCommon.Instance.UseApplicationPermissions = true;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TenantAdminCenterSite))
            {
                var admins = context.GetSharePointAdmin().GetTenantAdmins();
                Assert.IsTrue(admins != null);
                Assert.IsTrue(admins.Any());
            }
        }

        [TestMethod]
        public async Task IsCurrentUserSharePointAdmin()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {                
                Assert.IsTrue(context.GetSharePointAdmin().IsCurrentUserTenantAdmin());
            }
        }

        [TestMethod]
        public async Task CurrentUserIsNotSharePointAdmin()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                context.BatchClient.MockingFileRewriteHandler = (string input) =>
                {                    
                    return "{ \"@odata.context\": \"https://graph.microsoft.com/v1.0/$metadata#directoryObjects\", \"@odata.count\": \"0\", \"value\": [] }";
                };

                Assert.IsFalse(context.GetSharePointAdmin().IsCurrentUserTenantAdmin());
            }
        }
        [TestMethod]
        public async Task GetTenantProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantProperties = context.GetSharePointAdmin().GetTenantProperties();
                Assert.IsNotNull(tenantProperties);

                Assert.IsTrue(tenantProperties.AllowCommentsTextOnEmailEnabled.Test());
                Assert.IsTrue(tenantProperties.AllowDownloadingNonWebViewableFiles.Test());
                Assert.IsTrue(tenantProperties.AllowEditing.Test());
                Assert.IsTrue(tenantProperties.AllowGuestUserShareToUsersNotInSiteCollection.Test());
                Assert.IsTrue(tenantProperties.AllowLimitedAccessOnUnmanagedDevices.Test());
                Assert.IsTrue(tenantProperties.AllowOverrideForBlockUserInfoVisibility.Test());
                Assert.IsTrue(tenantProperties.AnyoneLinkTrackUsers.Test());
                Assert.IsTrue(tenantProperties.ApplyAppEnforcedRestrictionsToAdHocRecipients.Test());
                Assert.IsTrue(tenantProperties.BccExternalSharingInvitations.Test());
                Assert.IsTrue(tenantProperties.BlockAccessOnUnmanagedDevices.Test());
                Assert.IsTrue(tenantProperties.BlockDownloadOfAllFilesForGuests.Test());
                Assert.IsTrue(tenantProperties.BlockDownloadOfAllFilesOnUnmanagedDevices.Test());
                Assert.IsTrue(tenantProperties.BlockDownloadOfViewableFilesForGuests.Test());
                Assert.IsTrue(tenantProperties.BlockDownloadOfViewableFilesOnUnmanagedDevices.Test());
                Assert.IsTrue(tenantProperties.BlockMacSync.Test());
                Assert.IsTrue(tenantProperties.BlockSendLabelMismatchEmail.Test());
                Assert.IsTrue(tenantProperties.CommentsOnFilesDisabled.Test());
                Assert.IsTrue(tenantProperties.CommentsOnListItemsDisabled.Test());
                Assert.IsTrue(tenantProperties.CommentsOnSitePagesDisabled.Test());
                Assert.IsTrue(tenantProperties.DisableAddToOneDrive.Test());
                Assert.IsTrue(tenantProperties.DisableBackToClassic.Test());
                Assert.IsTrue(tenantProperties.DisableCustomAppAuthentication.Test());
                Assert.IsTrue(tenantProperties.DisableListSync.Test());
                Assert.IsTrue(tenantProperties.DisableOutlookPSTVersionTrimming.Test());
                Assert.IsTrue(tenantProperties.DisablePersonalListCreation.Test());
                Assert.IsTrue(tenantProperties.DisableReportProblemDialog.Test());
                Assert.IsTrue(tenantProperties.DisableSpacesActivation.Test());
                Assert.IsTrue(tenantProperties.DisallowInfectedFileDownload.Test());
                Assert.IsTrue(tenantProperties.DisplayNamesOfFileViewers.Test());
                Assert.IsTrue(tenantProperties.DisplayNamesOfFileViewersInSpo.Test());
                Assert.IsTrue(tenantProperties.DisplayStartASiteOption.Test());
                Assert.IsTrue(tenantProperties.EmailAttestationEnabled.Test());
                Assert.IsTrue(tenantProperties.EmailAttestationRequired.Test());
                Assert.IsTrue(tenantProperties.EnableAIPIntegration.Test());
                Assert.IsTrue(tenantProperties.EnableAutoNewsDigest.Test());
                Assert.IsTrue(tenantProperties.EnableAzureADB2BIntegration.Test());
                Assert.IsTrue(tenantProperties.EnabledFlightAllowAADB2BSkipCheckingOTP.Test());
                Assert.IsTrue(tenantProperties.EnableGuestSignInAcceleration.Test());
                Assert.IsTrue(tenantProperties.EnableMinimumVersionRequirement.Test());
                Assert.IsTrue(tenantProperties.EnableMipSiteLabel.Test());
                Assert.IsTrue(tenantProperties.EnablePromotedFileHandlers.Test());
                Assert.IsTrue(tenantProperties.ExternalServicesEnabled.Test());
                Assert.IsTrue(tenantProperties.ExternalUserExpirationRequired.Test());
                Assert.IsTrue(tenantProperties.FilePickerExternalImageSearchEnabled.Test());
                Assert.IsTrue(tenantProperties.HasAdminCompletedCUConfiguration.Test());
                Assert.IsTrue(tenantProperties.HasIntelligentContentServicesCapability.Test());
                Assert.IsTrue(tenantProperties.HasTopicExperiencesCapability.Test());
                Assert.IsTrue(tenantProperties.HideSyncButtonOnDocLib.Test());
                Assert.IsTrue(tenantProperties.HideSyncButtonOnODB.Test());
                Assert.IsTrue(tenantProperties.IncludeAtAGlanceInShareEmails.Test());
                Assert.IsTrue(tenantProperties.InformationBarriersSuspension.Test());
                Assert.IsTrue(tenantProperties.IPAddressEnforcement.Test());
                Assert.IsTrue(tenantProperties.IsAppBarTemporarilyDisabled.Test());
                Assert.IsTrue(tenantProperties.IsCollabMeetingNotesFluidEnabled.Test());
                Assert.IsTrue(tenantProperties.IsFluidEnabled.Test());
                Assert.IsTrue(tenantProperties.IsHubSitesMultiGeoFlightEnabled.Test());
                Assert.IsTrue(tenantProperties.IsMnAFlightEnabled.Test());
                Assert.IsTrue(tenantProperties.IsMultiGeo.Test());
                Assert.IsTrue(tenantProperties.IsUnmanagedSyncClientForTenantRestricted.Test());
                Assert.IsTrue(tenantProperties.IsUnmanagedSyncClientRestrictionFlightEnabled.Test());
                Assert.IsTrue(tenantProperties.IsWBFluidEnabled.Test());
                Assert.IsTrue(tenantProperties.LegacyAuthProtocolsEnabled.Test());
                Assert.IsTrue(tenantProperties.MachineLearningCaptureEnabled.Test());
                Assert.IsTrue(tenantProperties.MobileFriendlyUrlEnabledInTenant.Test());
                Assert.IsTrue(tenantProperties.NotificationsInOneDriveForBusinessEnabled.Test());
                Assert.IsTrue(tenantProperties.NotificationsInSharePointEnabled.Test());
                Assert.IsTrue(tenantProperties.NotifyOwnersWhenInvitationsAccepted.Test());
                Assert.IsTrue(tenantProperties.NotifyOwnersWhenItemsReshared.Test());
                Assert.IsTrue(tenantProperties.OfficeClientADALDisabled.Test());
                Assert.IsTrue(tenantProperties.OneDriveForGuestsEnabled.Test());
                Assert.IsTrue(tenantProperties.OwnerAnonymousNotification.Test());
                Assert.IsTrue(tenantProperties.PreventExternalUsersFromResharing.Test());
                Assert.IsTrue(tenantProperties.ProvisionSharedWithEveryoneFolder.Test());
                Assert.IsTrue(tenantProperties.PublicCdnEnabled.Test());
                Assert.IsTrue(tenantProperties.RequireAcceptingAccountMatchInvitedAccount.Test());
                Assert.IsTrue(tenantProperties.RestrictedOneDriveLicense.Test());
                Assert.IsTrue(tenantProperties.SearchResolveExactEmailOrUPN.Test());
                Assert.IsTrue(tenantProperties.ShowAllUsersClaim.Test());
                Assert.IsTrue(tenantProperties.ShowEveryoneClaim.Test());
                Assert.IsTrue(tenantProperties.ShowEveryoneExceptExternalUsersClaim.Test());
                Assert.IsTrue(tenantProperties.ShowPeoplePickerSuggestionsForGuestUsers.Test());
                Assert.IsTrue(tenantProperties.SocialBarOnSitePagesDisabled.Test());
                Assert.IsTrue(tenantProperties.StopNew2010Workflows.Test());
                Assert.IsTrue(tenantProperties.StopNew2013Workflows.Test());
                Assert.IsTrue(tenantProperties.SyncAadB2BManagementPolicy.Test());
                Assert.IsTrue(tenantProperties.SyncPrivacyProfileProperties.Test());
                Assert.IsTrue(tenantProperties.UseFindPeopleInPeoplePicker.Test());
                Assert.IsTrue(tenantProperties.UserVoiceForFeedbackEnabled.Test());
                Assert.IsTrue(tenantProperties.ViewersCanCommentOnMediaDisabled.Test());
                Assert.IsTrue(tenantProperties.ViewInFileExplorerEnabled.Test());
                Assert.IsTrue(tenantProperties.Workflow2010Disabled.Test());

                Assert.IsTrue(tenantProperties.CompatibilityRange.Test());
                Assert.IsTrue(tenantProperties.ConditionalAccessPolicyErrorHelpLink.Test());
                Assert.IsTrue(tenantProperties.CustomizedExternalSharingServiceUrl.Test());
                Assert.IsTrue(tenantProperties.DefaultODBMode.Test());
                Assert.IsTrue(tenantProperties.GuestSharingGroupAllowListInTenant.Test());
                Assert.IsTrue(tenantProperties.IPAddressAllowList.Test());
                Assert.IsTrue(tenantProperties.LabelMismatchEmailHelpLink.Test());
                Assert.IsTrue(tenantProperties.NoAccessRedirectUrl.Test());
                Assert.IsTrue(tenantProperties.OrgNewsSiteUrl.Test());
                Assert.IsTrue(tenantProperties.PublicCdnAllowedFileTypes.Test());
                Assert.IsTrue(tenantProperties.RootSiteUrl.Test());
                Assert.IsTrue(tenantProperties.SharingAllowedDomainList.Test());
                Assert.IsTrue(tenantProperties.SharingBlockedDomainList.Test());
                Assert.IsTrue(tenantProperties.StartASiteFormUrl.Test());
                Assert.IsTrue(tenantProperties.WhoCanShareAllowListInTenant.Test());

                Assert.IsTrue((int)tenantProperties.BlockDownloadLinksFileType >= 0);
                Assert.IsTrue((int)tenantProperties.BlockUserInfoVisibilityInOneDrive >= 0);
                Assert.IsTrue((int)tenantProperties.BlockUserInfoVisibilityInSharePoint >= 0);
                Assert.IsTrue((int)tenantProperties.ConditionalAccessPolicy >= 0);
                Assert.IsTrue((int)tenantProperties.DefaultLinkPermission >= 0);
                Assert.IsTrue((int)tenantProperties.DefaultSharingLinkType >= 0);
                Assert.IsTrue(tenantProperties.EmailAttestationReAuthDays >= 0);
                Assert.IsTrue(tenantProperties.ExternalUserExpireInDays >= 0);
                Assert.IsTrue((int)tenantProperties.FileAnonymousLinkType >= 0);
                Assert.IsTrue((int)tenantProperties.FolderAnonymousLinkType >= 0);
                Assert.IsTrue(tenantProperties.IPAddressWACTokenLifetime >= 0);
                Assert.IsTrue((int)tenantProperties.LimitedAccessFileType >= 0);
                Assert.IsTrue((int)tenantProperties.MarkNewFilesSensitiveByDefault >= 0);
                Assert.IsTrue((int)tenantProperties.MediaTranscription >= 0);
                Assert.IsTrue((int)tenantProperties.ODBAccessRequests >= 0);
                Assert.IsTrue((int)tenantProperties.ODBMembersCanShare >= 0);
                Assert.IsTrue((int)tenantProperties.ODBSharingCapability >= 0);
                Assert.IsTrue(tenantProperties.OneDriveStorageQuota >= 0);
                Assert.IsTrue(tenantProperties.OrphanedPersonalSitesRetentionPeriod >= 0);
                Assert.IsTrue(tenantProperties.RequireAnonymousLinksExpireInDays >= -1);
                Assert.IsTrue((int)tenantProperties.SharingCapability >= 0);
                Assert.IsTrue((int)tenantProperties.SharingDomainRestrictionMode >= 0);
                Assert.IsTrue((int)tenantProperties.SpecialCharactersStateInFileFolderNames >= 0);
                Assert.IsTrue(tenantProperties.StorageQuota >= 0);
                Assert.IsTrue(tenantProperties.StorageQuotaAllocated >= 0);
                Assert.IsTrue((int)tenantProperties.Workflows2013State >= 0);

                Assert.IsTrue(tenantProperties.AIBuilderDefaultPowerAppsEnvironment.Test());
                Assert.IsTrue(tenantProperties.AIBuilderEnabled.Test());
                Assert.IsTrue((int)tenantProperties.AIBuilderEnabledInContentCenter >= -1);
                Assert.IsTrue(tenantProperties.AIBuilderSiteListFileName.Test());
                Assert.IsTrue((int)tenantProperties.AllowAnonymousMeetingParticipantsToAccessWhiteboards >= 0);
                Assert.IsTrue(tenantProperties.AllowEveryoneExceptExternalUsersClaimInPrivateSite.Test());
                Assert.IsTrue(tenantProperties.AllowSelectSecurityGroupsInSPSitesList == null || tenantProperties.AllowSelectSecurityGroupsInSPSitesList != null);
                Assert.IsTrue((int)tenantProperties.AuthContextResilienceMode >= 0);
                Assert.IsTrue(tenantProperties.BlockDownloadFileTypePolicy.Test());
                Assert.IsTrue(tenantProperties.CoreDefaultLinkToExistingAccess.Test());
                Assert.IsTrue((int)tenantProperties.CoreDefaultShareLinkRole >= 0);
                Assert.IsTrue((int)tenantProperties.CoreDefaultShareLinkScope >= -1);
                Assert.IsTrue(tenantProperties.CoreRequestFilesLinkEnabled.Test());
                Assert.IsTrue(tenantProperties.CoreRequestFilesLinkExpirationInDays >= -1);
                Assert.IsTrue((int)tenantProperties.CoreSharingCapability >= 0);
                Assert.IsTrue(tenantProperties.DenySelectSecurityGroupsInSPSitesList == null || tenantProperties.DenySelectSecurityGroupsInSPSitesList != null);
                Assert.IsTrue(tenantProperties.DisableVivaConnectionsAnalytics.Test());
                Assert.IsTrue(tenantProperties.EnableRestrictedAccessControl.Test());
                Assert.IsTrue(tenantProperties.IBImplicitGroupBased.Test());
                Assert.IsTrue(tenantProperties.IsLoopEnabled.Test());
                Assert.IsTrue(tenantProperties.IsMultipleHomeSitesFlightEnabled.Test());
                Assert.IsTrue(tenantProperties.IsVivaHomeFlightEnabled.Test());
                Assert.IsTrue(tenantProperties.OCRAdminSiteListFileName.Test());
                Assert.IsTrue(tenantProperties.OCRComplianceSiteListFileName.Test());
                Assert.IsTrue((int)tenantProperties.OCRModeForAdminSites >= 0);
                Assert.IsTrue((int)tenantProperties.OCRModeForComplianceODBs >= 0);
                Assert.IsTrue((int)tenantProperties.OCRModeForComplianceSites >= 0);
                Assert.IsTrue(tenantProperties.OneDriveDefaultLinkToExistingAccess.Test());
                Assert.IsTrue((int)tenantProperties.OneDriveDefaultShareLinkRole >= 0);
                Assert.IsTrue((int)tenantProperties.OneDriveDefaultShareLinkScope >= -1);
                Assert.IsTrue(tenantProperties.OneDriveRequestFilesLinkEnabled.Test());
                Assert.IsTrue(tenantProperties.OneDriveRequestFilesLinkExpirationInDays >= -1);
                Assert.IsTrue(tenantProperties.ReduceTempTokenLifetimeEnabled.Test());
                Assert.IsTrue((int)tenantProperties.ReduceTempTokenLifetimeValue >= 0);
                Assert.IsTrue(tenantProperties.RestrictedSharePointLicense.Test());
                Assert.IsTrue(tenantProperties.ShowOpenInDesktopOptionForSyncedFiles.Test());
                Assert.IsTrue(tenantProperties.ShowPeoplePickerGroupSuggestionsForIB.Test());
                Assert.IsTrue((int)tenantProperties.StreamLaunchConfig >= 0);
                Assert.IsTrue(tenantProperties.StreamLaunchConfigLastUpdated >= DateTime.MinValue);
                Assert.IsTrue(tenantProperties.StreamLaunchConfigUpdateCount >= 0);
                Assert.IsTrue((int)tenantProperties.TlsTokenBindingPolicyValue >= 0);
                Assert.IsTrue(tenantProperties.UsePersistentCookiesForExplorerView.Test());

            }
        }

        [TestMethod]
        public async Task SetTenantProperties()
        {
            //TestCommon.Instance.Mocking = false;
            using (var context = await TestCommon.Instance.GetContextAsync(TestCommon.TestSite))
            {
                var tenantProperties = context.GetSharePointAdmin().GetTenantProperties();
                Assert.IsNotNull(tenantProperties);

                // update properties
                var valueToSet = !tenantProperties.BlockMacSync;
                tenantProperties.BlockMacSync = valueToSet;
                tenantProperties.Update();

                tenantProperties = context.GetSharePointAdmin().GetTenantProperties();
                Assert.AreEqual(tenantProperties.BlockMacSync, valueToSet);

                // Ensure mac sync blocking is turned off again
                tenantProperties.BlockMacSync = false;
                tenantProperties.Update();

            }
        }

    }
}
