using PnP.Core.Model.SharePoint;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PnP.Core.Admin.Model.SharePoint
{
    /// <summary>
    /// Properties of a site collection
    /// </summary>
    public interface ISiteCollectionProperties
    {
        #region Properties

        /// <summary>
        /// Indicates whether end users can download non-viewable files (e.g. zip)
        /// from browser. By default, this would be set to true if setting ConditionalAccessPolicy to AllowLimitedAccess.
        /// This can be set to false to block automatic download of files that can't be vieweed in the browser
        /// </summary>
        bool AllowDownloadingNonWebViewableFiles { get; set; }

        /// <summary>
        /// Indicates whether WAC files should be open in Edit mode.
        /// By default, this would be set to true if setting ConditionalAccessPolicy to AllowLimitedAccess.
        /// This can be set to false to enable view only mode in WAC.
        /// </summary>
        bool AllowEditing { get; set; }

        /// <summary>
        /// Flag that indicates is a site supports self-service upgrade
        /// </summary>
        bool AllowSelfServiceUpgrade { get; set; }

        /// <summary>
        /// Anonymous link expiration in days
        /// </summary>
        int AnonymousLinkExpirationInDays { get; set; }

        /// <summary>
        /// Gets or sets the authentication context strength for this site for all the webs
        /// </summary>
        [Obsolete("Please use the AuthenticationContextName property")]
        string AuthContextStrength { get; set; }

        /// <summary>
        /// Gets or sets the authentication context for this site for all the webs
        /// </summary>
        string AuthenticationContextName { get; set; }

        /// <summary>
        /// Specifies the types of files that can be displayed when the block download links feature is being used
        /// </summary>
        BlockDownloadLinksFileTypes BlockDownloadLinksFileType { get; set; }

        /// <summary>
        /// Gets or sets the Block download policy flag
        /// </summary>
        bool BlockDownloadPolicy { get; set; }

        /// <summary>
        /// Clears the Restricted access control groups
        /// </summary>
        bool ClearRestrictedAccessControl { get; set; }

        /// <summary>
        /// Whether comments on site pages are disabled or not
        /// </summary>
        bool CommentsOnSitePagesDisabled { get; set; }
        
        /// <summary>
        /// The compatibility leve of this site
        /// </summary>
        int CompatibilityLevel { get; }

        /// <summary>
        /// Flag that controls access from devices that aren't compliant or joined to a domain to have
        /// limited access (web-only, without the Download, Print, and Sync commands)
        /// </summary>
        SPOConditionalAccessPolicyType ConditionalAccessPolicy { get; set; }

        /// <summary>
        /// The default link permission for this site
        /// </summary>
        SharingPermissionType DefaultLinkPermission { get; set; }

        /// <summary>
        /// The default link to existing access for this site
        /// </summary>
        bool DefaultLinkToExistingAccess { get; set; }

        /// <summary>
        /// This is to reset default link to existing access for this site. After resetting, the value will be default (false) or respect the higher level value
        /// </summary>
        bool DefaultLinkToExistingAccessReset { get; set; }

        /// <summary>
        /// Default share link role
        /// </summary>
        Role DefaultShareLinkRole { get; set; }

        /// <summary>
        /// Default share link scope
        /// </summary>
        SharingScope DefaultShareLinkScope { get; set; }

        /// <summary>
        /// The default link type for this site
        /// </summary>
        SharingLinkType DefaultSharingLinkType { get; set; }

        /// <summary>
        /// Determines whether the site has AddAndCustomizePages denied
        /// </summary>
        DenyAddAndCustomizePagesStatus DenyAddAndCustomizePages { get; set; }

        /// <summary>
        /// Site's description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Indicates whether app views are disabled in all the webs of this site
        /// </summary>
        AppViewsPolicy DisableAppViews { get; set; }

        /// <summary>
        /// Indicates whether company wide sharing links are disabled in all the webs of this site
        /// </summary>
        CompanyWideSharingLinksPolicy DisableCompanyWideSharingLinks { get; set; }

        /// <summary>
        /// Indicates whether flows are disabled in all the webs of this site
        /// </summary>
        FlowsPolicy DisableFlows { get; set; }

        /// <summary>
        /// Gets or sets the exclude site owners flag for Block download policy
        /// </summary>
        bool ExcludeBlockDownloadPolicySiteOwners { get; set; }
        
        /// <summary>
        /// External user expiration in days
        /// </summary>
        int ExternalUserExpirationInDays { get; set; }

        /// <summary>
        /// The GroupId of the site
        /// </summary>
        Guid GroupId { get; }

        /// <summary>
        /// The login name of the group owner
        /// </summary>
        string GroupOwnerLoginName { get; }

        /// <summary>
        /// Flag that indicates a site has Holds
        /// </summary>
        bool HasHolds { get; }

        /// <summary>
        /// The HubSiteId of the HubSite this site is associated with
        /// </summary>
        Guid HubSiteId { get; }

        /// <summary>
        /// get or Set IBMode (Information Barrier Mode)
        /// </summary>
        string IBMode { get; set; }

        /// <summary>
        /// Gets or sets the IB (Information Barrier Mode) segment GUIDs
        /// </summary>
        Guid[] IBSegments { get; set; }

        /// <summary>
        /// Gets or sets the IB (Information Barrier Mode) segments to add
        /// </summary>
        Guid[] IBSegmentsToAdd { get; set; }

        /// <summary>
        /// Gets or sets the IB (Information Barrier Mode) segments to remove
        /// </summary>
        Guid[] IBSegmentsToRemove { get; set; }

        /// <summary>
        /// Whether group owner is site admin
        /// </summary>
        bool IsGroupOwnerSiteAdmin { get; }
        
        /// <summary>
        /// Whether or not this site is a HubSite
        /// </summary>
        bool IsHubSite { get; }
        
        /// <summary>
        /// Gets if the site is connected to a team channel in Microsoft Teams.
        /// </summary>
        bool IsTeamsChannelConnected { get; }

        /// <summary>
        /// Gets if the site is connected to a team in Microsoft Teams
        /// </summary>
        bool IsTeamsConnected { get; }
        
        /// <summary>
        /// The last time content was modified on the site
        /// </summary>
        DateTime LastContentModifiedDate { get; }

        /// <summary>
        /// The Locale ID of the site
        /// </summary>
        int Lcid { get; set; }

        /// <summary>
        /// Specifies what files can be viewed when ConditionalAccessPolicy is set to AllowLimitedAccess
        /// </summary>
        SPOLimitedAccessFileType LimitedAccessFileType { get; set; }

        /// <summary>
        /// A description of the lock issue
        /// </summary>
        string LockIssue { get; }

        /// <summary>
        /// A string representing the lock state of the site. Valid values are 
        /// - Unlock: the site is not locked, default state
        /// - NoAccess: the site is locked for all access
        /// - ReadOnly: the site is set to read only status
        /// </summary>
        string LockState { get; set; }

        /// <summary>
        /// Gets or sets the media transcription policy
        /// </summary>
        MediaTranscriptionPolicyType MediaTranscription { get; set; }

        /// <summary>
        /// Indicates what the state of the browse user info policy in the site
        /// </summary>
        SiteUserInfoVisibilityPolicyValue OverrideBlockUserInfoVisibility { get; set; }

        /// <summary>
        /// This site overrides the tenant anonymous link expiration policy
        /// </summary>
        bool OverrideTenantAnonymousLinkExpirationPolicy { get; set; }

        /// <summary>
        /// This site overrides the tenant external user expiration policy
        /// </summary>
        bool OverrideTenantExternalUserExpirationPolicy { get; set; }

        /// <summary>
        /// The decoded login name of the site owner
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// The email address of the site owner
        /// </summary>
        string OwnerEmail { get; }

        /// <summary>
        /// The encoded login name of the site owner
        /// </summary>
        string OwnerLoginName { get; }

        /// <summary>
        /// The site owner name
        /// </summary>
        string OwnerName { get; }

        /// <summary>
        /// Determines whether PWA is enabled for the site
        /// </summary>
        PWAEnabledStatus PWAEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Read only access for unmanaged devices policy flag
        /// </summary>
        bool ReadOnlyForUnmanagedDevices { get; set; }

        /// <summary>
        /// The GroupId of the group this site is associated with
        /// </summary>
        Guid RelatedGroupId { get; }

        /// <summary>
        /// Gets or sets request files link enabled
        /// </summary>
        bool RequestFilesLinkEnabled { get; set; }

        /// <summary>
        /// Gets or sets request files link expiration days
        /// </summary>
        int RequestFilesLinkExpirationInDays { get; set; }

        /// <summary>
        /// Gets or sets the Restricted access control policy flag based on group membership
        /// </summary>
        bool RestrictedAccessControl { get; set; }

        /// <summary>
        /// Gets or sets the Restricted Access Control Groups
        /// </summary>
        IList<Guid> RestrictedAccessControlGroups { get; set; }

        /// <summary>
        /// Gets or sets the restricted access control groups to be added
        /// </summary>
        IList<Guid> RestrictedAccessControlGroupsToAdd { get; set; }

        /// <summary>
        /// Gets or sets the restricted access control groups to be removed
        /// </summary>
        IList<Guid> RestrictedAccessControlGroupsToRemove { get; set; }

        /// <summary>
        /// Determines whether the site is resticted to a specific geo location
        /// </summary>
        RestrictedToRegion RestrictedToRegion { get; set; }

        /// <summary>
        /// The Guid of an Information Protection label
        /// </summary>
        Guid SensitivityLabel { get; set; }

        /// <summary>
        /// The Guid of an Information Protection label
        /// </summary>
        string SensitivityLabel2 { get; set; }

        /// <summary>
        /// Whether update secondary admin during setting primary admin
        /// </summary>
        bool SetOwnerWithoutUpdatingSecondaryAdmin { get; set; }

        /// <summary>
        /// A list of allowed domain names for this site
        /// </summary>
        string SharingAllowedDomainList { get; set; }

        /// <summary>
        /// A list of blocked domain names for this site
        /// </summary>
        string SharingBlockedDomainList { get; set; }

        /// <summary>
        /// Specifies what type of external user and guest link sharing is enabled
        /// </summary>
        SharingCapabilities SharingCapability { get; set; }

        /// <summary>
        /// Indicates what this site's domain restriction mode is
        /// </summary>
        SharingDomainRestrictionModes SharingDomainRestrictionMode { get; set; }

        /// <summary>
        /// Get whether the sharing lock can be cleared
        /// </summary>
        bool SharingLockDownCanBeCleared { get; }

        /// <summary>
        /// Gets the SharingLockDownEnabled setting
        /// </summary>
        bool SharingLockDownEnabled { get; }

        /// <summary>
        /// Flag that controls allowing members to search guest users in the directory
        /// </summary>
        bool ShowPeoplePickerSuggestionsForGuestUsers { get; set; }

        /// <summary>
        /// Specifies what type of external user and guest link sharing is enabled for the site
        /// </summary>
        SharingCapabilities SiteDefinedSharingCapability { get; }

        /// <summary>
        /// Whether social bar on site pages is enabled or not
        /// </summary>
        bool SocialBarOnSitePagesDisabled { get; set; }
        /// <summary>
        /// The status of the site, possible values are:
        /// - Active: default status for a site
        /// - Recycled: the site is the tenant's site collection recyclebin
        /// </summary>
        string Status { get; }

        /// <summary>
        /// The Storage Quota
        /// </summary>
        long StorageMaximumLevel { get; set; }

        /// <summary>
        /// The storage quota type for the site
        /// </summary>
        string StorageQuotaType { get; }
        /// <summary>
        /// The current usage of storage for the site
        /// </summary>
        long StorageUsage { get; }

        /// <summary>
        /// The warning level for storage usage
        /// </summary>
        long StorageWarningLevel { get; set; }

        /// <summary>
        /// When the site is connected to a team channel in Microsoft Teams, gets the type of channel the site is connected to
        /// </summary>
        TeamsChannelTypeValue TeamsChannelType { get; }

        /// <summary>
        /// The site's web template name
        /// </summary>
        string Template { get; set; }
        
        /// <summary>
        /// The TimeZone
        /// </summary>
        TimeZone TimeZoneId { get; set; }

        /// <summary>
        /// The site's title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The Url of the site
        /// </summary>
        string Url { get; }
        
        /// <summary>
        /// The number of webs in the site
        /// </summary>
        int WebsCount { get; }

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
