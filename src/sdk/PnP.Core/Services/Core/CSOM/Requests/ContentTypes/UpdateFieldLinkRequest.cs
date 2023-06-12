using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PnP.Core.Services.Core.CSOM.Requests.ContentTypes
{
    internal sealed class UpdateFieldLinkRequest : IRequest<object>
    {
        private Guid siteId;
        private Guid webId;

        internal UpdateFieldLinkRequest(IContentType contentType, IFieldLink fieldLink, Guid siteId, Guid webId, bool updateContentTypeChildren = true)
        {
            ContentType = contentType;
            FieldLink = fieldLink;
            UpdateContentType = updateContentTypeChildren;
            this.siteId = siteId;
            this.webId = webId;
            VerifyField(fieldLink);
        }

        internal IContentType ContentType { get; set; }

        internal IFieldLink FieldLink { get; set; }

        internal bool IsListField { get; set; }

        internal IList List { get; set; }

        internal bool UpdateContentType { get; set; }

        internal CSOMResponseHelper ResponseHelper { get; set; } = new CSOMResponseHelper();

        public object Result { get; set; }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            #region XML Payload generated
            /*
                <Request AddExpandoFieldTypeSuffix="true" SchemaVersion="15.0.0.0" LibraryVersion="16.0.0.0" ApplicationName=".NET Library"
                    xmlns="http://schemas.microsoft.com/sharepoint/clientquery/2009">
                    <Actions>
                        <SetProperty Id="1042" ObjectPathId="1038" Name="Required">
                            <Parameter Type="Boolean">true</Parameter>
                        </SetProperty>
                        <Method Name="Update" Id="1043" ObjectPathId="82">
                            <Parameters>
                                <Parameter Type="Boolean">true</Parameter>
                            </Parameters>
                        </Method>
                    </Actions>
                    <ObjectPaths>
                        <Identity Id="1038" Name="88be0da0-204d-3000-7e87-438991c722f3|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC:fl:4d4fbc21-ecf7-40e4-b42b-d23b6c854789" />
                        <Identity Id="82" Name="87be0da0-1066-3000-787b-4a007d7fdced|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:b56adf79-ff6a-4964-a63a-ff1fa23be9f8:web:8c8e101c-1b0d-4253-85e7-c30039bf46e2:contenttype:0x01007D2D52B86CBD804BA6D9F006DF19F8DC" />
                    </ObjectPaths>
                </Request>
            */
            #endregion

            int fieldLinkIdentityId = idProvider.GetActionId();
            int contentTypeIdentityId = idProvider.GetActionId();

            List<ActionObjectPath> actions = new List<ActionObjectPath>();

            foreach (PropertyDescriptor cp in (FieldLink as FieldLink).ChangedProperties)
            {
                ActionObjectPath objectPathForChangedProperty = new ActionObjectPath
                {
                    Action = new SetPropertyAction
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = fieldLinkIdentityId.ToString(),
                        Name = cp.Name,
                        SetParameter = new Parameter
                        {
                            Type = cp.PropertyType.Name,
                            Value = cp.GetValue(FieldLink)
                        }
                    }
                };
                actions.Add(objectPathForChangedProperty);
            }

            ActionObjectPath updateMethod = new ActionObjectPath
            {
                Action = new MethodAction
                {
                    Name = "Update",
                    ObjectPathId = contentTypeIdentityId.ToString(),
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

            ActionObjectPath contentTypeIdentity = new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = contentTypeIdentityId,
                    Name = $"6ba80da0-b09a-3000-7e8f-67b158a69089|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:contenttype:{ContentType.StringId}"
                }
            };

            ActionObjectPath fieldLinkIdentity = new ActionObjectPath
            {
                ObjectPath = new Identity
                {
                    Id = fieldLinkIdentityId,
                    Name = $"6ba80da0-b09a-3000-7e8f-67b158a69089|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:contenttype:{ContentType.StringId}:fl:{FieldLink.Id}"
                }
            };

            ActionObjectPath listContentTypeIdentity = null;
            ActionObjectPath listFieldLinkIdentity = null;

            if (IsListField)
            {
                listContentTypeIdentity = new ActionObjectPath
                {
                    ObjectPath = new Identity
                    {
                        Id = contentTypeIdentityId,
                        Name = $"6ba80da0-b09a-3000-7e8f-67b158a69089|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:list:{List.Id}:contenttype:{ContentType.StringId}"
                    }
                };

                listFieldLinkIdentity = new ActionObjectPath
                {
                    ObjectPath = new Identity
                    {
                        Id = fieldLinkIdentityId,
                        Name = $"6ba80da0-b09a-3000-7e8f-67b158a69089|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{siteId}:web:{webId}:list:{List.Id}:contenttype:{ContentType.StringId}:fl:{FieldLink.Id}"
                    }
                };
            }

            actions.Add(updateMethod);

            if (IsListField)
            {
                actions.Add(listContentTypeIdentity);
                actions.Add(listFieldLinkIdentity);
            }
            else
            {
                actions.Add(contentTypeIdentity);
                actions.Add(fieldLinkIdentity);
            }

            return actions;
        }

        public void ProcessResponse(string response)
        {
            #region Json response
            /*
            [
            {
            "SchemaVersion":"15.0.0.0","LibraryVersion":"16.0.22001.12005","ErrorInfo":null,"TraceCorrelationId":"8abe0da0-009f-3000-787b-4264c606f979"
            }
            ]
            */
            #endregion
        }

        private void VerifyField(IFieldLink field)
        {
            if (field.Parent.Parent.Parent.Parent is IList parentList)
            {
                IsListField = true;
                List = parentList;
            }
        }

    }
}
