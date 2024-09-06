using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Tenant properties
    /// </summary>
    public interface ITenantProperties
    {
        #region Properties

        /// <summary>
        /// Gets or sets the default PowerApps environment in which the Microsoft Syntex form processing feature will create models
        /// </summary>
        string AIBuilderDefaultPowerAppsEnvironment { get; set; }

        /// <summary>
        /// Gets or sets the value if the AIBuilder settings should be shown in the tenant
        /// </summary>
        bool AIBuilderEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value if the AIBuilder settings should be shown in content centers. True means that it is shown
        /// </summary>
        NullableBool AIBuilderEnabledInContentCenter { get; set; }

        /// <summary>
        /// It is a name of the file which contains the list of AIBuilder enabled sites
        /// </summary>
        string AIBuilderSiteListFileName { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate whether to allow anonymous meeting participants to access whiteboards
        /// </summary>
        SharingState AllowAnonymousMeetingParticipantsToAccessWhiteboards { get; set; }

        /// <summary>
        /// Gets or sets a value AllowCommentsTextOnEmail boolean
        /// </summary>
        bool AllowCommentsTextOnEmailEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value to specify the advanced setting of the conditional access policy
        /// </summary>
        bool AllowDownloadingNonWebViewableFiles { get; set; }

        /// <summary>
        /// Get/Set sync client trusted domain guids
        /// </summary>
        IList<Guid> AllowedDomainListForSyncClient { get; set; }

        /// <summary>
        /// Gets or sets a value to specify the advanced setting of the conditional access policy.
        /// This controls if WAC files should be opened in Edit mode
        /// </summary>
        bool AllowEditing { get; set; }

        /// <summary>
        /// Gets or sets the value if EveryoneExceptExternalUsers claim is allowed or not in people picker in a private group site. False value means it is blocked.
        /// </summary>
        bool AllowEveryoneExceptExternalUsersClaimInPrivateSite { get; set; }

        /// <summary>
        /// Gets or sets a value to handle guest sharing to users not in guest users' site collection
        /// </summary>
        bool AllowGuestUserShareToUsersNotInSiteCollection { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following access setting is enabled: set allow access on unmanaged devices
        /// </summary>
        bool AllowLimitedAccessOnUnmanagedDevices { get; set; }

        /// <summary>
        /// Gets or sets BlockUserInfoVisibility value
        /// </summary>
        bool AllowOverrideForBlockUserInfoVisibility { get; set; }

        /// <summary>
        /// Gets or sets a value to handle guest sharing group's allow list
        /// </summary>
        IList<string> AllowSelectSecurityGroupsInSPSitesList { get; set; }

        /// <summary>
        /// Gets or sets a value to handle the tenant allowing select security groups access to ODB setting
        /// </summary>
        IList<string> AllowSelectSGsInODBListInTenant { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether anyone links should track link users
        /// </summary>
        bool AnyoneLinkTrackUsers { get; set; }

        /// <summary>
        /// Gets or sets whether app-enforced restrictions apply to TOAA users
        /// </summary>
        bool ApplyAppEnforcedRestrictionsToAdHocRecipients { get; set; }

        /// <summary>
        /// Gets or sets a value of AuthContextResilienceMode
        /// </summary>
        SPResilienceModeType AuthContextResilienceMode { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if BCC functionality is enabled for external invitations
        /// </summary>
        bool BccExternalSharingInvitations { get; set; }

        /// <summary>
        /// Gets or sets list of recipients to be BCC'ed on all external sharing invitations
        /// </summary>
        string BccExternalSharingInvitationsList { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following access setting is enabled: set allow access on unmanaged devices
        /// </summary>
        bool BlockAccessOnUnmanagedDevices { get; set; }

        /// <summary>
        /// Gets or sets the tenant's block download links' file type.
        /// There is an equivalent site level policy
        /// </summary>
        BlockDownloadLinksFileTypes BlockDownloadLinksFileType { get; set; }

        /// <summary>
        /// Indicates whether Block Download by File Type Policy is enabled or not
        /// </summary>
        bool BlockDownloadFileTypePolicy { get; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following policy is enabled: set block download of all files for guests
        /// </summary>
        bool BlockDownloadOfAllFilesForGuests { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following policy is enabled: set block download of all files on unmanaged devices
        /// </summary>
        bool BlockDownloadOfAllFilesOnUnmanagedDevices { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following policy is enabled: set block download of browser viewable files for guests
        /// </summary>
        bool BlockDownloadOfViewableFilesForGuests { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the following policy is enabled: set block download of browser viewable files on unmanaged devices
        /// </summary>
        bool BlockDownloadOfViewableFilesOnUnmanagedDevices { get; set; }

        /// <summary>
        /// Get/Set whether Mac clients should be blocked from sync
        /// </summary>
        bool BlockMacSync { get; set; }

        /// <summary>
        /// Gets or sets the BlockSendLabelMismatchEmail property
        /// </summary>
        bool BlockSendLabelMismatchEmail { get; set; }

        /// <summary>
        /// Gets or sets BlockUserInfoVisibilityInOneDrive value
        /// </summary>
        TenantBrowseUserInfoPolicyValue BlockUserInfoVisibilityInOneDrive { get; set; }

        /// <summary>
        /// Gets or sets BlockUserInfoVisibilityInSharePoint value
        /// </summary>
        TenantBrowseUserInfoPolicyValue BlockUserInfoVisibilityInSharePoint { get; set; }

        /// <summary>
        /// Whether comments on files are disabled or not
        /// </summary>
        bool CommentsOnFilesDisabled { get; set; }

        /// <summary>
        /// Whether comments on list items are disabled or not
        /// </summary>
        bool CommentsOnListItemsDisabled { get; set; }

        /// <summary>
        /// Whether comments on site pages are disabled or not
        /// </summary>
        bool CommentsOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Determines which compatibility range is available for new site collections
        /// </summary>
        string CompatibilityRange { get; }

        /// <summary>
        /// Gets or sets a value to specify the conditional access policy for the tenant
        /// </summary>
        SPOConditionalAccessPolicyType ConditionalAccessPolicy { get; set; }

        /// <summary>
        /// Gets or sets the link to organization help page in case of access denied due to
        /// conditional access policy
        /// </summary>
        string ConditionalAccessPolicyErrorHelpLink { get; set; }

        /// <summary>
        /// A list of site templates that the tenant has opted-in to sync Content types
        /// </summary>
        IEnumerable<string> ContentTypeSyncSiteTemplatesList { get; set; }

        /// <summary>
        /// Gets or sets default share link to existing access on core partition
        /// </summary>
        bool CoreDefaultLinkToExistingAccess { get; set; }

        /// <summary>
        /// Gets or sets default share link role on core partition
        /// </summary>
        Role CoreDefaultShareLinkRole { get; set; }

        /// <summary>
        /// Gets or sets default share link scope on core partition
        /// </summary>
        SharingScope CoreDefaultShareLinkScope { get; set; }

        /// <summary>
        /// Gets or sets request files link expiration days on core partition
        /// </summary>
        int CoreRequestFilesLinkExpirationInDays { get; set; }

        /// <summary>
        /// Enable the request files functionality for the tenant
        /// </summary>
        bool CoreRequestFilesLinkEnabled { get; set; }

        /// <summary>
        /// Gets or sets collaboration type on core partition
        /// </summary>
        SharingCapabilities CoreSharingCapability { get; set; }

        /// <summary>
        /// Gets or sets customized external sharing service url
        /// </summary>
        string CustomizedExternalSharingServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets default link permission
        /// </summary>
        SharingPermissionType DefaultLinkPermission { get; set; }

        /// <summary>
        /// Gets or sets DefaultODBMode value
        /// </summary>
        string DefaultODBMode { get; set; }

        /// <summary>
        /// Gets or sets default sharing link type for the tenant
        /// </summary>
        SharingLinkType DefaultSharingLinkType { get; set; }

        /// <summary>
        /// Gets or sets a value to handle guest sharing group's allow list
        /// </summary>
        IList<string> DenySelectSecurityGroupsInSPSitesList { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Add To OneDrive is disabled
        /// </summary>
        bool DisableAddToOneDrive { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether back to classic link is disabled in Modern UX
        /// </summary>
        bool DisableBackToClassic { get; set; }

        /// <summary>
        /// Gets or sets the value of whether ACS app only tokens are blocked. True means it's blocked
        /// </summary>
        bool DisableCustomAppAuthentication { get; set; }

        /// <summary>
        /// Get/Set whether Nucleus Sync should be disabled for Lists
        /// </summary>
        bool DisableListSync { get; set; }

        /// <summary>
        /// Indicates whether Viva Connections Analytics is disabled or not on the tenant
        /// </summary>
        bool DisableVivaConnectionsAnalytics { get; set; }

        /// <summary>
        /// An array of modern List template ids that are disabled on the tenant
        /// </summary>
        Guid[] DisabledModernListTemplateIds { get; set; }

        /// <summary>
        /// Gets or sets the list to disable web parts. The Guid is the web part Guid defined in web part's manifest
        /// </summary>
        Guid[] DisabledWebPartIds { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Outlook PST version trimming is disabled or not
        /// </summary>
        bool DisableOutlookPSTVersionTrimming { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether personal list creation is disabled or not
        /// </summary>
        bool DisablePersonalListCreation { get; set; }

        /// <summary>
        /// Disable sync client report problem dialog
        /// </summary>
        bool DisableReportProblemDialog { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether activation of spaces are disabled or not
        /// </summary>
        bool DisableSpacesActivation { get; set; }

        /// <summary>
        /// Don't allow download of files marked as infected
        /// </summary>
        bool DisallowInfectedFileDownload { get; set; }

        /// <summary>
        /// Get/Set DisplayNamesOfFileViewers Tenant settings for Analytics Privacy property
        /// </summary>
        bool DisplayNamesOfFileViewers { get; set; }

        /// <summary>
        /// Get/Set DisplayNamesOfFileViewersInSpo Tenant settings for Analytics Privacy property
        /// </summary>
        bool DisplayNamesOfFileViewersInSpo { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether tenant users see the "Start a fresh site" menu option
        /// </summary>
        /// <value>
        /// True indicates that this menu option is present for tenant users.
        /// False indicates the menu option is hidden for tenant users.
        /// </value>
        bool DisplayStartASiteOption { get; set; }

        /// <summary>
        /// Gets or sets a value to handle email attestation
        /// </summary>
        bool EmailAttestationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the time between reattestation
        /// </summary>
        int EmailAttestationReAuthDays { get; set; }

        /// <summary>
        /// Gets or sets the EmailAttestationRequired setting for the tenant
        /// </summary>
        bool EmailAttestationRequired { get; set; }

        /// <summary>
        /// Gets or sets the AIPIntegrationKey cache property for the tenant
        /// </summary>
        bool EnableAIPIntegration { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Auto news digest is enabled
        /// </summary>
        bool EnableAutoNewsDigest { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether AAD B2B integration is enabled
        /// </summary>
        bool EnableAzureADB2BIntegration { get; set; }

        /// <summary>
        /// Get status of flight AllowAADB2BSkipCheckingOTP
        /// </summary>
        bool EnabledFlightAllowAADB2BSkipCheckingOTP { get; }

        /// <summary>
        /// Gets or sets a whether we force the auto-acceleration sign-in
        /// </summary>
        bool EnableGuestSignInAcceleration { get; set; }

        /// <summary>
        /// Gets or sets the MinimumVersioning cache property for the tenant
        /// </summary>
        bool EnableMinimumVersionRequirement { get; set; }

        /// <summary>
        /// Gets the EnableMipSiteLabel property of the tenant
        /// </summary>
        bool EnableMipSiteLabel { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if promoted file handlers are enabled
        /// </summary>
        bool EnablePromotedFileHandlers { get; set; }

        /// <summary>
        /// Gets or sets the value of policy which determines whether Restricted Access Control Policy is enabled
        /// </summary>
        bool EnableRestrictedAccessControl { get; set; }

        /// <summary>
        /// Get/Set excluded file extensions for sync client
        /// </summary>
        IList<string> ExcludedFileExtensionsForSyncClient { get; set; }

        /// <summary>
        /// Determines whether external services are enabled
        /// </summary>
        bool ExternalServicesEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether external user expiration is enabled for the tenant
        /// </summary>
        bool ExternalUserExpirationRequired { get; set; }

        /// <summary>
        /// Gets or sets the number of days before external user expiration if not individually extended
        /// </summary>
        int ExternalUserExpireInDays { get; set; }

        /// <summary>
        /// Gets or sets file anonymous link type for the tenant
        /// </summary>
        AnonymousLinkType FileAnonymousLinkType { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if External Image Search is enabled on the File Picker
        /// </summary>
        bool FilePickerExternalImageSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets folder anonymous link type for the tenant
        /// </summary>
        AnonymousLinkType FolderAnonymousLinkType { get; set; }

        /// <summary>
        /// Gets or sets a value to handle guest sharing group's allow list 
        /// </summary>
        string GuestSharingGroupAllowListInTenant { get; set; }

        /// <summary>
        /// Gets: a list of PrincipalNames
        /// Example output: List of PrincipalNames. e.g. ["c:0-.f|rolemanager|contoso-all-users/35533f7d-4536-4c82-9dbc-352f9424578b"]
        /// Sets: take a list of principalNames
        /// </summary>
        IList<string> GuestSharingGroupAllowListInTenantByPrincipalIdentity { get; set; }

        /// <summary>
        /// Gets or sets the value if the tenant admin has completed CU configuration
        /// </summary>
        bool HasAdminCompletedCUConfiguration { get; set; }

        /// <summary>
        /// Gets or sets whether the tenant has Intelligent Content Services Capability or not
        /// </summary>
        bool HasIntelligentContentServicesCapability { get; set; }

        /// <summary>
        /// Gets or sets whether the tenant has Topic Experiences Capability or not
        /// </summary>
        bool HasTopicExperiencesCapability { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether the sync button on team sites and other ODBs is hidden.
        /// (Basically this hides the sync button on all document libraries except the OneDrive for Business that the user owns.)
        /// </summary>
        bool HideSyncButtonOnDocLib { get; set; }

        /// <summary>
        /// Get/Set whether to hide the sync button on OneDrive for Business sites
        /// </summary>
        bool HideSyncButtonOnODB { get; set; }

        /// <summary>
        /// Gets or sets the value of the image tagging option
        /// </summary>
        ImageTaggingChoice ImageTaggingOption { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether to include at a glance in the sharing emails
        /// </summary>
        bool IncludeAtAGlanceInShareEmails { get; set; }

        /// <summary>
        /// Gets or sets IBImplicitGroupBased value
        /// </summary>
        bool IBImplicitGroupBased { get; set; }

        /// <summary>
        /// Gets or sets InformationBarriersSuspension value
        /// </summary>
        bool InformationBarriersSuspension { get; set; }

        /// <summary>
        /// Enforces incoming requests are coming from the address range in IPAddressAllowList
        /// </summary>
        string IPAddressAllowList { get; set; }

        /// <summary>
        /// nforces incoming requests are coming from the address range in IPAddressAllowList
        /// </summary>
        bool IPAddressEnforcement { get; set; }

        /// <summary>
        /// The WAC Loopback token lifetime. Default is 15 minutes
        /// </summary>
        int IPAddressWACTokenLifetime { get; set; }

        /// <summary>
        /// Get or sets the IsAppBarTemporarilyDisabled flag
        /// </summary>
        bool IsAppBarTemporarilyDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether CollabMeetingNotes Fluid Framework is enabled
        /// If IsFluidEnabled disabled, IsCollabMeetingNotesFluidEnabled will be disabled automatically
        /// If IsFluidEnabled enabled, IsCollabMeetingNotesFluidEnabled will be enabled automatically
        /// IsCollabMeetingNotesFluidEnabled can be enabled only when IsFluidEnabled is already enabled
        /// </summary>
        bool IsCollabMeetingNotesFluidEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Fluid Framework is enabled
        /// </summary>
        bool IsFluidEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether the Loop feature for the Fluid Framework is enabled
        /// </summary>
        bool IsLoopEnabled { get; set; }

        /// <summary>
        /// Get status of HubSitesMultiGeo flight
        /// </summary>
        bool IsHubSitesMultiGeoFlightEnabled { get; }

        /// <summary>
        /// Get status of MnA flight
        /// </summary>
        bool IsMnAFlightEnabled { get; }

        /// <summary>
        /// The property indicates if tenant has multi-geo tenant instances
        /// </summary>
        bool IsMultiGeo { get; }

        /// <summary>
        /// Status of flight IsMultipleHomeSitesFlightEnabled
        /// </summary>
        bool IsMultipleHomeSitesFlightEnabled { get; }

        /// <summary>
        /// Get/Set sync client restrictions
        /// </summary>
        bool IsUnmanagedSyncClientForTenantRestricted { get; set; }

        /// <summary>
        /// Get status of feature sync client restriction allowed
        /// </summary>
        bool IsUnmanagedSyncClientRestrictionFlightEnabled { get; }

        /// <summary>
        /// Status of flight IsVivaHomeFlightEnabled
        /// </summary>
        bool IsVivaHomeFlightEnabled { get; }

        /// <summary>
        /// Gets or sets a value to specify whether Whiteboard Fluid Framework is enabled
        /// If IsFluidEnabled disabled, IsWBFluidEnabled will be disabled automatically
        /// If IsFluidEnabled enabled, IsWBFluidEnabled will be enabled automatically
        /// IsWBFluidEnabled can be enabled only when IsFluidEnabled is already enabled
        /// </summary>
        bool IsWBFluidEnabled { get; set; }

        /// <summary>
        /// Gets or sets the LabelMismatchEmailHelpLink property
        /// </summary>
        string LabelMismatchEmailHelpLink { get; set; }

        /// <summary>
        /// Gets or sets the value if ADAL is disabled in the tenant. True value means it is disabled
        /// </summary>
        bool LegacyAuthProtocolsEnabled { get; set; }

        /// <summary>
        /// Specifies what files can be viewed when ConditionalAccessPolicy is set to AllowLimitedAccess
        /// </summary>
        SPOLimitedAccessFileType LimitedAccessFileType { get; set; }

        /// <summary>
        /// Gets or sets the value if the ML capture settings should be shown in the tenant. True means that it is shown
        /// </summary>
        bool MachineLearningCaptureEnabled { get; set; }

        /// <summary>
        /// Gets or sets the MarkNewFilesSensitiveByDefault property
        /// </summary>
        SensitiveByDefaultState MarkNewFilesSensitiveByDefault { get; set; }

        /// <summary>
        /// Gets or sets the media transcription policy
        /// </summary>
        MediaTranscriptionPolicyType MediaTranscription { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if user checks handle mobile friendly url
        /// </summary>
        bool MobileFriendlyUrlEnabledInTenant { get; set; }

        /// <summary>
        /// When a site in the tenancy is locked it is redirected to this Url
        /// </summary>
        string NoAccessRedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value PushNotificationsEnabled in ODB
        /// </summary>
        bool NotificationsInOneDriveForBusinessEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value PushNotificationsEnabled in SharePoint
        /// </summary>
        bool NotificationsInSharePointEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value for owner notification accept
        /// </summary>
        bool NotifyOwnersWhenInvitationsAccepted { get; set; }

        /// <summary>
        /// Gets or sets a value for owner notification reshare
        /// </summary>
        bool NotifyOwnersWhenItemsReshared { get; set; }

        /// <summary>
        /// Gets or sets name of the file which contains the list which contains OCR for full text search enabled sites by the tenant admin
        /// </summary>
        string OCRAdminSiteListFileName { get; set; }

        /// <summary>
        /// Gets or sets name of the file which contains the list which contains OCR for full text search enabled sites by the compliance admin
        /// </summary>
        string OCRComplianceSiteListFileName { get; set; }

        /// <summary>
        /// Gets or sets an enum value that specifies whether the tenant admin has enabled OCR on SharePoint sites for full text search through Syntex
        /// </summary>
        ObjectCharacterRecognitionMode OCRModeForAdminSites { get; set; }

        /// <summary>
        /// Gets or sets an enum value that specifies whether the compliance admin has enabled OCR for full text search through Syntex for ODBs
        /// </summary>
        /// <value>
        /// Disabled indicates that the OCR features should be disabled
        /// InclusionList indicates that the OCR features should be enabled and filter to include specific ODBs
        /// ExclusionList indicates that the OCR features should be enabled and filter to exclude specific ODBs
        /// </value>
        ObjectCharacterRecognitionMode OCRModeForComplianceODBs { get; set; }

        /// <summary>
        /// Gets or sets an enum value that specifies whether the compliance admin has enabled OCR for full text search through Syntex for SharePoint sites
        /// </summary>
        /// <value>
        /// Disabled indicates that the OCR features should be disabled
        /// InclusionList indicates that the OCR features should be enabled and filter to include specific sites
        /// ExclusionList indicates that the OCR features should be enabled and filter to exclude specific sites
        /// </value>
        ObjectCharacterRecognitionMode OCRModeForComplianceSites { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if AccessRequests is On, Off or Unspecified for Onedrive for Business
        /// </summary>
        SharingState ODBAccessRequests { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if MembersCanShare is On, Off or Unspecified for Onedrive for Business
        /// </summary>
        SharingState ODBMembersCanShare { get; set; }

        /// <summary>
        /// Gets or sets a value to Onedrive for Business sharing capability
        /// </summary>
        SharingCapabilities ODBSharingCapability { get; set; }

        /// <summary>
        /// Gets or sets the value if ADAL is disabled in the tenant for Office clients. True value means it is disabled
        /// </summary>
        bool OfficeClientADALDisabled { get; set; }

        /// <summary>
        /// Gets or sets default share link to existing access on OneDrive partition
        /// </summary>
        bool OneDriveDefaultLinkToExistingAccess { get; set; }

        /// <summary>
        /// Gets or sets default share link role on OneDrive partition
        /// </summary>
        Role OneDriveDefaultShareLinkRole { get; set; }

        /// <summary>
        /// Gets or sets default share link scope on OneDrive partition
        /// </summary>
        SharingScope OneDriveDefaultShareLinkScope { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if OneDriveForGuestUsers functionality is enabled for the tenant
        /// </summary>
        bool OneDriveForGuestsEnabled { get; set; }

        /// <summary>
        /// Gets or sets request files link enabled on OneDrive partition
        /// </summary>
        bool OneDriveRequestFilesLinkEnabled { get; set; }

        /// <summary>
        /// Gets or sets request files link expiration days on OneDrive partition
        /// </summary>
        int OneDriveRequestFilesLinkExpirationInDays { get; set; }

        /// <summary>
        /// The default OneDrive quota allocated to new OneDrive sites for the tenant's users
        /// </summary>
        long OneDriveStorageQuota { get; set; }

        /// <summary>
        /// Gets or sets OrgNewsSiteUrl
        /// </summary>
        string OrgNewsSiteUrl { get; set; }

        /// <summary>
        /// Gets or Sets The default Retention Days set to Personal Sites for a tenant
        /// </summary>
        int OrphanedPersonalSitesRetentionPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value for anonymous owner notification
        /// </summary>
        bool OwnerAnonymousNotification { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if external users are allowed to reshare, regardless of Members Can Share state
        /// </summary>
        bool PreventExternalUsersFromResharing { get; set; }

        /// <summary>
        /// Gets or Sets whether Onedrive for Business sites should have the Shared with Everyone folder automatically provisioned or not
        /// </summary>
        bool ProvisionSharedWithEveryoneFolder { get; set; }

        /// <summary>
        /// Gets or sets a value to specify what file types can be exposed through Public CDN
        /// </summary>
        string PublicCdnAllowedFileTypes { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Public CDN feature is enabled or disabled for the tenant
        /// </summary>
        bool PublicCdnEnabled { get; set; }

        /// <summary>
        /// Gets a list of the Public CDN origins
        /// </summary>
        IList<string> PublicCdnOrigins { get; }

        /// <summary>
        /// Gets or sets whether Hashed Proof Token IP binding is enabled
        /// </summary>
        bool ReduceTempTokenLifetimeEnabled { get; set; }

        /// <summary>
        /// Determines the grace period for Hashed Proof Tokens from an IP address that doesn't match the
        /// IP address in the token, when the IP policy is not enabled and IP Binding is enabled.
        /// </summary>
        int ReduceTempTokenLifetimeValue { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if user accepting invitation must use the same email address invitation was sent to
        /// </summary>
        bool RequireAcceptingAccountMatchInvitedAccount { get; set; }

        /// <summary>
        /// Gets or sets a value to specify what external sharing capabilities are available for the tenant
        /// </summary>
        int RequireAnonymousLinksExpireInDays { get; set; }

        /// <summary>
        /// Gets the info whether tenant has license for Restricted Onedrive for Business
        /// </summary>
        bool RestrictedOneDriveLicense { get; }

        /// <summary>
        /// Gets the info whether tenant has license for Restricted SharePoint
        /// </summary>
        bool RestrictedSharePointLicense { get; }

        /// <summary>
        /// The tenant's root site url
        /// </summary>
        string RootSiteUrl { get; }

        /// <summary>
        /// Gets or sets the value if Search and resolve in People Picker use exact match on Email or UPN. False value means default behavior of "starts with" matching against common user properties
        /// </summary>
        bool SearchResolveExactEmailOrUPN { get; set; }

        /// <summary>
        /// Gets or sets list of allowed domains
        /// </summary>
        string SharingAllowedDomainList { get; set; }

        /// <summary>
        /// Gets or sets list of blocked domains
        /// </summary>
        string SharingBlockedDomainList { get; set; }

        /// <summary>
        /// Gets or sets a value to specify what external sharing capabilities are available for the tenant
        /// </summary>
        SharingCapabilities SharingCapability { get; set; }

        /// <summary>
        /// Gets or sets the restriction mode
        /// </summary>
        SharingDomainRestrictionModes SharingDomainRestrictionMode { get; set; }

        /// <summary>
        /// Gets or sets the value if AllUsers claim is visible or not in people picker. False value means it is hidden
        /// </summary>
        bool ShowAllUsersClaim { get; set; }

        /// <summary>
        /// Gets or sets the value if Everyone claim is visible or not in people picker. False value means it is hidden
        /// </summary>
        bool ShowEveryoneClaim { get; set; }

        /// <summary>
        /// Gets or sets the value if EveryoneExceptExternalUsers claim is visible or not in people picker. False value means it is hidden
        /// </summary>
        bool ShowEveryoneExceptExternalUsersClaim { get; set; }

        /// <summary>
        /// Get/Set ShowOpenInDesktopOptionForSyncedFiles value
        /// </summary>
        bool ShowOpenInDesktopOptionForSyncedFiles { get; set; }

        /// <summary>
        /// Gets or sets a value to handle if showing group suggestions for IB is supported
        /// </summary>
        bool ShowPeoplePickerGroupSuggestionsForIB { get; set; }

        /// <summary>
        /// Gets or sets a value that allows members to search all existing guest users in the directory.       
        /// When set to true, members can search all existing guest users in the directory.
        /// </summary>
        bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }

        /// <summary>
        /// Gets or sets a string which specifies the SignInAccelerationDomain
        /// </summary>
        /// <remarks>
        /// When set, end-user sign-in will skip the default sign-in page, and will take the user directly
        /// to the sign-in page on the ADFS sign-in (OnPremises Active Directory).
        /// To be used only by organizations that don’t have Guest Sign-ins enabled.
        /// When not set, the behavior of end-user sign-in will be the default behavior as it is today –
        /// i.e. the end user will be taken to the default sign-in page, and from there onward to ADFS sign-in.
        /// </remarks>
        string SignInAccelerationDomain { get; set; }

        /// <summary>
        /// Whether social bar on site pages is enabled or not
        /// </summary>
        bool SocialBarOnSitePagesDisabled { get; set; }

        /// <summary>
        /// Gets or Sets a value to specify whether # and % are valid in file and folder names in SPO document libraries and OneDrive for Business
        /// </summary>
        SpecialCharactersState SpecialCharactersStateInFileFolderNames { get; set; }

        /// <summary>
        /// Gets or sets a string which specifies the URL of the form to load in the Start a Site dialog
        /// </summary>
        string StartASiteFormUrl { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether new 2010 workflows can be created
        /// </summary>
        bool StopNew2010Workflows { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether new 2013 workflows can be created
        /// </summary>
        bool StopNew2013Workflows { get; set; }

        /// <summary>
        /// Storage quota that is available for all sites in the tenant
        /// </summary>
        long StorageQuota { get; }

        /// <summary>
        /// Storage quota that is allocated for all sites in the tenant
        /// </summary>
        long StorageQuotaAllocated { get; }

        /// <summary>
        /// Gets or sets a value to specify the Stream launch tile URL in office.com
        /// </summary>
        int StreamLaunchConfig { get; set; }

        /// <summary>
        /// Gets last updated timestamp for StreamLaunchConfig property
        /// </summary>
        DateTime StreamLaunchConfigLastUpdated { get; }

        /// <summary>
        /// Gets update count for StreamLaunchConfig property
        /// </summary>
        int StreamLaunchConfigUpdateCount { get; }

        /// <summary>
        /// Gets or sets whether or not the AAD B2B management policy will be synced on the next request
        /// </summary>
        bool SyncAadB2BManagementPolicy { get; set; }

        /// <summary>
        /// Gets or sets whether or not the synced tenant properties will be updated on the next request
        /// </summary>
        bool SyncPrivacyProfileProperties { get; set; }

        /// <summary>
        /// Gets or sets the value of the TLS token binding policy
        /// </summary>
        SPOTlsTokenBindingPolicyValue TlsTokenBindingPolicyValue { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if use FindPeople in PeoplePicker
        /// </summary>
        bool UseFindPeopleInPeoplePicker { get; set; }

        /// <summary>
        /// Gets or sets a Boolean value that specifies whether ExplorerView feature uses persistent cookies
        /// </summary>
        bool UsePersistentCookiesForExplorerView { get; set; }

        /// <summary>
        /// Gets or sets a value to specify if User Voice for customer feedback is enabled
        /// </summary>
        bool UserVoiceForFeedbackEnabled { get; set; }

        /// <summary>
        /// Whether viewers commenting on media items is disabled or not
        /// </summary>
        bool ViewersCanCommentOnMediaDisabled { get; set; }

        /// <summary>
        /// Gets or sets the value of the setting which enables users to view files in Explorer
        /// </summary>
        bool ViewInFileExplorerEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value to handle the tenant who can share settings
        /// </summary>
        string WhoCanShareAllowListInTenant { get; set; }

        /// <summary>
        /// Gets: a list of PrincipalNames
        /// Example output: List of PrincipalNames. e.g. ["c:0-.f|rolemanager|contoso-all-users/35533f7d-4536-4c82-9dbc-352f9424578b"]
        /// Sets: take a list of principalNames
        /// </summary>
        IList<string> WhoCanShareAllowListInTenantByPrincipalIdentity { get; set; }

        /// <summary>
        /// Gets or sets a value to specify whether Workflow 2010 is disabled
        /// </summary>
        bool Workflow2010Disabled { get; set; }

        /// <summary>
        /// Gets whether 2013 workflows are configured and enabled for the tenant
        /// </summary>
        Workflows2013State Workflows2013State { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the properties of this site collection
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        Task UpdateAsync(VanityUrlOptions vanityUrlOptions = null);

        /// <summary>
        /// Updates the properties of this site collection
        /// </summary>
        /// <param name="vanityUrlOptions">Optionally specify the custom vanity URI's used by this tenant</param>
        /// <returns></returns>
        void Update(VanityUrlOptions vanityUrlOptions = null);

        #endregion
    }
}
