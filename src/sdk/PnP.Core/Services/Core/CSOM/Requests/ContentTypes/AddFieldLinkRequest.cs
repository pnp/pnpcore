using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PnP.Core.Services.Core.CSOM.Requests.ContentTypes
{
    internal sealed class AddFieldLinkRequest : IRequest<object>
    {

        private Guid siteId;
        private Guid webId;

        internal AddFieldLinkRequest(IContentType contentType, IField field, Guid siteId, Guid webId, bool updateContentTypeChildren = true)
        {
            ContentType = contentType;
            Field = field;
            UpdateContentType = updateContentTypeChildren;
            this.siteId = siteId;
            this.webId = webId;
            VerifyField(field);
        }

        internal IContentType ContentType { get; set; }

        internal IField Field { get; set; } 

        internal bool IsListField { get; set; }

        internal IList List { get; set; }

        internal bool UpdateContentType { get; set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        internal int IdentityQuery { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {

            #region XML Payload generated
            /*
            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="1034" ObjectPathId="1033" />
                    <ObjectIdentityQuery Id="1035" ObjectPathId="1033" />
                    <Method Name="Update" Id="1036" ObjectPathId="82">
                        <Parameters>
                            <Parameter Type="Boolean">true</Parameter>
                        </Parameters>
                    </Method>
                </Actions>
                <ObjectPaths>
                    <Method Id="1033" ParentId="83" Name="Add">
                        <Parameters>
                            <Parameter TypeId="{63fb2c92-8f65-4bbb-a658-b6cd294403f4}">
                                <Property Name="Field" ObjectPathId="9" />
                            </Parameter>
                        </Parameters>
                    </Method>
                    <Identity Id="82" Name="6ba80da0-b09a-3000-7e8f-67b158a69089|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC" />
                    <Property Id="83" ParentId="82" Name="FieldLinks" />
                    <Identity Id="9" Name="6ba80da0-a076-3000-b5f5-7110738edd1f|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:field:4d4fbc21-ecf7-40e4-b42b-d23b6c854789" />
                </ObjectPaths>
            </Request>

            or

            <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                <Actions>
                    <ObjectPath Id="1145" ObjectPathId="1144" />
                    <ObjectIdentityQuery Id="1146" ObjectPathId="1144" />
                    <Method Name="Update" Id="1147" ObjectPathId="1140">
                        <Parameters>
                            <Parameter Type="Boolean">false</Parameter>
                        </Parameters>
                    </Method>
                </Actions>
                <ObjectPaths>
                    <Method Id="1144" ParentId="1141" Name="Add">
                        <Parameters>
                            <Parameter TypeId="{63fb2c92-8f65-4bbb-a658-b6cd294403f4}">
                                <Property Name="Field" ObjectPathId="84" />
                            </Parameter>
                        </Parameters>
                    </Method>
                    <Identity Id="1140" Name="0d9a0ea0-b0c6-3000-787b-49b762c80954|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:a7d48f4d-9be1-4616-ba24-eef4f3ce8170:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC0077B18BE608EC2643BCDCF04EE620AABD" />
                    <Property Id="1141" ParentId="1140" Name="FieldLinks" />
                    <Identity Id="84" Name="0b9a0ea0-80e3-3000-baac-8a54b2429926|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:a7d48f4d-9be1-4616-ba24-eef4f3ce8170:field:2ac05f93-32ab-44a4-b701-facff0068b4a" />
                </ObjectPaths>
            </Request>

            */
            #endregion

            int addFieldLinkMethodId = idProvider.GetActionId();
            int fieldLinks = idProvider.GetActionId();
            int fieldIdentityId = idProvider.GetActionId();
            int contentTypeIdentityId = idProvider.GetActionId();
            int listContentTypeIdentityId = idProvider.GetActionId();
            int listFieldIdentityId = idProvider.GetActionId();
            IdentityQuery = idProvider.GetActionId();

            List<ActionObjectPath> actions = new List<ActionObjectPath>();

            ActionObjectPath objectPath = new ActionObjectPath
            {
                Action = new BaseAction
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = addFieldLinkMethodId.ToString()
                }
            };

            ActionObjectPath objectIdentityQuery = new ActionObjectPath
            {
                Action = new IdentityQueryAction
                {
                    Id = IdentityQuery,
                    ObjectPathId = addFieldLinkMethodId.ToString()
                }
            };

            ActionObjectPath updateMethod = new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Name = "Update",
                    ObjectPathId = IsListField ? listContentTypeIdentityId.ToString() : contentTypeIdentityId.ToString(),
                    Id = idProvider.GetActionId(),
                    Parameters = new List<Parameter>()
                    {
                        new Parameter
                        {
                            Type = "Boolean",
                            Value = IsListField ? false : UpdateContentType
                        }
                    }
                }
            };

            ActionObjectPath addFieldLinkAction = new ActionObjectPath()
            {
                ObjectPath = new ObjectPathMethod()
                {
                    Id = addFieldLinkMethodId,
                    ParentId = fieldLinks,
                    Name = "Add",
                    Parameters = new MethodParameter()
                    {
                        Properties = new List<Parameter>()
                        {
                            new Parameter()
                            {
                                TypeId = "63fb2c92-8f65-4bbb-a658-b6cd294403f4",
                                Value = new IdentityProperty
                                {
                                    Name = "Field",
                                    ObjectPathId = IsListField ? listFieldIdentityId.ToString() : fieldIdentityId.ToString(),
                                }
                            }
                        }
                    }
                }
            };

            ActionObjectPath fieldLinkProperty = new ActionObjectPath
            {
                ObjectPath = new Property
                {
                    Id = fieldLinks,
                    ParentId = IsListField ? listContentTypeIdentityId : contentTypeIdentityId,
                    Name = "FieldLinks"
                }
            };

            ActionObjectPath contentTypeIdentity = new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = contentTypeIdentityId,
                    Name = $"6e940ea0-6072-3000-787b-48a3d44d7291|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:contenttype:{ContentType.StringId}"
                }
            };

            ActionObjectPath fieldIdentity = new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = fieldIdentityId,
                    Name = $"6ba80da0-a076-3000-b5f5-7110738edd1f|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:field:{Field.Id}"
                }
            };

            ActionObjectPath listContentTypeIdentity = null;
            ActionObjectPath listFieldIdentity = null;

            if (IsListField)
            {
                listContentTypeIdentity = new ActionObjectPath
                {
                    ObjectPath = new Identity
                    {
                        Id = listContentTypeIdentityId,
                        Name = $"0d9a0ea0-b0c6-3000-787b-49b762c80954|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:list:{List.Id}:contenttype:{ContentType.StringId}"
                    }
                };

                listFieldIdentity = new ActionObjectPath
                {
                    ObjectPath = new Identity
                    {
                        Id = listFieldIdentityId,
                        Name = $"0b9a0ea0-80e3-3000-baac-8a54b2429926|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:list:{List.Id}:field:{Field.Id}"
                    }
                };
            }

            actions.Add(objectPath);
            actions.Add(objectIdentityQuery);
            actions.Add(updateMethod);
            actions.Add(addFieldLinkAction);            

            if (IsListField)
            {
                actions.Add(listContentTypeIdentity);
                actions.Add(fieldLinkProperty);
                actions.Add(listFieldIdentity);
            }
            else
            {
                actions.Add(contentTypeIdentity);
                actions.Add(fieldLinkProperty);
                actions.Add(fieldIdentity);
            }

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
            [
                {
                    "SchemaVersion": "15.0.0.0",
                    "LibraryVersion": "16.0.22001.12005",
                    "ErrorInfo": null,
                    "TraceCorrelationId": "6da80da0-3051-3000-b5f5-766bda76f94e"
                },
                1034,
                {
                    "IsNull": false
                },
                1035,
                {
                    "_ObjectIdentity_": "6da80da0-3051-3000-b5f5-766bda76f94e|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC:fl:4d4fbc21-ecf7-40e4-b42b-d23b6c854789"
                }
            ]

            or

            [
                {
                    "SchemaVersion": "15.0.0.0",
                    "LibraryVersion": "16.0.22001.12005",
                    "ErrorInfo": null,
                    "TraceCorrelationId": "74940ea0-d0b5-3000-b5f5-7f4918c59d07"
                },
                1124,
                {
                    "IsNull": false
                },
                1126,
                {
                    "IsNull": false
                },
                1127,
                {
                    "_ObjectIdentity_": "74940ea0-d0b5-3000-b5f5-7f4918c59d07|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:list:a7d48f4d-9be1-4616-ba24-eef4f3ce8170:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC00B648EAA156BD4B4393657D518726E6C0:fl:2ac05f93-32ab-44a4-b701-facff0068b4a"
                }
            ]

            */
            #endregion

            var identityElement = ResponseHelper.ProcessResponse<JsonElement>(response, IdentityQuery).GetProperty("_ObjectIdentity_").GetString();
            Result = Guid.Parse(identityElement.Substring(identityElement.LastIndexOf(":fl:") + 4));
        }

        private void VerifyField(IField field)
        {
            if (field.Parent.Parent is IList parentList)
            {
                IsListField = true;
                List = parentList;
            }
        }
        
    }
}
