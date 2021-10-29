using PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant;
using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using PnP.Core.Services.Core.CSOM.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    internal class TenantProperties : TransientObject, ITenantProperties, IDataModelWithContext
    {
        #region Properties
        [JsonPropertyName("_ObjectIdentity_")]
        public string ObjectIdentity { get; set; }

        public bool AllowCommentsTextOnEmailEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowDownloadingNonWebViewableFiles { get => GetValue<bool>(); set => SetValue(value); }
        
        public IList<Guid> AllowedDomainListForSyncClient { get => GetValue<IList<Guid>>(); set => SetValue(value); }
        
        public bool AllowEditing { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowGuestUserShareToUsersNotInSiteCollection { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowLimitedAccessOnUnmanagedDevices { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool AllowOverrideForBlockUserInfoVisibility { get => GetValue<bool>(); set => SetValue(value); }
        
        public IList<string> AllowSelectSGsInODBListInTenant { get => GetValue<IList<string>>(); set => SetValue(value); }
        
        public bool AnyoneLinkTrackUsers { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ApplyAppEnforcedRestrictionsToAdHocRecipients { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool BccExternalSharingInvitations { get => GetValue<bool>(); set => SetValue(value); }
        
        public string BccExternalSharingInvitationsList { get => GetValue<string>(); set => SetValue(value); }
        
        public bool BlockAccessOnUnmanagedDevices { get => GetValue<bool>(); set => SetValue(value); }
        
        public BlockDownloadLinksFileTypes BlockDownloadLinksFileType { get => GetValue<BlockDownloadLinksFileTypes>(); set => SetValue(value); }
        
        public bool BlockDownloadOfAllFilesForGuests { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool BlockDownloadOfAllFilesOnUnmanagedDevices { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool BlockDownloadOfViewableFilesForGuests { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool BlockDownloadOfViewableFilesOnUnmanagedDevices { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool BlockMacSync { get => GetValue<bool>(); set => SetValue(value); }
        public bool BlockSendLabelMismatchEmail { get => GetValue<bool>(); set => SetValue(value); }
                
        public TenantBrowseUserInfoPolicyValue BlockUserInfoVisibilityInOneDrive { get => GetValue<TenantBrowseUserInfoPolicyValue>(); set => SetValue(value); }
        
        public TenantBrowseUserInfoPolicyValue BlockUserInfoVisibilityInSharePoint { get => GetValue<TenantBrowseUserInfoPolicyValue>(); set => SetValue(value); }
        
        public ChannelMeetingRecordingPermissionType ChannelMeetingRecordingPermission { get => GetValue<ChannelMeetingRecordingPermissionType>(); set => SetValue(value); }
        
        public bool CommentsOnFilesDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool CommentsOnListItemsDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public string CompatibilityRange { get => GetValue<string>(); set => SetValue(value); }
        
        public SPOConditionalAccessPolicyType ConditionalAccessPolicy { get => GetValue<SPOConditionalAccessPolicyType>(); set => SetValue(value); }
        
        public string ConditionalAccessPolicyErrorHelpLink { get => GetValue<string>(); set => SetValue(value); }
        
        public IEnumerable<string> ContentTypeSyncSiteTemplatesList { get => GetValue<IEnumerable<string>>(); set => SetValue(value); }
        
        public string CustomizedExternalSharingServiceUrl { get => GetValue<string>(); set => SetValue(value); }
        
        public SharingPermissionType DefaultLinkPermission { get => GetValue<SharingPermissionType>(); set => SetValue(value); }
        
        public string DefaultODBMode { get => GetValue<string>(); set => SetValue(value); }
        
        public SharingLinkType DefaultSharingLinkType { get => GetValue<SharingLinkType>(); set => SetValue(value); }
        
        public bool DisableAddToOneDrive { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisableBackToClassic { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisableCustomAppAuthentication { get => GetValue<bool>(); set => SetValue(value); }

        public bool DisableListSync { get => GetValue<bool>(); set => SetValue(value); }

        public Guid[] DisabledModernListTemplateIds { get => GetValue<Guid[]>(); set => SetValue(value); }
        
        public Guid[] DisabledWebPartIds { get => GetValue<Guid[]>(); set => SetValue(value); }
        
        public bool DisableOutlookPSTVersionTrimming { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisablePersonalListCreation { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisableReportProblemDialog { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisableSpacesActivation { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisallowInfectedFileDownload { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisplayNamesOfFileViewers { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisplayNamesOfFileViewersInSpo { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool DisplayStartASiteOption { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EmailAttestationEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public int EmailAttestationReAuthDays { get => GetValue<int>(); set => SetValue(value); }
        
        public bool EmailAttestationRequired { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnableAIPIntegration { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnableAutoNewsDigest { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnableAzureADB2BIntegration { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnabledFlightAllowAADB2BSkipCheckingOTP { get => GetValue<bool>(); set => SetValue(value); }

        public bool EnableGuestSignInAcceleration { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnableMinimumVersionRequirement { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnableMipSiteLabel { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool EnablePromotedFileHandlers { get => GetValue<bool>(); set => SetValue(value); }
        
        public IList<string> ExcludedFileExtensionsForSyncClient { get => GetValue<IList<string>>(); set => SetValue(value); }
        
        public bool ExternalServicesEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ExternalUserExpirationRequired { get => GetValue<bool>(); set => SetValue(value); }
        
        public int ExternalUserExpireInDays { get => GetValue<int>(); set => SetValue(value); }
        
        public AnonymousLinkType FileAnonymousLinkType { get => GetValue<AnonymousLinkType>(); set => SetValue(value); }
        
        public bool FilePickerExternalImageSearchEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public AnonymousLinkType FolderAnonymousLinkType { get => GetValue<AnonymousLinkType>(); set => SetValue(value); }
        
        public string GuestSharingGroupAllowListInTenant { get => GetValue<string>(); set => SetValue(value); }
        
        public IList<string> GuestSharingGroupAllowListInTenantByPrincipalIdentity { get => GetValue<IList<string>>(); set => SetValue(value); }
        
        public bool HasAdminCompletedCUConfiguration { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool HasIntelligentContentServicesCapability { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool HasTopicExperiencesCapability { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool HideSyncButtonOnDocLib { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool HideSyncButtonOnODB { get => GetValue<bool>(); set => SetValue(value); }
        
        public ImageTaggingChoice ImageTaggingOption { get => GetValue<ImageTaggingChoice>(); set => SetValue(value); }
        
        public bool IncludeAtAGlanceInShareEmails { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool InformationBarriersSuspension { get => GetValue<bool>(); set => SetValue(value); }
        
        public string IPAddressAllowList { get => GetValue<string>(); set => SetValue(value); }
        
        public bool IPAddressEnforcement { get => GetValue<bool>(); set => SetValue(value); }
        
        public int IPAddressWACTokenLifetime { get => GetValue<int>(); set => SetValue(value); }
        
        public bool IsAppBarTemporarilyDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool IsCollabMeetingNotesFluidEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool IsFluidEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHubSitesMultiGeoFlightEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMnAFlightEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsMultiGeo { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsUnmanagedSyncClientForTenantRestricted { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsUnmanagedSyncClientRestrictionFlightEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsWBFluidEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public string LabelMismatchEmailHelpLink { get => GetValue<string>(); set => SetValue(value); }
        
        public bool LegacyAuthProtocolsEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public SPOLimitedAccessFileType LimitedAccessFileType { get => GetValue<SPOLimitedAccessFileType>(); set => SetValue(value); }
        
        public bool MachineLearningCaptureEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public SensitiveByDefaultState MarkNewFilesSensitiveByDefault { get => GetValue<SensitiveByDefaultState>(); set => SetValue(value); }
        
        public MediaTranscriptionPolicyType MediaTranscription { get => GetValue<MediaTranscriptionPolicyType>(); set => SetValue(value); }
        
        public bool MobileFriendlyUrlEnabledInTenant { get => GetValue<bool>(); set => SetValue(value); }
        
        public string NoAccessRedirectUrl { get => GetValue<string>(); set => SetValue(value); }
        
        public bool NotificationsInOneDriveForBusinessEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool NotificationsInSharePointEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool NotifyOwnersWhenInvitationsAccepted { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool NotifyOwnersWhenItemsReshared { get => GetValue<bool>(); set => SetValue(value); }
        
        public SharingState ODBAccessRequests { get => GetValue<SharingState>(); set => SetValue(value); }
        
        public SharingState ODBMembersCanShare { get => GetValue<SharingState>(); set => SetValue(value); }
        
        public SharingCapabilities ODBSharingCapability { get => GetValue<SharingCapabilities>(); set => SetValue(value); }
        
        public bool OfficeClientADALDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool OneDriveForGuestsEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public long OneDriveStorageQuota { get => GetValue<long>(); set => SetValue(value); }
                
        public string OrgNewsSiteUrl { get => GetValue<string>(); set => SetValue(value); }
        
        public int OrphanedPersonalSitesRetentionPeriod { get => GetValue<int>(); set => SetValue(value); }
        
        public bool OwnerAnonymousNotification { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool PreventExternalUsersFromResharing { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ProvisionSharedWithEveryoneFolder { get => GetValue<bool>(); set => SetValue(value); }
        
        public string PublicCdnAllowedFileTypes { get => GetValue<string>(); set => SetValue(value); }
        
        public bool PublicCdnEnabled { get => GetValue<bool>(); set => SetValue(value); }
             
        public IList<string> PublicCdnOrigins { get => GetValue<IList<string>>(); set => SetValue(value); }

        public bool RequireAcceptingAccountMatchInvitedAccount { get => GetValue<bool>(); set => SetValue(value); }

        public int RequireAnonymousLinksExpireInDays { get => GetValue<int>(); set => SetValue(value); }

        public bool RestrictedOneDriveLicense { get => GetValue<bool>(); set => SetValue(value); }

        public string RootSiteUrl { get => GetValue<string>(); set => SetValue(value); }

        public bool SearchResolveExactEmailOrUPN { get => GetValue<bool>(); set => SetValue(value); }
        
        public string SharingAllowedDomainList { get => GetValue<string>(); set => SetValue(value); }
        
        public string SharingBlockedDomainList { get => GetValue<string>(); set => SetValue(value); }
        
        public SharingCapabilities SharingCapability { get => GetValue<SharingCapabilities>(); set => SetValue(value); }
        
        public SharingDomainRestrictionModes SharingDomainRestrictionMode { get => GetValue<SharingDomainRestrictionModes>(); set => SetValue(value); }
        
        public bool ShowAllUsersClaim { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ShowEveryoneClaim { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ShowEveryoneExceptExternalUsersClaim { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ShowPeoplePickerSuggestionsForGuestUsers { get => GetValue<bool>(); set => SetValue(value); }
        
        public string SignInAccelerationDomain { get => GetValue<string>(); set => SetValue(value); }
        
        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public SpecialCharactersState SpecialCharactersStateInFileFolderNames { get => GetValue<SpecialCharactersState>(); set => SetValue(value); }
        
        public string StartASiteFormUrl { get => GetValue<string>(); set => SetValue(value); }
        
        public bool StopNew2010Workflows { get => GetValue<bool>(); set => SetValue(value); }        

        public bool StopNew2013Workflows { get => GetValue<bool>(); set => SetValue(value); }

        public long StorageQuota { get => GetValue<long>(); set => SetValue(value); }

        public long StorageQuotaAllocated { get => GetValue<long>(); set => SetValue(value); }
        
        public bool SyncAadB2BManagementPolicy { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool SyncPrivacyProfileProperties { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool UseFindPeopleInPeoplePicker { get => GetValue<bool>(); set => SetValue(value); }
               
        public bool UserVoiceForFeedbackEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ViewersCanCommentOnMediaDisabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public bool ViewInFileExplorerEnabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public string WhoCanShareAllowListInTenant { get => GetValue<string>(); set => SetValue(value); }
        
        public IList<string> WhoCanShareAllowListInTenantByPrincipalIdentity { get => GetValue<IList<string>>(); set => SetValue(value); }
        
        public bool Workflow2010Disabled { get => GetValue<bool>(); set => SetValue(value); }
        
        public Workflows2013State Workflows2013State { get => GetValue<Workflows2013State>(); set => SetValue(value); }

        public PnPContext PnPContext { get; set; }

        #endregion

        #region Methods

        public async Task UpdateAsync()
        {
            using (var tenantAdminContext = await PnPContext.GetSharePointAdmin().GetTenantAdminCenterContextAsync().ConfigureAwait(false))
            {
                List<IRequest<object>> csomRequests = new List<IRequest<object>>
                {
                    new SetTenantPropertiesRequest(this)
                };

                await (tenantAdminContext.Web as Web).RawRequestAsync(new ApiCall(csomRequests), HttpMethod.Post).ConfigureAwait(false);
            }
        }

        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }
        #endregion

    }
}
