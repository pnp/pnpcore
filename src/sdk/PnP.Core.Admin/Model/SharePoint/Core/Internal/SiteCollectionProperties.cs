using PnP.Core.Model;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    [SharePointType("Microsoft.Online.SharePoint.TenantAdministration.SiteProperties")]
    internal sealed class SiteCollectionProperties : TransientObject, ISiteCollectionProperties, IDataModelWithContext
    {
        #region Properties
        [JsonPropertyName("_ObjectIdentity_")]
        public string ObjectIdentity { get; set; }

        public bool AllowDownloadingNonWebViewableFiles { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowEditing { get => GetValue<bool>(); set => SetValue(value); }

        public bool AllowSelfServiceUpgrade { get => GetValue<bool>(); set => SetValue(value); }

        public int AnonymousLinkExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public string AuthContextStrength { get => GetValue<string>(); set => SetValue(value); }

        public string AuthenticationContextName { get => GetValue<string>(); set => SetValue(value); }

        public BlockDownloadLinksFileTypes BlockDownloadLinksFileType { get => GetValue<BlockDownloadLinksFileTypes>(); set => SetValue(value); }

        public bool BlockDownloadPolicy { get => GetValue<bool>(); set => SetValue(value); }

        public bool ClearRestrictedAccessControl { get => GetValue<bool>(); set => SetValue(value); }

        public bool CommentsOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public int CompatibilityLevel { get => GetValue<int>(); set => SetValue(value); }

        public SPOConditionalAccessPolicyType ConditionalAccessPolicy { get => GetValue<SPOConditionalAccessPolicyType>(); set => SetValue(value); }

        public SharingPermissionType DefaultLinkPermission { get => GetValue<SharingPermissionType>(); set => SetValue(value); }

        public bool DefaultLinkToExistingAccess { get => GetValue<bool>(); set => SetValue(value); }

        public bool DefaultLinkToExistingAccessReset { get => GetValue<bool>(); set => SetValue(value); }

        public Role DefaultShareLinkRole { get => GetValue<Role>(); set => SetValue(value); }

        public SharingScope DefaultShareLinkScope { get => GetValue<SharingScope>(); set => SetValue(value); }

        public SharingLinkType DefaultSharingLinkType { get => GetValue<SharingLinkType>(); set => SetValue(value); }

        public DenyAddAndCustomizePagesStatus DenyAddAndCustomizePages { get => GetValue<DenyAddAndCustomizePagesStatus>(); set => SetValue(value); }

        public string Description { get => GetValue<string>(); set => SetValue(value); }

        public AppViewsPolicy DisableAppViews { get => GetValue<AppViewsPolicy>(); set => SetValue(value); }

        public CompanyWideSharingLinksPolicy DisableCompanyWideSharingLinks { get => GetValue<CompanyWideSharingLinksPolicy>(); set => SetValue(value); }

        public FlowsPolicy DisableFlows { get => GetValue<FlowsPolicy>(); set => SetValue(value); }

        public bool ExcludeBlockDownloadPolicySiteOwners { get => GetValue<bool>(); set => SetValue(value); }

        public int ExternalUserExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public Guid GroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public string GroupOwnerLoginName { get => GetValue<string>(); set => SetValue(value); }

        public bool HasHolds { get => GetValue<bool>(); set => SetValue(value); }

        public Guid HubSiteId { get => GetValue<Guid>(); set => SetValue(value); }

        public string IBMode { get => GetValue<string>(); set => SetValue(value); }

        public Guid[] IBSegments { get => GetValue<Guid[]>(); set => SetValue(value); }

        public Guid[] IBSegmentsToAdd { get => GetValue<Guid[]>(); set => SetValue(value); }

        public Guid[] IBSegmentsToRemove { get => GetValue<Guid[]>(); set => SetValue(value); }

        public bool IsGroupOwnerSiteAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsHubSite { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsTeamsChannelConnected { get => GetValue<bool>(); set => SetValue(value); }

        public bool IsTeamsConnected { get => GetValue<bool>(); set => SetValue(value); }

        public DateTime LastContentModifiedDate { get => GetValue<DateTime>(); set => SetValue(value); }

        public int Lcid { get => GetValue<int>(); set => SetValue(value); }

        public SPOLimitedAccessFileType LimitedAccessFileType { get => GetValue<SPOLimitedAccessFileType>(); set => SetValue(value); }

        public string LockIssue { get => GetValue<string>(); set => SetValue(value); }

        public string LockState { get => GetValue<string>(); set => SetValue(value); }

        public MediaTranscriptionPolicyType MediaTranscription { get => GetValue<MediaTranscriptionPolicyType>(); set => SetValue(value); }

        public SiteUserInfoVisibilityPolicyValue OverrideBlockUserInfoVisibility { get => GetValue<SiteUserInfoVisibilityPolicyValue>(); set => SetValue(value); }

        public bool OverrideTenantAnonymousLinkExpirationPolicy { get => GetValue<bool>(); set => SetValue(value); }

        public bool OverrideTenantExternalUserExpirationPolicy { get => GetValue<bool>(); set => SetValue(value); }

        public string Owner { get => GetValue<string>(); set => SetValue(value); }

        public string OwnerEmail { get => GetValue<string>(); set => SetValue(value); }

        public string OwnerLoginName { get => GetValue<string>(); set => SetValue(value); }

        public string OwnerName { get => GetValue<string>(); set => SetValue(value); }

        public PWAEnabledStatus PWAEnabled { get => GetValue<PWAEnabledStatus>(); set => SetValue(value); }

        public bool ReadOnlyForUnmanagedDevices { get => GetValue<bool>(); set => SetValue(value); }

        public Guid RelatedGroupId { get => GetValue<Guid>(); set => SetValue(value); }

        public bool RequestFilesLinkEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public int RequestFilesLinkExpirationInDays { get => GetValue<int>(); set => SetValue(value); }

        public bool RestrictedAccessControl { get => GetValue<bool>(); set => SetValue(value); }

        public IList<Guid> RestrictedAccessControlGroups { get => GetValue<IList<Guid>>(); set => SetValue(value); }

        public IList<Guid> RestrictedAccessControlGroupsToAdd { get => GetValue<IList<Guid>>(); set => SetValue(value); }

        public IList<Guid> RestrictedAccessControlGroupsToRemove { get => GetValue<IList<Guid>>(); set => SetValue(value); }

        public RestrictedToRegion RestrictedToRegion { get => GetValue<RestrictedToRegion>(); set => SetValue(value); }

        public Guid SensitivityLabel { get => GetValue<Guid>(); set => SetValue(value); }

        public string SensitivityLabel2 { get => GetValue<string>(); set => SetValue(value); }

        public bool SetOwnerWithoutUpdatingSecondaryAdmin { get => GetValue<bool>(); set => SetValue(value); }

        public string SharingAllowedDomainList { get => GetValue<string>(); set => SetValue(value); }

        public string SharingBlockedDomainList { get => GetValue<string>(); set => SetValue(value); }

        public SharingCapabilities SharingCapability { get => GetValue<SharingCapabilities>(); set => SetValue(value); }

        public SharingDomainRestrictionModes SharingDomainRestrictionMode { get => GetValue<SharingDomainRestrictionModes>(); set => SetValue(value); }

        public bool SharingLockDownCanBeCleared { get => GetValue<bool>(); set => SetValue(value); }

        public bool SharingLockDownEnabled { get => GetValue<bool>(); set => SetValue(value); }

        public bool ShowPeoplePickerSuggestionsForGuestUsers { get => GetValue<bool>(); set => SetValue(value); }

        public SharingCapabilities SiteDefinedSharingCapability { get => GetValue<SharingCapabilities>(); set => SetValue(value); }

        public bool SocialBarOnSitePagesDisabled { get => GetValue<bool>(); set => SetValue(value); }

        public string Status { get => GetValue<string>(); set => SetValue(value); }

        public long StorageMaximumLevel { get => GetValue<long>(); set => SetValue(value); }

        public string StorageQuotaType { get => GetValue<string>(); set => SetValue(value); }

        public long StorageUsage { get => GetValue<long>(); set => SetValue(value); }

        public long StorageWarningLevel { get => GetValue<long>(); set => SetValue(value); }

        public TeamsChannelTypeValue TeamsChannelType { get => GetValue<TeamsChannelTypeValue>(); set => SetValue(value); }

        public string Template { get => GetValue<string>(); set => SetValue(value); }

        public TimeZone TimeZoneId { get => GetValue<TimeZone>(); set => SetValue(value); }

        public string Title { get => GetValue<string>(); set => SetValue(value); }

        public string Url { get => GetValue<string>(); set => SetValue(value); }

        public int WebsCount { get => GetValue<int>(); set => SetValue(value); }

        public PnPContext PnPContext { get; set; }

        #endregion

        #region Methods

        public async Task UpdateAsync(VanityUrlOptions vanityUrlOptions = null)
        {
            await SiteCollectionManagement.UpdateSiteCollectionPropertiesAsync(PnPContext, this, vanityUrlOptions).ConfigureAwait(false);
        }

        public void Update(VanityUrlOptions vanityUrlOptions = null)
        {
            UpdateAsync(vanityUrlOptions).GetAwaiter().GetResult();
        }        
        #endregion
    }
}
