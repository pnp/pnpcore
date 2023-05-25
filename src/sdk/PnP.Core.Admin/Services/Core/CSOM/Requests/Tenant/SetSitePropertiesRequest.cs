using PnP.Core.Admin.Model.SharePoint;
using PnP.Core.Services.Core.CSOM;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Requests;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal sealed class SetSitePropertiesRequest : IRequest<object>
    {
        internal SetSitePropertiesRequest(SiteCollectionProperties properties)
        {
            Properties = properties;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal SiteCollectionProperties Properties { get; set; }

        internal int IdentityPath { get; private set; }

        internal int QueryIdPath { get; private set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            IdentityPath = idProvider.GetActionId();
            QueryIdPath = idProvider.GetActionId();
            int updateId = idProvider.GetActionId();

            #region XML Payload generated
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName="pnp core sdk"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <SetProperty Id="4" ObjectPathId="1" Name="DisableFlows">
                        <Parameter Type="Enum">2</Parameter>
                    </SetProperty>
                    <SetProperty Id="5" ObjectPathId="1" Name="Title">
                        <Parameter Type="String">New title 1</Parameter>
                    </SetProperty>
                    <Query Id="2" ObjectPathId="3">
                        <Query SelectAllProperties="false">
                            <Properties>
                                <Property Name="PollingInterval" ScalarProperty="true" />
                                <Property Name="IsComplete" ScalarProperty="true" />
                            </Properties>
                        </Query>
                    </Query>
                    <ObjectPath Id="6" ObjectPathId="3" />
                    <ObjectIdentityQuery Id="7" ObjectPathId="1" />
                </Actions>
                <ObjectPaths>
                    <Method Id="3" ParentId="1" Name="Update" />
                    <Identity Id="1" Name="0a94fb9f-8071-3000-513c-5633d32c844e|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9&#xA;SiteProperties&#xA;https://bertonline.sharepoint.com/sites/prov-1" />
                </ObjectPaths>
            </Request>            
            */
            #endregion

            List<ActionObjectPath> actions = new List<ActionObjectPath>();

            var fieldsToUpdate = GetFieldsToUpdate();
            foreach(var field in fieldsToUpdate)
            {
                ActionObjectPath updateProperty = new ActionObjectPath()
                {
                    Action = new SetPropertyAction()
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = IdentityPath.ToString(),
                        Name = field.FieldName,
                        SetParameter = new Parameter()
                        {
                            Type = field.FieldType,
                            Value = field.FieldValue
                        }
                    }
                };

                actions.Add(updateProperty);
            }

            ActionObjectPath spoOperation = new ActionObjectPath()
            {
                Action = new QueryAction()
                {
                    Id = QueryIdPath,
                    ObjectPathId = updateId.ToString(),
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

            ActionObjectPath spoOperation2 = new ActionObjectPath()
            {
                Action = new BaseAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = updateId.ToString()
                },
            };

            ActionObjectPath spoOperation3 = new ActionObjectPath()
            {
                Action = new IdentityQueryAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = IdentityPath.ToString()
                },
            };

            ActionObjectPath updateSitePropertiesAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = updateId,
                    ParentId = IdentityPath,
                    Name = "Update"
                }
            };

            ActionObjectPath updateSitePropertiesIdentity = new ActionObjectPath()
            {
                ObjectPath = new Identity()
                {
                    Id = IdentityPath,
                    Name = Properties.ObjectIdentity.Replace("\n", "&#xA;")
                }
            };

            actions.Add(spoOperation);
            actions.Add(spoOperation2);
            actions.Add(spoOperation3);
            actions.Add(updateSitePropertiesAction);
            actions.Add(updateSitePropertiesIdentity);

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
                [
                {
                "SchemaVersion":"15.0.0.0","LibraryVersion":"16.0.21813.12000","ErrorInfo":null,"TraceCorrelationId":"6a98fb9f-408c-3000-4ada-4a494003f3c0"
                },2,{
                "_ObjectType_":"Microsoft.Online.SharePoint.TenantAdministration.SpoOperation","_ObjectIdentity_":"6a98fb9f-408c-3000-4ada-4a494003f3c0|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9\nSpoOperation\nSetSite\n637704920542784645\nhttps%3a%2f%2fbertonline.sharepoint.com%2fsites%2fprov-1\n00000000-0000-0000-0000-000000000000","PollingInterval":15000,"IsComplete":true
                },5,{
                "IsNull":false
                },6,{
                "_ObjectIdentity_":"6a98fb9f-408c-3000-4ada-4a494003f3c0|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9\nSiteProperties\nhttps%3a%2f%2fbertonline.sharepoint.com%2fsites%2fprov-1"
                }
                ]                          
            */
            #endregion

            SpoOperation resultItem = ResponseHelper.ProcessResponse<SpoOperation>(response, QueryIdPath);
            Result = resultItem;
        }

        private List<CSOMItemField> GetFieldsToUpdate()
        {
            List<CSOMItemField> fields = new List<CSOMItemField>();

            if (Properties.HasChanges)
            {
                foreach (PropertyDescriptor changedProperty in Properties.ChangedProperties)
                {
                    // Deal with "special" cases

                    // When RestrictedAccessControlGroups was set that works fine, however clearing them does not work by setting an empty array, one needs to set
                    // the RemoveRestrictedAccessControlGroups guid array
                    if (changedProperty.Name == "RestrictedAccessControlGroups" && GetFieldValue(changedProperty) is List<Guid> restrictedAccessControlGroups)
                    {
                        if (!restrictedAccessControlGroups.Any())
                        {
                            continue;
                        }
                    }


                    fields.Add(new CSOMItemField()
                    {
                        FieldName = changedProperty.Name,
                        FieldValue = GetFieldValue(changedProperty),
                        FieldType = changedProperty.PropertyType.IsEnum ? "Enum" : null
                    });
                }
            }

            return fields;
        }        

        private object GetFieldValue(PropertyDescriptor changedProperty)
        {
            if (changedProperty.PropertyType.IsEnum)
            {
                return (int)Enum.Parse(changedProperty.PropertyType, Properties.GetValue(changedProperty.Name).ToString());
            }
            else
            {
                return Properties.GetValue(changedProperty.Name);
            }
        }
    }
}
