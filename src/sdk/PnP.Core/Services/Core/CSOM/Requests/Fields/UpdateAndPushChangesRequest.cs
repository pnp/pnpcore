using PnP.Core.Model.SharePoint;
using PnP.Core.Services.Core.CSOM.QueryAction;
using PnP.Core.Services.Core.CSOM.QueryIdentities;
using PnP.Core.Services.Core.CSOM.Utils;
using PnP.Core.Services.Core.CSOM.Utils.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PnP.Core.Services.Core.CSOM.Requests.Fields
{
    internal sealed class UpdateAndPushChangesRequest : IRequest<object>
    {
        public object Result { get; set; }

        internal string SiteId { get; private set; }

        internal string FieldId { get; private set; }

        internal string WebId { get; private set; }

        internal Field Properties { get; set; }

        internal UpdateAndPushChangesRequest(Guid siteId, Guid webId, Field field)
        {
            Properties = field;
            SiteId = siteId.ToString();
            WebId = webId.ToString();
            FieldId = field.Id.ToString();
        }

        public List<ActionObjectPath> GetRequest(IIdProvider idProvider)
        {
            List<ActionObjectPath> result = new List<ActionObjectPath>();

            // Object Paths
            var IdentityPathId = idProvider.GetActionId();
            ActionObjectPath path1 = new ActionObjectPath()
            {
                ObjectPath = new Identity()
                {
                    Id = IdentityPathId,
                    Name = $"c87d4ba0-30c7-4000-8eae-5ce3dac63bb9|740c6a0b-85e2-48a0-a494-e0f1759d4aa7:site:{SiteId}:web:{WebId}:field:{FieldId}"
                }
            };

            // Actions

            var fieldsToUpdate = GetFieldsToUpdate();
            foreach (var field in fieldsToUpdate)
            {
                ActionObjectPath updateProperty = new ActionObjectPath()
                {
                    Action = new SetPropertyAction()
                    {
                        Id = idProvider.GetActionId(),
                        ObjectPathId = IdentityPathId.ToString(),
                        Name = field.FieldName,
                        SetParameter = new Parameter()
                        {
                            Type = field.FieldType,
                            Value = field.FieldValue
                        }
                    }
                };
                result.Add(updateProperty);
            }

            ActionObjectPath updateAndPushChangesMethod = new ActionObjectPath()
            {
                Action = new MethodAction()
                {
                    Id = idProvider.GetActionId(),
                    ObjectPathId = IdentityPathId.ToString(),
                    Name = "UpdateAndPushChanges",
                    Parameters = new List<Parameter>() {
                        new Parameter()
                        {
                            Type = "Boolean",
                            Value = true
                        }
                    }
                },
            };
            result.Add(updateAndPushChangesMethod);

            result.Add(path1);

            return result;
        }

        public void ProcessResponse(string response)
        {
        }

        private List<CSOMItemField> GetFieldsToUpdate()
        {
            List<CSOMItemField> fields = new List<CSOMItemField>();

            if (Properties.HasChanges)
            {
                foreach (PropertyDescriptor changedProperty in Properties.ChangedProperties)
                {
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
