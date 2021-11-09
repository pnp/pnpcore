using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal sealed class GetTenantPropertiesRequest : IRequest<object>
    {
        internal GetTenantPropertiesRequest()
        {
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();

            #region XML Payload generated
            /*

            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="2" ObjectPathId="1" />
                    <Query Id="3" ObjectPathId="1">
                        <Query SelectAllProperties="true">
                            <Properties />
                        </Query>
                    </Query>
                </Actions>
                <ObjectPaths>
                    <Constructor Id="1" TypeId="{268004ae-ef6b-4e9b-8425-127220d84719}" />
                </ObjectPaths>
            </Request>
            
             */
            #endregion

            List<ActionObjectPath> actions = new List<ActionObjectPath>();

            ActionObjectPath spoOperation = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = IdentityPath.ToString()
                },                
            };

            ActionObjectPath spoOperation3 = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = QueryIdPath,
                    ObjectPathId = IdentityPath.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true,
                    }
                },
            };

            ActionObjectPath spoGetSitePropertiesByUrlCollection = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath
                {
                    Id = IdentityPath,
                    TypeId = PnPAdminConstants.CsomTenantObject
                }
            };

            actions.Add(spoOperation);
            actions.Add(spoOperation3);
            actions.Add(spoGetSitePropertiesByUrlCollection);

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
            [
                {
                    "SchemaVersion": "15.0.0.0",
                    "LibraryVersion": "16.0.21820.12003",
                    "ErrorInfo": null,
                    "TraceCorrelationId": "02a1fd9f-a051-3000-4ada-4be8a5a8d4b2"
                },
                3,
                {
                    "IsNull": false
                },
                2,
                {
                    "_ObjectType_": "Microsoft.Online.SharePoint.TenantAdministration.Tenant",
                    "_ObjectIdentity_": "02a1fd9f-a051-3000-4ada-4be8a5a8d4b2|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9\nTenant",
                    "AIBuilderDefaultPowerAppsEnvironment": "",
                    "AIBuilderEnabled": true,
                    "AIBuilderSiteListFileName": "",
                    "AllowCommentsTextOnEmailEnabled": true,
                    "AllowDownloadingNonWebViewableFiles": true,
                    "AllowedDomainListForSyncClient": [],
                    "AllowEditing": true,
                    "AllowGuestUserShareToUsersNotInSiteCollection": false,
                    "AllowLimitedAccessOnUnmanagedDevices": false,
                    "AllowOverrideForBlockUserInfoVisibility": false,
                    "AllowSelectSGsInODBListInTenant": null,
                    "AnyoneLinkTrackUsers": false,
                    "ApplyAppEnforcedRestrictionsToAdHocRecipients": true,
                    "AuthContextResilienceMode": 0,
                    "BccExternalSharingInvitations": false,
                    "BccExternalSharingInvitationsList": null,
                    "BlockAccessOnUnmanagedDevices": false,
                    "BlockDownloadLinksFileType": 1,
                    "BlockDownloadOfAllFilesForGuests": false,
                    "BlockDownloadOfAllFilesOnUnmanagedDevices": false,
                    "BlockDownloadOfViewableFilesForGuests": false,
                    "BlockDownloadOfViewableFilesOnUnmanagedDevices": false,
                    "BlockMacSync": false,
                    "BlockSendLabelMismatchEmail": false,
                    "BlockUserInfoVisibility": "",
                    "BlockUserInfoVisibilityInOneDrive": 0,
                    "BlockUserInfoVisibilityInSharePoint": 0,
                    "ChannelMeetingRecordingPermission": 1,
                    "CommentsOnFilesDisabled": false,
                    "CommentsOnListItemsDisabled": false,
                    "CommentsOnSitePagesDisabled": false,
                    "CompatibilityRange": "15,15",
                    "ConditionalAccessPolicy": 0,
                    "ConditionalAccessPolicyErrorHelpLink": "",
                    "ContentTypeSyncSiteTemplatesList": [],
                    "CustomizedExternalSharingServiceUrl": "",
                    "DefaultContentCenterSite": {
                        "_ObjectType_": "Microsoft.Online.SharePoint.TenantAdministration.SiteInfoForSitePicker",
                        "Error": null,
                        "SiteId": "\/Guid(52e1f1b3-fed1-4a2f-afb0-d4680e3ac816)\/",
                        "SiteName": "AdminContentCenter",
                        "Url": "https:\u002f\u002fbertonline.sharepoint.com\u002fsites\u002fAdminContentCenter"
                    },
                    "DefaultLinkPermission": 2,
                    "DefaultODBMode": "Explicit",
                    "DefaultSharingLinkType": 2,
                    "DisableAddToOneDrive": false,
                    "DisableBackToClassic": false,
                    "DisableCustomAppAuthentication": false,
                    "DisabledModernListTemplateIds": [],
                    "DisabledWebPartIds": null,
                    "DisableListSync": false,
                    "DisableOutlookPSTVersionTrimming": false,
                    "DisablePersonalListCreation": false,
                    "DisableReportProblemDialog": false,
                    "DisableSpacesActivation": false,
                    "DisallowInfectedFileDownload": false,
                    "DisplayNamesOfFileViewers": true,
                    "DisplayNamesOfFileViewersInSpo": true,
                    "DisplayStartASiteOption": true,
                    "EmailAttestationEnabled": false,
                    "EmailAttestationReAuthDays": 30,
                    "EmailAttestationRequired": false,
                    "EnableAIPIntegration": false,
                    "EnableAutoNewsDigest": true,
                    "EnableAzureADB2BIntegration": false,
                    "EnabledFlightAllowAADB2BSkipCheckingOTP": false,
                    "EnableGuestSignInAcceleration": false,
                    "EnableMinimumVersionRequirement": true,
                    "EnableMipSiteLabel": false,
                    "EnablePromotedFileHandlers": true,
                    "ExcludedFileExtensionsForSyncClient": [
                        ""
                    ],
                    "ExternalServicesEnabled": true,
                    "ExternalUserExpirationRequired": false,
                    "ExternalUserExpireInDays": 60,
                    "FileAnonymousLinkType": 2,
                    "FilePickerExternalImageSearchEnabled": true,
                    "FolderAnonymousLinkType": 2,
                    "GuestSharingGroupAllowListInTenant": "",
                    "GuestSharingGroupAllowListInTenantByPrincipalIdentity": null,
                    "HasAdminCompletedCUConfiguration": true,
                    "HasIntelligentContentServicesCapability": false,
                    "HasTopicExperiencesCapability": true,
                    "HideSyncButtonOnDocLib": false,
                    "HideSyncButtonOnODB": false,
                    "IBImplicitGroupBased": false,
                    "ImageTaggingOption": 1,
                    "IncludeAtAGlanceInShareEmails": true,
                    "InformationBarriersSuspension": true,
                    "IPAddressAllowList": "",
                    "IPAddressEnforcement": false,
                    "IPAddressWACTokenLifetime": 15,
                    "IsAppBarTemporarilyDisabled": false,
                    "IsCollabMeetingNotesFluidEnabled": true,
                    "IsFluidEnabled": true,
                    "IsHubSitesMultiGeoFlightEnabled": true,
                    "IsMnAFlightEnabled": false,
                    "IsMultiGeo": true,
                    "IsUnmanagedSyncClientForTenantRestricted": false,
                    "IsUnmanagedSyncClientRestrictionFlightEnabled": true,
                    "IsWBFluidEnabled": true,
                    "LabelMismatchEmailHelpLink": null,
                    "LegacyAuthProtocolsEnabled": true,
                    "LimitedAccessFileType": 1,
                    "MachineLearningCaptureEnabled": true,
                    "MarkNewFilesSensitiveByDefault": 0,
                    "MediaTranscription": 0,
                    "MobileFriendlyUrlEnabledInTenant": true,
                    "NoAccessRedirectUrl": null,
                    "NotificationsInOneDriveForBusinessEnabled": true,
                    "NotificationsInSharePointEnabled": true,
                    "NotifyOwnersWhenInvitationsAccepted": true,
                    "NotifyOwnersWhenItemsReshared": true,
                    "ODBAccessRequests": 0,
                    "ODBMembersCanShare": 0,
                    "ODBSharingCapability": 1,
                    "OfficeClientADALDisabled": false,
                    "OneDriveForGuestsEnabled": false,
                    "OneDriveStorageQuota": 1048576,
                    "OptOutOfGrooveBlock": false,
                    "OptOutOfGrooveSoftBlock": false,
                    "OrgNewsSiteUrl": null,
                    "OrphanedPersonalSitesRetentionPeriod": 30,
                    "OwnerAnonymousNotification": true,
                    "PermissiveBrowserFileHandlingOverride": false,
                    "PreventExternalUsersFromResharing": false,
                    "ProvisionSharedWithEveryoneFolder": false,
                    "PublicCdnAllowedFileTypes": "CSS,EOT,GIF,ICO,JPEG,JPG,JS,MAP,PNG,SVG,TTF,WOFF",
                    "PublicCdnEnabled": false,
                    "PublicCdnOrigins": [],
                    "RequireAcceptingAccountMatchInvitedAccount": false,
                    "RequireAnonymousLinksExpireInDays": -1,
                    "ResourceQuota": 0,
                    "ResourceQuotaAllocated": 0,
                    "RestrictedOneDriveLicense": false,
                    "RootSiteUrl": "https:\u002f\u002fbertonline.sharepoint.com",
                    "SearchResolveExactEmailOrUPN": false,
                    "SharingAllowedDomainList": "outlook.com",
                    "SharingBlockedDomainList": null,
                    "SharingCapability": 1,
                    "SharingDomainRestrictionMode": 0,
                    "ShowAllUsersClaim": false,
                    "ShowEveryoneClaim": false,
                    "ShowEveryoneExceptExternalUsersClaim": true,
                    "ShowNGSCDialogForSyncOnODB": true,
                    "ShowPeoplePickerSuggestionsForGuestUsers": false,
                    "SignInAccelerationDomain": "",
                    "SocialBarOnSitePagesDisabled": false,
                    "SpecialCharactersStateInFileFolderNames": 1,
                    "StartASiteFormUrl": null,
                    "StopNew2010Workflows": false,
                    "StopNew2013Workflows": false,
                    "StorageQuota": 1304576,
                    "StorageQuotaAllocated": 0,
                    "StreamLaunchConfig": 0,
                    "StreamLaunchConfigLastUpdated": "\/Date(1,0,1,0,0,0,0)\/",
                    "StreamLaunchConfigUpdateCount": 0,
                    "SyncAadB2BManagementPolicy": true,
                    "SyncPrivacyProfileProperties": true,
                    "UseFindPeopleInPeoplePicker": false,
                    "UsePersistentCookiesForExplorerView": false,
                    "UserVoiceForFeedbackEnabled": false,
                    "ViewersCanCommentOnMediaDisabled": false,
                    "ViewInFileExplorerEnabled": false,
                    "WhoCanShareAllowListInTenant": "",
                    "WhoCanShareAllowListInTenantByPrincipalIdentity": null,
                    "Workflow2010Disabled": false,
                    "Workflows2013State": 2
                }
            ]             
            */
            #endregion

            TenantProperties resultItem = ResponseHelper.ProcessResponse<TenantProperties>(response, QueryIdPath);
            Result = resultItem;
        }
    }
}
