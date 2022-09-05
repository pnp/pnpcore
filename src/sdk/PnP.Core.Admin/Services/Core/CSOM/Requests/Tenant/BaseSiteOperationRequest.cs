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
    internal abstract class BaseSiteOperationRequest : IRequest<object>
    {
        private string operationName;

        internal BaseSiteOperationRequest(string operation, Uri siteUrl)
        {
            SiteUrl = siteUrl;
            operationName = operation;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        public Uri SiteUrl { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            int removeSiteId = idProvider.GetActionId();
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();

            #region XML Payload generated
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName="pnp core sdk"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <Query Id="3" ObjectPathId="1">
                        <Query SelectAllProperties="false">
                            <Properties>
                                <Property Name="PollingInterval" ScalarProperty="true" />
                                <Property Name="IsComplete" ScalarProperty="true" />
                            </Properties>
                        </Query>
                    </Query>
                </Actions>
                <ObjectPaths>
                    <Constructor Id="2" TypeId="{268004ae-ef6b-4e9b-8425-127220d84719}" />
                    <Method Id="1" ParentId="2" Name="RemoveSite">
                        <Parameters>
                            <Parameter Type="String">https://bertonline.sharepoint.com/sites/removeme</Parameter>
                        </Parameters>
                    </Method>
                </ObjectPaths>
            </Request>
            */
            #endregion

            List<ActionObjectPath> actions = new List<ActionObjectPath>();
            ActionObjectPath spoOperation = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = QueryIdPath,
                    ObjectPathId = removeSiteId.ToString(),
                    SelectQuery = new SelectQuery()
                    {
                        SelectAllProperties = false,
                        Properties = new List<Property>()
                        {
                            new Property()
                            {
                                Name = "PollingInterval"
                            },
                            new Property()
                            {
                                Name = "IsComplete"
                            },
                        }
                    }
                }
            };

            ActionObjectPath removeSiteAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = removeSiteId,
                    ParentId = IdentityPath,
                    Name = operationName,
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>()
                        {
                            new Parameter()
                            {
                                Value = SiteUrl.AbsoluteUri.ToString(),
                            }
                        }
                    }
                }
            };


            ActionObjectPath spoOperationCollection = new ActionObjectPath()
            {
                ObjectPath = new ConstructorPath
                {
                    Id = IdentityPath,
                    TypeId = PnPAdminConstants.CsomTenantObject
                }
            };

            actions.Add(spoOperation);
            actions.Add(spoOperationCollection);
            actions.Add(removeSiteAction);

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
             [
                {
                    "SchemaVersion": "15.0.0.0",
                    "LibraryVersion": "16.0.21722.12002",
                    "ErrorInfo": null,
                    "TraceCorrelationId": "fd97f49f-8009-3000-4ac5-078be4dab272"
                },
                3,
                {
                    "_ObjectType_": "Microsoft.Online.SharePoint.TenantAdministration.SpoOperation",
                    "_ObjectIdentity_": "fd97f49f-8009-3000-4ac5-078be4dab272|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9\nSpoOperation\nRemoveSite\n637686125576711919\nhttps%3a%2f%2fbertonline.sharepoint.com%2fsites%2f130021\n35708a6b-88bc-4c47-9c13-d227dabbee4d",
                    "PollingInterval": 15000,
                    "IsComplete": true
                }
            ]
            */
            #endregion

            SpoOperation resultItem = ResponseHelper.ProcessResponse<SpoOperation>(response, QueryIdPath);
            Result = resultItem;
        }

    }
}
