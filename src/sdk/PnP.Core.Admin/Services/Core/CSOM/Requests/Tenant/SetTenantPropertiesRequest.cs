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

namespace PnP.Core.Admin.Services.Core.CSOM.Requests.Tenant
{
    internal class SetTenantPropertiesRequest : IRequest<object>
    {
        internal SetTenantPropertiesRequest(TenantProperties properties)
        {
            Properties = properties;
        }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal TenantProperties Properties { get; set; }

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
                    <SetProperty Id="4" ObjectPathId="1" Name="AllowedDomainListForSyncClient">
                        <Parameter Type="Array"></Parameter>
                    </SetProperty>
                    <SetProperty Id="5" ObjectPathId="1" Name="BlockMacSync">
                        <Parameter Type="Boolean">false</Parameter>
                    </SetProperty>
                    <SetProperty Id="6" ObjectPathId="1" Name="ContentTypeSyncSiteTemplatesList">
                        <Parameter Type="Array"></Parameter>
                    </SetProperty>
                    <SetProperty Id="7" ObjectPathId="1" Name="ExcludedFileExtensionsForSyncClient">
                        <Parameter Type="Array">
                            <Object Type="String"></Object>
                        </Parameter>
                    </SetProperty>
                </Actions>
                <ObjectPaths>
                    <Method Id="3" ParentId="1" Name="Update" />
                    <Identity Id="1" Name="e4d2fd9f-408c-3000-4ada-425ef57c66a4|908bed80-a04a-4433-b4a0-883d9847d110:6492ece7-7f5d-4499-8130-50e761e25bd9&#xA;Tenant" />
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

            actions.Add(updateSitePropertiesAction);
            actions.Add(updateSitePropertiesIdentity);

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
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
                    // Adding public CDN origins needs to happen via a method
                    if (changedProperty.Name == "PublicCdnOrigins")
                    {
                        continue;
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
