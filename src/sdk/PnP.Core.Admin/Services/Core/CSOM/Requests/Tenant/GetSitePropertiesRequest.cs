using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class GetSitePropertiesRequest : IRequest<object>
    {
        internal GetSitePropertiesRequest(Uri siteUrl, bool detailed)
        {
            SiteUrl = siteUrl;
            Detailed = detailed;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal Uri SiteUrl { get; set; }

        internal bool Detailed { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();
            int getSitePropertiesByUrl = idProvider.GetActionId();

            #region XML Payload generated
            /*
                <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName="pnp core sdk"
                    xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                    <Actions>
                        <ObjectPath Id="4" ObjectPathId="1" />
                        <ObjectPath Id="5" ObjectPathId="3" />
                        <Query Id="6" ObjectPathId="3">
                            <Query SelectAllProperties="true">
                                <Properties />
                            </Query>
                        </Query>
                    </Actions>
                    <ObjectPaths>
                        <Method Id="3" ParentId="1" Name="GetSitePropertiesByUrl">
                            <Parameters>
                                <Parameter Type="String">https://bertonline.sharepoint.com/sites/removeme</Parameter>
                                <Parameter Type="Boolean">true</Parameter>
                            </Parameters>
                        </Method>
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

            ActionObjectPath spoOperation2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = getSitePropertiesByUrl.ToString()
                },
            };

            ActionObjectPath spoOperation3 = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = QueryIdPath,
                    ObjectPathId = getSitePropertiesByUrl.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = true,
                    }
                },
            };

            ActionObjectPath createSiteAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = getSitePropertiesByUrl,
                    ParentId = IdentityPath,
                    Name = "GetSitePropertiesByUrl",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>()
                        {
                            new Parameter()
                            {
                                Type = "String",
                                Value = SiteUrl.AbsoluteUri
                            },
                            new Parameter()
                            {
                                Type = "Boolean",
                                Value = Detailed
                            }
                        }
                    }
                }
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
            actions.Add(spoOperation2);
            actions.Add(spoOperation3);
            actions.Add(createSiteAction);
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
                    "LibraryVersion": "16.0.21813.12000",
                    "ErrorInfo": null,
                    "TraceCorrelationId": "cb55fb9f-a0cf-3000-4ada-42a9208b0a6c"
                },
                4,
                {
                    "IsNull": false
                },
                5,
                {
                    "IsNull": false
                },
                6,
                {
                    "_ObjectType_": "Microsoft.Online.SharePoint.TenantAdministration.SiteProperties",
                    "_ObjectIdentity_": "cb55fb9f-a0cf-3000-4ada-42a9208b0a6c|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9\nSiteProperties\nhttps%3a%2f%2fbertonline.sharepoint.com%2fsites%2fprov-1",
                    "AllowDownloadingNonWebViewableFiles": false,
                    "AllowEditing": true,
                    "AllowSelfServiceUpgrade": true,
                    "AnonymousLinkExpirationInDays": 0,
                    "AuthContextStrength": null,
                    "AuthenticationContextName": null,
                    "AverageResourceUsage": 0,
                    "BlockDownloadLinksFileType": 1,
                    "CommentsOnSitePagesDisabled": false,
                    "CompatibilityLevel": 15,
                    "ConditionalAccessPolicy": 0,
                    "CurrentResourceUsage": 0,
                    "DefaultLinkPermission": 0,
                    "DefaultLinkToExistingAccess": false,
                    "DefaultLinkToExistingAccessReset": false,
                    "DefaultSharingLinkType": 0,
                    "DenyAddAndCustomizePages": 2,
                    "Description": "",
                    "DisableAppViews": 2,
                    "DisableCompanyWideSharingLinks": 2,
                    "DisableFlows": 2,
                    "ExternalUserExpirationInDays": 0,
                    "GroupId": "\/Guid(b32780e9-df70-4548-a735-4e7f055415a1)\/",
                    "GroupOwnerLoginName": "c:0o.c|federateddirectoryclaimprovider|b32780e9-df70-4548-a735-4e7f055415a1_o",
                    "HasHolds": false,
                    "HubSiteId": "\/Guid(00000000-0000-0000-0000-000000000000)\/",
                    "IBMode": "",
                    "IBSegments": [],
                    "IBSegmentsToAdd": null,
                    "IBSegmentsToRemove": null,
                    "IsGroupOwnerSiteAdmin": true,
                    "IsHubSite": false,
                    "IsTeamsChannelConnected": false,
                    "IsTeamsConnected": true,
                    "LastContentModifiedDate": "\/Date(2021,9,20,13,7,6,117)\/",
                    "Lcid": 2067,
                    "LimitedAccessFileType": 1,
                    "LockIssue": null,
                    "LockState": "Unlock",
                    "MediaTranscription": 0,
                    "OverrideBlockUserInfoVisibility": 0,
                    "OverrideTenantAnonymousLinkExpirationPolicy": false,
                    "OverrideTenantExternalUserExpirationPolicy": false,
                    "Owner": "b32780e9-df70-4548-a735-4e7f055415a1_o",
                    "OwnerEmail": "prov-1@bertonline.onmicrosoft.com",
                    "OwnerLoginName": "c:0o.c|federateddirectoryclaimprovider|b32780e9-df70-4548-a735-4e7f055415a1_o",
                    "OwnerName": "prov-1",
                    "PWAEnabled": 1,
                    "RelatedGroupId": "\/Guid(b32780e9-df70-4548-a735-4e7f055415a1)\/",
                    "RestrictedToRegion": 3,
                    "SandboxedCodeActivationCapability": 2,
                    "SensitivityLabel": "\/Guid(00000000-0000-0000-0000-000000000000)\/",
                    "SensitivityLabel2": "",
                    "SetOwnerWithoutUpdatingSecondaryAdmin": false,
                    "SharingAllowedDomainList": "",
                    "SharingBlockedDomainList": "",
                    "SharingCapability": 1,
                    "SharingDomainRestrictionMode": 0,
                    "ShowPeoplePickerSuggestionsForGuestUsers": false,
                    "SiteDefinedSharingCapability": 1,
                    "SocialBarOnSitePagesDisabled": false,
                    "Status": "Active",
                    "StorageMaximumLevel": 26214400,
                    "StorageQuotaType": null,
                    "StorageUsage": 4329,
                    "StorageWarningLevel": 25574400,
                    "TeamsChannelType": 0,
                    "Template": "GROUP#0",
                    "TimeZoneId": 3,
                    "Title": "prov-1",
                    "Url": "https:\u002f\u002fbertonline.sharepoint.com\u002fsites\u002fprov-1",
                    "UserCodeMaximumLevel": 300,
                    "UserCodeWarningLevel": 200,
                    "WebsCount": 4
                }
            ]             
            */
            #endregion

            SiteCollectionProperties resultItem = ResponseHelper.ProcessResponse<SiteCollectionProperties>(response, QueryIdPath);
            Result = resultItem;
        }
    }
}
