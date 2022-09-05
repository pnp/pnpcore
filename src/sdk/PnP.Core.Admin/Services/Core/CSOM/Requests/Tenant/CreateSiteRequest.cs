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
    internal sealed class CreateSiteRequest : IRequest<object>
    {
        internal CreateSiteRequest(Uri siteUrl, string title, string owner, string webTemplate, int lcid, int timeZoneId)
        {
            SiteUrl = siteUrl;
            Title = title;
            Owner = owner;
            WebTemplate = webTemplate;
            Lcid = lcid;
            TimeZoneId = timeZoneId;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        public Uri SiteUrl { get; set; }

        public int Lcid { get; set; }

        public string Owner { get; set; }

        public string WebTemplate { get; set; }

        public int TimeZoneId { get; set; }

        public string Title { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            int createSiteId = idProvider.GetActionId();
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
                    <Method Id="1" ParentId="2" Name="CreateSite">
                        <Parameters>
                            <Parameter TypeId="{11f84fff-b8cf-47b6-8b50-34e692656606}">
                                <Property Name="CompatibilityLevel" Type="Int32">0</Property>
                                <Property Name="Lcid" Type="Int32">1033</Property>
                                <Property Name="Owner" Type="String">bert.jansen@bertonline.onmicrosoft.com</Property>
                                <Property Name="StorageMaximumLevel" Type="Int64">27487790694400</Property>
                                <Property Name="StorageWarningLevel" Type="Int64">27487790694400</Property>
                                <Property Name="Template" Type="String">STS#0</Property>
                                <Property Name="TimeZoneId" Type="Int32">3</Property>
                                <Property Name="Title" Type="String">PnP Core SDK Test</Property>
                                <Property Name="Url" Type="String">https://bertonline.sharepoint.com/sites/pnpcoresdktestclassicsitef03a62586d3e41658f8ad2ceb7e53acf</Property>
                                <Property Name="UserCodeMaximumLevel" Type="Double">0</Property>
                                <Property Name="UserCodeWarningLevel" Type="Double">0</Property>
                            </Parameter>
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
                    ObjectPathId = createSiteId.ToString(),
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

            ActionObjectPath createSiteAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = createSiteId,
                    ParentId = IdentityPath,
                    Name = "CreateSite",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>()
                        {
                            new Parameter()
                            {
                                TypeId = "11f84fff-b8cf-47b6-8b50-34e692656606",
                                Value = new List<NamedProperty>()
                                {
                                    new NamedProperty()
                                    {
                                        Name = "CompatibilityLevel",
                                        Type = "Int32",
                                        Value = "0"
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "Lcid",
                                        Type = "Int32",
                                        Value = Lcid.ToString()
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "Owner",
                                        Type = "String",
                                        Value = Owner
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "StorageMaximumLevel",
                                        Type = "Int64",
                                        Value = "27487790694400"
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "StorageWarningLevel",
                                        Type = "Int64",
                                        Value = "27487790694400"
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "Template",
                                        Type = "String",
                                        Value = WebTemplate
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "TimeZoneId",
                                        Type = "Int32",
                                        Value = TimeZoneId.ToString()
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "Title",
                                        Type = "String",
                                        Value = Title
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "Url",
                                        Type = "String",
                                        Value = SiteUrl.AbsoluteUri.ToString()
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "UserCodeMaximumLevel",
                                        Type = "Double",
                                        Value = "0"
                                    },
                                    new NamedProperty()
                                    {
                                        Name = "UserCodeWarningLevel",
                                        Type = "Double",
                                        Value = "0"
                                    },
                                }
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
            actions.Add(createSiteAction);

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
